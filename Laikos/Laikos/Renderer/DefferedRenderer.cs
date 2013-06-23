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
    class DefferedRenderer
    {
        #region Variables
        private FullscreenQuad fsq;
        private GraphicsDevice device;
        private LightManager lights;
        private Water water;

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
        private bool minimap = true;
        public static bool debug = false;
        public static float lightIntensity;

        private Menu menu;
        public static ParticleSystem explosionParticles;
        public static ParticleSystem explosionSmokeParticles;
        public static ParticleSystem SmokePlumeParticles;
        #endregion

        public DefferedRenderer(GraphicsDevice device, ContentManager content, SpriteBatch spriteBatch, SpriteFont font, Game game)
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
            lightIntensity = 0.5f;

            colorRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth24);
            normalRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);
            depthRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Single, DepthFormat.None);
            lightRT = new RenderTarget2D(device, backbufferWidth, backbufferHeight, false, SurfaceFormat.Color, DepthFormat.None);

            explosionParticles = new ParticleSystem(game, content, "ExplosionSettings");
            explosionSmokeParticles = new ParticleSystem(game, content, "ExplosionSmokeSettings");
            SmokePlumeParticles = new ParticleSystem(game, content, "SmokePlumeSettings");
            menu = new Menu(spriteBatch, content, device.PresentationParameters);
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
            water = new Water(device, content, GBuffer);
            explosionSmokeParticles.LoadContent(device);
            explosionParticles.LoadContent(device);
            SmokePlumeParticles.LoadContent(device);
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
        void RenderSceneTo3Targets(List<GameObject> objects, Terrain terrain, float time)
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
                else if (obj is Building)
                {
                    Building building = (Building)obj;
                    building.currentModel.Draw(device, building.GetWorldMatrix(), GBuffer, normals, speculars, false);
                    //building.currentModel.Model.Draw(building.GetWorldMatrix(), Camera.viewMatrix, Camera.projectionMatrix);
                }
                else if (obj is Decoration)
                {
                    Decoration decoration = (Decoration)obj;
                    decoration.currentModel.Draw(device, decoration.GetWorldMatrix(), GBuffer, normals, speculars, false);
                    //decoration.currentModel.Model.Draw(decoration.GetWorldMatrix(), Camera.viewMatrix, Camera.projectionMatrix);
                }
            }
            water.DrawSkyDome(Camera.viewMatrix);
            terrain.DrawTerrain(GBuffer);
            water.DrawWater(time);
            device.SetRenderTarget(null);
            /*if (minimap)
            {
                Minimap.SetRenderTarget(device);
                terrain.DrawTerrain(GBuffer);
                water.DrawWater(time);
                Minimap.ResolveRenderTarger(device);
                Minimap.SaveMiniMap();
                minimap = false;
            }*/
        }

        public void Draw(List<GameObject> objects, Terrain terrain, GameTime GameTime)
        {
            if (!menu.inMenu)
            {
                List<Model> models = new List<Model>();
                foreach (GameObject obj in objects)
                    models.Add(obj.currentModel.Model);
                float time = (float)GameTime.TotalGameTime.TotalMilliseconds / 100.0f;
                float waterTime = (float)GameTime.TotalGameTime.TotalMilliseconds / 300.0f;
                water.DrawRefractionMap(terrain, objects, normals, speculars);
                water.DrawReflectionMap(terrain, objects, normals, speculars);
                gameTime = GameTime;
                CreateLights(objects);
                SetGBuffer();
                ClearGBuffer();
                RenderSceneTo3Targets(objects, terrain, waterTime);
                ResolveGBuffer();
                lights.CreateShadowMap(objects, terrain);
                DrawLights(objects);
                explosionParticles.Draw(gameTime, device);
                explosionSmokeParticles.Draw(gameTime, device);
              /*  foreach (GameObject obj in objects)
                {
                    if (obj is Decoration)
                    {
                        Decoration decoration = (Decoration)obj;
                        //decoration.currentModel.Draw(device, decoration.GetWorldMatrix(), GBuffer, normals, speculars, false);
                        //decoration.currentModel.Model.Draw(decoration.GetWorldMatrix(), Camera.viewMatrix, Camera.projectionMatrix);
                    }
                    if (obj is Building)
                    {
                        Building building = (Building)obj;
                        //building.currentModel.Draw(device, building.GetWorldMatrix(), GBuffer, normals, speculars, false);
                        //building.currentModel.Model.Draw(building.GetWorldMatrix(), Camera.viewMatrix, Camera.projectionMatrix);
                    }
                }*/
                GUI.Draw();
                GUI.Update(gameTime);
                
            }
            else
            menu.Draw();
            menu.Update();
            //Debug();
        }

        private void Debug()
        {
            //Begin SpriteBatch
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);

            spriteBatch.DrawString(font, "FPS: " + (1000 / (gameTime.ElapsedGameTime.Milliseconds > 0 ? gameTime.ElapsedGameTime.Milliseconds : 1000)), new Vector2(10.0f, 20.0f), Color.White);

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


            //shadowMap = lights.getSpotLights()[0].shadowMap;
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

            lights.AddLight(new DirectionalLight(Vector3.Down, Color.White, lightIntensity));

           /* foreach (GameObject obj in objects)
            {
                if (obj is Unit)
                {
                    Vector3 lightPosition = new Vector3(obj.Position.X, obj.Position.Y + 20, obj.Position.Z);
                    //lights.AddLight(new PointLight(lightPosition, Color.White, 50, 1, false, 1));
                    //lights.AddLight(new SpotLight(lightPosition, Vector3.Down, Color.White, 0.5f, true, 64));
                }
            }*/
        }
    }
}
