using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Laikos
{
    public class EventManager : GameComponent
    {
        enum Events
        {
            FixCollisions,
            ScaleUp,
            Heal
        };

        List<Message> messages;


        public EventManager(Game game) : base(game)
        {
            messages = new List<Message>();
        }

        public void CreateMessage(Message message)
        {
            messages.Add(message);
        }

        public void FindMessageByDestination(GameObject destinationObject, List<Message> result)
        {
            for (int i = 0; i < result.Count; i++)

        }



        public override void Update(GameTime gameTime)
        {
            
            base.Update(gameTime);
        }
    }
}
