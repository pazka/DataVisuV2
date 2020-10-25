using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityDataManager : DataManager
{
    private DensityDataReader densityDataReader;

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

    public DensityDataManager()
    {
        this.densityDataReader = (DensityDataReader)FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.DENSITY);
        this.bounds[0, 0] = float.PositiveInfinity;
        this.bounds[0, 1] = float.NegativeInfinity;
        this.bounds[1, 0] = float.PositiveInfinity;
        this.bounds[1, 1] = float.NegativeInfinity;

    }

    public override void Init(int screenBoundX, int screenBoundY)
    {
        screenBounds = new int[2] { screenBoundX, screenBoundY};
        densityDataReader.Init();
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

        densityDataReader.Clean();
    }

    private DensityData RegisterData(DensityData densityData)
    {
        if (densityData.RawX < bounds[0,0])
            bounds[0,0] = densityData.RawX;

        if (densityData.RawX > bounds[0,1])
            bounds[0,1] = densityData.RawX;

        if (densityData.RawY < bounds[1,0])
            bounds[1,0] = densityData.RawY;

        if (densityData.RawY > bounds[1,1])
            bounds[1,1] = densityData.RawY;

        return densityData;
    }

    public override IData GetNextData()
    {
        densityDataReader.GoToNextData();
        if (!densityDataReader.EndOfStream)
        {
            return RegisterData((DensityData)densityDataReader.GetData());
        }

        return null;
    }

    public override IEnumerable<IData> GetAllData()
    {
        if(allData != null)
        {
            return allData;
        }

        List<DensityData> densityData = new List<DensityData>();
        DensityData tmpData;

        //get raw data first
        while (!densityDataReader.EndOfStream)
        {
            tmpData = (DensityData)GetNextData();
            if (!densityDataReader.EndOfStream)
            {
                densityData.Add(tmpData);
            }
        }

        //Transforming raw data by converting to screen next

        //prepare ratio for getting coords in bounds
        //OPTIONAL make scalling modulable gien screen size
        
        float dataBoundsXYRatio =  (bounds[0, 1] - bounds[0, 0]) / ((bounds[1, 1] - bounds[1, 0]))   ;

        for (int i = 0; i < densityData.Count; i++)
        {
            //voluntary inversion
            float widthAsRatioOfOriginalTotalWidth = ((densityData[i].RawY - bounds[1, 0]) / (bounds[1, 1] - bounds[1, 0]));
            densityData[i].SetX(widthAsRatioOfOriginalTotalWidth  * screenBounds[0]);

            // Y is set as the % of total orginal height * the current width * the old % totalwith by totalheight 
            float heightAsRatioOfOriginalTotalHeight = ((densityData[i].RawX - bounds[0, 0]) / (bounds[0, 1] - bounds[0, 0]));
            float newMaxYHeight = dataBoundsXYRatio * screenBounds[1];
            densityData[i].SetY( heightAsRatioOfOriginalTotalHeight  * newMaxYHeight);
        }

        allData = densityData;
        return densityData;
    }

    public Vector3[] GetAllVectoredData()
    {
        if(allVectoredData != null)
        {
            return allVectoredData;
        }

        DensityData[] densityData = ((List<DensityData>)GetAllData()).ToArray();

        allVectoredData = new Vector3[densityData.Length];  
        for (int i = 0; i < densityData.Length; i++)
        {
            allVectoredData[i] = new Vector3(densityData[i].X, densityData[i].Y,1);
        }

        return allVectoredData;
    }

    public override object GetDataBounds()
    {
        return bounds;
    }

    //return X : max,min; Y: max,min
    public override IDataReader GetDataReader()
    {
        return densityDataReader;
    }
}
