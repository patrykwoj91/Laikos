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
    static class SourcesButton
    {
        private static Texture2D sourcesButton;
        private static Texture2D sourcesIcon;
        private static SpriteFont font;
        private static Rectangle position;
        public static int width;
        public static int height;
        private static int iconWidth;
        private static int iconHeight;
        private static int souls;

        public static void Initialize(ContentManager content, int Souls)
        {
            width = 100;
            height = 50;
            iconHeight = 32;
            iconWidth = 32;
            souls = Souls;
            sourcesButton = content.Load<Texture2D>("GUI/button");
            sourcesIcon = content.Load<Texture2D>("GUI/soul");
            font = content.Load<SpriteFont>("Georgia");
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            position = new Rectangle(GUI.screenWidth - 2 * width, 0, width, height);
            spriteBatch.Draw(sourcesButton, position, Color.White);
            position = new Rectangle(GUI.screenWidth - 2 * width + 5, height / 4, iconWidth, iconHeight);
            spriteBatch.Draw(sourcesIcon, position, Color.White);
            spriteBatch.DrawString(font, souls.ToString(), new Vector2(GUI.screenWidth - 2 * width + iconWidth + 10, height / 4), Color.White);
        }
    }
}
