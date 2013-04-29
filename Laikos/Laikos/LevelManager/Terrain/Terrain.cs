using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

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
        private static float[,] heightData;
        public static int width;
        public static int height;

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
        private Texture2D terrainMap;
        private QTNode rootNode;
        private List<Triangle> triangleList;
        private List<int> indicesList;
        DynamicIndexBuffer dynamicIndexBuffer;
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
            terrainMap = Game.Content.Load<Texture2D>("Models/Terrain/Heightmaps/heightmap-2");
            grassTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/grass");
            sandTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/sand");
            snowTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/snow");
            rockTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/rock");
            TerrainUtils.LoadHeightData(terrainMap, ref heightData, ref width, ref height);
            TerrainUtils.SetUpVertices(heightData, ref vertices, width, height);
            indices = TerrainUtils.CreateTerrainIndices(width, height);
            vertices = TerrainUtils.GenerateNormalsForTriangleStrip(vertices, indices);
            VertexMultiTextured[,] vertexArray = TerrainUtils.Reshape1DTo2D<VertexMultiTextured>(vertices, width, height);
            rootNode = new QTNode(vertexArray, device, grassTexture, 64, effect, rockTexture, snowTexture, sandTexture);
            TerrainUtils.CopyToBuffer(ref vertexBuffer, ref indexBuffer, vertices, indices, device);

            int terrainSize = 32;
            Triangle leftTriangle = new Triangle(null, new Vector2(0, 0), new Vector2(terrainSize, 0), new Vector2(0, terrainSize), heightData);
            Triangle rightTriangle = new Triangle(null, new Vector2(terrainSize, terrainSize), new Vector2(0, terrainSize), new Vector2(terrainSize, 0), heightData);
            leftTriangle.AddNeighs(null, null, rightTriangle);
            rightTriangle.AddNeighs(null, null, leftTriangle);

            triangleList = new List<Triangle>();
            triangleList.Add(leftTriangle);
            triangleList.Add(rightTriangle);

            indicesList = new List<int>();
            foreach (Triangle t in triangleList)
                t.AddIndices(ref indicesList);

            dynamicIndexBuffer = new DynamicIndexBuffer(device, typeof(int), indicesList.Count, BufferUsage.WriteOnly);
            dynamicIndexBuffer.SetData(indicesList.ToArray(), 0, indicesList.Count, SetDataOptions.Discard);
            dynamicIndexBuffer.ContentLost += dynamicIndexBuffer_ContentLost;
        }

        public override void Draw(GameTime gameTime)
        {
            DrawTerrain();
            base.Draw(gameTime);
        }

        private void DrawTerrain()
        {

            QTNode.nodesRendered = 0;
            BoundingFrustum cameraFrustrum = new BoundingFrustum(Camera.viewMatrix * Camera.projectionMatrix);
            rootNode.Draw(Matrix.Identity, Camera.viewMatrix, Camera.projectionMatrix, cameraFrustrum);
            //Console.WriteLine(QTNode.nodesRendered.ToString());
        }

        //Moving terrain to the center of the world (0, 0, 0)
        public Matrix SetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.Identity;
            return worldMatrix;
        }

        private void dynamicIndexBuffer_ContentLost(object sender, EventArgs e)
        {
            dynamicIndexBuffer.Dispose();
            dynamicIndexBuffer.SetData(indicesList.ToArray(), 0, indicesList.Count, SetDataOptions.Discard);
        }

        //Getting exact height at given point (x and z) returns y - height.
        public static float GetExactHeightAt(float xCoord, float zCoord)
        {
            bool invalid = xCoord < 0;
            invalid |= zCoord < 0;
            invalid |= xCoord > width -1;
            invalid |= zCoord > height - 1;
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

        public static float GetClippedHeightAt(float xCoord, float zCoord)
        {
            bool invalid = xCoord < 0;
            invalid |= zCoord < 0;
            invalid |= xCoord > width;
            invalid |= zCoord > height;
            if (invalid)
                return 10;
            else
                return heightData[(int)xCoord, (int)zCoord];
        }
    }
}
