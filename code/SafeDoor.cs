using Sandbox;
namespace trollface;

public sealed class SafeDoor : Component, Component.ITriggerListener
{
	Vrmovement vrmovement;
	[Property] public bool Open {get;set;}
	[Property] public SkinnedModelRenderer ModelRenderer {get;set;}
	protected override void OnStart()
	{
		vrmovement = Scene.Components.GetInChildren<Vrmovement>();
	}
	void ITriggerListener.OnTriggerEnter(Collider other)
	{
		if(other.GameObject != vrmovement.characterController.GameObject) return;
		Open = true;
		ModelRenderer.Set("Open", true);
	}
	void ITriggerListener.OnTriggerExit(Collider other)
	{
		if(other.GameObject != vrmovement.characterController.GameObject) return;
		Open = false;
		ModelRenderer.Set("Open", false);
	}
}
