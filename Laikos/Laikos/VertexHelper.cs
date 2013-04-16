using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Laikos
{
    public class VertexHelper
    {
        public VertexHelper()
        {
        }

        public struct TriangleVertexIndices
        {
            public int A;
            public int B;
            public int C;
        }

        /// <summary>
        /// Extract the vertices and indices from the specified model
        /// </summary>
        /// <param name="vertices">Output the list of vertices</param>
        /// <param name="indices">Output the list of indices</param>
        /// <param name="worldPosition">The models world position or use Matrix.Identity for object space</param>
        public static void ExtractTrianglesFrom(Model modelToUse, List<Vector3> vertices, Matrix worldPosition)
        {
            Matrix transform = Matrix.Identity;
            foreach (ModelMesh mesh in modelToUse.Meshes)
            {
                // If the model has bones the vertices have to be transformed by the bone position
                transform = Matrix.Multiply(GetAbsoluteTransform(mesh.ParentBone), worldPosition);
                ExtractModelMeshData(mesh, ref transform, vertices);
            }
        }

        /// <summary>
        /// Transform by a bone position or Identity if no bone is supplied
        /// </summary>
        public static Matrix GetAbsoluteTransform(ModelBone bone)
        {
            if (bone == null)
            {
                return Matrix.Identity;
            }
            return bone.Transform * GetAbsoluteTransform(bone.Parent);
        }

        /// <summary>
        /// Get all the triangles from all mesh parts
        /// </summary>
        public static void ExtractModelMeshData(ModelMesh mesh, ref Matrix transform,
            List<Vector3> vertices)
        {
            foreach (ModelMeshPart meshPart in mesh.MeshParts)
            {
                ExtractModelMeshPartData(meshPart, ref transform, vertices);
            }
        }

        /// <summary>
        /// Get all the triangles from each mesh part (Changed for XNA 4)
        /// </summary>
        public static void ExtractModelMeshPartData(ModelMeshPart meshPart, ref Matrix transform,
            List<Vector3> vertices)
        {
            // Before we add any more where are we starting from
            int offset = vertices.Count;

            // == Vertices (Changed for XNA 4.0)

            // Read the format of the vertex buffer
            VertexDeclaration declaration = meshPart.VertexBuffer.VertexDeclaration;
            VertexElement[] vertexElements = declaration.GetVertexElements();
            // Find the element that holds the position
            VertexElement vertexPosition = new VertexElement();
            foreach (VertexElement vert in vertexElements)
            {
                if (vert.VertexElementUsage == VertexElementUsage.Position &&
                    vert.VertexElementFormat == VertexElementFormat.Vector3)
                {
                    vertexPosition = vert;
                    // There should only be one
                    break;
                }
            }
            // Check the position element found is valid
            if (vertexPosition == null ||
                vertexPosition.VertexElementUsage != VertexElementUsage.Position ||
                vertexPosition.VertexElementFormat != VertexElementFormat.Vector3)
            {
                throw new Exception("Model uses unsupported vertex format!");
            }
            // This where we store the vertices until transformed
            Vector3[] allVertex = new Vector3[meshPart.NumVertices];
            // Read the vertices from the buffer in to the array
            meshPart.VertexBuffer.GetData<Vector3>(
                meshPart.VertexOffset * declaration.VertexStride + vertexPosition.Offset,
                allVertex,
                0,
                meshPart.NumVertices,
                declaration.VertexStride);
            // Transform them based on the relative bone location and the world if provided
            for (int i = 0; i != allVertex.Length; ++i)
            {
                Vector3.Transform(ref allVertex[i], ref transform, out allVertex[i]);
            }
            // Store the transformed vertices with those from all the other meshes in this model
            vertices.AddRange(allVertex);

            // == Indices (Changed for XNA 4)

            // Find out which vertices make up which triangles
            /*if (meshPart.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
            {
                // This could probably be handled by using int in place of short but is unnecessary
                throw new Exception("Model uses 32-bit indices, which are not supported.");
            }
            // Each primitive is a triangle
            short[] indexElements = new short[meshPart.PrimitiveCount * 3];
            meshPart.IndexBuffer.GetData<short>(
                meshPart.StartIndex * 2,
                indexElements,
                0,
                meshPart.PrimitiveCount * 3);
            // Each TriangleVertexIndices holds the three indexes to each vertex that makes up a triangle
            TriangleVertexIndices[] tvi = new TriangleVertexIndices[meshPart.PrimitiveCount];
            for (int i = 0; i != tvi.Length; ++i)
            {
                // The offset is because we are storing them all in the one array and the 
                // vertices were added to the end of the array.
                tvi[i].A = indexElements[i * 3 + 0] + offset;
                tvi[i].B = indexElements[i * 3 + 1] + offset;
                tvi[i].C = indexElements[i * 3 + 2] + offset;
            }
            // Store our triangles
            indices.AddRange(tvi);*/
        }


    }
}
