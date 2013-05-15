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
            foreach (Unit unit in unitlist)
            {
                unit.lastPosition = unit.Position;

                if (unit.selected)
                {
                    if (currentKeyboardState.IsKeyDown(Keys.W))
                    {
                        if (!unit.walk) { unit.walk = !unit.walk; }
                        unit.Position.Z += 0.1f;
                        unit.Rotation.Y = MathHelper.ToRadians(0);
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.S))
                    {
                        if (!unit.walk) { unit.walk = !unit.walk; }
                        unit.Position.Z -= 0.1f;
                        unit.Rotation.Y = MathHelper.ToRadians(180);
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.A))
                    {
                        if (!unit.walk) { unit.walk = !unit.walk; }
                        unit.Position.X -= 0.1f;
                        unit.Rotation.Y = MathHelper.ToRadians(-90);
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D))
                    {
                        if (!unit.walk) { unit.walk = !unit.walk; }
                        unit.Position.X += 0.1f;
                        unit.Rotation.Y = MathHelper.ToRadians(90);
                    }

                    if (currentKeyboardState.IsKeyDown(Keys.D1))
                    {
                        unit.currentModel.player.PlayClip("Idle",false);
                        //unit.currentModel.player.Looping = true;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D2))
                    {
                        unit.currentModel.player.PlayClip("Walk",false);
                      //  unit.currentModel.player.Looping = true;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D3))
                    {
                        unit.currentModel.player.PlayClip("Run", false);
                       // unit.player.Looping = false;
                    }
                   /* if (currentKeyboardState.IsKeyDown(Keys.D4))
                    {
                        unit.currentModel.player.PlayClip("Heavy_Fire", false);
                        //unit.player.Looping = false;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D5))
                    {
                        unit.currentModel.player.PlayClip("Transform", false);
                        //unit.player.Looping = false;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D6))
                    {
                        unit.currentModel.player.PlayClip("Rotate", false);
                        //unit.player.Looping = false;
                    }*/
                }

                if (currentKeyboardState.IsKeyDown(Keys.F1) && oldKeyboardState.IsKeyUp(Keys.F1))
                {
                    DefferedRenderer.debug = !DefferedRenderer.debug;
                }
            }
        }

        /// <summary>
        /// Handles mouse Left click and Right Click actions
        /// </summary>
        public static void HandleMouse(List<Unit> unitlist, List<Decoration> decorationlist, GraphicsDevice device)
        {
            List<GameObject> allObjects = new List<GameObject>(unitlist.Count + decorationlist.Count); //ad building list later here
            allObjects.AddRange(unitlist);
            allObjects.AddRange(decorationlist);

            #region Left Click (Selecting)
            if (currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released)
            {
                Console.WriteLine("Left Button " + currentMouseState.LeftButton.ToString());
                bool selected = false;
                
                Vector2 pointerPos = new Vector2(currentMouseState.X, currentMouseState.Y);
                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);

                for (int i = 0; i < allObjects.Count; i++)
                {
                    selected = Collisions.RayModelCollision(clippedRay, allObjects[i].currentModel.Model, allObjects[i].GetWorldMatrix());
                    if (selected)
                    {
                        if ((allObjects[i] is Unit) && !allObjects[i].selected)
                        {
                            foreach (Unit unit in unitlist)
                                EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));

                            EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, allObjects[i], null));
                            break;
                        }
                    }
                    if (!selected)
                        foreach (GameObject unit in unitlist)
                            EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));
                }                             
            }
            #endregion

            #region Right Click (Moving and Interactions)
            if (currentMouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
            {
                Console.WriteLine("Right Button " + currentMouseState.RightButton.ToString());

                bool selected = false;
                bool obj_selected = false;
                Vector2 pointerPos = new Vector2(currentMouseState.X, currentMouseState.Y);
                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);
                Ray shorterRay = Collisions.LinearSearch(clippedRay);
                Vector3 pointerPosition = Collisions.BinarySearch(shorterRay);

                foreach (GameObject obj in allObjects) //random right click?
                    if (obj.selected == true)
                    {
                        obj_selected = true;
                        break;
                    }
                if (obj_selected) //if not random right click
                {
                    foreach (GameObject obj in allObjects)
                    {
                        selected = Collisions.RayModelCollision(clippedRay, obj.currentModel.Model, obj.GetWorldMatrix());

                        if (selected && (obj is Unit || obj is Decoration))
                            foreach (GameObject reciever in allObjects)
                                if (reciever.selected)
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.Interaction, null, reciever, obj)); //interaction Event unit - unit , unit-decoration , unit-buiding itp.
                        if(!selected)
                            EventManager.CreateMessage(new Message((int)EventManager.Events.MoveUnit, null, obj, pointerPosition));
                    }

                }
            }
            #endregion
        }

        /// <summary>
        /// Handling input from mouse and keyboard to move camera
        /// </summary>
        public static void HandleCamera(float amount, Camera cam)
        {
            Vector3 moveVector = new Vector3(0, 0, 0);

            //Simple zoom in
            if (currentMouseState.ScrollWheelValue > oldMouseState.ScrollWheelValue)
                if (cam.bezTime > 1.0f)
                    cam.InitBezier(true);
            //Simple zoom out
            if (currentMouseState.ScrollWheelValue < oldMouseState.ScrollWheelValue)
                if (cam.bezTime > 1.0f)
                    cam.InitBezier(false);

            if (cam.bezTime > 1.0f)
            {
                //Moving camera if mouse is near edge of screen
                if (Mouse.GetState().X > cam.backBufferWidth - 5.0f) //right
                    moveVector += new Vector3(3, 0, 0);
                if (Mouse.GetState().X < 5.0f)    //left
                    moveVector += new Vector3(-3, 0, 0);
                if (Mouse.GetState().Y > cam.backBufferHeight - 5.0f)   //down
                    moveVector += new Vector3(0, -2, 2);
                if (Mouse.GetState().Y < 5.0f)    //up
                    moveVector += new Vector3(0, 2, -2);
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
            HandleMouse(unitlist,decorationlist, device);
            HandleKeyboard(unitlist);

            oldMouseState = currentMouseState;
            oldKeyboardState = currentKeyboardState;
        }
    }
}
