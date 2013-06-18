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
        public static int iconWidth;
        public static int iconHeight;
        private static Player player;

        public static void Initialize(ContentManager content, Player Player)
        {
            width = 100;
            height = 50;
            iconHeight = 32;
            iconWidth = 32;
            player = Player;
            sourcesButton = content.Load<Texture2D>("GUI/button");
            sourcesIcon = content.Load<Texture2D>("GUI/soul");
            font = content.Load<SpriteFont>("Georgia");
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            position = new Rectangle(GUI.screenWidth - 2 * width, 0, width, height);
            Rectangle iconPosition = new Rectangle(GUI.screenWidth - 2 * width + 5, height / 4, iconWidth, iconHeight);
            Vector2 stringPosition = new Vector2(GUI.screenWidth - 2 * width + iconWidth + 10, height / 4);
            DrawButton(spriteBatch, position, player.Souls, iconPosition, stringPosition);
        }

        public static void DrawButton(SpriteBatch spriteBatch, Rectangle buttonPosition, int souls, Rectangle iconPosition, Vector2 stringPosition)
        {
            spriteBatch.Draw(sourcesButton, buttonPosition, Color.White);
            spriteBatch.Draw(sourcesIcon, iconPosition, Color.White);
            spriteBatch.DrawString(font, souls.ToString(), stringPosition, Color.White);
        }
    }
}
