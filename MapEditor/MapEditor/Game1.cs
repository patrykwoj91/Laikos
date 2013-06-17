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
        private SpriteFont Times;

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
        Dictionary<String, BuildingType> BuildingTypes;

        Player player;
        Player enemy;

        int activePlayer = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;
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
            //GenerateSchema();

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
            Times = Content.Load<SpriteFont>("Times");

            defferedRenderer = new DefferedRenderer(device, Content, spriteBatch, Times);
            objects = new List<GameObject>();

            UnitTypes = Content.Load<UnitType[]>("UnitTypes").ToDictionary(t => t.name);
            BuildingTypes = Content.Load<BuildingType[]>("BuildingTypes").ToDictionary(t => t.Name);

            player = new Player(this, UnitTypes, BuildingTypes);
            enemy = new Player(this, UnitTypes, BuildingTypes);

            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));

            objectsSchema = Content.Load<ObjectsSchema>("ObjectsSchema");
        }

        private void GenerateSchema()
        {
            ObjectsSchema schemat = new ObjectsSchema();
            schemat.unitGroups_1.Add(new UnitGroup("Demons"));
            schemat.unitGroups_1[0].units.Add(new UnitSchema("Alien Worker"));
            schemat.unitGroups_1[0].units.Add(new UnitSchema("Alien Melee"));
            schemat.unitGroups_1[0].units.Add(new UnitSchema("Alien Rider"));
            schemat.unitGroups_1[0].units.Add(new UnitSchema("Unit_4"));
            schemat.unitGroups_1[0].units.Add(new UnitSchema("Unit_5"));
            schemat.unitGroups_1.Add(new UnitGroup("Angels"));
            schemat.unitGroups_1[1].units.Add(new UnitSchema("Reconnaissance Eye"));
            schemat.unitGroups_1[1].units.Add(new UnitSchema("Antigravity Tank"));
            schemat.unitGroups_1[1].units.Add(new UnitSchema("Droid Worker"));
            schemat.unitGroups_1[1].units.Add(new UnitSchema("Unit_4"));
            schemat.unitGroups_1[1].units.Add(new UnitSchema("Unit_5"));
            schemat.unitGroups_1.Add(new UnitGroup("The Oldest"));
            schemat.unitGroups_1[2].units.Add(new UnitSchema("Unit_1"));
            schemat.unitGroups_1[2].units.Add(new UnitSchema("Unit_2"));
            schemat.unitGroups_1[2].units.Add(new UnitSchema("Unit_3"));
            schemat.unitGroups_1[2].units.Add(new UnitSchema("Unit_4"));
            schemat.unitGroups_1[2].units.Add(new UnitSchema("Unit_5"));

            schemat.buildingsGroups_1.Add(new BuildingGroup("Demons"));
            schemat.buildingsGroups_1[0].buildings.Add(new BuildingSchema("Diabelny Dwór1"));
            schemat.buildingsGroups_1[0].buildings.Add(new BuildingSchema("BJ Alienow1"));
            schemat.buildingsGroups_1[0].buildings.Add(new BuildingSchema("Nekropolis"));
            schemat.buildingsGroups_1[0].buildings.Add(new BuildingSchema("Wie¿a"));
            schemat.buildingsGroups_1[0].buildings.Add(new BuildingSchema("Building_5"));
            schemat.buildingsGroups_1.Add(new BuildingGroup("Angels"));
            schemat.buildingsGroups_1[1].buildings.Add(new BuildingSchema("Pa³ac rady2"));
            schemat.buildingsGroups_1[1].buildings.Add(new BuildingSchema("BJ Niebian1"));
            schemat.buildingsGroups_1[1].buildings.Add(new BuildingSchema("Stra¿nica"));
            schemat.buildingsGroups_1[1].buildings.Add(new BuildingSchema("Cementary"));
            schemat.buildingsGroups_1[1].buildings.Add(new BuildingSchema("Building_5"));
            schemat.buildingsGroups_1.Add(new BuildingGroup("The Oldest"));
            schemat.buildingsGroups_1[2].buildings.Add(new BuildingSchema("Building_1"));
            schemat.buildingsGroups_1[2].buildings.Add(new BuildingSchema("Building_2"));
            schemat.buildingsGroups_1[2].buildings.Add(new BuildingSchema("Building_3"));
            schemat.buildingsGroups_1[2].buildings.Add(new BuildingSchema("Building_4"));
            schemat.buildingsGroups_1[2].buildings.Add(new BuildingSchema("Building_5"));

            schemat.decorationsGroups.Add(new DecoarationGroup("Ruins"));
            schemat.decorationsGroups[0].decorations.Add(new DecorationSchema("Ruiny1"));
            schemat.decorationsGroups[0].decorations.Add(new DecorationSchema("Ruiny2"));
            schemat.decorationsGroups[0].decorations.Add(new DecorationSchema("Ruiny3"));

            schemat.decorationsGroups.Add(new DecoarationGroup("Forests"));
            schemat.decorationsGroups[1].decorations.Add(new DecorationSchema("Drzewo1"));
            schemat.decorationsGroups[1].decorations.Add(new DecorationSchema("Drzewo2"));
            schemat.decorationsGroups[1].decorations.Add(new DecorationSchema("Drzewo3"));
            schemat.decorationsGroups[1].decorations.Add(new DecorationSchema("Drzewo4"));


            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));

            System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Zielu\Desktop\SerializationOverview.xml");
            writer.Serialize(file, schemat);
            file.Close();
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

                    if ((creationMode == CREATION_MODE.TERRAIN_DOWN) || (creationMode == CREATION_MODE.BUILDINGS_MOVE) || (creationMode == CREATION_MODE.UNITS_MOVE) || (creationMode == CREATION_MODE.DECORATIONS_MOVE))
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
                    String directoryName = @"Mapa\";

                    Console.Out.WriteLine("Zapisywanie mapy w lokalizacji: " + directoryName);

                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    terrain.TerrainMap.SaveAsJpeg(File.Open(directoryName + "HighMap.jpg", FileMode.Create), terrain.TerrainMap.Width, terrain.TerrainMap.Height);

                    ObjectsSchema schemat = new ObjectsSchema();
                    System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));

                    #region Player_1
                    schemat.unitGroups_1.Add(new UnitGroup("Units"));
                    foreach (Unit unit in player.UnitList)
                    {
                        schemat.unitGroups_1[0].units.Add(new UnitSchema(unit.type.name, unit.Position.X, unit.Position.Z));
                    }

                    schemat.buildingsGroups_1.Add(new BuildingGroup("Buildings"));
                    foreach (Building building in player.BuildingList)
                    {
                        schemat.buildingsGroups_1[0].buildings.Add(new BuildingSchema(building.type.Name, building.Position.X, building.Position.Z));
                    }
                    #endregion Player_1

                    #region Player_2
                    schemat.unitGroups_2.Add(new UnitGroup("Units"));
                    foreach (Unit unit in enemy.UnitList)
                    {
                        schemat.unitGroups_2[0].units.Add(new UnitSchema(unit.type.name, unit.Position.X, unit.Position.Z));
                    }

                    schemat.buildingsGroups_2.Add(new BuildingGroup("Buildings"));
                    foreach (Building building in enemy.BuildingList)
                    {
                        schemat.buildingsGroups_2[0].buildings.Add(new BuildingSchema(building.type.Name, building.Position.X, building.Position.Z));
                    }
                    #endregion Player_2

                    schemat.decorationsGroups.Add(new DecoarationGroup("Decorations"));
                    foreach (Decoration decoration in decorations.DecorationList)
                    {
                        schemat.decorationsGroups[0].decorations.Add(new DecorationSchema(decoration.type.name, decoration.Position.X, decoration.Position.Z));
                    }

                    System.IO.StreamWriter file = new System.IO.StreamWriter(directoryName + "Objects.xml");
                    writer.Serialize(file, schemat);
                    file.Close();

                    Console.Out.WriteLine("Zapisano mapê w lokalizacji: " + directoryName);
                }
                else if (keyboardState.IsKeyDown(Keys.F3))
                {

                    Console.Out.WriteLine("Odczytywanie mapy...");

                    System.Drawing.Bitmap b = new System.Drawing.Bitmap(@"Mapa\HighMap.jpg");
                    using (MemoryStream s = new MemoryStream())
                    {
                        b.Save(s, System.Drawing.Imaging.ImageFormat.Jpeg);
                        s.Seek(0, SeekOrigin.Begin);
                        terrain.TerrainMap = Texture2D.FromStream(device, s);
                        terrain.ReloadTerrainMap();
                    }

                    ObjectsSchema tmp = new ObjectsSchema();
                    System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));
                    System.IO.StreamReader file = new System.IO.StreamReader(@"Mapa\Objects.xml");
                    tmp = (ObjectsSchema)reader.Deserialize(file);

                    #region Player_1
                    foreach (UnitSchema unit in tmp.unitGroups_1[0].units)
                    {
                        player.UnitList.Add(new Unit(player.game, UnitTypes[unit.name], new Vector3(unit.x, 30, unit.y),0.2f));
                    }

                    foreach (BuildingSchema building in tmp.buildingsGroups_1[0].buildings)
                    {
                        player.BuildingList.Add(new Building(player.game, BuildingTypes[building.name], new Vector3(building.x, 30, building.y), BuildingTypes[building.name].Scale));
                    }
                    #endregion Player_1

                    #region Player_2
                    foreach (UnitSchema unit in tmp.unitGroups_2[0].units)
                    {
                        enemy.UnitList.Add(new Unit(player.game, UnitTypes[unit.name], new Vector3(unit.x, 30, unit.y),0.2f));
                    }

                    foreach (BuildingSchema building in tmp.buildingsGroups_2[0].buildings)
                    {
                        enemy.BuildingList.Add(new Building(player.game, BuildingTypes[building.name], new Vector3(building.x, 30, building.y), BuildingTypes[building.name].Scale));
                    }
                    #endregion Player_2

                    foreach (DecorationSchema decoration in tmp.decorationsGroups[0].decorations)
                    {
                        decorations.DecorationList.Add(new Decoration(player.game, decorations.DecorationTypes[decoration.name], new Vector3(decoration.x, 30, decoration.y),0.5f));
                    }

                    Console.Out.WriteLine("Odczytano mapê.");
                }
                else if (keyboardState.IsKeyDown(Keys.D8) &&
                        (
                            (creationMode == CREATION_MODE.BUILDINGS_BUILD) ||
                            (creationMode == CREATION_MODE.UNITS_BUILD) ||
                            (creationMode == CREATION_MODE.DECORATIONS_BUILD)
                        )
                    )
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
                        case CREATION_MODE.BUILDINGS_BUILD:
                            creationMode = CREATION_MODE.BUILDINGS_MOVE;
                            break;
                        case CREATION_MODE.BUILDINGS_MOVE:
                            creationMode = CREATION_MODE.BUILDINGS_BUILD;
                            break;
                        case CREATION_MODE.UNITS_BUILD:
                            creationMode = CREATION_MODE.UNITS_MOVE;
                            break;
                        case CREATION_MODE.UNITS_MOVE:
                            creationMode = CREATION_MODE.UNITS_BUILD;
                            break;
                        case CREATION_MODE.DECORATIONS_BUILD:
                            creationMode = CREATION_MODE.DECORATIONS_MOVE;
                            break;
                        case CREATION_MODE.DECORATIONS_MOVE:
                            creationMode = CREATION_MODE.DECORATIONS_BUILD;
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
                #region Units
                else if (creationMode == CREATION_MODE.UNITS_BUILD)
                {
                    if (!oneMouseClickDetected)
                    {
                        oneMouseClickDetected = true;

                        if ((creationOption < 6) && (!objectsSchema.unitGroups_1[creationType].units[creationOption - 1].name.StartsWith("Unit_")))
                        {
                            switch (activePlayer)
                            {
                                case 0:
                                    player.UnitList.Add(new Unit(player.game, UnitTypes[objectsSchema.unitGroups_1[creationType].units[creationOption - 1].name], new Vector3(positionWidth, 30, positionHeight),0.1f));
                                    break;
                                case 1:
                                    enemy.UnitList.Add(new Unit(player.game, UnitTypes[objectsSchema.unitGroups_1[creationType].units[creationOption - 1].name], new Vector3(positionWidth, 30, positionHeight),0.1f));
                                    break;
                            }
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
                #endregion Units
                #region Buildings
                else if (creationMode == CREATION_MODE.BUILDINGS_BUILD)
                {
                    if (!oneMouseClickDetected)
                    {
                        oneMouseClickDetected = true;

                        if ((creationOption < 6) && (!objectsSchema.buildingsGroups_1[creationType].buildings[creationOption - 1].name.StartsWith("Building_")))
                        {
                            Building building = new Building(player.game, BuildingTypes[objectsSchema.buildingsGroups_1[creationType].buildings[creationOption - 1].name], new Vector3(positionWidth, 30, positionHeight),0.6f);

                            if (building.checkIfPossible(pointerPosition))
                            {
                                switch (activePlayer)
                                {
                                    case 0:
                                        player.BuildingList.Add(building);
                                        break;
                                    case 1:
                                        enemy.BuildingList.Add(building);
                                        break;
                                }
                            }
                        }
                    }
                }
                else if (creationMode == CREATION_MODE.BUILDINGS_MOVE)
                {
                    Building buildingSelected = null;
                    foreach (Building building in player.BuildingList)
                    {
                        if (building.selected)
                        {
                            buildingSelected = building;
                            break;
                        }
                    }

                    if (buildingSelected.checkIfPossible(pointerPosition))
                    {
                        for (int i = 0; i < player.BuildingList.Count; i++)
                        {
                            bool selected = Collisions.RayModelCollision(clippedRay, player.BuildingList[i].currentModel.Model, player.BuildingList[i].GetWorldMatrix());
                            if (selected)
                            {
                                if ((player.BuildingList[i] is Building) && !player.BuildingList[i].selected)
                                {
                                    foreach (Building building in player.BuildingList)
                                        EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, building, null));

                                    EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, player.BuildingList[i], null));
                                    break;
                                }
                            }
                            if ((!selected) && (buildingSelected != null))
                            {
                                buildingSelected.Position.Z = pointerPosition.Z;
                                buildingSelected.Position.X = pointerPosition.X;
                            }
                        }
                    }
                }
                #endregion Building
                #region Decorations
                else if (creationMode == CREATION_MODE.DECORATIONS_BUILD)
                {
                    if (!oneMouseClickDetected)
                    {
                        oneMouseClickDetected = true;

                        if ((creationOption < 6) && (!objectsSchema.buildingsGroups_1[creationType].buildings[creationOption - 1].name.StartsWith("Decorations_")))
                        {
                            Decoration decoration = new Decoration(player.game, decorations.DecorationTypes[objectsSchema.decorationsGroups[creationType].decorations[creationOption - 1].name], new Vector3(positionWidth, 30, positionHeight),0.1f);

                            if (decoration.checkIfPossible(pointerPosition))
                            {
                                decorations.DecorationList.Add(decoration);
                            }
                        }
                    }
                }
                else if (creationMode == CREATION_MODE.DECORATIONS_MOVE)
                {
                    if (CheckGroundForBuilding(new Vector2(pointerPosition.Z, pointerPosition.X)))
                    {
                        Decoration decorationSelected = null;
                        foreach (Decoration decoration in decorations.DecorationList)
                        {
                            if (decoration.selected)
                            {
                                decorationSelected = decoration;
                                break;
                            }
                        }

                        if (decorationSelected.checkIfPossible(pointerPosition))
                        {
                            for (int i = 0; i < decorations.DecorationList.Count; i++)
                            {
                                bool selected = Collisions.RayModelCollision(clippedRay, decorations.DecorationList[i].currentModel.Model, decorations.DecorationList[i].GetWorldMatrix());
                                if (selected)
                                {
                                    if ((decorations.DecorationList[i] is Decoration) && !decorations.DecorationList[i].selected)
                                    {
                                        foreach (Decoration decoration in decorations.DecorationList)
                                            EventManager.CreateMessage(new Message((int)EventManager.Events.Unselected, null, decoration, null));

                                        EventManager.CreateMessage(new Message((int)EventManager.Events.Selected, null, decorations.DecorationList[i], null));
                                        break;
                                    }
                                }
                                if ((!selected) && (decorationSelected != null))
                                {
                                    decorationSelected.Position.Z = pointerPosition.Z;
                                    decorationSelected.Position.X = pointerPosition.X;
                                }
                            }
                        }
                    }
                }
                #endregion Decorations
            }
            else if (mouseState.LeftButton == ButtonState.Released)
            {
                oneMouseClickDetected = false;
            }

            Input.Update(gameTime, device, camera, player.UnitList, decorations.DecorationList);

            base.Update(gameTime);
        }

        private bool CheckGroundForBuilding(Vector2 _position)
        {
            Color[] data = new Color[terrain.TerrainMap.Height * terrain.TerrainMap.Width];
            terrain.TerrainMap.GetData<Color>(data);

            int max = 20;
            int standardHeight = data[(int)(_position.X * terrain.TerrainMap.Height + _position.Y)].R;

            for (int i = 0; i < max; ++i)
            {
                for (int j = max; j < max; ++j)
                {
                    if (Math.Abs(data[((int)_position.X - max / 2 + i) * terrain.TerrainMap.Height + ((int)_position.Y - max / 2 + j)].R - standardHeight) > 100)
                    {
                        return false;
                    }
                }
            }

            return true;
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
            objects.AddRange(player.BuildingList);

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

            spriteBatch.DrawString(Times, "Budowanie dla gracza nr " + (activePlayer + 1), new Vector2(0, positionOfText += heightOfText), Color.White);

            switch (creationMode)
            {
                case CREATION_MODE.TERRAIN_UP:
                    spriteBatch.DrawString(Times, "Podnoszenie terenu", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "Rozmiar pedzla: " + pencilSize, new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "Moc pedzla: " + pencilPower, new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "9. Obnizanie terenu.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.TERRAIN_DOWN:
                    spriteBatch.DrawString(Times, "Obnizanie terenu", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "Rozmiar pedzla: " + pencilSize, new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "Moc pedzla: " + pencilPower, new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "9. Podnoszenie terenu.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.BUILDINGS_BUILD:
                    spriteBatch.DrawString(Times, "Budynki: " + objectsSchema.buildingsGroups_1[creationType].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    for (int i = 0; i < objectsSchema.buildingsGroups_1[creationType].buildings.Count; ++i)
                    {
                        spriteBatch.DrawString(Times, (i + 1) + ". " + objectsSchema.buildingsGroups_1[creationType].buildings[i].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    }
                    spriteBatch.DrawString(Times, "8. Zmiana grupy.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "9. Przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.BUILDINGS_MOVE:
                    spriteBatch.DrawString(Times, "Budynki przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "9. Budowanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.UNITS_BUILD:
                    spriteBatch.DrawString(Times, "Jednostki: " + objectsSchema.unitGroups_1[creationType].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    for (int i = 0; i < objectsSchema.unitGroups_1[creationType].units.Count; ++i)
                    {
                        spriteBatch.DrawString(Times, (i + 1) + ". " + objectsSchema.unitGroups_1[creationType].units[i].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    }
                    spriteBatch.DrawString(Times, "8. Zmiana grupy.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "9. Przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.UNITS_MOVE:
                    spriteBatch.DrawString(Times, "Jednostki przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "9. Budowanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.DECORATIONS_BUILD:
                    spriteBatch.DrawString(Times, "Dekoracje: " + objectsSchema.decorationsGroups[creationType].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    for (int i = 0; i < objectsSchema.decorationsGroups[creationType].decorations.Count; ++i)
                    {
                        spriteBatch.DrawString(Times, (i + 1) + ". " + objectsSchema.decorationsGroups[creationType].decorations[i].name, new Vector2(0, positionOfText += heightOfText), Color.White);
                    }
                    spriteBatch.DrawString(Times, "8. Zmiana grupy.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "9. Przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
                case CREATION_MODE.DECORATIONS_MOVE:
                    spriteBatch.DrawString(Times, "Dekoracje przemieszczanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "9. Budowanie.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    spriteBatch.DrawString(Times, "0. Zmien tryb.", new Vector2(0, positionOfText += heightOfText), Color.White);
                    break;
            }
            spriteBatch.End();
        }

        private void DrawHelp()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(Times, "F2 - zapisz bitmape" + pencilSize, new Vector2(0, 215), Color.Green);
            spriteBatch.End();
        }
    }
}
