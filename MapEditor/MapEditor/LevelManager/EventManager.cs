using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MapEditor
{
    public static class EventManager
    {
        public enum Events
        {
            FixCollisions,
            ScaleUp,
            ScaleDown,
            Selected,
            Unselected,
            PickBox,
            DeleteBox,
            MoveUnit
        };
    }
}
