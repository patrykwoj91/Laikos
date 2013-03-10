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

        public Matrix viewMatrix { get; set; }
        public Matrix projectionMatrix { get; set; }

        private float viewAngle;
        private float aspectRatio;
        private float nearPlane;
        private float farPlane;

        private Vector3 cameraPosition;
        private float leftRightRot;
        private float upDownRot;

        const float rotationSpeed = 0.3f;
        const float moveSpeed = 30.0f;

        GraphicsDevice device;
        //*************************************************//

        public Camera(Game game)
            : base(game)
        {

        }

        public override void Initialize()
        {
            device = Game.GraphicsDevice;
            viewAngle = MathHelper.PiOver4;
            aspectRatio = device.Viewport.AspectRatio;
            nearPlane = 1.0f;
            farPlane = 1000.0f;
            cameraPosition = new Vector3(0, 200, 0);
            leftRightRot = MathHelper.PiOver2;
            upDownRot =  - MathHelper.Pi / 3.0f;
            SetUpCamera();
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            float timeDifferene = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            HandleInput(timeDifferene);
            base.Update(gameTime);
        }

        //Setting up view and projection matrix for camera
        private void SetUpCamera()
        {
            viewMatrix = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Forward);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearPlane, farPlane);
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, Vector3.Up);
        }

        private void HandleInput(float amount)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);
            KeyboardState keyState = Keyboard.GetState();
            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 1, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, -1, 1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);
            AddToCameraPosition(moveVector * amount);
        }


        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

    }
}
