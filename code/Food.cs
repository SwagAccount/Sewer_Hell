using Sandbox;

public sealed class Food : Component
{
	[Property] public float HungerRegen {get;set;}
	[Property] public float StamRegen {get;set;}
	[Property] public float HealthRegen {get;set;}
	protected override void OnUpdate()
	{

	}
}
