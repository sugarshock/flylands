using Godot;
using System;
using System.Collections.Generic;
using MarchingCubesProject;
using System.Linq;

namespace Islands.IslandGeneration
{
     public static class MarchingCubesHelper
     {
          public enum MARCHING_MODE {  CUBES, TETRAHEDRON };


          public static Mesh GetMeshFrom(VoxelArray voxels, MARCHING_MODE mode = MARCHING_MODE.CUBES, bool smoothNormals = false, bool drawNormals = false)
          {
               Marching marching = new MarchingCubes();

               //Surface is the value that represents the surface of mesh
               //For example the perlin noise has a range of -1 to 1 so the mid point is where we want the surface to cut through.
               //The target value does not have to be the mid point it can be any value with in the range.
               marching.Surface = 0.5f;

               var verts = new List<Vector3>();
               var normals = new List<Vector3>();
               var indices = new List<int>();
               var uv1 = new List<Vector3>();
               var uv2 = new List<Vector3>();

               marching.Generate(voxels, verts, indices, uv1, uv2);



               if (smoothNormals)
               {
                    for (int i = 0; i < verts.Count; i++)
                    {
                         //Presumes the vertex is in local space where
                         //the min value is 0 and max is width/height/depth.
                         Vector3 p = verts[i];

                         float u = p.X / (voxels.Width - 1.0f);
                         float v = p.Y / (voxels.Height - 1.0f);
                         float w = p.Z / (voxels.Depth - 1.0f);

                         Vector3 n = voxels.GetNormal(u, v, w);
                         normals.Add(n);
                    }
               }

               var position = new Vector3(-voxels.Width / 2, -voxels.Height / 2, -voxels.Depth / 2);

               return CreateMesh(verts, normals, indices, uv1, uv2, position);
          }


          private static Mesh CreateMesh(List<Vector3> verts, List<Vector3> normals, List<int> indices, List<Vector3> uv1, List<Vector3> uv2, Vector3 position)
          {
               var arrMesh = new ArrayMesh();
               var arrays = new Godot.Collections.Array();
               arrays.Resize((int)Mesh.ArrayType.Max);

               arrays[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
               arrays[(int)Mesh.ArrayType.Index] = indices.ToArray();
               arrays[(int)Mesh.ArrayType.TexUV2] = uv1.Select(bar => new Vector2(bar.X, bar.Y)).ToArray(); // MAP TO COLOR HERE!
               arrays[(int)Mesh.ArrayType.Color] = uv2.Select(vox => new Color(vox.X * 0.1f, vox.Y * 0.1f, vox.Z * 0.1f)).ToArray();// MAP TO COLOR HERE!
               //arrays[(int)Mesh.ArrayType.Color] = uv2.Select(vox => new Color(1.0f * 0.1f, 1.0f * 0.1f, 1.0f * 0.1f)).ToArray();
               //arrays[(int)Mesh.ArrayType.Color] = uv2.Select(vox => new Color(0.1f, 0.1f, 0.1f)).ToArray();// MAP TO COLOR HERE!

               if(normals.Count == verts.Count)
                    arrays[(int)Mesh.ArrayType.Normal] = normals.ToArray();

               arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
               
               return CorrectCenter(arrMesh);
          }

          private static Mesh CorrectCenter(ArrayMesh mesh)
          {
               var aabb = mesh.GetAabb();
               var center = new Vector3(aabb.Size.X/2, aabb.Size.Y, aabb.Size.Z/2);
               var mdt = new MeshDataTool();
               mdt.CreateFromSurface(mesh, 0);

               for(int i = 0; i < mdt.GetVertexCount(); i++)
               {
                    var vertex = mdt.GetVertex(i);
                    mdt.SetVertex(i, vertex - center);
               }
               mesh.ClearSurfaces();
               mdt.CommitToSurface(mesh);
               return mesh;
          }

          public static Color ColorFrom(TerrainType terrainType)
          {    
               switch(terrainType)
               {    
                    case TerrainType.Grass:
                         return Colors.Green;
                    case TerrainType.Dirt:
                         return Colors.Brown;
                    case TerrainType.Sand:
                         return Colors.SandyBrown;
                    case TerrainType.Rock:
                         return Colors.DarkGray;
               }
               
               // all other terrains are supposed to be colored by the shader (depending on palette)
               return new Color(0);
          }
     }
}

