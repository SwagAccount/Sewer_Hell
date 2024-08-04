using Sandbox;
using System;
using System.Numerics;

namespace trollface;
//Script from https://github.com/Unity-Technologies/SuperScience
public sealed class PhysicsTracker : Component
{
	/// <summary>
	/// The time period that the physics values are averaged over
	/// </summary>
	const float k_Period = 0.125f;

	/// <summary>
	/// The number of discrete steps to store physics samples in
	/// </summary>
	const int k_Steps = 4;

	/// <summary>
	/// The time period stored within a single step sample
	/// </summary>
	const float k_SamplePeriod = k_Period / k_Steps;

	/// <summary>
	/// Weight to use for the most recent physics sample, when doing prediction
	/// </summary>
	const float k_NewSampleWeight = 2.0f;
	const float k_AdditiveWeight = k_NewSampleWeight - 1.0f;

	/// <summary>
	/// If we are doing prediction, the time period we average over is stretched out
	/// to simulate having more data than we've actually recorded
	/// </summary>
	const float k_PredictedPeriod = k_Period + k_SamplePeriod * k_AdditiveWeight;

	/// <summary>
	/// We need to keep one extra sample in our sample buffer to have a smooth transition
	/// when dropping one sample for another
	/// </summary>
	const int k_SampleLength = k_Steps + 1;

	/// <summary>
	/// Minimum distance we'll update our active direction for
	/// This value is 1mm as most VR hardware is designed to be sub-millimeter
	/// </summary>
	const float k_MinOffset = 0.001f;

	/// <summary>
	/// Minimum angle needed to actually record angular velocity.
	/// Too small a value results in wildly flailing angular axis at low velocities
	/// </summary>
	const float k_MinAngle = 0.5f;

	/// <summary>
	/// The minimum length a vector can have and still give sensible results
	/// </summary>
	const float k_MinLength = 0.00001f;

	/// <summary>
	/// Stores one sample of tracked physics data
	/// </summary>
	struct Sample
	{
		public float distance;
		public float angle;

		public Vector3 offset;
		public Vector3 axisOffset;

		public float speed;
		public float angularSpeed;

		public float time;

		/// <summary>
		/// Helper function used to combine all the tracked samples up
		/// </summary>
		/// <param name="other">A sample to combine with</param>
		/// <param name="scalar">How much to scale the other sample's values</param>
		/// <param name="directionAnchor">A direction that informs which way the offset values should be pointing</param>
		/// <param name="axisAnchor">A direction that informs which way the axis values should be pointing</param>
		public void Accumulate(ref Sample other, float scalar, Vector3 directionAnchor, Vector3 axisAnchor)
		{
			distance += other.distance * scalar;
			angle += other.angle * scalar;

			// Ideally the offset vectors would be Normal for the dot product, but in practice we don't really need to
			offset += other.offset * Vector3.Dot(directionAnchor, other.offset) * scalar;
			axisOffset += other.axisOffset * Vector3.Dot(axisAnchor, other.axisOffset) * scalar;
			time += other.time * scalar;

			// We want the oldest speed for acceleration integration
			// We do a lerp so that as the oldest sample fades out, we smooth switch to the next sample 
			speed = MathX.Lerp(speed, other.speed, scalar);
			angularSpeed = MathX.Lerp(angularSpeed, other.angularSpeed, scalar);
		}
	}

	// We store all the sampled frame data in a circular array for tightest packing 
	// We don't need to store the 'end' index in our array, as when we reset we always
	// make sure the frame time in that reset sample is the maximum we need
	int m_CurrentSampleIndex = -1;
	Sample[] m_Samples = new Sample[k_SampleLength];

	// Previous-frame history for integrating velocity
	Vector3 m_LastOffsetPosition = Vector3.Zero;
	Vector3 m_LastDirectionPosition = Vector3.Zero;
	Rotation m_LastRotation = Rotation.Identity;


	[Property] public GameObject TrackedObject {get;set;}
	// Output data
	[Property] public float Speed { get; private set; }
	[Property] public float AccelerationStrength { get; private set; } // This value can be negative, for deceleration
	[Property] public Vector3 Direction { get; private set; }
	[Property] public Vector3 Velocity { get; private set; }
	[Property] public Vector3 Acceleration { get; private set; }

