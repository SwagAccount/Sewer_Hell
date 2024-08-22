using Sandbox;
namespace trollface;
public sealed class HealthComponent : Component
{
	[Property] public GameObject lastAttacker {get;set;}
	[Property] public float Health {get;set;}
	[Property] public float MaxHealth {get;set;}

	public void DoDamage(float Damage, GameObject from)
	{
		Health-=Damage;
		lastAttacker = from;
	}
	protected override void OnUpdate()
	{

	}
}
