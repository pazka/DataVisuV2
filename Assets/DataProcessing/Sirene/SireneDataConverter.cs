using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bounds;
using DataProcessing.City;
using DataProcessing.Generic;
using UnityEngine;
using Tools;

namespace DataProcessing.Sirene
{
    public class SireneDataConverter : DataConverter
    {
        private SireneDataReader sireneDataReader;

        int[] screenBounds = new int[2];
        int[] screenOffset = new int[2];
        GeographicBounds geoBounds; // From City Data , so that city and geo bound are aligned
        TimeBounds timeBounds;
        private SireneBounds dataBounds;


        //tmps Big Vars
        private IEnumerable<IData> allData;

        public SireneDataConverter()
        {
            this.sireneDataReader =
                (SireneDataReader) FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.SIRENE);
            this.geoBounds = (GeographicBounds)
                BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.GEOGRAPHIC);
            this.timeBounds =
                (TimeBounds) BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.TIME);
            this.dataBounds =
                (SireneBounds) BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.SIRENE);
        }


        public override void Init(int screenBoundX, int screenBoundY)
        {
            // var cityLineConverter = (CityDataConverter) FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes.CITY);
            // cityLineConverter.Init(screenBoundX, screenBoundY);
            // cityLineConverter.RegisterAllData(); // Do so so that geoBounds is set with correct bounds to convert our sirene data

            screenBounds = new int[2] {screenBoundX, screenBoundY};
            sireneDataReader.Init();
        }

        public void Init(int screenBoundX, int screenBoundY, int screenOffsetX, int screenOffsetY)
        {
            screenBounds = new int[2] {screenBoundX, screenBoundY};
            screenOffset = new int[2] {screenOffsetX, screenOffsetY};
            sireneDataReader.Init();
        }

        public override void Clean()
        {
            this.screenBounds = new int[2];
            allData = null;

            sireneDataReader.Clean();
        }

        //Obligatory step that all data has to go through either if gotten from a batch or
        // gotten one by one
        private SireneData RegisterData(SireneData sireneData)
        {
            //this.geoBounds.RegisterNewBounds(new float[] {sireneData.RawX, sireneData.RawY}); // We used to do this for data that was not aligned on on the coordiante system than our city line, but now its the case so we reutilise the geographic bounds of the city line
            this.timeBounds.RegisterNewBounds(sireneData.T);
            this.dataBounds.RegisterNewBounds(sireneData.EntityCount);

            return sireneData;
        }

        public override void RegisterAllData()
        {
            var data = GetNextData();
            while (data != null)
            {
                data = GetNextData();
            }
        }

        public override IData GetNextData()
        {
            sireneDataReader.GoToNextData();
            if (!sireneDataReader.EndOfStream)
            {
                return RegisterData((SireneData) sireneDataReader.GetData());
            }

            return null;
        }
        private IEnumerable<IData> GetCopyOfAllData()
        {
            List<SireneData> copiedData = new List<SireneData>();
            foreach (IData data in allData)
            {
                copiedData.Add((SireneData)data.Clone());
            }

            return copiedData;
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
            //using the cache
            if (allData != null)
            {
                return GetCopyOfAllData();
            }

            List<SireneData> notConvertedSireneData = new List<SireneData>();
            SireneData tmpData;

            //get raw data first
            while (!sireneDataReader.EndOfStream)
            {
                tmpData = (SireneData) GetNextData();
                if (!sireneDataReader.EndOfStream)
                {
                    notConvertedSireneData.Add(tmpData);
                }
            }

            this.geoBounds.StopRegisteringNewBounds();
            this.timeBounds.StopRegisteringNewBounds();
            this.dataBounds.StopRegisteringNewBounds();

            List<SireneData> sireneData = DataUtils.LinearizeTimedData(notConvertedSireneData, 1);

            //Transforming raw data by converting to screen 

            //OPTIONAL make scaling modulable given screen size

            float[,] _geoBounds = (float[,]) this.geoBounds.GetCurrentBounds();
            float[] _timeBounds = (float[]) this.timeBounds.GetCurrentBounds();
            float[] _dataBounds = (float[]) this.dataBounds.GetCurrentBounds();

            //prepare ratio for getting coords in bounds

            for (int i = 0; i < sireneData.Count; i++)
            {
                sireneData[i].SetX(ConvertX(sireneData[i].RawY, _geoBounds));
                sireneData[i].SetY(ConvertY(sireneData[i].RawX, _geoBounds));

                //Convert Real time to time [0->1] relative to min and max of it's times 
                float timeRange = _timeBounds[1] - _timeBounds[0];
                sireneData[i].SetT((sireneData[i].T - _timeBounds[0]) / timeRange);

                //Convert Real NOMBRE_LOG to NOMBRE_LOG [0->1] relative to min and max of it's times 
                float dataRange = _dataBounds[1] - _dataBounds[0];
                sireneData[i].EntityCount = ((sireneData[i].EntityCount - _dataBounds[0]) / dataRange);
            }

            this.allData = sireneData.OrderBy(r => r.T).ToList();
            
            return GetCopyOfAllData();
        }


        //return X : max,min; Y: max,min
        public override IDataReader GetDataReader()
        {
            return sireneDataReader;
        }

        public override IData[] GetDataBounds()
        {
            throw new System.NotImplementedException();
        }
    }
}