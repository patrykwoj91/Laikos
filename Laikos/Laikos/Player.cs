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
        public Game game;

        public Dictionary<String,UnitType> UnitTypes;
        public List<Unit> UnitList;
        public Dictionary<String, BuildingType> BuildingTypes;
        public List<Building> BuildingList;

        public List<Message> messages;
       
        public static int Souls;
 
        public Player(Game game, Dictionary<String, UnitType> UnitTypes, Dictionary<String, BuildingType> BuildingTypes)
        {
            this.game = game;
            this.UnitTypes = UnitTypes;
            UnitList = new List<Unit>();
            this.BuildingTypes = BuildingTypes;
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
       

       public void Initialize()
       {
            Souls = 1000;

            UnitList.Add(new Unit(game,this, UnitTypes["Droid Worker"], new Vector3(680, 0, 680), 0.05f));
            UnitList.Add(new Unit(game, this, UnitTypes["Reconnaissance Eye"], new Vector3(680, 0, 670), 0.05f));
            BuildingList.Add(new Building(game, BuildingTypes["Pałac rady2"], new Vector3(720, 0, 650), BuildingTypes["Pałac rady2"].Scale,true));
       }

       public bool Build(BuildingType building, Vector3 position)
       {
           if (Souls >= building.Souls)
           {
               BuildingList.Add(new Building(game, building, position, building.Scale));
               Souls -= building.Souls;
               Console.WriteLine(Souls);
               return true;
           }
           return false;
       }
    }
}