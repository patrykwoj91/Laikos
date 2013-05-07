using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.Linq;
using System.Text;

namespace Laikos
{
    class LightManager
    {
        List<DirectionalLight> directionalLights;
        List<PointLight> pointLights;

        public List<DirectionalLight> getDirectionalLights() { return directionalLights; }

        public LightManager()
        {
            directionalLights = new List<DirectionalLight>();
            pointLights = new List<PointLight>();
        }

        public void AddLight(DirectionalLight light)
        {
            directionalLights.Add(light);
        }

        public void AddLight(PointLight light)
        {
            pointLights.Add(light);
        }

        public void RemoveLight(DirectionalLight light)
        {
            directionalLights.Remove(light);
        }

        public void RemoveLight(PointLight light)
        {
            pointLights.Remove(light);
        }

        public void CreateLightMap()
        {
            foreach (DirectionalLight light in directionalLights)
                light.CreateLightMap();
            foreach (PointLight light in pointLights)
                light.CreateLightMap();
        }

        public void RemoveAllLights()
        {
            directionalLights.Clear();
            pointLights.Clear();
        }
    }
}
