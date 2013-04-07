using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Animation
{
    public class AnimationEvent //informacje dotyczace naszych eventow
    {
        public String EventName
        {
            get;
            set;
        }

        public TimeSpan EventTime //czas 
        {
            get;
            set;
        }
    }
}
