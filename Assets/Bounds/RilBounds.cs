namespace Bounds
{
    public class RilBounds : GenericBound
    {
        private readonly float[] nbLogBounds = new float[2] {float.PositiveInfinity, float.NegativeInfinity};

        public override object GetCurrentBounds()
        {
            return nbLogBounds;
        }

        public override void RegisterNewBounds(object data)
        {
            if (!CanRegisterNewBounds) return;

            if ((float) data < nbLogBounds[0])
                nbLogBounds[0] = (float) data;
            else if ((float) data > nbLogBounds[1])
                nbLogBounds[1] = (float) data;
        }
    }
}