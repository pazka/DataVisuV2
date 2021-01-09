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
    LineRenderer _lineRenderer;
    Vector3[] cityData;

    void Start()
    {
        //Prepare entities
        _cityBoundsMesh = new Mesh();

        //The game object will inherit from the canvas ! 
        //so you need to have a canvas above in the hierarchy
        gameObject.transform.position = new Vector3(0, 0, -1);
        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _cityDataManager = (CityDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.CITY);
        _cityDataManager.Init(Screen.width, Screen.height);

        //Prepare Linerenderer
        _lineRenderer.sortingOrder = 1;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.material.color = Color.blue;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.loop = true;

        _lineRenderer.startWidth = 1f;
        _lineRenderer.endWidth = 1f;

        //link LineRenderer to Data
        cityData = _cityDataManager.GetAllVectoredData();
        _lineRenderer.positionCount = cityData.Length;
        _lineRenderer.SetPositions(cityData);
        //TODO : Bake when unity is less shitty
        //_lineRenderer.BakeMesh(_cityBoundsMesh, true);
    }

    void Update()
    {
        //TODO : Bake when unity is less shitty
        //_lineRenderer.SetPositions(_cityBoundsMesh.vertices);
    }
}
