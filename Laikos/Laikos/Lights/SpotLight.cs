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
        public Texture2D attentuationTexture { get; set; }
        GraphicsDevice device;
        Effect spotLightEffect;

        public SpotLight(GraphicsDevice device, Vector3 position, Vector3 direction, Color color, float intensity, 
            bool withShadows, int shadowMapResolution, Texture2D attentuationTexture, Effect spotLightEffect)
        {
            this.position = position;
            this.device = device;
            this.direction = direction;
            this.color = color;
            this.intensity = intensity;
            this.withShadows = withShadows;
            this.shadowMapResolution = shadowMapResolution;
            this.attentuationTexture = attentuationTexture;
            this.spotLightEffect = spotLightEffect;

            nearPlane = 1.0f;
            farPlane = 100.0f;
            FOV = MathHelper.PiOver2;
            depthBias = 1.0f / 2000.0f;
            projection = Matrix.CreatePerspectiveFieldOfView(FOV, 1.0f, nearPlane, farPlane);
            shadowMap = new RenderTarget2D(device, shadowMapResolution, shadowMapResolution, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

            Update();
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

        public void CreateShadowMaps(SpotLight light, List<Model> models, Effect depthWriter)
        {
            device.SetRenderTarget(shadowMap);
            device.Clear(Color.Transparent);

            depthWriter.Parameters["View"].SetValue(view);
            depthWriter.Parameters["Projection"].SetValue(projection);
            depthWriter.Parameters["LightPosition"].SetValue(position);
            depthWriter.Parameters["DepthPrecision"].SetValue(farPlane);

            DrawModels(models, depthWriter);
        }

        void CreateLightMaps()
        {

        }

        void DrawModels(List<Model> Models, Effect depthWriter)
        {
            //Draw Each Model
            foreach (Model model in Models)
            {
                //Get Transforms
                Matrix[] transforms = new Matrix[model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(transforms);

                //Draw Each ModelMesh
                foreach (ModelMesh mesh in model.Meshes)
                {
                    //Draw Each ModelMeshPart
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        //Set Vertex Buffer
                        device.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);

                        //Set Index Buffer
                        device.Indices = part.IndexBuffer;

                        //Set World
                        depthWriter.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index]);

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
