using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Animation;
using System.IO;

namespace Laikos
{
    class Minimap
    {
        public static RenderTarget2D minimap;
        public static Texture2D miniMap;

        public static void Initialize(GraphicsDevice device)
        {
            PresentationParameters pp = device.PresentationParameters;
            minimap = new RenderTarget2D(device, 1000, 1000, false, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        public static void SetRenderTarget(GraphicsDevice device)
        {
            device.SetRenderTarget(minimap);
        }

        public static void ResolveRenderTarger(GraphicsDevice device)
        {
            device.SetRenderTarget(null);
            miniMap = minimap;
            Camera.upDownRot = MathHelper.ToRadians(-45);
            Camera.cameraPosition = new Vector3(30, 80, 100);
        }

        public static void LoadMiniMap(ContentManager content)
        {
            miniMap = content.Load<Texture2D>("Models/Terrain/minimap");
        }

        public static void SaveMiniMap()
        {
            FileStream stream = new FileStream("minimap.jpg", FileMode.OpenOrCreate);
            miniMap.SaveAsPng(stream, miniMap.Width, miniMap.Height);
            stream.Close();
        }
    }
}
