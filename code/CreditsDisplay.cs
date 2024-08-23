using Sandbox;
using Sandbox.UI;

public sealed class CreditsDisplay : Component
{
	[Property] public List<Credit> Credits {get;set;}
	[Property] public float DisplayTime {get;set;} = 1;
	[Property] public ModelRenderer Display {get;set;}
	[Property] public TextRenderer Author {get;set;}

	public class Credit
	{
		[KeyProperty] [TextArea] public string Author {get;set;}
		[KeyProperty] public Material Image {get;set;}
	}
	float time = 1000;
	List<Credit> Pool = new List<Credit>();
	protected override void OnUpdate()
	{
		time += Time.Delta;
		if(Pool.Count == 0)
			Pool = Credits.OrderBy(x => Game.Random.Next()).ToList();
		
		if(time > DisplayTime)
		{
			time = 0;
			
			Display.MaterialOverride = Pool[0].Image;
			Author.Text = Pool[0].Author;
			Pool.RemoveAt(0);
			
		}
	}

	protected override void OnStart()
	{
		FileSystem.Data.WriteAllText("Credits.txt", FileSystem.Mounted.ReadAllText("Credits.txt"));
	}

	public class Sequence
	{
		public string Source { get; set; }
		public bool IsLooping { get; set; }
		public int Frames { get; set; }
	}

	public class Root
	{
		public List<Sequence> Sequences { get; set; }
		public string OutputTypeString { get; set; }
	}
}
