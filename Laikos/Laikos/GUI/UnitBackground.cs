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
    static class UnitBackground
    {
        public static int width;
        public static int height;
        private static Texture2D unitBackground;
        private static Rectangle position;
        public static bool isUp;
        public static float upTime;
        public static float downTime;
        private static GifAnimation.GifAnimation animation;

        public static void Initialize(ContentManager content)
        {
            width = 150;
            height = 150;
            position = new Rectangle(0, GUI.screenHeight, width, height);
            unitBackground = content.Load<Texture2D>("GUI/tlo");
            animation = content.Load<GifAnimation.GifAnimation>("GUI/GIFS/Drone_Worker");
            isUp = true;
            upTime = 1.0f;
            downTime = 1.0f;
        }

        public static void UpdateAnimation(GameTime gameTime)
        {
            animation.Update(gameTime.ElapsedGameTime.Ticks * 3);
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(animation.GetTexture(), position, Color.White);
        }

        public static void MoveUp()
        {

            upTime += 0.01f;
            if (upTime >= 1.0f)
            {
                isUp = true;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight, GUI.screenHeight - UnitBackground.width, upTime);
            position.Y = x;
        }

        public static void MoveDown()
        {
            downTime += 0.01f;
            if (downTime >= 1.0f)
            {
                isUp = false; 
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight - UnitBackground.width, GUI.screenHeight, downTime);
            position.Y = x;
        }
    }
}
