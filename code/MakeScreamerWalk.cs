using Sandbox;

public sealed class MakeScreamerWalk : Component
{
	SkinnedModelRenderer modelRenderer;
	protected override void OnUpdate()
	{
		if(modelRenderer==null) modelRenderer =Components.Get<SkinnedModelRenderer>();
		modelRenderer.Set("Walking", true);
	}
}
