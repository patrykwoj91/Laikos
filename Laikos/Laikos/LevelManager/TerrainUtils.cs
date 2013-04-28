using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Laikos
{
    public static class TerrainUtils
    {
        public static T[,] Reshape1DTo2D<T>(T[] vertices, int width, int height)
        {
            T[,] vertexArray = new T[width, height];
            int i = 0;
            for (int h = 0; h < height; h++)
                for (int w = 0; w < width; w++)
                    vertexArray[w, h] = vertices[i++];

            return vertexArray;
        }

        public static T[] Reshape2DTo1D<T>(T[,] array2D)
        {
            int width = array2D.GetLength(0);
            int height = array2D.GetLength(1);
            T[] array1D = new T[width * height];

            int i = 0;
            for (int z = 0; z < height; z++)
                for (int x = 0; x < width; x++)
                    array1D[i++] = array2D[x, z];

            return array1D;
        }

        public static BoundingBox CreateBoundingBox(VertexMultiTextured[,] vertexArray)
        {
            List<Vector3> pointList = new List<Vector3>();
            foreach (VertexMultiTextured vertex in vertexArray)
                pointList.Add(vertex.Position);

            return BoundingBox.CreateFromPoints(pointList);
        }

        //Loading data from bitmap file about height in our map.
        public static void LoadHeightData(Texture2D heightMap, ref float[,] heightData, ref int width, ref int height)
        {

            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            width = heightMap.Width;
            height = heightMap.Height;
            //Getting data about colors in heightmap file
            Color[] heightMapColors = new Color[width * height];
            heightMap.GetData(heightMapColors);

            //Initializing heightData array
            heightData = new float[width, height];

            //In this loop we are going to fill heightData 
            //with numbers based on color of heightmap file
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    //Loading data based on red color 0 - white 255 -black
                    heightData[x, y] = heightMapColors[x + y * width].R / 15.0f;
                    //Setting data about maximum and minimum point in map
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            //In this loop we are going to make sure that every point in map is < 60
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 60.0f;
        }

        //Setting up position and texture coordinates of our vertices in triangles.
        //We are not connecting them yet, they are just points.
        //Connection between them will be made in SetUpIndices() function
        public static void SetUpVertices(float[,] heightData, ref VertexMultiTextured[] vertices, int width, int height)
        {
            vertices = new VertexMultiTextured[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //Setting position and texturecoordinates of each vertex
                    vertices[x + y * width].Position = new Vector3(x, heightData[x, y], y);
                    vertices[x + y * width].TextureCoordinate.X = (float)x / 80.0f;
                    vertices[x + y * width].TextureCoordinate.Y = (float)y / 80.0f;


                    //Setting weights for each texture
                    vertices[x + y * width].TexWeights.X = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 0) / 14.0f, 0, 1);
                    vertices[x + y * width].TexWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 22) / 10.0f, 0, 1);
                    vertices[x + y * width].TexWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 40) / 10.0f, 0, 1);
                    vertices[x + y * width].TexWeights.W = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 56) / 10.0f, 0, 1);

                    //Normalization of weights: makeing sure that in every vertex total weight of texture sums up to 1
                    float total = vertices[x + y * width].TexWeights.X;
                    total += vertices[x + y * width].TexWeights.Y;
                    total += vertices[x + y * width].TexWeights.Z;
                    total += vertices[x + y * width].TexWeights.W;

                    vertices[x + y * width].TexWeights.X /= total;
                    vertices[x + y * width].TexWeights.Y /= total;
                    vertices[x + y * width].TexWeights.Z /= total;
                    vertices[x + y * width].TexWeights.W /= total;
                }
            }
        }

        public static int[] CreateTerrainIndices(int width, int height)
        {
            int[] terrainIndices = new int[(width) * 2 * (height - 1)];

            int i = 0;
            int z = 0;
            while (z < height - 1)
            {
                for (int x = 0; x < width; x++)
                {
                    terrainIndices[i++] = x + (z + 1) * width;
                    terrainIndices[i++] = x + z * width;
                }
                z++;

                if (z < height - 1)
                {
                    for (int x = width - 1; x >= 0; x--)
                    {
                        terrainIndices[i++] = x + z * width;
                        terrainIndices[i++] = x + (z + 1) * width;
                    }
                }
                z++;
            }

            return terrainIndices;
        }

        public static VertexMultiTextured[] GenerateNormalsForTriangleStrip(VertexMultiTextured[] vertices, int[] indices)
        {
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = new Vector3(0, 0, 0);

            bool swappedWinding = false;
            for (int i = 2; i < indices.Length; i++)
            {
                Vector3 firstVec = vertices[indices[i - 1]].Position - vertices[indices[i]].Position;
                Vector3 secondVec = vertices[indices[i - 2]].Position - vertices[indices[i]].Position;
                Vector3 normal = Vector3.Cross(firstVec, secondVec);
                normal.Normalize();

                if (swappedWinding)
                    normal *= -1;

                if (!float.IsNaN(normal.X))
                {
                    vertices[indices[i]].Normal += normal;
                    vertices[indices[i - 1]].Normal += normal;
                    vertices[indices[i - 2]].Normal += normal;
                }

                swappedWinding = !swappedWinding;
            }

            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal.Normalize();

            return vertices;
        }

        //We are going to fill our vertex and index buffer
        public static void CopyToBuffer(ref VertexBuffer vertexBuffer, ref IndexBuffer indexBuffer, VertexMultiTextured[] vertices, int[] indices, GraphicsDevice device)
        {
            //Allocate piece of memory on graphics card, so we can store there all of our vertices
            vertexBuffer = new VertexBuffer(device, typeof(VertexMultiTextured), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            //Here we are going to do the same thing with indices
            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }
    }
}
