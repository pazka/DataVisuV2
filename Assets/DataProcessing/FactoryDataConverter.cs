using System;
using System.Collections.Generic;
using DataProcessing.City;
using DataProcessing.Density;
using DataProcessing.Generic;
using DataProcessing.Ril;
using DataProcessing.Sirene;

public static class FactoryDataConverter
{
    public enum AvailableDataManagerTypes
    {
        CITY,
        RIL,
        DENSITY,
        SIRENE
    }

    private static readonly Dictionary<AvailableDataManagerTypes, IDataConverter> instances =
        new Dictionary<AvailableDataManagerTypes, IDataConverter>();

    public static IDataConverter GetInstance(AvailableDataManagerTypes dataManagerType)
    {
        switch (dataManagerType)
        {
            case AvailableDataManagerTypes.CITY:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new CityDataConverter());
                break;

            case AvailableDataManagerTypes.DENSITY:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new DensityDataConverter());
                break;

            case AvailableDataManagerTypes.RIL:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new RilDataConverter());
                break;
            
            case AvailableDataManagerTypes.SIRENE:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new SireneDataConverter());
                break;

            default:
                throw new Exception("DataManagerType isn't implemented : " + dataManagerType);
        }

        return instances[dataManagerType];
    }
}