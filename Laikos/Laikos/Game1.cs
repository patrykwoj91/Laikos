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
using MyDataTypes.Serialization;

namespace Laikos
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static TimeSpan time;
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Vector3 pointerPosition = new Vector3(0, 0, 0);
        Camera camera;
        Terrain terrain;
        public DecorationManager decorations;
        DefferedRenderer defferedRenderer;
        public static List<GameObject> objects;

        Dictionary<String, UnitType> UnitTypes;
        Dictionary<String, BuildingType> BuildingTypes;

        public Player player;
        public Player enemy;

        System.Drawing.Bitmap bitmapTmp;


        public Game1()
        {
            time = new TimeSpan();
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
            device = graphics.GraphicsDevice;
            this.IsMouseVisible = true;

            terrain = new Terrain(this);
            camera = new Camera(this, graphics);
            decorations = new DecorationManager(this, device, graphics);

            Minimap.Initialize(device);
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
            defferedRenderer = new DefferedRenderer(device, Content, spriteBatch, font, this);
            objects = new List<GameObject>();

            UnitTypes = Content.Load<UnitType[]>("ObjectTypes/UnitTypes").ToDictionary(t => t.name);
            BuildingTypes = Content.Load<BuildingType[]>("ObjectTypes/BuildingTypes").ToDictionary(t => t.Name);

            player = new Player(this, UnitTypes, BuildingTypes);
            enemy = new Player(this, UnitTypes, BuildingTypes);

           
            //LoadMap(@"Mapa\Objects.xml");

            player.Initialize();
            //enemy.Initialize();

            Laikos.PathFiding.Map.loadMap(Content.Load<Texture2D>("Models/Terrain/Heightmaps/heightmap4"), decorations);

            InitializeMapAfterLoad();

            Minimap.LoadMiniMap(Content);

            Console.WriteLine(player.UnitList.Count);
            Console.WriteLine(enemy.UnitList.Count);

            SelectingGUI.Init(device, graphics, this, player.UnitList, player.BuildingList, enemy.UnitList, enemy.BuildingList);
            GUI.Initialize(device, spriteBatch, Content);
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
            float frameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            time = gameTime.TotalGameTime;

            player.Update(gameTime);
            enemy.Update(gameTime);

            Input.Update(this, gameTime, device, camera, player, decorations.DecorationList);

            EventManager.Update();

            UpdateExplosions(gameTime, objects);
            UpdateExplosionSmoke(gameTime, objects);

            base.Update(gameTime);

            // TODO: Add your update logic here

            bool collision = false;
            for (int i = 0; i < player.UnitList.Count; i++)
            {
                for (int j = i + 1; j < player.UnitList.Count; j++)
                {
                    collision = Collisions.DetailedCollisionCheck(player.UnitList[i], player.UnitList[j]);

                    if (collision)
                    {
                        player.UnitList[i].Position = player.UnitList[i].lastPosition;
                        player.UnitList[j].Position = player.UnitList[j].lastPosition;
                    }
                }
            }
            foreach (Unit unit in player.UnitList)
            {
                foreach (Building building in player.BuildingList)
                {
                    collision = Collisions.DetailedDecorationCollisionCheck(unit, building);
                    if (collision)
                        unit.Position = unit.lastPosition;
                }
            }

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

            objects.AddRange(player.UnitList);
            objects.AddRange(player.BuildingList);

            objects.AddRange(enemy.UnitList);
            objects.AddRange(enemy.BuildingList);

            objects.AddRange(decorations.DecorationList);
            
            defferedRenderer.Draw(objects, terrain, gameTime);
            SelectingGUI.Draw();

            objects.Clear();
            base.Draw(gameTime);


        }

        private void LoadMap(String _name)
        {
            ObjectsSchema tmp = new ObjectsSchema();
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(ObjectsSchema));
            System.IO.StreamReader file = new System.IO.StreamReader(_name);
            tmp = (ObjectsSchema)reader.Deserialize(file);

            #region Player_1
            foreach (UnitSchema unit in tmp.unitGroups_1[0].units)
            {
                player.UnitList.Add(new Unit(player.game, player, UnitTypes[unit.name], new Vector3(unit.x, 30, unit.y), 0.05f));
            }

            foreach (BuildingSchema building in tmp.buildingsGroups_1[0].buildings)
            {
                player.BuildingList.Add(new Building(player.game,player, BuildingTypes[building.name], new Vector3(building.x, 30, building.y), BuildingTypes[building.name].Scale));
            }
            #endregion Player_1

            #region Player_2
            foreach (UnitSchema unit in tmp.unitGroups_2[0].units)
            {
                enemy.UnitList.Add(new Unit(player.game, enemy, UnitTypes[unit.name], new Vector3(unit.x, 30, unit.y), 0.05f));
            }

            foreach (BuildingSchema building in tmp.buildingsGroups_2[0].buildings)
            {
                enemy.BuildingList.Add(new Building(enemy.game,enemy, BuildingTypes[building.name], new Vector3(building.x, 30, building.y), BuildingTypes[building.name].Scale));
            }
            #endregion Player_2

            foreach (DecorationSchema decoration in tmp.decorationsGroups[0].decorations)
            {
                decorations.DecorationList.Add(new Decoration(player.game, decorations.DecorationTypes[decoration.name], new Vector3(decoration.x, 30, decoration.y), 0.05f));
            }
        }

        private void InitializeMapAfterLoad()
        {
            foreach (Unit _unit in player.UnitList)
            {
                _unit.pathFiding.mapaUstaw();
            }

            foreach (Unit _unit in enemy.UnitList)
            {
                _unit.pathFiding.mapaUstaw();
            }
        }

        void UpdateExplosions(GameTime gameTime, List<GameObject> objects)
        {
            for (int i = /*player.UnitList.Count*/ objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] is Unit)
                {
                    if (((Unit)objects[i]).HP <= 0)
                    {
                        defferedRenderer.explosionParticles.AddParticle(((Unit)objects[i]).Position, Vector3.Zero);
                        defferedRenderer.explosionSmokeParticles.AddParticle(((Unit)objects[i]).Position, Vector3.Zero);
                        ((Unit)objects[i]).HP = 10;
                    }
                }
                else if (objects[i] is Building)
                {
                    if (((Building)objects[i]).HP <= 0)
                    {
                        defferedRenderer.explosionParticles.AddParticle(((Building)objects[i]).Position, Vector3.Zero);
                        defferedRenderer.explosionSmokeParticles.AddParticle(((Building)objects[i]).Position, Vector3.Zero);
                        ((Building)objects[i]).HP  = 10;
                    }
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
                    if (100 * unit.HP / unit.maxHP <= 5)
                    {
                        defferedRenderer.SmokePlumeParticles.AddParticle(objects[i].Position, Vector3.Zero);
                    }
                }
                defferedRenderer.explosionSmokeParticles.Update(gameTime);
            }
        }
    }
}

