using Godot;
using Flylands.Helpers;
using Flylands.MarchingCubesProject;


namespace Flylands 
{
	[Tool]
	/// <summary>
     /// Subgenerator to generate an Islands' base mesh based on a number of parameters.
     /// </summary>
	public partial class IslandBaseGenerator : Node3D
	{	
		[Export]
		public float Frequency {get; set;} = 0.3f;

		[Export]
		public float vOffset {get; set;} = 0.5f;

		[Export]
		public float vScale {get; set;} = 0.5f;

		[Export]
		public int Seed {get; set;} = 55;

		[Export]
		public float ShapeFactor {get; set;} = 0.5f;
		

		[Export]
		public float ShapeScale {get; set;} = 1.0f;

		[Export]
		public int Steps {get; set;} = 6;

		[Export]
		public float ErrodingFactor {get; set;} = 5f;

		[Export]
		public Vector3I Size {get; set;} = new Vector3I(30,50,30);

		[Export]
		public float StretchFactor {get; set;} = 1f;
		

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

          /// <summary>
          /// Generates an Islands' base mesh based on the given parameters. Returns the voxel data and the finished mesh.
          /// </summary>
		public (VoxelArray voxelArray, Mesh mesh) GenerateBase()
		{	
			// Generate the original heightmap.
			var heightmap = HeightMapHelper.GenerateHeightmap(Size.X,Size.Z,Frequency,vOffset, Seed);

			// Apply a number of modifiers to create the top half of the island.
			var topHeightmap = HeightMapHelper.Shape(heightmap, ShapeFactor, ShapeScale, "EUCLIDIAN2");
			topHeightmap = HeightMapHelper.Errode(topHeightmap, ErrodingFactor);
			topHeightmap = HeightMapHelper.Stepify(topHeightmap, Steps);
			topHeightmap = HeightMapHelper.HeightScale(topHeightmap, vScale);


			// Apply modifiers and InvertAndStretch to create the bottom half.
			var bottomHeightMap = HeightMapHelper.Errode(heightmap, ErrodingFactor * 2);
			//bottomHeightMap = HeightMapHelper.Shape(bottomHeightMap, ShapeFactor/2, ShapeScale/2, "EUCLIDIAN2");
			bottomHeightMap = HeightMapHelper.InvertAndStretch(bottomHeightMap, StretchFactor);

			// Generate the voxel data between the two halves. 
			var voxelArray = VoxelArrayHelper.GenerateVoxelArray(topHeightmap, bottomHeightMap, Size.Y);

			// Use MarchingCubes to generate an actual mesh from the voxel data.
			var mesh = MarchingCubesHelper.GetMeshFrom(voxelArray);
			
			// Draw a debug island from minecraft blocks
			DrawBlocks(voxelArray);

			// Return tuple with the voxel data and the mesh representation.
			return  (voxelArray, mesh);
		}

		/// <summary>
          /// Draws a minecraft-like block representation of the island for debugging.
          /// </summary>
		private void DrawBlocks(VoxelArray voxelArray)
		{	
			var voxels = voxelArray.Terrains;
			var island = GetNode<GridMap>("BlockIsland");
			island.Clear();
			for(var x = 0; x < voxels.GetLength(0); x++)
			{
				for(var y = 0; y < voxels.GetLength(1); y++)
				{
					for(var z = 0; z < voxels.GetLength(2); z++)
					{
						switch(voxels[x,y,z])
						{
							case TerrainType.Grass:
								island.SetCellItem(new Vector3I(x,y,z), 0);
								break;
							case TerrainType.Dirt:
								island.SetCellItem(new Vector3I(x,y,z), 1);
								break;
							case TerrainType.Rock:
								island.SetCellItem(new Vector3I(x,y,z), 2);
								break;
							case TerrainType.Sand:
								island.SetCellItem(new Vector3I(x,y,z), 2);
								break;
						}
					}
				}
			}
		}
	}
}

