using Assets.Bounds;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityDataManager : DataManager
{
    private DensityDataReader densityDataReader;
    
    int[] screenBounds = new int[2];

    /**
     * | minData | maxData |
     */
    DensityData[] dataBounds = new DensityData[2];
    float densitySquareSize = 0;
    GeographicBounds geoBounds;

    //tmps Big Vars
    private IEnumerable<IData> allData;

    public float DensitySquareSize { get => densitySquareSize; set => densitySquareSize = value; }

    public DensityDataManager()
    {
        this.densityDataReader = (DensityDataReader)FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.DENSITY);
        this.geoBounds = (GeographicBounds)BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.GEOGRAPHIC);
    }

    public override void Init(int screenBoundX, int screenBoundY)
    {
        screenBounds = new int[2] { screenBoundX, screenBoundY };
        densityDataReader.Init();
    }

    public override void Clean()
    {
        this.screenBounds = new int[2];
        this.densitySquareSize = 0;
        allData = null;

        densityDataReader.Clean();
    }

    /// <summary>
    /// Important method that will see all data pass through. 
    /// It will update all Meta-Data during the reading. 
    /// </summary>
    /// <param name="densityData"> Data to register </param>
    /// <returns></returns>
    private DensityData RegisterData(DensityData densityData)
    {
        //first data init
        if(this.densitySquareSize == 0)
            this.densitySquareSize = densityData.H;

        if (this.dataBounds[0] == null)
        {
            this.dataBounds[0] = new DensityData(densityData);
            this.dataBounds[1] = new DensityData(densityData);
        }

        this.geoBounds.RegisterNewBounds(new float[] { densityData.RawX, densityData.RawY });


        if (densityData.M25ans < this.dataBounds[0].M25ans)
            this.dataBounds[0].M25ans = densityData.M25ans;
        if (densityData.M25ans > this.dataBounds[1].M25ans)
            this.dataBounds[1].M25ans = densityData.M25ans;

        if (densityData.Pop < this.dataBounds[0].Pop)
            this.dataBounds[0].Pop = densityData.Pop;
        if (densityData.Pop > this.dataBounds[1].Pop)
            this.dataBounds[1].Pop = densityData.Pop;

        if (densityData.Rev < this.dataBounds[0].Rev)
            this.dataBounds[0].Rev = densityData.Rev;
        if (densityData.Rev > this.dataBounds[1].Rev)
            this.dataBounds[1].Rev = densityData.Rev;

        if (densityData.P65ans < this.dataBounds[0].P65ans)
            this.dataBounds[0].P65ans = densityData.P65ans;
        if (densityData.P65ans > this.dataBounds[1].P65ans)
            this.dataBounds[1].P65ans = densityData.P65ans;

        if (densityData.Men_basr < this.dataBounds[0].Men_basr)
            this.dataBounds[0].Men_basr = densityData.Men_basr;
        if (densityData.Men_basr > this.dataBounds[1].Men_basr)
            this.dataBounds[1].Men_basr = densityData.Men_basr;

        if (densityData.Men > this.dataBounds[1].Men)
            this.dataBounds[1].Men = densityData.Men;
        if (densityData.Men < this.dataBounds[0].Men)
            this.dataBounds[0].Men = densityData.Men;

        if (densityData.Men_coll < this.dataBounds[0].Men_coll)
            this.dataBounds[0].Men_coll = densityData.Men_coll;
        if (densityData.Men_coll > this.dataBounds[1].Men_coll)
            this.dataBounds[1].Men_coll = densityData.Men_coll;

        if (densityData.Men_prop < this.dataBounds[0].Men_prop)
            this.dataBounds[0].Men_prop = densityData.Men_prop;
        if (densityData.Men_prop > this.dataBounds[1].Men_prop)
            this.dataBounds[1].Men_prop = densityData.Men_prop;

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
        
        //get all raw data first
        while (!densityDataReader.EndOfStream)
        {
            tmpData = (DensityData)GetNextData();
            if (!densityDataReader.EndOfStream)
            {
                densityData.Add(tmpData);
            }
        }

        float[,] geoBounds = (float[,])this.geoBounds.GetCurrentBounds();
        //Transforming raw data by converting to screen next

        //prepare ratio for getting coords in bounds
        //OPTIONAL make scalling modulable gien screen size
        float delX = geoBounds[0, 1] - geoBounds[0, 0];
        float delY = geoBounds[1, 1] - geoBounds[1, 0];
        float dataBoundsXYRatio =  delX / delY   ;

        for (int i = 0; i < densityData.Count; i++)
        {
            //voluntary h/w geo inversion
            float widthAsRatioOfOriginalTotalWidth = ((densityData[i].RawY - geoBounds[1, 0]) / delY);
            densityData[i].SetX(widthAsRatioOfOriginalTotalWidth  * screenBounds[0]);

            // Y is set as the % of total orginal height * the current width * the old % totalwith by totalheight 
            float heightAsRatioOfOriginalTotalHeight = ((densityData[i].RawX - geoBounds[0, 0]) / delX);
            float newMaxYHeight = dataBoundsXYRatio * screenBounds[1];
            densityData[i].SetY(screenBounds[1] - heightAsRatioOfOriginalTotalHeight  * newMaxYHeight);


            densityData[i].SetW(densityData[i].W * (screenBounds[0] / delX));
            densityData[i].SetH(densityData[i].H * (screenBounds[0] / delY));
        }

        allData = densityData;
        return densityData;
    }

    public override IData[] getDataBounds()
    {
        return this.dataBounds;
    }

    //return X : max,min; Y: max,min
    public override IDataReader GetDataReader()
    {
        return densityDataReader;
    }
}
