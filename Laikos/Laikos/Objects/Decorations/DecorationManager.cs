﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Animation;


namespace Laikos
{
    class DecorationManager : DrawableGameComponent
    {
        public List<Decoration> DecorationList;
        public Dictionary<String, Model> ModelList;

        public DecorationManager(Game game)
            : base(game)
        {
            ModelList = new Dictionary<String, Model>();
            DecorationList = new List<Decoration>();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //tu z pliku bedziemy sciezki do modeli wczytywac do listy modeli (na razie recznie)
            String path = "Models/Decorations/Ruins1/Ruins";
            ModelList.Add("ruin1", Game.Content.Load<Model>(path));
            DecorationList.Add(new Decoration(ModelList["ruin1"]));
        }

        public override void Update(GameTime gameTime)
        {

           foreach (Decoration decoration in DecorationList)
            {
            decoration.Update(gameTime);
            }
        
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (Decoration decoration in DecorationList)
            {
                decoration.Draw();
            }
        }



    }
}