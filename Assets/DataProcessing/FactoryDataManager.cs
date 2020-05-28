using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FactoryDataManager 
{
    public enum AvailableDataManagerTypes
    {
        CITY, LUMINOSITY
    };

    private static Dictionary<AvailableDataManagerTypes, IDataManager> instances = new Dictionary<AvailableDataManagerTypes, IDataManager>();

    public static IDataManager GetInstance(AvailableDataManagerTypes dataManagerType)
    {
        switch (dataManagerType)
        {
            case AvailableDataManagerTypes.CITY:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new CityDataManager());

                return instances[dataManagerType];

            default:
                throw new System.Exception("DataManagerType isn't implemented : " + dataManagerType);
        }
    }
}
