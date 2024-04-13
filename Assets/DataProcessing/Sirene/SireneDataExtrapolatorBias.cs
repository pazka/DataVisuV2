using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Permissions;
using System.Threading;
using Bounds;
using DataProcessing.Generic;
using Tools;
using UnityEngine;
using Random = System.Random;


namespace DataProcessing.Sirene
{
    struct SireneExtrapolationParameters
    {
        public bool isOnlyFutureExtrapolating;
        public float extrapolationRate;
    }
    
    public class SireneDataExtrapolatorBias : DataExtrapolator
    {
        private static readonly Random rnd = new Random();
        private List<SireneData> extrapolatedData;
        private List<SireneData> dataToExtrapolate;
        private readonly Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();

        private readonly Tools.DebugLine debugLine = GameObject.Find("DebugLine").GetComponent<Tools.DebugLine>();
        private readonly Tools.DebugLine debugLineRed = GameObject.Find("DebugLineRed").GetComponent<Tools.DebugLine>();


        public SireneDataExtrapolatorBias() : base()
        {
        }

        protected override IEnumerable<IData> GetConcreteExtrapolation()
        {
            return extrapolatedData;
        }

        protected override void SetConcreteDataToExtrapolate(IEnumerable<IData> sourceData)
        {
            this.dataToExtrapolate = ((List<SireneData>) sourceData);
        }

