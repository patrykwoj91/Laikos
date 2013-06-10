using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Laikos
{
    class DirectionalLight
    {
        private Vector3 direction;
        private Color color;
        private float intensity;

        private static Effect directionalLight;
        private static RenderTarget2D colorRT;
        private static RenderTarget2D normalRT;
        private static RenderTarget2D depthRT;
        private static FullscreenQuad fsq;
        private static Vector2 halfPixel;

        public static void Initialize(Effect DirectionalLight, RenderTarget2D ColorRT, RenderTarget2D NormalRT, RenderTarget2D DepthRT, Vector2 HalfPixel, FullscreenQuad Fsq)
        {
            directionalLight = DirectionalLight;
            colorRT = ColorRT;
            normalRT = NormalRT;
            depthRT = DepthRT;
            halfPixel = HalfPixel;
            fsq = Fsq;
        }

        public DirectionalLight(Vector3 direction, Color color, float intensity)
        {
            this.direction = direction;
            this.color = color;
            this.intensity = intensity;
        }

        public void CreateLightMap()
        {
            directionalLight.Parameters["colorMap"].SetValue(colorRT);
            directionalLight.Parameters["normalMap"].SetValue(normalRT);
            directionalLight.Parameters["depthMap"].SetValue(depthRT);

            directionalLight.Parameters["lightDirection"].SetValue(direction);
            directionalLight.Parameters["Color"].SetValue(color.ToVector3());
            directionalLight.Parameters["lightIntensity"].SetValue(intensity);

            directionalLight.Parameters["cameraPosition"].SetValue(Camera.cameraPosition);
            directionalLight.Parameters["InvertViewProjection"].SetValue(Matrix.Invert(Camera.viewMatrix * Camera.projectionMatrix));

            directionalLight.Parameters["halfPixel"].SetValue(halfPixel);

            directionalLight.Techniques[0].Passes[0].Apply();
            fsq.Render(Vector2.One * -1, Vector2.One);
        }
    }
}
