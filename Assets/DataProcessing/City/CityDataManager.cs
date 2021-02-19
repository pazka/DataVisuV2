using System.Collections.Generic;
using Assets.Bounds;

namespace DataProcessing.City
{
    public class CityDataManager : DataManager
    {
        private CityDataReader cityDataReader;
    
        int[] screenBounds = new int[2];
        GeographicBounds geoBounds;


        //tmps Big Vars
        private IEnumerable<IData> allData;

        public CityDataManager()
        {
            this.cityDataReader = (CityDataReader)FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.CITY);
            this.geoBounds = (GeographicBounds)BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.GEOGRAPHIC);
        }

        public override void Init(int screenBoundX, int screenBoundY)
        {
            screenBounds = new int[2] { screenBoundX, screenBoundY};
            cityDataReader.Init();
        } 

        public override void Clean()
        {
            this.screenBounds = new int[2];
            allData = null;

            cityDataReader.Clean();
        }

        //Obligatory step that all data has to go through either if gotten from a batch or
        // gotten one by one
        private CityData RegisterData(CityData cityData)
        {
            this.geoBounds.RegisterNewBounds(new float[] { cityData.RawX, cityData.RawY });

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

            this.geoBounds.StopRegisteringNewBounds();

            //Transforming raw data by converting to screen next

            //prepare ratio for getting coords in bounds
            //OPTIONAL make scalling modulable gien screen size

            float[,] geoBounds = (float[,])this.geoBounds.GetCurrentBounds();

            float dataBoundsXYRatio =  (geoBounds[0, 1] - geoBounds[0, 0]) / ((geoBounds[1, 1] - geoBounds[1, 0]))   ;

            for (int i = 0; i < cityData.Count; i++)
            {
                //voluntary inversion
                float widthAsRatioOfOriginalTotalWidth = ((cityData[i].RawY - geoBounds[1, 0]) / (geoBounds[1, 1] - geoBounds[1, 0]));
                cityData[i].SetX(widthAsRatioOfOriginalTotalWidth  * screenBounds[0]);

                // Y is set as the % of total orginal height * the current width * the old % totalwith by totalheight 
                float heightAsRatioOfOriginalTotalHeight = ((cityData[i].RawX - geoBounds[0, 0]) / (geoBounds[0, 1] - geoBounds[0, 0]));
                float newMaxYHeight = dataBoundsXYRatio * screenBounds[1];
                cityData[i].SetY( heightAsRatioOfOriginalTotalHeight  * newMaxYHeight);
            }

            this.allData = cityData;
            return cityData;
        }

 

        //return X : max,min; Y: max,min
        public override IDataReader GetDataReader()
        {
            return cityDataReader;
        }

        public override IData[] getDataBounds()
        {
            throw new System.NotImplementedException();
        }
    }
}
