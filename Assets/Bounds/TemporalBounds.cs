namespace Bounds
{
    internal class TimeBounds : GenericBound
    {
        /**
         * | MinT | MaxT |
         */
        private readonly float[] bounds = new float[2];

        public TimeBounds()
        {
            bounds[0] = float.PositiveInfinity;
            bounds[1] = float.NegativeInfinity;
        }

        public override object GetCurrentBounds()
        {
            return bounds;
        }

        public override void RegisterNewBounds(object data)
        {
            var proposedBound = (float) data;

            RegisterNewBounds(proposedBound);
        }

        public void RegisterNewBounds(float proposedBound)
        {
            if (!CanRegisterNewBounds)
                return;

            if (proposedBound < bounds[0]) bounds[0] = proposedBound;

            if (proposedBound > bounds[1]) bounds[1] = proposedBound;
        }
    }
}