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
        ModelRenderer soldier;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = true;

            camera = new Camera(this, graphics);
            terrain = new Terrain(this);

            Components.Add(camera);
            Components.Add(terrain);
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
            
            //adds soldier_model and ask renderer to render it
            //caly ten kod bedzie potem w klasie specjalistycznej danego obiektu 
            Model soldier_model = Content.Load<Model>("Models/Test_model/dude");
            soldier = new ModelRenderer(soldier_model);
            soldier.Scale = 0.05f;
            soldier.Position = new Vector3(0,-20,0);//Move it to the centre Z - up-/down+ X:left+/right- , Y:high down +/high up -
            soldier.Rotation = new Vector3(MathHelper.ToRadians(180), 0, MathHelper.ToRadians(0));
            soldier.PlayAnimation("Take 001");//Play the default swimming animation
            //--------------------------------------------------------------------//
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
            soldier.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);
            effect.Parameters["xWorld"].SetValue(terrain.SetWorldMatrix());
            soldier.Draw(camera);
            base.Draw(gameTime);

            
        }
    }
}
