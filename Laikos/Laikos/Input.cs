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
        public static void HandleKeyboard(Player player, Game game, GraphicsDevice device)
        {
            foreach (Unit unit in player.UnitList)
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
                        unit.currentModel.player.PlayClip("Idle",true);
                        //unit.currentModel.player.Looping = true;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D2))
                    {
                        unit.currentModel.player.PlayClip("Walk",true);
                      //  unit.currentModel.player.Looping = true;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D3))
                    {
                        unit.currentModel.player.PlayClip("Run", true);
                       // unit.player.Looping = false;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D4))
                    {
                        unit.currentModel.player.PlayClip("Alert", false);
                        //unit.player.Looping = false;
                    }
                    
                    if (currentKeyboardState.IsKeyDown(Keys.D5))
                    {
                        unit.HP = 0;
                        Console.WriteLine(unit.HP);
                    }
                    
                }

                if (currentKeyboardState.IsKeyDown(Keys.B) && oldKeyboardState.IsKeyUp(Keys.B))
                {
                    player.Build(player.BuildingTypes["Pałac rady"], GetPointerCoord(device));
                }

                // Allows the game to exit
                if (currentKeyboardState.IsKeyDown(Keys.Escape) && oldKeyboardState.IsKeyUp(Keys.Escape))
                    game.Exit();

                if (currentKeyboardState.IsKeyDown(Keys.F1) && oldKeyboardState.IsKeyUp(Keys.F1))
                {
                    DefferedRenderer.debug = false;
                }
                if (currentKeyboardState.IsKeyDown(Keys.F2) && oldKeyboardState.IsKeyUp(Keys.F2))
                {
                    DefferedRenderer.debug = true;
                }
            }
        }

        /// <summary>
        /// Handles mouse Left click and Right Click actions
        /// </summary>
        public static void HandleMouse(Player player, List<Decoration> decorationlist, GraphicsDevice device)
        {
            List<GameObject> allObjects = new List<GameObject>(player.UnitList.Count + decorationlist.Count); //ad building list later here
            allObjects.AddRange(player.UnitList);
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
                            foreach (Unit unit in player.UnitList)
                                EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));

                            EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, allObjects[i], null));
                            break;
                        }
                    }
                    if (!selected)
                        foreach (GameObject unit in player.UnitList)
                            EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));
                }
            }

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {

                if (currentMouseState.X < 200 && currentMouseState.Y < 200 && currentMouseState.X > 0 && currentMouseState.Y > 0)
                {
                    Camera.cameraPosition.X = currentMouseState.X * 3;
                    Camera.cameraPosition.Z = currentMouseState.Y * 3;
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

                        if (selected && (obj is Unit || obj is Building))
                        {
                            foreach (GameObject reciever in allObjects)
                            {
                                if (reciever.selected)
                                {
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.Interaction, null, reciever, obj)); //interaction Event unit - unit , unit-decoration , unit-buiding itp.
                                }
                            }
                        }

                        if ((!selected) && (obj.GetType() == typeof(Unit)))
                        {
                            Laikos.PathFiding.Wspolrzedne wspBegin = new Laikos.PathFiding.Wspolrzedne ((int)((Unit)obj).Position.X, (int)((Unit)obj).Position.Z);
                            Laikos.PathFiding.Wspolrzedne wspEnd = new Laikos.PathFiding.Wspolrzedne ((int)pointerPosition.X, (int) pointerPosition.Z);

                            DateTime tp0 = DateTime.Now;
                            ((Unit)obj).destinyPoints = ((Unit)obj).pathFiding.obliczSciezke(wspBegin, wspEnd);
                            ((Unit)obj).destinyPointer = null;
                            DateTime tp1 = DateTime.Now;
                            Console.WriteLine(tp1 - tp0);

                            if (((Unit)obj).destinyPoints.Count > 0)
                            {
                                EventManager.CreateMessage(new Message((int)EventManager.Events.MoveUnit, null, obj, pointerPosition));
                            }
                        }
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
            Matrix cameraRotation = Matrix.CreateRotationX(Camera.upDownRot) * Matrix.CreateRotationY(Camera.leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(moveVector * amount, cameraRotation);
            Camera.cameraPosition += cam.moveSpeed * rotatedVector;
        }

        /// <summary>
        /// Handling Inputs: Camera Movement, Keyboard, Mouse Buttons 
        /// </summary>
        public static void Update(Game game,GameTime gameTime, GraphicsDevice device, Camera camera, Player player, List<Decoration> decorationlist)
        {
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            HandleCamera(timeDifference, camera);
            HandleMouse(player,decorationlist, device);
            HandleKeyboard(player,game, device);

            oldMouseState = currentMouseState;
            oldKeyboardState = currentKeyboardState;
        }

       public static Vector3 GetPointerCoord(GraphicsDevice device)
        {
            Vector2 pointerPos = new Vector2(currentMouseState.X, currentMouseState.Y);
            Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
            Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);
            Ray shorterRay = Collisions.LinearSearch(clippedRay);
            Vector3 pointerPosition = Collisions.BinarySearch(shorterRay);

            return pointerPosition;
        }
    }
}
