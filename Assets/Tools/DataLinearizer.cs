using System;
using System.Collections.Generic;
using System.Reflection;
using DataProcessing.Generic;
using UnityEngine.Assertions.Comparers;

namespace Tools
{
    public static class DataUtils
    {
        public static List<T> LinearizeTimedData<T>(List<T> nonLinearData, float gapTolerance) where T : ITimedData
        {
            List<ITimedData> linearData = nonLinearData as List<ITimedData>;

            if (linearData == null)
            {
                throw new ArgumentException("Argument is not IITimedData", nameof(nonLinearData));
            }
            
            if (nonLinearData.Count < 2)
            {
                return nonLinearData;
            }
            
            int cursor = 0;
            
            
            while (cursor + 1 < nonLinearData.Count)
            {
                
                //increment toward next gap
                float gap;
                
                do {
                    float currentDataProp = linearData[cursor].GetT();
                    float nextDataProp = linearData[cursor+1].GetT();
                    gap = GetNextDataTimeDistance(currentDataProp, nextDataProp);
                }
                while (++cursor + 1 < nonLinearData.Count && gap <= gapTolerance) ;
                
                //Get info to close the gap
                int numberOfElementToCloseTheGap = GetFirstDataGroupLengthAfterIndex(linearData, cursor);
                float step = (float)Math.Floor(gap / numberOfElementToCloseTheGap);

                CloseGap(linearData, cursor, cursor + numberOfElementToCloseTheGap , step);
                cursor = cursor + numberOfElementToCloseTheGap ;
            }

            return nonLinearData;
        }

        private static float GetNextDataTimeDistance(float currentData, float nextData)
        {
            return nextData - currentData;
        }

        private static int GetFirstDataGroupLengthAfterIndex(List<ITimedData> nonLinearData, int startIndex)
        {
            int lengthOfDataGroup = 1;
            
            for (int i = startIndex+1; i < nonLinearData.Count; i++)
            {
                if (!nonLinearData[i].GetT().Equals(nonLinearData[i + 1].GetT()))
                {
                    return lengthOfDataGroup;
                }

                ++lengthOfDataGroup;
            }

            return lengthOfDataGroup;
        }

        private static void CloseGap(List<ITimedData> nonLinearData, int startIndex, int endIndex, float step)
        {
            int stepIndex = 1;

            for (int i = startIndex; i <= endIndex; i++)
            {
                nonLinearData[i].SetT(stepIndex * step);
                ++stepIndex;
            }
        }
    }
}