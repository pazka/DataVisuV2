using System;
using System.Threading;
using DataProcessing.Generic;
using UnityEngine;

namespace Visuals.Ril
{
    public class RilEventHatcher : EventHatcher<RilDataVisual>
    {
        public static RilEventHatcher Instance { get; } = new RilEventHatcher();
        //private readonly Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();

        protected override bool DecideIfReady(RilDataVisual data, dynamic timePassed)
        {
            return data.Data.T.CompareTo((float)timePassed) <= 0;
        }


        protected override RilDataVisual ExecuteData(RilDataVisual dataToTrigger)
        {
            GameObject visual = dataToTrigger.Visual;

            if (!visual)
            {
                throw new ArgumentException(@"Couldn't convert the visual");
            }
            
            visual.SetActive(true);

            return dataToTrigger;
        }
    }
}