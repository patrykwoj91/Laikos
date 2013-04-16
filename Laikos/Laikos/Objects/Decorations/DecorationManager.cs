using System;
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
        public GraphicsDevice device;
        public Game game;
        GraphicsDeviceManager graphics;
        List<Message> messages;

        public DecorationManager(Game game, GraphicsDevice device, GraphicsDeviceManager graphics)
            : base(game)
        {
            this.game = game;
            this.graphics = graphics;
            DecorationList = new List<Decoration>();
            // ModelList = new Dictionary<String, Model>();
            this.device = device;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //tu z pliku bedziemy sciezki do modeli wczytywac do listy modeli (na razie recznie)
            String path = "Models/Decorations/Ruins2/Ruins2"; 
            DecorationList.Add(new Decoration(game, path, new Vector3(30, 0, 150), 1.5f));
            path = "Models/Decorations/chest";
            DecorationList.Add(new Decoration(game, path, new Vector3(30, 0, 50), 1.5f));
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Decoration unit in DecorationList)
            {
                
                unit.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (Decoration unit in DecorationList)
            {
                unit.Draw(graphics);
            }
        }

    }
}
