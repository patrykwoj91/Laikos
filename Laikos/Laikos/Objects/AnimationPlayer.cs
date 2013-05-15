using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Animation;
using System.ComponentModel;
using Microsoft.Xna.Framework;


namespace Laikos
{
    /// <summary>
    /// Animation clip player. It maps an animation clip onto a model
    /// </summary>
    public class AnimationPlayer
    {
        #region Fields

        /// <summary>
        /// position in time in the clip
        /// </summary>
        private float current_position = 0;
        private float next_position = 0;
        /// <summary>
        /// The clips
        /// </summary>
        public AnimationClip current_clip = null;
        public AnimationClip next_clip = null;

        /// <summary>
        /// The clip we are playing
        /// </summary>
        public Dictionary<String, AnimationClip> Clips = null;

        /// <summary>
        /// We maintain a BoneInfo class for each bone. This class does
        /// most of the work in playing the animation.
        /// </summary>
        private BoneInfo[] current_boneInfos;
        private BoneInfo[] next_boneInfos;
        private BoneInfo[] blended_boneInfos; //tu wrzucac wszystko i tym sterowac caloscia 
        /// <summary>
        /// The number of bones
        /// </summary>
        private int boneCnt;
        private int boneCnt2;

        /// <summary>
        /// An assigned model
        /// </summary>
        private AnimatedModel model = null;

        /// <summary>
        /// The looping option
        /// </summary>
        private bool looping = false;

        /// <summary>
        /// How much to blend by
        /// </summary>
        public double totalblendTime = 1; //miliseconds
        public float currentblendTime;
        public AnimationClip.Bone[] blendedBones;




        #endregion

        #region Properties

        public float Position
        {
            get
            {
                if (next_clip != null) return next_position;
                else return current_position;
            }
            set
            {
                
                if (next_clip != null)
                {
                    if (value > Duration)
                        value = (float)Duration;
                    next_position = value;
                    foreach (BoneInfo bone in next_boneInfos)
                    {
                        bone.SetPosition(next_position);
                    }
                }
                else
                {
                    if (value > Duration)
                        value = (float)Duration;
                    current_position = value;
                    foreach (BoneInfo bone in current_boneInfos)
                    {
                        bone.SetPosition(current_position);
                    }
                }
            }
        }

        /// <summary>
        /// The clip duration
        /// </summary>
        [Browsable(false)]
        public double Duration
        {
            get
            {
                if (next_clip != null)
                    return next_clip.Duration;
                else
                    return current_clip.Duration;
            }
        }

        /// <summary>
        /// A model this animation is assigned to. It will play on that model.
        /// </summary>
        [Browsable(false)]
        public AnimatedModel Model { get { return model; } }

        /// <summary>
        /// The looping option. Set to true if you want the animation to loop
        /// back at the end
        /// </summary>
        public bool Looping { get { return looping; } set { looping = value; } }

        #endregion

        #region Construction

        /// <summary>
        /// Constructor for the animation player. It makes the 
        /// association between a clip and a model and sets up for playing
        /// </summary>
        /// <param name="clip"></param>
        public AnimationPlayer(Dictionary<String, AnimationClip> Clips, AnimatedModel model)
        {
            blendedBones = new AnimationClip.Bone[model.Bones.Count];
                
            this.Clips = Clips;
            this.model = model;
            current_clip = Clips["Take 001"];

            looping = true;

            // Create the bone information classes
            boneCnt = current_clip.Bones.Count;

            current_boneInfos = new BoneInfo[boneCnt];

            for (int b = 0; b < current_boneInfos.Length; b++)
            {
                // Create it
                current_boneInfos[b] = new BoneInfo(current_clip.Bones[b]);

                // Assign it to a model bone
                current_boneInfos[b].SetModel(model);
            }

            Rewind();
        }

        #endregion

        #region Update and Transport Controls


        /// <summary>
        /// Reset back to time zero.
        /// </summary>
        public void Rewind()
        {
            Position = 0;
        }

