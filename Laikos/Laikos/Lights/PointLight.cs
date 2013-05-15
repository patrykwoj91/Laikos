using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace Laikos
{
    class PointLight
    {
        private Vector3 position;
        private Color color;
        private float radius;
        private float intensity;

        private static Effect pointLightEffect;

        private static RenderTarget2D colorRT;
        private static RenderTarget2D normalRT;
        private static RenderTarget2D depthRT;

        private static FullscreenQuad fsq;
        private static Vector2 halfPixel;
        private static GraphicsDevice device;
        private static Model sphereModel;

        private RenderTargetCube shadowMap;
        public bool withShadows { get; set; }
        private int shadowMapResoultion;
        private float depthBias;

        public static void Initialize(Effect pointLight, RenderTarget2D ColorRT, RenderTarget2D NormalRT, 
            RenderTarget2D DepthRT, Vector2 HalfPixel, FullscreenQuad Fsq,
            GraphicsDevice Device, Model SphereModel)
        {
            pointLightEffect = pointLight;
            colorRT = ColorRT;
            normalRT = NormalRT;
            depthRT = DepthRT;
            halfPixel = HalfPixel;
            fsq = Fsq;
            device = Device;
            sphereModel = SphereModel;
        }

        public PointLight(Vector3 position, Color color, float lightRadius, float intensity, bool withShadows, int shadowMapResolution)
        {
            this.position = position;
            this.color = color;
            this.radius = lightRadius;
            this.intensity = intensity;
            this.withShadows = withShadows;
            this.shadowMapResoultion = shadowMapResolution;
            if(withShadows)
                shadowMap = new RenderTargetCube(device, shadowMapResolution, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            this.depthBias = 1.0f / (20 * radius);
        }

        public void CreateLightMap()
        {
            //set the G-Buffer parameters
            pointLightEffect.Parameters["colorMap"].SetValue(colorRT);
            pointLightEffect.Parameters["normalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["depthMap"].SetValue(depthRT);
            pointLightEffect.Parameters["shadowMap"].SetValue(shadowMap);

            
            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(radius) * Matrix.CreateTranslation(position);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(Camera.viewMatrix);
            pointLightEffect.Parameters["Projection"].SetValue(Camera.projectionMatrix);
            //light position
            pointLightEffect.Parameters["lightPosition"].SetValue(position);

            //set the color, radius and Intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["lightRadius"].SetValue(radius);
            pointLightEffect.Parameters["lightIntensity"].SetValue(intensity);
            pointLightEffect.Parameters["depthPrecision"].SetValue(radius);
            pointLightEffect.Parameters["depthBias"].SetValue(depthBias);
            pointLightEffect.Parameters["shadowMapSize"].SetValue(shadowMapResoultion);
            //parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(Camera.cameraPosition);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.viewMatrix * Camera.projectionMatrix));
            //size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            pointLightEffect.Parameters["shadows"].SetValue(withShadows);

            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(Camera.cameraPosition, position);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < radius)
                device.RasterizerState = RasterizerState.CullClockwise;
            else
                device.RasterizerState = RasterizerState.CullCounterClockwise;

            device.DepthStencilState = DepthStencilState.None;

            pointLightEffect.Techniques[0].Passes[0].Apply();
            foreach (ModelMesh mesh in sphereModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    device.Indices = meshPart.IndexBuffer;
                    device.SetVertexBuffer(meshPart.VertexBuffer);

                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }

            device.RasterizerState = RasterizerState.CullCounterClockwise;
            device.DepthStencilState = DepthStencilState.Default;
        }

        public void CreateShadowMap(PointLight light, List<GameObject> Models, Effect depthWriter, Terrain terrain)
        {
            Matrix[] views = new Matrix[6];

            //Creating view matrices
            views[0] = Matrix.CreateLookAt(light.position, light.position + Vector3.Forward, Vector3.Up);
            views[1] = Matrix.CreateLookAt(light.position, light.position + Vector3.Backward, Vector3.Up);
            views[2] = Matrix.CreateLookAt(light.position, light.position + Vector3.Left, Vector3.Up);
            views[3] = Matrix.CreateLookAt(light.position, light.position + Vector3.Right, Vector3.Up);
            views[4] = Matrix.CreateLookAt(light.position, light.position + Vector3.Down, Vector3.Forward);
            views[5] = Matrix.CreateLookAt(light.position, light.position + Vector3.Up, Vector3.Backward);

            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90.0f), 1.0f, 1.0f, radius);

            depthWriter.Parameters["Projection"].SetValue(projection);
            depthWriter.Parameters["LightPosition"].SetValue(position);
            depthWriter.Parameters["DepthPrecision"].SetValue(radius);

            //Forward
            device.SetRenderTarget(shadowMap, CubeMapFace.PositiveZ);
            device.Clear(Color.Transparent);
            depthWriter.Parameters["View"].SetValue(views[0]);
            DrawModels(Models, depthWriter);

            //Backward
            device.SetRenderTarget(shadowMap, CubeMapFace.NegativeZ);
            device.Clear(Color.Transparent);
            depthWriter.Parameters["View"].SetValue(views[1]);
            DrawModels(Models, depthWriter);

            //Left
            device.SetRenderTarget(shadowMap, CubeMapFace.NegativeX);
            device.Clear(Color.Transparent);
            depthWriter.Parameters["View"].SetValue(views[2]);
            DrawModels(Models, depthWriter);

            //Right
            device.SetRenderTarget(shadowMap, CubeMapFace.PositiveX);
            device.Clear(Color.Transparent);
            depthWriter.Parameters["View"].SetValue(views[3]);
            DrawModels(Models, depthWriter);

            //Down
            device.SetRenderTarget(shadowMap, CubeMapFace.NegativeY);
            device.Clear(Color.Transparent);
            depthWriter.Parameters["View"].SetValue(views[4]);
            DrawModels(Models, depthWriter);

            //Up
            device.SetRenderTarget(shadowMap, CubeMapFace.PositiveY);
            device.Clear(Color.Transparent);
            depthWriter.Parameters["View"].SetValue(views[5]);
            DrawModels(Models, depthWriter);

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
