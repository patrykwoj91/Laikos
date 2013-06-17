using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDataTypes
{
    public class DecorationType
    {
        public String name;
        public String model;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
