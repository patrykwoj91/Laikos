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
        

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = false;

            terrain = new Terrain(this);
            camera = new Camera(this, graphics);
            units = new UnitManager(this);
            decorations = new DecorationManager(this);
            
            Components.Add(camera);
            Components.Add(terrain);
            Components.Add(units);
            Components.Add(decorations);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;

            effect = Content.Load<Effect>("effects");
            
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            KeyboardState kb = Keyboard.GetState();
        
            if (kb.IsKeyDown(Keys.K))
            {
                MouseState mouse = Mouse.GetState();
                Vector2 pointerPos = new Vector2(mouse.X, mouse.Y);
                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);
                Ray shorterRay = Collisions.LinearSearch(clippedRay);
                Vector3 pointerPosCol = Collisions.BinarySearch(shorterRay);
                Console.WriteLine(pointerPosCol.ToString());
            }

            if (kb.IsKeyDown(Keys.J))
            {
                bool collision;
                collision = Collisions.GeneralDecorationCollisionCheck(units.UnitList[0].currentModel, units.UnitList[0].GetWorldMatrix(),
                                                  decorations.DecorationList[0].currentModel, decorations.DecorationList[0].GetWorldMatrix());
                Console.WriteLine(collision);
            }

            if (kb.IsKeyDown(Keys.L))
            {
                MouseState mouse = Mouse.GetState();
                Vector2 pointerPos = new Vector2(mouse.X, mouse.Y);
                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);
                bool collision = Collisions.RayModelCollision(clippedRay, units.UnitList[0].currentModel, units.UnitList[0].GetWorldMatrix());
                if (collision == false)
                    Console.WriteLine("Brak kolizji");
                else
                    Console.WriteLine("Kolizja");
            }

            
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            effect.Parameters["xView"].SetValue(Camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(Camera.projectionMatrix);
            effect.Parameters["xWorld"].SetValue(terrain.SetWorldMatrix());
          
            base.Draw(gameTime);

            
        }
    }
}
