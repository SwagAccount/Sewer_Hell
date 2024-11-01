using Sandbox;
using trollface;

public sealed class Can : Component
{
	[Property] public HealthComponent healthComponent {get;set;}
	[Property] public Rigidbody Lid {get;set;}
	[Property] public Interactable interactable {get;set;}
	[Property] public float LidForce {get;set;} = 1000;

	[Property] public Collider collider {get;set;}
	ExternalMagazine externalMagazine;
	protected override void OnStart()
	{
		externalMagazine = Components.Get<ExternalMagazine>();
		externalMagazine.CantEject = true;
		externalMagazine.CantLoad = true;
	}
	protected override void OnUpdate()
	{
		if(healthComponent.Health <= 0)
		{
			Destroy();
			externalMagazine.CantEject = false;
			externalMagazine.CantLoad = false;
			interactable.GameObject.Destroy();
			Lid.Enabled = true;
			Lid.GameObject.SetParent(Scene);
			Lid.ApplyForce(Lid.Transform.World.Up * LidForce);
			collider.Enabled = true;
		}
		if(interactable.interacted)
		{
			healthComponent.Health = 0;
		}
	}
}
