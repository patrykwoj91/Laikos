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

        public static bool selectionbox, drawselectionbox;
        private static SpriteBatch spriteBatch;
        private static SpriteFont spriteFont;
        public static Vector2 startDrag = new Vector2(-99, -99);
        public static Vector2 stopDrag;
        private static Texture2D pixel;


        public static void Init(GraphicsDeviceManager graphics, Game game)
        {
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = game.Content.Load<SpriteFont>("Georgia");
            pixel = game.Content.Load<Texture2D>("selection");
        }
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
            List<GameObject> allObjects = new List<GameObject>(); //ad building list later here
            allObjects.AddRange(player.UnitList);
            allObjects.AddRange(decorationlist);
            allObjects.AddRange(player.BuildingList);

            #region Left Click (Selecting)
            // MOUSE DRAG - START
            if (currentMouseState.LeftButton == ButtonState.Pressed &&
                oldMouseState.LeftButton == ButtonState.Pressed)
            {
                if (!MiniMapClicked(currentMouseState.X, currentMouseState.Y))
                {
                    selectionbox = true;

                    if (startDrag.X < 0)
                    {
                        startDrag.X = currentMouseState.X;
                        startDrag.Y = currentMouseState.Y;

                        drawselectionbox = true;
                        startDrag.X = stopDrag.X = currentMouseState.X;
                        startDrag.Y = stopDrag.Y = currentMouseState.Y;
                    }
                    else
                    {
                        stopDrag.X = currentMouseState.X;
                        stopDrag.Y = currentMouseState.Y;
                    }
                }
            } // MOUSE DRAG - STOP
            else if (currentMouseState.LeftButton == ButtonState.Released &&
                   oldMouseState.LeftButton == ButtonState.Pressed)
            {
                if (selectionbox && (!MiniMapClicked(currentMouseState.X, currentMouseState.Y)))
                {

                    if ((Math.Abs(startDrag.X - currentMouseState.X) * Math.Abs(startDrag.Y - currentMouseState.Y)) >
                        100)
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        SelectUnitsInWindow(player, startDrag,
                            new Vector2(currentMouseState.X, currentMouseState.Y), allObjects, device);
                        stopwatch.Stop();
                        Console.WriteLine("SelectMultipleUnits(...) : {0}", stopwatch.Elapsed);
                    }
                    else
                    {
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                        SelectSingleUnit(player, device, allObjects);

                        stopwatch.Stop();
                        Console.WriteLine("SelectSingleUnit(...) : {0}", stopwatch.Elapsed);
                        
                    }
                    drawselectionbox = selectionbox = false;
                    startDrag.X = -9999;
                    startDrag.Y = -9999;
                }
            }

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {

                if (currentMouseState.X < 200 && currentMouseState.Y < 200 && currentMouseState.X > 0 && currentMouseState.Y > 0)
                {
                    Camera.cameraPosition.X = currentMouseState.X * 5;
                    Camera.cameraPosition.Z = currentMouseState.Y * 5 + 75;
                }
            }
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
                            Console.WriteLine("SendCommand(...) : {0}", stopwatch.Elapsed);
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
                                Unit test = (Unit)obj;
                                Console.WriteLine(test.messages.Count);
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
                   if(Camera.cameraPosition.X + (Camera.cameraPosition.Y*6)/8 < Terrain.width-200)
                    moveVector += new Vector3(3, 0, 0);
                if (Mouse.GetState().X < 5.0f)    //left
                    if (Camera.cameraPosition.X - (Camera.cameraPosition.Y * 6) / 8 > 0 + 100)
                    moveVector += new Vector3(-3, 0, 0);

                if (Mouse.GetState().Y > cam.backBufferHeight - 5.0f)   //down
                    if (Camera.cameraPosition.Z < Terrain.height)
                    moveVector += new Vector3(0, -2, 2);
              
                if (Mouse.GetState().Y < 5.0f)    //up
                    if (Camera.cameraPosition.Z - Camera.cameraPosition.Y > 0 + Camera.cameraPosition.Y*1.4)
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
        public static void Update(Game game,GameTime gameTime, GraphicsDevice device, Camera camera, Player player, List<Decoration> decorationlist)
        {
            

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            HandleCamera(gameTime, camera);
            HandleMouse(player,decorationlist, device);
            HandleKeyboard(player,game, device);

            oldMouseState = currentMouseState;
            oldKeyboardState = currentKeyboardState;
        }

        public static void Draw()
        {
            if (selectionbox)
                DrawSelection();
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

       private static void DrawSelection()
       {
           MathUtils.SafeSquare(ref startDrag, ref stopDrag);
           spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
           spriteBatch.Draw(pixel, startDrag, null, Color.White, 0.0f, Vector2.Zero,
                            new Vector2(stopDrag.X - startDrag.X, 1), SpriteEffects.None, 0);
           spriteBatch.Draw(pixel, startDrag, null, Color.White, 0.0f, Vector2.Zero,
                            new Vector2(1, stopDrag.Y - startDrag.Y), SpriteEffects.None, 0);
           spriteBatch.Draw(pixel, new Vector2(startDrag.X + (stopDrag.X - startDrag.X), startDrag.Y), null,
                            Color.White, 0.0f, Vector2.Zero, new Vector2(1, stopDrag.Y - startDrag.Y),
                            SpriteEffects.None, 0);
           spriteBatch.Draw(pixel, new Vector2(startDrag.X, startDrag.Y + (stopDrag.Y - startDrag.Y)), null,
                            Color.White, 0.0f, Vector2.Zero, new Vector2(stopDrag.X - startDrag.X, 1),
                            SpriteEffects.None, 0);
           spriteBatch.End();
       }

        private static void SelectSingleUnit(Player player,GraphicsDevice device, List<GameObject> allObjects)
        {
                bool object_clicked;
                Vector2 pointerPos = new Vector2(currentMouseState.X, currentMouseState.Y);
                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);

                DeselectAllUnits(player);

                for (int i = 0; i < allObjects.Count; i++)
                {
                    object_clicked = Collisions.RayModelCollision(clippedRay, allObjects[i].currentModel.Model, allObjects[i].GetWorldMatrix());
                    if (object_clicked)
                    {
                        if (allObjects[i] is Unit)
                        {
                           EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, allObjects[i], null));
                           Unit test = (Unit)allObjects[i];
                           Console.WriteLine(test.messages.Count);
                           Console.WriteLine(test.selected);
                           break;
                        }
                    }
                }
        }

        private static void SelectUnitsInWindow(Player player, Vector2 startDrag, Vector2 stopDrag, List<GameObject> allObjects,GraphicsDevice device)
        {
            DeselectAllUnits(player);

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
            
        private static bool MiniMapClicked(float X,float Y)
        {
            if (X < 200 && Y < 200)
                return true;
            else
                return false;
        }

        private static void DeselectAllUnits(Player player)
        {
             foreach (Unit unit in player.UnitList)
                                EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));
        }
    }
}