        protected override void ExecuteExtrapolation(object parameters)
        {
            if (dataToExtrapolate.Count == 0)
            {
                return;
            }

            SireneExtrapolationParameters extrapolationParameters = (SireneExtrapolationParameters) parameters;
            bool isOnlyFutureExtrapolating = extrapolationParameters.isOnlyFutureExtrapolating;
            float extrapolationRate = extrapolationParameters.extrapolationRate;
            WaitForMutex();

            if (isOnlyFutureExtrapolating)
            {
                this.extrapolatedData = ExtrapolateFutureData(this.dataToExtrapolate);
            }
            else
            {
                this.extrapolatedData = ExtrapolateData(this.dataToExtrapolate, extrapolationRate);
            }

            this.extrapolatedData = this.extrapolatedData.OrderBy(x => x.T).ToList();
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

        private List<SireneData> ExtrapolateFutureData(List<SireneData> pastData)
        {
            const int nbSlices = 1000;

            //get growth Coefficient
            SpawnCoeff spawnCoeffs = CalculateSpawnCoefficient(pastData, 0.3f, nbSlices);
            GrowthCoeff growthCoeffs = CalculateGrowthCoefficient(pastData, 0.3f, nbSlices);

            return PredictFutureData(pastData, spawnCoeffs, growthCoeffs);
        }

        private static SpawnCoeff CalculateSpawnCoefficient(List<SireneData> pastData, float percentageToSample,
            int nbSlices)
        {
            List<SireneData> selectedDataToExtrapolate = pastData.Where(data => data.IsOnePerson).ToList();
            float[] spawnCoeffs = new float[nbSlices];

            int sliceIndex = 0, i = 0;
            int indexOfFirstData = selectedDataToExtrapolate.Count - (int) Math.Round((float) selectedDataToExtrapolate.Count * percentageToSample);
            float timeOfFirstData = selectedDataToExtrapolate[indexOfFirstData].T;

            float normalizedTimeSlot = ((1f - timeOfFirstData) / nbSlices);
            int dataCountInSlot = 0;

            while (indexOfFirstData + i < selectedDataToExtrapolate.Count && sliceIndex < nbSlices - 1)
            {
                if (selectedDataToExtrapolate[indexOfFirstData + i].T <= timeOfFirstData + (normalizedTimeSlot * (sliceIndex + 1)))
                {
                    dataCountInSlot += 1;
                }
                else
                {
                    //end of slice, divide by number of iteration made
                    spawnCoeffs[sliceIndex] = dataCountInSlot;
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

        private static GrowthCoeff CalculateGrowthCoefficient(List<SireneData> pastData, float percentageToSample,
            int nbSlices)
        {
            float countBetweenSlices = 0;
            float[] growthCoeffs = new float[nbSlices];
            List<SireneData> selectedDataToExtrapolate = pastData.Where(data => data.IsOnePerson).ToList();
            
            int indexOfFirstData = selectedDataToExtrapolate.Count - (int) Math.Round((float) selectedDataToExtrapolate.Count * percentageToSample);
            float timeOfFirstData = selectedDataToExtrapolate[indexOfFirstData].T;
            float normalizedTimeSlot = ((1f - timeOfFirstData) / nbSlices);

            int i = 0;
            int sliceIndex = 0;

            while (indexOfFirstData + i < selectedDataToExtrapolate.Count && sliceIndex < nbSlices)
            {
                if (selectedDataToExtrapolate[indexOfFirstData + i].T <= timeOfFirstData + (normalizedTimeSlot * (sliceIndex + 1)))
                {
                    growthCoeffs[sliceIndex] += selectedDataToExtrapolate[indexOfFirstData + i].EntityCount;
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

        private static List<SireneData> PredictFutureData(List<SireneData> pastData,
            SpawnCoeff spawnCoeffs,
            GrowthCoeff growthCoeffs)
        {
            List<SireneData> selectedDataToExtrapolate = pastData.Where(data => data.IsOnePerson).ToList();
            
            if (spawnCoeffs.Values.Length != growthCoeffs.Values.Length)
                throw new Exception("Coefficient arrays are not the same length");

            List<SireneData> newData = new List<SireneData>(pastData);

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
                    SireneData randomPastData = pastData[rnd.Next(0, pastData.Count - 1)];
                    float[] futurePos = {randomPastData.X, randomPastData.Y};


                    FutureSireneData sireneData = new FutureSireneData(futurePos[0], futurePos[1], futureT)
                    {
                        EntityCount = batSize
                    };
                    sireneData.Randomize();

                    newData.Add(sireneData);
                }
            }

            //Reassign new T with max being extrapolation
            float newMaxT = newData.Max(data => data.T);
            foreach (SireneData sireneData in newData)
            {
                sireneData.SetT(sireneData.T / newMaxT);
            }

            return newData;
        }

        private static List<SireneData> ExtrapolateData(List<SireneData> pastData, float extrapolationRate)
        {
            List<SireneData> newData = new List<SireneData>();
            List<SireneData> selectedDataToExtrapolate = pastData.Where(data => data.IsOnePerson).ToList();
            
            float xMidPoint = (selectedDataToExtrapolate.Max(d => d.X) + selectedDataToExtrapolate.Min(d => d.X)) / 2;
            float xfirstQuartPoint = (selectedDataToExtrapolate.Max(d => d.X) + selectedDataToExtrapolate.Min(d => d.X)) / 4;

            newData.Add(selectedDataToExtrapolate[0]);

            for (int i = 1; i < selectedDataToExtrapolate.Count; i++)
            {
                SireneData pastSireneData = selectedDataToExtrapolate[i];
                newData.Add(pastSireneData);

                float alteredExtrapolationRate = pastSireneData.T < 0.75 ? extrapolationRate : .66f;

                if (rnd.NextDouble() > alteredExtrapolationRate ) continue; // don't create a new future data
                
                // create a new future data
                float extrapolatedT = (selectedDataToExtrapolate[i - 1].T + pastSireneData.T) / 2;

                FutureSireneData oneExtrapolatedData = new FutureSireneData(pastSireneData.X, pastSireneData.Y, extrapolatedT)
                {
                    EntityCount = pastSireneData.EntityCount
                };

                if (oneExtrapolatedData.X < xMidPoint)
                {
                    if (oneExtrapolatedData.X < xfirstQuartPoint)
                    {
                        oneExtrapolatedData.Randomize(bias: new float[] {-100, 50});
                    }
                    else
                    {
                        oneExtrapolatedData.Randomize(bias: new float[] {-50, 100});
                    }
                }
                else
                {
                    oneExtrapolatedData.Randomize(bias: new float[] {50, 50});
                }

                newData.Add(oneExtrapolatedData);
            }

            //Reassign new T with max being extrapolation
            
            // I don't think we need this in fact
            // float newMaxT = newData.Max(data => data.T);
            // foreach (SireneData sireneData in newData)
            // {
            //     sireneData.SetT(sireneData.T / newMaxT);
            // }
            //
             return newData;
        }
    }
}