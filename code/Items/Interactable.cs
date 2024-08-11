using Sandbox;
using trollface;

public sealed class Interactable : Component
{
	[Property] public Item item {get;set;}	
	[Property] public bool ShowWithoutMain {get;set;}	
	[Property] public bool interacted {get;set;}	
}
