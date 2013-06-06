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
    class BuildingManager : DrawableGameComponent
    {
        public Dictionary<String,BuildingType> BuildingTypes;
        public List<Building> BuildingList;
        
        public GraphicsDevice device;
        GraphicsDeviceManager graphics;
        public Game game;
        
        //List<Message> messages;

        public BuildingManager(Game game, GraphicsDevice device, GraphicsDeviceManager graphics)
            : base(game)
        {
            this.game = game;
            this.graphics = graphics;
            this.device = device;
        }

        public override void Initialize()
        {
            BuildingTypes = new Dictionary<String, BuildingType>();
            BuildingList = new List<Building>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            BuildingTypes = Game.Content.Load<BuildingType[]>("BuildingTypes").ToDictionary(t => t.name);
             
            BuildingList.Add(new Building(game, BuildingTypes["Obserwatorium"], new Vector3(10, 30, 50), 0.1f));

        }

        public override void Update(GameTime gameTime)
        {
            foreach (Building unit in BuildingList)
            {   
                unit.Update(gameTime);
            }
        }
    }
}
