
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Animation
{
    public class AnimationData
    {
        /// <summary>
        /// Constructs a new skinning data object.
        /// </summary>
        public AnimationData(Dictionary<string, AnimationClip> animationClips,
                            List<Matrix> bindPose, List<Matrix> inverseBindPose,
                            List<int> skeletonHierarchy, BoundingBox boundingBox)
        {
            AnimationClips = animationClips;
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            SkeletonHierarchy = skeletonHierarchy;
            BoundingBox = boundingBox;
        }


        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private AnimationData()
        {
        }


        /// <summary>
        /// Gets a collection of animation clips. These are stored by name in a
        /// dictionary, so there could for instance be clips for "Walk", "Run",
        /// "JumpReallyHigh", etc.
        /// </summary>
        [ContentSerializer]
        public Dictionary<string, AnimationClip> AnimationClips { get; private set; }


        /// <summary>
        /// Bindpose matrices for each bone in the skeleton,
        /// relative to the parent bone.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> BindPose { get; private set; }


        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> InverseBindPose { get; private set; }


        /// <summary>
        /// For each bone in the skeleton, stores the index of the parent bone.
        /// </summary>
        [ContentSerializer]
        public List<int> SkeletonHierarchy { get; private set; }

        /// <summary>
        /// Contains Bounding sphere for collision detection
        /// </summary>
        [ContentSerializer]
        public BoundingBox BoundingBox { get; private set; }
    }
}
