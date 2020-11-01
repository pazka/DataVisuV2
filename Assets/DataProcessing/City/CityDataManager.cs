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
    private float[,] dataBounds = new float[2,2];
    int[] screenBounds = new int[2];


    //tmps Big Vars
    private IEnumerable<IData> allData;
    private Vector3[] allVectoredData;

    public CityDataManager()
    {
        this.cityDataReader = (CityDataReader)FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.CITY);
        this.dataBounds[0, 0] = float.PositiveInfinity;
        this.dataBounds[0, 1] = float.NegativeInfinity;
        this.dataBounds[1, 0] = float.PositiveInfinity;
        this.dataBounds[1, 1] = float.NegativeInfinity;

    }

    public override void Init(int screenBoundX, int screenBoundY)
    {
        screenBounds = new int[2] { screenBoundX, screenBoundY};
        cityDataReader.Init();
    } 

    public override void Clean()
    {
        this.dataBounds[0, 0] = float.PositiveInfinity;
        this.dataBounds[0, 1] = float.NegativeInfinity;
        this.dataBounds[1, 0] = float.PositiveInfinity;
        this.dataBounds[1, 1] = float.NegativeInfinity;
        this.screenBounds = new int[2];
        allData = null;
        allVectoredData = null;

        cityDataReader.Clean();
    }

    //Obligatory step that all data has to go through either if gotten from a batch or
    // gotten one by one
    private CityData RegisterData(CityData cityData)
    {
        if (cityData.RawX < dataBounds[0,0])
            dataBounds[0,0] = cityData.RawX;

        if (cityData.RawX > dataBounds[0,1])
            dataBounds[0,1] = cityData.RawX;

        if (cityData.RawY < dataBounds[1,0])
            dataBounds[1,0] = cityData.RawY;

        if (cityData.RawY > dataBounds[1,1])
            dataBounds[1,1] = cityData.RawY;
         
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
        //using the cache
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
        
        float dataBoundsXYRatio =  (dataBounds[0, 1] - dataBounds[0, 0]) / ((dataBounds[1, 1] - dataBounds[1, 0]))   ;

        for (int i = 0; i < cityData.Count; i++)
        {
            //voluntary inversion
            float widthAsRatioOfOriginalTotalWidth = ((cityData[i].RawY - dataBounds[1, 0]) / (dataBounds[1, 1] - dataBounds[1, 0]));
            cityData[i].SetX(widthAsRatioOfOriginalTotalWidth  * screenBounds[0]);

            // Y is set as the % of total orginal height * the current width * the old % totalwith by totalheight 
            float heightAsRatioOfOriginalTotalHeight = ((cityData[i].RawX - dataBounds[0, 0]) / (dataBounds[0, 1] - dataBounds[0, 0]));
            float newMaxYHeight = dataBoundsXYRatio * screenBounds[1];
            cityData[i].SetY( heightAsRatioOfOriginalTotalHeight  * newMaxYHeight);
        }

        this.allData = cityData;
        return cityData;
    }

    public Vector3[] GetAllVectoredData()
    {
        if(allVectoredData != null)
        {
            return allVectoredData;
        }

        List<CityData> cityData = (List<CityData>)GetAllData();

        allVectoredData = new Vector3[cityData.Count];  
        for (int i = 0; i < cityData.Count; i++)
        {
            allVectoredData[i] = new Vector3(cityData[i].X, cityData[i].Y,1);
        }

        return allVectoredData;
    }

    public override object GetDataBounds()
    {
        return dataBounds;
    }

    //return X : max,min; Y: max,min
    public override IDataReader GetDataReader()
    {
        return cityDataReader;
    }
}
