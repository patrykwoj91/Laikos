using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Laikos
{
    static class Input
    {
        public static KeyboardState currentKeyboardState, oldKeyboardState;
        public static MouseState currentMouseState, oldMouseState;

        public static bool selectionbox;
        public static Vector2 startDrag = new Vector2(-99, -99);
        public static Vector2 stopDrag;
        public static bool building_mode = false;
   
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
                        unit.currentModel.player.PlayClip("Idle", true);
                        //unit.currentModel.player.Looping = true;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D2))
                    {
                        unit.currentModel.player.PlayClip("Walk", true);
                        //  unit.currentModel.player.Looping = true;
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D3))
                    {
                        unit.currentModel.player.PlayClip("Attack", true);
                        // unit.player.Looping = false;
                    }

                    if (currentKeyboardState.IsKeyDown(Keys.D5))
                    {
                        unit.HP = 0;
                        Console.WriteLine(unit.HP);
                    }

                }

                if (currentKeyboardState.IsKeyDown(Keys.B) && oldKeyboardState.IsKeyUp(Keys.B))
                {
                    building_mode = true;
                    //i zwraca nazwe budynku na podstawie miniaturki
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
            List<GameObject> allObjects = new List<GameObject>(); //ad building list later here
            allObjects.AddRange(player.UnitList);
            allObjects.AddRange(decorationlist);
            allObjects.AddRange(player.BuildingList);

            #region Left Click (Selecting)
            // MOUSE DRAG - START

            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                oldMouseState.LeftButton == ButtonState.Pressed)
            {
                if (!SelectingGUI.GUIClicked(currentMouseState.X, currentMouseState.Y))
                {
                    selectionbox = true;

                    if (startDrag.X < 0)
                    {
                        startDrag.X = currentMouseState.X;
                        startDrag.Y = currentMouseState.Y;

                        SelectingGUI.selectionbox = true;
                        SelectingGUI.startDrag.X = SelectingGUI.stopDrag.X = currentMouseState.X;
                        SelectingGUI.startDrag.Y = SelectingGUI.stopDrag.Y = currentMouseState.Y;
                    }
                    else
                    {
                        SelectingGUI.startDrag.X = startDrag.X;
                        SelectingGUI.startDrag.Y = startDrag.Y;

                        SelectingGUI.stopDrag.X = currentMouseState.X;
                        SelectingGUI.stopDrag.Y = currentMouseState.Y;
                    }
                }
            } // MOUSE DRAG - STOP

                
            else if (currentMouseState.LeftButton == ButtonState.Released &&
                   oldMouseState.LeftButton == ButtonState.Pressed)
            {
                if (selectionbox && (!SelectingGUI.GUIClicked(currentMouseState.X, currentMouseState.Y)))
                {

                    if ((Math.Abs(startDrag.X - currentMouseState.X) * Math.Abs(startDrag.Y - currentMouseState.Y)) >
                        100)
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        SelectInWindow(player, startDrag,
                            new Vector2(currentMouseState.X, currentMouseState.Y), allObjects, device);
                        stopwatch.Stop();
                        Console.WriteLine("SelectMultipleUnits(...) : {0}", stopwatch.Elapsed);
                    }
                    else
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        SelectSingle(player, device, allObjects);

                        stopwatch.Stop();
                        Console.WriteLine("SelectSingleUnit(...) : {0}", stopwatch.Elapsed);

                    }
                    SelectingGUI.selectionbox = selectionbox = false;
                    startDrag.X = -9999;
                    startDrag.Y = -9999;
                }
            }
            if (currentMouseState.LeftButton == ButtonState.Released &&
                   oldMouseState.LeftButton == ButtonState.Pressed)
            {
                if (building_mode == true)
                {
                     foreach (Unit unit in player.UnitList)
                         if (unit.selected == true && unit.budowniczy == true)
                         {
                             Vector3 pointerPosition = GetPointerCoord(device);
                             Laikos.PathFiding.Wspolrzedne wspBegin = new Laikos.PathFiding.Wspolrzedne((int)unit.Position.X, (int)unit.Position.Z);
                             Laikos.PathFiding.Wspolrzedne wspEnd = new Laikos.PathFiding.Wspolrzedne((int)pointerPosition.X, (int)pointerPosition.Z);

                             DateTime tp0 = DateTime.Now;
                             unit.destinyPoints = unit.pathFiding.obliczSciezke(wspBegin, wspEnd);
                             unit.destinyPointer = null;
                             DateTime tp1 = DateTime.Now;
                          
                             if (unit.destinyPoints.Count > 0)
                             {
                                 EventManager.CreateMessage(new Message((int)EventManager.Events.Build, null, unit, pointerPosition)); //zamiast ostatniego nulla trzeba przeslac co i gdzie ma zbudowac
                                 unit.walk = true;
                             }
                             building_mode = false;
                         }
                }

            }
            GUI.ProcessInput();
            #endregion

            #region Right Click (Moving and Interactions)
            if (currentMouseState.RightButton == ButtonState.Pressed &&
               oldMouseState.RightButton == ButtonState.Released)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

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
                    }
                if (obj_selected) //if not random right click
                {
                    foreach (GameObject obj in allObjects)
                    {
                        selected = Collisions.RayModelCollision(clippedRay, obj.currentModel.Model, obj.GetWorldMatrix());

                        if (selected && (obj is Unit || obj is Building))
                        {
                            foreach (GameObject sender in allObjects)
                            {
                                if (sender.selected)
                                {
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.Interaction, sender, obj, null)); //interaction Event unit - unit , unit-decoration , unit-buiding itp.
                                }
                            }
                            Console.WriteLine("SendCommand(...) : {0}", stopwatch.Elapsed);
                        }
                    }
                    if (!selected)
                    {
                        foreach (GameObject obj in allObjects)
                            if (obj.selected == true && obj is Unit)
                            {
                              /*  for (int i = 0; i < ((Unit)obj).messages.Count; ++i)
                                {
                                    if (((Unit)obj).messages[i].Type == (int)EventManager.Events.MoveUnit)
                                    {
                                        ((Unit)obj).messages.Remove (((Unit)obj).messages[i]);
                                    }
                                }*/

                                Laikos.PathFiding.Wspolrzedne wspBegin = new Laikos.PathFiding.Wspolrzedne((int)((Unit)obj).Position.X, (int)((Unit)obj).Position.Z);
                                Laikos.PathFiding.Wspolrzedne wspEnd = new Laikos.PathFiding.Wspolrzedne((int)pointerPosition.X, (int)pointerPosition.Z);

                                DateTime tp0 = DateTime.Now;
                                ((Unit)obj).destinyPoints = ((Unit)obj).pathFiding.obliczSciezke(wspBegin, wspEnd);
                                ((Unit)obj).destinyPointer = null;
                                DateTime tp1 = DateTime.Now;
                                //Console.WriteLine(tp1 - tp0);

                                if (((Unit)obj).destinyPoints.Count > 0)
                                {
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.MoveUnit, null, obj, pointerPosition));
                                    ((Unit)obj).walk = true;
                                }
                                stopwatch.Stop();
                                Console.WriteLine("MoveCommand(...) : {0}", stopwatch.Elapsed);
                            }
                    }
                }
            }
            #endregion
        }


        /// <summary>
        /// Handling input from mouse and keyboard to move camera
        /// </summary>
        public static void HandleCamera(GameTime gameTime, Camera cam)
        {
            float amount = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            var time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

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
                    if (Camera.cameraPosition.X + (Camera.cameraPosition.Y * 6) / 8 < Terrain.width - 200)
                        moveVector += new Vector3(3, 0, 0);
                if (Mouse.GetState().X < 5.0f)    //left
                    if (Camera.cameraPosition.X - (Camera.cameraPosition.Y * 6) / 8 > 0 + 100)
                        moveVector += new Vector3(-3, 0, 0);

                if (Mouse.GetState().Y > cam.backBufferHeight - 5.0f)   //down
                    if (Camera.cameraPosition.Z < Terrain.height)
                        moveVector += new Vector3(0, -2, 2);

                if (Mouse.GetState().Y < 5.0f)    //up
                    if (Camera.cameraPosition.Z - Camera.cameraPosition.Y > 0 + Camera.cameraPosition.Y * 1.4)
                        //  if (.Z > 0)
                        moveVector += new Vector3(0, 2, -2);


            }

            //add created earlier vector to camera position
            Matrix cameraRotation = Matrix.CreateRotationX(Camera.upDownRot) * Matrix.CreateRotationY(Camera.leftRightRot);
            Vector3 rotatedVector = Vector3.Transform(moveVector * amount, cameraRotation);
            Camera.cameraPosition += cam.moveSpeed * rotatedVector;

            float pitch = 0.0f;
            float turn = 0.0f;

            /*  if (currentMouseState.MiddleButton == ButtonState.Pressed)
              {
                  if (currentMouseState.Y > oldMouseState.Y)
                      pitch += time * 0.0025f;

                  if (currentMouseState.Y < oldMouseState.Y)
                      pitch -= time * 0.0025f;

                  if (currentMouseState.X < oldMouseState.X)
                      turn += time * 0.0025f;

                  if (currentMouseState.X > oldMouseState.X)
                      turn -= time * 0.0025f;

                  oldMouseState = currentMouseState;
                  Console.WriteLine("noob");
              }

              Vector3 cameraRight = Vector3.Cross(Vector3.Up, cam.cameraFront);
              Vector3 flatFront = Vector3.Cross(cameraRight, Vector3.Up);

              Matrix pitchMatrix = Matrix.CreateFromAxisAngle(cameraRight, pitch);
              Matrix turnMatrix = Matrix.CreateFromAxisAngle(Vector3.Up, turn);

              Vector3 tiltedFront = Vector3.TransformNormal(cam.cameraFront, pitchMatrix *
                                                                         turnMatrix);

              if (Vector3.Dot(tiltedFront, flatFront) > 0.1f && Vector3.Dot(tiltedFront, Vector3.Up) <= 0.2f)
              {
                  cam.cameraFront = Vector3.Normalize(tiltedFront);
              }*/

            // camera shake
            if (currentKeyboardState.IsKeyDown(Keys.Q) && oldKeyboardState.IsKeyUp(Keys.Q))
            {
                cam.CameraShake(0.8f, 3);
            }
        }

        /// <summary>
        /// Handling Inputs: Camera Movement, Keyboard, Mouse Buttons 
        /// </summary>
        public static void Update(Game game, GameTime gameTime, GraphicsDevice device, Camera camera, Player player, List<Decoration> decorationlist)
        {


            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            HandleCamera(gameTime, camera);
            HandleMouse(player, decorationlist, device);
            HandleKeyboard(player, game, device);

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
    
        private static void SelectSingle(Player player, GraphicsDevice device, List<GameObject> allObjects)
        {
            bool object_clicked;
            Vector2 pointerPos = new Vector2(currentMouseState.X, currentMouseState.Y);
            Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
            Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);

            DeselectAll(player);

            for (int i = 0; i < allObjects.Count; i++)
            {
                object_clicked = Collisions.RayModelCollision(clippedRay, allObjects[i].currentModel.Model, allObjects[i].GetWorldMatrix());
                if (object_clicked)
                {
                    if (allObjects[i] is Unit)
                    {
                        Console.WriteLine("lol");
                        EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, allObjects[i], null));
                        break;
                    }
                    if (allObjects[i] is Building)
                    {
                        Building building = (Building)allObjects[i];
                        if (building.selectable == true)
                            EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, allObjects[i], null));
                        break;          
                    }
                }
            }
        }

        private static void SelectInWindow(Player player, Vector2 startDrag, Vector2 stopDrag, List<GameObject> allObjects, GraphicsDevice device)
        {
            DeselectAll(player);

            MathUtils.SafeSquare(ref startDrag, ref stopDrag);

            for (int i = 0; i < allObjects.Count; i++)
            {
                if (allObjects[i] is Unit)
                {
                    Vector3 pos = device.Viewport.Project(allObjects[i].Position, Camera.projectionMatrix,
                                                                           Camera.viewMatrix,
                                                                           Matrix.CreateTranslation(Vector3.Zero));
                    if (pos.X >= startDrag.X && pos.X <= stopDrag.X && pos.Y >= startDrag.Y &&
                        pos.Y <= stopDrag.Y)
                    {
                        EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, allObjects[i], null));
                    }
                }
            }
        }

        private static void DeselectAll(Player player)
        {
            foreach (Unit unit in player.UnitList)
                EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));

            foreach (Building building in player.BuildingList)
                EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, building, null));
        }
    }
}


