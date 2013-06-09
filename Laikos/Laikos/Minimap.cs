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
        private RenderTarget2D minimap;
        public static Texture2D miniMap;
        private Effect GBuffer;
        private Terrain terrain;
        private GraphicsDevice device;

        public Minimap(GraphicsDevice device, Terrain terrain, ContentManager content)
        {
            PresentationParameters pp = device.PresentationParameters;
            minimap = new RenderTarget2D(device, 600, 600, false, SurfaceFormat.Color, DepthFormat.Depth24);
            this.device = device;
            this.terrain = terrain;
            GBuffer = content.Load<Effect>("Effects/GBuffer");
            miniMap = content.Load<Texture2D>("Models/Terrain/minimap");
        }

        public void CreateMiniMap()
        {
            device.SetRenderTarget(minimap);
            //device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            terrain.DrawTerrain(GBuffer);
            device.SetRenderTarget(null);
            miniMap = minimap;
            FileStream stream = new FileStream("minimap.jpg", FileMode.OpenOrCreate);
            miniMap.SaveAsPng(stream, miniMap.Width, miniMap.Height);
            stream.Close();
            Camera.upDownRot = MathHelper.ToRadians(-45);
            Camera.cameraPosition = new Vector3(30, 80, 100);
        }
    }
}
