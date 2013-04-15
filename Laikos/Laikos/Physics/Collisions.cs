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
        //Whole model is surrounded by BoundingBox stored in model.Tag info
       public static bool GeneralCollisionCheck(Model model1, Matrix world1, Model model2, Matrix world2)
        {
            //Retrieving data about BoundingBox from model.Tag for first model
            ModelExtra modelExtra = model1.Tag as ModelExtra;
            BoundingSphere originalSphere1 = modelExtra.boundingSphere;
            BoundingSphere Sphere1 = XNAUtils.TransformBoundingSphere(originalSphere1, world1);

            //Doing the same thing for second model
            ModelExtra modelExtra1 = model2.Tag as ModelExtra;
            BoundingSphere originalSphere2 = modelExtra.boundingSphere;
            BoundingSphere Sphere2 = XNAUtils.TransformBoundingSphere(originalSphere2, world2);

            //Checking if global bounding Box(surronds whole model) intersects another Box
            bool collision = Sphere1.Intersects(Sphere2);
            
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

            //Here we are creating BoundingBox for each mesh for model1
            Matrix[] model1Transforms = new Matrix[model1.Bones.Count];
            model1.CopyAbsoluteBoneTransformsTo(model1Transforms);
            BoundingBox[] model1Boxs = new BoundingBox[model1.Meshes.Count];
            
            for (int i = 0; i < model1.Meshes.Count; i++)
            {
                ModelMesh mesh = model1.Meshes[i];
                BoundingSphere origSphere = mesh.BoundingSphere;
                BoundingBox origBox = BoundingBox.CreateFromSphere(origSphere);
                Matrix trans = model1Transforms[mesh.ParentBone.Index] * world1;
                BoundingBox transBox = XNAUtils.TransformBoundingBox(origBox, trans);
                model1Boxs[i] = transBox;
            }

            //and here for second model
            Matrix[] model2Transforms = new Matrix[model2.Bones.Count];
            model2.CopyAbsoluteBoneTransformsTo(model2Transforms);
            BoundingBox[] model2Boxs = new BoundingBox[model2.Meshes.Count];
            
            for (int i = 0; i < model2.Meshes.Count; i++)
            {
                ModelMesh mesh = model2.Meshes[i];
                BoundingSphere origSphere = mesh.BoundingSphere;
                BoundingBox origBox = BoundingBox.CreateFromSphere(origSphere);
                Matrix trans = model2Transforms[mesh.ParentBone.Index] * world2;
                BoundingBox transBox = XNAUtils.TransformBoundingBox(origBox, trans);
                model2Boxs[i] = transBox;
            }

            bool collision = false;

            //Check if any of created before Boxs intersects with another Box
            for (int i = 0; i < model1Boxs.Length; i++)
                for (int j = 0; j < model2Boxs.Length; j++)
                    if (BoundingSphere.CreateFromBoundingBox(model1Boxs[i]).Intersects(BoundingSphere.CreateFromBoundingBox(model2Boxs[j])))
                        return true;

            return collision;
        }

       //This method is performing basic collision detection between two models
       //Whole model is surrounded by BoundingBox stored in model.Tag info
       public static bool GeneralDecorationCollisionCheck(Model model1, Matrix world1, Model model2, Matrix world2)
       {
           //Retrieving data about BoundingBox from model.Tag for first model
           ModelExtra animationData1 = model1.Tag as ModelExtra;
           BoundingBox originalBox1 = animationData1.boundingBox;
           BoundingBox Box1 = XNAUtils.TransformBoundingBox(originalBox1, world1);
           
           //Doing the same thing for second model
           ModelExtra animationData2 = model2.Tag as ModelExtra;
           BoundingBox originalBox2 = animationData2.boundingBox;
           BoundingBox Box2 = XNAUtils.TransformBoundingBox(originalBox2, world2);
           
           //Checking if global bounding Box(surronds whole model) intersects another Box
           bool collision = BoundingSphere.CreateFromBoundingBox(Box1).Intersects(Box2);
           return collision;
       }

       //This method performs much more detailed collision check.
       //It checks if there is collision for each mesh of model
       public static bool DetailedDecorationCollisionCheck(Model model1, Matrix world1, Model model2, Matrix world2)
       {
           //first we check if there is general collision between two models
           //If method returns false we dont have to perform detailed check
           if (!GeneralDecorationCollisionCheck(model1, world1, model2, world2))
               return false;

           //Here we are creating BoundingBox for each mesh for model1
           Matrix[] model1Transforms = new Matrix[model1.Bones.Count];
           model1.CopyAbsoluteBoneTransformsTo(model1Transforms);
           BoundingBox[] model1Boxs = new BoundingBox[model1.Meshes.Count];
           for (int i = 0; i < model1.Meshes.Count; i++)
           {
               ModelMesh mesh = model1.Meshes[i];
               BoundingSphere origSphere = mesh.BoundingSphere;
               BoundingBox origBox = BoundingBox.CreateFromSphere(origSphere);
               Matrix trans = model1Transforms[mesh.ParentBone.Index] * world1;
               BoundingBox transBox = XNAUtils.TransformBoundingBox(origBox, trans);
               model1Boxs[i] = transBox;
           }

           //and here for second model
           Matrix[] model2Transforms = new Matrix[model2.Bones.Count];
           model2.CopyAbsoluteBoneTransformsTo(model2Transforms);
           BoundingBox[] model2Boxs = new BoundingBox[model2.Meshes.Count];

           for (int i = 0; i < model2.Meshes.Count; i++)
           {
               ModelMesh mesh = model2.Meshes[i];
               BoundingSphere origSphere = mesh.BoundingSphere;
               BoundingBox origBox = BoundingBox.CreateFromSphere(origSphere);
               Matrix trans = model2Transforms[mesh.ParentBone.Index] * world2;
               BoundingBox transBox = XNAUtils.TransformBoundingBox(origBox, trans);
               model2Boxs[i] = transBox;
           }

           bool collision = false;

           //Check if any of created before Boxs intersects with another Box
           for (int i = 0; i < model1Boxs.Length; i++)
               for (int j = 0; j < model2Boxs.Length; j++)
                   if (BoundingSphere.CreateFromBoundingBox(model1Boxs[i]).Intersects(BoundingSphere.CreateFromBoundingBox(model2Boxs[j])))
                       return true;

           return collision;
       }

        //Simple method to add gravity to every model
        public static void AddGravity(ref Vector3 currentPosition)
        {
            currentPosition.Y -= 0.3f;
        }

        //Collision between pointer and model to select it
        //Returns true if collision occured
        public static bool RayModelCollision(Ray ray, Model model, Matrix world)
        {
            bool collision = false;
            ModelExtra animationData = model.Tag as ModelExtra;
            BoundingBox originalBox = animationData.boundingBox;
            BoundingBox Box = XNAUtils.TransformBoundingBox(originalBox, world);

            //Determines intersection between mouse Ray and model's box
            float? intersection = Box.Intersects(ray);
            if (intersection <= ray.Direction.Length())
                return true;

            return collision;
        }

        //Mouse and building collision
        public static bool RayBuildingCollision(Ray ray, Model model, Matrix world)
        {
            bool collision = false;
            BoundingBox originalBox = (BoundingBox)model.Tag;
            BoundingBox Box = XNAUtils.TransformBoundingBox(originalBox, world);

            float? intersection = Box.Intersects(ray);
            if (intersection <= ray.Direction.Length())
                return true;

            return collision;
        }

        //Binary search of intersection between pointer's ray and terrain
        //This function returns Vector with coordinates where our pointer collides with terrain
        public static Vector3 BinarySearch(Ray ray)
        {
            //Set all needed variables (accuracy, height etc.)
            float accuracy = 0.01f;
            float heightAtStartingPoint = Terrain.GetExactHeightAt(ray.Position.X, ray.Position.Z);
            float currentError = ray.Position.Y - heightAtStartingPoint;
            int counter = 0;

            //This loop will find our collision point
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
                //There is no point to iterate more than 1000 times
                //Accuracy is still good enough so we can break the loop
                if (counter++ == 1000) break;
            }

            return ray.Position;
        }

        //We need linear search to improve collision detection
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

        //Getting Ray of our pointer
        public static Ray GetPointerRay(Vector2 pointerPosition, GraphicsDevice device)
        {
            // Here we set near and fat screen point. Z coordinate needs to be between 0 and 1
            // 0 for near
            // 1 for far
            Vector3 nearScreenPoint = new Vector3(pointerPosition.X, pointerPosition.Y, 0);
            Vector3 farScreenPoint = new Vector3(pointerPosition.X, pointerPosition.Y, 1);

            // Unproject far and near point in 3d world
            Vector3 near3DWorldPoint = device.Viewport.Unproject(nearScreenPoint, Camera.projectionMatrix, Camera.viewMatrix, Matrix.Identity);
            Vector3 far3DWorldPoint = device.Viewport.Unproject(farScreenPoint, Camera.projectionMatrix, Camera.viewMatrix, Matrix.Identity);

            // Setting and normalizing pointer ray direction
            Vector3 pointerRayDirection = far3DWorldPoint - near3DWorldPoint;
            pointerRayDirection.Normalize();

            return new Ray(near3DWorldPoint, pointerRayDirection);
        }

        //Shorten our Ray so its between max and lowest height of terrain
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
