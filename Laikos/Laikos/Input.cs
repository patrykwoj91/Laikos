using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using MyDataTypes;

namespace Laikos
{
    static class Input
    {
        public static KeyboardState currentKeyboardState, oldKeyboardState;
        public static MouseState currentMouseState, oldMouseState;
        public static bool selectionbox;
        public static Vector2 startDrag = new Vector2(-99, -99);
        public static bool building_mode = false;
        public static WhereToBuild next_build;

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
                       
                        unit.Position.Z += 0.1f;
                        unit.Rotation.Y = MathHelper.ToRadians(0);
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.S))
                    {
    
                        unit.Position.Z -= 0.1f;
                        unit.Rotation.Y = MathHelper.ToRadians(180);
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.A))
                    {
                 
                        unit.Position.X -= 0.1f;
                        unit.Rotation.Y = MathHelper.ToRadians(-90);
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D))
                    {
                        unit.Position.X += 0.1f;
                        unit.Rotation.Y = MathHelper.ToRadians(90);
                    }
                    if (currentKeyboardState.IsKeyDown(Keys.D5))
                    {
                        unit.HP = 0;
                    }

                }
                // Allows the game to exit
                if (currentKeyboardState.IsKeyDown(Keys.Escape) && oldKeyboardState.IsKeyUp(Keys.Escape))
                    game.Exit();

                if (currentKeyboardState.IsKeyDown(Keys.F1))
                {
                    DefferedRenderer.lightIntensity += 0.01f;
                }
                if (currentKeyboardState.IsKeyDown(Keys.F2))
                {
                    DefferedRenderer.lightIntensity -= 0.01f;
                }
            }
        }

        /// <summary>
        /// Handles mouse Left click and Right Click actions
        /// </summary>
        public static void HandleMouse(Player player, Game game, List<Decoration> decorationlist, GraphicsDevice device)
        {
            #region Left Click

            #region SelectionBox
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
            }
            #endregion

            else if (currentMouseState.LeftButton == ButtonState.Released &&
                   oldMouseState.LeftButton == ButtonState.Pressed)
            {
                #region Building
                if (building_mode == true)
                {
                    next_build.position = GetPointerCoord(device);
                    foreach (Unit unit in player.UnitList)
                        if (unit.selected == true && unit.budowniczy == true)
                        {
                            EventManager.CreateMessage(new Message((int)EventManager.Events.MoveToBuild, null, unit, next_build)); //zamiast ostatniego nulla trzeba przeslac co i gdzie ma zbudowac
                           // unit.setWalk();
                        }
                    building_mode = false;
                }
                #endregion
                #region Selections
                else if (selectionbox && (!SelectingGUI.GUIClicked(currentMouseState.X, currentMouseState.Y)))
                {
                    if ((Math.Abs(startDrag.X - currentMouseState.X) * Math.Abs(startDrag.Y - currentMouseState.Y)) >
                        100)
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        SelectInWindow(player, startDrag,
                            new Vector2(currentMouseState.X, currentMouseState.Y), device);
                        stopwatch.Stop();
                        Console.WriteLine("SelectMultipleUnits(...) : {0}", stopwatch.Elapsed);
                    }
                    else
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        SelectSingle(player, device);

                        stopwatch.Stop();
                        Console.WriteLine("SelectSingleUnit(...) : {0}", stopwatch.Elapsed);

                    }
                    SelectingGUI.selectionbox = selectionbox = false;
                    startDrag.X = -9999;
                    startDrag.Y = -9999;
                }
                #endregion
            }

            GUI.ProcessInput((Game1)game);

            #endregion

            #region Right Click
            if (currentMouseState.RightButton == ButtonState.Pressed &&
               oldMouseState.RightButton == ButtonState.Released)
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                Vector2 pointerPos = new Vector2(currentMouseState.X, currentMouseState.Y);
                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);
                Ray shorterRay = Collisions.LinearSearch(clippedRay);
                Vector3 pointerPosition = Collisions.BinarySearch(shorterRay);
                
                Object clicked = WhatClicked((Game1)game, clippedRay);

                if (clicked is Unit || clicked is Building)
                { 
                    if
                            (
                                ((clicked is Unit) && (IsEnemy((Unit)clicked, (Game1)game)))
                                ||
                                ((clicked is Building) && (IsEnemy((Building)clicked, (Game1)game)))
                            )
                    {
                        foreach (Unit _unit in player.UnitList)
                        {
                            if (_unit.selected)
                            {
                                EventManager.CreateMessage(new Message((int)EventManager.Events.MoveToAttack, null, _unit, clicked));
                               // _unit.setWalk();
                            }
                            Console.WriteLine("AttackCommand(...) : {0}", stopwatch.Elapsed);
                        }
                    }
                    else
                    {
                        EventManager.CreateMessage(new Message((int)EventManager.Events.Interaction, null, clicked, null));
                        Console.WriteLine("InteractionCommand(...) : {0}", stopwatch.Elapsed);
                    } 
                }
                else if (clicked is Decoration)
                {

                }
                else
                {
                    foreach (Unit unit in player.UnitList)
                    {
                        if (unit.selected)
                        {
                            foreach (Message _msg in unit.messages)
                            {
                                if
                                (
                                    (
                                        (_msg.Type == (int)EventManager.Events.Attack)
                                        ||
                                        (_msg.Type == (int)EventManager.Events.MoveToAttack)
                                    )
                                    &&
                                    (!_msg.Done))
                                {
                                    _msg.Done = true;
                                }
                            }

                            unit.destinyPoints = null;
                            EventManager.CreateMessage(new Message((int)EventManager.Events.MoveUnit, null, unit, pointerPosition));
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
                    if (Camera.cameraPosition.X + (Camera.cameraPosition.Y * 6) / 8 < Terrain.width - 100)
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
            HandleMouse(player, game, decorationlist, device);
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

        private static void SelectSingle(Player player, GraphicsDevice device)
        {
            bool object_clicked;
            Vector2 pointerPos = new Vector2(currentMouseState.X, currentMouseState.Y);
            Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
            Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);

            DeselectAll(player);

            for (int i = 0; i < player.UnitList.Count; i++)
            {
                object_clicked = Collisions.RayModelCollision(clippedRay, player.UnitList[i].currentModel.Model, player.UnitList[i].GetWorldMatrix());
                if (object_clicked)
                {
                    EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, player.UnitList[i], null));
                    break;
                }
            }

            for (int i = 0; i < player.BuildingList.Count; i++)
            {
                object_clicked = Collisions.RayModelCollision(clippedRay, player.BuildingList[i].currentModel.Model, player.BuildingList[i].GetWorldMatrix());
                if (object_clicked)
                {
                    EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, player.BuildingList[i], null));
                    break;
                }
            }
        }

        private static void SelectInWindow(Player player, Vector2 startDrag, Vector2 stopDrag, GraphicsDevice device)
        {
            DeselectAll(player);

            MathUtils.SafeSquare(ref startDrag, ref stopDrag);

            for (int i = 0; i < player.UnitList.Count; i++)
            {
                Vector3 pos = device.Viewport.Project(player.UnitList[i].Position, Camera.projectionMatrix,
                                                                       Camera.viewMatrix,
                                                                       Matrix.CreateTranslation(Vector3.Zero));
                if (pos.X >= startDrag.X && pos.X <= stopDrag.X && pos.Y >= startDrag.Y &&
                    pos.Y <= stopDrag.Y)
                {
                    EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, player.UnitList[i], null));
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

        public static Object WhatClicked(Game1 game, Ray clippedRay)
        {
            bool selected = false;
            foreach (Unit unit in game.player.UnitList)
            {
                selected = Collisions.RayModelCollision(clippedRay, unit.currentModel.Model, unit.GetWorldMatrix());
                if (selected)
                    return unit;
            }

            foreach (Building building in game.player.BuildingList)
            {
                selected = Collisions.RayModelCollision(clippedRay, building.currentModel.Model, building.GetWorldMatrix());
                if (selected)
                    return building;
            }

            foreach (Unit unit in game.enemy.UnitList)
            {
                selected = Collisions.RayModelCollision(clippedRay, unit.currentModel.Model, unit.GetWorldMatrix());
                if (selected)
                    return unit;
            }

            foreach (Building building in game.enemy.BuildingList)
            {
                selected = Collisions.RayModelCollision(clippedRay, building.currentModel.Model, building.GetWorldMatrix());
                if (selected)
                    return building;
            }

            foreach (Decoration decoration in game.decorations.DecorationList)
            {
                selected = Collisions.RayModelCollision(clippedRay, decoration.currentModel.Model, decoration.GetWorldMatrix());
                if (selected)
                    return decoration;
            }
            return 0;

        }

        private static bool IsEnemy(Unit _unit, Game1 _game)
        {
            return _unit.player == _game.enemy;
        }

        private static bool IsEnemy(Building _building, Game1 _game)
        {
            return _building.player == _game.enemy;
        }
    }

    public class WhereToBuild
    {
        public Building building;
        public Vector3 position = Vector3.Zero;

        public WhereToBuild(Building building)
        {
            this.building = building;
        }
    }


}


