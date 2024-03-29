﻿using System;
using System.Collections.Generic;
using DataProcessing.City;
using DataProcessing.Density;
using DataProcessing.Generic;
using DataProcessing.Ril;

public static class FactoryDataConverter
{
    public enum AvailableDataManagerTypes
    {
        CITY,
        RIL,
        DENSITY
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

                return instances[dataManagerType];

            case AvailableDataManagerTypes.DENSITY:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new DensityDataConverter());

                return instances[dataManagerType];

            case AvailableDataManagerTypes.RIL:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new RilDataConverter());

                return instances[dataManagerType];

            default:
                throw new Exception("DataManagerType isn't implemented : " + dataManagerType);
        }
    }
}