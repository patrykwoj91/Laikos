using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Laikos
{

    //This structure that holds data about position, color and normal for each vertex.
    public struct VertexMultiTextured
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 TextureCoordinate;
        public Vector4 TexWeights;

        public static int SizeInBytes = (3 + 3 + 4 + 4) * sizeof(float);

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 6, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 10, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
        );
    }

    class Terrain : DrawableGameComponent
    {
        //****************************************************************//
        //Variables created to store information about creation of terrain//
        //****************************************************************//

        //These variables describes parameters of terrain
        private float[,] heightData;
        private int terrainWidth;
        private int terrainHeight;

        //These variables are needed to create triangles in terrain
        private VertexMultiTextured[] vertices;
        private int[] indices;

        //Buffers used to store terrain in memory of graphics card
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        //Effects and direct link to graphics card
        private GraphicsDevice device;
        private Effect effect;

        //Textures will be loaded later to correctly render terrain
        private Texture2D grassTexture;
        private Texture2D rockTexture;
        private Texture2D sandTexture;
        private Texture2D snowTexture;
        private Texture2D heightMap;
        //***************************************************************//

        public Terrain(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            device = Game.GraphicsDevice;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //Loading textures and effects from content
            effect = Game.Content.Load<Effect>("effects");
            heightMap = Game.Content.Load<Texture2D>("Textures/Terrain/Heightmaps/heightmap2");
            grassTexture = Game.Content.Load<Texture2D>("Textures/Terrain/Textures/grass");
            sandTexture = Game.Content.Load<Texture2D>("Textures/Terrain/Textures/sand");
            snowTexture = Game.Content.Load<Texture2D>("Textures/Terrain/Textures/snow");
            rockTexture = Game.Content.Load<Texture2D>("Textures/Terrain/Textures/rock");
            //All preparations to draw terrain are loaded here
            LoadHeightData();
            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            CopyToBuffer();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            device.RasterizerState = rs;

            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(sandTexture);
            effect.Parameters["xTexture1"].SetValue(grassTexture);
            effect.Parameters["xTexture2"].SetValue(rockTexture);
            effect.Parameters["xTexture3"].SetValue(snowTexture);

            Vector3 lightDirection = new Vector3(-0.5f, -1.0f, -0.5f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.8f);
            effect.Parameters["xEnableLighting"].SetValue(true);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.Indices = indexBuffer;
                device.SetVertexBuffer(vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);

            }

            base.Draw(gameTime);
        }

        //Loading data from bitmap file about height in our map.
        private void LoadHeightData()
        {

            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            terrainWidth = heightMap.Width;
            terrainHeight = heightMap.Height;

            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);

            heightData = new float[terrainWidth, terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                {
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R;
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 30.0f;
        }

        //Setting up position and texture coordinates of our vertices in triangles.
        //We are not connecting them yet, they are just points.
        //Connection between them will be made in SetUpIndices() function
        private void SetUpVertices()
        {
            vertices = new VertexMultiTextured[terrainWidth * terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    vertices[x + y * terrainWidth].Position = new Vector3(x, -heightData[x, y], -y);
                    vertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 80.0f;
                    vertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 80.0f;

                    vertices[x + y * terrainWidth].TexWeights.X = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 0) / 8.0f, 0, 1);
                    vertices[x + y * terrainWidth].TexWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 12) / 6.0f, 0, 1);
                    vertices[x + y * terrainWidth].TexWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 20) / 6.0f, 0, 1);
                    vertices[x + y * terrainWidth].TexWeights.W = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 30) / 6.0f, 0, 1);

                    float total = vertices[x + y * terrainWidth].TexWeights.X;
                    total += vertices[x + y * terrainWidth].TexWeights.Y;
                    total += vertices[x + y * terrainWidth].TexWeights.Z;
                    total += vertices[x + y * terrainWidth].TexWeights.W;

                    vertices[x + y * terrainWidth].TexWeights.X /= total;
                    vertices[x + y * terrainWidth].TexWeights.Y /= total;
                    vertices[x + y * terrainWidth].TexWeights.Z /= total;
                    vertices[x + y * terrainWidth].TexWeights.W /= total;
                }
            }
        }

        //We are going to connect vertices with each other in this function
        //to create triangles. It's optimization to not duplicate some of the vertices
        private void SetUpIndices()
        {
            indices = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    //Calculating indices
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    //Filling up indices array with calculated indices
                    //for bottom sided triangles
                    indices[counter++] = lowerLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = topLeft;

                    //Filling up indices array for bottom sided triangles
                    indices[counter++] = lowerRight;
                    indices[counter++] = topRight;
                    indices[counter++] = topLeft;
                }
            }
        }

        //This method calculate and fills up our vertices with normalized normals
        private void CalculateNormals()
        {
            //Set all normals to 0
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = Vector3.Zero;

            //Calculating all normal for each triangle
            for (int i = 0; i < vertices.Length; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                //Filling up vertices array with calculated normals
                vertices[index1].Normal += normal;
                vertices[index2].Normal += normal;
                vertices[index3].Normal += normal;
            }

            //Now we need to normalize length of each normal
            for (int x = 0; x < vertices.Length; x++)
                vertices[x].Normal.Normalize();
        }

        //We are going to fill our vertex and index buffer
        private void CopyToBuffer()
        {
            //Allocate piece of memory on graphics card, so we can store there all of our vertices
            vertexBuffer = new VertexBuffer(device, VertexMultiTextured.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            //Here we are going to do the same thing with indices
            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        //Moving terrain to the center of the world (0, 0, 0)
        public Matrix SetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateTranslation(-terrainWidth / 2.0f, 0, terrainHeight / 2.0f);
            return worldMatrix;
        }

    }
}
