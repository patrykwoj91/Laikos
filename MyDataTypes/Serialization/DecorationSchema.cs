using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDataTypes.Serialization
{
    public class DecorationSchema
    {
        public String name;
        public float x;
        public float y;

        public DecorationSchema()
        {
            name = "";
            x = 0;
            y = 0;
        }

        public DecorationSchema(String _name, float _x, float _y)
        {
            name = _name;
            x = _x;
            y = _y;
        }

        public DecorationSchema(String _name)
        {
            name = _name;
            x = 0;
            y = 0;
        }
    }
}
