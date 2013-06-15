using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Laikos
{
    static class GUI
    {
        public static int screenWidth;
        public static int screenHeight;
        private static SpriteBatch spriteBatch;


        public static void Initialize(GraphicsDevice Device, SpriteBatch SpriteBatch, ContentManager content)
        {
            screenWidth = Device.PresentationParameters.BackBufferWidth;
            screenHeight = Device.PresentationParameters.BackBufferHeight;
            spriteBatch = SpriteBatch;
            MinimapBackground.Initialize(content);
            UpperBackground.Initialize(content);
            UnitBackground.Initialize(content);
            LowerBackground.Initialize(content);
        }

        public static void Draw()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            //Minimap
            MinimapBackground.Create(spriteBatch);
            UpperBackground.Create(spriteBatch);
            LowerBackground.Create(spriteBatch);
            UnitBackground.Create(spriteBatch);
            spriteBatch.End();
        }

        public static void Update(GameTime gameTime)
        {
            UnitBackground.UpdateAnimation(gameTime);

            if (UnitBackground.isUp && UnitBackground.time <= 1.0f)
            {
                UnitBackground.MoveUp();
                LowerBackground.MoveUp();
            }
            else if (!UnitBackground.isUp && UnitBackground.time <= 1.0f)
            {
                UnitBackground.MoveDown();
                LowerBackground.MoveDown();
            }
        }

        public static void ProcessInput()
        {
            if (Input.currentMouseState.LeftButton == ButtonState.Pressed)
            {

                if (Input.currentMouseState.X < Minimap.width + Minimap.diff && Input.currentMouseState.Y < Minimap.height + Minimap.diff)
                {
                    Camera.cameraPosition.X = Input.currentMouseState.X * 5;
                    Camera.cameraPosition.Z = Input.currentMouseState.Y * 5 + 75;
                }

                if (UnitBackground.time > 1.0f && LowerBackground.time > 1.0f)
                {
                        UnitBackground.time = 0;
                        LowerBackground.time = 0;
                }
            }
        }
    }
}
