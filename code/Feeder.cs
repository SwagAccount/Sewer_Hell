using Sandbox;

public sealed class Feeder : Component, Component.ITriggerListener
{

	[Property] private Survival survival {get; set;}
	void ITriggerListener.OnTriggerEnter(Collider other)
	{
		if(!other.Tags.Contains("food")) return;
		Food food = other.Components.Get<Food>();
		survival.Hunger += food.HungerRegen;
		survival.Stamina += food.StamRegen;
		other.GameObject.Destroy();
	}
}
