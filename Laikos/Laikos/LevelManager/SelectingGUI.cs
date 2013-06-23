using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Laikos
{
    public static class SelectingGUI
    {
        public static bool selectionbox;
        public static Vector2 startDrag;
        public static Vector2 stopDrag;
        private static SpriteBatch spriteBatch;
        private static SpriteFont spriteFont;
        private static Texture2D pixel;
        private static Texture2D mmap_units;
        private static Texture2D healthbar;
        private static Texture2D mmap_buildings;
        private static Texture2D selectionBox;
        private static List<Unit> player_Units;
        private static List<Building> player_Buildings;
        private static List<Unit> enemy_Units;
        private static List<Building> enemy_Buildings;
        private static Vector2 tmp = new Vector2();
        private static GraphicsDevice Device;
        private static GraphicsDeviceManager Graphics;

        public static void Init(GraphicsDevice device, GraphicsDeviceManager graphics, Game game, List<Unit> units, List<Building> buildings,List<Unit> units2, List<Building> buildings2)
        {
            Graphics = graphics;
            Device = device;
            spriteBatch = new SpriteBatch(Graphics.GraphicsDevice);

            spriteFont = game.Content.Load<SpriteFont>("Georgia");
            pixel = game.Content.Load<Texture2D>("selection");
            mmap_units = game.Content.Load<Texture2D>("mmap_units");
            mmap_buildings = game.Content.Load<Texture2D>("mmap_buildings");
            healthbar = game.Content.Load<Texture2D>("healthbar");
            selectionBox = game.Content.Load<Texture2D>("SelectionBox");
            player_Units = units;
            player_Buildings = buildings;
            enemy_Units = units2;
            enemy_Buildings = buildings2;
        }

        public static bool GUIClicked(float X, float Y)
        {
            if (UnitBackground.isUp && (Input.currentMouseState.Y < GUI.screenHeight - UnitBackground.height ||
                (Input.currentMouseState.Y < GUI.screenHeight - LowerBackground.height && Input.currentMouseState.X > UnitBackground.width)) &&
                Input.currentMouseState.Y > MinimapBackground.height ||
                (Input.currentMouseState.Y < MinimapBackground.height && Input.currentMouseState.X > MinimapBackground.width && Input.currentMouseState.Y > UpperBackground.height) ||
                !UnitBackground.isUp && Input.currentMouseState.X < GUI.screenWidth && Input.currentMouseState.Y < GUI.screenHeight &&
                Input.currentMouseState.Y > MinimapBackground.height ||
                (Input.currentMouseState.Y < MinimapBackground.height && Input.currentMouseState.X > MinimapBackground.width && Input.currentMouseState.Y > UpperBackground.height))
                return false;
            else
                return true;
        }

        private static void DrawSelection(SpriteBatch spriteBatch)
        {
            MathUtils.SafeSquare(ref startDrag, ref stopDrag);
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
            
        }

        private static void DrawOnMiniMap(SpriteBatch spriteBatch)
        {

            foreach (Unit unit in player_Units)
            {
                    tmp.X = unit.Position.X / 5;
                    tmp.Y = unit.Position.Z / 5;
                if (unit.selected ==false)
                    spriteBatch.Draw(mmap_units, tmp, Color.CornflowerBlue);
                else
                    spriteBatch.Draw(mmap_units, tmp, Color.White);
            }
            foreach (Building building in player_Buildings)
            {
                tmp.X = building.Position.X / 5;
                tmp.Y = building.Position.Z / 5;
                if (building.selected == false)
                    spriteBatch.Draw(mmap_buildings, tmp, Color.CornflowerBlue);
                else
                    spriteBatch.Draw(mmap_buildings, tmp, Color.White);
            }
        }

        public static void DrawUnitInfo(SpriteBatch spriteBatch)
        {
           
            foreach (Building building in player_Buildings)
            {
                // Project the 3d position first
                Vector3 screenPos3D = Device.Viewport.Project(BoundingSphere.CreateFromBoundingBox(building.boundingBox).Center, Camera.projectionMatrix, Camera.viewMatrix, Matrix.Identity);
                // Just to make it easier to use we create a Vector2 from screenPos3D
                Vector2 screenPos2D = new Vector2(screenPos3D.X, screenPos3D.Y - BoundingSphere.CreateFromBoundingBox(building.boundingBox).Radius * 2);

                if (building.selected == true)
                {
                    spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)(screenPos2D.Y - healthbar.Height / 2), healthbar.Width, healthbar.Height), null, Color.Red);
                    spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)(screenPos2D.Y - healthbar.Height / 2), (int)(healthbar.Width * ((double)building.HP / building.maxHP)), healthbar.Height), null, Color.ForestGreen);
                }
                /* else
                 {
                     // Draw the healthbar
                     spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)screenPos2D.Y - healthbar.Height / 2, healthbar.Width, healthbar.Height), null, Color.Red * 0.5f);
                     spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)screenPos2D.Y - healthbar.Height / 2, (int)(healthbar.Width * ((double)building.HP / building.maxHP)), healthbar.Height), null, Color.ForestGreen * 0.5f);
                 }*/
            }

             foreach (Unit unit in player_Units)
            {
                // Project the 3d position first
                Vector3 screenPos3D = Device.Viewport.Project(unit.boundingSphere.Center, Camera.projectionMatrix, Camera.viewMatrix, Matrix.Identity);
                // Just to make it easier to use we create a Vector2 from screenPos3D
                Vector2 screenPos2D = new Vector2(screenPos3D.X, screenPos3D.Y - unit.boundingSphere.Radius * 7);

                if (unit.selected == true)
                {
                    spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)(screenPos2D.Y - healthbar.Height/2), healthbar.Width, healthbar.Height), null, Color.Red);
                    spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)(screenPos2D.Y - healthbar.Height / 2), (int)(healthbar.Width * ((double)unit.HP / unit.maxHP)), healthbar.Height), null, Color.ForestGreen);
                    spriteBatch.Draw(selectionBox, new Rectangle((int)screenPos2D.X - 20, (int)screenPos2D.Y + 10, 35, 35), Color.White);

                }
               /* else
                {
                    // Draw the healthbar
                    spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)screenPos2D.Y - healthbar.Height / 2, healthbar.Width, healthbar.Height), null, Color.Red*0.5f);
                    spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)screenPos2D.Y - healthbar.Height / 2, (int)(healthbar.Width * ((double)unit.HP / unit.maxHP)), healthbar.Height), null, Color.ForestGreen*0.5f);
                }*/
            }

             foreach (Building building in enemy_Buildings)
             {
                 // Project the 3d position first
                 Vector3 screenPos3D = Device.Viewport.Project(BoundingSphere.CreateFromBoundingBox(building.boundingBox).Center, Camera.projectionMatrix, Camera.viewMatrix, Matrix.Identity);
                 // Just to make it easier to use we create a Vector2 from screenPos3D
                 Vector2 screenPos2D = new Vector2(screenPos3D.X, screenPos3D.Y - BoundingSphere.CreateFromBoundingBox(building.boundingBox).Radius * 2);

                 
                 
                     spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)(screenPos2D.Y - healthbar.Height / 2), healthbar.Width, healthbar.Height), null, Color.Red);
                     spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)(screenPos2D.Y - healthbar.Height / 2), (int)(healthbar.Width * ((double)building.HP / building.maxHP)), healthbar.Height), null, Color.Violet);
                 
                 /* else
                  {
                      // Draw the healthbar
                      spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)screenPos2D.Y - healthbar.Height / 2, healthbar.Width, healthbar.Height), null, Color.Red * 0.5f);
                      spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)screenPos2D.Y - healthbar.Height / 2, (int)(healthbar.Width * ((double)building.HP / building.maxHP)), healthbar.Height), null, Color.ForestGreen * 0.5f);
                  }*/
             }

             foreach (Unit unit in enemy_Units)
             {
                 // Project the 3d position first
                 Vector3 screenPos3D = Device.Viewport.Project(unit.boundingSphere.Center, Camera.projectionMatrix, Camera.viewMatrix, Matrix.Identity);
                 // Just to make it easier to use we create a Vector2 from screenPos3D
                 Vector2 screenPos2D = new Vector2(screenPos3D.X, screenPos3D.Y - unit.boundingSphere.Radius * 7);

               
                 
                     spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)(screenPos2D.Y - healthbar.Height / 2), healthbar.Width, healthbar.Height), null, Color.Red);
                     spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)(screenPos2D.Y - healthbar.Height / 2), (int)(healthbar.Width * ((double)unit.HP / unit.maxHP)), healthbar.Height), null, Color.Violet);
                 
                 /* else
                  {
                      // Draw the healthbar
                      spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)screenPos2D.Y - healthbar.Height / 2, healthbar.Width, healthbar.Height), null, Color.Red*0.5f);
                      spriteBatch.Draw(healthbar, new Rectangle((int)screenPos2D.X - healthbar.Width / 2, (int)screenPos2D.Y - healthbar.Height / 2, (int)(healthbar.Width * ((double)unit.HP / unit.maxHP)), healthbar.Height), null, Color.ForestGreen*0.5f);
                  }*/
             }
            
        }

        public static void Draw()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            
            DrawOnMiniMap(spriteBatch);
            if (selectionbox)
                DrawSelection(spriteBatch);

            //DrawUnitInfo(spriteBatch);

            

            spriteBatch.End();
        }

    }
}
