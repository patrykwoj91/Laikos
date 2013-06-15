using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDataTypes.Serialization
{
    public class ObjectsSchema
    {
        public List<UnitGroup> unitGroups;
        public List<BuildingGroup> buildingsGroups;
        public List<DecoarationGroup> decorationsGroups;

        public ObjectsSchema()
        {
            unitGroups = new List<UnitGroup>();
            buildingsGroups = new List<BuildingGroup>();
            decorationsGroups = new List<DecoarationGroup>();
        }
    }
}
