using Godot;
using System;

namespace Flylands
{
	[Tool]
	public partial class IslandGenerator : Node3D
	{	
		
		[Export]
		/// <summary>
		/// Used as a "generate" button in the inspector.
		/// </summary>
		public bool Generate {get => false; set => GenerateIsland();}

		[Export]
		public string SAVE_PATH {get; set;} = "res://Islands/Scenes/";

		[Export]
		public String FileName {get; set;} = "Island";

		[Export]
		/// <summary>
		/// Used as a "save" button in the inspector.
		/// </summary>
		public bool Save {get => false; set => SaveCurrentToScene();}

		private Island island;
		private IslandBaseGenerator baseGenerator;


		
		public override void _Ready()
		{	
			GenerateIsland();
		}

		/// <summary>
		/// Generates an island based on the current configuration of the subgenerators (currently only IslandBaseGenerator).
		/// </summary>
		public void GenerateIsland()
		{	
			Clean();
			
			PackedScene ISLAND_PACK = ResourceLoader.Load<PackedScene>("res://Scenes/Island.tscn");
			island = ISLAND_PACK.Instantiate<Island>();
			AddChild(island);
			

			var baseTupel = GetNode<IslandBaseGenerator>("BaseGenerator").GenerateBase();
			island.Mesh = baseTupel.mesh;
			island.Voxels = baseTupel.voxelArray;

			// Unfortunetly, we need to redundantly save the voxels in this custom resource, because Godot cannot export the 3D VoxelArray.
			island.VoxelResource = new VoxelResource(island.Voxels.Voxels, island.Voxels.Terrains);

			// For my application, I generate both a Trimesh and a Polygon  collider shape for the island.
			// You should probably choose one to improve performance.
			island.TrimeshShape = island.Mesh.CreateTrimeshShape();
			island.PolygonShape = island.Mesh.CreateConvexShape();

			// Passing the generated mesh to the Grass node and rebuild grass MultiMesh.
			island.GetNode("Grass").Set("mesh", baseTupel.Item2);
			island.GetNode("Grass").Call("rebuild");
			
		}


		/// <summary>
		/// Removes all currently cached Islands from this subtree.
		/// </summary>
		public void Clean()
		{
			foreach(Node node in GetChildren())
			{	
				if(node is MeshInstance3D mesh)
					RemoveChild(mesh);
			}
			island = null;
		}


		/// <summary>
		/// Saves currently generated Island to a .tscn file in SAVE_PATH.
		/// </summary>
		public void SaveCurrentToScene()
		{
			if(island == null)
				return;
			
			var pack = new PackedScene();
			pack.Pack(island);
			if (ResourceSaver.Save(pack, SAVE_PATH+FileName+".tscn") == Error.Ok)
			{
				FileName = "";
				GD.Print("Saved island: " + SAVE_PATH+FileName+".tscn");
			}
		}
	}
}


