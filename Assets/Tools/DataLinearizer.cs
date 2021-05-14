using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataProcessing.Generic;
using UnityEngine.Assertions.Comparers;

namespace Tools
{
    public static class DataUtils
    {
        public static List<T> LinearizeTimedData<T>(List<T> nonLinearData, float gapTolerance) where T : ITimedData
        {
            if (nonLinearData == null)
            {
                throw new ArgumentException("Argument is not ITimedData", nameof(nonLinearData));
            }
            
            if (nonLinearData.Count < 2)
            {
                return nonLinearData.OrderBy(rd => rd.GetT()).ToList();
            }
            
            List<T> linearData = nonLinearData.OrderBy(rd => rd.GetT()).ToList();
            int cursor = 0;

            while (cursor + 1 < linearData.Count)
            {
                //increment toward next gap
                float gap;
                
                do {
                    float currentDataProp = linearData[cursor].GetT();
                    float nextDataProp = linearData[cursor+1].GetT();
                    gap = GetNextDataTimeDistance(currentDataProp, nextDataProp);
                }while ((++cursor + 1) < nonLinearData.Count && gap <= gapTolerance) ;

                if (gap > gapTolerance)
                {
                    //Get info to close the gap
                    int nbElemToCloseTheGap = GetFirstDataGroupLengthAfterIndex(linearData, cursor);
                    float step = (float) gap / nbElemToCloseTheGap;

                    CloseGap(linearData, cursor, cursor + nbElemToCloseTheGap , linearData[cursor].GetT() - gap,step);
                    cursor = cursor + nbElemToCloseTheGap - 1 ;
                }

            }

            return linearData;
        }

        private static float GetNextDataTimeDistance(float currentData, float nextData)
        {
            return nextData - currentData;
        }

        private static int GetFirstDataGroupLengthAfterIndex<T>(List<T> nonLinearData, int startIndex) where T : ITimedData
        {
            int dataGroupLength = 1;
            
            for (int i = startIndex+1; i < nonLinearData.Count; i++)
            {
                ++dataGroupLength;
                
                if (!nonLinearData[i].GetT().Equals(nonLinearData[i + 1].GetT()))
                {
                    return dataGroupLength;
                }
            }

            return dataGroupLength;
        }

        private static void CloseGap<T>(List<T> nonLinearData, int startIndex, int endIndex, float gapStart,float step)where T : ITimedData
        {
            int stepIndex = 0;

            //Case less elem than gap
            for (int i = startIndex; i < endIndex; i++)
            {
                nonLinearData[i].SetT(gapStart + stepIndex * step);
                ++stepIndex;
            }
        }
    }
}