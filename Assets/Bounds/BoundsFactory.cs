using System;
using System.Collections.Generic;

namespace Bounds
{
    internal static class BoundsFactory
    {
        public enum AvailableBoundsTypes
        {
            GEOGRAPHIC,
            BATIMENT,
            TIME,
            RIL,
            SIRENE
        }

        private static readonly Dictionary<AvailableBoundsTypes, IBounds> instances =
            new Dictionary<AvailableBoundsTypes, IBounds>();

        public static IBounds GetInstance(AvailableBoundsTypes boundsType)
        {
            switch (boundsType)
            {
                case AvailableBoundsTypes.GEOGRAPHIC:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new GeographicBounds());
                    break;

                case AvailableBoundsTypes.TIME:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new TimeBounds());
                    break;

                case AvailableBoundsTypes.BATIMENT:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new GeographicBatBounds());
                    break;

                case AvailableBoundsTypes.RIL:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new RilBounds());
                    break;

                case AvailableBoundsTypes.SIRENE:
                    if (!instances.ContainsKey(boundsType))
                        instances.Add(boundsType, new SireneBounds());
                    break;

                default:
                    throw new Exception("BoundsType isn't implemented : " + boundsType);
            }
            
            return instances[boundsType];
        }
    }
}