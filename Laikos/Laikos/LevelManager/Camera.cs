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

        //Basic camera settings
        private float viewAngle;
        private float aspectRatio;
        private float nearPlane;
        private float farPlane;

        //Camera settings used to move 
        public Vector3 cameraPosition;
        public float leftRightRot;
        public float upDownRot;
        private float zoom;

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
            //Input.oldMouseState = Mouse.GetState();
            //Initializing camera initial parameters
            viewAngle = MathHelper.PiOver4;
            aspectRatio = device.Viewport.AspectRatio;
            nearPlane = 1.0f;
            farPlane = 200.0f;
            zoom = 10.0f;
            cameraPosition = new Vector3(30, 80, 100);
            leftRightRot = MathHelper.ToRadians(0.0f);
            upDownRot = MathHelper.ToRadians(-45);
            //Initializing projection matrix
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearPlane, farPlane);
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateViewMatrix(ref viewMatrix, cameraPosition, upDownRot, leftRightRot);
            UpdateBezier(ref cameraPosition);
            //Checking for collision with terrain if camera is within range of our terrain
            if (cameraPosition.X < Terrain.currentWidth - 1 && cameraPosition.Z < Terrain.currentHeight - 1)
            {
                Collisions.CheckWithTerrain(ref cameraPosition, zoom);
            }
            base.Update(gameTime);
        }

        public void UpdateViewMatrix(ref Matrix viewMatrix, Vector3 cameraPosition, float upDownRot, float leftRightRot)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            //cameraPosition = Vector3.Transform(cameraPosition - cameraTarget, Matrix.CreateFromAxisAngle(axis, angle)) + cameraTarget;
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

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

    }
}
