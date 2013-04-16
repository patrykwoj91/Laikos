using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Laikos
{
    public class EventManager
    {
        enum Events
        {
            FixCollisions,
            ScaleUp,
            Heal
        };

        List<Message> messages;


        public EventManager(Game game)
        {
            messages = new List<Message>();
        }

        public void CreateMessage(Message message)
        {
            messages.Add(message);
        }

        public void FindMessageByDestination(GameObject destinationObject, List<Message> result)
        {
            for (int i = 0; i < messages.Count; i++)
                if (messages[i].Destination.Equals(destinationObject))
                    result.Add(messages[i]);
        }

        public void FindMessageBySender(GameObject senderObject, List<Message> result)
        {
            for (int i = 0; i < messages.Count; i++)
                if(messages[i].Destination.Equals(senderObject))
                    result.Add(messages[i]);
        }
    }
}
