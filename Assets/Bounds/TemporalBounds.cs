using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Bounds
{
    class TimeBounds : GenericBound
    {
        /**
         * | MinT | MaxT |
         */
        private float[] bounds = new float[2];
        private bool canRegisterNewBounds = true;

        public TimeBounds()
        {
            this.bounds[0] = float.PositiveInfinity;
            this.bounds[1] = float.NegativeInfinity;
        }

        override public object GetCurrentBounds()
        {
            return bounds;
        }

        override public void RegisterNewBounds(object data)
        {
            float proposedBound = (float)data;

            this.RegisterNewBounds(proposedBound);
        }

        public void RegisterNewBounds(float proposedBound)
        {
            if (!canRegisterNewBounds)
                return;

            if (proposedBound < this.bounds[0])
            {
                this.bounds[0] = proposedBound;
            }

            if (proposedBound > this.bounds[1])
            {
                this.bounds[1] = proposedBound;
            }
        }

        public void StopRegisteringNewBounds()
        {
            canRegisterNewBounds = false;
        }
    }
}