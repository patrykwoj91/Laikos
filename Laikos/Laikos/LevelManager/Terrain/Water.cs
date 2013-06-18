using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Animation;
namespace Laikos
{
    class Water
    {

        private RenderTarget2D refractionRenderTarget;
        private RenderTarget2D reflectionRenderTarget;
        public Texture2D reflectionMap;
        public Texture2D refractionMap;
        private Texture2D waterBump;
        private GraphicsDevice device;
        public static float waterHeight = 6.0f;

        private Effect waterEffect;
        private Model skyDome;
        private Texture2D cloudMap;
        private VertexBuffer waterVertexBuffer;
        private VertexDeclaration waterVertexDeclaration;
        Vector3 windDirection = new Vector3(-0.5f, 0, 1);

        Effect GBuffer;

        public Water(GraphicsDevice device, ContentManager content, Effect GBuffer)
        {
            this.GBuffer = GBuffer;
            this.device = device;
            int backbufferWidth = device.PresentationParameters.BackBufferWidth;
            int backbufferHeight = device.PresentationParameters.BackBufferHeight;

            refractionRenderTarget = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, device.DisplayMode.Format, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);
            reflectionRenderTarget = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, device.DisplayMode.Format, DepthFormat.Depth24Stencil8, 1, RenderTargetUsage.DiscardContents);

