﻿using System.Collections.Generic;
using DataProcessing.Generic;
using DataProcessing.Ril;

namespace DataProcessing
{
    public class FactoryDataExtrapolator
    {
        
        public enum AvailableDataExtrapolatorTypes
        {
            RIL
        };

        private static Dictionary<AvailableDataExtrapolatorTypes, IDataExtrapolator> instances = new Dictionary<AvailableDataExtrapolatorTypes, IDataExtrapolator>();

        public static IDataExtrapolator GetInstance(AvailableDataExtrapolatorTypes dataExtrapolatorType)
        {
            switch (dataExtrapolatorType)
            {
                case AvailableDataExtrapolatorTypes.RIL:
                    if (!instances.ContainsKey(dataExtrapolatorType))
                        instances.Add(dataExtrapolatorType, new RilDataExtrapolator());

                    return instances[dataExtrapolatorType];

                default:
                    throw new System.Exception("DataExtrapolatorType isn't implemented : " + dataExtrapolatorType);
            }
        }
    }
}