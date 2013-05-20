using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Animation;

namespace Laikos
{
    class DefferedRenderer
    {
        #region Variables
        private FullscreenQuad fsq;
        private GraphicsDevice device;
        private LightManager lights;

        private RenderTarget2D colorRT;
        private RenderTarget2D normalRT;
        private RenderTarget2D depthRT;
        private RenderTarget2D lightRT;
        private RenderTarget2D shadowMap;

        private Effect clearBuffer;
        private Effect directionalLight;
        private Effect pointLightEffect;
        private Effect spotLightEffect;
        private Effect finalComposition;
        private Effect spotLight;
        private Effect GBuffer;

        private SpriteBatch spriteBatch;
        private Model sphereModel;
        private Model spotLightGeometry;
        private Texture2D normals;
        private Texture2D speculars;
        private Texture2D spotCookie;

        private Vector2 halfPixel;
        private GameTime gameTime;
        private SpriteFont font;
        public static bool debug = true;
        #endregion

        public DefferedRenderer(GraphicsDevice device, ContentManager content, SpriteBatch spriteBatch, SpriteFont font)
        {
            #region Initialize Variables
            this.device = device;
            this.spriteBatch = spriteBatch;
            this.lights = new LightManager(content, device);
            this.font = font;
            fsq = new FullscreenQuad(device);

            halfPixel = new Vector2()
            {
                X = 0.5f / (float)device.PresentationParameters.BackBufferWidth,
                Y = 0.5f / (float)device.PresentationParameters.BackBufferHeight
            };


            int backbufferWidth = device.PresentationParameters.BackBufferWidth;
            int backbufferHeight = device.PresentationParameters.BackBufferHeight;

            colorRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            depthRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            lightRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            #endregion

            #region Load Content
            clearBuffer = content.Load<Effect>("Effects/Clear");
            GBuffer = content.Load<Effect>("Effects/GBuffer");
            directionalLight = content.Load<Effect>("Effects/DirectionalLight");
            finalComposition = content.Load<Effect>("Effects/Composition");
            pointLightEffect = content.Load<Effect>("Effects/PointLight");
            sphereModel = content.Load<Model>("Models/sphere");
            normals = content.Load<Texture2D>("null_normal");
            speculars = content.Load<Texture2D>("null_specular");
            spotLight = content.Load<Effect>("Effects/SpotLight");
            spotLightGeometry = content.Load<Model>("SpotLightGeometry");
            spotCookie = content.Load<Texture2D>("SpotCookie");
            #endregion
        }

        private void SetGBuffer()
        {
            device.SetRenderTargets(colorRT, normalRT, depthRT);
        }

        private void ResolveGBuffer()
        {
            device.SetRenderTargets(null);
        }

        private void ClearGBuffer()
        {
            clearBuffer.Techniques[0].Passes[0].Apply();
            fsq.Render(Vector2.One * -1, Vector2.One);
        }
        void RenderSceneTo3Targets(List<GameObject> objects, Terrain terrain)
        {
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            foreach (GameObject obj in objects)
            {
                if (obj is Unit)
                {
                    Unit unit = (Unit)obj;
                    unit.currentModel.Draw(device, unit.GetWorldMatrix(), GBuffer, normals, speculars, false);
                }
                if (obj is Decoration)
                {
                    Decoration decoration = (Decoration)obj;
                    decoration.currentModel.Draw(device, decoration.GetWorldMatrix(), GBuffer, normals, speculars, false);
                }
            }
            terrain.DrawTerrain(GBuffer);

            device.SetRenderTarget(null);
        }

        public void Draw(List<GameObject> objects, Terrain terrain, GameTime GameTime)
        {
            gameTime = GameTime;
            CreateLights(objects);
            SetGBuffer();
            ClearGBuffer();
            RenderSceneTo3Targets(objects, terrain);
            ResolveGBuffer();
            List<Model> models = new List<Model>();
            foreach (GameObject obj in objects)
                models.Add(obj.currentModel.Model);
            lights.CreateShadowMap(objects, terrain);
            DrawLights(objects);
            if(debug)
                Debug();
            
        }

        private void Debug()
        {
            //Begin SpriteBatch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);

            //Width + Height
            int width = 128;
            int height = 128;

            //Set up Drawing Rectangle
            Rectangle rect = new Rectangle(0, 0, width, height);

            //Draw GBuffer 0
            spriteBatch.Draw((Texture2D)colorRT, rect, Color.White);

            //Draw GBuffer 1
            rect.X += width;
            spriteBatch.Draw((Texture2D)normalRT, rect, Color.White);

            //Draw GBuffer 2
            rect.X += width;
            spriteBatch.Draw((Texture2D)depthRT, rect, Color.White);

            //Draw LightMap
            rect.X += width;
            spriteBatch.Draw((Texture2D)lightRT, rect, Color.White);

            rect.X += width;
            spriteBatch.Draw((Texture2D)shadowMap, rect, Color.White);

            spriteBatch.DrawString(font, "FPS: " + (1000 / gameTime.ElapsedGameTime.Milliseconds), new Vector2(10.0f, 20.0f), Color.White);
            //End SpriteBatch
            spriteBatch.End();
        }

        private void DrawLights(List<GameObject> objects)
        {
            DirectionalLight.Initialize(directionalLight, colorRT, normalRT, depthRT, halfPixel, fsq);
            PointLight.Initialize(pointLightEffect, colorRT, normalRT, depthRT, halfPixel, fsq, device, sphereModel);
            SpotLight.Initialize(device, spotLight, spotCookie, spotLightGeometry, colorRT, normalRT, depthRT);

            device.SetRenderTarget(lightRT);
            device.Clear(Color.Transparent);
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.None;


            shadowMap = lights.getSpotLights()[0].shadowMap;
            lights.CreateLightMap();
            lights.RemoveAllLights();
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullCounterClockwise;

            device.SetRenderTarget(null);

            //Combine everything
            finalComposition.Parameters["colorMap"].SetValue(colorRT);
            finalComposition.Parameters["lightMap"].SetValue(lightRT);
            finalComposition.Parameters["halfPixel"].SetValue(halfPixel);

            finalComposition.Techniques[0].Passes[0].Apply();
            fsq.Render(Vector2.One * -1, Vector2.One);

        }

        private void CreateLights(List<GameObject> objects)
        {
            DirectionalLight.Initialize(directionalLight, colorRT, normalRT, depthRT, halfPixel, fsq);
            PointLight.Initialize(pointLightEffect, colorRT, normalRT, depthRT, halfPixel, fsq, device, sphereModel);
            SpotLight.Initialize(device, spotLight, spotCookie, spotLightGeometry, colorRT, normalRT, depthRT);

            lights.AddLight(new DirectionalLight(Vector3.Down, Color.White, 0.7f));

            foreach (GameObject obj in objects)
            {
                if (obj is Unit)
                {
                    Vector3 lightPosition = new Vector3(obj.Position.X, obj.Position.Y + 10, obj.Position.Z);
                    lights.AddLight(new PointLight(lightPosition, Color.White, 50, 1, false, 1));
                    lights.AddLight(new SpotLight(lightPosition, Vector3.Down, Color.White, 0.5f, true, 512));
                }
            }
        }

    }
}
