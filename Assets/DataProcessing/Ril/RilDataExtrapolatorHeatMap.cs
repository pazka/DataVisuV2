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


using  ProbabilityCell = System.Tuple<int,int,float>;
namespace DataProcessing.Ril
{
    struct RilExtrapolationParameters
    {
        public bool isOnlyFutureExtrapolating;
        public float extrapolationRate;
    }

    public class RilDataExtrapolatorHeatMap : DataExtrapolator
    {
        private static readonly Random rnd = new Random();
        private List<RilData> extrapolatedData;
        private List<RilData> dataToExtrapolate;
        public int GridPrecision = 100;
        public List<ProbabilityCell> CellSpawnProbability = new List<ProbabilityCell>(); // x,y,proba
        private readonly Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();

        private readonly VisualRestrictor.VisualRestrictor restrictor =
            GameObject.Find("VisualRestrictor").GetComponent<VisualRestrictor.VisualRestrictor>();

        private readonly Tools.DebugLine debugLine = GameObject.Find("DebugLine").GetComponent<Tools.DebugLine>();
        private readonly Tools.DebugLine debugLineRed = GameObject.Find("DebugLineRed").GetComponent<Tools.DebugLine>();


        public RilDataExtrapolatorHeatMap() : base()
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
            logger.Log("I wait for mutex");
            WaitForMutex();

            logger.Log("I got my mutex");
            if (isOnlyFutureExtrapolating)
            {
                this.extrapolatedData = ExtrapolateFutureData(this.dataToExtrapolate);
            }
            else
            {
                this.extrapolatedData = ExtrapolateData(this.dataToExtrapolate, extrapolationRate);
            }

            this.extrapolatedData = this.extrapolatedData.OrderBy(x => x.T).ToList();
            logger.Log("I release my mutex");
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

        private void InitCellProbabilityCalculation(float[][] bounds)
        {
            if (CellSpawnProbability.Count > 0)
            {
                return;
            }

            float cellWidth = (bounds[0][1] - bounds[0][0]) / GridPrecision;
            float cellHeight = (bounds[1][1] - bounds[1][0]) / GridPrecision;
            List<int2> availableCells = new List<int2>();

            for (int i = 0; i < GridPrecision; i++)
            {
                for (int j = 0; j < GridPrecision; j++)
                {
                    if (!IsSquareForbidden(new Vector3(i * cellWidth, j * cellHeight, 0), cellWidth, cellHeight))
                    {
                        availableCells.Add(new int2(i,j));
                    }
                }
            }

            for (int i = 0; i < availableCells.Count; i++)
            {
                CellSpawnProbability.Add(new ProbabilityCell(availableCells[i].x,availableCells[i].y,1f / availableCells.Count));
            }
        }


        private Vector2 GetNewPosition(float[][] bounds)
        {
            float cellWidth = (bounds[0][1] - bounds[0][0]) / GridPrecision;
            float cellHeight = (bounds[1][1] - bounds[1][0]) / GridPrecision;

            ProbabilityCell chosenCell = CellSpawnProbability.Last();
            int chosenCellIndex = 0;
            bool found = false;
            float currentPotential = 0f;
            float potentialToMatch = (float) rnd.NextDouble();

            // find cell
            for (chosenCellIndex = 0; chosenCellIndex < CellSpawnProbability.Count && !found; chosenCellIndex++)
            {
                var cellProba = CellSpawnProbability[chosenCellIndex];
                
                if (currentPotential < potentialToMatch)
                {
                    currentPotential += cellProba.Item3;
                }
                else
                {
                    found = true;
                    chosenCell = cellProba;
                }
            }
            
            // upgrade cell proba and lower others
            var incrValue = 0.0001f;
            var decrValue = incrValue / (CellSpawnProbability.Count-1);
            var newCellProbaList = new List<ProbabilityCell>();
            for (int i = 0; i < CellSpawnProbability.Count && !found; i++)
            {
                var cellProba = CellSpawnProbability[i];
                if (i == chosenCellIndex)
                {
                    newCellProbaList.Add(new ProbabilityCell(cellProba.Item1,cellProba.Item2,cellProba.Item3 - decrValue));
                }
                else
                {
                    newCellProbaList.Add(new ProbabilityCell(cellProba.Item1,cellProba.Item2,cellProba.Item3 + incrValue));
                }
            }

            CellSpawnProbability = newCellProbaList;
            
            
            var position = new Vector2(chosenCell.Item1 * cellWidth, chosenCell.Item2 * cellHeight);

            return position;
        }

        private List<RilData> ExtrapolateData(List<RilData> pastData, float extrapolationRate)
        {
            float[][] bounds = GetDataBounds(pastData);
            logger.Log("Initing cellProbability calculation");
            InitCellProbabilityCalculation(bounds);

            List<RilData> newData = new List<RilData>();

            newData.Add(pastData[0]);
            
            for (int i = 1; i < pastData.Count; i++)
            {
                RilData pastRilData = pastData[i];
                newData.Add(pastRilData);

                if (rnd.NextDouble() <= extrapolationRate)
                {
                    float extrapolatedT = (pastData[i - 1].T + pastRilData.T) / 2;

                    Vector2 newPosition = GetNewPosition(bounds);
                    FutureRilData extrapolatedData = new FutureRilData(newPosition[0], newPosition[1], extrapolatedT)
                    {
                        NOMBRE_LOG = pastRilData.NOMBRE_LOG
                    };

                    extrapolatedData.Randomize(bias: new float[] {0, 50});
                    newData.Add(extrapolatedData);
                }

                if (((i/pastData.Count) * 10) % 5 == 0)
                {
                    logger.Log((i/pastData.Count * 100) + "% extrapolated");
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