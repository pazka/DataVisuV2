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
        public static int GridPrecision = 100;
        private readonly Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();

        private readonly VisualRestrictor.VisualRestrictor restrictor =
            GameObject.Find("VisualRestrictor").GetComponent<VisualRestrictor.VisualRestrictor>();

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

        private static float[,] GetSimpleProbabilityMap(List<RilData> allData, float[][] bounds)
        {
            float[,] probabilityMap = new float[GridPrecision, GridPrecision];
            float maxCellsValue = 0f;


            float cellWidth = (bounds[0][1] - bounds[0][0]) / GridPrecision;
            float cellHeight = (bounds[1][1] - bounds[1][0]) / GridPrecision;

            foreach (RilData data in allData)
            {
                int i = 0, j = 0;
                while ((i + 1) * cellWidth < -data.X && i < GridPrecision - 1)
                {
                    i++;
                }

                //TODO warning bug here
                while ((j + 1) * cellHeight < data.Y && j < GridPrecision - 1)
                {
                    j++;
                }

                probabilityMap[i, j] += data.NOMBRE_LOG;
                maxCellsValue += data.NOMBRE_LOG;
            }

            for (int i = 0; i < GridPrecision; i++)
            {
                for (int j = 0; j < GridPrecision; j++)
                {
                    probabilityMap[i, j] = probabilityMap[i, j] / maxCellsValue;
                }
            }

            //debug

            var debugProb = 0.0f;
            
            for (int i = 0; i < GridPrecision; i++)
            {
                for (int j = 0; j < GridPrecision; j++)
                {
                    debugProb += probabilityMap[i, j];
                    if (probabilityMap[i, j] > 0)
                    {
                        Console.WriteLine(i + " " + j);
                        //Debug.DrawLine(new Vector3(i*cellWidth,j*cellHeight),new Vector3((i+1)*cellWidth,(j+1)*cellHeight),Color.green);
                    }
                }
            }

            return probabilityMap;
        }

        private bool IsSquareForbidden(Vector3 position, float width, float height)
        {
            bool isABitInPoly = false;
            Vector3[] pointsToTest = new Vector3[4];
            pointsToTest[0] = new Vector3(position[0], position[1]);
            pointsToTest[1] = new Vector3(position[0] + width, position[1]);
            pointsToTest[2] = new Vector3(position[0], position[1] + height);
            pointsToTest[3] = new Vector3(position[0] + width, position[1] + height);

            foreach (Vector3 sqrPoint in pointsToTest)
            {
                isABitInPoly = isABitInPoly || restrictor.IsPointInPoly(sqrPoint, restrictor.restrictionLine);
            }

            return !isABitInPoly;
        }

        private float[,] GetAlteredProbabilityMap(float[,] probabilityMap, float[][] bounds)
        {
            float[,] alteredProbabilityMap = probabilityMap;
            float cellWidth = (bounds[0][1] - bounds[0][0]) / GridPrecision;
            float cellHeight = (bounds[1][1] - bounds[1][0]) / GridPrecision;
            float totalProba = 0f;

            //invert and restrict
            for (int i = 0; i < GridPrecision; i++)
            {
                for (int j = 0; j < GridPrecision; j++)
                {

                    //restrict
                    if (IsSquareForbidden(new Vector3(i * cellWidth, j * cellHeight, 0), cellWidth, cellHeight))
                    {
                        alteredProbabilityMap[i, j] = 0;
                    }
                    else
                    {
                        alteredProbabilityMap[i, j] = 1 - alteredProbabilityMap[i, j];
                        totalProba += alteredProbabilityMap[i, j];
                    }
                }
            }

            //normalize
            for (int i = 0; i < GridPrecision; i++)
            {
                for (int j = 0; j < GridPrecision; j++)
                {
                    alteredProbabilityMap[i, j] = alteredProbabilityMap[i, j] / totalProba;
                }
            }
            
            //debug

            var debugProb = 0.0f;
            for (int i = 0; i < GridPrecision; i++)
            {
                for (int j = 0; j < GridPrecision; j++)
                {
                    debugProb += alteredProbabilityMap[i, j];
                }
            }

            return alteredProbabilityMap;
        }

        private static Vector2 GetNewPosition(float[,] probabilityMap, float[][] bounds)
        {
            float cellWidth = (bounds[0][1] - bounds[0][0]) / GridPrecision;
            float cellHeight = (bounds[1][1] - bounds[1][0]) / GridPrecision;

            Vector2 position = new Vector2(0.0f, 0.0f);
            float currentPotential = 0f;
            float potentialToMatch = (float) rnd.NextDouble();

            int i = 0;
            int j = 0;
            while (i < GridPrecision && currentPotential < potentialToMatch)
            {
                while (j < GridPrecision && currentPotential < potentialToMatch)
                {
                    currentPotential += probabilityMap[i, j];
                    j++;
                }

                j = 0;
                i++;
            }

            position[0] = -(cellWidth * i);
            position[1] = cellHeight * j;

            return position;
        }

        private List<RilData> ExtrapolateData(List<RilData> pastData, float extrapolationRate)
        {
            float[][] bounds = GetDataBounds(pastData);
            float[,] simpleProbabilityMap = GetSimpleProbabilityMap(pastData, bounds);
            float[,] probabilityMap = GetAlteredProbabilityMap(simpleProbabilityMap, bounds);

            List<RilData> newData = new List<RilData>();

            newData.Add(pastData[0]);

            for (int i = 1; i < pastData.Count; i++)
            {
                RilData pastRilData = pastData[i];
                newData.Add(pastRilData);

                if (rnd.NextDouble() <= extrapolationRate)
                {
                    float extrapolatedT = (pastData[i - 1].T + pastRilData.T) / 2;

                    Vector2 newPosition = GetNewPosition(probabilityMap, bounds);
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

        private List<RilData> PredictFutureData(List<RilData> pastData,
            SpawnCoeff spawnCoeffs,
            GrowthCoeff growthCoeffs)
        {
            if (spawnCoeffs.Values.Length != growthCoeffs.Values.Length)
                throw new Exception("Coefficient arrays are not the same length");
            //
            // float[][] bounds = GetDataBounds(pastData);
            // float[,] simpleProbabilityMap = GetSimpleProbabilityMap(pastData, bounds);
            // float[,] probabilityMap = GetAlteredProbabilityMap(simpleProbabilityMap, bounds);

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
                    float[] newPosition = {randomPastData.X, randomPastData.Y};

                    //Vector2 newPosition = GetNewPosition(probabilityMap, bounds);

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