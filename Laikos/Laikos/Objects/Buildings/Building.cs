using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Animation;
using MyDataTypes;

namespace Laikos
{
   public class Building : GameObject
    {
       public BuildingType type;
       public List<BoundingBox> meshBoundingBoxes;
       public double HP;
       public double maxHP;
       public BoundingBox boundingBox;
       public bool selectable;
       public float buildtime;
       public int Souls;
       float timer;         //Initialize a 10 second timer
       const float TIMER = 10;


       public Building()
           :base()
       {
       }

       public Building(Game game,Player player, BuildingType type, Vector3 position, float scale = 1.0f, bool selectable = false, Vector3 rotation = default(Vector3))
            : base(game,player, type.Model)
        {
            timer = 10;
            Souls = 100;
           this.Position = position;
           this.Rotation = rotation;
           this.Scale = scale;
           this.type = (BuildingType)type.Clone();
           this.messages = new List<Message>();
           this.meshBoundingBoxes = new List<BoundingBox>();
           maxHP = this.type.maxhp;
           HP = maxHP;
           this.selectable = selectable;
           buildtime = this.type.buildtime;
      
           Matrix[] modelTransforms = new Matrix[currentModel.Model.Bones.Count];
           currentModel.Model.CopyAbsoluteBoneTransformsTo(modelTransforms);
           foreach (ModelMesh mesh in currentModel.Model.Meshes)
           {
               List<Vector3> meshVertices = new List<Vector3>();
               Matrix transformations = modelTransforms[mesh.ParentBone.Index] * GetWorldMatrix();
               VertexHelper.ExtractModelMeshData(mesh, ref transformations, meshVertices);
               meshBoundingBoxes.Add(BoundingBox.CreateFromPoints(meshVertices));
           }
        }

        public void Update(GameTime gameTime)
        {
            HP = (int)MathHelper.Clamp((float)HP, 0, (float)maxHP);
            if (this.type.Name.Equals("Cementary") || this.type.Name.Equals("Nekropolis"))
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                    timer -= elapsed;
                        if (timer < 0)
                        {
                            if (Souls <= 598)
                                Souls += 2;
                            timer = TIMER;   //Reset Timer
                        }
            }

            HandleEvent(gameTime);
            this.CleanMessages();
            base.Update(gameTime);
        }

        public bool checkIfPossible(Vector3 startPosition)
        {
            BoundingBox box = XNAUtils.TransformBoundingBox(Collisions.GetBoundingBox(currentModel.Model), GetWorldMatrix());
            Vector3 size = box.Max - box.Min;
            float lowestPoint = float.MaxValue;
            float highestPoint = float.MinValue;
                
            for (int i = (int)startPosition.X; i < size.X + startPosition.X; i++)
            {
                 for (int j = (int)startPosition.Z; j < size.Z + startPosition.Z; j++)
                 {
                     if (Terrain.GetClippedHeightAt(i, j) < lowestPoint) lowestPoint = Terrain.GetClippedHeightAt(i, j);
                     if (Terrain.GetClippedHeightAt(i, j) > lowestPoint) highestPoint = Terrain.GetClippedHeightAt(i, j);
                 }
            }

            Console.WriteLine(lowestPoint + " " + highestPoint);
            if (highestPoint - lowestPoint < 1)
                return true;
            else
                return false;
        }

        public override void HandleEvent(GameTime gameTime)
        {
            EventManager.FindMessagesByDestination(this, messages);
            FindDoubledMessages();
            LowerOptionPanel.soulNumbers = Souls;
            if (messages.Count > 0)
            {
                int i = 0;
                if (messages[i].Done == false)
                switch (messages[i].Type)
                {
                    case (int)EventManager.Events.Selected:
                        selected = true;
                        messages[i].Done = true;
                        EventManager.CreateMessage(new Message((int)EventManager.Events.GuiUP, this, null, null));
                        break;

                    case (int)EventManager.Events.Unselected:
                        selected = false;
                        messages[i].Done = true;
                        EventManager.CreateMessage(new Message((int)EventManager.Events.GuiDOWN, this, null, null));
                        break;

                    case (int)EventManager.Events.Interaction:
                        if (this.type.Name.Equals("Cementary"))
                            foreach (Unit unit in player.UnitList)
                                if (unit.selected && unit.budowniczy)
                                {
                                    EventManager.CreateMessage(new Message((int)EventManager.Events.Gather, this, unit, null));
                                    unit.timeSpan = TimeSpan.FromMilliseconds(3000);
                                    unit.walk = true;
                                }

                        messages[i].Done = true;
                        break;
                }
            }
        }
    }
}