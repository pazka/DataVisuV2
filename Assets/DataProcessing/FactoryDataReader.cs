using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FactoryDataReader
{
    public enum AvailableDataReaderTypes
    {
        CITY,LUMINOSITY
    };

    private static Dictionary<AvailableDataReaderTypes, IDataReader> instances = new Dictionary<AvailableDataReaderTypes, IDataReader>();

    public static IDataReader GetInstance(AvailableDataReaderTypes dataReaderType)
    {
        switch (dataReaderType)
        {
            case AvailableDataReaderTypes.CITY:
                if (!instances.ContainsKey(dataReaderType))
                    instances.Add(dataReaderType, new CityDataReader());

                return instances[dataReaderType];

            default:
                throw new System.Exception("DataReaderType isn't implemented : " + dataReaderType);
        }
    }
}