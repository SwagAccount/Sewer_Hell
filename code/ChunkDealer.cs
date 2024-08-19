using System;
using Sandbox;
using trollface;

public sealed class ChunkDealer : Component
{
	[Property] public float ChunkSize {get;set;} = 5000f;
	[Property] public Vector2 SafeChunk {get;set;}
	[Property] public Vector2 ChunkCount {get;set;}
	[Property] public Vrmovement Player {get;set;}
	[Property] public GameObject ActiveChunk {get;set;}
	[Property] public bool CreateChunksNow {get;set;}
	[Property] public int ChunkDistance { get; set; } = 2;

	[Property] public bool TestChunkFind {get;set;}

	[Property] public List<List<ChunkSaver>> chunks {get;set;}
	int SetChunkDis;
	GameManager gameManager;

	protected override void OnStart()
	{
		gameManager = Scene.Components.GetInChildren<GameManager>();
		Player = Scene.Components.GetInChildren<Vrmovement>();
		SetChunkDis = ChunkDistance;
		playerChunkX = (int)SafeChunk.x;
		playerChunkY = (int)SafeChunk.y;
	}
	protected override void DrawGizmos()
	{
		if(CreateChunksNow)
		{
			CreateChunks();
			CreateChunksNow = false;
		}

		if(TestChunkFind)
		{
			TestChunkFind = false;
			UnActivateAll();
		}


		for (int x = 0; x < chunks.Count; x++)
        {
            for (int y = 0; y < chunks[x].Count; y++)
            {
				if(chunks[x][y].GameObject.Enabled) Gizmo.Draw.Line(chunks[x][y].Transform.Position, chunks[x][y].Transform.Position+Vector3.Up*50);
            }
        }
	}

	int playerChunkX;
	int playerChunkY;
	int lastPlayerChunkX = -100;
	int lastPlayerChunkY = -100;
	protected override void OnFixedUpdate()
	{
		Vector3 playerPosition = Player.characterController.Transform.Position;
        playerChunkX = MathX.FloorToInt((playerPosition.x + (ChunkCount.x * ChunkSize) / 2) / ChunkSize);
        playerChunkY = MathX.FloorToInt((playerPosition.y + (ChunkCount.y * ChunkSize) / 2) / ChunkSize);

		if(lastPlayerChunkX != playerChunkX || lastPlayerChunkY != playerChunkY)
		{
			UpdateLoadedChunks();
		}

		lastPlayerChunkX = playerChunkX;
		lastPlayerChunkY = playerChunkY;
	}
	
	void UpdateLoadedChunks()
	{
		if (Player == null)
        {
            return;
        }

		if(playerChunkX == SafeChunk.x && playerChunkY == SafeChunk.y) EnterSafeChunk();
		if((playerChunkX != SafeChunk.x || playerChunkY != SafeChunk.y) && safe) ExitSafeChunk();

		for (int x = 0; x < chunks.Count; x++)
        {
            for (int y = 0; y < chunks[x].Count; y++)
            {

                float distance = MathF.Sqrt(MathF.Pow(x - playerChunkX, 2) + MathF.Pow(y - playerChunkY, 2));

                chunks[x][y].GameObject.Enabled = distance <= ChunkDistance;
            }
        }
	}
	bool safe;
	public void UnActivateAll()
	{
		while(ActiveChunk.Children.Count > 0)
		{
			PlaceInChunk(ActiveChunk.Children[0]);
		}
	}
	public void SaveAllChunks()
	{
		for (int x = 0; x < chunks.Count; x++)
        {
            for (int y = 0; y < chunks[x].Count; y++)
            {
				bool enabled = chunks[x][y].Enabled;
				chunks[x][y].Enabled = true;
                chunks[x][y].Save();
				chunks[x][y].Enabled = enabled;
            }
        }
	}

	public void LoadAllChunks()
	{
		for (int x = 0; x < chunks.Count; x++)
        {
            for (int y = 0; y < chunks[x].Count; y++)
            {
				bool enabled = chunks[x][y].Enabled;
				chunks[x][y].Enabled = true;
                chunks[x][y].Load();
				chunks[x][y].Enabled = enabled;
            }
        }
	}
	void EnterSafeChunk()
	{
		safe = true;
		UnActivateAll();
		gameManager.Save();
		ChunkDistance = 0;
	}
	void ExitSafeChunk()
	{
		safe = false;
		ChunkDistance = SetChunkDis;
	}
	
	public void PlaceInChunk(GameObject gameObject, bool test = false)
	{
		if(!test) gameObject.Enabled = false;
		Vector3 position = gameObject.Transform.Position;

		int chunkX = MathX.FloorToInt((position.x + (ChunkCount.x * ChunkSize) / 2) / ChunkSize);
		int chunkY = MathX.FloorToInt((position.y + (ChunkCount.y * ChunkSize) / 2) / ChunkSize);

		chunkX = Math.Clamp(chunkX, 0, (int)ChunkCount.x - 1);
		chunkY = Math.Clamp(chunkY, 0, (int)ChunkCount.y - 1);

		ChunkSaver chunkSaver = chunks[chunkX][chunkY];

		gameObject.SetParent(chunkSaver.GameObject);
		if(!test) gameObject.Enabled = true;
	}

	void CreateChunks()
	{
		if(GameObject.Children.Count > 0)
		{
			GameObject chunkBackup = new GameObject{
				Name = "Chunk Backup"
			};
			while(GameObject.Children.Count > 0)
			{
				GameObject.Children[0].SetParent(chunkBackup);
			}
			chunkBackup.Enabled = false;
		}
		chunks = new List<List<ChunkSaver>>();
		
		for(int x = 0; x < ChunkCount.x; x++)
		{
			List<ChunkSaver> chunkRow = new List<ChunkSaver>();
			for(int y = 0; y < ChunkCount.y; y++)
			{
				GameObject chunk = new GameObject{
					Name = $"Chunk {x}-{y}"
				};
				chunk.SetParent(GameObject);
				chunk.Transform.LocalPosition = new Vector3(
					(x*ChunkSize) - ((ChunkCount.x*ChunkSize)/2) + ChunkSize/2,
					(y*ChunkSize) - ((ChunkCount.y*ChunkSize)/2) + ChunkSize/2,
					0);
				
				chunkRow.Add(chunk.Components.GetOrCreate<ChunkSaver>());
				
			}
			chunks.Add(chunkRow);
		}
	}
}
