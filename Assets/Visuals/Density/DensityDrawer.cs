using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Visuals
{ 
public class DensityDrawer : MonoBehaviour
{
    // Creates a line renderer that follows a Sin() function
    // and animates it.
    CityDataManager _cityDataManager;
    DensityDataManager _densityDataManager;
    List<DensityData> _densityData = new List<DensityData>();
    DensityData[] _dataBounds;


    //visual vars
    int scaleGradientDetail = 5;
    GUIStyle[] colorScales = new GUIStyle[5];
    public Color[] gradientColors = {Color.blue, Color.magenta, Color.red, Color.green, Color.yellow};
    public SpriteRenderer _SpriteRenderer;
    public Transform spriteTransform;

    void Start()
    {
        //Prepare entities        
        _SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        _densityDataManager = (DensityDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.DENSITY);

        _densityDataManager.Init(Screen.width, Screen.height);
        //_densityDataManager.Init(1920, 1080, (float[,])FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.CITY).GetGeoBounds());

        for(Int16 i = 0; i < scaleGradientDetail; i++)
        {
            Texture2D square = new Texture2D(1, 1);
            square.SetPixel(0, 0, gradientColors[i]);
            square.wrapMode = TextureWrapMode.Repeat;
            square.Apply();

            colorScales[i] = new GUIStyle();
            colorScales[i].normal.background = square;
        }
    }

    public void FillWithData()
    {
        _densityData = (List<DensityData>)_densityDataManager.GetAllData();

        _dataBounds = (DensityData[])_densityDataManager.getDataBounds();
    }

    void Update()
    {

    }

    void OnGUI()
    {
        int i;
        for (i = 0;  i < colorScales.Length; i++)
        {
            GUI.Label(new Rect(
                i * 20, 
                5, 
                20, 
                20), 
                "" + (i + 1), 
                colorScales[i]
                );
        }

        foreach(DensityData densityData in _densityData)
        {
            i++;
            float tmpPop = densityData.Pop - _dataBounds[0].Pop;
            int indexSlice = (int) Math.Floor( tmpPop / (((_dataBounds[1].Pop + 1f) - _dataBounds[0].Pop) / scaleGradientDetail));

            GUILayout.BeginArea(new Rect(densityData.X, densityData.Y, densityData.W, densityData.H), colorScales[indexSlice]);
            GUILayout.EndArea();
        }
    }
}
}