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
        
        private class HeatMap
        {
            public float[][] Map;
            public float[] HeaderX;
            public float[] HeaderY;
            private Random random = new Random();

            public float[] GetWeightedPosition()
            {
                float acc = 0f;
                double rnd = random.NextDouble();
                
                for (int x = 0; x < HeaderX.Length; x++)
                {
                    for (int y = 0; y < HeaderY.Length; y++)
                    {
                        acc += Map[x][y];
                        if (rnd < acc)
                        {
                            return new float[] {x, y};
                        }
                    }
                }
                
                return new float[]{0,0};
            }
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

            //HeatMap heatMap = CalculateHeatMap(pastData, 0.2f, 1921, 1081);
            HeatMap heatMap = new HeatMap();
            return PredictFutureData(pastData, spawnCoeffs, growthCoeffs, heatMap);
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

        private static HeatMap CalculateHeatMap(List<RilData> pastData, float percentageToSample, int limitX,
            int limitY)
        {
            float[] mapXs = new float[limitX];
            float[] mapYs = new float[limitY];

            int indexOfFirstData = pastData.Count - (int) Math.Round((float) pastData.Count * percentageToSample);

            for (int i = 0; indexOfFirstData + i < pastData.Count; i++)
            {
                //round the pos to fill heat map cells
                RilData rilData = pastData[indexOfFirstData + i];
                int mapXPosIndex = (int) Math.Round(rilData.X);
                int mapYPosIndex = (int) Math.Round(rilData.Y);

                //fill the meta heatmap information ( Acc X and Acc Y)
                mapXs[mapXPosIndex] += rilData.NOMBRE_LOG;
                mapYs[mapYPosIndex] += rilData.NOMBRE_LOG;
            }

            HeatMap valueHeatMap = ConvertArraysToHeatMap(mapXs, mapYs);

            return valueHeatMap;
        }


        private static HeatMap ConvertArraysToHeatMap(float[] mapXs, float[] mapYs)
        {
            int limitX = mapXs.Length;
            int limitY = mapYs.Length;

            float[][] map = new float[limitX][];
            float[][] resultMap = new float[limitX][];
            for (int i = 0; i < limitX; i++)
            {
                map[i] = new float[limitY];
                resultMap[i] = new float[limitY];
            }

            float DiffusionCoeffEquation(float x)
            {
                return Math.Max((1 - (x * x) * 100), 0);
            }

            float DiffusionCoeffEquation1(float x)
            {
                return x == 0 ? 1 : 0;
            }

            float[] normalizedDiffusedMapX = new float[limitX];
            float[] normalizedDiffusedMapY = new float[limitY];

            for (int x = 0; x < limitX; x++)
            {
                for (int x1 = 0; x1 < limitX; x1++)
                {
                    float test = DiffusionCoeffEquation((float) (x - x1) / (float) limitX) * mapXs[x1];
                    normalizedDiffusedMapX[x] += test;
                }
            }

            for (int y = 0; y < limitY; y++)
            {
                for (int y1 = 0; y1 < limitY; y1++)
                {
                    float test = DiffusionCoeffEquation((float) (y - y1) / (float) limitY) * mapYs[y1];
                    normalizedDiffusedMapY[y] += test;
                }
            }

            //Convert to probability arrays
            float acc = normalizedDiffusedMapX.Sum();
            float[] normalizedDiffusedProbabilityMapX = normalizedDiffusedMapX.Select(x => x / acc).ToArray();

            acc = normalizedDiffusedMapY.Sum();
            float[] normalizedDiffusedProbabilityMapY = normalizedDiffusedMapY.Select(x => x / acc).ToArray();

            float mapProbaTotal = 0f;

            //combine the two arrays into the heatmap
            for (int x = 0; x < limitX; x++)
            {
                for (int y = 0; y < limitY; y++)
                {
                    resultMap[x][y] = (float) Math.Sqrt(normalizedDiffusedProbabilityMapX[x] *normalizedDiffusedProbabilityMapY[y]);
                    mapProbaTotal += resultMap[x][y];
                }
            }
            
            
            //normalize them just in case
            for (int x = 0; x < limitX; x++)
            {
                for (int y = 0; y < limitY; y++)
                {
                    resultMap[x][y] = resultMap[x][y] / mapProbaTotal;
                }
            }

            return new HeatMap
            {
                HeaderX = normalizedDiffusedProbabilityMapX,
                HeaderY = normalizedDiffusedProbabilityMapY,
                Map = resultMap
            };
        }

        private static List<RilData> PredictFutureData(List<RilData> pastData,
            SpawnCoeff spawnCoeffs,
            GrowthCoeff growthCoeffs,
            HeatMap heatMap)
        {
            if (spawnCoeffs.Values.Length != growthCoeffs.Values.Length)
                throw new Exception("Coefficient arrays are not the same length");

            List<RilData> newData = new List<RilData>(pastData); 
            
            Random rnd = new Random();
            for (int i = 0; i < spawnCoeffs.Values.Length; i++)
            {
                int nbOfDataToCreate = (int) Math.Ceiling(spawnCoeffs.Values[i]);
                float timespanBetweenTwoSpawn = spawnCoeffs.NormalizedTimespanOfSlice / nbOfDataToCreate;
                float batSize = growthCoeffs.Values[i] / nbOfDataToCreate;
                float lastT = 1f; // we start the time at the end of the normalized timeline

                for (int dataToCreateIndex = 0; dataToCreateIndex < nbOfDataToCreate; dataToCreateIndex++)
                {
                    float futureT = lastT + timespanBetweenTwoSpawn;

                    //generate new data with rejection sampling for position and noise function for Nb log
                    //float[] futurePos = heatMap.GetWeightedPosition();
                    RilData pastDatatoGetPosFrom = pastData[rnd.Next(0, pastData.Count - 1)];
                    float[] futurePos = new[] {pastDatatoGetPosFrom.X, pastDatatoGetPosFrom.Y};

                    futurePos[0] += (rnd.Next(0, 50) - 25);
                    futurePos[1] += (rnd.Next(0, 50) - 25);
                    RilData rilData = new RilData("future", futurePos[0], futurePos[1], futureT)
                    {
                        //TODO : Add Noise here for organic results
                        NOMBRE_LOG = batSize
                    };

                    newData.Add(rilData);
                }
            }

            //Reassign new T with max being extrapolation
            float newMaxT = newData.Max(data => data.T);
            foreach (RilData rilData in pastData)
            {
                rilData.SetT( rilData.T / newMaxT);
            }

            return newData;
        }
    }
}