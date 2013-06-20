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
        public static bool building_mode = false;

        /// <summary>
        /// Handle Keyboard - temporary unit WSAD movement and Animation swap
        /// </summary>
        public static void HandleKeyboard(List<Unit> unitlist)
        {

        }

        /// <summary>
        /// Handles mouse Left click and Right Click actions
        /// </summary>
        public static void HandleMouse(List<Unit> unitlist, List <Building> buildingList, List<Decoration> decorationlist, GraphicsDevice device)
        {
            List<GameObject> allObjects = new List<GameObject>(unitlist.Count + decorationlist.Count + buildingList.Count); //ad building list later here
            allObjects.AddRange(unitlist);
            allObjects.AddRange(buildingList);
            allObjects.AddRange(decorationlist);

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

                            foreach (Unit unit in unitlist)
                            {
                                EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));
                            }

                            foreach (Building building in buildingList)
                            {
                                EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, building, null));
                            }

                            foreach (Decoration decoration in decorationlist)
                            {
                                EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, decorationlist, null));
                            }

                            EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, allObjects[i], null));
                            break;
                    }
                }
                if (!selected)
                {
                    foreach (Unit unit in unitlist)
                    {
                        EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));
                    }

                    foreach (Building building in buildingList)
                    {
                        EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, building, null));
                    }

                    foreach (Decoration decoration in decorationlist)
                    {
                        EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, decorationlist, null));
                    }
                }
            }
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
        public static void Update(GameTime gameTime, GraphicsDevice device, Camera camera, List<Unit> unitlist, List<Building> buildingList, List<Decoration> decorationlist)
        {
            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();

            HandleCamera(timeDifference, camera);
            HandleMouse(unitlist, buildingList, decorationlist, device);
            HandleKeyboard(unitlist);

            oldMouseState = currentMouseState;
            oldKeyboardState = currentKeyboardState;
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
