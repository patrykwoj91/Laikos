using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace Laikos
{
    class SpotLight
    {
        public Vector3 position { get; set; }
        public Vector3 direction { get; set; }
        public Color color { get; set; }

        public float intensity { get; set; }
        public float nearPlane { get; set; }
        public float farPlane { get; set; }
        public float FOV { get; set; }

        public bool withShadows { get; set; }
        public int shadowMapResolution { get; set; }
        public float depthBias { get; set; }

        public Matrix world { get; set; }
        public Matrix view { get; set; }
        public Matrix projection { get; set; }

        public RenderTarget2D shadowMap { get; set; }
        public static GraphicsDevice device;

        private static Model spotLightGeometry;
        private static Effect spotLightEffect;
        private static Vector2 GBufferTextureSize;
        private static Texture2D attentuationTexture;
        private static RenderTarget2D colorRT;
        private static RenderTarget2D normalRT;
        private static RenderTarget2D depthRT;

        public SpotLight(Vector3 position, Vector3 direction, Color color, float intensity, 
            bool withShadows, int shadowMapResolution)
        {
            this.position = position;
            this.direction = direction;
            this.color = color;
            this.intensity = intensity;
            this.withShadows = withShadows;
            this.shadowMapResolution = shadowMapResolution;

            nearPlane = 1.0f;
            farPlane = 30.0f;
            FOV = MathHelper.PiOver2;
            depthBias = 1.0f / 6.5f;
            projection = Matrix.CreatePerspectiveFieldOfView(FOV, 1.0f, nearPlane, farPlane);
            shadowMap = new RenderTarget2D(device, shadowMapResolution, shadowMapResolution, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            GBufferTextureSize = new Vector2(device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight);

            Update();
        }

        public static void Initialize(GraphicsDevice Device, Effect SpotLightEffect, Texture2D AttentuationTexture, Model SpotLightGeometry,
            RenderTarget2D ColorRT, RenderTarget2D NormalRT, RenderTarget2D DepthRT)
        {
            spotLightEffect = SpotLightEffect;
            attentuationTexture = AttentuationTexture;
            spotLightGeometry = SpotLightGeometry;
            device = Device;
            GBufferTextureSize = new Vector2(device.PresentationParameters.BackBufferWidth, device.PresentationParameters.BackBufferHeight);
            colorRT = ColorRT;
            normalRT = NormalRT;
            depthRT = DepthRT;
        }

        public float LightAngleCos()
        {
            float ConeAngle = FOV;
            return (float)Math.Cos((double)ConeAngle);
        }

        public void Update()
        {
            Vector3 target = (position + direction);

            if (target == Vector3.Zero) target = -Vector3.Up;

            Vector3 up = Vector3.Cross(direction, Vector3.Up);

            if (up == Vector3.Zero) up = Vector3.Right;
            else up = Vector3.Up;

            view = Matrix.CreateLookAt(position, target, up);
            float radial = (float)Math.Tan((double)FOV / 2.0) * 2 * farPlane;

            Matrix scaling = Matrix.CreateScale(radial, radial, farPlane);
            Matrix translation = Matrix.CreateTranslation(position.X, position.Y, position.Z);
            Matrix inverseView = Matrix.Invert(view);
            Matrix semiProduct = scaling * inverseView;
            Vector3 s; Vector3 p; Quaternion q;
            semiProduct.Decompose(out s, out q, out p);
            Matrix rotation = Matrix.CreateFromQuaternion(q);
            world = scaling * rotation * translation;
        }

        public void CreateShadowMap(List<GameObject> models, Effect depthWriter, Terrain terrain)
        {
            device.SetRenderTarget(shadowMap);
            device.Clear(Color.Transparent);

            depthWriter.Parameters["View"].SetValue(view);
            depthWriter.Parameters["Projection"].SetValue(projection);
            depthWriter.Parameters["LightPosition"].SetValue(position);
            depthWriter.Parameters["DepthPrecision"].SetValue(farPlane);

            terrain.DrawShadow(depthWriter);
            DrawModels(models, depthWriter);
            
        }

        public void CreateLightMap()
        {
            spotLightEffect.Parameters["View"].SetValue(Camera.viewMatrix);
            spotLightEffect.Parameters["Projection"].SetValue(Camera.projectionMatrix);
            spotLightEffect.Parameters["inverseView"].SetValue(Matrix.Invert(Camera.viewMatrix));
            spotLightEffect.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(Camera.viewMatrix * Camera.projectionMatrix));
            spotLightEffect.Parameters["CameraPosition"].SetValue(Camera.cameraPosition);
            spotLightEffect.Parameters["GBufferTextureSize"].SetValue(GBufferTextureSize);

            device.SetVertexBuffer(spotLightGeometry.Meshes[0].MeshParts[0].VertexBuffer,
                    spotLightGeometry.Meshes[0].MeshParts[0].VertexOffset);
            device.Indices = spotLightGeometry.Meshes[0].MeshParts[0].IndexBuffer;

            spotLightEffect.Parameters["AttentuationTexture"].SetValue(attentuationTexture);
            spotLightEffect.Parameters["shadowMap"].SetValue(shadowMap);
            spotLightEffect.Parameters["ColorMap"].SetValue(colorRT);
            spotLightEffect.Parameters["NormalMap"].SetValue(normalRT);
            spotLightEffect.Parameters["DepthMap"].SetValue(depthRT);

            spotLightEffect.Parameters["World"].SetValue(world);
            spotLightEffect.Parameters["LightViewProjection"].SetValue(view * projection);
            spotLightEffect.Parameters["LightPosition"].SetValue(position);
            spotLightEffect.Parameters["LightColor"].SetValue(color.ToVector4());
            spotLightEffect.Parameters["LightIntensity"].SetValue(intensity);
            spotLightEffect.Parameters["S"].SetValue(direction);
            spotLightEffect.Parameters["LightAngleCos"].SetValue(LightAngleCos());
            spotLightEffect.Parameters["LightHeight"].SetValue(farPlane);
            spotLightEffect.Parameters["Shadows"].SetValue(withShadows);
            spotLightEffect.Parameters["shadowMapSize"].SetValue(shadowMapResolution);
            spotLightEffect.Parameters["DepthPrecision"].SetValue(farPlane);
            spotLightEffect.Parameters["DepthBias"].SetValue(depthBias);

            //Set cull mode
            Vector3 L = Camera.cameraPosition - position;
            float SL = Math.Abs(Vector3.Dot(L, direction));

            if (SL < LightAngleCos())
                device.RasterizerState = RasterizerState.CullCounterClockwise;
            else
                device.RasterizerState = RasterizerState.CullClockwise;

            spotLightEffect.CurrentTechnique.Passes[0].Apply();

            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, spotLightGeometry.Meshes[0].MeshParts[0].NumVertices,
                spotLightGeometry.Meshes[0].MeshParts[0].StartIndex, spotLightGeometry.Meshes[0].MeshParts[0].PrimitiveCount);
        }

        void DrawModels(List<GameObject> Models, Effect depthWriter)
        {
            foreach (GameObject model in Models)
            {
                //Get Transforms
                Matrix[] transforms = new Matrix[model.currentModel.Bones.Count];
                model.currentModel.Model.CopyAbsoluteBoneTransformsTo(transforms);

                //Draw Each ModelMesh
                foreach (ModelMesh mesh in model.currentModel.Model.Meshes)
                {
                    //Draw Each ModelMeshPart
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        //Set Vertex Buffer
                        device.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);

                        //Set Index Buffer
                        device.Indices = part.IndexBuffer;

                        //Set World
                        depthWriter.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * model.GetWorldMatrix());

                        //Apply Effect
                        depthWriter.CurrentTechnique.Passes[0].Apply();

                        //Draw
                        device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                    }
                }
            }
        }
    }
}
