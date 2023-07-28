using Godot;
using System;

namespace Islands.IslandGeneration
{
     public partial class ArrayMeshHelper : Node
     {
          public static Mesh CreateTerrainMesh(float[,] heightmap)
          {    
               var mesh = CreateArrayMesh(heightmap);

               // Create MDT to work on the mesh 
               var meshDataTool = new MeshDataTool();
               meshDataTool.CreateFromSurface(mesh, 0);
               
               // Walk through the vertices and apply height from heightmap
               for(int i = 0; i < meshDataTool.GetVertexCount(); i++)
               {
                    var vertex = meshDataTool.GetVertex(i);
                    int x = (int) ((vertex.X + 0.5) * (heightmap.GetLength(0)-1));
                    int z = (int) ((vertex.Z + 0.5) * (heightmap.GetLength(1)-1));
                    try{
                         vertex.Y = heightmap[x, z];
                    } catch (IndexOutOfRangeException e)
                    {
                         GD.Print("ERROR at " + x + "/" + z);
                    }
                    

                    meshDataTool.SetVertex(i, vertex);
               }

               // save changes to new surface in mesh
               mesh.ClearSurfaces();
               meshDataTool.CommitToSurface(mesh);

               return mesh;
          }

          private static ArrayMesh CreateArrayMesh(float[,] heightmap)
          {
               // Create plane first
               var plane = new PlaneMesh();
               plane.Size = new Vector2(1,1);
               plane.SubdivideWidth = heightmap.GetLength(0);
               plane.SubdivideDepth = heightmap.GetLength(1);

               // Turn it into moldable ArrayMesh
               var mesh = new ArrayMesh();
               mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, plane.GetMeshArrays());


               return mesh;
          }
     }
}