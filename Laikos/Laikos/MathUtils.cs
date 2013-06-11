using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Laikos
{
    public static class MathUtils
    {
        public static int RandomNumber(int min, int max)
        {
            var random = new Random(DateTime.Now.Millisecond);
            return random.Next(min, max);
        }


        public static Matrix CreateOrientation(float rotation, Vector3 normal)
        {
            Matrix orientation = Matrix.CreateRotationY(rotation);
            orientation.Up = normal;
            orientation.Right = Vector3.Cross(orientation.Forward, orientation.Up);
            orientation.Right = Vector3.Normalize(orientation.Right);
            orientation.Forward = Vector3.Cross(orientation.Up, orientation.Right);
            orientation.Forward = Vector3.Normalize(orientation.Forward);

            return orientation;
        }

        public static Matrix CreateOrientation(GameObject obj, Vector3 normal)
        {
            return CreateOrientation(obj.Rotation.Y, normal);
        }

        public static void Turn(ref double actualDirection, ref double direction, float turnSpeed, float ElapsedTime)
        {
            double difference = WrapAngle(direction - actualDirection);
            difference = MathHelper.Clamp((float)difference, -turnSpeed, turnSpeed);
            actualDirection = WrapAngle(actualDirection + difference);
        }

        public static double WrapAngle(double radians)
        {
            while (radians < -MathHelper.Pi)
            {
                radians += MathHelper.TwoPi;
            }
            while (radians > MathHelper.Pi)
            {
                radians -= MathHelper.TwoPi;
            }
            return radians;
        }

        //Return 2D Postion on line form start and end point at given time
        public static Vector2 Linear(Vector2 startPos, Vector2 endPos, float time)
        {
            return startPos + (endPos - startPos) * time;
        }

        public static float Linear(float startPos, float endPos, float time)
        {
            return startPos + (endPos - startPos) * time;
        }

        // metoda vyrobi stvorec kde zaciatok bude lavy horny roh a koniec pravy dolny
        public static void SafeSquare(ref Vector2 in1, ref Vector2 in2)
        {
            if (in1.X < in2.X && in1.Y < in2.Y)
            {
                return;
            }

            var tmp = new Vector2();
            tmp.X = Math.Max(in2.X, in1.X);
            tmp.Y = Math.Max(in2.Y, in1.Y);
            in1.X = Math.Min(in2.X, in1.X);
            in1.Y = Math.Min(in2.Y, in1.Y);
            in2 = tmp;
        }


 
        public static Ray CalculateCursorRay(GraphicsDevice device, Matrix projectionMatrix, Matrix viewMatrix,
                                             Vector2 Position)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            var nearSource = new Vector3(Position, 0f);
            var farSource = new Vector3(Position, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = device.Viewport.Unproject(nearSource,
                                                          projectionMatrix, viewMatrix, Matrix.Identity);

            Vector3 farPoint = device.Viewport.Unproject(farSource,
                                                         projectionMatrix, viewMatrix, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }
    }
}
