using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;
using MyDataTypes;


namespace Laikos
{
    class UnitManager : DrawableGameComponent
    {
        public Dictionary<String,UnitType> UnitTypes;
        public List<Unit> UnitList;
        public GraphicsDevice device;
        public Game game;
        GraphicsDeviceManager graphics;


        public UnitManager(Game game, GraphicsDevice device, GraphicsDeviceManager graphics)
            : base(game)
            
        {
            this.game = game;
            this.graphics = graphics;
            this.device = device;
        }

        public override void Initialize()
        {
            UnitTypes = new Dictionary<String,UnitType>();
            UnitList = new List<Unit>(); 
            base.Initialize();
        }

        protected override void LoadContent()
        {
            UnitTypes = Game.Content.Load<UnitType[]>("UnitTypes").ToDictionary(t => t.name);

            UnitList.Add(new Unit(game, UnitTypes["Reconnaissance Eye"], new Vector3(10, 30, 150), 0.05f));
            UnitList.Add(new Unit(game, UnitTypes["Dummy"], new Vector3(10, 30, 50), 0.1f));
            UnitList.Add(new Unit(game, UnitTypes["Reconnaissance Eye"], new Vector3(10, 30, 100), 0.05f));
            //UnitList.Add(new Unit(game, UnitTypes["Mobile Cannon"], new Vector3(40, 30, 100), 0.1f));
            
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Unit unit in UnitList)
            {
                unit.Update(gameTime);
            //    Console.WriteLine(unit.currentModel.player.clip.Name);
            }

        }       
    }
}