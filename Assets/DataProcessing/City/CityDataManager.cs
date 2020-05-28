using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityDataManager : DataManager
{
    private CityDataReader cityDataReader;

    /*
     * |---------- |
     * | MinX|MaxX |
     * |-----|-----|
     * | MinY|MaxY |
     * |-----------|
     */
    private float[,] bounds = new float[2,2];
    int[] screenBounds = new int[2];


    //tmps Big Vars
    private IEnumerable<IData> allData;
    private Vector3[] allVectoredData;

    public CityDataManager()
    {
        this.cityDataReader = (CityDataReader)FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.CITY);
        this.bounds[0, 0] = float.PositiveInfinity;
        this.bounds[0, 1] = float.NegativeInfinity;
        this.bounds[1, 0] = float.PositiveInfinity;
        this.bounds[1, 1] = float.NegativeInfinity;

    }

    public override void Init(int screenBoundX, int screenBoundY)
    {
        screenBounds = new int[2] { screenBoundX, screenBoundY};
        cityDataReader.Init();
    } 

    public override void Clean()
    {
        this.bounds[0, 0] = float.PositiveInfinity;
        this.bounds[0, 1] = float.NegativeInfinity;
        this.bounds[1, 0] = float.PositiveInfinity;
        this.bounds[1, 1] = float.NegativeInfinity;
        this.screenBounds = new int[2];
        allData = null;
        allVectoredData = null;

        cityDataReader.Clean();
    }

    private CityData RegisterData(CityData cityData)
    {
        if (cityData.RawX < bounds[0,0])
            bounds[0,0] = cityData.RawX;

        if (cityData.RawX > bounds[0,1])
            bounds[0,1] = cityData.RawX;

        if (cityData.RawY < bounds[1,0])
            bounds[1,0] = cityData.RawY;

        if (cityData.RawY > bounds[1,1])
            bounds[1,1] = cityData.RawY;

        return cityData;
    }

    public override IData GetNextData()
    {
        cityDataReader.GoToNextData();
        if (!cityDataReader.EndOfStream)
        {
            return RegisterData((CityData)cityDataReader.GetData());
        }

        return null;
    }

    public override IEnumerable<IData> GetAllData()
    {
        if(allData != null)
        {
            return allData;
        }

        List<CityData> cityData = new List<CityData>();
        CityData tmpData;

        //get raw data first
        while (!cityDataReader.EndOfStream)
        {
            tmpData = (CityData)GetNextData();
            if (!cityDataReader.EndOfStream)
            {
                cityData.Add(tmpData);
            }
        }

        //Transforming raw data by converting to screen next

        //prepare ratio for getting coords in bounds
        //OPTIONAL make scalling modulable gien screen size
        
        float dataBoundsXYRatio =  (bounds[0, 1] - bounds[0, 0]) / ((bounds[1, 1] - bounds[1, 0]))   ;

        for (int i = 0; i < cityData.Count; i++)
        {
            //voluntary inversion
            float widthAsRatioOfOriginalTotalWidth = ((cityData[i].RawY - bounds[1, 0]) / (bounds[1, 1] - bounds[1, 0]));
            cityData[i].SetX(widthAsRatioOfOriginalTotalWidth  * screenBounds[0]);

            // Y is set as the % of total orginal height * the current width * the old % totalwith by totalheight 
            float heightAsRatioOfOriginalTotalHeight = ((cityData[i].RawX - bounds[0, 0]) / (bounds[0, 1] - bounds[0, 0]));
            float newMaxYHeight = dataBoundsXYRatio * screenBounds[1];
            cityData[i].SetY( heightAsRatioOfOriginalTotalHeight  * newMaxYHeight);
        }

        allData = cityData;
        return cityData;
    }

    public Vector3[] GetAllVectoredData()
    {
        if(allVectoredData != null)
        {
            return allVectoredData;
        }

        CityData[] cityData = ((List<CityData>)GetAllData()).ToArray();

        allVectoredData = new Vector3[cityData.Length];
        for (int i = 0; i < cityData.Length; i++)
        {
            allVectoredData[i] = new Vector3(cityData[i].X, cityData[i].Y,1);
        }

        return allVectoredData;
    }

    public override dynamic GetDataBounds()
    {
        return bounds;
    }

    //return X : max,min; Y: max,min
    public override IDataReader GetDataReader()
    {
        return cityDataReader;
    }
}
