using System.Collections;
using System.Collections.Generic;
using Godot;
using Islands.IslandGeneration;

namespace MarchingCubesProject
{

    public enum TerrainType {Air, Dirt, Grass, Rock, Sand}
    /// <summary>
    /// A helper class to hold voxel data.
    /// </summary>
    
    [Tool]
    public partial class VoxelArray
    {   

        /// <summary>
        /// Create a new voxel array.
        /// </summary>
        /// <param name="width">The size of the voxels on the x axis.</param>
        /// <param name="height">The size of the voxels on the y axis.</param>
        /// <param name="depth">The size of the voxels on the z axis.</param>
        public VoxelArray(int width, int height, int depth)
        {
            Voxels = new float[width, height, depth];
            Terrains = new TerrainType[width, height, depth];
            FlipNormals = true;
        }

        // Make sure you provide a parameterless constructor.
        // In C#, a parameterless constructor is different from a
        // constructor with all default values.
        // Without a parameterless constructor, Godot will have problems
        // creating and editing your resource via the inspector.
        public VoxelArray() : this(0, 0, 0) {}

        /// <summary>
        /// The size of the voxels on the x axis.
        /// </summary>
        public int Width => Voxels.GetLength(0);

        /// <summary>
        /// The size of the voxels on the y axis.
        /// </summary>
        public int Height => Voxels.GetLength(1);

        /// <summary>
        /// The size of the voxels on the z axis.
        /// </summary>
        public int Depth => Voxels.GetLength(2);

        /// <summary>
        /// 
        /// </summary>
        public bool FlipNormals { get; set; }

        /// <summary>
        /// Get/set the voxel data.
        /// </summary>
        /// <param name="x">The index on the x axis.</param>
        /// <param name="y">The index on the y axis.</param>
        /// <param name="z">The index on the z axis.</param>
        /// <returns>The voxels data.</returns>
        public float this[int x, int y, int z]
        {
            get { return Voxels[x, y, z]; }
            set { Voxels[x, y, z] = value; }
        }

        /// <summary>
        /// THe voxel data.
        /// </summary>
        public float[,,] Voxels { get; set; }

        /// <summary>
        /// THe voxel type.
        /// </summary>
        public TerrainType[,,] Terrains { get; set; }

        /// <summary>
        /// Get the voxel data at clamped indices x,y,z.
        /// </summary>
        /// <param name="x">The index on the x axis.</param>
        /// <param name="y">The index on the y axis.</param>
        /// <param name="z">The index on the z axis.</param>
        /// <returns>The voxels data.</returns>
        public float GetVoxel(int x, int y, int z)
        {
            x = Mathf.Clamp(x, 0, Width - 1);
            y = Mathf.Clamp(y, 0, Height - 1);
            z = Mathf.Clamp(z, 0, Depth - 1);
            return Voxels[x, y, z];
        }

        /// <summary>
        /// Get the voxel data at normalized (0-1) clamped indices u,v,w.
        /// </summary>
        /// <param name="u">The normalized index (0-1) on the x axis.</param>
        /// <param name="v">The normalized index (0-1) on the y axis.</param>
        /// <param name="w">The normalized index (0-1) on the z axis.</param>
        /// <returns>The voxel data</returns>
        public float GetVoxel(float u, float v, float w)
        {
            float x = u * (Width - 1);
            float y = v * (Height - 1);
            float z = w * (Depth - 1);

            int xi = (int)Mathf.Floor(x);
            int yi = (int)Mathf.Floor(y);
            int zi = (int)Mathf.Floor(z);

            float v000 = GetVoxel(xi, yi, zi);
            float v100 = GetVoxel(xi + 1, yi, zi);
            float v010 = GetVoxel(xi, yi + 1, zi);
            float v110 = GetVoxel(xi + 1, yi + 1, zi);

            float v001 = GetVoxel(xi, yi, zi + 1);
            float v101 = GetVoxel(xi + 1, yi, zi + 1);
            float v011 = GetVoxel(xi, yi + 1, zi + 1);
            float v111 = GetVoxel(xi + 1, yi + 1, zi + 1);

            float tx = Mathf.Clamp(x - xi, 0, 1);
            float ty = Mathf.Clamp(y - yi, 0, 1);
            float tz = Mathf.Clamp(z - zi, 0, 1);

            //use bilinear interpolation the find these values.
            float v0 = BLerp(v000, v100, v010, v110, tx, ty);
            float v1 = BLerp(v001, v101, v011, v111, tx, ty);

            //Now lerp those values for the final trilinear interpolation.
            return Lerp(v0, v1, tz);
        }

        /// <summary>
        /// Get the voxels normal at the indices x,y,z.
        /// The normal will be all zeros in any areas of the voxels that are constant.
        /// </summary>
        /// <param name="x">The index on the x axis.</param>
        /// <param name="y">The index on the y axis.</param>
        /// <param name="z">The index on the z axis.</param>
        /// <returns></returns>
        public Vector3 GetNormal(int x, int y, int z)
        {
            var n = GetFirstDerivative(x, y, z);

            if (FlipNormals)
                return n.Normalized() * -1;
            else
                return n.Normalized();
        }

