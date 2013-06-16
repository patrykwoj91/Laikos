using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDataTypes.Serialization
{
    public class ObjectsSchema
    {
        public List<UnitGroup> unitGroups_1;
        public List<UnitGroup> unitGroups_2;

        public List<BuildingGroup> buildingsGroups_1;
        public List<BuildingGroup> buildingsGroups_2;

        public List<DecoarationGroup> decorationsGroups;

        public ObjectsSchema()
        {
            unitGroups_1 = new List<UnitGroup>();
            unitGroups_2 = new List<UnitGroup>();

            buildingsGroups_1 = new List<BuildingGroup>();
            buildingsGroups_2 = new List<BuildingGroup>();

            decorationsGroups = new List<DecoarationGroup>();
        }
    }
}