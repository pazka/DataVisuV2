using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityDrawer : MonoBehaviour
{
    // Creates a line renderer that follows a Sin() function
    // and animates it.
    CityDataManager _cityDataManager;
    DensityDataManager _densityDataManager;
    LineRenderer _lineRenderer;

    void Start()
    {
        //Prepare entities
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        _densityDataManager = (DensityDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.DENSITY);
        _densityDataManager.Init(1920, 1080);
        

        
        
        
    }

    void Update()
    {
    }
}
