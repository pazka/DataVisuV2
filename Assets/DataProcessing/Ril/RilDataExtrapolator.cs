using System;
using System.Collections.Generic;
using System.Threading;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.Ril
{
    public class RilDataExtrapolator : IDataExtrapolator
    {
        private readonly Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();
        private List<RilData> dataToExtrapolate;
        private List<RilData> extrapolatedData;
        private readonly Mutex generatingData;
        private Thread generationThread;
        private CancellationToken token;
        
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

            for (var i = 1; i <= 30; i++)
            {
                logger.Log($"I'm at {( (double)i / 30 ) * 100}% completion");
                Thread.Sleep(1000);
            }

            extrapolatedData = dataToExtrapolate;
            
            generatingData.ReleaseMutex();
            logger.Log($"Extrapolation is Ready ! ");
        }

        public IEnumerable<IData> RetrieveExtrapolation()
        {
            logger.Log($"Waiting for extrapolation thread ot end");
            generationThread.Join();
            return extrapolatedData;
        }
    }
}