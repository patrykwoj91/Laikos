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
    static class LowerBackground
    {
        public static int width;
        public static int height;
        private static Texture2D lowerBackground;
        private static Rectangle position;
        private static Rectangle source;
        public static float upTime;
        public static float downTime;

        public static void Initialize(ContentManager content)
        {
            lowerBackground = content.Load<Texture2D>("GUI/desen");
            width = GUI.screenWidth - UnitBackground.width;
            height = 100;
            position = new Rectangle(UnitBackground.width, GUI.screenHeight, width, height);
            source = new Rectangle(0, 0, lowerBackground.Width, lowerBackground.Height);
            upTime = 1.0f;
            downTime = 1.0f;
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(lowerBackground, position, source, Color.White);
        }

        public static void MoveUp()
        {

            upTime += 0.03f;
            if (upTime >= 1.0f)
            {
                UnitBackground.isUp = true;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight, GUI.screenHeight - height, upTime);
            position.Y = x;
        }

        public static void MoveDown()
        {
            downTime += 0.03f;
            if (downTime >= 1.0f)
            {
                UnitBackground.isUp = false;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight - height, GUI.screenHeight, downTime);
            position.Y = x;
        }
    }
}
