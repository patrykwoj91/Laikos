using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDataTypes.Serialization
{
    public class DecoarationGroup
    {
        public String name;
        public List<DecorationSchema> decorations;

        public DecoarationGroup()
        {
            name = "";
            decorations = new List<DecorationSchema>();
        }

        public DecoarationGroup(String _name)
        {
            name = _name;
            decorations = new List<DecorationSchema>();
        }
    }
}
