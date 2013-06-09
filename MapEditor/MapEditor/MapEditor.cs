//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;

//using System.IO;

//namespace MapEditor
//{
//    /// <summary>
//    /// This is the main type for your game
//    /// </summary>
//    public class MapEditor : Microsoft.Xna.Framework.Game
//    {
//        GraphicsDeviceManager graphics;
//        GraphicsDevice device;

//        SpriteBatch spriteBatch;
//        SpriteFont spriteFont;

//        Effect effect;
//        Camera camera;
//        Terrain terrain;

//        Vector3 pointerPosition = new Vector3(0, 0, 0);

//        UInt32 pencilSize = 3;
//        bool oneClickDetected = false;

//        bool showHelp = false;


//        public MapEditor()
//        {
//            graphics = new GraphicsDeviceManager(this);

//            Content.RootDirectory = "Content";

//            graphics.PreferredBackBufferWidth = 800;
//            graphics.PreferredBackBufferHeight = 600;
//            graphics.IsFullScreen = false;
//        }

//        /// <summary>
//        /// Allows the game to perform any initialization it needs to before starting to run.
//        /// This is where it can query for any required services and load any non-graphic
//        /// related content.  Calling base.Initialize will enumerate through any components
//        /// and initialize them as well.
//        /// </summary>
//        protected override void Initialize()
//        {
//            device = graphics.GraphicsDevice;
//            this.IsMouseVisible = true;

//            terrain = new Terrain(this);
//            camera = new Camera(this, graphics);

//            Components.Add(camera);
//            Components.Add(terrain);

//            base.Initialize();
//        }

//        /// <summary>
//        /// LoadContent will be called once per game and is the place to load
//        /// all of your content.
//        /// </summary>
//        protected override void LoadContent()
//        {
//            spriteBatch = new SpriteBatch(GraphicsDevice);
//            spriteFont = Content.Load<SpriteFont>("TextFont");

//            effect = Content.Load<Effect>("effects");
//        }

//        /// <summary>
//        /// UnloadContent will be called once per game and is the place to unload
//        /// all content.
//        /// </summary>
//        protected override void UnloadContent()
//        {
//            // TODO: Unload any non ContentManager content here
//        }

//        /// <summary>
//        /// Allows the game to run logic such as updating the world,
//        /// checking for collisions, gathering input, and playing audio.
//        /// </summary>
//        /// <param name="gameTime">Provides a snapshot of timing values.</param>
//        protected override void Update(GameTime gameTime)
//        {
//            // Allows the game to exit
//            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
//                this.Exit();

//            KeyboardState keyboardState = Keyboard.GetState();

//            if (!oneClickDetected)
//            {
//                if (keyboardState.IsKeyDown(Keys.OemPlus) || keyboardState.IsKeyDown(Keys.Add))
//                {
//                    pencilSize += 1;
//                }
//                else if (keyboardState.IsKeyDown(Keys.OemMinus) || keyboardState.IsKeyDown(Keys.Subtract))
//                {
//                    pencilSize -= 1;
//                }
//                else if (keyboardState.IsKeyDown(Keys.F2))
//                {
//                    terrain.TerrainMap.SaveAsJpeg(File.Open("nowaMapa.jpg", FileMode.Create), terrain.TerrainMap.Width, terrain.TerrainMap.Height);
//                }

//                oneClickDetected = true;
//            }
//            else if (keyboardState.IsKeyUp(Keys.OemPlus) && keyboardState.IsKeyUp(Keys.Add) && keyboardState.IsKeyUp(Keys.OemMinus) && keyboardState.IsKeyUp(Keys.Subtract))
//            {
//                oneClickDetected = false;
//            }

//            MouseState mouseState = Mouse.GetState();

//            if (mouseState.LeftButton == ButtonState.Pressed)
//            {
//                MouseState mouse = Mouse.GetState();
//                Vector2 pointerPos = new Vector2(mouse.X, mouse.Y);
//                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
//                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);
//                Ray shorterRay = Collisions.LinearSearch(clippedRay);
//                pointerPosition = Collisions.BinarySearch(shorterRay);

//                Color[] data = new Color[terrain.TerrainMap.Height * terrain.TerrainMap.Width];
//                terrain.TerrainMap.GetData<Color>(data);

//                Int32 positionHeight = (int)pointerPosition.Z;
//                Int32 positionWidth = (int)pointerPosition.X;

//                for (int i = 0; i < pencilSize; ++i)
//                {
//                    for (int j = 0; j < pencilSize; ++j)
//                    {
//                        if (((positionHeight - pencilSize / 2 + i) > 0) && ((positionWidth - pencilSize / 2 + j) > 0))
//                        {
//                            data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].R += 5;
//                            data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].G += 5;
//                            data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].B += 5;
//                        }
//                    }
//                }

//                Console.Out.WriteLine(data[(positionHeight) * terrain.TerrainMap.Height + (positionWidth)]);

//                GraphicsDevice.Textures[0] = null;
//                terrain.TerrainMap.SetData<Color>(data);

//                terrain.reloadTerrainMap();

//                Console.WriteLine(pointerPosition);
//            }

//            base.Update(gameTime);
//        }

//        /// <summary>
//        /// This is called when the game should draw itself.
//        /// </summary>
//        /// <param name="gameTime">Provides a snapshot of timing values.</param>
//        protected override void Draw(GameTime gameTime)
//        {
//            GraphicsDevice.Clear(Color.Black);

//            effect.Parameters["xView"].SetValue(Camera.viewMatrix);
//            effect.Parameters["xProjection"].SetValue(Camera.projectionMatrix);
//            effect.Parameters["xWorld"].SetValue(terrain.SetWorldMatrix());

//            base.Draw(gameTime);

//            DrawMiniMap();

//            if (showHelp)
//            {
//                DrawHelp();
//            }
//        }

//        /// <summary>
//        /// Drow minimap in left-top corner.
//        /// </summary>
//        private void DrawMiniMap()
//        {
//            spriteBatch.Begin();
//            spriteBatch.Draw(terrain.TerrainMap, new Rectangle(0, 0, 200, 200), Color.White);
//            spriteBatch.DrawString(spriteFont, "Rozmiar pêdzla: " + pencilSize, new Vector2(0, 205), Color.White);
//            spriteBatch.End();
//        }

//        private void DrawHelp()
//        {
//            spriteBatch.Begin();
//            spriteBatch.DrawString(spriteFont, "F2 - zapisz bitmapê" + pencilSize, new Vector2(0, 215), Color.Green);
//            spriteBatch.End();
//        }
//    }
//}
