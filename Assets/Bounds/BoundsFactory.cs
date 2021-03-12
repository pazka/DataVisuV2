using System.Collections.Generic;

namespace Bounds
{
    static class BoundsFactory
    {
        public enum AvailableBoundsTypes
        {
            GEOGRAPHIC,BATIMENT, TIME, RIL
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

                case AvailableBoundsTypes.TIME:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new TimeBounds());

                    return instances[boundsType];

                case AvailableBoundsTypes.BATIMENT:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new GeographicBatBounds());

                    return instances[boundsType];

                case AvailableBoundsTypes.RIL:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new RilBounds());

                    return instances[boundsType];

                default:
                    throw new System.Exception("BoundsType isn't implemented : " + boundsType);
            }
        }
    }
}
