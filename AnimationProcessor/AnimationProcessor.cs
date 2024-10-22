using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Animation;

namespace AnimationPipeline
{
    /// <summary>
    /// This class extends the standard ModelProcessor to include code
    /// that extracts a skeleton, pulls any animations, and does any
    /// necessary prep work to support animation and skinning.
    /// </summary>
    [ContentProcessor(DisplayName = "Animation Processor")]
    public class AnimationProcessor : ModelProcessor
    {
        /// <summary>
        /// The model we are reading
        /// </summary>
        private ModelContent model;

        /// <summary>
        /// Extra content to associated with the model. This is where we put the stuff that is 
        /// unique to this project.
        /// </summary>
        private ModelExtra modelExtra = new ModelExtra();

        /// <summary>
        /// A lookup dictionary that remembers when we changes a material to 
        /// skinned material.
        /// </summary>
        private Dictionary<MaterialContent, SkinnedMaterialContent> toSkinnedMaterial = new Dictionary<MaterialContent, SkinnedMaterialContent>();

        #region Deffered Lighting Variables
        string directory;

        //NormalMap texture property
        [DisplayName("Normal Map Texture")]
        [Description("This will be used as the normal map on the model, if not set will use a default.")]
        [DefaultValue("")]
        public string NormalMapTexture { get; set; }

        //Normal Map Key, will be used to search for the normal map in the opaque data of the model
        [DisplayName("Normal Map Key")]
        [Description("This will be the key that will be used to search for the normal map in the opaque data of the model")]
        [DefaultValue("NormalMap")]
        public string NormalMapKey
        {
            get { return normalMapKey; }
            set { normalMapKey = value; }
        }
        private string normalMapKey = "NormalMap";

        //Specular Map Texture Property
        [DisplayName("Specular Map Texture")]
        [Description("This will be used as the specular map on model, if not set will use a default")]
        [DefaultValue("")]
        public string SpecularMapTexture { get; set; }

        //Specular Map Key, will be used to search for the specular map in the opaque data of the model
        [DisplayName("Specular Map Key")]
        [Description("This will be the key that will be used to search for the specular map in the opaque data of the model")]
        [DefaultValue("SpecularMap")]
        public string SpecularMapKey
        {
            get { return specularMapKey; }
            set { specularMapKey = value; }
        }
        private string specularMapKey = "SpecularMap";

        //Turn the GenerateTangentFrames option to be always on
        [Browsable(false)]
        public override bool GenerateTangentFrames { get { return true; } set { } }

        static IList<string> acceptableVertexChannelNames = new string[]
        {
            VertexChannelNames.TextureCoordinate(0),
            VertexChannelNames.Normal(0),
            VertexChannelNames.Binormal(0),
            VertexChannelNames.Tangent(0),
            VertexChannelNames.Weights(0),
        };
        #endregion

        #region Deffered Lighting Support
        protected override void ProcessVertexChannel(GeometryContent geometry, int vertexChannelIndex, ContentProcessorContext context)
        {
            string vertexChannelName = geometry.Vertices.Channels[vertexChannelIndex].Name;

            if (acceptableVertexChannelNames.Contains(vertexChannelName))
                base.ProcessVertexChannel(geometry, vertexChannelIndex, context);
            else
                geometry.Vertices.Channels.Remove(vertexChannelName);
        }

