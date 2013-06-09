using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Laikos
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

        public void AddNeighs(Triangle lNeigh, Triangle rNeigh, Triangle bNeigh)
        {
            this.lNeigh = lNeigh;
            this.rNeigh = rNeigh;
            this.bNeigh = bNeigh;

            if (lChild != null)
            {
                Triangle bNeighRightChild = null;
                Triangle bNeighLeftChild = null;
                Triangle lNeighRightChild = null;
                Triangle rNeighLeftChild = null;

                if (bNeigh != null)
                {
                    bNeighLeftChild = bNeigh.lChild;
                    bNeighRightChild = bNeigh.rChild;
                }
                 
                if (lNeigh != null)
                    lNeighRightChild = lNeigh.rChild;
                if (rNeigh != null)
                    rNeighLeftChild = rNeigh.lChild;

                lChild.AddNeighs(rChild, bNeighRightChild, lNeighRightChild);
                rChild.AddNeighs(bNeighLeftChild, lChild, rNeighLeftChild);
            }
        }

        public void AddIndices(ref List<int> indicesList)
        {
            indicesList.Add(lInd);
            indicesList.Add(tInd);
            indicesList.Add(rInd);
        }
    }
}
