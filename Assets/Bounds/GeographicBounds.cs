using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Bounds
{
    class GeographicBounds : IBounds
    {
        /**
         * |---------- |
         * | MinX|MaxX |
         * |-----|-----|
         * | MinY|MaxY |
         * |-----------|
         */
        private float[,] bounds = new float[2,2];

        public GeographicBounds()
        {
            this.bounds[0, 0] = float.PositiveInfinity;
            this.bounds[0, 1] = float.NegativeInfinity;
            this.bounds[1, 0] = float.PositiveInfinity;
            this.bounds[1, 1] = float.NegativeInfinity;
        }

        public object GetCurrentBounds()
        {
            return bounds;
        }

        public void RegisterNewBounds(object data)
        {
            float[] proposedBound = (float[])data;

            this.RegisterNewBounds(proposedBound);
        }

        public void RegisterNewBounds(float[] proposedBound)
        {
            if (proposedBound[0] < this.bounds[0, 0])
            {
                this.bounds[0, 0] = proposedBound[0];
            }

            if (proposedBound[0] > this.bounds[0, 1])
            {
                this.bounds[0, 1] = proposedBound[0];
            }

            if (proposedBound[1] < this.bounds[1, 0])
            {
                this.bounds[1, 0] = proposedBound[1];
            }

            if (proposedBound[1] > this.bounds[1, 1])
            {
                this.bounds[1, 1] = proposedBound[1];
            }
        }
    }
}
