﻿/* 
 * OneEuroFilter.cs
 * Author: Dario Mazzanti (dario.mazzanti@iit.it), 2016
 * 
 * This Unity C# utility is based on the C++ implementation of the OneEuroFilter algorithm by Nicolas Roussel (http://www.lifl.fr/~casiez/1euro/OneEuroFilter.cc)
 * More info on the 1€ filter by Géry Casiez at http://www.lifl.fr/~casiez/1euro/
 *
 */

using System;

class LowPassFilter 
{
	float y, a, s;
	bool initialized;

	public void setAlpha(float _alpha) 
	{
		if (_alpha<=0.0f || _alpha>1.0f)
		{
			Log.Error("alpha should be in (0.0., 1.0]");
			return;
		}
		a = _alpha;
	}

	public LowPassFilter(float _alpha, float _initval=0.0f) 
	{
		y = s = _initval;
		setAlpha(_alpha);
		initialized = false;
	}

	public float Filter(float _value) 
	{
		float result;
		if (initialized)
			result = a*_value + (1.0f-a)*s;
		else 
		{
			result = _value;
			initialized = true;
		}
		y = _value;
		s = result;
		return result;
	}

	public float filterWithAlpha(float _value, float _alpha) 
	{
		setAlpha(_alpha);
		return Filter(_value);
	}

	public bool hasLastRawValue() 
	{
		return initialized;
	}

	public float lastRawValue() 
	{
		return y;
	}

};

// -----------------------------------------------------------------

public class OneEuroFilter 
{
	float freq;
	float mincutoff;
	float beta;
	float dcutoff;
	LowPassFilter x;
	LowPassFilter dx;
	float lasttime;

	// currValue contains the latest value which have been succesfully filtered
	// prevValue contains the previous filtered value
	public float currValue {get; protected set;}
	public float prevValue {get; protected set;}

	float alpha(float _cutoff) 
	{
		float te = 1.0f/freq;
		float tau = 1.0f/(2.0f*MathF.PI*_cutoff);
		return 1.0f/(1.0f + tau/te);
	}

	void setFrequency(float _f) 
	{
		if (_f<=0.0f)
		{
			Log.Error("freq should be > 0");
			return;
		}
		freq = _f;
	}

	void setMinCutoff(float _mc) 
	{
		if (_mc<=0.0f)
		{
			Log.Error("mincutoff should be > 0");
			return;
		}
		mincutoff = _mc;
	}

	void setBeta(float _b) 
	{
		beta = _b;
	}

	void setDerivateCutoff(float _dc) 
	{
		if (_dc<=0.0f)
		{
			Log.Error("dcutoff should be > 0");
			return;
		}
		dcutoff = _dc;
	}

	public OneEuroFilter(float _freq, float _mincutoff=1.0f, float _beta=0.0f, float _dcutoff=1.0f) 
	{
		setFrequency(_freq);
		setMinCutoff(_mincutoff);
		setBeta(_beta);
		setDerivateCutoff(_dcutoff);
		x = new LowPassFilter(alpha(mincutoff));
		dx = new LowPassFilter(alpha(dcutoff));
		lasttime = -1.0f;

		currValue = 0.0f;
		prevValue = currValue;
	}

	public void UpdateParams(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
	{
		setFrequency(_freq);
		setMinCutoff(_mincutoff);
		setBeta(_beta);
		setDerivateCutoff(_dcutoff);
		x.setAlpha(alpha(mincutoff));
		dx.setAlpha(alpha(dcutoff));	
	}

	public float Filter(float value, float timestamp = -1.0f) 
	{
		prevValue = currValue;
		
		// update the sampling frequency based on timestamps
		if (lasttime!=-1.0f && timestamp != -1.0f)
			freq = 1.0f/(timestamp-lasttime);
		lasttime = timestamp;
		// estimate the current variation per second 
		float dvalue = x.hasLastRawValue() ? (value - x.lastRawValue())*freq : 0.0f; // FIXME: 0.0 or value? 
		float edvalue = dx.filterWithAlpha(dvalue, alpha(dcutoff));
		// use it to update the cutoff frequency
		float cutoff = mincutoff + beta*MathF.Abs(edvalue);
		// filter the given value
		currValue = x.filterWithAlpha(value, alpha(cutoff));

		return currValue;
	}
} ;
	

// this class instantiates an array of OneEuroFilter objects to filter each component of Vector2, Vector3, Vector4 or Rotation types
public class OneEuroFilter<T> where T : struct
{
	// containst the type of T
	Type type;
	// the array of filters
	OneEuroFilter[] oneEuroFilters;

