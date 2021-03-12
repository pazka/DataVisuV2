using System;

namespace Bounds
{
    public class RilBounds : GenericBound
    {
        
        private float[] nbLogBounds = new float[2]{float.PositiveInfinity,float.NegativeInfinity};
        private bool canRegisterNewBounds = true;


        public override object GetCurrentBounds()
        {
            return nbLogBounds;
        }

        public override void RegisterNewBounds(object data)
        {
            if (!canRegisterNewBounds) return;
            
            if ((float)data < nbLogBounds[0])
                nbLogBounds[0] = (float)data;
            else if ((float)data > nbLogBounds[1])
                nbLogBounds[1] = (float)data;
        }

        public override void StopRegisteringNewBounds()
        {
            canRegisterNewBounds = false;
        }
    }
}