using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laikos.Serialization
{
    public class BuildingSchema
    {
        public String name;
        public float x;
        public float y;

        public BuildingSchema()
        {
            name = "";
            x = 0;
            y = 0;
        }

        public BuildingSchema(String _name, float _x, float _y)
        {
            name = _name;
            x = _x;
            y = _y;
        }

        public BuildingSchema(String _name)
        {
            name = _name;
            x = 0;
            y = 0;
        }
    }
}
