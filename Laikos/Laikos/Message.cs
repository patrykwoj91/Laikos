using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laikos
{
    public class Message
    {
        public string Type;
        public GameObject Sender; //nadawca
        public GameObject Destination; // odbiorca
        public GameObject Payload; // dodatkowy obiekt np. przeniesienie zasobu od jednostki do budynku

        public Message(string type, GameObject sender, GameObject destination, GameObject payload)
        {
            Type = type;
            Sender = sender;
            Destination = destination;
            Payload = payload;
        }
    }
}
