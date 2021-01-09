using Assets.Bounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Bounds
{
    static class BoundsFactory
    {
        public enum AvailableBoundsTypes
        {
            GEOGRAPHIC, SCREEN
        };

        private static Dictionary<AvailableBoundsTypes, IBounds> instances = new Dictionary<AvailableBoundsTypes, IBounds>();

        public static IBounds GetInstance(AvailableBoundsTypes boundsType)
        {
            switch (boundsType)
            {
                case AvailableBoundsTypes.GEOGRAPHIC:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new GeographicBounds());

                    return instances[boundsType];

                default:
                    throw new System.Exception("BoundsType isn't implemented : " + boundsType);
            }
        }
    }
}
