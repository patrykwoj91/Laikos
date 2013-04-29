using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Laikos
{
    class QTNode
    {
        private BoundingBox nodeBoundingBox;

        private bool isEndNode;
        private QTNode nodeUL;
        private QTNode nodeUR;
        private QTNode nodeLL;
        private QTNode nodeLR;

        private int width;
        private int height;
        private GraphicsDevice device;
        private Effect effect;
        private VertexBuffer nodeVertexBuffer;
        private IndexBuffer nodeIndexBuffer;
        private Texture2D grassTexture;
        private Texture2D rockTexture;
        private Texture2D sandTexture;
        private Texture2D snowTexture;

        public static int nodesRendered;

        public QTNode(VertexMultiTextured[,] vertexArray, GraphicsDevice device, Texture2D grassTexture, int maxSize, Effect effect,Texture2D rockTexture, Texture2D snowTexture, Texture2D sandTexture)
        {
            this.device = device;
            this.grassTexture = grassTexture;
            this.rockTexture = rockTexture;
            this.sandTexture = sandTexture;
            this.snowTexture = snowTexture;
            this.effect = effect;
            width = vertexArray.GetLength(0);
            height = vertexArray.GetLength(1);

            nodeBoundingBox = TerrainUtils.CreateBoundingBox(vertexArray);

            isEndNode = width <= maxSize;
            isEndNode &= height <= maxSize;
            if (isEndNode)
            {
                VertexMultiTextured[] vertices = TerrainUtils.Reshape2DTo1D<VertexMultiTextured>(vertexArray);
                int[] indices = TerrainUtils.CreateTerrainIndices(width, height);
                TerrainUtils.CopyToBuffer(ref nodeVertexBuffer, ref nodeIndexBuffer, vertices, indices, device);
            }
            else
                CreateChildNodes(vertexArray, maxSize);
        }

        private void CreateChildNodes(VertexMultiTextured[,] vertexArray, int maxSize)
        {
            VertexMultiTextured[,] ulArray = new VertexMultiTextured[width / 2 + 1, height / 2 + 1];
            for (int w = 0; w < width / 2 + 1; w++)
                for (int h = 0; h < height / 2 + 1; h++)
                    ulArray[w, h] = vertexArray[w, h];
            nodeUL = new QTNode(ulArray, device, grassTexture, maxSize, effect, rockTexture, snowTexture, sandTexture);

            VertexMultiTextured[,] urArray = new VertexMultiTextured[width - (width / 2), height / 2 + 1];
            for (int w = 0; w < width - (width / 2); w++)
                for (int h = 0; h < height / 2 + 1; h++)
                    urArray[w, h] = vertexArray[width / 2 + w, h];
            nodeUR = new QTNode(urArray, device, grassTexture, maxSize, effect, rockTexture, snowTexture, sandTexture);

            VertexMultiTextured[,] llArray = new VertexMultiTextured[width / 2 + 1, height - (height / 2)];
            for (int w = 0; w < width / 2 + 1; w++)
                for (int h = 0; h < height - (height / 2); h++)
                    llArray[w, h] = vertexArray[w, height / 2 + h];
            nodeLL = new QTNode(llArray, device, grassTexture, maxSize, effect, rockTexture, snowTexture, sandTexture);

            VertexMultiTextured[,] lrArray = new VertexMultiTextured[width - (width / 2), height - (height / 2)];
            for (int w = 0; w < width - (width / 2); w++)
                for (int h = 0; h < height - (height / 2); h++)
                    lrArray[w, h] = vertexArray[width / 2 + w, height / 2 + h];
            nodeLR = new QTNode(lrArray, device, grassTexture, maxSize, effect, rockTexture, snowTexture, sandTexture);

        }

        public void Draw(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix, BoundingFrustum cameraFrustrum)
        {
            BoundingBox transformedBox = XNAUtils.TransformBoundingBox(nodeBoundingBox, worldMatrix);
            ContainmentType cameraNodeContainment = cameraFrustrum.Contains(transformedBox);
            if (cameraNodeContainment != ContainmentType.Disjoint)
            {
                if (isEndNode)
                {
                    DrawCurrentNode(worldMatrix, viewMatrix, projectionMatrix);
                }
                else
                {
                    nodeUL.Draw(worldMatrix, viewMatrix, projectionMatrix, cameraFrustrum);
                    nodeUR.Draw(worldMatrix, viewMatrix, projectionMatrix, cameraFrustrum);
                    nodeLL.Draw(worldMatrix, viewMatrix, projectionMatrix, cameraFrustrum);
                    nodeLR.Draw(worldMatrix, viewMatrix, projectionMatrix, cameraFrustrum);
                }
            }
        }

        private void DrawCurrentNode(Matrix worldMatrix, Matrix viewMatrix, Matrix projectionMatrix)
        {
            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(sandTexture);
            effect.Parameters["xTexture1"].SetValue(grassTexture);
            effect.Parameters["xTexture2"].SetValue(rockTexture);
            effect.Parameters["xTexture3"].SetValue(snowTexture);

            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.1f);
            effect.Parameters["xEnableLighting"].SetValue(true);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.SetVertexBuffer(nodeVertexBuffer);
                device.Indices = nodeIndexBuffer;
                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, width * height, 0, (width * 2 * (height - 1) - 2));
            }

            nodesRendered++;
        }
    }
}
