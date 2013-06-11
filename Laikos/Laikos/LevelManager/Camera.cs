using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Laikos
{
    class Camera : GameComponent
    {
        //***************************************************//
        //Variables created to store information about camera//
        //***************************************************//

        //View and projection matrix
        public static Matrix viewMatrix;
        public static Matrix projectionMatrix;
        public static Matrix reflectionViewMatrix;

        //Basic camera settings
        private float viewAngle;
        private float aspectRatio;
        private float nearPlane;
        private float farPlane;

        //Camera settings used to move 
        public static Vector3 cameraPosition;
        public Vector3 cameraFront;
        public static float leftRightRot;
        public static float upDownRot;
        private float zoom;
        private MouseState ms;
        public static Vector3 cameraFinalTarget;
        //******************************//

        //Additional features
        private bool shake;
        private float shakeTime;
        private int intensity;
        //***************************//

        //Variables to control camera//
        public float bezTime = 1.0f;
        static Vector3 bezStartPosition;
        static Vector3 bezMidPosition;
        static Vector3 bezEndPosition;
        public float moveSpeed = 60.0f;

        //***************************//

        //Constant variables that describe camera parameters
        public float backBufferHeight;
        public float backBufferWidth;

        //Variable links to hardware
        GraphicsDevice device;

        //*************************************************//

        public Camera(Game game, GraphicsDeviceManager graphics)
            : base(game)
        {
            //Resolution of the game used to move camera with mouse
            backBufferHeight = graphics.PreferredBackBufferHeight;
            backBufferWidth = graphics.PreferredBackBufferWidth;
        }

        //Here we initialize all variables
        public override void Initialize()
        {
            //Initializing hardware variables
            device = Game.GraphicsDevice;
            //Initializing camera initial parameters
            viewAngle = MathHelper.PiOver4;
            aspectRatio = device.Viewport.AspectRatio;
            nearPlane = 1.0f;
            farPlane = 2000.0f;
            zoom = 10.0f;
            Camera.upDownRot = MathHelper.ToRadians(-45);
            Camera.cameraPosition = new Vector3(30, 80, 100);
            cameraFront = new Vector3(0.5848334f, -0.7775874f, -0.230928f);
            leftRightRot = MathHelper.ToRadians(0.0f);
            //Initializing projection matrix
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearPlane, farPlane);
            cameraFinalTarget = cameraPosition;
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateViewMatrix(ref viewMatrix, cameraPosition, upDownRot, leftRightRot);
            UpdateBezier(ref cameraPosition);
            //Checking for collision with terrain if camera is within range of our terrain

            ms = Mouse.GetState();

            // check if mouse is not out of window
            if (ms.X < 3) Mouse.SetPosition(3, ms.Y);
            if (ms.X > backBufferWidth - 3) Mouse.SetPosition((int)backBufferWidth - 3, ms.Y);

            if (ms.Y < 3) Mouse.SetPosition(ms.X, 3);
            if (ms.Y > backBufferHeight - 3) Mouse.SetPosition(ms.X, (int)backBufferHeight - 3);


            if (cameraPosition.Z < Terrain.width -1 && cameraPosition.Z < Terrain.height - 1)
            {
                Collisions.CheckWithTerrain(ref cameraPosition, zoom);
            }
            if (shake)
            {
                int sign;

                sign = MathUtils.RandomNumber(0, intensity);
                if (sign % 2 == 0) cameraPosition.X += MathUtils.RandomNumber(0, intensity);
                else cameraPosition.X -= MathUtils.RandomNumber(0, intensity);

                sign = MathUtils.RandomNumber(0, intensity);
                if (sign % 2 == 0) cameraPosition.Z += MathUtils.RandomNumber(0, intensity);
                else cameraPosition.Z -= MathUtils.RandomNumber(0, intensity);

                shakeTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (shakeTime <= 0)
                {
                    shake = false;
                }
            }
            base.Update(gameTime);
        }

        public void UpdateViewMatrix(ref Matrix viewMatrix, Vector3 cameraPosition, float upDownRot, float leftRightRot)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            //cameraPosition = Vector3.Transform(cameraPosition - cameraTarget, Matrix.CreateFromAxisAngle(axis, angle)) + cameraTarget;
            cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            Vector3 reflCameraPosition = cameraPosition;
            reflCameraPosition.Y = -cameraPosition.Y + Water.waterHeight * 2;
            Vector3 reflTargetPosition = cameraFinalTarget;
            reflTargetPosition.Y = -cameraFinalTarget.Y + Water.waterHeight * 2;

            Vector3 cameraRight = Vector3.Transform(new Vector3(1, 0, 0), cameraRotation);
            Vector3 invUpVector = Vector3.Cross(cameraRight, reflTargetPosition - reflCameraPosition);
            reflectionViewMatrix = Matrix.CreateLookAt(reflCameraPosition, reflTargetPosition, invUpVector);

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, Vector3.Forward);
        }

        public void InitBezier(bool scroll)
        {
            bezStartPosition = cameraPosition;
            if (scroll)
            {
                bezEndPosition = cameraPosition - new Vector3(0, zoom, zoom);
                Vector3 cameraDirection = bezEndPosition - cameraPosition;
            }
            if (!scroll)
            {
                bezEndPosition = cameraPosition - new Vector3(0, -zoom, -zoom);
                Vector3 cameraDirection = bezEndPosition - cameraPosition;
            }
            bezMidPosition = (bezStartPosition + bezEndPosition) / 2.0f;
            bezTime = 0.0f;
        }
        public void UpdateBezier(ref Vector3 cameraPosition)
        {
            bezTime += 0.05f;
            if (bezTime > 1.0f)
                return;

            float smoothValue = MathHelper.SmoothStep(0, 1, bezTime);
            Vector3 newCamPos = Bezier(bezStartPosition, bezMidPosition, bezEndPosition, smoothValue);

            cameraPosition = newCamPos;
        }
        private Vector3 Bezier(Vector3 startPoint, Vector3 midPoint, Vector3 endPoint, float time)
        {
            float invTime = 1.0f - time;
            float powTime = (float)Math.Pow(time, 2);
            float powInvTime = (float)Math.Pow(invTime, 2);

            Vector3 result = startPoint * powInvTime;
            result += 2 * midPoint * time * invTime;
            result += endPoint * powTime;

            return result;
        }

        public void CameraShake(float _seconds, int _intensity)
        {
            if (_seconds > 10) _seconds = 10;
            if (_seconds < 0) _seconds = 1;
            if (_intensity < 0) _intensity = 1;
            if (_intensity > 10) _intensity = 10;

            shake = true;
            shakeTime = _seconds;
            intensity = _intensity;
        }
    }
}
