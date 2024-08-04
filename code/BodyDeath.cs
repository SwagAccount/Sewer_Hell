using Sandbox;

public sealed class BodyDeath : Component
{
	ModelPhysics modelPhysics;
	protected override async void OnStart()
	{
		modelPhysics = Components.Get<ModelPhysics>(true);
		modelPhysics.Enabled = true;
		foreach(PhysicsBody physicsBody in modelPhysics.PhysicsGroup.Bodies)
        {
            physicsBody.GravityScale = 0.05f;
            physicsBody.ApplyForce(Vector3.Random*100);
        }
		await Task.DelaySeconds(5);
		GameObject.Tags.Add("nocollide");
		await Task.DelaySeconds(2);
		GameObject.Destroy();
	}
}
