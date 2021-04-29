using System;
using System.Collections.Generic;
using System.Threading;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.Ril
{
    public class RilDataExtrapolator : IDataExtrapolator
    {
        private Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();
        private List<RilData> dataToExtrapolate;
        private List<RilData> extrapolatedData;
        private Mutex generatingData;
        private Thread generationThread;
        AutoResetEvent waitForExtrapolation = new AutoResetEvent(false);
        
        public RilDataExtrapolator()
        {
            generatingData = new Mutex();
        }
        
        public void InitExtrapolation(IEnumerable<IData> inputData)
        {
            generatingData.WaitOne();
            dataToExtrapolate = (List<RilData>)inputData;
            extrapolatedData = new List<RilData>();
            generatingData.ReleaseMutex();
            
            generationThread = new Thread(ExecuteExtrapolation);
            generationThread.Start();
        }
        
        private void ExecuteExtrapolation()
        {
            generatingData.WaitOne();

            for (int i = 0; i < 100; i++)
            {
                logger.Log($"I'm at {i}% completion");
                Thread.Sleep(1000);
            }

            extrapolatedData = dataToExtrapolate;
            
            generatingData.ReleaseMutex();
            waitForExtrapolation.Set();
        }

        public IEnumerable<IData> RetreiveExtrapolation()
        {
            waitForExtrapolation.WaitOne();
            return extrapolatedData;
        }
    }
}