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
    public class Player
    {
        private Dictionary<String, UnitType> UnitTypes;
        private Dictionary<String, BuildingType> BuildingTypes;

        public List<Unit> UnitList;
        public List<Building> BuildingList;

        public Game game;

        public Player(Game game, Dictionary<String, UnitType> UnitTypes, Dictionary<String, BuildingType> BuildingTypes)
        {
            this.game = game;
            this.UnitTypes = UnitTypes;
            this.BuildingTypes = BuildingTypes;

            UnitList = new List<Unit>();
            BuildingList = new List<Building>();
        }

        public void Update(GameTime gameTime)
        {
            foreach (Unit unit in UnitList)
            {
                unit.Update(gameTime);
            }

            foreach (Building building in BuildingList)
            {
                building.Update(gameTime);
            }
        }
    }
}