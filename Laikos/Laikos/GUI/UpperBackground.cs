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
    class UpperBackground
    {
        private static int width;
        private static int height;
        private static Texture2D upperBackground;
        private static Rectangle position;
        private static Rectangle source;

        public static void Initialize(ContentManager content)
        {
            upperBackground = content.Load<Texture2D>("GUI/desen");
            width = GUI.screenWidth - Minimap.width + 2 * Minimap.diff;
            height = 50;
            position = new Rectangle(Minimap.width + 2 * Minimap.diff, 0, width, height);
            source = new Rectangle(0, 0, upperBackground.Width, height);
        }

        public static void Create(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(upperBackground, position, source, Color.White);
        }
    }
}
