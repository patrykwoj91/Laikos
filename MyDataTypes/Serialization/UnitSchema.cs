using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDataTypes.Serialization
{
    public class UnitSchema
    {
        public String name;
        public float x;
        public float y;

        public UnitSchema()
        {
            name = "";
            x = 0;
            y = 0;
        }

        public UnitSchema(String _name, float _x, float _y)
        {
            name = _name;
            x = _x;
            y = _y;
        }

        public UnitSchema(String _name)
        {
            name = _name;
            x = 0;
            y = 0;
        }
    }
}
