using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laikos
{
    public class Message
    {
        public int Type;
        public Object Sender; //nadawca
        public Object Destination; // odbiorca
        public Object Payload; // dodatkowy obiekt np. przeniesienie zasobu od jednostki do budynku
        public bool Done; // czy akcja sie wykonala?

        public Message(int type, Object sender, Object destination, Object payload)
        {
            Type = type;
            Sender = sender;
            Destination = destination;
            Payload = payload;
            Done = false;
        }
    }
}
