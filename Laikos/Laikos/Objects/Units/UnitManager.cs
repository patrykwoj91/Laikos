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
        public List<Unit> UnitList;
        public Dictionary<String,Model> ModelList;
        public GraphicsDevice device;
        public Game game;
        GraphicsDeviceManager graphics;

        public UnitManager(Game game, GraphicsDevice device, GraphicsDeviceManager graphics)
            : base(game)
            
        {
            this.game = game;
            this.graphics = graphics;
            UnitList = new List<Unit>();
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
            String path = "Models/Test_model/Test_FBX_Y";
            UnitList.Add(new Unit(game, path,new Vector3(10, 30, 150),0.1f));
            UnitList.Add(new Unit(game, path, new Vector3(20, 30, 50), 0.1f));
         
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Unit unit in UnitList)
            {
                unit.Update(gameTime);
            }
            Input.SelectUnit(UnitList, device);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (Unit unit in UnitList)
            {
                unit.Draw(graphics);
            }
        }
       
    }
}