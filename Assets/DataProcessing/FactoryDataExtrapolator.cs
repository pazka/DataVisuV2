using System;
using System.Collections.Generic;
using DataProcessing.Generic;
using DataProcessing.Ril;
using DataProcessing.Sirene;

namespace DataProcessing
{
    public class FactoryDataExtrapolator
    {
        public enum AvailableDataExtrapolatorTypes
        {
            RIL,
            SIRENE
        }

        private static readonly Dictionary<AvailableDataExtrapolatorTypes, IDataExtrapolator> instances =
            new Dictionary<AvailableDataExtrapolatorTypes, IDataExtrapolator>();

        public static IDataExtrapolator GetInstance(AvailableDataExtrapolatorTypes dataExtrapolatorType)
        {
            switch (dataExtrapolatorType)
            {
                case AvailableDataExtrapolatorTypes.RIL:
                    if (!instances.ContainsKey(dataExtrapolatorType))
                        instances.Add(dataExtrapolatorType, new RilDataExtrapolatorBias());
                    break;

                case AvailableDataExtrapolatorTypes.SIRENE:
                    if (!instances.ContainsKey(dataExtrapolatorType))
                        instances.Add(dataExtrapolatorType, new SireneDataExtrapolatorBiasOnePerson());
                    break;

                default:
                    throw new Exception("DataExtrapolatorType isn't implemented : " + dataExtrapolatorType);
            }

            return instances[dataExtrapolatorType];
        }
    }
}