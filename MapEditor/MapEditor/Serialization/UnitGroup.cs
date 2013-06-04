using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laikos.Serialization
{
    public class UnitGroup
    {
        public String name;
        public List<UnitSchema> units;

        public UnitGroup()
        {
            name = "";
            units = new List<UnitSchema>();
        }

        public UnitGroup(String _name)
        {
            name = _name;
            units = new List<UnitSchema>();
        }
    }
}
