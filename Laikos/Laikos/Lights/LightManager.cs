using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Laikos
{
    class LightManager
    {
        public List<DirectionalLight> directionalLights;
        public List<PointLight> pointLights;
        public List<SpotLight> spotLights;

        private GraphicsDevice device;
        private Effect depthWriter;

        public List<DirectionalLight> getDirectionalLights() { return directionalLights; }
        public List<PointLight> getPointLights() { return pointLights; }
        public List<SpotLight> getSpotLights() { return spotLights; }

        public LightManager(ContentManager content, GraphicsDevice device)
        {
            directionalLights = new List<DirectionalLight>();
            pointLights = new List<PointLight>();
            spotLights = new List<SpotLight>();
            depthWriter = content.Load<Effect>("Effects/DepthWriter");
            this.device = device;
        }

        public void AddLight(DirectionalLight light)
        {
            directionalLights.Add(light);
        }

        public void AddLight(PointLight light)
        {
            pointLights.Add(light);
        }

        public void AddLight(SpotLight light)
        {
            spotLights.Add(light);
        }

        public void RemoveLight(DirectionalLight light)
        {
            directionalLights.Remove(light);
        }

        public void RemoveLight(PointLight light)
        {
            pointLights.Remove(light);
        }

        public void RemoveLight(SpotLight light)
        {
            spotLights.Remove(light);
        }

        public void RemoveAllLights()
        {
            directionalLights.Clear();
            pointLights.Clear();
            spotLights.Clear();
        }

        public void CreateLightMap()
        {
            foreach (DirectionalLight light in directionalLights)
                light.CreateLightMap();
            foreach (PointLight light in pointLights)
                light.CreateLightMap();
            foreach (SpotLight light in spotLights)
                light.CreateLightMap();
        }

        public void CreateShadowMap(List<GameObject> models, Terrain terrain)
        {
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;
            device.RasterizerState = RasterizerState.CullCounterClockwise;
            foreach (PointLight light in pointLights)
                if (light.withShadows)
                {
                    light.CreateShadowMap(light, models, depthWriter, terrain);
                }
            foreach (SpotLight light in spotLights)
                if (light.withShadows)
                {
                    light.CreateShadowMap(models, depthWriter, terrain);
                }

        }
    }
}
