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
            Unselected,
            Selected,
            Interaction,
            Build,
            MoveUnit,
            MoveToBuild,
            GuiUP,
            GuiDOWN
            
            
        };

        static List<Message> lastFrame_messages = new List<Message>();

        static List<Message> currentFrame_messages = new List<Message>();

        public static void CreateMessage(Message message)
        {
            if(!currentFrame_messages.Contains(message))
                currentFrame_messages.Add(message);
        }

        public static void FindMessagesByDestination(GameObject destination, List<Message> result)
        {

            for (var i = 0; i < lastFrame_messages.Count; i++)
            {
                var m = lastFrame_messages[i];
                if (m.Destination != null && m.Destination.Equals(destination))
                {
                    result.Add(m);    
                }
            }

            result.Sort((x, y) => x.Type.CompareTo(y.Type));
        }

        public static void FindMessagesBySender(GameObject sender, List<Message> result)
        {
            for (var i = 0; i < lastFrame_messages.Count; i++)
            {
                var m = lastFrame_messages[i];
                if (m.Sender.Equals(sender))
                {
                    result.Add(m);
                }
            }
            result.Sort((x,y) => x.Type.CompareTo(y.Type));
        }

        public static void FindMessage(Predicate<Message> criteria, List<Message> result)
        {
            for (int i = 0; i < lastFrame_messages.Count; i++)
            {
                Message m = lastFrame_messages[i];
                if (criteria.Invoke(m))
                {
                    result.Add(m);
                }
            }
        }

        public static void Update()
        {
            var t = lastFrame_messages;
            lastFrame_messages = currentFrame_messages;
            currentFrame_messages = t;
            currentFrame_messages.Clear();
        }
    }
}