        private void LookUpTextures(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                #region Normal Map Path Lookup
                string normalMapPath;

                if (!string.IsNullOrEmpty(NormalMapTexture))
                    normalMapPath = NormalMapTexture;
                else
                    normalMapPath = mesh.OpaqueData.GetValue<string>(NormalMapKey, null);

                if (normalMapPath == null)
                {
                    normalMapPath = Path.Combine(directory, mesh.Name + "_n.tga");

                    if (!File.Exists(normalMapPath))
                        normalMapPath = "null_normal.tga";
                }
                else
                    normalMapPath = Path.Combine(directory, normalMapPath);
                #endregion

                #region Specular Map Path Lookup
                string specularMapPath;

                if (!string.IsNullOrEmpty(SpecularMapTexture))
                    specularMapPath = SpecularMapTexture;
                else
                    specularMapPath = mesh.OpaqueData.GetValue<string>(SpecularMapKey, null);

                if (specularMapPath == null)
                {
                    specularMapPath = Path.Combine(directory, mesh.Name + "_s.tga");

                    if (!File.Exists(specularMapPath))
                        specularMapPath = "null_specular.tga";
                }
                else
                {
                    specularMapPath = Path.Combine(directory, specularMapPath);
                }
                #endregion

                foreach (GeometryContent geo in mesh.Geometry)
                {
                    if (geo.Material != null)
                    {
                        if (geo.Material.Textures.ContainsKey(NormalMapKey))
                        {
                            ExternalReference<TextureContent> texRef = geo.Material.Textures[NormalMapKey];
                            geo.Material.Textures.Remove(NormalMapKey);
                            geo.Material.Textures.Add("NormalMap", texRef);
                        }
                        else
                            geo.Material.Textures.Add("NormalMap", new ExternalReference<TextureContent>(normalMapPath));

                        if (geo.Material.Textures.ContainsKey(SpecularMapKey))
                        {
                            ExternalReference<TextureContent> texRef = geo.Material.Textures[SpecularMapKey];
                            geo.Material.Textures.Remove(SpecularMapKey);
                            geo.Material.Textures.Add("SpecularMap", texRef);
                        }
                        else
                            geo.Material.Textures.Add("SpecularMap", new ExternalReference<TextureContent>(specularMapPath));
                    }
                }
            }

            foreach (NodeContent child in node.Children)
            {
                LookUpTextures(child);
            }
        }

        /*protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            EffectMaterialContent defferedShadingMarerial = new EffectMaterialContent();

            defferedShadingMarerial.Effect = new ExternalReference<EffectContent>("Effects/GBuffer.fx");

            foreach (KeyValuePair<string, ExternalReference<TextureContent>> texture in material.Textures)
            {
                if ((texture.Key == "Texture") || (texture.Key == "NormalMap") || (texture.Key == "SpecularMap"))
                    defferedShadingMarerial.Textures.Add(texture.Key, texture.Value);
            }

            return context.Convert<MaterialContent, MaterialContent>(defferedShadingMarerial, typeof(MaterialProcessor).Name);
        }*/

        #endregion

        /// <summary>
        /// The function to process a model from original content into model content for export
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            if (input == null) throw new ArgumentNullException("input");

            directory = Path.GetDirectoryName(input.Identity.SourceFilename);

            LookUpTextures(input);
            // Process the skeleton for skinned character animation
            BoneContent skeleton = ProcessSkeleton(input);

            SwapSkinnedMaterial(input);

            model = base.Process(input, context);

            ProcessAnimations(model, input, context,input.Identity);

            List<Vector3> vertices = new List<Vector3>();
            AddVerticesToList(input, vertices);
            modelExtra.boundingBox = BoundingBox.CreateFromPoints(vertices);
            modelExtra.boundingSphere = BoundingSphere.CreateFromPoints(vertices);

            // Add the extra content to the model 
            model.Tag = modelExtra;

            List<Vector3[]> meshVertices = new List<Vector3[]>();
            meshVertices = AddModelMeshVertexArrayToList(input, meshVertices);

            int i = 0;
            foreach (ModelMeshContent mesh in model.Meshes)
            {
                List<Vector3> modelMeshVertices = new List<Vector3>();
                foreach (ModelMeshPartContent part in mesh.MeshParts)
                    {
                        if (i < meshVertices.Count)
                        modelMeshVertices.AddRange(meshVertices[i++]);
                    }
                mesh.Tag = modelMeshVertices.ToArray();
            }

