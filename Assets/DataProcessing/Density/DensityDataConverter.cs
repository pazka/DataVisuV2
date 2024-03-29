﻿using System.Collections.Generic;
using Bounds;
using DataProcessing.Generic;

namespace DataProcessing.Density
{
    public class DensityDataConverter : DataConverter
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

        public DensityDataConverter()
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

        private float ConvertX(float rawX, float[,] geoBounds)
        {
            float delX = geoBounds[0, 1] - geoBounds[0, 0];
            float delY = geoBounds[1, 1] - geoBounds[1, 0];

            //prepare ratio for getting coords in bounds
            float dataBoundsXYRatio = delX / delY;
            float widthAsRatioOfOriginalTotalWidth = ((rawX - geoBounds[1, 0]) / delY);

            return widthAsRatioOfOriginalTotalWidth * screenBounds[0];
        }

        private float ConvertY(float rawY, float[,] geoBounds)
        {
            float delX = geoBounds[0, 1] - geoBounds[0, 0];
            float delY = geoBounds[1, 1] - geoBounds[1, 0];

            //prepare ratio for getting coords in bounds
            float dataBoundsXYRatio = delX / delY;

            // Y is set as the % of total orginal height * the current width * the old % totalwith by totalheight 
            float heightAsRatioOfOriginalTotalHeight = ((rawY - geoBounds[0, 0]) / delX);
            float newMaxYHeight = dataBoundsXYRatio * screenBounds[1];

            return heightAsRatioOfOriginalTotalHeight * newMaxYHeight;
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

            //OPTIONAL make scalling modulable gien screen size
            for (int i = 0; i < densityData.Count; i++)
            {
                //voluntary h/w geo inversion
                densityData[i].SetX1(ConvertX(densityData[i].RawY1, geoBounds));
                densityData[i].SetY1(ConvertY(densityData[i].RawX1, geoBounds));

                densityData[i].SetX2(ConvertX(densityData[i].RawY2, geoBounds));
                densityData[i].SetY2(ConvertY(densityData[i].RawX2, geoBounds));

                densityData[i].SetX3(ConvertX(densityData[i].RawY3, geoBounds));
                densityData[i].SetY3(ConvertY(densityData[i].RawX3, geoBounds));

                densityData[i].SetX4(ConvertX(densityData[i].RawY4, geoBounds));
                densityData[i].SetY4(ConvertY(densityData[i].RawX4, geoBounds));
            }

            allData = densityData;
            return densityData;
        }

        public override IData[] GetDataBounds()
        {
            return this.dataBounds;
        }

        //return X : max,min; Y: max,min
        public override IDataReader GetDataReader()
        {
            return densityDataReader;
        }
    }
}
