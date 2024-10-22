﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDataTypes.Serialization
{
    public class BuildingSchema
    {
        public String name;
        public float x;
        public float y;

        public float scale;
        public float rotation;

        public BuildingSchema()
        {
            name = "";
            x = 0;
            y = 0;

            scale = 0;
            rotation = 0;
        }

        public BuildingSchema(String _name, float _x, float _y, float _scale, float _rotation)
        {
            name = _name;
            x = _x;
            y = _y;
            scale = _scale;
            rotation = _rotation;
        }

        public BuildingSchema(String _name)
        {
            name = _name;
            x = 0;
            y = 0;
            scale = 0;
            rotation = 0;
        }
    }
}
