using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityDrawer : MonoBehaviour
{
    // Creates a line renderer that follows a Sin() function
    // and animates it.
    Mesh _cityBoundsMesh;
    CityDataManager _cityDataManager;
    DensityDataManager _densityDataManager;
    LineRenderer _lineRenderer;

    void Start()
    {
        //Prepare entities
        _cityBoundsMesh = new Mesh();
        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _cityDataManager = (CityDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.CITY);
        _cityDataManager.Init(1920, 1080);

        _densityDataManager = (DensityDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.DENSITY);
        _densityDataManager.Init(1920, 1080);

        //Prepare Linerenderer
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.material.color = Color.blue;
        _lineRenderer.useWorldSpace = false;
        gameObject.transform.position = new Vector3(-960, -540, -1);
        _lineRenderer.loop = true;

        _lineRenderer.startWidth = 1f;
        _lineRenderer.endWidth = 1f;

        //link LineRenderer to Data
        Vector3[] cityData;
        Vector3[] densityData;
        cityData = _cityDataManager.GetAllVectoredData();
        densityData = _densityDataManager.GetAllVectoredData();

        _lineRenderer.positionCount = cityData.Length;
        _lineRenderer.SetPositions(cityData);


        _lineRenderer.positionCount = densityData.Length;
        _lineRenderer.SetPositions(densityData);

        //TODO : Bake when unity is less shitty
        //_lineRenderer.BakeMesh(_cityBoundsMesh, true);
    }

    void Update()
    {
        //TODO : Bake when unity is less shitty
        //_lineRenderer.SetPositions(_cityBoundsMesh.vertices);
    }
}
