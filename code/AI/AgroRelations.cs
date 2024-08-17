using Sandbox;
namespace trollface;
public sealed class AgroRelations : Component
{
	[Property] public string Faction {get; set;}
	[Property] public List<string> Enemies {get; set;}
	[Property] public Vector3 attackPoint {get; set;}

	[Property] public GameObject ObjectRef {get;set;}

	protected override void OnAwake()
	{
		if(ObjectRef == null) ObjectRef = GameObject;
	}
}
