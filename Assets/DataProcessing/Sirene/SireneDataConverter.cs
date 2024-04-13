using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bounds;
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
        GeographicBatBounds geoBounds;
        TimeBounds timeBounds;
        private SireneBounds dataBounds;


        //tmps Big Vars
        private IEnumerable<IData> allData;

        public SireneDataConverter()
        {
            this.sireneDataReader =
                (SireneDataReader) FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.RIL);
            this.geoBounds =
                (GeographicBatBounds) BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.BATIMENT);
            this.timeBounds =
                (TimeBounds) BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.TIME);
            this.dataBounds =
                (SireneBounds) BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.RIL);
        }

        public override void Init(int screenBoundX, int screenBoundY)
        {
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
            this.geoBounds.RegisterNewBounds(new float[] {sireneData.RawX, sireneData.RawY});
            this.timeBounds.RegisterNewBounds(sireneData.T);
            this.dataBounds.RegisterNewBounds(sireneData.EntityCount);

            return sireneData;
        }

        public override IData GetNextData()
        {
            sireneDataReader.GoToNextData();
            if (!sireneDataReader.streamEnd)
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

        public override IEnumerable<IData> GetAllData()
        {
            //TODO : IMPORTANT : convert the data from the bad file format to the good one. 
            //The Sirene data format is in spherical projection when the other data are already projected


            //using the cache
            if (allData != null)
            {
                return GetCopyOfAllData();
            }

            List<SireneData> notConvertedSireneData = new List<SireneData>();
            SireneData tmpData;

            //get raw data first
            while (!sireneDataReader.streamEnd)
            {
                tmpData = (SireneData) GetNextData();
                if (!sireneDataReader.streamEnd)
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
            float dataBoundsXYRatio = (_geoBounds[0, 1] - _geoBounds[0, 0]) / ((_geoBounds[1, 1] - _geoBounds[1, 0]));

            for (int i = 0; i < sireneData.Count; i++)
            {
                //voluntary inversion
                float widthAsRatioOfOriginalTotalWidth =
                    ((_geoBounds[1, 0] - sireneData[i].RawY) / (_geoBounds[1, 1] - _geoBounds[1, 0]));

                sireneData[i].SetX(this.screenOffset[0] + widthAsRatioOfOriginalTotalWidth * screenBounds[0]);

                // Y is set as the % of total original height * the current width * the old % totalwidth by totalheight 
                float heightAsRatioOfOriginalTotalHeight =
                    ((sireneData[i].RawX - _geoBounds[0, 0]) / (_geoBounds[0, 1] - _geoBounds[0, 0]));
                float newMaxYHeight = dataBoundsXYRatio * screenBounds[1];
                sireneData[i].SetY(this.screenOffset[1] + screenBounds[1] -
                                heightAsRatioOfOriginalTotalHeight * screenBounds[1]);

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