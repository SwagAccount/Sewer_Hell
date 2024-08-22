using System.Threading.Tasks;
using Sandbox;

public sealed class NixieTubeNumber : Component
{
	[Property] public TextRenderer MainNumber {get;set;}
	[Property] public PointLight pointLight {get;set;}
	[Property] public int Number {get;set;}
	[Property] public float delay {get;set;} = 0.1f;

	[Button("TestNumber")] public void TestNumber() => SetNumber(Number);

 	public async Task SetNumber(int Number)
	{
		await Task.DelaySeconds(delay);
		pointLight.Enabled = Number > -1;
		MainNumber.Enabled = Number > -1;
		MainNumber.Text = Number.ToString();
	}
}
