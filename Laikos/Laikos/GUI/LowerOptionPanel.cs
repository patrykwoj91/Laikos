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
    class LowerOptionPanel
    {
        public static int width;
        public static int height;
        private static Texture2D optionPanel;
        private static Texture2D cmentarz;
        public static float upTime;
        public static float downTime;
        private static Rectangle position;
        public static Rectangle cementaryPosition;

        public static void Initialize(ContentManager content)
        {
            optionPanel = content.Load<Texture2D>("GUI/tab");
            cmentarz = content.Load<Texture2D>("GUI/option_panel/buildings/cmentarz");
            width = LowerBackground.width / 2;
            height = 75;
            position = new Rectangle(UnitBackground.width + 50, GUI.screenHeight , width, height);
            cementaryPosition = new Rectangle(UnitBackground.width + 55, GUI.screenHeight, 120, 70);
            upTime = 1.0f;
            downTime = 1.0f;
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(optionPanel, position, Color.White);
            if (UnitBackground.whichUnit == 0)
                spriteBatch.Draw(cmentarz, cementaryPosition, Color.White);
        }

        public static void MoveUp()
        {

            upTime += 0.01f;
            if (upTime >= 1.0f)
            {
                UnitBackground.isUp = true;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight, GUI.screenHeight - (LowerBackground.height + height) / 2, upTime);
            int y = (int)MathHelper.SmoothStep(GUI.screenHeight, GUI.screenHeight - 85, upTime);
            position.Y = x;
            cementaryPosition.Y = y;
        }

        public static void MoveDown()
        {
            downTime += 0.01f;
            if (downTime >= 1.0f)
            {
                UnitBackground.isUp = false;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight - (LowerBackground.height + height) / 2, GUI.screenHeight, downTime);
            int y = (int)MathHelper.SmoothStep(GUI.screenHeight - 85, GUI.screenHeight, downTime);
            position.Y = x;
            cementaryPosition.Y = y;
        }
    }
}
