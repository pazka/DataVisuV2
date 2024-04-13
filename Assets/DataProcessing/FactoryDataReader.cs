using System;
using System.Collections.Generic;
using DataProcessing.City;
using DataProcessing.Density;
using DataProcessing.Generic;
using DataProcessing.Ril;
using DataProcessing.Sirene;

public static class FactoryDataReader
{
    public enum AvailableDataReaderTypes
    {
        CITY,
        RIL,
        DENSITY,
        SIRENE
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
                break;

            case AvailableDataReaderTypes.DENSITY:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new DensityDataReader());
                break;

            case AvailableDataReaderTypes.RIL:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new RilDataReader());
                break;

            case AvailableDataReaderTypes.SIRENE:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new SireneDataReader());
                break;

            default:
                throw new Exception("DataReaderType isn't implemented : " + dataReaderType);
        }

        return instances[dataReaderType];
    }
}