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
                extrapolatedData = ExtrapolateFutureData(dataToExtrapolate);
            }

            ReleaseMutex();

            logger.Log($"Extrapolation is Ready ! ");
        }

        struct HeatMap
        {
            public float[][] map;
            public float[] headerX;
            public float[] headerY;
        }
        private List<RilData> ExtrapolateFutureData(List<RilData> pastData)
        {
            const int nbSlices = 1000;
            
            //get growth Coefficient
            float[] spawnCoeffs = CalculateSpawnCoefficient(pastData,nbSlices);
            float[] growthCoeffs = CalculateGrowthCoefficient(pastData,nbSlices);

            HeatMap heatMap = CalculateHeatMap(pastData,(float)0.2,1921,1081);
            
            return PredictFutureData(pastData,spawnCoeffs,growthCoeffs,heatMap);
        }

        private float[] CalculateSpawnCoefficient(List<RilData> pastData, int nbSlices)
        {
            float[] spawnCoeffs = new float[nbSlices];

            int i = 0;
            int sliceIndex = 0;

            while (i < pastData.Count && sliceIndex < nbSlices)
            {
                if (pastData[i].T > (1 / nbSlices) * sliceIndex + 1)
                {
                    spawnCoeffs[sliceIndex] /= ((float)1 / nbSlices);
                    ++sliceIndex;
                }

                spawnCoeffs[sliceIndex] += 1;
                i++;
            }
            
            return spawnCoeffs;
        }

        private float[] CalculateGrowthCoefficient(List<RilData> pastData,int nbSlices)
        {
            float countBetweenSlices = 0;
            float[] growthCoeffs = new float[nbSlices];

            int i = 0;
            int sliceIndex = 0;

            while (i < pastData.Count && sliceIndex < nbSlices)
            {
                if (pastData[i].T > (1 / nbSlices) * sliceIndex + 1)
                {
                    growthCoeffs[sliceIndex] /= countBetweenSlices;
                    countBetweenSlices = 0;
                    ++sliceIndex;
                }

                growthCoeffs[sliceIndex] += 1;
                i++;
            }
            
            return growthCoeffs;
        }

        private HeatMap ConvertArraysToHeatMap(float[] mapXs, float[] mapYs)
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

            float DiffusionCoeffEquation(float x) { return Math.Max((1 - (x * x) * 1000), 0);}
            float DiffusionCoeffEquation1(float x) { return x == 0 ? 1 :0;}
            float[] normalizedDiffusedMapX = new float[limitX];
            float[] normalizedDiffusedMapY = new float[limitY];
            
            for (int x = 0; x < limitX; x++)
            {
                for (int x1 = 0; x1 < limitX; x1++)
                {
                    float test = DiffusionCoeffEquation((float)(x - x1)/(float)limitX) * mapXs[x1];
                    normalizedDiffusedMapX[x] += test;
                }
            }
            
            for (int y = 0; y < limitY; y++)
            {
                for (int y1 = 0; y1 < limitY; y1++)
                {
                    float test = DiffusionCoeffEquation((float)(y - y1)/(float)limitY) * mapYs[y1];
                    normalizedDiffusedMapY[y] += test;
                }
            }
            
            //Convert to probability arrays
            float acc = normalizedDiffusedMapX.Sum();
            float[] normalizedDiffusedProbabilityMapX = normalizedDiffusedMapX.Select(x =>x/acc).ToArray();
            
            acc = normalizedDiffusedMapY.Sum();
            float[] normalizedDiffusedProbabilityMapY = normalizedDiffusedMapY.Select(x =>x/acc).ToArray();
            
            // for (int x = 0; x < limitX; x++)
            // {
            //     normalizedDiffusedMapX[x] = mapXs[x];
            // }
            //
            // for (int y = 0; y < limitY; y++)
            // {
            //     normalizedDiffusedMapY[y] = mapYs[y];
            // }
            
            //combine the two arrays into the heatmap
            for (int x = 0; x < limitX; x++)
            {
                for (int y = 0; y < limitY; y++)
                {
                    resultMap[x][y] =  (float)Math.Sqrt(normalizedDiffusedProbabilityMapX[x] * normalizedDiffusedProbabilityMapY[y]);
                }
            }

            HeatMap resHeatMap = new HeatMap();
            resHeatMap.headerX = normalizedDiffusedProbabilityMapX;
            resHeatMap.headerY = normalizedDiffusedProbabilityMapY;
            resHeatMap.map = resultMap;
            return resHeatMap;
        }


        private HeatMap CalculateHeatMap(List<RilData> pastData, float percentageToSample,int limitX, int limitY)
        {
            float[] mapXs = new float[limitX];
            float[] mapYs = new float[limitY];
            float acc = 0;

            int lengthOfLastData = (int)Math.Round((float)pastData.Count * percentageToSample);

            for (int i = 0; i < lengthOfLastData; i++)
            {
                //round the pos to fill heat map cells
                RilData rilData = pastData[pastData.Count - 1 - lengthOfLastData + i];
                int mapXPosIndex = (int)Math.Round(rilData.X);
                int mapYPosIndex = (int)Math.Round(rilData.Y);

                //fill the meta heatmap information ( Acc X and Acc Y)
                mapXs[mapXPosIndex] += rilData.NOMBRE_LOG;
                mapYs[mapYPosIndex] += rilData.NOMBRE_LOG;
            }

            HeatMap valueHeatMap = ConvertArraysToHeatMap(mapXs, mapYs);

            string smap = "";
            using (StreamWriter sw = File.CreateText("debugHeatMap"))
            {
                sw.Write("");
            }
            using (StreamWriter sw = File.AppendText("debugHeatMap"))
            {
                for (int x = 0; x < valueHeatMap.map.Length; x++)
                {
                    for (int y = 0; y < valueHeatMap.map[x].Length; y++)
                    {
                        sw.Write(valueHeatMap.map[x][y] + ";");
                    }
                    sw.Write( "\n");
                }
            }

            return valueHeatMap;
        }

        private List<RilData> PredictFutureData(List<RilData> pastData, float[] spawnCoeff, float[] growthCoeff,
            HeatMap heatMap)
        {
            //generate heat map with rejection sampling for position and noise function for Nb log
            //Add Noise here for organic results
            return pastData;
        }
    }
}