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
using MyDataTypes;

namespace Laikos
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Vector3 pointerPosition = new Vector3(0, 0, 0);
        Camera camera;
        Terrain terrain;
        DecorationManager decorations;
        DefferedRenderer defferedRenderer;
        List<GameObject> objects;
        Minimap minimap;
        bool noob = true;

        Dictionary<String, UnitType> UnitTypes;
        Dictionary<String, BuildingType> BuildingTypes;

        Player player;
        Player enemy;

        System.Drawing.Bitmap bitmapTmp;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
            graphics.IsFullScreen =false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            device = graphics.GraphicsDevice;
            this.IsMouseVisible = true;
            Input.Init(graphics, this);
            terrain = new Terrain(this);
            camera = new Camera(this, graphics);
            decorations = new DecorationManager(this, device, graphics);
         
            Components.Add(camera);
            Components.Add(terrain);
            Components.Add(decorations);
            minimap = new Minimap(device, terrain, Content);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Georgia");
            defferedRenderer = new DefferedRenderer(device, Content, spriteBatch, font,this);
            objects = new List<GameObject>();

            UnitTypes = Content.Load<UnitType[]>("UnitTypes").ToDictionary(t => t.name);
            BuildingTypes = Content.Load<BuildingType[]>("BuildingTypes").ToDictionary(t => t.Name);
             
            Laikos.PathFiding.Map.loadMap(Content.Load<Texture2D>("Models/Terrain/Heightmaps/heightmap4"));


            player = new Player(this, UnitTypes, BuildingTypes);
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            player.Update(gameTime);

            Input.Update(this, gameTime, device, camera, player,decorations.DecorationList);

            EventManager.Update();
            UpdateExplosions(gameTime, objects);
            UpdateExplosionSmoke(gameTime, objects);
            base.Update(gameTime);

            // TODO: Add your update logic here

            //bool collision;


            /*collision = Collisions.DetailedDecorationCollisionCheck(units.UnitList[2],
                                      decorations.DecorationList[0]);
            //Console.WriteLine(collision);
            if (collision)
                units.UnitList[2].Position = units.UnitList[2].lastPosition;

            collision = Collisions.DetailedCollisionCheck(units.UnitList[1].currentModel.Model, units.UnitList[1].GetWorldMatrix(),
                 units.UnitList[2].currentModel.Model, units.UnitList[2].GetWorldMatrix());
            
            if (collision)
            {
                units.UnitList[0].Position = units.UnitList[0].lastPosition;
                units.UnitList[1].Position = units.UnitList[1].lastPosition;
            }*/

        }

       
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            device.Clear(Color.CornflowerBlue);
            //RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //device.RasterizerState = rs;
            defferedRenderer.explosionParticles.SetCamera(Camera.viewMatrix, Camera.projectionMatrix);
            defferedRenderer.explosionSmokeParticles.SetCamera(Camera.viewMatrix, Camera.projectionMatrix);
            defferedRenderer.SmokePlumeParticles.SetCamera(Camera.viewMatrix, Camera.projectionMatrix);
                        //if (noob)
            //{

                //minimap.CreateMiniMap();
                //noob = false;
            //}
            objects.AddRange(player.UnitList);
            objects.AddRange(decorations.DecorationList);
            objects.AddRange(player.BuildingList);
            
            defferedRenderer.Draw(objects, terrain, gameTime);
            Input.Draw();
  
            objects.Clear();
            base.Draw(gameTime);

            
        }

         void UpdateExplosions(GameTime gameTime, List<GameObject> objects)
        {

            for (int i = player.UnitList.Count - 1; i >= 0; i--)
            {


                if (player.UnitList[i].HP == 0)
                    {
                        defferedRenderer.explosionParticles.AddParticle(player.UnitList[i].Position, Vector3.Zero);
                        defferedRenderer.explosionSmokeParticles.AddParticle(player.UnitList[i].Position, Vector3.Zero);
                        player.UnitList[i].HP = 10;
                    }
                
                defferedRenderer.explosionParticles.Update(gameTime);
            }
        }

        void UpdateExplosionSmoke(GameTime gameTime, List<GameObject> objects)
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] is Unit)
                {
                    Unit unit = (Unit)objects[i];
                    if (100*unit.HP/unit.maxHP <= 5)
                    {
                        defferedRenderer.SmokePlumeParticles.AddParticle(objects[i].Position, Vector3.Zero);
                    }
                }
                defferedRenderer.explosionSmokeParticles.Update(gameTime);
            }    
        }
    }
}

