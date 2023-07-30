using Godot;
using System;
using Flylands.MarchingCubesProject;
using Flylands.Helpers;

namespace Flylands
{
	[Tool]
	public partial class Island : MeshInstance3D
	{	
		[Export]
		public ConcavePolygonShape3D TrimeshShape;

		[Export]
		public ConvexPolygonShape3D PolygonShape;

		/// <summary>
		/// Actual voxel data you should modify, read-out, etc. at runtime. This is *not* serialized to a scene, since Godot cannot export 3-dimensional arrays.
		/// </summary>
		public VoxelArray Voxels;

		[Export]
		/// <summary>
		/// Redundant resource to allow Godot to serialize the voxel data for saving as scenes.
		/// </summary>
		public VoxelResource VoxelResource;

	
		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{	
			// Deserializing the Godot resource to a proper VoxelArray
			if(Voxels == null && VoxelResource != null)
				Voxels = VoxelResource.ToVoxelArray();
			
		}

		/// <summary>
		/// Experimental method to remove the voxel hit at position. Still pretty inaccurate and buggy, poor performance.
		/// </summary>
		public void Dig(Vector3 globalPos)
		{	
			// Convert the global digging position to local position
			var localPos = ToLocal(globalPos);

			// Since the VoxelArray origin is an upper corner and the island origin is at the center (see MarchingCubesHelper.CorrectCenter())
			// we need to reset the offset to map the position to voxel coordinates.
			var aabb = Mesh.GetAabb();
               var center = new Vector3(aabb.Size.X/2, aabb.Size.Y, aabb.Size.Z/2);
			localPos += center;

			// Convert the position to normalized UVW values (values between 0 and 1)
			var uvw = new Vector3(localPos.X/aabb.Size.X, localPos.Y/aabb.Size.Y, localPos.Z/aabb.Size.Z);

			// Map UVWs to discrete coordinates.
			var coords = Voxels.GetCoords(uvw.X, uvw.Y, uvw.Z);

			// If we hit an air voxel, stop here.
			if(Voxels.Terrains[coords.X, coords.Y, coords.Z] == TerrainType.Air)
				return;
			
			// Otherwise, set voxel value to 0 and its type to air.
			Voxels[coords.X, coords.Y, coords.Z] = 0;
			Voxels.Terrains[coords.X, coords.Y, coords.Z] = TerrainType.Air;

			// Rebuild the mesh. Performance not suitable for real-time application yet.
			RefreshMesh();
		}

		/// <summary>
		/// Rebuild the mesh based on the current Voxels data. Performance not suitable for real-time application yet. 
		/// </summary>
		public void RefreshMesh(){
			this.Mesh = MarchingCubesHelper.GetMeshFrom(Voxels, smoothNormals: true);
			GetNode("Grass").Set("mesh", Mesh);
		}
	}
}

