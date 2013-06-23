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
        public float Scale;
        public float rotation;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