        /// <summary>
        /// Get the voxels normal at the normalized indices u,v,w.
        /// The normal will be all zeros in any areas of the voxels that are constant.
        /// </summary>
        /// <param name="u">The normalized index (0-1) on the x axis.</param>
        /// <param name="v">The normalized index (0-1 on the y axis.</param>
        /// <param name="w">The normalized index (0-1 on the z axis.</param>
        /// <returns></returns>
        public Vector3 GetNormal(float u, float v, float w)
        {
            var n = GetFirstDerivative(u, v, w);

            if (FlipNormals)
                return n.Normalized() * -1;
            else
                return n.Normalized();
        }

        /// <summary>
        /// Get the voxels first derivative at the indices x,y,z.
        /// The derivative will be all zeros in any areas of the voxels that are constant.
        /// The derivative is calculated using back and forward finite differance.
        /// </summary>
        /// <param name="x">The index on the x axis.</param>
        /// <param name="y">The index on the y axis.</param>
        /// <param name="z">The index on the z axis.</param>
        /// <returns></returns>
        public Vector3 GetFirstDerivative(int x, int y, int z)
        {
            float dx_p1 = GetVoxel(x + 1, y, z);
            float dy_p1 = GetVoxel(x, y + 1, z);
            float dz_p1 = GetVoxel(x, y, z + 1);

            float dx_m1 = GetVoxel(x - 1, y, z);
            float dy_m1 = GetVoxel(x, y - 1, z);
            float dz_m1 = GetVoxel(x, y, z - 1);

            float dx = (dx_p1 - dx_m1) * 0.5f;
            float dy = (dy_p1 - dy_m1) * 0.5f;
            float dz = (dz_p1 - dz_m1) * 0.5f;

            return new Vector3(dx, dy, dz);
        }

        /// <summary>
        /// Get the voxels first derivative at the normalized indices u,v,w.
        /// The first derivative will be all zeros in any areas of the voxels that are constant.
        /// The derivative is calculated using back and forward finite differance.
        /// </summary>
        /// <param name="u">The normalized index (0-1) on the x axis.</param>
        /// <param name="v">The normalized index (0-1 on the y axis.</param>
        /// <param name="w">The normalized index (0-1 on the z axis.</param>
        /// <returns></returns>
        public Vector3 GetFirstDerivative(float u, float v, float w)
        {
            const float h = 0.005f;
            const float hh = h * 0.5f;
            const float ih = 1.0f / h;

            float dx_p1 = GetVoxel(u + hh, v, w);
            float dy_p1 = GetVoxel(u, v + hh, w);
            float dz_p1 = GetVoxel(u, v, w + hh);

            float dx_m1 = GetVoxel(u - hh, v, w);
            float dy_m1 = GetVoxel(u, v - hh, w);
            float dz_m1 = GetVoxel(u, v, w - hh);

            float dx = (dx_p1 - dx_m1) * ih;
            float dy = (dy_p1 - dy_m1) * ih;
            float dz = (dz_p1 - dz_m1) * ih;

            return new Vector3(dx, dy, dz);
        }

        public Vector3I GetCoords(float u, float v, float w)
        {
            int x = (int) (u * (float) (Width - 1));
            int y = (int) (v * (float) (Height - 1));
            int z = (int) (w * (float) (Depth - 1));

            return new Vector3I(x,y,z);
        }

        public Texture3D GetTerrainSampler()
        {
            var images = new Godot.Collections.Array<Image>();
            for(int z = 0; z < Depth; z++)
            {
                var image = Image.Create(Width, Height, false, Image.Format.Rgba8);
                for(int y = 0; y < Height; y++)
                {
                    for(int x = 0; x < Width; x++)
                    {
                        image.SetPixel(x,y,MarchingCubesHelper.ColorFrom(Terrains[x,y,z]));
                    }
                }
                images[z] = image;
            }
            var sampler = new ImageTexture3D();
            sampler.Create(Image.Format.Rgba8, Width, Height, Depth, false, images);
            return sampler;
        }

        /// <summary>
        /// Linear interpolation.
        /// </summary>
        /// <param name="v0"></param>
        /// <param name="v1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private static float Lerp(float v0, float v1, float t)
        {
            return v0 + (v1 - v0) * t;
        }

        /// <summary>
        /// Bilinear interpolation.
        /// </summary>
        /// <param name="v00"></param>
        /// <param name="v10"></param>
        /// <param name="v01"></param>
        /// <param name="v11"></param>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        private static float BLerp(float v00, float v10, float v01, float v11, float tx, float ty)
        {
            return Lerp(Lerp(v00, v10, tx), Lerp(v01, v11, tx), ty);
        }

    }


}