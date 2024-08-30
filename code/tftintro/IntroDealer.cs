using System;
using Sandbox;

public sealed class IntroDealer : Component
{
	[Property] public GameObject TrollFace  {get;set;}
	[Property] public Angles targetRot  {get;set;}
	[Property] public Vector3 targetPos  {get;set;}
	[Property] public float TurnTime  {get;set;} = 10;
	[Property] public float StingTime  {get;set;} = 4;
	[Property] public float OverallTime  {get;set;} = 10;
	[Property] public string LoadedScene {get;set;}
	[Property] public Curve BrightnessCurve {get;set;}
	[Property] public List<GameObject> OnObjects {get;set;}

	[Property] public ColorAdjustments colorAdjustments {get;set;}
	Angles startAngles;
	Vector3 startPos;
	float time;
	protected override async void OnStart()
	{
		startAngles = TrollFace.Transform.Rotation;
		startPos = TrollFace.Transform.Position;
		base.OnStart();
	}
	bool Loaded;
	protected override void OnUpdate()
	{
		time += Time.Delta;
		if(time > OverallTime && !Loaded)
		{
			Loaded = true;
			Scene.LoadFromFile(LoadedScene);
			return;
		}
		foreach(GameObject c in OnObjects)
		{
			c.Enabled = time >= StingTime;
		}
		TrollFace.Transform.Rotation = Angles.Lerp(startAngles,targetRot,easeOutQuart(MathX.Clamp(time/TurnTime,0,1)));
		TrollFace.Transform.Position = Vector3.Lerp(startPos,targetPos,easeOutQuart(MathX.Clamp(time/TurnTime,0,1)));

		colorAdjustments.Brightness = BrightnessCurve.Evaluate(time/OverallTime);
	}

	float easeOutQuart(float x) 
	{
		return 1 - MathF.Pow(1 - x, 4);
	}
}
