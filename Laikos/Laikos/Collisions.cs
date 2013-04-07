using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Animation;
using System;
namespace Laikos
{
    //Static class created to perform collision test between models, terrain and camera.
    static class Collisions
    {
        //Here we are testing collision of Object(Model, Camera etc.) with terrain.
        //currentPosition - it's position of camera or model
        //distance - describe distance between model or camera and terrain
        //terrain - variable to terrain, so we can check exact height at given point(x,z)
        public static void CheckWithTerrain(ref Vector3 currentPosition, float distance)
        {
            //Returns exact height(y) at given point(x,z) of terrain.
            float terrainHeight = Terrain.GetExactHeightAt(currentPosition.X, currentPosition.Z);

            //If position of model or camera i smaller than terrain height + max distance then we slightly move it up
            if (currentPosition.Y < terrainHeight + distance)
            {
                Vector3 newPos = currentPosition;
                newPos.Y = (terrainHeight + distance);
                currentPosition = newPos;
            }
        }

        //This method is performing basic collision detection between two models
        //Whole model is surrounded by BoundingSphere stored in model.Tag info
        public static bool GeneralCollisionCheck(Model model1, Matrix world1, Model model2, Matrix world2)
        {
            //Retrieving data about BoundingSphere from model.Tag for first model
            AnimationData animationData1 = model1.Tag as AnimationData;
            BoundingSphere originalSphere1 = animationData1.BoundingSphere;
            BoundingSphere sphere1 = XNAUtils.TransformBoundingSphere(originalSphere1, world1);

            //Doing the same thing for second model
            AnimationData animationData2 = model2.Tag as AnimationData;
            BoundingSphere originalSphere2 = animationData2.BoundingSphere;
            BoundingSphere sphere2 = XNAUtils.TransformBoundingSphere(originalSphere2, world2);

            //Checking if global bounding sphere(surronds whole model) intersects another sphere
            bool collision = sphere1.Intersects(sphere2);
            return collision;
        }

        //This method performs much more detailed collision check.
        //It checks if there is collision for each mesh of model
        public static bool DetailedCollisionCheck(Model model1, Matrix world1, Model model2, Matrix world2)
        {
            //first we check if there is general collision between two models
            //If method returns false we dont have to perform detailed check
            if (!GeneralCollisionCheck(model1, world1, model2, world2))
                return false;

            //Here we are creating BoundingSphere for each mesh for model1
            Matrix[] model1Transforms = new Matrix[model1.Bones.Count];
            model1.CopyAbsoluteBoneTransformsTo(model1Transforms);
            BoundingSphere[] model1Spheres = new BoundingSphere[model1.Meshes.Count];
            for (int i = 0; i < model1.Meshes.Count; i++)
            {
                ModelMesh mesh = model1.Meshes[i];
                BoundingSphere origSphere = mesh.BoundingSphere;
                Matrix trans = model1Transforms[mesh.ParentBone.Index] * world1;
                BoundingSphere transSphere = XNAUtils.TransformBoundingSphere(origSphere, trans);
                model1Spheres[i] = transSphere;
            }

            //and here for second model
            Matrix[] model2Transforms = new Matrix[model2.Bones.Count];
            model2.CopyAbsoluteBoneTransformsTo(model2Transforms);
            BoundingSphere[] model2Spheres = new BoundingSphere[model2.Meshes.Count];
            for (int i = 0; i < model2.Meshes.Count; i++)
            {
                ModelMesh mesh = model2.Meshes[i];
                BoundingSphere origSphere = mesh.BoundingSphere;
                Matrix trans = model2Transforms[mesh.ParentBone.Index] * world2;
                BoundingSphere transSphere = XNAUtils.TransformBoundingSphere(origSphere, trans);
                model2Spheres[i] = transSphere;
            }

            bool collision = false;

            //Check if any of created before spheres intersects with another sphere
            for (int i = 0; i < model1Spheres.Length; i++)
                for (int j = 0; j < model2Spheres.Length; j++)
                    if (model1Spheres[i].Intersects(model2Spheres[j]))
                        return true;

            return collision;
        }

        //Simple method to add gravity to every model
        public static void AddGravity(ref Vector3 currentPosition)
        {
            currentPosition.Y -= 0.3f;
        }

        public static bool RayModelCollision(Ray ray, Model model, Matrix world)
        {
            bool collision = false;
            AnimationData animationData = model.Tag as AnimationData;
            BoundingSphere originalSphere = animationData.BoundingSphere;
            BoundingSphere sphere = XNAUtils.TransformBoundingSphere(originalSphere, world);
            Console.WriteLine(ray.Position.ToString() + " " + ray.Direction.ToString());
            float? intersection = sphere.Intersects(ray);
            if (intersection <= ray.Direction.Length())
                return true;

            return collision;
        }

        public static Vector3 BinarySearch(Ray ray)
        {
            float accuracy = 0.01f;
            float heightAtStartingPoint = Terrain.GetExactHeightAt(ray.Position.X, ray.Position.Z);
            float currentError = ray.Position.Y - heightAtStartingPoint;
            int counter = 0;

            while (currentError < accuracy)
            {
                ray.Direction /= 2.0f;
                Vector3 nextPoint = ray.Position + ray.Direction;
                float heightAtNextPoint = Terrain.GetExactHeightAt(nextPoint.X, nextPoint.Z);
                if (nextPoint.Y < heightAtNextPoint)
                {
                    ray.Position = nextPoint;
                    currentError = ray.Position.Y - heightAtNextPoint;
                }
                if (counter++ == 1000) break;
            }

            return ray.Position;
        }

        public static Ray LinearSearch(Ray ray)
        {
            ray.Direction /= 300.0f;

            Vector3 nextPoint = ray.Position + ray.Direction;
            float heightAtNextPoint = Terrain.GetExactHeightAt(nextPoint.X, nextPoint.Z);
            while (heightAtNextPoint < nextPoint.Y)
            {
                ray.Position = nextPoint;

                nextPoint = ray.Position + ray.Direction;
                heightAtNextPoint = Terrain.GetExactHeightAt(nextPoint.X, nextPoint.Z);
            }
            return ray;
        }

        public static Ray GetPointerRay(Vector2 pointerPosition, GraphicsDevice device)
        {
            Vector3 nearScreenPoint = new Vector3(pointerPosition.X, pointerPosition.Y, 0);
            Vector3 farScreenPoint = new Vector3(pointerPosition.X, pointerPosition.Y, 1);

            Vector3 near3DWorldPoint = device.Viewport.Unproject(nearScreenPoint, Camera.projectionMatrix, Camera.viewMatrix, Matrix.Identity);
            Vector3 far3DWorldPoint = device.Viewport.Unproject(farScreenPoint, Camera.projectionMatrix, Camera.viewMatrix, Matrix.Identity);

            Vector3 pointerRayDirection = far3DWorldPoint - near3DWorldPoint;
            pointerRayDirection.Normalize();

            return new Ray(near3DWorldPoint, pointerRayDirection);
        }

        public static Ray ClipRay(Ray ray, float highest, float lowest)
        {
            Vector3 oldStartPoint = ray.Position;

            float factorH = -(oldStartPoint.Y - highest) / ray.Direction.Y;
            Vector3 pointA = oldStartPoint + factorH * ray.Direction;

            float factorL = -(oldStartPoint.Y - lowest) / ray.Direction.Y;
            Vector3 pointB = oldStartPoint + factorL * ray.Direction;

            Vector3 newDirection = pointB - pointA;

            return new Ray(pointA, newDirection);
        }
    }
}
