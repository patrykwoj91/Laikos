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
    public class BuildingType : ICloneable
    {
        public String Name;
        public Double HP;
        public String Model;
        public int Souls;
        public float Scale;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
