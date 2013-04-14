using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Animation;


namespace Laikos
{
    class UnitManager : DrawableGameComponent
    {
        public List<GameUnit> UnitList;
        public Dictionary<String,Model> ModelList;
        public GraphicsDevice device;
        public Game game;
        GraphicsDeviceManager graphics;

        public UnitManager(Game game, GraphicsDevice device, GraphicsDeviceManager graphics)
            : base(game)
            
        {
            this.game = game;
            this.graphics = graphics;
            UnitList = new List<GameUnit>();
           // ModelList = new Dictionary<String, Model>();
            this.device = device;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //tu z pliku bedziemy sciezki do modeli wczytywac do listy modeli (na razie recznie)
            String path = "Models/Test_model2/Test_FBX_Y";
            
           // ModelList.Add("dude",Game.Content.Load<Model>(path));
            UnitList.Add(new GameUnit(game,path));
            UnitList.Add(new GameUnit(game, path));
            UnitList[1].Position = new Vector3(20, 30, 50);
        }

        public override void Update(GameTime gameTime)
        {
            foreach (GameUnit unit in UnitList)
            {
                Input.PickUnit(unit, device);
                unit.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (GameUnit unit in UnitList)
            {
                unit.Draw(graphics);
            }
        }
       
    }
}