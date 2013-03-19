using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Animation;

namespace AnimationProcessor
{
    [ContentProcessor]
    public class AnimationProcessor : ModelProcessor
    {

        static AnimationClip ProcessAnimation(AnimationContent animation, Dictionary<string, int> boneMap)
        {
            List<Keyframe> keyframes = new List<Keyframe>();

            foreach (KeyValuePair<string, AnimationChannel> channel in animation.Channels)
            {
                //sprawdz jaka kosc kontroluje ten channel
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    throw new InvalidContentException(string.Format("Fount animation for bone '{0}', " + "which is not part of the skeleton.", channel.Key));
                }

                //konwertuje dane z klatek
                foreach (AnimationKeyframe keyframe in channel.Value)
                {
                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time, keyframe.Transform));
                }
            }

            //sortowanie polaczonych klatek po czasie
            keyframes.Sort(CompareKeyframesTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new AnimationClip(animation.Duration, keyframes);
        }

        static int CompareKeyframesTimes(Keyframe a, Keyframe b)
        {
            return a.Time.CompareTo(b.Time);
        }

        static void ValidateMesh(NodeContent node, ContentProcessorContext context, string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                //waliduj
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null, "Mesh {0} is a child of bone {1}. AnimationProcessor " + "does not currently handle meshes that are children of bones.", mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null, "Mesh {0} has no skinning information, so it has been deleted.", mesh.Name);
                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                //jesli to kosc pamietaj ze patrzysz do srodka
                parentBoneName = node.Name;
            }

            //rekurencja
            //iteruje na kopii childrenow 
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);

        }

        static bool MeshHasSkinning(MeshContent mesh) //czy mesh zawiera informacje ?
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }
            return true;
        }

        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {

                //pomin szkielet
                if (child == skeleton)
                    continue;

                //bake local transforms
                MeshHelper.TransformScene(child, child.Transform);

                //ustaw system koordynatow na identity
                child.Transform = Matrix.Identity;

                //rekurencja
                FlattenTransforms(child, skeleton);
            }
        }

        //materialy uzywac beda skinned efektu
        [DefaultValue(MaterialProcessorDefaultEffect.SkinnedEffect)]
        public override MaterialProcessorDefaultEffect DefaultEffect
        {
            get
            {
                return MaterialProcessorDefaultEffect.SkinnedEffect;
            }
            set
            {

            }
        }

        static Dictionary<string, AnimationClip> ProcessAnimations(AnimationContentDictionary animations, IList<BoneContent> bones)
        {
            //mapa kosci i nazw
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            Dictionary<string, AnimationClip> animationClips;
            animationClips = new Dictionary<string, AnimationClip>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations)
            {
                AnimationClip processed = ProcessAnimation(animation.Value, boneMap);
                animationClips.Add(animation.Key, processed);
            }

            if (animationClips.Count == 0)
            {
                throw new InvalidContentException("Input file does not contain any animations.");
            }

            return animationClips; //tu mozna sprawdzic ile animacji ma nasz model
        }

        //konwersja nodecontent do model contentu objectu z wtopiona animacja
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ValidateMesh(input, context, null);

            //szukamy szkieletu
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            //bake'ujemy all zeby nie rozjechaly sie koordynaty
            FlattenTransforms(input, skeleton);

            //odczytujemy bind pose i hierarchie kosci
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            if (bones.Count > SkinnedEffect.MaxBones)
            {
                throw new InvalidContentException(string.Format("Skeleton has {0} bones, but max supported is {1}", bones.Count, SkinnedEffect.MaxBones));
            }

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();

            foreach (BoneContent bone in bones)
            {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            }

            //zapisujemy format AnimationContentDictionary jako animationClips
            Dictionary<string, AnimationClip> animationClips;
            animationClips = ProcessAnimations(skeleton.Animations, bones);

            //umozliwiamy konwersje 
            ModelContent model = base.Process(input, context);

            //zapisz w tagu

            model.Tag = new AnimationData(animationClips, bindPose, inverseBindPose, skeletonHierarchy);

            return model;
        }
    }
}
