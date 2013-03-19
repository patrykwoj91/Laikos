using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace Animation
{
    public class Keyframe //klasa opisuje animacje (transformacje kosci i czas) danej klatki.
    {
        [ContentSerializer]
        public int Bone { get; private set; } // index animowanej kosci
        [ContentSerializer]
        public TimeSpan Time { get; private set; } //offset czasu od poczatku animacji do tego momentu
        [ContentSerializer]
        public Matrix Transform { get; private set; } //transformacja kosci 

        public Keyframe(int bone, TimeSpan time, Matrix transform)
        {
            Bone = bone;
            Time = time;
            Transform = transform;
        }

        private Keyframe() //destruktor do deserializacji 
        {
        }


    }
}
