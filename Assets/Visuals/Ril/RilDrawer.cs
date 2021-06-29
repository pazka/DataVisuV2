using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using DataProcessing;
using DataProcessing.Ril;
using UnityEngine;

namespace Visuals.Ril
{
    enum DrawingState
    {
        Drawing,Destroying,Inactive,Active
    }
    public class RilDrawer : MonoBehaviour
    {
        public Tools.Logger logger;
        RilDataConverter rilDataConverter;
        private RilDataExtrapolator rilDataExtrapolator;
        [SerializeField] private float timelapseDuration = 30;
        [SerializeField] private float extrapolationRate = .1f;
        [SerializeField] private bool controlledUpdateTime = false;
        [SerializeField] private float disappearingRate = .01f;
        private List<RilData> allData;
        private Queue<RilDataVisual> remainingBatDataVisuals = new Queue<RilDataVisual>();
        private List<RilDataVisual> usedBatDataVisuals = new List<RilDataVisual>();
        private RilEventHatcher rilEventHatcher = RilEventHatcher.Instance;
        private float currentIterationStartTimestamp = 0f;

        private int nbIteration = 1;
        private float myTime = 0f;
        private float step = 0.0005f ;
        private DrawingState drawingState = DrawingState.Drawing;

        public GameObject batRessource;
        public GameObject batFutureRessource;
        public GameObject progressBar;
        
        public void SetActive()
        {
            drawingState = DrawingState.Active;
            InitDrawing();
        }
        void Start()
        {
            //Prepare entities        
            rilDataConverter =
                (RilDataConverter) FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes.RIL);
            rilDataConverter.Init(Screen.width, Screen.height);

            rilDataExtrapolator =
                (RilDataExtrapolator) FactoryDataExtrapolator.GetInstance(FactoryDataExtrapolator
                    .AvailableDataExtrapolatorTypes.RIL);
        }

        void Update()
        {
            switch (drawingState)
            {
                case DrawingState.Drawing : 
                    DisplayData();
                    break;
                
                case DrawingState.Destroying:
                    DestroyData();
                    break;
                
                case DrawingState.Inactive:
                    return;
            }
        }

        void DisplayData()
        {
            progressBar.transform.localScale = new Vector3((Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration * 1920, 10);
            
            if(controlledUpdateTime)
                UpdateControlledFrameRate();
            else
                UpdateRealtime();
            
            
            if (remainingBatDataVisuals.Count == 0)
            {
                drawingState = DrawingState.Destroying;
            }
        }

        void ResetData()
        {
            ClearVisuals();
            InitDrawing();
        }
        
        void DestroyData()
        {
            
            if (usedBatDataVisuals.Count == 0)
            {
                ResetData();
                return;
            }
            
            HideSomeVisuals(disappearingRate);
            
        }

        private void InitDrawing()
        {
            allData = GetDataToDisplay();
            logger.Log("#" + nbIteration++ + " = " + allData.Count);

            foreach (RilData currentRilData in allData)
            {
                GameObject batVisual;
                if (currentRilData.Raw == "future")
                {
                    batVisual = Instantiate(batFutureRessource);
                }
                else
                {
                    batVisual = Instantiate(batRessource);
                }
                
                batVisual.tag = "bat:tmp";
                batVisual.SetActive(false);

                if (!batVisual)
                    break;

                Vector3 currentPosition =
                    new Vector3(currentRilData.X, currentRilData.Y, (float) VisualPlanner.Layers.Ril);
                batVisual.transform.position = currentPosition;
                batVisual.transform.localScale = new Vector3(
                    5 + currentRilData.NOMBRE_LOG * 50,
                    5 + currentRilData.NOMBRE_LOG * 50);

                remainingBatDataVisuals.Enqueue(new RilDataVisual(currentRilData, batVisual));
            }
            
            drawingState = DrawingState.Drawing;
        }

        
        private List<RilData> GetDataToDisplay()
        {
            List<RilData> tmpAllData;
            if (currentIterationStartTimestamp == 0f)
            {
                tmpAllData = (List<RilData>) rilDataConverter.GetAllData();
                rilDataExtrapolator.InitExtrapolation(tmpAllData,new RilExtrapolationParameters()
                {
                    isOnlyFutureExtrapolating = true,
                    extrapolationRate = extrapolationRate
                });
            }
            
            tmpAllData = (List<RilData>) rilDataExtrapolator.RetrieveExtrapolation();
            
            rilDataExtrapolator.InitExtrapolation(tmpAllData,new RilExtrapolationParameters()
            {
                isOnlyFutureExtrapolating = false,
                extrapolationRate = extrapolationRate
            });
            step = timelapseDuration / tmpAllData.Count ;

            return tmpAllData;
        }
        public void ClearVisuals()
        {
            GameObject[] visualsToDestroy = usedBatDataVisuals.Select(x => x.Visual).ToArray();
            
            foreach (var visualToDestroy in visualsToDestroy)
            {
                Destroy(visualToDestroy);
            }

            usedBatDataVisuals = new List<RilDataVisual>();
            remainingBatDataVisuals = new Queue<RilDataVisual>();
            currentIterationStartTimestamp = Time.realtimeSinceStartup;
            
            //specific to updateControlled FrameRate
            myTime = 0;
            
            //logger 
            //logger.Reset();
        }
        
        public void HideSomeVisuals(float disappearingRate)
        {
            int nbToTake = (int)Math.Max( Math.Round(usedBatDataVisuals.Count * disappearingRate),50);
            GameObject[] visualsToDestroy = usedBatDataVisuals.Select(x => x.Visual).Take(nbToTake).ToArray();
            int nbTook = visualsToDestroy.Length;
            foreach (var visualToDestroy in visualsToDestroy)
            {
                visualToDestroy.SetActive(false);
                Destroy(visualToDestroy.gameObject);
            }

            usedBatDataVisuals = usedBatDataVisuals.GetRange(nbTook,usedBatDataVisuals.Count()-nbTook);
        }
        
        void UpdateControlledFrameRate()
        {
            ICollection<RilDataVisual> hatchedData  = rilEventHatcher.HatchEvents(remainingBatDataVisuals, myTime);
            myTime += step;
            
            foreach (RilDataVisual rilDataVisual in hatchedData)
            {
                usedBatDataVisuals.Add(rilDataVisual);
            }
        }

        void UpdateRealtime()
        {
            ICollection<RilDataVisual> hatchedData  = rilEventHatcher
                .HatchEvents(remainingBatDataVisuals, (Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration);

            foreach (RilDataVisual rilDataVisual in hatchedData)
            {
                usedBatDataVisuals.Add(rilDataVisual);
            }
        }
    }
}