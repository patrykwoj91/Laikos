﻿using System;
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
       
        public int Souls;
 
        public Player(Game game, Dictionary<String, UnitType> UnitTypes, Dictionary<String, BuildingType> BuildingTypes)
        {
            this.game = game;
            this.UnitTypes = UnitTypes;
            UnitList = new List<Unit>();
            this.BuildingTypes = BuildingTypes;
            BuildingList = new List<Building>();
            Initialize();
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
            //UnitList.Add(new Unit(game, UnitTypes["Alien Worker"], new Vector3(620, 20, 700), 0.1f));
            UnitList.Add(new Unit(game, UnitTypes["Reconnaissance Eye"], new Vector3(620, 20, 750), 0.05f));
            UnitList.Add(new Unit(game, UnitTypes["Reconnaissance Eye"], new Vector3(620, 20, 700), 0.05f));
            //UnitList.Add(new Unit(game, UnitTypes["Alien Rider"], new Vector3(600, 25, 750), 0.03f));
            
            BuildingList.Add(new Building(game, BuildingTypes["Nekropolis"], new Vector3(650, 20, 750), BuildingTypes["Nekropolis"].Scale));

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
           //Console.WriteLine("false");
           return false;
       }
    }
}