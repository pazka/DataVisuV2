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
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;


namespace DataProcessing.Ril
{
    struct RilExtrapolationParameters
    {
        public bool isOnlyFutureExtrapolating;
        public float extrapolationRate;
    }

    public class RilDataExtrapolator : DataExtrapolator
    {
        private static readonly Random rnd = new Random();
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
            if (dataToExtrapolate.Count == 0)
            {
                return;
            }

            RilExtrapolationParameters extrapolationParameters = (RilExtrapolationParameters) parameters;
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

        private static float[][] GetDataBounds(List<RilData> allData)
        {
            //could get bounds from the conversion right away

            float[] boundX = new[]
                {allData.Select(x => x.X).Max(), -allData.Select(x => x.X).Min()}; // volontary inversion
            float[] boundY = new[] {allData.Select(x => x.Y).Min(), allData.Select(x => x.Y).Max()};

            return new[] {boundX, boundY};
        }

        private static float[,] GetSimpleProbabilityMap(List<RilData> allData, float[][] bounds, int precision = 10)
        {
            float[,] probabilityMap = new float[precision, precision];
            float maxCellsValue = 0f;


            float cellWidth = (bounds[0][1] - bounds[0][0]) / precision;
            float cellHeight = (bounds[1][1] - bounds[1][0]) / precision;

            foreach (RilData data in allData)
            {
                int i = 0, j = 0;
                for (i = 0; (i + 1) * cellWidth < -data.X && i < precision; i++)
                {
                }

                for (j = 0; (j + 1) * cellHeight < data.Y && j < precision; j++)
                {
                }

                if (maxCellsValue < probabilityMap[i, j])
                {
                    maxCellsValue = probabilityMap[i, j];
                }
            }

            for (int i = 0; i < precision; i++)
            {
                for (int j = 0; j < precision; j++)
                {
                    probabilityMap[i, j] = probabilityMap[i, j] / allData.Count;
                }
            }


            return probabilityMap;
        }

        private static float[,] GetAlteredProbabilityMap(float[,] probabilityMap, float[][] bounds, int precision = 10)
        {
            float[,] alteredProbabilityMap = probabilityMap;

            for (int i = 0; i < precision; i++)
            {
                for (int j = 0; j < precision; j++)
                {
                    alteredProbabilityMap[i, j] = 1 - alteredProbabilityMap[i, j];
                }
            }

            return alteredProbabilityMap;
        }

        private static Vector2 GetNewPosition(float[,] probabilityMap, float[][] bounds, int precision = 10)
        {
            float cellWidth = (bounds[0][1] - bounds[0][0]) / precision;
            float cellHeight = (bounds[1][1] - bounds[1][0]) / precision;

            Vector2 position = new Vector2(0.0f, 0.0f);
            float currentPotential = 0f;
            float potentialToMatch = (float) rnd.NextDouble()*precision*precision;

            int i = 0;
            int j = 0;
            for (i = 0; i < precision && currentPotential < potentialToMatch; i++)
            {
                for (j = 0; j < precision && currentPotential < potentialToMatch; j++)
                {
                    currentPotential += probabilityMap[i, j];
                }
            }

            position[0] = -(cellWidth * i);
            position[1] = cellHeight * j;

            return position;
        }

        private static List<RilData> ExtrapolateData(List<RilData> pastData, float extrapolationRate)
        {
            float[][] bounds = GetDataBounds(pastData);
            float[,] simpleProbabilityMap = GetSimpleProbabilityMap(pastData, bounds, 10);
            float[,] probabilityMap = GetAlteredProbabilityMap(simpleProbabilityMap, bounds, 10);

            List<RilData> newData = new List<RilData>();

            newData.Add(pastData[0]);

            for (int i = 1; i < pastData.Count; i++)
            {
                RilData pastRilData = pastData[i];
                newData.Add(pastRilData);

                if (rnd.NextDouble() <= extrapolationRate)
                {
                    float extrapolatedT = (pastData[i - 1].T + pastRilData.T) / 2;

                    Vector2 newPosition = GetNewPosition(probabilityMap, bounds, 10);
                    FutureRilData extrapolatedData = new FutureRilData(newPosition[0], newPosition[1], extrapolatedT)
                    {
                        NOMBRE_LOG = pastRilData.NOMBRE_LOG
                    };

                    extrapolatedData.Randomize(bias: new float[] {0, 50});
                    newData.Add(extrapolatedData);
                }
            }

            //Reassign new T with max being extrapolation
            float newMaxT = newData.Max(data => data.T);
            foreach (RilData rilData in newData)
            {
                rilData.SetT(rilData.T / newMaxT);
            }

            return newData;
        }

        private static List<RilData> PredictFutureData(List<RilData> pastData,
            SpawnCoeff spawnCoeffs,
            GrowthCoeff growthCoeffs)
        {
            if (spawnCoeffs.Values.Length != growthCoeffs.Values.Length)
                throw new Exception("Coefficient arrays are not the same length");

            float[][] bounds = GetDataBounds(pastData);
            float[,] simpleProbabilityMap = GetSimpleProbabilityMap(pastData, bounds, 10);
            float[,] probabilityMap = GetAlteredProbabilityMap(simpleProbabilityMap, bounds, 10);

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
                    RilData randomPastData = pastData[rnd.Next(0, pastData.Count - 1)];
                    //float[] futurePos = {randomPastData.X, randomPastData.Y};

                    Vector2 newPosition = GetNewPosition(probabilityMap, bounds, 10);

                    FutureRilData rilData = new FutureRilData(newPosition[0], newPosition[1], futureT)
                    {
                        NOMBRE_LOG = batSize
                    };
                    rilData.Randomize();

                    newData.Add(rilData);
                }
            }

            //Reassign new T with max being extrapolation
            float newMaxT = newData.Max(data => data.T);
            foreach (RilData rilData in newData)
            {
                rilData.SetT(rilData.T / newMaxT);
            }

            return newData;
        }
    }
}