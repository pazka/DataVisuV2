using System;
using System.Collections.Generic;
using DataProcessing.City;
using DataProcessing.Density;
using DataProcessing.Generic;
using DataProcessing.Ril;

public static class FactoryDataReader
{
    public enum AvailableDataReaderTypes
    {
        CITY,
        RIL,
        DENSITY
    }

    private static readonly Dictionary<AvailableDataReaderTypes, IDataReader> instances =
        new Dictionary<AvailableDataReaderTypes, IDataReader>();

    public static IDataReader GetInstance(AvailableDataReaderTypes dataReaderType)
    {
        switch (dataReaderType)
        {
            case AvailableDataReaderTypes.CITY:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new CityDataReader());

                return instances[dataReaderType];

            case AvailableDataReaderTypes.DENSITY:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new DensityDataReader());

                return instances[dataReaderType];

            case AvailableDataReaderTypes.RIL:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new RilDataReader());

                return instances[dataReaderType];

            default:
                throw new Exception("DataReaderType isn't implemented : " + dataReaderType);
        }
    }
}