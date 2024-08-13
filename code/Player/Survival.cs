using System;
using Sandbox;
using trollface;

public sealed class Survival : Component
{
	[Property] public float StaminaTime {get;set;} = 30;
	[Property] public float StaminaUseTime {get;set;} = 15;
	[Property] public float HungerUseTime {get;set;} = 60;
	[Property] public float BreathTime {get;set;} = 1;
	[Property] public float Stamina {get;set;} = 1;
	[Property] public float Hunger {get;set;} = 1;

	Vrmovement vrMovement;

	protected override void OnStart()
	{
		vrMovement = Components.Get<Vrmovement>();
	}

	float staminaRanOutTime = -100;
	protected override void OnUpdate()
	{
		bool running = vrMovement.characterController.IsOnGround && vrMovement.characterController.Velocity.Length > (vrMovement.WalkSpeed+vrMovement.RunSpeed)/2;
		Stamina = MathX.Clamp(
			Stamina + (running ? Time.Delta * -(1/StaminaUseTime) : 1/StaminaTime * Time.Delta),
			0,Hunger);

		Hunger = MathX.Clamp(
			Hunger + (running ? Time.Delta * -(1/HungerUseTime) : 0) ,0 , 1);

		if(Stamina <= 0.01f) staminaRanOutTime = Time.Now;
		vrMovement.canRun = Time.Now - staminaRanOutTime > BreathTime;
	}
}
