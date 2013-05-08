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
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Effect effect;
        Camera camera;
        Terrain terrain;
        UnitManager units;
        DecorationManager decorations;
        List<GameObject> objects;
        DefferedRenderer defferedRenderer;
        private SpriteFont font;

        Vector3 pointerPosition = new Vector3(0, 0, 0);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            objects = new List<GameObject>();
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.IsFullScreen = false;
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

            terrain = new Terrain(this);
            camera = new Camera(this, graphics);
            units = new UnitManager(this, device, graphics);
            decorations = new DecorationManager(this, device, graphics);

            Components.Add(camera);
            Components.Add(terrain);
            Components.Add(units);
            Components.Add(decorations);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            effect = Content.Load<Effect>("effects");
            font = Content.Load<SpriteFont>("Georgia");
            defferedRenderer = new DefferedRenderer(device, Content, spriteBatch, font);
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
            EventManager.Update();
            // Allows the game to exit
          if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            
            bool collision;


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


            Input.Update(gameTime, device, camera, units.UnitList,decorations.DecorationList);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //RasterizerState rs = new RasterizerState();
            //rs.CullMode = CullMode.None;
            //device.RasterizerState = rs;

            GraphicsDevice.Clear(Color.Black);
            objects.AddRange(units.UnitList);
            objects.AddRange(decorations.DecorationList);

            defferedRenderer.Draw(objects, terrain, gameTime);
            objects.Clear();
            base.Draw(gameTime);

            
        }
    }
}
