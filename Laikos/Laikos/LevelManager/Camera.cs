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
        private float leftRightRot;
        private float upDownRot;
        private float zoom;

        //Constant variables that describe camera parameters
        float backBufferHeight;
        float backBufferWidth;

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
            Input.oldMouseState = Mouse.GetState();
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
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            Input.HandleCamera(timeDifference, ref cameraPosition, zoom, backBufferHeight, backBufferWidth, upDownRot, leftRightRot);
            Input.UpdateViewMatrix(ref viewMatrix, cameraPosition, upDownRot, leftRightRot);
            Input.UpdateBezier(ref cameraPosition);
            //Checking for collision with terrain if camera is within range of our terrain
            if (cameraPosition.X < Terrain.width -1 && cameraPosition.Z < Terrain.height - 1)
            {
                Collisions.CheckWithTerrain(ref cameraPosition, zoom);
            }
            base.Update(gameTime);
        }
    }
}
