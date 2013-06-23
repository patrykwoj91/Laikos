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
        private static Texture2D defaultBackground;
        private static Texture2D cementary;
        private static Texture2D palace;
        private static Rectangle position;
        public static bool isUp;
        public static float upTime;
        public static float downTime;
        private static GifAnimation.GifAnimation animation;
        private static GifAnimation.GifAnimation tank;
        private static GifAnimation.GifAnimation droneWorker;
        private static GifAnimation.GifAnimation eye;
        public static int whichUnit;
        public static bool animated;

        public static void Initialize(ContentManager content)
        {
            width = 150;
            height = 150;
            position = new Rectangle(0, GUI.screenHeight, width, height);
            defaultBackground = content.Load<Texture2D>("GUI/tlo");
            droneWorker = content.Load<GifAnimation.GifAnimation>("GUI/units/Drone_Worker");
            tank = content.Load<GifAnimation.GifAnimation>("GUI/units/czolg");
            eye = content.Load<GifAnimation.GifAnimation>("GUI/units/Oko");
            cementary = content.Load<Texture2D>("GUI/buildings/cementary");
            palace = content.Load<Texture2D>("GUI/buildings/palac");
            isUp = true;
            upTime = 1.0f;
            downTime = 1.0f;
            animated = false;
            whichUnit = int.MaxValue;
        }

        public static void UpdateAnimation(GameTime gameTime)
        {
            if(animated)
                animation.Update(gameTime.ElapsedGameTime.Ticks * 3);
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            if (animated)
            {
                switch (whichUnit)
                {
                    case 0:
                        animation = droneWorker;
                        break;
                    case 1:
                        animation = eye;
                        break;
                }
                spriteBatch.Draw(animation.GetTexture(), position, Color.White);
            }
            else
            {
                switch (whichUnit)
                {
                    case 0:
                        unitBackground = cementary;
                        break;
                    case 1:
                        unitBackground = palace;
                        break;
                    case 2:
                        unitBackground = defaultBackground;
                        break;
                    default:
                        unitBackground = defaultBackground;
                        break;
                }
                spriteBatch.Draw(unitBackground, position, Color.White);
            }
        }

        public static void MoveUp()
        {

            upTime += 0.1f;
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
            downTime += 0.1f;
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
