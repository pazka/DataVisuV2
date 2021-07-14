using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using DataProcessing;
using DataProcessing.Ril;
using SoundProcessing;
using Tools;
using UnityEngine;
using UnityEngine.UI;

namespace Visuals.Ril
{
    enum DrawingState
    {
        Drawing,
        Destroying,
        Inactive,
        Active
    }

    public class RilDrawer : MonoBehaviour
    {
        public Tools.Logger logger;
        public PureDataConnector pureData;
        RilDataConverter rilDataConverter;
        private RilDataExtrapolator rilDataExtrapolator;
        private RilEventHatcher rilEventHatcher = RilEventHatcher.Instance;

        [SerializeField] private float timelapseDuration = 30;
        [SerializeField] private float extrapolationRate = .1f;
        [SerializeField] private bool controlledUpdateTime = false;
        [SerializeField] private float controlledFramerateStep = 0.0005f;
        [SerializeField] private float disappearingRate = .01f;

        private List<RilData> allData = new List<RilData>();
        private Queue<RilDataVisual> remainingBatDataVisuals = new Queue<RilDataVisual>();
        private List<RilDataVisual> usedBatDataVisuals = new List<RilDataVisual>();

        private float currentIterationStartTimestamp = 0f;
        private int nbIteration = 1;
        private float currentIterationTime = 0f;
        private DrawingState drawingState = DrawingState.Inactive;
        private DrawingState previousDrawingState = DrawingState.Inactive;

        public GameObject batRessource;
        public GameObject batFutureRessource;
        public GameObject progressBar;

        struct CityAlign
        {
            public static Vector3 position = new Vector3(-3410f,-1014.5f,0);
            public static Quaternion rotation = Quaternion.Euler(0, 0, 20f);
            public static Vector3 localScale = new Vector3(-0.79f,0.88f,1);
        }
        
        public void SetActive(bool state)
        {
            drawingState = state ? DrawingState.Active : DrawingState.Inactive;

            if (state)
            {
                ClearVisuals();
                InitData();
                InitDrawing();
            }
            else
            {
                ClearVisuals();
                usedBatDataVisuals = remainingBatDataVisuals.ToList();
                ClearVisuals();
            }

        }

        void Start()
        {
            transform.position = transform.position + CityAlign.position;
            transform.rotation = transform.rotation * CityAlign.rotation;
            transform.localScale = Vector3.Scale(transform.localScale ,CityAlign.localScale);
            
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
            if (Input.GetKeyDown(KeyBindings.PauseRilDrawing))
            {
                if (drawingState != DrawingState.Inactive)
                {
                    previousDrawingState = drawingState;
                    drawingState = DrawingState.Inactive;
                }
                else
                {
                    drawingState = previousDrawingState;
                }
            }

            switch (drawingState)
            {
                case DrawingState.Drawing:
                    DisplayData();
                    break;

                case DrawingState.Destroying:
                    DestroyVisuals();
                    break;
                
                case DrawingState.Inactive:
                    return;
            }
        }

        void DisplayData()
        {

            
            
            if (controlledUpdateTime)
            {
                UpdateControlledFrameRate();
                float progress =  Convert.ToSingle(usedBatDataVisuals.Count) / Convert.ToSingle(allData.Count);
                progressBar.transform.localScale = new Vector3(progress * 1920,10);
            }
            else
            {
                UpdateRealtime();
                float progress = (Time.realtimeSinceStartup - currentIterationStartTimestamp)/ timelapseDuration;
                progressBar.transform.localScale =new Vector3( progress * 1920, 10);
            }

            if (remainingBatDataVisuals.Count == 0)
            {
                drawingState = DrawingState.Destroying;
            }
        }

        void ResetVisual()
        {
            ClearVisuals();
            InitDrawing();
        }

        void DestroyVisuals()
        {
            if (usedBatDataVisuals.Count == 0)
            {
                ResetVisual();
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

                Vector3 currentPosition = new Vector3(
                        currentRilData.X, 
                        currentRilData.Y,
                        (float) VisualPlanner.Layers.Ril
                        ) + transform.position;

                currentPosition = transform.rotation * Vector3.Scale(currentPosition, transform.localScale);
                
                batVisual.transform.position = currentPosition;
                batVisual.transform.localScale = new Vector3(
                    5 + currentRilData.NOMBRE_LOG * 25,
                    5 + currentRilData.NOMBRE_LOG * 25);

                remainingBatDataVisuals.Enqueue(new RilDataVisual(currentRilData, batVisual));
            }

            drawingState = DrawingState.Drawing;
        }

