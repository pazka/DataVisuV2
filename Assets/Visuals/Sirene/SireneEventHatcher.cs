using System;
using System.Threading;
using DataProcessing.Generic;
using UnityEngine;

namespace Visuals.Sirene
{
    public class SireneEventHatcher : EventHatcher<SireneDataVisual>
    {
        public static SireneEventHatcher Instance { get; } = new SireneEventHatcher();
        //private readonly Tools.Logger logger = GameObject.Find("Logger").GetComponent<Tools.Logger>();

        protected override bool DecideIfReady(SireneDataVisual data, dynamic timePassed)
        {
            return data.Data.T.CompareTo((float)timePassed) <= 0;
        }


        protected override SireneDataVisual ExecuteData(SireneDataVisual dataToTrigger)
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