using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Laikos
{
    static class Input
    {
        static KeyboardState keyboardState;

        public static MouseState oldMouseState;
        static MouseState currentMouseState;

        //***************************//
        //Variables to control camera//
        static float bezTime = 1.0f;
        static Vector3 bezStartPosition;
        static Vector3 bezMidPosition;
        static Vector3 bezEndPosition;
        static float moveSpeed = 60.0f;
        //***************************//

        //******************************************************************************************//
        //************************** Methods created to handle UNITS input *************************//
        //******************************************************************************************//

        public static void HandleUnit(ref bool walk, ref Vector3 lastPosition, ref Vector3 Position, ref Vector3 Rotation, bool picked)
        {
            keyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            lastPosition = Position;

            if (picked)
            {
                if (keyboardState.IsKeyDown(Keys.W))
                {
                    if (!walk) { walk = !walk; }
                    Position.Z += 0.1f;
                    Rotation.Y = MathHelper.ToRadians(0);
                }

                if (keyboardState.IsKeyDown(Keys.S))
                {
                    if (!walk) { walk = !walk; }
                    Position.Z -= 0.1f;
                    Rotation.Y = MathHelper.ToRadians(180);

                }

                if (keyboardState.IsKeyDown(Keys.A))
                {
                    if (!walk) { walk = !walk; }
                    Position.X -= 0.1f;
                    Rotation.Y = MathHelper.ToRadians(-90);

                }
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    if (!walk) { walk = !walk; }
                    Position.X += 0.1f;
                    Rotation.Y = MathHelper.ToRadians(90);
                }

                if (keyboardState.IsKeyUp(Keys.D) && keyboardState.IsKeyUp(Keys.S) && keyboardState.IsKeyUp(Keys.A) && keyboardState.IsKeyUp(Keys.W))
                {
                    walk = false;
                }
            }
        }

        public static void PickUnit(GameUnit unit, GraphicsDevice device)
        {
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                currentMouseState = Mouse.GetState();
                Vector2 pointerPos = new Vector2(currentMouseState.X, currentMouseState.Y);
                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);
                if (Collisions.RayModelCollision(clippedRay, unit.currentModel, unit.GetWorldMatrix()))
                    unit.picked = true;
                else
                    unit.picked = false;
            }
        }



        //******************************************************************************************//
        //************************** Methods created to handle CAMERA input ************************//
        //******************************************************************************************//

        //Handling input from mouse and keyboard to move camera
        public static void HandleCamera(float amount, ref Vector3 cameraPosition, float zoom, 
            float backBufferHeight, float backBufferWidth, float upDownRot, float leftRightRot)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);

            currentMouseState = Mouse.GetState();

                //Simple zoom in
            if (currentMouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                 if (bezTime > 1.0f)
                     InitBezier(cameraPosition, cameraPosition - new Vector3(0, zoom, zoom));
                //Simple zoom out
            if (currentMouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                 if (bezTime > 1.0f)
                     InitBezier(cameraPosition, cameraPosition - new Vector3(0, -zoom, -zoom));

            oldMouseState = currentMouseState;

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
            AddToCameraPosition(ref cameraPosition, moveVector * amount, upDownRot, leftRightRot);
        }

        //Updating viewMatrix so camera can move
        public static void UpdateViewMatrix(ref Matrix viewMatrix, Vector3 cameraPosition, float upDownRot, float leftRightRot)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);

            Vector3 cameraOriginalTarget = new Vector3(0, 0, -1);
            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = cameraPosition + cameraRotatedTarget;

            viewMatrix = Matrix.CreateLookAt(cameraPosition, cameraFinalTarget, Vector3.Forward);
        }

        //Adding vector created before to current camera position
        public static void AddToCameraPosition(ref Vector3 cameraPosition, Vector3 vectorToAdd, float upDownRot, float leftRightRot)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(upDownRot) * Matrix.CreateRotationY(leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            cameraPosition += moveSpeed * rotatedVector;
        }

        private static void InitBezier(Vector3 startPosition, Vector3 endPosition)
        {
            bezStartPosition = startPosition;
            bezEndPosition = endPosition;
            bezMidPosition = (bezStartPosition + bezEndPosition) / 2.0f;

            Vector3 cameraDirection = endPosition - startPosition;

            bezTime = 0.0f;
        }

        public static void UpdateBezier(ref Vector3 cameraPosition)
        {
            bezTime += 0.05f;
            if (bezTime > 1.0f)
                return;

            float smoothValue = MathHelper.SmoothStep(0, 1, bezTime);
            Vector3 newCamPos = Bezier(bezStartPosition, bezMidPosition, bezEndPosition, smoothValue);

            cameraPosition = newCamPos;
        }

        private static Vector3 Bezier(Vector3 startPoint, Vector3 midPoint, Vector3 endPoint, float time)
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
