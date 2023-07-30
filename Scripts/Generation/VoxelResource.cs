using System;
using Godot;
using Flylands.MarchingCubesProject;
     
[Tool]
/// <summary>
/// Unfortunetly, Godot is unable to export or serialize the actual 3D VoxelArray. Therefore, if we want to keep your voxel data in our Island scenes,
/// we need to save it redundantly in this custom resource.
/// </summary>
public partial class VoxelResource : Resource
{   
     [Export]
     public Godot.Collections.Dictionary Voxels { get; set; }
     [Export]
     public Godot.Collections.Dictionary Terrains { get; set; }

     [Export]
     public int Width;
     
     [Export]
     public int Height;
     
     [Export]
     public int Depth;

     public VoxelResource()
     {

     }

     public VoxelResource(float[,,] voxels, TerrainType[,,] terrains)
     {    
          Width = voxels.GetLength(0);
          Height = voxels.GetLength(1);
          Depth = voxels.GetLength(2);
          Voxels = new Godot.Collections.Dictionary();
          Terrains = new Godot.Collections.Dictionary();
          for(int x = 0; x < voxels.GetLength(0); x++)
          {
               for(int y = 0; y < voxels.GetLength(1); y++)
               {
                    for(int z = 0; z < voxels.GetLength(2); z++)
                    {
                         Voxels[new Vector3I(x,y,z)] = voxels[x,y,z];
                         Terrains[new Vector3I(x,y,z)] = (int)terrains[x,y,z];
                    }
               }
          }
     }

     public VoxelArray ToVoxelArray()
     {
          var array = new VoxelArray(Width, Height, Depth);
          
          for(int x = 0; x < Width; x++)
          {
               for(int y = 0; y < Height; y++)
               {
                    for(int z = 0; z < Depth; z++)
                    {
                         array[x,y,z] = (float)Voxels[new Vector3I(x,y,z)];
                         array.Terrains[x,y,z] = (TerrainType) ((int) Terrains[new Vector3I(x,y,z)]);
                    }
               }
          }
          return array;
     }
}