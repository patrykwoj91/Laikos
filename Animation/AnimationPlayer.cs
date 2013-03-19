using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Animation
{ 
    //zajmuje sie przetwazaniem matrixow kosci z animation clip
    public class AnimationPlayer
    {
        //informacje o bierzacej animacji
        AnimationClip currentClipValue; //bieżacy klip
        TimeSpan currentTimeValue; //bieżacy czas
        int currentKeyFrame; //bieżaca klatka

        //matrixy swiata, kosci , i skory
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;
        Matrix[] boneTransforms;

        //dane o bindposie i hierarchi kosci szkieletu
        AnimationData animationDataValue;

        public AnimationPlayer(AnimationData animationData)
        {
            if (animationData == null)
                throw new ArgumentNullException("animationData");

            animationDataValue = animationData;

            worldTransforms = new Matrix[animationData.BindPose.Count];
            skinTransforms = new Matrix[animationData.BindPose.Count];
            boneTransforms = new Matrix[animationData.BindPose.Count];
        }

        public void StartClip(AnimationClip clip)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            currentClipValue = clip;
            currentTimeValue = TimeSpan.Zero;
            currentKeyFrame = 0;

            //transfomacja kosci do bind pose
            animationDataValue.BindPose.CopyTo(boneTransforms, 0);
        }

        public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        {
            if (relativeToCurrentTime) //updatuje pozycje animacji
            {
                time += currentTimeValue;

                //na koniec wroc do startu
                while (time >= currentClipValue.Duration)
                    time -= currentClipValue.Duration;
            }

            //jesli pozycja ruszyla wstecz, resetuj index klatki
            if (time < currentTimeValue)
            {
                currentKeyFrame = 0;
                animationDataValue.BindPose.CopyTo(boneTransforms, 0);
            }

            currentTimeValue = time;

            //odczyt klatek
            IList<Keyframe> keyframes = currentClipValue.Keyframes;

            while (currentKeyFrame < keyframes.Count)
            {
                Keyframe keyframe = keyframes[currentKeyFrame];

                //stop jesli doszlo do bierzacego czasu
                if (keyframe.Time > currentTimeValue)
                    break;

                boneTransforms[keyframe.Bone] = keyframe.Transform;
                currentKeyFrame++;
            }
        }
        //update world transforms
        public void UpdateWorldTransforms(Matrix rootTransform)
        {
            //root
            worldTransforms[0] = boneTransforms[0] * rootTransform;

            //dzieci
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = animationDataValue.SkeletonHierarchy[bone];

                worldTransforms[bone] = boneTransforms[bone] * worldTransforms[parentBone];

            }
        }

        //update skin transforms
        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = animationDataValue.InverseBindPose[bone] * worldTransforms[bone];
            }
        }

        public void Update(TimeSpan time, bool relativeToCurrentTime, Matrix rootTransform)
        {
            UpdateBoneTransforms(time, relativeToCurrentTime);
            UpdateWorldTransforms(rootTransform);
            UpdateSkinTransforms();
        }

        public Matrix[] GetBoneTransforms()
        {
            return boneTransforms;
        }

        /// Zwraca tranformacje kosci absolute.
        public Matrix[] GetWorldTransforms()
        {
            return worldTransforms;
        }

        // transformacje kosci w zaleznosci od bind pose
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }

        // zwraca dekodowany klip
        public AnimationClip CurrentClip
        {
            get { return currentClipValue; }
        }

        // zwraca obecny odtwarzany czas
        public TimeSpan CurrentTime
        {
            get { return currentTimeValue; }
        }
    }
}
