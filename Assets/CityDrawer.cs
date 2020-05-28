using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityDrawer : MonoBehaviour
{
    // Creates a line renderer that follows a Sin() function
    // and animates it.

    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    public float lineWidth = 1f;
    Vector3[] cityData;
    CityDataManager cityDataManager;

    void Start()
    {
        cityDataManager = (CityDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.CITY);
        cityDataManager.Init(1920, 1080);
        cityData = cityDataManager.GetAllVectoredData();

        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = Color.red;
        lineRenderer.positionCount = cityData.Length;
        lineRenderer.useWorldSpace = false;
        gameObject.transform.position = new Vector3(-960, -540, -1);

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;

    }

    void Update()
    {
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.SetPositions(cityData);
        lineRenderer.loop = true;
    }
}
