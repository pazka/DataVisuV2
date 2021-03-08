using System.Collections.Generic;
using DataProcessing.City;
using UnityEngine;

namespace Visuals { 
public class CityDrawer : MonoBehaviour
{
    // Creates a line renderer that follows a Sin() function
    // and animates it.
    Mesh _cityBoundsMesh;
    CityDataConverter cityDataConverter;
    LineRenderer _lineRenderer;
    Vector3[] cityData;
   

    void Start()
    {
        //Prepare entities
        _cityBoundsMesh = new Mesh();

        //The game object will inherit from the canvas ! 
        //so you need to have a canvas above in the hierarchy
        gameObject.transform.position = new Vector3(0, 0, (float)VisualPlanner.Layers.City);
        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        cityDataConverter = (CityDataConverter)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.CITY);
        cityDataConverter.Init(Screen.width, Screen.height);

        //Prepare Linerenderer
        _lineRenderer.sortingOrder = 1;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.material.color = Color.blue;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.loop = true;

        _lineRenderer.startWidth = 1f;
        _lineRenderer.endWidth = 1f;

        //link LineRenderer to Data
        //TODO : Bake when unity is less shitty
        //_lineRenderer.BakeMesh(_cityBoundsMesh, true);

        this.LoadAllVectoredData();
    }

    public void FillWithData()
    {
        _lineRenderer.positionCount = cityData.Length;
        _lineRenderer.SetPositions(cityData);
    }


    public void LoadAllVectoredData()
    {
        if (this.cityData != null)
        {
            return;
        }

        List<CityData> cityData = (List<CityData>)cityDataConverter.GetAllData();

        this.cityData = new Vector3[cityData.Count];
        for (int i = 0; i < cityData.Count; i++)
        {
            this.cityData[i] = new Vector3(cityData[i].X, cityData[i].Y, 1);
        }
    }

    void Update()
    {
        //TODO : Bake when unity is less shitty
        //_lineRenderer.SetPositions(_cityBoundsMesh.vertices);
    }
}
}