using System;
using System.Collections.Generic;
using DataProcessing.Generic;
using DataProcessing.Ril;

namespace DataProcessing
{
    public class FactoryDataExtrapolator
    {
        public enum AvailableDataExtrapolatorTypes
        {
            RIL
        }

        private static readonly Dictionary<AvailableDataExtrapolatorTypes, IDataExtrapolator> instances =
            new Dictionary<AvailableDataExtrapolatorTypes, IDataExtrapolator>();

        public static IDataExtrapolator GetInstance(AvailableDataExtrapolatorTypes dataExtrapolatorType)
        {
            switch (dataExtrapolatorType)
            {
                case AvailableDataExtrapolatorTypes.RIL:
                    if (!instances.ContainsKey(dataExtrapolatorType))
                        instances.Add(dataExtrapolatorType, new RilDataExtrapolatorOld());

                    return instances[dataExtrapolatorType];

                default:
                    throw new Exception("DataExtrapolatorType isn't implemented : " + dataExtrapolatorType);
            }
        }
    }
}