        private void InitData()
        {
            List<RilData> initialDataToExtrapolate;
            if (currentIterationStartTimestamp == 0f)
            {
                logger.Log("Initializing all data for the first time Or maybe the second");
            }
            else
            {
                logger.Log("Re-initializing all data");
            }

            //starting point, we extrapolate the future of the original dataset once
            //the next extrapolation will only be on the current timeline
            
            initialDataToExtrapolate = (List<RilData>) rilDataConverter.GetAllData();

            rilDataExtrapolator.InitExtrapolation(initialDataToExtrapolate, new RilExtrapolationParameters()
            {
                isOnlyFutureExtrapolating = true,
                extrapolationRate = extrapolationRate
            });
        }

        private List<RilData> GetDataToDisplay()
        {
            if (currentIterationStartTimestamp == 0f || allData.Count > 90000)
            {
                InitData();
            }

            List<RilData> tmpAllData = (List<RilData>) rilDataExtrapolator.RetrieveExtrapolation();

            rilDataExtrapolator.InitExtrapolation(tmpAllData, new RilExtrapolationParameters()
            {
                isOnlyFutureExtrapolating = false,
                extrapolationRate = extrapolationRate
            });

            controlledFramerateStep = timelapseDuration / tmpAllData.Count;

            return tmpAllData;
        }

        public void ClearVisuals()
        {
            GameObject[] visualsToDestroy = usedBatDataVisuals.Select(x => x.Visual).ToArray();

            foreach (var visualToDestroy in visualsToDestroy)
            {
                Destroy(visualToDestroy);
            }
            while (remainingBatDataVisuals.Count > 0)
            {
                var visualToDestroy = remainingBatDataVisuals.Dequeue().Visual;
                Destroy(visualToDestroy);
            }

            usedBatDataVisuals = new List<RilDataVisual>();
            remainingBatDataVisuals = new Queue<RilDataVisual>();
            currentIterationStartTimestamp = Time.realtimeSinceStartup;

            //specific to updateControlled FrameRate
            currentIterationTime = 0;

            //logger 
            //logger.Reset();
        }

        public void HideSomeVisuals(float disappearingRate)
        {
            int nbToTake = (int) Math.Max(Math.Round(usedBatDataVisuals.Count * disappearingRate), 50);
            GameObject[] visualsToDestroy = usedBatDataVisuals.Select(x => x.Visual).Take(nbToTake).ToArray();
            int nbTook = visualsToDestroy.Length;
            foreach (var visualToDestroy in visualsToDestroy)
            {
                visualToDestroy.SetActive(false);
                Destroy(visualToDestroy.gameObject);
            }

            usedBatDataVisuals = usedBatDataVisuals.GetRange(nbTook, usedBatDataVisuals.Count() - nbTook);
        }

        void UpdateControlledFrameRate()
        {
            ICollection<RilDataVisual> hatchedData =
                rilEventHatcher.HatchEvents(remainingBatDataVisuals, currentIterationTime);
            currentIterationTime += controlledFramerateStep;

            DefinitiveUpdateAction(hatchedData);
        }

        void UpdateRealtime()
        {
            ICollection<RilDataVisual> hatchedData = rilEventHatcher
                .HatchEvents(remainingBatDataVisuals,
                    (Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration);
            
            DefinitiveUpdateAction(hatchedData);
        }

        void DefinitiveUpdateAction(ICollection<RilDataVisual> hatchedData)
        {

            float progress = Convert.ToSingle(usedBatDataVisuals.Count) / Convert.ToSingle(allData.Count);
            pureData.SendOscMessage("/data_clock", progress);
            Renderer batVisualRenderer;
            
            foreach (RilDataVisual rilDataVisual in hatchedData)
            {
                usedBatDataVisuals.Add(rilDataVisual);

                pureData.SendOscMessage("/data_bang", 1);
            }

            foreach (RilDataVisual rilDataVisual in usedBatDataVisuals)
            {
                rilDataVisual.Visual.TryGetComponent<Renderer>(out batVisualRenderer);
                batVisualRenderer.material.SetFloat("_Clock",progress);
            }
        }
    }
}