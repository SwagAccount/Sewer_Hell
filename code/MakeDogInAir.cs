using Sandbox;
using trollface;

public sealed class MakeDogInAir : Component
{
	SkinnedModelRenderer modelRenderer;
	protected override void OnUpdate()
	{
		if(modelRenderer==null) modelRenderer = Components.Get<SkinnedModelRenderer>();
		modelRenderer.Set("State", DogAI.DogAnimState.AIR.AsInt());
	}
}
