using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Laikos
{
    class Camera
    {
        //**************************************//
        //Variables created to store information
        //about camera
        //**************************************//

        public Matrix viewMatrix { get; set; }
        public Matrix projectionMatrix { get; set; }

        //**************************************//

        //Setting up view and projection matrix for camera
        public void SetUpCamera(GraphicsDevice device)
        {
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 150, 80), Vector3.Zero, Vector3.Forward);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
        }

    }
}
