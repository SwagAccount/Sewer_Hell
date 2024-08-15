using System;
using Sandbox;
using Sandbox.UI;
using NativeEngine;
namespace trollface;
public sealed class Water : Component
{
	[Property] public string folder {get;set;}
	[Property] List<Texture> textures {get;set;}
	//[Property] public Texture texture {get;set;}
	[Property] public float FPS {get;set;} = 24f;
	[Property] ModelRenderer renderer {get;set;}

	protected override void OnStart()
	{
		renderer = Components.Get<ModelRenderer>();
		List<string> texturePaths = FileSystem.Mounted.FindFile(folder)
			.Where(file => file.EndsWith(".vtex", StringComparison.OrdinalIgnoreCase))
			.ToList();
		textures = new List<Texture>();
		foreach(string s in texturePaths)
		{
			textures.Add(Texture.Load($"{folder}/{s}"));
		}
	}
	protected override void OnUpdate()
	{
		renderer.SceneObject.Attributes.Set("Colour", textures[LoopClamp((int)MathF.Round(Time.Now*FPS),0,textures.Count-1)]);
	}

	public int LoopClamp(int value, int min, int max)
	{
		int range = max - min + 1;

		if (value < min)
		{
			value += range * ((min - value) / range + 1);
		}

		return min + (value - min) % range;
	}

}


