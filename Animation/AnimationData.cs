
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Animation
{
    public class AnimationData //sklada wszystkie dane do wyrenderowania i animacji obiektu
    {
        [ContentSerializer]
        public Dictionary<string, AnimationClip> AnimationClips { get; private set; } //kolekcja clipow animacji po nazwach "Take 001" etc.
        [ContentSerializer]
        public List<Matrix> BindPose { get; private set; } //macierze bindpose dla kazdej kosci szkieletu 
        [ContentSerializer]
        public List<Matrix> InverseBindPose { get; private set; } //macierze transformacji vertexow do bone space
        [ContentSerializer]
        public List<int> SkeletonHierarchy { get; private set; } //dla kazdej kosci index kosci rodzica (hierarchia szkieletu)
        
        public AnimationData(Dictionary <string, AnimationClip> animationClips, List<Matrix> bindPose, List<Matrix> inverseBindPose, List<int> skeletonHierarchy)
        {
            AnimationClips = animationClips;
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            SkeletonHierarchy = skeletonHierarchy;
        }

        private AnimationData()
        {
        }
    }
}
