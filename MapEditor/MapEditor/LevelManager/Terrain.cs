using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MapEditor
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
        private int terrainWidth;
        private int terrainHeight;
        private float[,] terrainHeightData;

        private int undergroundWidth;
        private int undergroundHeight;
        private float[,] undergroundHeightData;

        public static int currentHeight;
        public static int currentWidth;
        private static float[,] currentHeightData;

        private bool currentTerrain;

        //These variables are needed to create triangles in terrain
        private VertexMultiTextured[] terrainVertices;
        private int[] terrainIndices;

        private VertexMultiTextured[] undergroundVertices;
        private int[] undergroundIndices;

        //Buffers used to store terrain in memory of graphics card
        private VertexBuffer terrainVertexBuffer;
        private IndexBuffer terrainIndexBuffer;

        private VertexBuffer undergroundVertexBuffer;
        private IndexBuffer undergroundIndexBuffer;

        //Effects and direct link to graphics card
        private GraphicsDevice device;
        private Effect effect;

        //Textures will be loaded later to correctly render terrain
        private Texture2D grassTexture;
        private Texture2D rockTexture;
        private Texture2D sandTexture;
        private Texture2D snowTexture;

        private Texture2D terrainMap;
        public Texture2D TerrainMap
        {
            get { return terrainMap; }
            set { terrainMap = value; }
        }
        private Texture2D undergroundMap;
        //***************************************************************//

        public Terrain(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            currentTerrain = true;
            device = Game.GraphicsDevice;
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState kb = Keyboard.GetState();

            if (kb.IsKeyDown(Keys.Q))
            {
                currentTerrain = !currentTerrain;
            }

            base.Update(gameTime);
        }

        protected override void LoadContent()
        {
            //Loading textures and effects from content
            effect = Game.Content.Load<Effect>("effects");
            //terrainMap = Game.Content.Load<Texture2D>("Models/Terrain/Heightmaps/heightmap2");
            //undergroundMap = Game.Content.Load<Texture2D>("Models/Terrain/Heightmaps/heightmap1");
            grassTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/grass");
            sandTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/sand");
            snowTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/snow");
            rockTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/rock");

            //Loading basic map.
            terrainMap = Game.Content.Load<Texture2D>("Models/Terrain/Heightmaps/path");
            undergroundMap = Game.Content.Load<Texture2D>("Models/Terrain/Heightmaps/path");

            //All preparations to draw terrain are loaded here
            LoadHeightData(undergroundMap, ref undergroundHeightData, ref undergroundWidth, ref undergroundHeight);
            currentHeightData = new float[undergroundWidth, undergroundHeight];
            currentHeight = undergroundHeight;
            currentWidth = undergroundWidth;
            SetUpVertices(undergroundHeightData, ref undergroundVertices);
            SetUpIndices(ref undergroundIndices);
            CalculateNormals(ref undergroundVertices, undergroundIndices);
            CopyToBuffer(ref undergroundVertexBuffer, ref undergroundIndexBuffer, undergroundVertices, undergroundIndices);

            //All preparations to draw underground are loaded here
            LoadHeightData(terrainMap, ref terrainHeightData, ref terrainWidth, ref terrainHeight);
            currentHeightData = new float[terrainWidth, terrainHeight];
            currentHeight = terrainHeight;
            currentWidth = terrainWidth;
            SetUpVertices(terrainHeightData, ref terrainVertices);
            SetUpIndices(ref terrainIndices);
            CalculateNormals(ref terrainVertices, terrainIndices);
            CopyToBuffer(ref terrainVertexBuffer, ref terrainIndexBuffer, terrainVertices, terrainIndices);
        }

        public void reloadTerrainMap ()
        {
            //All preparations to draw underground are loaded here
            LoadHeightData(terrainMap, ref terrainHeightData, ref terrainWidth, ref terrainHeight);
            currentHeightData = new float[terrainWidth, terrainHeight];
            currentHeight = terrainHeight;
            currentWidth = terrainWidth;
            SetUpVertices(terrainHeightData, ref terrainVertices);
            SetUpIndices(ref terrainIndices);
            CalculateNormals(ref terrainVertices, terrainIndices);
            CopyToBuffer(ref terrainVertexBuffer, ref terrainIndexBuffer, terrainVertices, terrainIndices);
        }

        public override void Draw(GameTime gameTime)
        {
            if (currentTerrain)
                DrawTerrain();
            else
                DrawUnderground();
            base.Draw(gameTime);
        }

        private void DrawTerrain()
        {
            currentHeightData = new float[terrainWidth, terrainHeight];

            for (int i = 0; i < terrainWidth; i++)
            {
                for (int j = 0; j < terrainHeight; j++)
                {
                    currentHeightData[i, j] = terrainHeightData[i, j];
                }
            }

            currentWidth = terrainWidth;
            currentHeight = terrainHeight;

            //Setting technique for multitexturing and setting textures
            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(sandTexture);
            effect.Parameters["xTexture1"].SetValue(grassTexture);
            effect.Parameters["xTexture2"].SetValue(rockTexture);
            effect.Parameters["xTexture3"].SetValue(snowTexture);

            //Setting basic light for terrain
            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();

            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.1f);
            effect.Parameters["xEnableLighting"].SetValue(true);

            //Drawing terrain
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.Indices = terrainIndexBuffer;
                device.SetVertexBuffer(terrainVertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, terrainVertices.Length, 0, terrainIndices.Length / 3);
            }
        }

        private void DrawUnderground()
        {
            currentHeightData = new float[undergroundWidth, undergroundHeight];

            for (int i = 0; i < undergroundWidth; i++)
                for (int j = 0; j < undergroundHeight; j++)
                    currentHeightData[i, j] = undergroundHeightData[i, j];

            currentWidth = undergroundWidth;
            currentHeight = undergroundHeight;

            //Setting technique for multitexturing and setting textures
            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(sandTexture);
            effect.Parameters["xTexture1"].SetValue(rockTexture);
            effect.Parameters["xTexture2"].SetValue(rockTexture);
            effect.Parameters["xTexture3"].SetValue(rockTexture);

            //Setting basic light for terrain
            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(-0.3f);
            effect.Parameters["xEnableLighting"].SetValue(true);

            //Drawing terrain
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.Indices = undergroundIndexBuffer;
                device.SetVertexBuffer(undergroundVertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, undergroundVertices.Length, 0, undergroundIndices.Length / 3);

            }
        }

        //Loading data from bitmap file about height in our map.
        private void LoadHeightData(Texture2D heightMap, ref float[,] heightData, ref int Width, ref int Height)
        {
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            Width = heightMap.Width;
            Height = heightMap.Height;
            //Getting data about colors in heightmap file
            Color[] heightMapColors = new Color[Width * Height];
            heightMap.GetData(heightMapColors);

            //Initializing heightData array
            heightData = new float[Width, Height];

            //In this loop we are going to fill heightData 
            //with numbers based on color of heightmap file
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    //Loading data based on red color 0 - white 255 -black
                    heightData[x, y] = heightMapColors[x + y * Width].R / 15.0f;
                    //Setting data about maximum and minimum point in map
                    if (heightData[x, y] < minimumHeight) minimumHeight = heightData[x, y];
                    if (heightData[x, y] > maximumHeight) maximumHeight = heightData[x, y];
                }

            //In this loop we are going to make sure that every point in map is < 60
            for (int x = 0; x < terrainWidth; x++)
                for (int y = 0; y < terrainHeight; y++)
                    heightData[x, y] = (heightData[x, y] - minimumHeight) / (maximumHeight - minimumHeight) * 60.0f;
        }

        //Setting up position and texture coordinates of our vertices in triangles.
        //We are not connecting them yet, they are just points.
        //Connection between them will be made in SetUpIndices() function
        private void SetUpVertices(float[,] heightData, ref VertexMultiTextured[] vertices)
        {
            vertices = new VertexMultiTextured[currentWidth * currentHeight];

            for (int x = 0; x < currentWidth; x++)
            {
                for (int y = 0; y < currentHeight; y++)
                {
                    //Setting position and texturecoordinates of each vertex
                    vertices[x + y * currentWidth].Position = new Vector3(x, heightData[x, y], y);
                    vertices[x + y * currentWidth].TextureCoordinate.X = (float)x / 80.0f;
                    vertices[x + y * currentWidth].TextureCoordinate.Y = (float)y / 80.0f;


                    //Setting weights for each texture
                    vertices[x + y * currentWidth].TexWeights.X = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 0) / 14.0f, 0, 1);
                    vertices[x + y * currentWidth].TexWeights.Y = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 22) / 10.0f, 0, 1);
                    vertices[x + y * currentWidth].TexWeights.Z = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 40) / 10.0f, 0, 1);
                    vertices[x + y * currentWidth].TexWeights.W = MathHelper.Clamp(1.0f - Math.Abs(heightData[x, y] - 56) / 10.0f, 0, 1);

                    //Normalization of weights: makeing sure that in every vertex total weight of texture sums up to 1
                    float total = vertices[x + y * terrainWidth].TexWeights.X;
                    total += vertices[x + y * currentWidth].TexWeights.Y;
                    total += vertices[x + y * currentWidth].TexWeights.Z;
                    total += vertices[x + y * currentWidth].TexWeights.W;

                    vertices[x + y * currentWidth].TexWeights.X /= total;
                    vertices[x + y * currentWidth].TexWeights.Y /= total;
                    vertices[x + y * currentWidth].TexWeights.Z /= total;
                    vertices[x + y * currentWidth].TexWeights.W /= total;
                }
            }
        }

        //We are going to connect vertices with each other in this function
        //to create triangles. It's optimization to not duplicate some of the vertices
        private void SetUpIndices(ref int[] indices)
        {
            indices = new int[(currentWidth - 1) * (currentHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < currentHeight - 1; y++)
            {
                for (int x = 0; x < currentWidth - 1; x++)
                {
                    //Calculating indices
                    int lowerLeft = x + y * currentWidth;
                    int lowerRight = (x + 1) + y * currentWidth;
                    int topLeft = x + (y + 1) * currentWidth;
                    int topRight = (x + 1) + (y + 1) * currentWidth;

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
        private void CalculateNormals(ref VertexMultiTextured[] vertices, int[] indices)
        {
            //Set all normals to 0
            for (int i = 0; i < vertices.Length; i++)
                vertices[i].Normal = Vector3.Zero;

            //Calculating all normal for each triangle
            for (int i = 0; i < indices.Length / 3; i++)
            {
                int index1 = indices[i * 3];
                int index2 = indices[i * 3 + 1];
                int index3 = indices[i * 3 + 2];

                Vector3 side1 = vertices[index2].Position - vertices[index1].Position;
                Vector3 side2 = vertices[index1].Position - vertices[index3].Position;
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
        private void CopyToBuffer(ref VertexBuffer vertexBuffer, ref IndexBuffer indexBuffer, VertexMultiTextured[] vertices, int[] indices)
        {
            //Allocate piece of memory on graphics card, so we can store there all of our vertices
            vertexBuffer = new VertexBuffer(device, typeof(VertexMultiTextured), vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);
            //Here we are going to do the same thing with indices
            indexBuffer = new IndexBuffer(device, typeof(int), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        //Moving terrain to the center of the world (0, 0, 0)
        public Matrix SetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.Identity;
            return worldMatrix;
        }

        //Getting exact height at given point (x and z) returns y - height.
        public static float GetExactHeightAt(float xCoord, float zCoord)
        {
            bool invalid = xCoord < 0;
            invalid |= zCoord < 0;
            invalid |= xCoord > currentWidth - 1;
            invalid |= zCoord > currentHeight - 1;
            if (invalid)
                return 10;

            int xLower = (int)xCoord;
            int xHigher = xLower + 1;
            float xRelative = (xCoord - xLower) / ((float)xHigher - (float)xLower);

            int zLower = (int)zCoord;
            int zHigher = zLower + 1;
            float zRelative = (zCoord - zLower) / ((float)zHigher - (float)zLower);


            float heightLxLz = currentHeightData[xLower, zLower];
            float heightLxHz = currentHeightData[xLower, zHigher];
            float heightHxLz = currentHeightData[xHigher, zLower];
            float heightHxHz = currentHeightData[xHigher, zHigher];

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

        public static float GetClippedHeightAt(float xCoord, float zCoord)
        {
            bool invalid = xCoord < 0;
            invalid |= zCoord < 0;
            invalid |= xCoord > currentWidth;
            invalid |= zCoord > currentHeight;
            if (invalid)
                return 10;
            else
                return currentHeightData[(int)xCoord, (int)zCoord];
        }
    }
}
