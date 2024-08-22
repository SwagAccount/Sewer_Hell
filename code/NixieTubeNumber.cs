using Sandbox;

public sealed class NixieTubeNumber : Component
{
	[Property] public List<TextRenderer> TextRenderers {get;set;}
	[Property] public Color Off {get;set;}
	[Property] public Color On {get;set;}
	[Property] public int Number {get;set;}

	[Button("TestNumber")] public void TestNumber() => SetNumber(Number);

 	public void SetNumber(int Number)
	{
		for(int i = 0; i < TextRenderers.Count; i++)
		{
			TextRenderers[i].Color = i==Number ? On : Off;
		}
	}
}