	public float AngularSpeed { get; private set; }
	public Vector3 AngularAxis { get; private set; }
	public Vector3 AngularVelocity { get; private set; }
	public float AngularAccelerationStrength { get; private set; } // This value can be negative, for deceleration
	public Vector3 AngularAcceleration { get; private set; }

	/// <summary>
	/// Sets the PhysicsTracker to a 'known' linear state
	/// </summary>
	/// <param name="currentPosition">The position in space the PhysicsTracker should start sampling from</param>
	/// <param name="currentRotation">The rotation in space the PhysicsTracker should start sampling from</param>
	/// <param name="currentVelocity">Any predefined motion the PhysicsTracker should take into account for smoothing</param>
	/// <param name="currentAngularVelocity">Any predefined rotation the PhysicsTracker should take into account for smoothing</param>
	public void Reset(Vector3 currentPosition, Rotation currentRotation, Vector3 currentVelocity, Vector3 currentAngularVelocity)
	{
		// Reset history values
		m_LastOffsetPosition = currentPosition;
		m_LastDirectionPosition = currentPosition;
		m_LastRotation = currentRotation;

		// Then get new 'current' values based on this new history
		Speed = currentVelocity.Length;
		Direction = currentVelocity.Normal;
		Velocity = currentVelocity;
		AccelerationStrength = 0.0f;
		Acceleration = Vector3.Zero;

		AngularSpeed = MathX.RadianToDegree(currentAngularVelocity.Length);
		AngularAxis = currentAngularVelocity.Normal;
		AngularVelocity = currentAngularVelocity;

		m_CurrentSampleIndex = 0;
		m_Samples[0] = new Sample
		{
			distance = Speed * k_Period, offset = Velocity * k_Period,
			angle = AngularSpeed * k_Period, axisOffset = AngularAxis * k_Period,
			speed = Speed, angularSpeed = AngularSpeed,
			time = k_Period
		};
	}
	float lastTime;
	protected override void OnStart()
	{
		if(TrackedObject == null) TrackedObject = GameObject;
	}
	protected override void OnUpdate()
	{
		Vector3 newPosition = TrackedObject.Transform.Position;
		Rotation newRotation = TrackedObject.Transform.Rotation;
		float timeSlice = Time.Now - lastTime;
		lastTime = Time.Now;
		// Automatically reset, if we have not done so initially
		if (m_CurrentSampleIndex == -1)
		{
			Reset(newPosition, newRotation, Vector3.Zero, Vector3.Zero);
			return;
		}

		if (timeSlice <= 0.0f)
		{
			return;
		}

		// First get single-frame offset data that we will then feed into our smoothing and prediction steps
		// We use different techniques that are well suited for direction and 'speed', and then recombine to velocity later
		var currentOffset = newPosition - m_LastOffsetPosition;
		var currentDistance = currentOffset.Length;
		m_LastOffsetPosition = newPosition;

		var activeDirection = newPosition - m_LastDirectionPosition;

		// We skip extremely small deltas and wait for more reliable changes in direction
		if (activeDirection.Length < k_MinOffset)
		{
			activeDirection = Direction;
		}
		else
		{
			activeDirection = activeDirection.Normal;
			m_LastDirectionPosition = newPosition;
		}

		// Update angular data in the same fashion
		var rotationOffset = newRotation * m_LastRotation.Inverse;
		ToAngleAxis(rotationOffset, out var currentAngle, out var activeAxis);

		// Extremely small deltas make for a wildly unpredictable axis
		if (currentAngle < k_MinAngle)
		{
			currentAngle = 0.0f;
			activeAxis = AngularAxis;
		}
		else
		{
			m_LastRotation = newRotation;
		}

		// We let strong rotations have more of an effect on the axis of rotation than weak ones
		var axisDistance = 1.0f + currentAngle / 90.0f;

		// Add new data to the current sample
		m_Samples[m_CurrentSampleIndex].distance += currentDistance;
		m_Samples[m_CurrentSampleIndex].offset += currentOffset;
		m_Samples[m_CurrentSampleIndex].angle += currentAngle;
		m_Samples[m_CurrentSampleIndex].time += timeSlice;

		// The axis can flip direction, which during this accumulation step can result in values getting small and unpredictable
		// We manually make sure the axis are always adding direction, instead of taking away
		if (Vector3.Dot(activeAxis, m_Samples[m_CurrentSampleIndex].axisOffset) < 0)
		{
			m_Samples[m_CurrentSampleIndex].axisOffset += -activeAxis * axisDistance;
		}
		else
		{
			m_Samples[m_CurrentSampleIndex].axisOffset += activeAxis * axisDistance;
		}

		// Accumulate and generate our new smooth, predicted physics values
		var combinedSample = new Sample();
		var sampleIndex = m_CurrentSampleIndex;

		while (combinedSample.time < k_Period)
		{
			var overTimeScalar = MathX.Clamp((k_Period - combinedSample.time) / m_Samples[sampleIndex].time,0,1);

			combinedSample.Accumulate(ref m_Samples[sampleIndex], overTimeScalar, activeDirection, activeAxis);
			sampleIndex = (sampleIndex + 1) % k_SampleLength;
		}

		var oldestSpeed = combinedSample.speed;
		var oldestAngularSpeed = combinedSample.angularSpeed;

		// Another accumulation step to weight the most recent values stronger for prediction
		sampleIndex = m_CurrentSampleIndex;
		while (combinedSample.time < k_PredictedPeriod)
		{
			var overTimeScalar = MathX.Clamp((k_PredictedPeriod - combinedSample.time) / m_Samples[sampleIndex].time,0,1);

			combinedSample.Accumulate(ref m_Samples[sampleIndex], overTimeScalar, activeDirection, activeAxis);
			sampleIndex = (sampleIndex + 1) % k_SampleLength;
		}

		// Our combo sample is ready to be used to generate physics output
		Speed = combinedSample.distance / k_PredictedPeriod;

		// Try to use the weighted combination of offsets. 
		if (combinedSample.offset.Length > k_MinLength)
		{
			Direction = combinedSample.offset.Normal;
		}
		else
		{
			var directionVsActive = Vector3.Dot(Direction, activeDirection);
			if (directionVsActive < 0.0f)
			{
				directionVsActive = -directionVsActive;
				Direction = -Direction;
			}

			Direction = Vector3.Lerp(activeDirection, Direction, directionVsActive).Normal;
		}

		Velocity = Direction * Speed;

		AngularSpeed = combinedSample.angle / k_PredictedPeriod;

		// Try to use the weighted combination of angles
		// We do one additional smoothing step here - this data is simply noisier than position
		if (combinedSample.axisOffset.Length > k_MinLength)
		{
			activeAxis = combinedSample.axisOffset.Normal;
		}

		var axisVsActive = Vector3.Dot(AngularAxis, activeAxis);
		if (axisVsActive < 0.0f)
		{
			axisVsActive = -axisVsActive;
			AngularAxis = -AngularAxis;
		}

		AngularAxis = Vector3.Lerp(activeAxis, AngularAxis, axisVsActive).Normal;
		AngularVelocity = AngularAxis * AngularSpeed * 0.0174533f;

		// We compare the newest and oldest velocity samples to get the new acceleration
		var speedDelta = Speed - oldestSpeed;
		var angularSpeedDelta = AngularSpeed - oldestAngularSpeed;

		AccelerationStrength = speedDelta / k_Period;
		Acceleration = AccelerationStrength * Direction;

		AngularAccelerationStrength = angularSpeedDelta / k_Period;
		AngularAcceleration = AngularAxis * AngularAccelerationStrength * 0.0174533f;

		// If the current sample is full, clear out the oldest sample and make that the new current sample
		if (m_Samples[m_CurrentSampleIndex].time < k_SamplePeriod)
		{
			return;
		}

		// We record the last speed value before we switch to a new sample, for acceleration sampling
		m_Samples[m_CurrentSampleIndex].speed = Speed;
		m_Samples[m_CurrentSampleIndex].angularSpeed = AngularSpeed;
		m_CurrentSampleIndex = (m_CurrentSampleIndex - 1 + k_SampleLength) % k_SampleLength;
		m_Samples[m_CurrentSampleIndex] = new Sample();
	}

	public static void ToAngleAxis(Rotation rotation, out float angle, out Vector3 axis)
    {
        if (Math.Abs(rotation.w) > 1.0f)
            rotation = rotation.Normal;

        angle = 2.0f * (float)Math.Acos(rotation.w);
        float s = (float)Math.Sqrt(1.0f - rotation.w * rotation.w);

        if (s < 0.001f)
        {
            axis = new Vector3(rotation.x, rotation.y, rotation.z);
        }
        else
        {
            axis = new Vector3(rotation.x / s, rotation.y / s, rotation.z / s);
        }
    }
}

