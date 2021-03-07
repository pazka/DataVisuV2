using System;
using DataProcessing.Generic;
using UnityEngine;

namespace Visuals.Ril
{
    public class RilEventHatcher : EventHatcher<RilDataVisual>
    {
        public static RilEventHatcher Instance { get; } = new RilEventHatcher();

        protected override bool DecideIfReady(RilDataVisual data, dynamic timePassed)
        {
            return data.Data.T <= (float)timePassed;
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