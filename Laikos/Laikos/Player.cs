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
        private Dictionary<String,UnitType> UnitTypes;
        //private Dictionary<String, BuildingType> BuildingTypes;
        public List<Unit> UnitList;
        //public List<Building> BuildingList;
        public Game game;
        
        public Player(Game game, Dictionary<String,UnitType> UnitTypes)
        {
            this.game = game;
            this.UnitTypes = UnitTypes;

            UnitList = new List<Unit>();

            UnitList.Add(new Unit(game, UnitTypes["Reconnaissance Eye"], new Vector3(100, 30, 150), 0.05f));
            //UnitList.Add(new Unit(game, UnitTypes["Dummy"], new Vector3(10, 30, 50), 0.1f));
            //UnitList.Add(new Unit(game, UnitTypes["Reconnaissance Eye"], new Vector3(10, 30, 100), 0.05f));
            //UnitList.Add(new Unit(game, UnitTypes["Antigravity Tank"], new Vector3(10,30,50), 0.15f));
            //UnitList.Add(new Unit(game, UnitTypes["Alien"], new Vector3(10, 30, 70), 0.05f));
            

        }

        public void Update(GameTime gameTime)
        {
            foreach (Unit unit in UnitList)
            {
                unit.Update(gameTime);
            }
        }       
    }
}