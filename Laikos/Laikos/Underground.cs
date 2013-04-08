using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Laikos
{
    class Underground : Terrain
    {
        private Camera camera;

        public Underground(Game game, Camera camera) : base(game)
        {
            this.camera = camera;
        }

        public override void Initialize()
        {
            device = Game.GraphicsDevice;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            effect = Game.Content.Load<Effect>("effects");
            this.heightMap = Game.Content.Load<Texture2D>("Models/Terrain/Heightmaps/heightmap2");
            sandTexture = Game.Content.Load<Texture2D>("Models/Terrain/Textures/sand");
            LoadHeightData();
            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            CopyToBuffer();
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            effect.CurrentTechnique = effect.Techniques["Textured"];
            effect.Parameters["xTexture"].SetValue(sandTexture);

            effect.Parameters["xView"].SetValue(camera.viewMatrix);
            effect.Parameters["xProjection"].SetValue(camera.projectionMatrix);
            effect.Parameters["xWorld"].SetValue(SetWorldMatrix());

            Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
            lightDirection.Normalize();
            effect.Parameters["xLightDirection"].SetValue(lightDirection);
            effect.Parameters["xAmbient"].SetValue(0.6f);
            effect.Parameters["xEnableLighting"].SetValue(false);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                device.Indices = this.indexBuffer;
                device.SetVertexBuffer(this.vertexBuffer);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Length, 0, indices.Length / 3);
            }

            base.Draw(gameTime);
        }

        public Matrix SetWorldMatrix()
        {
            Matrix worldMatrix = Matrix.CreateTranslation(-50, 0, 0);
            return worldMatrix;
        }
    }
}
