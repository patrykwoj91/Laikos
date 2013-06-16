using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Laikos
{
    static class Input
    {
        static KeyboardState currentKeyboardState, oldKeyboardState;
        static MouseState currentMouseState, oldMouseState;

        /// <summary>
        /// Handle Keyboard - temporary unit WSAD movement and Animation swap
        /// </summary>
        public static void HandleKeyboard(List<Unit> unitlist)
        {

        }

        /// <summary>
        /// Handles mouse Left click and Right Click actions
        /// </summary>
        public static void HandleMouse(List<Unit> unitlist, List<Decoration> decorationlist, GraphicsDevice device)
        {

        }

        /// <summary>
        /// Handling input from mouse and keyboard to move camera
        /// </summary>
        public static void HandleCamera(float amount, Camera cam)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);

            //Simple zoom in
            if (currentMouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
            {
                if (cam.bezTime > 1.0f)
                {
                    cam.InitBezier(true);
                }
            }

            //Simple zoom out
            if (currentMouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
            {
                if (cam.bezTime > 1.0f)
                {
                    cam.InitBezier(false);
                }
            }

            if (cam.bezTime > 1.0f)
            {
                //Moving camera if mouse is near edge of screen
                if (Mouse.GetState().X > cam.backBufferWidth - 5.0f) //right
                {
                    moveVector += new Vector3(3, 0, 0);
                }
                if (Mouse.GetState().X < 5.0f) //left
                {
                    moveVector += new Vector3(-3, 0, 0);
                }
                if (Mouse.GetState().Y > cam.backBufferHeight - 5.0f) //down
                {
                    moveVector += new Vector3(0, -2, 2);
                }
                if (Mouse.GetState().Y < 5.0f) //up
                {
                    moveVector += new Vector3(0, 2, -2);
                }
            }

            //add created earlier vector to camera position
            Matrix cameraRotation = Matrix.CreateRotationX(cam.upDownRot) * Matrix.CreateRotationY(cam.leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(moveVector * amount, cameraRotation);
            Camera.cameraPosition += cam.moveSpeed * rotatedVector;
        }

        /// <summary>
        /// Handling Inputs: Camera Movement, Keyboard, Mouse Buttons 
        /// </summary>
        public static void Update(GameTime gameTime, GraphicsDevice device, Camera camera, List<Unit> unitlist, List<Decoration> decorationlist)
        {
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            HandleCamera(timeDifference, camera);
            HandleMouse(unitlist, decorationlist, device);
            HandleKeyboard(unitlist);

            oldMouseState = currentMouseState;
            oldKeyboardState = currentKeyboardState;
        }
    }
}
