using Godot;
using System;
using System.Collections.Generic;

namespace Flylands.Helpers
{    

     public static class HeightMapHelper
     {    
          public static readonly float EMPTY_VERTEX = -101f;


          public static float[,] GenerateHeightmap(int width, int depth, float freq, float vOffset, int seed)
          {
               var noise = new FastNoiseLite();
               noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
               noise.Seed = seed;
               noise.Frequency = freq;
               noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;
               
               var map = new float[width, depth];

               for (int y = 0; y < depth; y++) {
                    for (int x = 0; x < width; x++) {      
                         var nx = 2f*(float)x/(float)width - 1f;
                         var ny = 2f*(float)y/(float)depth - 1f;
                         if(x == 0 || y == 0 || x == width-1 || y == depth-1)
                         {
                              map[x, y] = 0f;
                         } 
                         else 
                         {    
                              var value = noise.GetNoise2D(nx, ny) + vOffset;
                              map[x, y] = Mathf.Max(0, value);
                         }
                    }
               }

               return map;
          }


          public static float[,] Shape(float[,] heightmap, float blendFactor, float shapeScale, string type)
          {    
               var width = heightmap.GetLength(0);
               var depth = heightmap.GetLength(1);
               var map = new float[heightmap.GetLength(0), heightmap.GetLength(1)];
               for (int y = 0; y < depth; y++) {
                    for (int x = 0; x < width; x++) 
                    {    
                         // will range from 0 to 1
                         float nx = (2.0f*x/width - 1.0f) / shapeScale;
                         float ny = (2.0f*y/depth - 1.0f) / shapeScale;

                         var distance = ManhattanDistance(nx, ny, 0, 0);
                         var elevation = (heightmap[x,y] + (1-distance)) * blendFactor;
                         //GD.Print("X/Y:"+x+"/"+y + " NXNY:" +nx+"/"+ny + " Distance: " + distance + " Elevation: " + elevation);
                         map[x, y] = Mathf.Max(0, elevation);
                    }
               }

               return map;
          }

          public static float[,] Stepify(float[,] heightmap, int steps, bool smooth)
          {
               var width = heightmap.GetLength(0);
               var depth = heightmap.GetLength(1);
               var map = new float[heightmap.GetLength(0), heightmap.GetLength(1)];

               for (int y = 0; y < depth; y++) {
                    for (int x = 0; x < width; x++) 
                    {    
                         var elevation = Mathf.Round(heightmap[x,y] * steps) / steps;
                         map[x, y] = Mathf.Max(0, elevation);
                    }
               }

               return map;
          }

          public static float[,] Stepify2(float[,] heightmap, float exponent)
          {
               var width = heightmap.GetLength(0);
               var depth = heightmap.GetLength(1);
               var map = new float[heightmap.GetLength(0), heightmap.GetLength(1)];

               for (int y = 0; y < depth; y++) {
                    for (int x = 0; x < width; x++) 
                    {    
                         var elevation = Mathf.Round(heightmap[x,y]) + 0.5f * MathF.Pow(2*(heightmap[x,y] - Mathf.Round(heightmap[x,y])), exponent);
                         map[x, y] = Mathf.Max(0, elevation);
                    }
               }

               return map;
          }

          public static float[,] Ridgify(float[,] heightmap, float factor)
          {
               var width = heightmap.GetLength(0);
               var depth = heightmap.GetLength(1);
               var map = new float[heightmap.GetLength(0), heightmap.GetLength(1)];

               for (int y = 0; y < depth; y++) {
                    for (int x = 0; x < width; x++) 
                    {    
                         var elevation = 2f * (0.5f - Mathf.Abs(0.5f - heightmap[x, y]));
                         map[x, y] = Mathf.Max(0, elevation);
                    }
               }

               return map;
          }

          public static float[,] Errode(float[,] heightmap, float amount)
          {
               var width = heightmap.GetLength(0);
               var depth = heightmap.GetLength(1);
               var map = new float[heightmap.GetLength(0), heightmap.GetLength(1)];

               for (int y = 0; y < depth; y++) {
                    for (int x = 0; x < width; x++) 
                    {    
                         var xDist = Mathf.Min(x, width-x) +1;
                         var yDist = Mathf.Min(y, depth-y)+1;

                         var elevation = heightmap[x,y] - (amount / (xDist * yDist));
                         map[x, y] = Mathf.Max(0, elevation);
                    }
               }

               return map;
          }

          public static float[,] YScale(float[,] heightmap, float factor)
          {
               var width = heightmap.GetLength(0);
               var depth = heightmap.GetLength(1);
               var map = new float[heightmap.GetLength(0), heightmap.GetLength(1)];

               for (int y = 0; y < depth; y++) {
                    for (int x = 0; x < width; x++) 
                    {    
                         var elevation = heightmap[x,y] * factor;
                         map[x, y] = Mathf.Max(0, elevation);
                    }
               }

               return map;
          }

