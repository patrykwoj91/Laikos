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
        public Terrain terrain;
        public Camera camera;

        public UnitManager(Game game, Terrain terrain, Camera camera)
            : base(game)
        {
            this.camera = camera;
            this.terrain = terrain;
            UnitList = new List<GameUnit>();
            ModelList = new Dictionary<String, Model>();
        }

        public override void Initialize()
        {
            base.Initialize();

        }

        protected override void LoadContent()
        {
            //tu z pliku bedziemy sciezki do modeli wczytywac do listy modeli (na razie recznie)
            String path = "Models/Test_model/dude";
            
            ModelList.Add("dude",Game.Content.Load<Model>(path));
            UnitList.Add(new GameUnit(ModelList["dude"], terrain));
            UnitList[0].animationPlayer.RegisteredEvents["Fire"].Add("FireFrame", new AnimationPlayer.EventCallback(GameUnit.Fire));
            
            
        }

        public override void Update(GameTime gameTime)
        {
         //update obiektow
            foreach (GameUnit unit in UnitList)
            {
            unit.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (GameUnit unit in UnitList)
            {
                unit.Draw(camera);
            }
        }
       
    }
}