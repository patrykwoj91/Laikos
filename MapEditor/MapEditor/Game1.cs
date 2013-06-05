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

using System.IO;
//using System.Windows.Forms;

using MyDataTypes;
using MyDataTypes.Serialization;

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

        //Editor variables.
        byte pencilSize = 3;
        byte pencilPower = 3;

        bool oneKeyboardClickDetected = false;
        bool oneMouseClickDetected = false;

        bool showHelp = false;

        CREATION_MODE creationMode = CREATION_MODE.TERRAIN_UP;
        byte creationOption = 0;
        byte creationType = 0;

        ObjectsSchema objectsSchema;

        //End editor variables.

        Camera camera;
        Terrain terrain;

        DecorationManager decorations;
        DefferedRenderer defferedRenderer;

        List<GameObject> objects;

        Dictionary<String, UnitType> UnitTypes;

        Player player;
        //Player enemy;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 900;
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
            decorations = new DecorationManager(this, device, graphics);

            Components.Add(camera);
            Components.Add(terrain);
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
            font = Content.Load<SpriteFont>("Georgia");
            defferedRenderer = new DefferedRenderer(device, Content, spriteBatch, font);
            objects = new List<GameObject>();
            UnitTypes = Content.Load<UnitType[]>("UnitTypes").ToDictionary(t => t.name);

            player = new Player(this, UnitTypes);

            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));
            //System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Zielu\Documents\GitHub\Laikos\MapEditor\MapEditor\Serialization\ObjectsSchema.xml");
            objectsSchema = Content.Load<ObjectsSchema>("ObjectsSchema");//(ObjectsSchema)reader.Deserialize(file);
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
            EventManager.Update();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            //Keyboard click.
            KeyboardState keyboardState = Keyboard.GetState();

            //Multiclick.
            if (creationMode == CREATION_MODE.TERRAIN_UP || creationMode == CREATION_MODE.TERRAIN_DOWN)
            {
                if (keyboardState.IsKeyDown(Keys.OemPlus) || keyboardState.IsKeyDown(Keys.Add))
                {
                    if (pencilSize < 50)
                    {
                        pencilSize += 1;
                    }
                }
                else if (keyboardState.IsKeyDown(Keys.OemMinus) || keyboardState.IsKeyDown(Keys.Subtract))
                {
                    if (pencilSize > 1)
                    {
                        pencilSize -= 1;
                    }
                }
            }

            if (keyboardState.IsKeyDown(Keys.D1))
            {
                creationOption = 1;
            }
            else if (keyboardState.IsKeyDown(Keys.D2))
            {
                creationOption = 2;
            }
            else if (keyboardState.IsKeyDown(Keys.D3))
            {
                creationOption = 3;
            }
            else if (keyboardState.IsKeyDown(Keys.D4))
            {
                creationOption = 4;
            }
            else if (keyboardState.IsKeyDown(Keys.D5))
            {
                creationOption = 5;
            }
            else if (keyboardState.IsKeyDown(Keys.D6))
            {
                creationOption = 6;
            }
            else if (keyboardState.IsKeyDown(Keys.D7))
            {
                creationOption = 7;
            }
            else if (keyboardState.IsKeyDown(Keys.D8))
            {
                creationOption = 8;
            }

            //OneClick.
            if (!oneKeyboardClickDetected)
            {
                oneKeyboardClickDetected = true;

                if (keyboardState.IsKeyDown(Keys.Delete))
                {
                    if (creationMode == CREATION_MODE.UNITS_MOVE)
                    {
                        Unit unitSelected = null;
                        foreach (Unit unit in player.UnitList)
                        {
                            if (unit.selected)
                            {
                                unitSelected = unit;
                                break;
                            }
                        }

                        if (unitSelected != null)
                        {
                            player.UnitList.Remove(unitSelected);
                        }
                    }
                }
                if (keyboardState.IsKeyDown(Keys.D0))
                {

                    ++creationMode;

                    if ((creationMode == CREATION_MODE.TERRAIN_DOWN) || (creationMode == CREATION_MODE.BUILDING_MOVE) || (creationMode == CREATION_MODE.UNITS_MOVE))
                    {
                        ++creationMode;
                    }

                    if ((byte)creationMode >= (Enum.GetNames(typeof(CREATION_MODE)).Length - 1))
                    {
                        creationMode = 0;
                    }

                    Console.Out.WriteLine(creationMode.ToString());
                }
                else if (keyboardState.IsKeyDown(Keys.F1))
                {
                    showHelp = !showHelp;
                }
                else if (keyboardState.IsKeyDown(Keys.F2))
                {
                    String directoryName = @"C:\Users\Zielu\Documents\GitHub\Mapa\";

                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    terrain.TerrainMap.SaveAsJpeg(File.Open(directoryName + "HighMap.jpg", FileMode.Create), terrain.TerrainMap.Width, terrain.TerrainMap.Height);

                    ObjectsSchema schemat = new ObjectsSchema();
                    System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));

                    schemat.unitGroups.Add(new UnitGroup("Units"));

                    foreach (Unit unit in player.UnitList)
                    {
                        schemat.unitGroups[0].units.Add(new UnitSchema(unit.type.name, unit.Position.X, unit.Position.Z));
                    }

                    System.IO.StreamWriter file = new System.IO.StreamWriter(directoryName + "Objects.xml");
                    writer.Serialize(file, schemat);
                    file.Close();

                    Console.Out.WriteLine("Zapisano mapê w lokalizacji: " + directoryName);
                }
                else if (keyboardState.IsKeyDown(Keys.F3))
                {
                    ObjectsSchema tmp = new ObjectsSchema();
                    System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));
                    System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Zielu\Documents\GitHub\Mapa\Objects.xml");
                    tmp = (ObjectsSchema)reader.Deserialize(file);

                    foreach (UnitSchema unit in tmp.unitGroups[0].units)
                    {
                        player.UnitList.Add(new Unit(player.game, UnitTypes[unit.name], new Vector3(unit.x, 30, unit.y), 0.05f));
                    }

                    Console.Out.WriteLine("Odczytano mapê.");
                }
                else if (keyboardState.IsKeyDown(Keys.F11))
                {
                    #region Stworzenie schematu
                    //ObjectsSchema schemat = new ObjectsSchema();
                    //schemat.unitGroups.Add(new UnitGroup("Demons"));
                    //schemat.unitGroups[0].units.Add(new UnitSchema ("Alien"));
                    //schemat.unitGroups[0].units.Add(new UnitSchema ("Unit_2"));
                    //schemat.unitGroups[0].units.Add(new UnitSchema ("Unit_3"));
                    //schemat.unitGroups[0].units.Add(new UnitSchema ("Unit_4"));
                    //schemat.unitGroups[0].units.Add(new UnitSchema ("Unit_5"));
                    //schemat.unitGroups.Add(new UnitGroup("Angels"));
                    //schemat.unitGroups[1].units.Add(new UnitSchema ("Reconnaissance Eye"));
                    //schemat.unitGroups[1].units.Add(new UnitSchema ("Antigravity Tank"));
                    //schemat.unitGroups[1].units.Add(new UnitSchema ("Unit_3"));
                    //schemat.unitGroups[1].units.Add(new UnitSchema ("Unit_4"));
                    //schemat.unitGroups[1].units.Add(new UnitSchema ("Unit_5"));
                    //schemat.unitGroups.Add(new UnitGroup("The Oldest"));
                    //schemat.unitGroups[2].units.Add(new UnitSchema ("Unit_1"));
                    //schemat.unitGroups[2].units.Add(new UnitSchema ("Unit_2"));
                    //schemat.unitGroups[2].units.Add(new UnitSchema ("Unit_3"));
                    //schemat.unitGroups[2].units.Add(new UnitSchema ("Unit_4"));
                    //schemat.unitGroups[2].units.Add(new UnitSchema ("Unit_5"));

                    //schemat.buildingsGroups.Add(new BuildingGroup("Demons"));
                    //schemat.buildingsGroups[0].buildings.Add(new BuildingSchema("Building_1"));
                    //schemat.buildingsGroups[0].buildings.Add(new BuildingSchema("Building_2"));
                    //schemat.buildingsGroups[0].buildings.Add(new BuildingSchema("Building_3"));
                    //schemat.buildingsGroups[0].buildings.Add(new BuildingSchema("Building_4"));
                    //schemat.buildingsGroups[0].buildings.Add(new BuildingSchema("Building_5"));
                    //schemat.buildingsGroups.Add(new BuildingGroup("Angels"));
                    //schemat.buildingsGroups[1].buildings.Add(new BuildingSchema("Building_1"));
                    //schemat.buildingsGroups[1].buildings.Add(new BuildingSchema("Building_2"));
                    //schemat.buildingsGroups[1].buildings.Add(new BuildingSchema("Building_3"));
                    //schemat.buildingsGroups[1].buildings.Add(new BuildingSchema("Building_4"));
                    //schemat.buildingsGroups[1].buildings.Add(new BuildingSchema("Building_5"));
                    //schemat.buildingsGroups.Add(new BuildingGroup("The Oldest"));
                    //schemat.buildingsGroups[2].buildings.Add(new BuildingSchema("Building_1"));
                    //schemat.buildingsGroups[2].buildings.Add(new BuildingSchema("Building_2"));
                    //schemat.buildingsGroups[2].buildings.Add(new BuildingSchema("Building_3"));
                    //schemat.buildingsGroups[2].buildings.Add(new BuildingSchema("Building_4"));
                    //schemat.buildingsGroups[2].buildings.Add(new BuildingSchema("Building_5"));

                    //System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));

                    //System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Zielu\Desktop\SerializationOverview.xml");
                    //writer.Serialize(file, schemat);
                    //file.Close();
                    #endregion Stworzenie schematu
                }
                else if (keyboardState.IsKeyDown(Keys.D8) && ((creationMode == CREATION_MODE.BUILDING_BUILD) || (creationMode == CREATION_MODE.UNITS_BUILD)))
                {
                    ++creationType;
                    if (creationType > 2)
                    {
                        creationType = 0;
                    }
                }
                else if (keyboardState.IsKeyDown(Keys.D9))
                {
                    switch (creationMode)
                    {
                        case CREATION_MODE.TERRAIN_UP:
                            creationMode = CREATION_MODE.TERRAIN_DOWN;
                            break;
                        case CREATION_MODE.TERRAIN_DOWN:
                            creationMode = CREATION_MODE.TERRAIN_UP;
                            break;
                        case CREATION_MODE.BUILDING_BUILD:
                            creationMode = CREATION_MODE.BUILDING_MOVE;
                            break;
                        case CREATION_MODE.BUILDING_MOVE:
                            creationMode = CREATION_MODE.BUILDING_BUILD;
                            break;
                        case CREATION_MODE.UNITS_BUILD:
                            creationMode = CREATION_MODE.UNITS_MOVE;
                            break;
                        case CREATION_MODE.UNITS_MOVE:
                            creationMode = CREATION_MODE.UNITS_BUILD;
                            break;
                    }
                }
            }
            else if
                (
                    (keyboardState.IsKeyUp(Keys.Delete)) &&
                    (keyboardState.IsKeyUp(Keys.D0)) &&
                    (keyboardState.IsKeyUp(Keys.D1)) &&
                    (keyboardState.IsKeyUp(Keys.D2)) &&
                    (keyboardState.IsKeyUp(Keys.D3)) &&
                    (keyboardState.IsKeyUp(Keys.D4)) &&
                    (keyboardState.IsKeyUp(Keys.D5)) &&
                    (keyboardState.IsKeyUp(Keys.D6)) &&
                    (keyboardState.IsKeyUp(Keys.D7)) &&
                    (keyboardState.IsKeyUp(Keys.D8)) &&
                    (keyboardState.IsKeyUp(Keys.D9)) &&
                    (keyboardState.IsKeyUp(Keys.F1)) &&
                    (keyboardState.IsKeyUp(Keys.F2)) &&
                    (keyboardState.IsKeyUp(Keys.F3))
                )
            {
                oneKeyboardClickDetected = false;
            }

            //Mouse click.
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                MouseState mouse = Mouse.GetState();
                Vector2 pointerPos = new Vector2(mouse.X, mouse.Y);

                Ray pointerRay = Collisions.GetPointerRay(pointerPos, device);
                Ray clippedRay = Collisions.ClipRay(pointerRay, 60, 0);
                Ray shorterRay = Collisions.LinearSearch(clippedRay);
                pointerPosition = Collisions.BinarySearch(shorterRay);

                Color[] data = new Color[terrain.TerrainMap.Height * terrain.TerrainMap.Width];
                terrain.TerrainMap.GetData<Color>(data);

                Int32 positionHeight = (int)pointerPosition.Z;
                Int32 positionWidth = (int)pointerPosition.X;

                if (creationMode == CREATION_MODE.TERRAIN_UP || creationMode == CREATION_MODE.TERRAIN_DOWN)
                {
                    Random rand = new Random();

                    for (int i = 0; i < pencilSize; ++i)
                    {
                        for (int j = 0; j < pencilSize; ++j)
                        {
                            if (((positionHeight - pencilSize / 2 + i) > 0) && ((positionWidth - pencilSize / 2 + j) > 0))
                            {
                                //Not all points will be change.
                                if (rand.Next(101) < 95)
                                {
                                    if (creationMode == CREATION_MODE.TERRAIN_UP)
                                    {
                                        data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].R += pencilPower;
                                        data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].G += pencilPower;
                                        data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].B += pencilPower;
                                    }
                                    else
                                    {
                                        data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].R -= pencilPower;
                                        data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].G -= pencilPower;
                                        data[(positionHeight - pencilSize / 2 + i) * terrain.TerrainMap.Height + (positionWidth - pencilSize / 2 + j)].B -= pencilPower;
                                    }
                                }
                            }
                        }
                    }

                    GraphicsDevice.Textures[0] = null;
                    terrain.TerrainMap.SetData<Color>(data);

                    terrain.ReloadTerrainMap();
                }
                else if (creationMode == CREATION_MODE.UNITS_BUILD)
                {
                    if (!oneMouseClickDetected)
                    {
                        oneMouseClickDetected = true;

                        if ((creationOption < 6) && (!objectsSchema.unitGroups[creationType].units[creationOption - 1].name.StartsWith("Unit_")))
                        {
                            player.UnitList.Add(new Unit(player.game, UnitTypes[objectsSchema.unitGroups[creationType].units[creationOption - 1].name], new Vector3(positionWidth, 30, positionHeight), 0.05f));
                        }
                    }
                }
                else if (creationMode == CREATION_MODE.UNITS_MOVE)
                {
                    Unit unitSelected = null;
                    foreach (Unit unit in player.UnitList)
                    {
                        if (unit.selected)
                        {
                            unitSelected = unit;
                            break;
                        }
                    }

                    for (int i = 0; i < player.UnitList.Count; i++)
                    {
                        bool selected = Collisions.RayModelCollision(clippedRay, player.UnitList[i].currentModel.Model, player.UnitList[i].GetWorldMatrix());
                        if (selected)
                        {
                            if ((player.UnitList[i] is Unit) && !player.UnitList[i].selected)
                            {
                                foreach (Unit unit in player.UnitList)
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, unit, null));

                                EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, player.UnitList[i], null));
                                break;
                            }
                        }
                        if ((!selected) && (unitSelected != null))
                        {
                            unitSelected.Position.Z = pointerPosition.Z;
                            unitSelected.Position.X = pointerPosition.X;
                        }
                    }
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released)
            {
                oneMouseClickDetected = false;
            }

            Input.Update(gameTime, device, camera, player.UnitList, decorations.DecorationList);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            objects.AddRange(player.UnitList);
            objects.AddRange(decorations.DecorationList);

            defferedRenderer.Draw(objects, terrain, gameTime);
            objects.Clear();

            base.Draw(gameTime);

            DrawMiniMap();
            DrawMenu();

            if (showHelp)
            {
                DrawHelp();
            }
        }

        /// <summary>
        /// Drow minimap in left-top corner.
        /// </summary>
        private void DrawMiniMap()
        {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(terrain.TerrainMap, new Rectangle(0, 0, 128, 128), Color.White);
            spriteBatch.End();
        }

        private void DrawMenu()
        {
            int positionOfText = 130;
            const int heightOfText = 20;

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            switch (creationMode)
            {
                case CREATION_MODE.TERRAIN_UP:
                    spriteBatch.DrawString(font, "Podnoszenie terenu", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "Rozmiar pedzla: " + pencilSize, new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "Moc pedzla: " + pencilPower, new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "9. Obnizanie terenu.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.TERRAIN_DOWN:
                    spriteBatch.DrawString(font, "Obnizanie terenu", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "Rozmiar pedzla: " + pencilSize, new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "Moc pedzla: " + pencilPower, new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "9. Podnoszenie terenu.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.BUILDING_BUILD:
                    spriteBatch.DrawString(font, "Budynki: " + objectsSchema.buildingsGroups[creationType].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    for (int i = 0; i < objectsSchema.buildingsGroups[creationType].buildings.Count; ++i)
                    {
                        spriteBatch.DrawString(font, (i + 1) + ". " + objectsSchema.buildingsGroups[creationType].buildings[i].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    }
                    spriteBatch.DrawString(font, "8. Zmiana grupy.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "9. Przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.BUILDING_MOVE:
                    spriteBatch.DrawString(font, "Budynki przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "9. Budowanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                    break;
                case CREATION_MODE.UNITS_BUILD:
                    spriteBatch.DrawString(font, "Jednostki: " + objectsSchema.unitGroups[creationType].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    for (int i = 0; i < objectsSchema.unitGroups[creationType].units.Count; ++i)
                    {
                        spriteBatch.DrawString(font, (i + 1) + ". " + objectsSchema.unitGroups[creationType].units[i].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    }
                    spriteBatch.DrawString(font, "8. Zmiana grupy.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "9. Przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.UNITS_MOVE:
                    spriteBatch.DrawString(font, "Jednostki przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "9. Budowanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(font, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
            }
            spriteBatch.End();
        }

        private void DrawHelp()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, "F2 - zapisz bitmape" + pencilSize, new Vector2(0, 215), Color.Green);
            spriteBatch.End();
        }
    }
}
