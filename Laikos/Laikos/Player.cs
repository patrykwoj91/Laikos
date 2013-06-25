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

        public Dictionary<String, UnitType> UnitTypes;
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


            UpdateExplosions(gameTime);
            UpdateExplosionSmoke(gameTime);
        }

        public void Initialize()
        {
            Souls = 250;
        }

        public bool Build(BuildingType building, Vector3 position)
        {
            if (Souls >= building.Souls)
            {
                BuildingList.Add(new Building(game, this, building, position, building.Scale));
                Souls -= building.Souls;
               // Console.WriteLine(Souls);
                return true;
            }
            return false;
        }

        void UpdateExplosions(GameTime gameTime)
        {
            for (int i = UnitList.Count - 1; i >= 0; i--)
            {
                if (UnitList[i].HP <= 0)
                {
                    DefferedRenderer.explosionParticles.AddParticle(UnitList[i].Position, Vector3.Zero);
                    DefferedRenderer.explosionSmokeParticles.AddParticle(UnitList[i].Position, Vector3.Zero);
                    UnitList.RemoveAt(i);
                }
            }
            for (int i = BuildingList.Count - 1; i >= 0; i--)
            {
                if (BuildingList[i].HP <= 0)
                {
                    DefferedRenderer.explosionParticles.AddParticle(BuildingList[i].Position, Vector3.Zero);
                    DefferedRenderer.explosionSmokeParticles.AddParticle(BuildingList[i].Position, Vector3.Zero);
                    BuildingList.RemoveAt(i);

                }
            }
            DefferedRenderer.explosionParticles.Update(gameTime);
        }

        void UpdateExplosionSmoke(GameTime gameTime)
        {
            for (int i = UnitList.Count - 1; i >= 0; i--)
            {
                if (100 * UnitList[i].HP / UnitList[i].maxHP <= 30)
                {
                    DefferedRenderer.SmokePlumeParticles.AddParticle(UnitList[i].Position, Vector3.Zero);
                }
                DefferedRenderer.explosionSmokeParticles.Update(gameTime);
            }
        }
    }
}