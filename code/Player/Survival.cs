using System;
using System.Threading.Tasks;
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
	[Property] public float TransSpeed {get;set;} = 1;
	[Property] public float HealthRegenTime {get;set;} = 60;

	Vrmovement vrMovement;

	HealthComponent healthComponent;

	ChunkDealer chunkDealer;

	protected override void OnStart()
	{
		chunkDealer = Scene.Components.GetInChildren<ChunkDealer>();
		healthComponent = Components.Get<HealthComponent>();
		vrMovement = Components.Get<Vrmovement>();
	}

	float staminaRanOutTime = -100;

	bool transitioning;
	protected override async void OnUpdate()
	{
		if(chunkDealer.PlayerInSafeChunk())
		{
			healthComponent.Health = MathX.Clamp(healthComponent.Health+(Time.Delta*(1/HealthRegenTime))*healthComponent.MaxHealth,0,healthComponent.MaxHealth);
		}
		if(transitioning)
			vrMovement.Camera.Brightness = MathX.Lerp(vrMovement.Camera.Brightness, 0, Time.Delta*TransSpeed);

		if(healthComponent.Health <= 0)
		{
			if(!transitioning) Transition("scenes/menu.scene");
			
			return;
		}
		
		bool running = vrMovement.characterController.IsOnGround && vrMovement.characterController.Velocity.Length > (vrMovement.WalkSpeed+vrMovement.RunSpeed)/2;
		Stamina = MathX.Clamp(
			Stamina + (running ? Time.Delta * -(1/StaminaUseTime) : 1/StaminaTime * Time.Delta),
			0,Hunger);

		Hunger = MathX.Clamp(
			Hunger + (running ? Time.Delta * -(1/HungerUseTime) : 0) ,0 , 1);

		if(Stamina <= 0.01f) staminaRanOutTime = Time.Now;
		vrMovement.canRun = Time.Now - staminaRanOutTime > BreathTime;
	}

	public async void Transition(string file)
	{
		vrMovement.inTransition = true;
		transitioning = true;
		vrMovement.Camera.Brightness = 1;
		while (vrMovement.Camera.Brightness > 0.00001f)
		{
			await Task.Delay(1);
		}

		Scene.LoadFromFile(file);
	}
}