          public static float[,] InvertAndStretch(float[,] heightmap, float stretchFactor)
          {
               var depth = heightmap.GetLength(1);
               var width = heightmap.GetLength(0);

               var map = new float[width, depth];

               for (int x = 0; x < width; x++)
               {
                    for (int z = 0; z < depth; z++)
                    {    
                         try{
                              var value = (float) (-heightmap[x, z] * stretchFactor);
                              map[x,z] = Mathf.Min(0, value);
                         } catch (IndexOutOfRangeException e) {
                              GD.Print("Error when accessing " + x+"/"+z + "of top.");
                         }   
                    }
               }
               return map;
          }


          public static float[,] GenerateTopHeightmap(int height, int width, float freq, float waterLevel = 0.15f, float vOffset = 0, float flatteningFactor = 0.5f)
          {    
               var noise = new FastNoiseLite();
               noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
               noise.Seed = 55;
               noise.Frequency = freq;
               
               var map = new float[width, height];

               for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {      
                         var nx = 2f*(float)x/(float)width - 1f;
                         var ny = 2f*(float)y/(float)height - 1f;
                         var distance = Euclidean2Distance(nx, ny);

                         var value = noise.GetNoise2D((float) nx, (float) ny);
                         var elevation = (1 - flatteningFactor) * value + (1-distance) * flatteningFactor;
                         //elevation = Step(elevation, 12);
                         //GD.Print(elevation);
                         if(elevation < waterLevel || x == 0 || y == 0 || x == width-1 || y == height-1)
                         {
                              map[x, y] = 0f;
                         } 
                         else 
                         {
                              map[x, y] = elevation + vOffset;
                         }

                    }
               }

               return map;
          }

          public static float[,] GenerateBottomHeightmap(float[,] topHeightMap, float distortionFactor)
          {    
               var depth = topHeightMap.GetLength(1);
               var width = topHeightMap.GetLength(0);

               var bottomHeightMap = new float[width, depth];

               for (int x = 0; x < width; x++)
               {
                    for (int z = 0; z < depth; z++)
                    {    
                         try{
                              var y = -topHeightMap[x, z] * distortionFactor;
                              var value = (float) 2f * (0.5f - Mathf.Abs(0.5f - y));
                              bottomHeightMap[x,z] = (float) value;
                         } catch (IndexOutOfRangeException e) {
                              GD.Print("Error when accessing " + x+"/"+z + "of top.");
                         }
                         
                         
                    }
               }
               return bottomHeightMap;
          }

          public static float SquareBumpDistance(float x, float y)
          {    
               return 1 - (1 - x * x) * (1 - y * y);
          }
          
          public static float Euclidean2Distance(float x, float y)
          {    
               return Mathf.Min(1, (x*x + y*y)/Mathf.Sqrt(2));
          }

          public static float ManhattanDistance(float x, float y, float x0, float y0)
          {
               return (Math.Abs(x - x0) + Math.Abs(y - y0))/2;
          }

          public static float HyperboloidDistance(float x, float y)
          {
               return Mathf.Sqrt(Mathf.Pow(x,2) + Mathf.Pow(y,2) + Mathf.Pow(0.2f,2));
          }

          public static float Step(float elevation, int steps)
          {
               return Mathf.Round(elevation * steps) / steps;
          }

          public static void PrintHeightMap(float[,] heightmap)
          {
               for (int y = heightmap.GetLength(1) -1 ; y >= 0; y--)
               {    
                    var line = "#";
                    for (int x = 0; x < heightmap.GetLength(0); x++)
                    {    
                         line += heightmap[x,y] + "#";
                    }
                    GD.Print(line);
               }
          }

          public static float Min(float[,] heightmap)
          {
               var depth = heightmap.GetLength(1);
               var width = heightmap.GetLength(0);
               var min = heightmap[0,0];
               for (int x = 0; x < width; x++)
               {
                    for (int z = 0; z < depth; z++)
                    {    
                         if(heightmap[x,z] < min)
                              min = heightmap[x,z];
                    }
               }
               return min;
          }

          public static float Max(float[,] heightmap)
          {
               var depth = heightmap.GetLength(1);
               var width = heightmap.GetLength(0);
               var max = heightmap[0,0];
               for (int x = 0; x < width; x++)
               {
                    for (int z = 0; z < depth; z++)
                    {    
                         if(heightmap[x,z] > max)
                              max = heightmap[x,z];
                    }
               }
               return max;
          }
     }
}


