using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Laikos.Serialization
{
    public class BuildingGroup
    {
        public String name;
        public List<BuildingSchema> buildings;

        public BuildingGroup()
        {
            name = "";
            buildings = new List <BuildingSchema> ();
        }

        public BuildingGroup(String _name)
        {
            name = _name;
            buildings = new List<BuildingSchema>();
        }
    }
}
