﻿using System;
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
        public int produktywnosc;
        float timer;         //Initialize a 10 second timer
        const float TIMER = 5;
        public bool dead = false;


        public Building()
            : base()
        {
        }

        public Building(Game game, Player player, BuildingType type, Vector3 position, float scale = 1.0f, bool selectable = false, Vector3 rotation = default(Vector3))
            : base(game, player, type.Model)
        {
            timer = 5;
            Souls = 50;
            produktywnosc = 600;
            this.player = player;
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
            this.game = game;

            Matrix[] modelTransforms = new Matrix[currentModel.Model.Bones.Count];
            currentModel.Model.CopyAbsoluteBoneTransformsTo(modelTransforms);
            foreach (ModelMesh mesh in currentModel.Model.Meshes)
            {
                List<Vector3> meshVertices = new List<Vector3>();
                Matrix transformations = modelTransforms[mesh.ParentBone.Index] * GetWorldMatrix();
                VertexHelper.ExtractModelMeshData(mesh, ref transformations, meshVertices);
                meshBoundingBoxes.Add(BoundingBox.CreateFromPoints(meshVertices));
            }

            ModelExtra modelExtra1 = currentModel.Model.Tag as ModelExtra;
            BoundingSphere originalSphere2 = modelExtra1.boundingSphere;
            boundingBox = BoundingBox.CreateFromSphere(XNAUtils.TransformBoundingSphere(originalSphere2, GetWorldMatrix()));

        }

        public void Update(GameTime gameTime)
        {
            HP = (int)MathHelper.Clamp((float)HP, 0, (float)maxHP);
            if (this.HP <= 0)
            {
                dead = true;
                return;
            }

            if (this.type.Name.Equals("Cementary") || this.type.Name.Equals("Nekropolis"))
            {
                float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                timer -= elapsed;
                if (timer < 0)
                {
                    if (Souls <= 550 && produktywnosc >= 50)
                    {
                        Souls += 50;
                        produktywnosc -= 50;
                        LowerOptionPanel.soulNumbers = Souls;
                      //  Console.WriteLine(LowerOptionPanel.soulNumbers);
                    }
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

          //  Console.WriteLine(lowestPoint + " " + highestPoint);
            if (highestPoint - lowestPoint < 1)
                return true;
            else
                return false;
        }

        public override void HandleEvent(GameTime gameTime)
        {
            EventManager.FindMessagesByDestination(this, messages);
            FindDoubledMessages();
            if (messages.Count > 0)
            {
                int i = 0;
                if (messages[i].Done == false)
                    switch (messages[i].Type)
                    {
                        case (int)EventManager.Events.Selected:
                            selected = true;
                            messages[i].Done = true;
                            GUIEventManager.CreateMessage(new Message((int)GUIEventManager.Events.GuiUP, this, null, null));
                            break;

                        case (int)EventManager.Events.Unselected:
                            selected = false;
                            messages[i].Done = true;
                            GUIEventManager.CreateMessage(new Message((int)GUIEventManager.Events.GuiDOWN, this, null, null));
                            break;

                        case (int)EventManager.Events.Interaction:
                            if (this.type.Name.Equals("Cementary"))
                                foreach (Unit unit in player.UnitList)
                                    if (unit.selected && unit.budowniczy)
                                    {
                                        EventManager.CreateMessage(new Message((int)EventManager.Events.Gather, this, unit, null));
                                        unit.timeSpan = TimeSpan.FromMilliseconds(3000);
                               
                                    }
                            messages[i].Done = true;
                            break;
                        case (int)EventManager.Events.BuildUnit:
                            if (this.type.Name.Equals("BJ Niebian2"))
                            {
                                    Vector3 stay_here = new Vector3(this.Position.X, this.Position.Y, this.Position.Z);

                                    if (MathUtils.RandomNumber(1, 2) == 1) //czy x czy Z
                                    {
                                        if (MathUtils.RandomNumber(1, 2) == 1) // czy + czy -
                                            stay_here.X += BoundingSphere.CreateFromBoundingBox(this.boundingBox).Radius / 4 * 3;


                                        else
                                            stay_here.X -= BoundingSphere.CreateFromBoundingBox(this.boundingBox).Radius / 4 * 3;

                                        stay_here.Z = MathUtils.RandomNumber((int)(stay_here.Z - BoundingSphere.CreateFromBoundingBox(this.boundingBox).Radius / 4 * 3),
                                            (int)(stay_here.Z + BoundingSphere.CreateFromBoundingBox(this.boundingBox).Radius / 4 * 3));

                                    }
                                    else
                                    {
                                        if (MathUtils.RandomNumber(1, 2) == 1) // czy + czy -
                                            stay_here.Z += BoundingSphere.CreateFromBoundingBox(this.boundingBox).Radius / 4 * 3;
                                        else
                                            stay_here.Z -= BoundingSphere.CreateFromBoundingBox(this.boundingBox).Radius / 4 * 3;

                                        stay_here.X = MathUtils.RandomNumber((int)(stay_here.X - BoundingSphere.CreateFromBoundingBox(this.boundingBox).Radius / 4 * 3),
                                            (int)(stay_here.X + BoundingSphere.CreateFromBoundingBox(this.boundingBox).Radius / 4 * 3));
                                    }
                                    

                                player.UnitList.Add(new Unit(game,player, player.UnitTypes["Antigravity Tank"], new Vector3(stay_here.X, 5,stay_here.Z ),player.UnitTypes["Antigravity Tank"].Scale));
                                player.Souls -= player.UnitTypes["Antigravity Tank"].Souls;                        
                                }
                            messages[i].Done = true;
                            break;
                    }
            }
        }
    }
}