        /// <summary>
        /// Update the clip position
        /// </summary>
        /// <param name="delta"></param>
        public void Update(GameTime gameTime)
        {
            #region Update current_clip
            Position = Position + (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (looping && Position >= Duration)
                Position = 0;
            #endregion  

            //if not blending, copy current transforms;
            if (next_clip == null)
            {
                #region Copying
                for (int i = 0; i < current_clip.Bones.Count(); i++)
                {
                    blendedBones[i] = new AnimationClip.Bone();
                    blendedBones[i].Name = current_clip.Bones[i].Name;
                    for (int j = 0; j < current_clip.Bones[i].Keyframes.Count(); j++)
                    {
                        blendedBones[i].Keyframes.Add(new AnimationClip.Keyframe());
                        blendedBones[i].Keyframes[j].Rotation = current_clip.Bones[i].Keyframes[j].Rotation;
                        blendedBones[i].Keyframes[j].Translation = current_clip.Bones[i].Keyframes[j].Translation;
                        blendedBones[i].Keyframes[j].Time = current_clip.Bones[i].Keyframes[j].Time;
                    }
                }
                #endregion
                return;
            }
            
            //if we get there, means that we blending WOW!
            if (Position < Duration)
            {
                
                if (currentblendTime > (float)Duration) //we try to catch , where are we in current animation
                    currentblendTime = 0;
                else
                    currentblendTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                
            }

            float blendAmount = currentblendTime / (float)totalblendTime;
            Console.WriteLine(blendAmount);
            if (blendAmount >= 1.0f)
            {
                current_clip = next_clip;
                #region Copying
                for (int i = 0; i < current_clip.Bones.Count(); i++)
                {
                    blendedBones[i] = new AnimationClip.Bone();
                    blendedBones[i].Name = current_clip.Bones[i].Name;
                    for (int j = 0; j < current_clip.Bones[i].Keyframes.Count(); j++)
                    {
                        blendedBones[i].Keyframes.Add(new AnimationClip.Keyframe());
                        blendedBones[i].Keyframes[j].Rotation = current_clip.Bones[i].Keyframes[j].Rotation;
                        blendedBones[i].Keyframes[j].Translation = current_clip.Bones[i].Keyframes[j].Translation;
                        blendedBones[i].Keyframes[j].Time = current_clip.Bones[i].Keyframes[j].Time;
                    }
                }
                #endregion
                next_clip = null;
                return;
            }
            Quaternion currentRotation, nextRotation, blendedRotation;
            Vector3 currentTranslation, nextTranslation, blendedTranslation;

            for (int i = 0; i < blendedBones.Count(); i++)
            {
                blendedBones[i] = new AnimationClip.Bone();
                blendedBones[i].Name = current_clip.Bones[i].Name;
                for (int j = 0; j < blendedBones[i].Keyframes.Count(); j++)
                {
                    blendedBones[i].Keyframes.Add(new AnimationClip.Keyframe());
                    currentRotation = current_clip.Bones[i].Keyframes[j].Rotation;
                    currentTranslation = current_clip.Bones[i].Keyframes[j].Translation;
                    nextRotation = next_clip.Bones[i].Keyframes[j].Rotation;
                    nextTranslation = next_clip.Bones[i].Keyframes[j].Translation;

                    Quaternion.Slerp(ref currentRotation, ref nextRotation, blendAmount, out blendedRotation);
                    Vector3.Lerp(ref currentTranslation, ref nextTranslation, blendAmount, out blendedTranslation);


                    blendedBones[i].Keyframes[j].Rotation = blendedRotation;
                    blendedBones[i].Keyframes[j].Translation = blendedTranslation;
                    blendedBones[i].Keyframes[j].Time = current_clip.Bones[i].Keyframes[j].Time;
                }
            }


        }

        #endregion

        #region Playing

        public void PlayClip(String name, Boolean looping)
        {
            this.looping = looping;
            next_clip = Clips[name];
            currentblendTime = 0;

            // Create the bone information classes
            boneCnt2 = next_clip.Bones.Count;

            next_boneInfos = new BoneInfo[boneCnt];

            for (int b = 0; b < next_boneInfos.Length; b++)
            {
                // Create it

                next_boneInfos[b] = new BoneInfo(current_clip.Bones[b]);

                // Assign it to a model bone
                next_boneInfos[b].SetModel(model);
            }
            Rewind();
        }
        #endregion

        #region BoneInfo class


        /// <summary>
        /// Information about a bone we are animating. This class connects a bone
        /// in the clip to a bone in the model.
        /// </summary>
        private class BoneInfo
        {
            #region Fields

            /// <summary>
            /// The current keyframe. Our position is a time such that the 
            /// we are greater than or equal to this keyframe's time and less
            /// than the next keyframes time.
            /// </summary>
            private int currentKeyframe = 0;

            /// <summary>
            /// Bone in a model that this keyframe bone is assigned to
            /// </summary>
            private Bone assignedBone = null;

            /// <summary>
            /// We are not valid until the rotation and translation are set.
            /// If there are no keyframes, we will never be valid
            /// </summary>
            public bool valid = false;

            /// <summary>
            /// Current animation rotation
            /// </summary>
            private Quaternion rotation;

            /// <summary>
            /// Current animation translation
            /// </summary>
            public Vector3 translation;

            /// <summary>
            /// We are at a location between Keyframe1 and Keyframe2 such 
            /// that Keyframe1's time is less than or equal to the current position
            /// </summary>
            public AnimationClip.Keyframe Keyframe1;

            /// <summary>
            /// Second keyframe value
            /// </summary>
            public AnimationClip.Keyframe Keyframe2;

            #endregion

            #region Properties

            /// <summary>
            /// The bone in the actual animation clip
            /// </summary>
            public AnimationClip.Bone ClipBone { get; set; }

            /// <summary>
            /// The bone this animation bone is assigned to in the model
            /// </summary>
            public Bone ModelBone { get { return assignedBone; } }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="bone"></param>
            public BoneInfo(AnimationClip.Bone bone)
            {
                this.ClipBone = bone;
                SetKeyframes();
                SetPosition(0);
            }


            #endregion

            #region Position and Keyframes

            /// <summary>
            /// Set the bone based on the supplied position value
            /// </summary>
            /// <param name="position"></param>
            public void SetPosition(float position)
            {
                List<AnimationClip.Keyframe> keyframes = ClipBone.Keyframes;
                if (keyframes.Count == 0)
                    return;

                // If our current position is less that the first keyframe
                // we move the position backward until we get to the right keyframe
                while (position < Keyframe1.Time && currentKeyframe > 0)
                {
                    // We need to move backwards in time
                    currentKeyframe--;
                    SetKeyframes();
                }

                // If our current position is greater than the second keyframe
                // we move the position forward until we get to the right keyframe
                while (position >= Keyframe2.Time && currentKeyframe < ClipBone.Keyframes.Count - 2)
                {
                    // We need to move forwards in time
                    currentKeyframe++;
                    SetKeyframes();
                }

                if (Keyframe1 == Keyframe2)
                {
                    // Keyframes are equal
                    rotation = Keyframe1.Rotation;
                    translation = Keyframe1.Translation;
                }
                else
                {
                    // Interpolate between keyframes
                    float t = (float)((position - Keyframe1.Time) / (Keyframe2.Time - Keyframe1.Time));
                    rotation = Quaternion.Slerp(Keyframe1.Rotation, Keyframe2.Rotation, t);
                    translation = Vector3.Lerp(Keyframe1.Translation, Keyframe2.Translation, t);
                }

                valid = true;
                if (assignedBone != null)
                {
                    // Send to the model
                    // Make it a matrix first
                    Matrix m = Matrix.CreateFromQuaternion(rotation);
                    m.Translation = translation;
                    assignedBone.SetCompleteTransform(m);

                }
            }



            /// <summary>
            /// Set the keyframes to a valid value relative to 
            /// the current keyframe
            /// </summary>
            private void SetKeyframes()
            {
                if (ClipBone.Keyframes.Count > 0)
                {
                    Keyframe1 = ClipBone.Keyframes[currentKeyframe];
                    if (currentKeyframe == ClipBone.Keyframes.Count - 1)
                        Keyframe2 = Keyframe1;
                    else
                        Keyframe2 = ClipBone.Keyframes[currentKeyframe + 1];
                }
                else
                {
                    // If there are no keyframes, set both to null
                    Keyframe1 = null;
                    Keyframe2 = null;
                }
            }

            /// <summary>
            /// Assign this bone to the correct bone in the model
            /// </summary>
            /// <param name="model"></param>
            public void SetModel(AnimatedModel model)
            {
                // Find this bone
                assignedBone = model.FindBone(ClipBone.Name);

            }

            #endregion
        }

        #endregion

    }
}
