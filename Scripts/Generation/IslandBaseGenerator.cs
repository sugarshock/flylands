using Godot;
using System;
using Flylands.Helpers;
using MarchingCubesProject;

[Tool]
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

	[Export]
	public bool SmoothNormals {get; set;} = false;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public (VoxelArray voxelArray, Mesh mesh) GenerateBase()
	{	
		var heightmap = HeightMapHelper.GenerateHeightmap(Size.X,Size.Z,Frequency,vOffset, Seed);

		
		var topHeightmap = HeightMapHelper.Shape(heightmap, ShapeFactor, ShapeScale, "EUCLIDIAN2");
		topHeightmap = HeightMapHelper.Errode(topHeightmap, ErrodingFactor);
		topHeightmap = HeightMapHelper.Stepify(topHeightmap, Steps, false);
		topHeightmap = HeightMapHelper.YScale(topHeightmap, vScale);


		var bottomHeightMap = HeightMapHelper.Errode(heightmap, ErrodingFactor * 2);
		//bottomHeightMap = HeightMapHelper.Shape(bottomHeightMap, ShapeFactor/2, ShapeScale/2, "EUCLIDIAN2");
		bottomHeightMap = HeightMapHelper.InvertAndStretch(bottomHeightMap, StretchFactor);

		//var mesh = ArrayMeshHelper.CreateTerrainMesh(topHeightmap);
		//var botMesh = ArrayMeshHelper.CreateTerrainMesh(bottomHeightMap);
		var voxelArray = VoxelArrayHelper.GenerateVoxelArray(topHeightmap, bottomHeightMap, Size.Y);
		DrawBlocks(voxelArray);
		var mesh = MarchingCubesHelper.GetMeshFrom(voxelArray, smoothNormals: SmoothNormals);

		return  (voxelArray, mesh);
	}

	
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
