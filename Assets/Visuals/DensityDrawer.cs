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

        //Prepare Linerenderer
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.material.color = Color.blue;
        _lineRenderer.useWorldSpace = false;
        gameObject.transform.position = new Vector3(-960, -540, -1);
        _lineRenderer.loop = true;

        _lineRenderer.startWidth = 1f;
        _lineRenderer.endWidth = 1f;

        //link LineRenderer to Data
        Vector3[] densityData;
        
        

        //TODO : Bake when unity is less shitty
        //_lineRenderer.BakeMesh(_cityBoundsMesh, true);
    }

    void Update()
    {
        //TODO : Bake when unity is less shitty
        //_lineRenderer.SetPositions(_cityBoundsMesh.vertices);
    }
}
