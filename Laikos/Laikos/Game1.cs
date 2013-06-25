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
        public static bool dText0, dText1, dText2, dText3, dText4, dText5, dText6, dText7;
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
        int Step;
        Dictionary<String, UnitType> UnitTypes;
        Dictionary<String, BuildingType> BuildingTypes;
        public static bool Intro,Intro2;
        public Player player;
        public Player enemy;
        float IntroTimer = 2.5f;
        const float IntroTIMER = 2.5f;
        float textTimer = 4;
        const float textTIMER = 4;

        public static SoundEffect[] sounds;
        public static Video video;
        private Texture2D videoTexture;

        // 0 - Never Play
        // 1 - Is Playing
        // 2 - Played
        public static int playIntro = 0;
        public static VideoPlayer videoPlayer;

        public Game1()
        {
            time = new TimeSpan();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1366;
            graphics.PreferredBackBufferHeight = 768;

            graphics.IsFullScreen = false;

            Intro = false;
            Intro2 = false;

            dText0 = false;
            dText1 = false;
            dText2 = false;
            dText3 = false;
            dText4 = false;
            dText5 = false;
            dText6 = false;
            dText7 = false;
            Step = 0;

            videoPlayer = new VideoPlayer();
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

            sounds = new SoundEffect[1];
            sounds[0] = Content.Load<SoundEffect>("Sounds/Shot");
            video = Content.Load<Video>("Video/Intro");

            LoadMap(@"Mapa\Objects.xml");

            player.Initialize();
            enemy.Initialize();

            Laikos.PathFiding.Map.loadMap(Content.Load<Texture2D>("Models/Terrain/Heightmaps/heightmap4"), decorations);

            InitializeMapAfterLoad();

            Minimap.LoadMiniMap(Content);
            SelectingGUI.Init(device, graphics, this, player.UnitList, player.BuildingList, enemy.UnitList, enemy.BuildingList);
            GUI.Initialize(device, spriteBatch, Content, player);
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

            //Intro = true;

            if (enemy.BuildingList.Count <= 0)
            {
                Intro = false;
            }
          
                if (Menu.inMenu == false && Intro2 == false)
                {
                    Console.WriteLine("trolls");
                   
                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    {
                        videoPlayer.Stop();
                        Intro2 = true;
                    }
                    if ((Game1.videoPlayer.State == MediaState.Stopped))
                    {
                        Console.WriteLine("trorlolo");
                        Intro2 = true;
                    }
                    base.Update(gameTime);
                    return;
                }
            

            if (Intro == false && !Menu.inMenu && Intro2 == true)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.J))
                {
                    Intro = true;
                }
                dText0 = true;
                GameIntro(gameTime);
                base.Update(gameTime);
                return;
            }

            float frameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            time = gameTime.TotalGameTime;
            Input.Update(this, gameTime, device, camera, player, decorations.DecorationList);

            player.Update(gameTime);
            enemy.Update(gameTime);
            GUI.Update(gameTime);

            GUIEventManager.Update();
            EventManager.Update();

         

            // TODO: Add your update logic here
            bool collision = false;
            for (int i = 0; i < player.UnitList.Count; i++)
            {
                bool InMove = false;
                foreach (Message _msg in player.UnitList[i].messages)
                {
                    if
                    (
                        (!_msg.Done)
                        &&
                        (
                            (_msg.Type == (int)EventManager.Events.MoveUnit)
                            ||
                            (_msg.Type == (int)EventManager.Events.MoveToAttack)
                            ||
                            (_msg.Type == (int)EventManager.Events.MoveToBuild)
                            ||
                            (_msg.Type == (int)EventManager.Events.Attack)
                        )
                    )
                    {
                        InMove = true;
                    }
                }

                if (InMove)
                {
                    for (int j = i + 1; j < player.UnitList.Count; j++)
                    {
                        collision = Collisions.DetailedCollisionCheck(player.UnitList[i], player.UnitList[j]);

                        if (collision)
                        {
                            EventManager.CreateMessage(new Message((int)EventManager.Events.FixCollisions, player.UnitList[j], player.UnitList[i], null));
                        }
                    }
                }

                for (int j = 0; j < enemy.UnitList.Count; j++)
                {
                    if (InMove)
                    {
                        collision = Collisions.DetailedCollisionCheck(player.UnitList[i], enemy.UnitList[j]);

                        if (collision)
                        {
                            EventManager.CreateMessage(new Message((int)EventManager.Events.FixCollisions, enemy.UnitList[j], player.UnitList[i], null));
                        }
                    }
                    else
                    {
                        collision = player.UnitList[i].attackRadius.Intersects(enemy.UnitList[j].boundingSphere);

                        if ((collision) && (player.UnitList[i].range > 0))
                        {
                            bool attacking = false;
                            foreach (Message _msg in player.UnitList[i].messages)
                            {
                                if ((!_msg.Done) && (_msg.Type == (int)EventManager.Events.Attack))
                                {
                                    attacking = true;
                                    break;
                                }
                            }

                            if (!attacking)
                            {
                                EventManager.CreateMessage(new Message((int)EventManager.Events.MoveToAttack, null, player.UnitList[i], enemy.UnitList[j]));
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < enemy.UnitList.Count; i++)
            {
                bool InMove = false;
                foreach (Message _msg in enemy.UnitList[i].messages)
                {
                    if
                    (
                        (!_msg.Done)
                        &&
                        (
                            (_msg.Type == (int)EventManager.Events.MoveUnit)
                            ||
                            (_msg.Type == (int)EventManager.Events.MoveToAttack)
                            ||
                            (_msg.Type == (int)EventManager.Events.MoveToBuild)
                            ||
                            (_msg.Type == (int)EventManager.Events.Attack)
                        )
                    )
                    {
                        InMove = true;
                    }
                }

                if (InMove)
                {
                    for (int j = i + 1; j < enemy.UnitList.Count; j++)
                    {
                        collision = Collisions.DetailedCollisionCheck(enemy.UnitList[i], enemy.UnitList[j]);

                        if (collision)
                        {
                            EventManager.CreateMessage(new Message((int)EventManager.Events.FixCollisions, enemy.UnitList[j], enemy.UnitList[i], null));
                        }
                    }
                }

                for (int j = 0; j < player.UnitList.Count; j++)
                {
                    if (InMove)
                    {
                        collision = Collisions.DetailedCollisionCheck(enemy.UnitList[i], player.UnitList[j]);

                        if (collision)
                        {
                            EventManager.CreateMessage(new Message((int)EventManager.Events.FixCollisions, player.UnitList[j], enemy.UnitList[i], null));
                        }
                    }
                    else
                    {
                        collision = enemy.UnitList[i].attackRadius.Intersects(player.UnitList[j].boundingSphere);

                        if ((collision) && (enemy.UnitList[i].range > 0))
                        {
                            bool attacking = false;
                            foreach (Message _msg in enemy.UnitList[i].messages)
                            {
                                if ((!_msg.Done) && (_msg.Type == (int)EventManager.Events.Attack))
                                {
                                    attacking = true;
                                    break;
                                }
                            }

                            if (!attacking)
                            {
                                EventManager.CreateMessage(new Message((int)EventManager.Events.MoveToAttack, null, enemy.UnitList[i], player.UnitList[j]));
                            }
                        }
                    }
                }
            }

            base.Update(gameTime);
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

            if (playIntro < 3)
            {
                if (videoPlayer.State == MediaState.Playing)
                {
                    Rectangle screen = new Rectangle(device.Viewport.X, device.Viewport.Y, device.Viewport.Width, device.Viewport.Height);

                    videoTexture = videoPlayer.GetTexture();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
                    spriteBatch.Draw(videoTexture, screen, Color.White);
                    spriteBatch.End();

                    return;
                }
            }

            objects.AddRange(player.UnitList);
            objects.AddRange(player.BuildingList);

            objects.AddRange(enemy.UnitList);
            objects.AddRange(enemy.BuildingList);

            objects.AddRange(decorations.DecorationList);

            DefferedRenderer.explosionParticles.SetCamera(Camera.viewMatrix, Camera.projectionMatrix);
            DefferedRenderer.explosionSmokeParticles.SetCamera(Camera.viewMatrix, Camera.projectionMatrix);
            DefferedRenderer.SmokePlumeParticles.SetCamera(Camera.viewMatrix, Camera.projectionMatrix);

            defferedRenderer.Draw(objects, terrain, gameTime);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, null, null);
            spriteBatch.End();

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
                player.UnitList.Add(new Unit(player.game, player, UnitTypes[unit.name], new Vector3(unit.x, 30, unit.y), UnitTypes[unit.name].Scale));
            }

            foreach (BuildingSchema building in tmp.buildingsGroups_1[0].buildings)
            {
                player.BuildingList.Add(new Building(player.game, player, BuildingTypes[building.name], new Vector3(building.x, 30, building.y), building.scale, true, new Vector3(MathHelper.ToRadians(0), MathHelper.ToRadians(building.rotation), MathHelper.ToRadians(0))));
            }
            #endregion Player_1

            #region Player_2
            foreach (UnitSchema unit in tmp.unitGroups_2[0].units)
            {
                enemy.UnitList.Add(new Unit(player.game, enemy, UnitTypes[unit.name], new Vector3(unit.x, 30, unit.y), UnitTypes[unit.name].Scale));
            }

            foreach (BuildingSchema building in tmp.buildingsGroups_2[0].buildings)
            {
                enemy.BuildingList.Add(new Building(enemy.game, enemy, BuildingTypes[building.name], new Vector3(building.x, 30, building.y), building.scale, true, new Vector3(MathHelper.ToRadians(0), building.rotation, MathHelper.ToRadians(0))));
            }
            #endregion Player_2

            foreach (DecorationSchema decoration in tmp.decorationsGroups[0].decorations)
            {
                decorations.DecorationList.Add(new Decoration(player.game, decorations.DecorationTypes[decoration.name], new Vector3(decoration.x, 30, decoration.y), decoration.scale, new Vector3(MathHelper.ToRadians(0), decoration.rotation, MathHelper.ToRadians(0))));
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


        public void GameIntro(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            IntroTimer -= elapsed;

            if (IntroTimer < 0)
            {
                //Timer expired, execute action
                dText0 = false;
                switch (Step)
                {
                    case 0:
                        dText1 = true;
                        Camera.InitBezier2(new Vector3(515, 91, 417)); //prison
                        if (Math.Round(Camera.cameraPosition.X) == 515 && Math.Round(Camera.cameraPosition.Y) == 91 && Math.Round(Camera.cameraPosition.Z) == 417)
                        {
                            textTimer -= elapsed;

                            if (textTimer < 0)
                            {
                                Step += 1;
                                dText1 = false;
                                textTimer = textTIMER;
                                GUI.typing = new StringTypingEffect(this, GUI.spriteBatch);
                                break;
                            }

                        }
                        break;
                    case 1:
                        dText2 = true;
                        Camera.InitBezier2(new Vector3(446, 91, 383)); // gen1
                        if (Math.Round(Camera.cameraPosition.X) == 446 && Math.Round(Camera.cameraPosition.Y) == 91 && Math.Round(Camera.cameraPosition.Z) == 383)
                        {

                            textTimer -= elapsed;

                            if (textTimer < 0)
                            {
                                Step += 1;
                                dText2 = false;
                                textTimer = textTIMER;
                                GUI.typing = new StringTypingEffect(this, GUI.spriteBatch);
                                break;
                            }
                        }
                        break;
                    case 2:
                        dText3 = true;
                        Camera.InitBezier2(new Vector3(338, 91, 366)); //gen2
                        if (Math.Round(Camera.cameraPosition.X) == 338 && Math.Round(Camera.cameraPosition.Y) == 91 && Math.Round(Camera.cameraPosition.Z) == 366)
                        {

                            textTimer -= elapsed;

                            if (textTimer < 0)
                            {
                                Step += 1;
                                dText3 = false;
                                textTimer = textTIMER;
                                GUI.typing = new StringTypingEffect(this, GUI.spriteBatch);
                                break;
                            }
                        }
                        break;
                    case 3:
                        dText4 = true;
                        Camera.InitBezier2(new Vector3(302, 91, 445)); //gen3
                        if (Math.Round(Camera.cameraPosition.X) == 302 && Math.Round(Camera.cameraPosition.Y) == 91 && Math.Round(Camera.cameraPosition.Z) == 445)
                        {

                            textTimer -= elapsed;

                            if (textTimer < 0)
                            {
                                Step += 1;
                                dText4 = false;
                                textTimer = textTIMER;
                                GUI.typing = new StringTypingEffect(this, GUI.spriteBatch);
                                break;
                            }
                        }
                        break;
                    case 4:
                        dText5 = true;
                        Camera.InitBezier2(new Vector3(380, 91, 490)); //gen4
                        if (Math.Round(Camera.cameraPosition.X) == 380 && Math.Round(Camera.cameraPosition.Y) == 91 && Math.Round(Camera.cameraPosition.Z) == 490)
                        {

                            textTimer -= elapsed;

                            if (textTimer < 0)
                            {
                                Step += 1;
                                dText5 = false;
                                textTimer = textTIMER;
                                GUI.typing = new StringTypingEffect(this, GUI.spriteBatch);
                                break;
                            }
                        }
                        break;
                    case 5:
                        dText6 = true;
                        Camera.InitBezier2(new Vector3(530, 121, 633));
                        if (Math.Round(Camera.cameraPosition.X) == 530 && Math.Round(Camera.cameraPosition.Y) == 121 && Math.Round(Camera.cameraPosition.Z) == 633)
                        {

                            textTimer -= elapsed;

                            if (textTimer < 0)
                            {

                                Step += 1;
                                dText6 = false;
                                textTimer = textTIMER;
                                GUI.typing = new StringTypingEffect(this, GUI.spriteBatch);
                                Intro = true;
                                break;
                            }
                        }
                        break;
                    case 6:
                        dText7 = true;
                        Camera.InitBezier2(new Vector3(515, 100, 417));
                        if (Math.Round(Camera.cameraPosition.X) == 515 && Math.Round(Camera.cameraPosition.Y) == 100 && Math.Round(Camera.cameraPosition.Z) == 417)
                        {
                            textTimer -= elapsed;


                        }
                        break;
                }

            }

        }
    }
}
