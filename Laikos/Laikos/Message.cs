using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Laikos
{
    public class Message 
    {
        public int Type;
        public Object Sender; //nadawca
        public Object Destination; // odbiorca
        public Object Payload; // dodatkowy obiekt np. przeniesienie zasobu od jednostki do budynku
        public bool Done; // czy akcja sie wykonala?
        public TimeSpan time;

        public Message(int type, Object sender, Object destination, Object payload)
        {
            Type = type;
            Sender = sender;
            Destination = destination;
            Payload = payload;
            Done = false;
            time = Game1.time;
        }


        public int CompareTo(Message message)
        {

            if (Type.Equals(message.Type) == false)
                return -1;

            if (message.Sender != null && Sender != null)
            {
                if (Sender.Equals(message.Sender) == false)
                    return -1;
            }
            else if ((message.Sender == null && Sender != null) || (message.Sender != null && Sender == null))
                return -1;

            if (message.Destination != null && Destination != null)
            {
                if (Destination.Equals(message.Destination) == false)
                    return -1;
            }
            else if ((message.Destination == null && Destination != null) || (message.Destination != null && Destination == null))
                return -1;

            return 0;
        }


    }
}



