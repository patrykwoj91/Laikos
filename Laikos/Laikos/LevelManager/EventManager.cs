using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Laikos
{
    public static class EventManager
    {
        public enum Events
        {
            FixCollisions,
            ScaleUp,
            ScaleDown,
            Heal
        };

        static List<Message> messages = new List<Message>();

        public static void CreateMessage(Message message)
        {
            messages.Add(message);
        }

        public static void ClearMessages()
        {
            messages.Clear();
        }

        public static void FindMessageByDestination(GameObject destinationObject, List<Message> result)
        {
            for (int i = 0; i < messages.Count; i++)
                if (messages[i].Destination.Equals(destinationObject))
                    result.Add(messages[i]);
        }

        public static void FindMessageBySender(GameObject senderObject, List<Message> result)
        {
            for (int i = 0; i < messages.Count; i++)
                if(messages[i].Destination.Equals(senderObject))
                    result.Add(messages[i]);
        }
    }
}
