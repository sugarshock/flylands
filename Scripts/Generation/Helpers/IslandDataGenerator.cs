using Godot;
using System;
using MarchingCubesProject;

namespace Islands.IslandGeneration
{
     public static class IslandDataGenerator
     {
          public static VoxelArray GenerateVoxelArray(float[,] topHeightMap, float[,] bottomHeightMap, int height)
          {    
               if(topHeightMap.GetLength(0) != bottomHeightMap.GetLength(0) || topHeightMap.GetLength(1) != bottomHeightMap.GetLength(1))
               {
                    throw new Exception("Heightmap dimensions are not equal!");
               }

               var width = topHeightMap.GetLength(0);
               var depth = topHeightMap.GetLength(1);

               var min = HeightMapHelper.Min(bottomHeightMap);
               var max = HeightMapHelper.Max(topHeightMap);

               var voxels = new VoxelArray(width, height, depth);

               var rng = new RandomNumberGenerator();

               for(int x = 0; x < width; x++)
               {
                    for(int z = 0; z < depth; z++)
                    {
                         var upper = toCoord(topHeightMap[x,z], min, max, (float) height);
                         var lower = toCoord(bottomHeightMap[x,z], min, max, (float) height);

                         for (int y = 0; y < height; y++)
                         {    
                              if(upper <= lower || upper < y || y < lower)
                              {
                                   voxels[x,y,z] = 0;
                                   voxels.Terrains[x,y,z] = TerrainType.Air;
                              }
                              else if( (y-lower) > (upper-lower) * 0.95)
                              {
                                   voxels[x,y,z] = 1;
                                   voxels.Terrains[x,y,z] = TerrainType.Grass;
                              }
                              /*else if(y < upper && y > upper * 0.8 && rng.Randf() > 0.2)
                              {
                                   voxels[x,y,z] = 1;
                                   voxels.Terrains[x,y,z] = VoxelArray.TerrainType.Sand;
                              }*/
                              else
                              {
                                   voxels[x,y,z] = 1;
                                   voxels.Terrains[x,y,z] = TerrainType.Dirt;
                              }

                         }

                    }
               }

               return voxels;
          }

          private static float toCoord(float value, float min, float max, float height)
          {    
               var b2 = height-(height * 0.1f);
               var b1 = (height * 0.1f);

               return b1 + ((value - min) * (b2 - b1)) / (max - min);
          }
     }
}


