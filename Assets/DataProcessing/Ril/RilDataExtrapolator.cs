using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Threading;
using DataProcessing.Generic;
using UnityEngine;
using Random = System.Random;


namespace DataProcessing.Ril
{
    public class RilDataExtrapolator : DataExtrapolator
    {
        private List<RilData> extrapolatedData;
        private List<RilData> dataToExtrapolate;
        private readonly Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();

        private readonly Tools.DebugLine debugLine = GameObject.Find("DebugLine").GetComponent<Tools.DebugLine>();
        private readonly Tools.DebugLine debugLineRed = GameObject.Find("DebugLineRed").GetComponent<Tools.DebugLine>();

        public RilDataExtrapolator() : base()
        {
        }

        protected override IEnumerable<IData> GetConcreteExtrapolation()
        {
            return extrapolatedData;
        }

        protected override void SetConcreteDataToExtrapolate(IEnumerable<IData> sourceData)
        {
            this.dataToExtrapolate = ((List<RilData>) sourceData);
        }

        protected override void ExecuteExtrapolation(object parameters)
        {
            bool isOnlyFutureExtrapolating = (bool) parameters;
            WaitForMutex();

            if (isOnlyFutureExtrapolating)
            {
                this.extrapolatedData = ExtrapolateFutureData(this.dataToExtrapolate);
            }

            this.extrapolatedData = this.extrapolatedData.OrderBy(x => x.T ).ToList();
            ReleaseMutex();

            logger.Log($"Extrapolation is Ready ! ");
        }

        private struct SpawnCoeff
        {
            public float NormalizedTimespanOfSlice;
            public float StartTime;
            public float[] Values;
        }

        private struct GrowthCoeff
        {
            public float NormalizedTimespanOfSlice;
            public float StartTime;
            public float[] Values;
        }

        private List<RilData> ExtrapolateFutureData(List<RilData> pastData)
        {
            const int nbSlices = 1000;

            //get growth Coefficient
            SpawnCoeff spawnCoeffs = CalculateSpawnCoefficient(pastData, 0.3f, nbSlices);
            GrowthCoeff growthCoeffs = CalculateGrowthCoefficient(pastData, 0.3f, nbSlices);

            return PredictFutureData(pastData, spawnCoeffs, growthCoeffs);
        }

        private static SpawnCoeff CalculateSpawnCoefficient(List<RilData> pastData, float percentageToSample,
            int nbSlices)
        {
            float[] spawnCoeffs = new float[nbSlices];

            int sliceIndex = 0, i = 0;
            int indexOfFirstData = pastData.Count - (int) Math.Round((float) pastData.Count * percentageToSample);
            float timeOfFirstData = pastData[indexOfFirstData].T;

            float normalizedTimeSlot = ((1f - timeOfFirstData) / nbSlices);
            int dataCountInSlot = 0;

            while (indexOfFirstData + i < pastData.Count && sliceIndex < nbSlices - 1)
            {
                if (pastData[indexOfFirstData + i].T <= timeOfFirstData + (normalizedTimeSlot * (sliceIndex + 1)))
                {
                    dataCountInSlot += 1;
                }
                else
                {
                    //end of slice, divide by number of iteration made
                    spawnCoeffs[sliceIndex] = dataCountInSlot ;
                    ++sliceIndex;
                    dataCountInSlot = 0;
                }

                i++;
            }

            spawnCoeffs[sliceIndex] = dataCountInSlot;

            return new SpawnCoeff
            {
                Values = spawnCoeffs,
                StartTime = timeOfFirstData,
                NormalizedTimespanOfSlice = normalizedTimeSlot
            };
        }

        private static GrowthCoeff CalculateGrowthCoefficient(List<RilData> pastData, float percentageToSample,
            int nbSlices)
        {
            float countBetweenSlices = 0;
            float[] growthCoeffs = new float[nbSlices];
            int indexOfFirstData = pastData.Count - (int) Math.Round((float) pastData.Count * percentageToSample);
            float timeOfFirstData = pastData[indexOfFirstData].T;
            float normalizedTimeSlot = ((1f - timeOfFirstData) / nbSlices);

            int i = 0;
            int sliceIndex = 0;

            while (indexOfFirstData + i < pastData.Count && sliceIndex < nbSlices)
            {
                if (pastData[indexOfFirstData + i].T <= timeOfFirstData + (normalizedTimeSlot * (sliceIndex + 1)))
                {
                    growthCoeffs[sliceIndex] += pastData[indexOfFirstData + i].NOMBRE_LOG;
                    countBetweenSlices++;
                }
                else
                {
                    growthCoeffs[sliceIndex] /= countBetweenSlices;
                    countBetweenSlices = 0;
                    ++sliceIndex;
                }

                i++;
            }

            return new GrowthCoeff
            {
                Values = growthCoeffs,
                StartTime = timeOfFirstData,
                NormalizedTimespanOfSlice = normalizedTimeSlot
            };
        }
        
        private static List<RilData> PredictFutureData(List<RilData> pastData,
            SpawnCoeff spawnCoeffs,
            GrowthCoeff growthCoeffs)
        {
            if (spawnCoeffs.Values.Length != growthCoeffs.Values.Length)
                throw new Exception("Coefficient arrays are not the same length");

            List<RilData> newData = new List<RilData>(pastData); 
            
            Random rnd = new Random();
            float lastT = 1f; // we start the time at the end of the normalized timeline
            
            for (int i = 0; i < spawnCoeffs.Values.Length; i++)
            {
                int nbOfDataToCreate = (int) Math.Ceiling(spawnCoeffs.Values[i]);
                float timespanBetweenTwoSpawn = spawnCoeffs.NormalizedTimespanOfSlice / nbOfDataToCreate;
                float batSize = growthCoeffs.Values[i] / nbOfDataToCreate;

                for (int dataToCreateIndex = 0; dataToCreateIndex < nbOfDataToCreate; dataToCreateIndex++)
                {
                    float futureT = lastT;
                    lastT += timespanBetweenTwoSpawn;

                    //noise function for Nb log
                    RilData pastDataToGetPosFrom = pastData[rnd.Next(0, pastData.Count - 1)];
                    float[] futurePos = {pastDataToGetPosFrom.X, pastDataToGetPosFrom.Y};

                    futurePos[0] += (rnd.Next(0, 50) - 25);
                    futurePos[1] += (rnd.Next(0, 50) - 25);
                    float ampl = Mathf.PerlinNoise( futurePos[0],futurePos[1]);
                    ampl = 1 + 10 * ampl * ampl * ampl; 
                    
                    FutureRilData rilData = new FutureRilData(futurePos[0], futurePos[1], futureT)
                    {
                        NOMBRE_LOG = batSize * ampl 
                    };

                    newData.Add(rilData);
                }
            }

            //Reassign new T with max being extrapolation
            float newMaxT = newData.Max(data => data.T);
            foreach (RilData rilData in newData)
            {
                rilData.SetT( rilData.T / newMaxT);
            }

            return newData;
        }
    }
}