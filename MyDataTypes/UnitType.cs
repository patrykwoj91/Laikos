using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace MyDataTypes
{
    public class UnitType : ICloneable
    {
        public String race;
        public String name;
        public Double maxhp;
        public Double mp;
        public Int32 damage;
        public Int32 range;
        public Int32 ratio;
        public int Souls;
        public float Scale;
        public String model;
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
