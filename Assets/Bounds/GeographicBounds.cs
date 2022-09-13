namespace Bounds
{
    internal class GeographicBounds : GenericBound
    {
        /**
         * |---------- |
         * | MinX|MaxX |
         * |-----|-----|
         * | MinY|MaxY |
         * |-----------|
         */
        private readonly float[,] bounds = new float[2, 2];

        public GeographicBounds()
        {
            bounds[0, 0] = float.PositiveInfinity;
            bounds[0, 1] = float.NegativeInfinity;
            bounds[1, 0] = float.PositiveInfinity;
            bounds[1, 1] = float.NegativeInfinity;
        }

        public override object GetCurrentBounds()
        {
            return bounds;
        }

        public override void RegisterNewBounds(object data)
        {
            var proposedBound = (float[]) data;

            RegisterNewBounds(proposedBound);
        }

        public void RegisterNewBounds(float[] proposedBound)
        {
            if (!CanRegisterNewBounds)
                return;

            if (proposedBound[0] < bounds[0, 0]) bounds[0, 0] = proposedBound[0];

            if (proposedBound[0] > bounds[0, 1]) bounds[0, 1] = proposedBound[0];

            if (proposedBound[1] < bounds[1, 0]) bounds[1, 0] = proposedBound[1];

            if (proposedBound[1] > bounds[1, 1]) bounds[1, 1] = proposedBound[1];
        }
    }
}