	// filter parameters
	public float freq {get; protected set;}
	public float mincutoff {get; protected set;}
	public float beta {get; protected set;}
	public float dcutoff {get; protected set;}

	// currValue contains the latest value which have been succesfully filtered
	// prevValue contains the previous filtered value
	public T currValue {get; protected set;}
	public T prevValue {get; protected set;}

	// initialization of our filter(s)
	public OneEuroFilter(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
	{
		type = typeof(T);
		currValue = new T();
		prevValue = new T();

		freq = _freq;
		mincutoff = _mincutoff;
		beta = _beta;
		dcutoff = _dcutoff;

		if(type == typeof(Vector2))
			oneEuroFilters = new OneEuroFilter[2];

		else if(type == typeof(Vector3))
			oneEuroFilters = new OneEuroFilter[3];

		else if(type == typeof(Vector4) || type == typeof(Rotation))
			oneEuroFilters = new OneEuroFilter[4];
		else
		{
			Log.Error(type + " is not a supported type");
			return;
		}

		for(int i = 0; i < oneEuroFilters.Length; i++)
			oneEuroFilters[i] = new OneEuroFilter(freq, mincutoff, beta, dcutoff);		
	}

	// updates the filter parameters
	public void UpdateParams(float _freq, float _mincutoff = 1.0f, float _beta = 0.0f, float _dcutoff = 1.0f)
	{
		freq = _freq;
		mincutoff = _mincutoff;
		beta = _beta;
		dcutoff = _dcutoff;
		
		for(int i = 0; i < oneEuroFilters.Length; i++)
			oneEuroFilters[i].UpdateParams(freq, mincutoff, beta, dcutoff);
	}


	// filters the provided _value and returns the result.
	// Note: a timestamp can also be provided - will override filter frequency.
	public T Filter<U>(U _value, float timestamp = -1.0f) where U : struct
	{
		prevValue = currValue;
		
		if(typeof(U) != type)
		{
			Log.Error("WARNING! " + typeof(U) + " when " + type + " is expected!\nReturning previous filtered value" );
			currValue = prevValue;
	
			return (T) Convert.ChangeType(currValue, typeof(T));
		}

		if(type == typeof(Vector3))
		{
			Vector3 output = Vector3.Zero;
			Vector3 input = (Vector3) Convert.ChangeType(_value, typeof(Vector3));

			for(int i = 0; i < oneEuroFilters.Length; i++)
				output[i] = oneEuroFilters[i].Filter(input[i], timestamp);

			currValue = (T) Convert.ChangeType(output, typeof(T));
		}
		else if (type == typeof(Rotation))
		{
			Rotation output = Rotation.Identity;
			Rotation input = (Rotation) Convert.ChangeType(_value, typeof(Rotation));
            
            // Workaround that take into account that some input device sends
            // quaternion that represent only a half of all possible values.
            // this piece of code does not affect normal behaviour (when the
            // input use the full range of possible values).
            if ((Normalize(new Vector4(oneEuroFilters[0].currValue, oneEuroFilters[1].currValue, oneEuroFilters[2].currValue, oneEuroFilters[3].currValue))
                - Normalize(new Vector4(input.x, input.y, input.z, input.w))).LengthSquared > 2)
            {
                input = new Rotation(-input.x, -input.y, -input.z, -input.w);
            }

			output.x = oneEuroFilters[0].Filter(input.x, timestamp);
			output.y = oneEuroFilters[1].Filter(input.y, timestamp);
			output.z = oneEuroFilters[2].Filter(input.z, timestamp);
			output.w = oneEuroFilters[3].Filter(input.w, timestamp);

			currValue = (T) Convert.ChangeType(output, typeof(T));
		}

		return (T) Convert.ChangeType(currValue, typeof(T));
	}
	public static Vector4 Normalize(Vector4 vector)
    {
        float length = vector.Length;
        if (length > 0)
        {
            return new Vector4(vector.x / length, vector.y / length, vector.z / length, vector.w / length);
        }
        return Vector4.Zero;
    }
}
