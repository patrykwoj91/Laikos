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
    public struct VertexMultiTextured : IVertexType
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

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }
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
            heightMap = Game.Content.Load<Texture2D>("Models/Terrain/Heightmaps/heightmap2");
            grassTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/grass");
            sandTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/sand");
            snowTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/snow");
            rockTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/rock");
            //All preparations to draw terrain are loaded here
            LoadHeightData();
            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            CopyToBuffer();
        }

        public override void Draw(GameTime gameTime)
        {
            RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            device.RasterizerState = rs;

            //Setting technique for multitexturing and setting textures
            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(sandTexture);
            effect.Parameters["xTexture1"].SetValue(grassTexture);
            effect.Parameters["xTexture2"].SetValue(rockTexture);
            effect.Parameters["xTexture3"].SetValue(snowTexture);

            //Setting basic light for terrain
            Vector3 lightDirection = new Vector3(0.5f, 1.0f, -1.0f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.8f);
            effect.Parameters["xEnableLighting"].SetValue(true);

            //Drawing terrain
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

            //Getting data about colors in heightmap file
            Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(heightMapColors);
            
            //Initializing heightData array
            heightData = new float[terrainWidth, terrainHeight];

            //In this loop we are going to fill heightData 
            //with numbers based on color of heightmap file
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                {
                    //Loading data based on red color 0 - white 255 -black
                    heightData[x, y] = heightMapColors[x + y * terrainWidth].R;
                    //Setting data about maximum and minimum point in map
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            //In this loop we are going to make sure that every point in map is < 30
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 60.0f;
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
                    //Setting position and texturecoordinates of each vertex
                    vertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
                    vertices[x + y * terrainWidth].TextureCoordinate.X = (float)x / 80.0f;
                    vertices[x + y * terrainWidth].TextureCoordinate.Y = (float)y / 80.0f;

                    //Setting weights for each texture
                    vertices[x + y * terrainWidth].TexWeights.X = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 0) / 14.0f, 0, 1);
                    vertices[x + y * terrainWidth].TexWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 22) / 10.0f, 0, 1);
                    vertices[x + y * terrainWidth].TexWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 40) / 10.0f, 0, 1);
                    vertices[x + y * terrainWidth].TexWeights.W = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 56) / 10.0f, 0, 1);

                    //Normalization of weights: makeing sure that in every vertex total weight of texture sums up to 1
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
            vertexBuffer = new VertexBuffer(device, typeof(VertexMultiTextured), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            //Here we are going to do the same thing with indices
            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        //Moving terrain to the center of the world (0, 0, 0) and rotating it
        public Matrix SetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateScale(1,1,-1);
            return worldMatrix;
        }

        //Getting exact height at given point (x and z) returns y - height.
        public float GetExactHeightAt(float xCoord, float zCoord)
        {
            bool invalid = xCoord < 0;
            invalid |= zCoord < 0;
            invalid |= xCoord > terrainWidth;
            invalid |= zCoord > terrainHeight;
            if (invalid)
                return 10;

            int xLower = (int)xCoord;
            int xHigher = xLower + 1;
            float xRelative = (xCoord - xLower) / ((float)xHigher - (float)xLower);

            int zLower = (int)zCoord;
            int zHigher = zLower + 1;
            float zRelative = (zCoord - zLower) / ((float)zHigher - (float)zLower);

            float heightLxLz = heightData[xLower, zLower];
            float heightLxHz = heightData[xLower, zHigher];
            float heightHxLz = heightData[xHigher, zLower];
            float heightHxHz = heightData[xHigher, zHigher];

            bool pointAboveLowerTriangle = (xRelative + zRelative < 1);

            float finalHeight;
            if (pointAboveLowerTriangle)
            {
                finalHeight = heightLxLz;
                finalHeight += zRelative * (heightLxHz - heightLxLz);
                finalHeight += xRelative * (heightHxLz - heightLxLz);
            }
            else
            {
                finalHeight = heightHxHz;
                finalHeight += (1.0f - zRelative) * (heightHxLz - heightHxHz);
                finalHeight += (1.0f - xRelative) * (heightLxHz - heightHxHz);
            }

            return finalHeight;
        }
    }
}