            skyDome = content.Load<Model>("Models/SkyDome/dome");
            skyDome.Meshes[0].MeshParts[0].Effect = GBuffer.Clone();
            cloudMap = content.Load<Texture2D>("Models/SkyDome/cloudMap");
            waterEffect = content.Load<Effect>("Effects/Water");
            waterBump = content.Load<Texture2D>("waterbump");
            SetUpWaterVertices();
            waterVertexDeclaration = VertexPositionTexture.VertexDeclaration;
        }


        public void DrawSkyDome(Matrix viewMatrix)
        {
            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(200) * Matrix.CreateTranslation(Camera.cameraPosition);

            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["SkyDome"];
                    currentEffect.Parameters["World"].SetValue(worldMatrix);
                    currentEffect.Parameters["View"].SetValue(viewMatrix);
                    currentEffect.Parameters["Projection"].SetValue(Camera.projectionMatrix);
                    currentEffect.Parameters["xTexture0"].SetValue(cloudMap);
                }
                device.RasterizerState = RasterizerState.CullNone;
                mesh.Draw();
            }
        }

        private Vector4 CreatePlane(float height, Vector3 planeNormalDirection, bool clipSide)
        {
            planeNormalDirection.Normalize();
            Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
            if (clipSide)
                planeCoeffs *= -1;

            Matrix worldViewProjection = Camera.viewMatrix * Camera.projectionMatrix;
            Matrix inverseWorldViewProjection = Matrix.Invert(worldViewProjection);
            inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

            Vector4 finalPlane = planeCoeffs;

            return finalPlane;
        }

        public void DrawRefractionMap(Terrain terrain, List<GameObject> objects, Texture2D normals, Texture2D speculars)
        {
            Vector4 refractionPlane = CreatePlane(waterHeight + 3.0f, new Vector3(0, -1, 0), false);

            device.SetRenderTarget(refractionRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            GBuffer.Parameters["xClip"].SetValue(true);
            GBuffer.Parameters["xClipPlane"].SetValue(refractionPlane);
            terrain.DrawTerrain(GBuffer);
            /*foreach (GameObject obj in objects)
            {
                if (obj is Unit)
                {
                    Unit unit = (Unit)obj;
                    unit.currentModel.Draw(device, unit.GetWorldMatrix(), GBuffer, normals, speculars, false);
                }
                if (obj is Decoration)
                {
                    Decoration decoration = (Decoration)obj;
                    decoration.currentModel.Draw(device, decoration.GetWorldMatrix(), GBuffer, normals, speculars, false);
                }
            }*/
            device.SetRenderTarget(null);
            GBuffer.Parameters["xClip"].SetValue(false);
            refractionMap = refractionRenderTarget;
        }

        public void DrawReflectionMap(Terrain terrain, List<GameObject> objects, Texture2D normals, Texture2D speculars)
        {
            Vector4 reflectionPlane = CreatePlane(waterHeight - 0.5f, new Vector3(0, -1, 0), true);
            device.SetRenderTarget(reflectionRenderTarget);
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            GBuffer.Parameters["xClip"].SetValue(true);
            GBuffer.Parameters["xClipPlane"].SetValue(reflectionPlane);
            terrain.DrawTerrain(GBuffer);
            /*foreach (GameObject obj in objects)
            {
                if (obj is Unit)
                {
                    Unit unit = (Unit)obj;
                    unit.currentModel.Draw(device, unit.GetWorldMatrix(), GBuffer, normals, speculars, false);
                }
                if (obj is Decoration)
                {
                    Decoration decoration = (Decoration)obj;
                    decoration.currentModel.Draw(device, decoration.GetWorldMatrix(), GBuffer, normals, speculars, false);
                }
            }*/
            DrawSkyDome(Camera.reflectionViewMatrix);
            device.SetRenderTarget(null);
            GBuffer.Parameters["xClip"].SetValue(false);
            reflectionMap = reflectionRenderTarget;
        }

        private void SetUpWaterVertices()
        {
            VertexPositionTexture[] waterVertices = new VertexPositionTexture[6];

            waterVertices[0] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[2] = new VertexPositionTexture(new Vector3(Terrain.width, waterHeight, Terrain.height), new Vector2(1, 0));
            waterVertices[1] = new VertexPositionTexture(new Vector3(0, waterHeight, Terrain.height), new Vector2(0, 0));

            waterVertices[3] = new VertexPositionTexture(new Vector3(0, waterHeight, 0), new Vector2(0, 1));
            waterVertices[5] = new VertexPositionTexture(new Vector3(Terrain.width, waterHeight, 0), new Vector2(1, 1));
            waterVertices[4] = new VertexPositionTexture(new Vector3(Terrain.width, waterHeight, Terrain.height), new Vector2(1, 0));

            waterVertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), waterVertices.Length, BufferUsage.WriteOnly);
            waterVertexBuffer.SetData(waterVertices);
        }

        public void DrawWater(float time)
        {
            waterEffect.CurrentTechnique = waterEffect.Techniques["Water"];
            Matrix worldMatrix = Matrix.Identity;
            waterEffect.Parameters["xWorld"].SetValue(worldMatrix);
            waterEffect.Parameters["xView"].SetValue(Camera.viewMatrix);
            waterEffect.Parameters["xReflectionView"].SetValue(Camera.reflectionViewMatrix);
            waterEffect.Parameters["xProjection"].SetValue(Camera.projectionMatrix);
            waterEffect.Parameters["xReflectionMap"].SetValue(reflectionMap);
            waterEffect.Parameters["xRefractionMap"].SetValue(refractionMap);
            waterEffect.Parameters["xWaterBumpMap"].SetValue(waterBump);
            waterEffect.Parameters["xWaveLength"].SetValue(0.15f);
            waterEffect.Parameters["xWaveHeight"].SetValue(0.4f);
            waterEffect.Parameters["xCamPos"].SetValue(Camera.cameraPosition);
            waterEffect.Parameters["xTime"].SetValue(time);
            waterEffect.Parameters["xWindForce"].SetValue(0.002f);
            waterEffect.Parameters["xWindDirection"].SetValue(windDirection);
            foreach (EffectPass pass in waterEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.SetVertexBuffer(waterVertexBuffer, 0);
                int noVertices = waterVertexBuffer.VertexCount;
                device.DrawPrimitives(PrimitiveType.TriangleList, 0, noVertices / 3);
            }
        }


    }
}
