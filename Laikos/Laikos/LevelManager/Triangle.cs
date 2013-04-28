using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Laikos.LevelManager
{
    class Triangle
    {
        Triangle lNeigh;
        Triangle rNeigh;
        Triangle bNeigh;

        Triangle parent;
        Triangle lChild;
        Triangle rChild;

        Vector3 lPos;
        Vector3 centerPos;

        int tInd;
        int lInd;
        int rInd;

        public bool split = false;
        public bool addedToMergeList = false;

        public Triangle(Triangle parent, Vector2 tPoint, Vector2 lPoint, Vector2 rPoint, float[,] heightData)
        {
            int resolution = heightData.GetLength(0);
            tInd = (int)(tPoint.X + tPoint.Y * resolution);
            lInd = (int)(lPoint.X + lPoint.Y * resolution);
            rInd = (int)(rPoint.X + rPoint.Y * resolution);

            lPos = new Vector3(lPoint.X, heightData[(int)lPoint.X, (int)lPoint.Y], lPoint.Y);
            Vector2 center = (lPoint + rPoint) / 2;
            centerPos = new Vector3(center.X, heightData[(int)center.X, (int)center.Y], center.Y);

            this.parent = parent;
            if (Vector2.Distance(lPoint, tPoint) > 1)
            {
                lChild = new Triangle(this, center, tPoint, lPoint, heightData);
                rChild = new Triangle(this, center, rPoint, tPoint, heightData);
            }
        }
    }
}
