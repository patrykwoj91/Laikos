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

        GraphicsDevice device;
        //*************************************************//

        public Camera(Game game)
            : base(game)
        {

        }

        //Here we initialize all our variables
        public override void Initialize()
        {
            device = Game.GraphicsDevice;
            viewAngle = MathHelper.PiOver4;
            aspectRatio = device.Viewport.AspectRatio;
            nearPlane = 1.0f;
            farPlane = 800.0f;
            cameraPosition = new Vector3(0, -400, 0);
            leftRightRot = MathHelper.ToRadians(0.0f);
            upDownRot =  MathHelper.PiOver4;
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, nearPlane, farPlane);
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            float timeDifferene = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            HandleInput(timeDifferene);
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
            if (Mouse.GetState().X > 1360.0f)
                moveVector += new Vector3(-1, 0, 0);
            if (Mouse.GetState().X < 5.0f)
                moveVector += new Vector3(1, 0, 0);
            if (Mouse.GetState().Y >760.0f)
                moveVector += new Vector3(0, 1, 1);
            if (Mouse.GetState().Y < 5.0f)
                moveVector += new Vector3(0, -1, -1);
            
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

    }
}
