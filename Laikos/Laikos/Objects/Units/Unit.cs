using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Laikos
{
    public class Unit : GameObject
    {
        public bool walk, picked;
        public Message Message = null;

        public Unit()
            : base()
        {
            walk = false;
            picked = false;
        }

        public Unit(Game game, string path, Vector3 position, float scale = 1.0f, Vector3 rotation = default(Vector3), Message message = null)
            : base(game, path)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.Scale = scale;
            this.Message = message;
        }

        public void Update(GameTime gameTime)
        {
            /*if (walk)
            {
              player = currentModel.PlayClip(currentModel.Clips["Walk"]);
              player.Looping = true;
              
            }
            else
            {
             player = currentModel.PlayClip(currentModel.Clips["Idle"]);
             // player.Looping = true;
            }*/

            Input.HandleUnit(ref walk, ref lastPosition, ref Position, ref Rotation, picked);
            base.Update(gameTime);
        }

        public void Draw(GraphicsDeviceManager graphics)
        {
            base.Draw(graphics);
        }
    }
}