            return model;
        }

        private List<Vector3[]> AddModelMeshVertexArrayToList(NodeContent node, List<Vector3[]> meshVertices)
        {
            foreach (NodeContent child in node.Children)
                meshVertices = AddModelMeshVertexArrayToList(child, meshVertices);

            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                List<Vector3> nodeVertices = new List<Vector3>();
                foreach (GeometryContent geo in mesh.Geometry)
                {
                    foreach (int index in geo.Indices)
                    {
                        Vector3 vertex = geo.Vertices.Positions[index];
                        nodeVertices.Add(vertex);
                    }
                }
                meshVertices.Add(nodeVertices.ToArray());
            }
            return meshVertices;
        }

        #region Skeleton Support

        /// <summary>
        /// Process the skeleton in support of skeletal animation...
        /// </summary>
        /// <param name="input"></param>
        private BoneContent ProcessSkeleton(NodeContent input)
        {
            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                return null;

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            //
            // 3D Studio Max includes helper bones that end with "Nub"
            // These are not part of the skinning system and can be 
            // discarded.  TrimSkeleton removes them from the geometry.
            //

            TrimSkeleton(skeleton);

            // Convert the heirarchy of nodes and bones into a list
            List<NodeContent> nodes = FlattenHeirarchy(input);
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            // Create a dictionary to convert a node to an index into the array of nodes
            Dictionary<NodeContent, int> nodeToIndex = new Dictionary<NodeContent, int>();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodeToIndex[nodes[i]] = i;
            }

            // Now create the array that maps the bones to the nodes
            foreach (BoneContent bone in bones)
            {
                modelExtra.Skeleton.Add(nodeToIndex[bone]);
            }

            return skeleton;
        }

        /// <summary>
        /// Convert a tree of nodes into a list of nodes in topological order.
        /// </summary>
        /// <param name="item">The root of the heirarchy</param>
        /// <returns></returns>
        private List<NodeContent> FlattenHeirarchy(NodeContent item)
        {
            List<NodeContent> nodes = new List<NodeContent>();
            nodes.Add(item);
            foreach (NodeContent child in item.Children)
            {
                FlattenHeirarchy(nodes, child);
            }

            return nodes;
        }


        private void FlattenHeirarchy(List<NodeContent> nodes, NodeContent item)
        {
            nodes.Add(item);
            foreach (NodeContent child in item.Children)
            {
                FlattenHeirarchy(nodes, child);
            }
        }

        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // This is important: Don't bake in the transforms except
                // for geometry that is part of a skinned mesh
                if(IsSkinned(child))
                {
                    FlattenAllTransforms(child);
                }
            }
        }

        /// <summary>
        /// Recursively flatten all transforms from this node down
        /// </summary>
        /// <param name="node"></param>
        void FlattenAllTransforms(NodeContent node)
        {
            // Bake the local transform into the actual geometry.
            MeshHelper.TransformScene(node, node.Transform);

            // Having baked it, we can now set the local
            // coordinate system back to identity.
            node.Transform = Matrix.Identity;

            foreach (NodeContent child in node.Children)
            {
                FlattenAllTransforms(child);
            }
        }

        /// <summary>
        /// 3D Studio Max includes an extra help bone at the end of each
        /// IK chain that doesn't effect the skinning system and is 
        /// redundant as far as any game is concerned.  This function
        /// looks for children who's name ends with "Nub" and removes
        /// them from the heirarchy.
        /// </summary>
        /// <param name="skeleton">Root of the skeleton tree</param>
        void TrimSkeleton(NodeContent skeleton)
        {
            List<NodeContent> todelete = new List<NodeContent>();

            foreach (NodeContent child in skeleton.Children)
            {
                if (child.Name.EndsWith("Nub") || child.Name.EndsWith("Footsteps"))
                    todelete.Add(child);
                else
                    TrimSkeleton(child);
            }

            foreach (NodeContent child in todelete)
            {
                skeleton.Children.Remove(child);
            }
        }


        #endregion

        #region Skinned Support

        /// <summary>
        /// Determine if a node is a skinned node, meaning it has bone weights
        /// associated with it.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool IsSkinned(NodeContent node)
        {
            // It has to be a MeshContent node
            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                // In the geometry we have to find a vertex channel that
                // has a bone weight collection
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    foreach (VertexChannel vchannel in geometry.Vertices.Channels)
                    {
                        if (vchannel is VertexChannel<BoneWeightCollection>)
                            return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// If a node is skinned, we need to use the skinned model 
        /// effect rather than basic effect. This function runs through the 
        /// geometry and finds the meshes that have bone weights associated 
        /// and swaps in the skinned effect. 
        /// </summary>
        /// <param name="node"></param>
        void SwapSkinnedMaterial(NodeContent node)
        {
            // It has to be a MeshContent node
            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                // In the geometry we have to find a vertex channel that
                // has a bone weight collection
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    bool swap = false;
                    foreach (VertexChannel vchannel in geometry.Vertices.Channels)
                    {
                        if (vchannel is VertexChannel<BoneWeightCollection>)
                        {
                            swap = true;
                            break;
                        }
                    }

                    if (swap)
                    {
                        if (toSkinnedMaterial.ContainsKey(geometry.Material))
                        {
                            // We have already swapped it
                            geometry.Material = toSkinnedMaterial[geometry.Material];
                        }
                        else
                        {
                            SkinnedMaterialContent smaterial = new SkinnedMaterialContent();
                            BasicMaterialContent bmaterial = geometry.Material as BasicMaterialContent;

                            // Copy over the data
                            smaterial.Alpha = bmaterial.Alpha;
                            smaterial.DiffuseColor = bmaterial.DiffuseColor;
                            smaterial.EmissiveColor = bmaterial.EmissiveColor;
                            smaterial.SpecularColor = bmaterial.SpecularColor;
                            smaterial.SpecularPower = bmaterial.SpecularPower;
                            smaterial.Texture = bmaterial.Texture;
                            
                            smaterial.WeightsPerVertex = 4;
                            toSkinnedMaterial[geometry.Material] = smaterial;
                            geometry.Material = smaterial;
                        }
                    }
                }
            }

            foreach (NodeContent child in node.Children)
            {
                SwapSkinnedMaterial(child);
            }
        }


        #endregion

        #region Animation Support

        /// <summary>
        /// Bones lookup table, converts bone names to indices.
        /// </summary>
        private Dictionary<string, int> bones = new Dictionary<string, int>();

        /// <summary>
        /// This will keep track of all of the bone transforms for a base pose
        /// </summary>
        private Matrix[] boneTransforms;

        /// <summary>
        /// A dictionary so we can keep track of the clips by name
        /// </summary>
        private Dictionary<string, AnimationClip> clips = new Dictionary<string, AnimationClip>();

        /// <summary>
        /// Entry point for animation processing. 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="input"></param>
        /// <param name="context"></param>
        private void ProcessAnimations(ModelContent model, NodeContent input, ContentProcessorContext context, ContentIdentity sourceIdentity)
        {
            // First build a lookup table so we can determine the 
            // index into the list of bones from a bone name.
            for (int i = 0; i < model.Bones.Count; i++)
            {
                bones[model.Bones[i].Name] = i;
            }

            // For saving the bone transforms
            boneTransforms = new Matrix[model.Bones.Count];

            //
            // Collect up all of the animation data
            //

            ProcessAnimationsRecursive(input);

            // Check to see if there's an animation clip definition
            // Here, we're checking for a file with the _Anims suffix.
            // So, if your model is named dude.fbx, we'd add dude_Anims.xml in the same folder
            // and the pipeline will see the file and use it to override the animations in the
            // original model file.
            string SourceModelFile = sourceIdentity.SourceFilename;
            string SourcePath = Path.GetDirectoryName(SourceModelFile);
            string AnimFilename = Path.GetFileNameWithoutExtension(SourceModelFile);
            AnimFilename += "_Anims.xml";
            string AnimPath = Path.Combine(SourcePath, AnimFilename);
            if (File.Exists(AnimPath))
            {
                // Add the filename as a dependency, so if it changes, the model is rebuilt
                context.AddDependency(AnimPath);

                // Load the animation definition from the XML file
                AnimationDefinition AnimDef = context.BuildAndLoadAsset<XmlImporter, AnimationDefinition>(new ExternalReference<XmlImporter>(AnimPath), null);
                
                if (modelExtra.Clips.Count > 0) //if there are some animations in our model
                {
                    foreach (AnimationDefinition.ClipPart Part in AnimDef.ClipParts)
                    {
                        // Grab the main clip that we are using and copy to MainClip

                        AnimationClip MainClip = new AnimationClip();

                        float StartTime = GetTimeSpanForFrame(Part.StartFrame, AnimDef.OriginalFrameCount, modelExtra.Clips[AnimDef.OriginalClipName].Duration);
                        float EndTime = GetTimeSpanForFrame(Part.EndFrame, AnimDef.OriginalFrameCount, modelExtra.Clips[AnimDef.OriginalClipName].Duration);

                        MainClip.Duration = EndTime-StartTime;
                        MainClip.Name = modelExtra.Clips[AnimDef.OriginalClipName].Name;

                        // Process each of our new animation clip parts
                        for (int i = 0; i < modelExtra.Clips[AnimDef.OriginalClipName].Bones.Count; i++)
                        {
                            AnimationClip.Bone clipBone = new AnimationClip.Bone();
                            clipBone.Name = modelExtra.Clips[AnimDef.OriginalClipName].Bones[i].Name;
                            LinkedList<AnimationClip.Keyframe> keyframes = new LinkedList<AnimationClip.Keyframe>();

                            if (modelExtra.Clips[AnimDef.OriginalClipName].Bones[i].Keyframes.Count != 0)
                            {

                                for (int j = 0; j < modelExtra.Clips[AnimDef.OriginalClipName].Bones[i].Keyframes.Count; j++)
                                {
                                    
                                    if ((modelExtra.Clips[AnimDef.OriginalClipName].Bones[i].Keyframes[j].Time >= StartTime) && (modelExtra.Clips[AnimDef.OriginalClipName].Bones[i].Keyframes[j].Time <= EndTime))
                                    {
                                        AnimationClip.Keyframe frame = new AnimationClip.Keyframe();
                                        frame.Rotation = modelExtra.Clips[AnimDef.OriginalClipName].Bones[i].Keyframes[j].Rotation;
                                        frame.Time = modelExtra.Clips[AnimDef.OriginalClipName].Bones[i].Keyframes[j].Time - StartTime;
                                        frame.Translation = modelExtra.Clips[AnimDef.OriginalClipName].Bones[i].Keyframes[j].Translation;
                                        keyframes.AddLast(frame);
                                       //clipBone.Keyframes.Add(frame);
                                    }
                                    
                                }
                            }
                           // LinearKeyframeReduction(keyframes);
                            clipBone.Keyframes = keyframes.ToList<AnimationClip.Keyframe>();
                            MainClip.Bones.Add(clipBone);

                        }
                        modelExtra.Clips.Add(Part.ClipName, MainClip);
                        
                    }
                }
            }



            // Ensure there is always a clip, even if none is included in the FBX
            // That way we can create poses using FBX files as one-frame 
            // animation clips
            if (modelExtra.Clips.Count == 0)
            {
                AnimationClip clip = new AnimationClip();
                modelExtra.Clips.Add("Take 001",clip);

                string clipName = "Take 001";

                // Retain by name
                clips[clipName] = clip;

                clip.Name = clipName;
                foreach (ModelBoneContent bone in model.Bones)
                {
                    AnimationClip.Bone clipBone = new AnimationClip.Bone();
                    clipBone.Name = bone.Name;

                    clip.Bones.Add(clipBone);
                }
            }

            //Ensure all animations have a first key frame for every bone
            foreach (KeyValuePair<string,AnimationClip> clip in modelExtra.Clips)
            {
                for (int b = 0; b < bones.Count; b++)
                {
                    List<AnimationClip.Keyframe> keyframes = clip.Value.Bones[b].Keyframes;
                    if (keyframes.Count == 0 || keyframes[0].Time > 0)
                    {
                        AnimationClip.Keyframe keyframe = new AnimationClip.Keyframe();
                        keyframe.Time = 0;
                        keyframe.Transform = boneTransforms[b];
                        keyframes.Insert(0, keyframe);
                    }
                }
            }
        }

        /// <summary>
        /// Recursive function that processes the entire scene graph, collecting up
        /// all of the animation data.
        /// </summary>
        private void ProcessAnimationsRecursive(NodeContent input)
        {
            // Look up the bone for this input channel
            int inputBoneIndex;
            if (bones.TryGetValue(input.Name, out inputBoneIndex))
            {
                // Save the transform
                boneTransforms[inputBoneIndex] = input.Transform;
            }


            foreach (KeyValuePair<string, AnimationContent> animation in input.Animations)
            {
                // Do we have this animation before?
                AnimationClip clip;
                string clipName = animation.Key;

                if (!clips.TryGetValue(clipName, out clip))
                {
                    // Never seen before clip
                    clip = new AnimationClip();
                    modelExtra.Clips.Add(clipName,clip);

                    // Retain by name
                    clips[clipName] = clip;

                    clip.Name = clipName;
                    foreach (ModelBoneContent bone in model.Bones)
                    {
                        AnimationClip.Bone clipBone = new AnimationClip.Bone();
                        clipBone.Name = bone.Name;

                        clip.Bones.Add(clipBone);
                    }
                }

                // Ensure the duration is always set
                if (animation.Value.Duration.TotalSeconds > clip.Duration)
                    clip.Duration = animation.Value.Duration.TotalSeconds;

                //
                // For each channel, determine the bone and then process all of the 
                // keyframes for that bone.
                //

                foreach (KeyValuePair<string, AnimationChannel> channel in animation.Value.Channels)
                {
                    // What is the bone index?
                    int boneIndex;
                    if (!bones.TryGetValue(channel.Key, out boneIndex))
                        continue;           // Ignore if not a named bone

                    // An animation is useless if it is for a bone not assigned to any meshes at all
                    if (UselessAnimationTest(boneIndex))
                        continue;

                    // I'm collecting up in a linked list so we can process the data
                    // and remove redundant keyframes
                    LinkedList<AnimationClip.Keyframe> keyframes = new LinkedList<AnimationClip.Keyframe>();
                    foreach (AnimationKeyframe keyframe in channel.Value)
                    {
                        Matrix transform = keyframe.Transform;      // Keyframe transformation

                        AnimationClip.Keyframe newKeyframe = new AnimationClip.Keyframe();
                        newKeyframe.Time = keyframe.Time.TotalSeconds;
                        newKeyframe.Transform = transform;

                        keyframes.AddLast(newKeyframe);
                    }
                
                   
                    foreach (AnimationClip.Keyframe keyframe in keyframes)
                    {
                        clip.Bones[boneIndex].Keyframes.Add(keyframe);
                    }

                }


            }

            foreach (NodeContent child in input.Children)
            {
                ProcessAnimationsRecursive(child);
            }
        }

        private const float TinyLength = 1e-8f;
        private const float TinyCosAngle = 0.9999999f;

        /// <summary>
        /// This function filters out keyframes that can be approximated well with 
        /// linear interpolation.
        /// </summary>
        /// <param name="keyframes"></param>
        private void LinearKeyframeReduction(LinkedList<AnimationClip.Keyframe> keyframes)
        {
            if (keyframes.Count < 3)
                return;

            for (LinkedListNode<AnimationClip.Keyframe> node = keyframes.First.Next; ; )
            {
                LinkedListNode<AnimationClip.Keyframe> next = node.Next;
                if (next == null)
                    break;

                // Determine nodes before and after the current node.
                AnimationClip.Keyframe a = node.Previous.Value;
                AnimationClip.Keyframe b = node.Value;
                AnimationClip.Keyframe c = next.Value;

                float t = (float)((node.Value.Time - node.Previous.Value.Time) /
                                   (next.Value.Time - node.Previous.Value.Time));

                Vector3 translation = Vector3.Lerp(a.Translation, c.Translation, t);
                Quaternion rotation = Quaternion.Slerp(a.Rotation, c.Rotation, t);

                if ((translation - b.Translation).LengthSquared() < TinyLength &&
                   Quaternion.Dot(rotation, b.Rotation) > TinyCosAngle)
                {
                    keyframes.Remove(node);
                }

                node = next;
            }
        }

        /// <summary>
        /// Discard any animation not assigned to a mesh or the skeleton
        /// </summary>
        /// <param name="boneId"></param>
        /// <returns></returns>
        bool UselessAnimationTest(int boneId)
        {
            // If any mesh is assigned to this bone, it is not useless
            foreach (ModelMeshContent mesh in model.Meshes)
            {
                if (mesh.ParentBone.Index == boneId)
                    return false;
            }

            // If this bone is in the skeleton, it is not useless
            foreach (int b in modelExtra.Skeleton)
            {
                if (boneId == b)
                    return false;
            }

            // Otherwise, it is useless
            return true;
        }

        private float GetTimeSpanForFrame(int FrameIndex, int TotalFrameCount, double TotalTicks)
        {
            float MaxFrameIndex = (float)TotalFrameCount - 1;
            float AmountOfAnimation = (float)FrameIndex / MaxFrameIndex;
            double NumTicks = AmountOfAnimation * TotalTicks;
            return (float)NumTicks;
        }

        private List<Vector3> AddVerticesToList(NodeContent node, List<Vector3> vertList)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                Matrix absTransform = mesh.AbsoluteTransform;

                foreach (GeometryContent geo in mesh.Geometry)
                {
                    foreach (int index in geo.Indices)
                    {
                        Vector3 vertex = geo.Vertices.Positions[index];
                        Vector3 transVertex = Vector3.Transform(vertex, absTransform);
                        vertList.Add(transVertex);
                    }
                }
            }

            foreach (NodeContent child in node.Children)
                vertList = AddVerticesToList(child, vertList);

            return vertList;
        }
        #endregion


    }
}