using Sandbox;
namespace trollface;
public sealed class HealthComponent : Component
{
	[Property] public GameObject lastAttacker {get;set;}
	[Property] public float Health {get;set;}
	[Property] public float MaxHealth {get;set;}
	protected override void OnUpdate()
	{

	}
}
