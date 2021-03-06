﻿namespace Bounds
{
    class GeographicBatBounds : GenericBound
    {
        /**
         * |---------- |
         * | MinX|MaxX |
         * |-----|-----|
         * | MinY|MaxY |
         * |-----------|
         */
        private float[,] bounds = new float[2,2];

        public GeographicBatBounds()
        {
            this.bounds[0, 0] = float.PositiveInfinity;
            this.bounds[0, 1] = float.NegativeInfinity;
            this.bounds[1, 0] = float.PositiveInfinity;
            this.bounds[1, 1] = float.NegativeInfinity;
        }

        override public object GetCurrentBounds()
        {
            return bounds;
        }

        override public void RegisterNewBounds(object data)
        {
            float[] proposedBound = (float[])data;

            this.RegisterNewBounds(proposedBound);
        }

        public void RegisterNewBounds(float[] proposedBound)
        {
            if (!CanRegisterNewBounds)
                return;

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
