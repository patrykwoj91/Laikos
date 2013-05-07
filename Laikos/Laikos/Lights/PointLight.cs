using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Laikos
{
    class PointLight
    {
        private Vector3 position;
        private Color color;
        private float lightRadius;
        private float intensity;

        private static Effect pointLightEffect;
        private static RenderTarget2D colorRT;
        private static RenderTarget2D normalRT;
        private static RenderTarget2D depthRT;
        private static FullscreenQuad fsq;
        private static Vector2 halfPixel;
        private static GraphicsDevice device;
        private static Model sphereModel;

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

        public PointLight(Vector3 position, Color color, float lightRadius, float intensity)
        {
            this.position = position;
            this.color = color;
            this.lightRadius = lightRadius;
            this.intensity = intensity;
        }

        public void CreateLightMap()
        {
            //set the G-Buffer parameters
            pointLightEffect.Parameters["colorMap"].SetValue(colorRT);
            pointLightEffect.Parameters["normalMap"].SetValue(normalRT);
            pointLightEffect.Parameters["depthMap"].SetValue(depthRT);

            //compute the light world matrix
            //scale according to light radius, and translate it to light position
            Matrix sphereWorldMatrix = Matrix.CreateScale(lightRadius) * Matrix.CreateTranslation(position);
            pointLightEffect.Parameters["World"].SetValue(sphereWorldMatrix);
            pointLightEffect.Parameters["View"].SetValue(Camera.viewMatrix);
            pointLightEffect.Parameters["Projection"].SetValue(Camera.projectionMatrix);
            //light position
            pointLightEffect.Parameters["lightPosition"].SetValue(position);

            //set the color, radius and Intensity
            pointLightEffect.Parameters["Color"].SetValue(color.ToVector3());
            pointLightEffect.Parameters["lightRadius"].SetValue(lightRadius);
            pointLightEffect.Parameters["lightIntensity"].SetValue(intensity);

            //parameters for specular computations
            pointLightEffect.Parameters["cameraPosition"].SetValue(Camera.cameraPosition);
            pointLightEffect.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.viewMatrix * Camera.projectionMatrix));
            //size of a halfpixel, for texture coordinates alignment
            pointLightEffect.Parameters["halfPixel"].SetValue(halfPixel);
            //calculate the distance between the camera and light center
            float cameraToCenter = Vector3.Distance(Camera.cameraPosition, position);
            //if we are inside the light volume, draw the sphere's inside face
            if (cameraToCenter < lightRadius)
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
    }
}
