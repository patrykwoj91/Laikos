using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Laikos
{
    class Camera : GameComponent
    {
        //***************************************************//
        //Variables created to store information about camera//
        //***************************************************//

        //View and projection matrix
        public Matrix viewMatrix { get; set; }
        public Matrix projectionMatrix { get; set; }

        //Basic camera settings
        private float viewAngle;
        private float aspectRatio;
        private float nearPlane;
        private float farPlane;

        //Camera settings used to move 
        private Vector3 cameraPosition;
        private float leftRightRot;
        private float upDownRot;

        //Constant variables that describe camera parameters
        const float rotationSpeed = 0.3f;
        const float moveSpeed = 60.0f;
        private float zoomSpeed = 30.0f;
        float backBufferHeight;
        float backBufferWidth;

        //Variable links to hardware
        GraphicsDevice device;
        MouseState oldMouseState;
        Terrain terrain;
        //*************************************************//

        public Camera(Game game, GraphicsDeviceManager graphics)
            : base(game)
        {
            backBufferHeight = graphics.PreferredBackBufferHeight;
            backBufferWidth = graphics.PreferredBackBufferWidth;
            terrain = new Terrain(game);
        }

        //Here we initialize all variables
        public override void Initialize()
        {
            //Initializing hardware variables
            device = Game.GraphicsDevice;
            oldMouseState = Mouse.GetState();
            //Initializing camera initial parameters
            viewAngle = MathHelper.PiOver4;
            aspectRatio = device.Viewport.AspectRatio;
            nearPlane = 1.0f;
            farPlane = 800.0f;
            cameraPosition = new Vector3(0, 50, 60);
            leftRightRot = MathHelper.ToRadians(0.0f);
            upDownRot = MathHelper.ToRadians(-45.0f);
            //Initializing projection matrix
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearPlane, farPlane);
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            HandleInput(timeDifference);
            CheckCameraCollision();
            base.Update(gameTime);
        }

        //Updating viewMatrix so camera can move
        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, Vector3.Forward);
        }

        //Handling input from mouse and keyboard to move camera
        private void HandleInput(float amount)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);

            MouseState currentMouseState = Mouse.GetState();
            //if (cameraPosition.Z > 50 && cameraPosition.Z < 200)
            {
                //Simple zoom in
                if (currentMouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                    moveVector += new Vector3(0, 0, -zoomSpeed);
                //Simple zoom out
                if (currentMouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                    moveVector += new Vector3(0, 0, zoomSpeed);
                oldMouseState = currentMouseState;

            }
            //Moving camera if mouse is near edge of screen
            if (Mouse.GetState().X > backBufferWidth - 5.0f) //right
                moveVector += new Vector3(3, 0, 0);
            if (Mouse.GetState().X < 5.0f)    //left
                moveVector += new Vector3(-3, 0, 0);
            if (Mouse.GetState().Y > backBufferHeight - 5.0f)   //down
                moveVector += new Vector3(0, -2, 2);
            if (Mouse.GetState().Y < 5.0f)    //up
                moveVector += new Vector3(0, 2, -2);
            
            //add created earlier vector to camera position
                AddToCameraPosition(moveVector * amount);
        }

        //Adding vector created before to current camera position
        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        private void CheckCameraCollision()
        {
            float terrainHeight = terrain.GetExactHeightAt(cameraPosition.X, cameraPosition.Z);
            if (cameraPosition.Y < terrainHeight + zoomSpeed)
            {
                Vector3 newPos = cameraPosition;
                newPos.Y = (terrainHeight + zoomSpeed);
                cameraPosition = newPos;
            }
        }
    }
}
