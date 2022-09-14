﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bounds;
using DataProcessing.Generic;
using UnityEngine;
using Tools;

namespace DataProcessing.Ril
{
    public class RilDataConverter : DataConverter
    {
        private RilDataReader rilDataReader;

        int[] screenBounds = new int[2];
        int[] screenOffset = new int[2];
        GeographicBatBounds geoBounds;
        TimeBounds timeBounds;
        private RilBounds dataBounds;


        //tmps Big Vars
        private IEnumerable<IData> allData;

        public RilDataConverter()
        {
            this.rilDataReader =
                (RilDataReader) FactoryDataReader.GetInstance(FactoryDataReader.AvailableDataReaderTypes.RIL);
            this.geoBounds =
                (GeographicBatBounds) BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.BATIMENT);
            this.timeBounds =
                (TimeBounds) BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.TIME);
            this.dataBounds =
                (RilBounds) BoundsFactory.GetInstance(BoundsFactory.AvailableBoundsTypes.RIL);
        }

        public override void Init(int screenBoundX, int screenBoundY)
        {
            screenBounds = new int[2] {screenBoundX, screenBoundY};
            rilDataReader.Init();
        }

        public void Init(int screenBoundX, int screenBoundY, int screenOffsetX, int screenOffsetY)
        {
            screenBounds = new int[2] {screenBoundX, screenBoundY};
            screenOffset = new int[2] {screenOffsetX, screenOffsetY};
            rilDataReader.Init();
        }

        public override void Clean()
        {
            this.screenBounds = new int[2];
            allData = null;

            rilDataReader.Clean();
        }

        //Obligatory step that all data has to go through either if gotten from a batch or
        // gotten one by one
        private RilData RegisterData(RilData rilData)
        {
            this.geoBounds.RegisterNewBounds(new float[] {rilData.RawX, rilData.RawY});
            this.timeBounds.RegisterNewBounds(rilData.T);
            this.dataBounds.RegisterNewBounds(rilData.NOMBRE_LOG);

            return rilData;
        }

        public override IData GetNextData()
        {
            rilDataReader.GoToNextData();
            if (!rilDataReader.streamEnd)
            {
                return RegisterData((RilData) rilDataReader.GetData());
            }

            return null;
        }

        public override IEnumerable<IData> GetAllData()
        {
            //TODO : IMPORTANT : convert the data from the bad file format to the good one. 
            //The Ril data format is in spherical projection when the other data are already projected

            //using the cache
            if (allData != null)
            {
                return allData;
            }

            List<RilData> notConvertedRilData = new List<RilData>();
            RilData tmpData;

            //get raw data first
            while (!rilDataReader.streamEnd)
            {
                tmpData = (RilData) GetNextData();
                if (!rilDataReader.streamEnd)
                {
                    notConvertedRilData.Add(tmpData);
                }
            }

            this.geoBounds.StopRegisteringNewBounds();
            this.timeBounds.StopRegisteringNewBounds();
            this.dataBounds.StopRegisteringNewBounds();

            List<RilData> rilData = DataUtils.LinearizeTimedData(notConvertedRilData, 1);

            //Transforming raw data by converting to screen 

            //OPTIONAL make scaling modulable given screen size

            float[,] _geoBounds = (float[,]) this.geoBounds.GetCurrentBounds();
            float[] _timeBounds = (float[]) this.timeBounds.GetCurrentBounds();
            float[] _dataBounds = (float[]) this.dataBounds.GetCurrentBounds();

            //prepare ratio for getting coords in bounds
            float dataBoundsXYRatio = (_geoBounds[0, 1] - _geoBounds[0, 0]) / ((_geoBounds[1, 1] - _geoBounds[1, 0]));

            for (int i = 0; i < rilData.Count; i++)
            {
                //voluntary inversion
                float widthAsRatioOfOriginalTotalWidth =
                    ((_geoBounds[1, 0] - rilData[i].RawY) / (_geoBounds[1, 1] - _geoBounds[1, 0]));
                rilData[i].SetX(this.screenOffset[0] + widthAsRatioOfOriginalTotalWidth * screenBounds[0]);

                // Y is set as the % of total original height * the current width * the old % totalwidth by totalheight 
                float heightAsRatioOfOriginalTotalHeight =
                    ((rilData[i].RawX - _geoBounds[0, 0]) / (_geoBounds[0, 1] - _geoBounds[0, 0]));
                float newMaxYHeight = dataBoundsXYRatio * screenBounds[1];
                rilData[i].SetY(this.screenOffset[1] + screenBounds[1] -
                                heightAsRatioOfOriginalTotalHeight * screenBounds[1]);

                float[] reFlattenedPosition = FlattenCurve.GetFlattenedTwoDimensionPoint(
                    new float[] {rilData[i].X, rilData[i].Y},
                    new float[] {0.5f, 0.5f, -0.2f}
                );

                rilData[i].SetX(reFlattenedPosition[0]);
                rilData[i].SetY(reFlattenedPosition[1]);

                //Convert Real time to time [0->1] relative to min and max of it's times 
                float timeRange = _timeBounds[1] - _timeBounds[0];
                rilData[i].SetT((rilData[i].T - _timeBounds[0]) / timeRange);

                //Convert Real NOMBRE_LOG to NOMBRE_LOG [0->1] relative to min and max of it's times 
                float dataRange = _dataBounds[1] - _dataBounds[0];
                rilData[i].NOMBRE_LOG = ((rilData[i].NOMBRE_LOG - _dataBounds[0]) / dataRange);
            }

            this.allData = rilData.OrderBy(r => r.T).ToList();
            return allData;
        }


        //return X : max,min; Y: max,min
        public override IDataReader GetDataReader()
        {
            return rilDataReader;
        }

        public override IData[] GetDataBounds()
        {
            throw new System.NotImplementedException();
        }
    }
}