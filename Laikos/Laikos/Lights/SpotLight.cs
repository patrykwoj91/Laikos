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
        Vector3 direction { get; set; }
        Color color { get; set; }

        float intensity { get; set; }
        float nearPlane { get; set; }
        float farPlane { get; set; }
        float FOV { get; set; }

        bool withShadows { get; set; }
        int shadowMapResolution { get; set; }
        float depthBias { get; set; }

        Matrix world { get; set; }
        Matrix view { get; set; }
        Matrix projection { get; set; }

        RenderTarget2D shadowMap { get; set; }
        Texture2D attentuationTexture { get; set; }
        GraphicsDevice device;

        public SpotLight(GraphicsDevice device, Vector3 position, Vector3 direction, Color color, float intensity, 
            bool withShadows, int shadowMapResolution, Texture2D attentuationTexture)
        {
            this.position = position;
            this.device = device;
            this.direction = direction;
            this.color = color;
            this.intensity = intensity;
            this.withShadows = withShadows;
            this.shadowMapResolution = shadowMapResolution;
            this.attentuationTexture = attentuationTexture;

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
    }
}
