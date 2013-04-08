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
        const float rotationSpeed = 0.3f;
        const float moveSpeed = 60.0f;
        float backBufferHeight;
        float backBufferWidth;

        //Variable links to hardware
        GraphicsDevice device;
        MouseState oldMouseState;

        //Variables to control fly-by camera
        float bezTime = 1.0f;
        Vector3 bezStartPosition;
        Vector3 bezMidPosition;
        Vector3 bezEndPosition;

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
            oldMouseState = Mouse.GetState();
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
            HandleInput(timeDifference);
            UpdateBezier();
            //Checking for collision with terrain if camera is within range of our terrain
            if (cameraPosition.X < Terrain.currentWidth -1 && cameraPosition.Z < Terrain.currentHeight - 1)
            {
                Collisions.CheckWithTerrain(ref cameraPosition, zoom);
            }
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
            KeyboardState kb = Keyboard.GetState();

            MouseState currentMouseState = Mouse.GetState();
            //if (cameraPosition.Z > 50 && cameraPosition.Z < 200)
            {
                //Simple zoom in
                if (currentMouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                    if (bezTime > 1.0f)
                        InitBezier(cameraPosition, cameraPosition - new Vector3(0, zoom, zoom));
                //Simple zoom out
                if (currentMouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                    if (bezTime > 1.0f)
                        InitBezier(cameraPosition, cameraPosition - new Vector3(0, -zoom, -zoom));

                oldMouseState = currentMouseState;

            }
            if (bezTime > 1.0f)
            {
                //Moving camera if mouse is near edge of screen
                if (Mouse.GetState().X > backBufferWidth - 5.0f) //right
                    moveVector += new Vector3(3, 0, 0);
                if (Mouse.GetState().X < 5.0f)    //left
                    moveVector += new Vector3(-3, 0, 0);
                if (Mouse.GetState().Y > backBufferHeight - 5.0f)   //down
                    moveVector += new Vector3(0, -2, 2);
                if (Mouse.GetState().Y < 5.0f)    //up
                    moveVector += new Vector3(0, 2, -2);
            }
            //add created earlier vector to camera position
                AddToCameraPosition(moveVector * amount);
        }

        //Adding vector created before to current camera position
        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot) ;
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        private void InitBezier(Vector3 startPosition, Vector3 endPosition)
        {
            bezStartPosition = startPosition;
            bezEndPosition = endPosition;
            bezMidPosition = (bezStartPosition + bezEndPosition) / 2.0f;

            Vector3 cameraDirection = endPosition - startPosition;

            bezTime = 0.0f;
        }

        private void UpdateBezier()
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
