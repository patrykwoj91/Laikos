using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Animation
{
    public class AnimationClip //zawiera wszystkie klatki i czas trwania pojedynczej animacji
    {
        [ContentSerializer]
        public TimeSpan Duration { get; private set; } //czas calej animacji
        [ContentSerializer]
        public List<Keyframe> Keyframes { get; private set; } //lista kolejnych klatek dla wszystkich kosci 

        public AnimationClip(TimeSpan duration, List<Keyframe> keyframes)
        {
            Duration = duration;
            Keyframes = keyframes;
        }

        private AnimationClip()
        {
        }
    }
}
