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
        private static int width;
        private static int height;
        private static Texture2D lowerBackground;
        private static Rectangle position;
        private static Rectangle source;
        public static float time;

        public static void Initialize(ContentManager content)
        {
            lowerBackground = content.Load<Texture2D>("GUI/desen");
            width = GUI.screenWidth - UnitBackground.width;
            height = 100;
            position = new Rectangle(UnitBackground.width, GUI.screenHeight - height, width, height);
            source = new Rectangle(0, 0, lowerBackground.Width, lowerBackground.Height);
            time = 1.0f;
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(lowerBackground, position, source, Color.White);
        }

        public static void MoveUp()
        {

            time += 0.01f;
            if (time >= 1.0f)
            {
                UnitBackground.isUp = false;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight - height, GUI.screenHeight, time);
            position.Y = x;
        }

        public static void MoveDown()
        {
            time += 0.01f;
            if (time >= 1.0f)
            {
                UnitBackground.isUp = true;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight, GUI.screenHeight - height, time);
            position.Y = x;
        }
    }
}
