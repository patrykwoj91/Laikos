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
        private static Texture2D observatory;
        private static Texture2D palace1;
        private static Texture2D palace3;
        private static Texture2D guardTower;
        private static Texture2D tank;
        private static Texture2D dronWorker;
        private static Texture2D eye;
        public static float upTime;
        public static float downTime;
        public static bool isUnit;
        private static Rectangle position;
        public static Rectangle firstTabPosition;
        public static Rectangle secondTabPosition;
        public static Rectangle thirdTabPosition;
        public static Rectangle fourthTabPosition;
        private static int tabWidth = 120;
        private static int tabHeight = 70;

        public static void Initialize(ContentManager content)
        {
            optionPanel = content.Load<Texture2D>("GUI/tab");
            cmentarz = content.Load<Texture2D>("GUI/option_panel/buildings/cmentarz");
            observatory = content.Load<Texture2D>("GUI/option_panel/buildings/obserwatorium");
            palace1 = content.Load<Texture2D>("GUI/option_panel/buildings/palacp1");
            palace3 = content.Load<Texture2D>("GUI/option_panel/buildings/palacp3");
            guardTower = content.Load<Texture2D>("GUI/option_panel/buildings/straznica");
            tank = content.Load<Texture2D>("GUI/option_panel/units/czolg");
            dronWorker = content.Load<Texture2D>("GUI/option_panel/units/dron_worker");
            eye = content.Load<Texture2D>("GUI/option_panel/units/oko");
            width = LowerBackground.width / 2;
            
            height = 75;
            int x = UnitBackground.width + 50;
            position = new Rectangle(UnitBackground.width + 50, GUI.screenHeight , width, height);
            firstTabPosition = new Rectangle(x, GUI.screenHeight, tabWidth, tabHeight);
            secondTabPosition = new Rectangle(x + width / 4, GUI.screenHeight, tabWidth, tabHeight);
            thirdTabPosition = new Rectangle(x + 2 * width / 4, GUI.screenHeight, tabWidth, tabHeight);
            fourthTabPosition = new Rectangle(x + 3 * width / 4, GUI.screenHeight, tabWidth, tabHeight);
            upTime = 1.0f;
            downTime = 1.0f;
            isUnit = true;
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(optionPanel, position, Color.White);
            if (UnitBackground.whichUnit == 0 && isUnit)
            {
                spriteBatch.Draw(cmentarz, firstTabPosition, Color.White);
                spriteBatch.Draw(observatory, secondTabPosition, Color.White);
                spriteBatch.Draw(palace1, thirdTabPosition, Color.White);
                spriteBatch.Draw(guardTower, fourthTabPosition, Color.White);
            }
            if (UnitBackground.whichUnit == 1 && !isUnit)
            {
                spriteBatch.Draw(tank, firstTabPosition, Color.White);
                spriteBatch.Draw(dronWorker, secondTabPosition, Color.White);
                spriteBatch.Draw(eye, thirdTabPosition, Color.White);
            }
        }

        public static void MoveUp()
        {

            upTime += 0.03f;
            if (upTime >= 1.0f)
            {
                UnitBackground.isUp = true;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight, GUI.screenHeight - (LowerBackground.height + height) / 2, upTime);
            int y = (int)MathHelper.SmoothStep(GUI.screenHeight, GUI.screenHeight - 85, upTime);
            position.Y = x;
            firstTabPosition.Y = y;
            secondTabPosition.Y = y;
            thirdTabPosition.Y = y;
            fourthTabPosition.Y = y;
        }

        public static void MoveDown()
        {
            downTime += 0.03f;
            if (downTime >= 1.0f)
            {
                UnitBackground.isUp = false;
                return;
            }

            int x = (int)MathHelper.SmoothStep(GUI.screenHeight - (LowerBackground.height + height) / 2, GUI.screenHeight, downTime);
            int y = (int)MathHelper.SmoothStep(GUI.screenHeight - 85, GUI.screenHeight, downTime);
            position.Y = x;
            firstTabPosition.Y = y;
            secondTabPosition.Y = y;
            thirdTabPosition.Y = y;
            fourthTabPosition.Y = y;
        }
    }
}
