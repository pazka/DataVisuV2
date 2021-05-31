using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.Ril
{
    public class RilDataExtrapolator : DataExtrapolator
    {
        private List<RilData> extrapolatedData;
        private List<RilData> dataToExtrapolate;
        private readonly Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();

        private readonly Tools.GenericLine debugLine = GameObject.Find("GenericLine").GetComponent<Tools.GenericLine>();

        public RilDataExtrapolator() : base()
        {
        }

        protected override IEnumerable<IData> GetConcreteExtrapolation()
        {
            return extrapolatedData;
        }

        protected override void SetConcreteDataToExtrapolate(IEnumerable<IData> sourceData)
        {
            this.dataToExtrapolate = (List<RilData>) sourceData;
        }

        protected override void ExecuteExtrapolation(object parameters)
        {
            bool isOnlyFutureExtrapolating = (bool) parameters;
            WaitForMutex();

            if (isOnlyFutureExtrapolating)
            {
                extrapolatedData = TryExtrapolateFutureData(dataToExtrapolate);
            }

            extrapolatedData = extrapolatedData.OrderBy(r => ((RilData) r).T).Reverse().ToList();
            ReleaseMutex();

            logger.Log($"Extrapolation is Ready ! ");
        }

        private List<RilData> TryExtrapolateFutureData(List<RilData> pastData)
        {
            List<RilData> dataToAddInTheFuture = new List<RilData>();

            // # 1 Temporal data : 

            // Get data point of rolling average 
            int rollingWidth = 501;
            float[] rollingAverages = new float[pastData.Count - rollingWidth];
            int midAverageWidth = (rollingWidth - 1) / 2;

            for (int i = 0; i < rollingAverages.Length; i++)
            {
                for (int j = -midAverageWidth; j < midAverageWidth; j++)
                {
                    // starting offset
                    var indexToAccumulate = midAverageWidth + i + j;

                    rollingAverages[i] += pastData[indexToAccumulate].NOMBRE_LOG;
                }

                rollingAverages[i] /= rollingWidth;
                if(i%10 == 0)
                    debugLine.AddPoint(((float)i / rollingAverages.Length) * 1920, rollingAverages[i] * 1080);
            }


            // Extract data from last decade
            // Get average of data data point y distance and same for X distance
            // make that into a linear equation 
            //Use this equation to extrapolate the time of appearance of data on next 100 years

            // #2 Heat map of XY

            // Generate a heat map of the position of of the data.
            //get rolling average on the x axis 
            // get rolling average on the y axis 
            // Compare those average together to gain a heat map on two dimensions

            // #3 Result
            // Using the heat map and the time , generate new values

            return pastData;
        }
    }
}