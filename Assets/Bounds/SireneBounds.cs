namespace Bounds
{
    public class SireneBounds : GenericBound
    {
        private readonly float[] EntityCount = new float[2] {float.PositiveInfinity, float.NegativeInfinity};

        public override object GetCurrentBounds()
        {
            return EntityCount;
        }

        public override void RegisterNewBounds(object data)
        {
            if (!CanRegisterNewBounds) return;

            if ((float) data < EntityCount[0])
                EntityCount[0] = (float) data;
            else if ((float) data > EntityCount[1])
                EntityCount[1] = (float) data;
        }
    }
}