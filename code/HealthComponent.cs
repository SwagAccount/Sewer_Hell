using Sandbox;
namespace trollface;
public sealed class HealthComponent : Component
{
	[Property] public GameObject lastAttacker {get;set;}
	[Property] public float Health {get;set;}
	[Property] public float MaxHealth {get;set;}
	[Property] public SoundEvent Ouch {get;set;}
	[Property] public GameObject OuchObject {get;set;}

	public void DoDamage(float Damage, GameObject from)
	{
		Health-=Damage;
		lastAttacker = from;
		if(Ouch!=null)
			Sound.Play(Ouch, Transform.Position);
	}
	protected override void OnStart()
	{
		if(OuchObject == null)
			OuchObject = GameObject;
	}
}
