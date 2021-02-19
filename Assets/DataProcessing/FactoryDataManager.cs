using System.Collections;
using System.Collections.Generic;
using DataProcessing.City;
using DataProcessing.Density;
using DataProcessing.Ril;
using UnityEngine;

public static class FactoryDataManager 
{
    public enum AvailableDataManagerTypes
    {
        CITY, RIL,DENSITY
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

            case AvailableDataManagerTypes.DENSITY:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new DensityDataManager());

                return instances[dataManagerType];

            case AvailableDataManagerTypes.RIL:
                if (!instances.ContainsKey(dataManagerType))
                    instances.Add(dataManagerType, new RilDataManager());

                return instances[dataManagerType];

            default:
                throw new System.Exception("DataManagerType isn't implemented : " + dataManagerType);
        }
    }
}
