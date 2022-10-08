using System;
using System.Collections.Generic;
using System.Linq;
using DataProcessing;
using DataProcessing.Generic;
using DataProcessing.Ril;
using DataProcessing.VisualRestrictor;
using SoundProcessing;
using Tools;
using UnityEngine;
using Logger = Tools.Logger;

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
        public Logger logger;
        public PureDataConnector pureData;
        IDataConverter rilDataConverter;
        private IDataExtrapolator rilDataExtrapolator;
        private EventHatcher<RilDataVisual> rilEventHatcher = RilEventHatcher.Instance;

        public VisualRestrictor restrictor;

        [SerializeField] private float timelapseDuration = 30;
        [SerializeField] private float extrapolationRate = .1f;
        [SerializeField] private bool controlledUpdateTime = false;
        [SerializeField] private float controlledFramerateStep = 0.0005f;
        [SerializeField] private float disappearingRate = .01f;
        [SerializeField] private int nbDataBeforeRestart = 90000;


        [SerializeField] private int minBatSize = 5;
        [SerializeField] private int batSizeCoeff = 25;
        [SerializeField] private float centerX = -50f;
        [SerializeField] private float centerY = 0f;
        [SerializeField] private float centerZ = 5000f;

        private JsonConfiguration config;
        private List<RilData> allData = new List<RilData>();
        private Queue<RilDataVisual> remainingBatDataVisualsToDisplay = new Queue<RilDataVisual>();
        private List<RilDataVisual> displayedBatDataVisuals = new List<RilDataVisual>();

        private float currentIterationStartTimestamp = 0f;
        private int nbIteration = 1;
        private float currentIterationTime = 0f;
        private DrawingState drawingState = DrawingState.Inactive;
        private DrawingState previousDrawingState = DrawingState.Inactive;

        public BatVisualPool batVisualPool;
        public BatVisualPool debugBatVisualPool;
        public GameObject progressBar;

        public struct CityAlign
        {
            public static Vector3 position = new Vector3(1024, -279.7f, -12);
            public static Quaternion rotation = Quaternion.Euler(0, 0, 18f);
            public static Vector3 localScale = new Vector3(0.8f, 0.8f, 1);
        }

        public void SetActive(bool state)
        {
            drawingState = state ? DrawingState.Active : DrawingState.Inactive;

            if (state)
            {
                logger.Log("Starting Ril Draw");
                ClearVisuals();
                InitData();
                InitDrawing();
            }
            else
            {
                logger.Log("Stopping Ril Draw");
                ClearVisuals();
                displayedBatDataVisuals = remainingBatDataVisualsToDisplay.ToList();
                ClearVisuals();
            }
        }

        void Start()
        {
            config = Configuration.GetConfig();

            if (!config.isDev)
            {
                this.timelapseDuration = config.timelapseDuration;
                this.extrapolationRate = config.extrapolationRate;
                this.disappearingRate = config.disappearingRate;
                this.nbDataBeforeRestart = config.nbDataBeforeRestart;
            }

            transform.position += Vector3.Scale(CityAlign.position, new Vector3(config.scaleX, config.scaleY, 1f));
            transform.rotation = CityAlign.rotation;
            transform.localScale = Vector3.Scale(transform.localScale, CityAlign.localScale);

            //Prepare entities        
            rilDataConverter =
                (RilDataConverter) FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes.RIL);
            rilDataConverter.Init(
                (int) (Screen.width),
                (int) (Screen.height)
            );

            rilDataExtrapolator = FactoryDataExtrapolator.GetInstance(FactoryDataExtrapolator
                .AvailableDataExtrapolatorTypes.RIL);
            
            debugBatVisualPool.PreloadNObjects(100000);
            batVisualPool.PreloadNObjects(40000);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyBindings.PauseRilDrawing))
            {
                if (drawingState != DrawingState.Inactive)
                {
                    logger.Log("Pausing Drawing");
                    previousDrawingState = drawingState;
                    drawingState = DrawingState.Inactive;
                }
                else
                {
                    logger.Log("Resuming Drawing");
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
                float progress = Convert.ToSingle(displayedBatDataVisuals.Count) / Convert.ToSingle(allData.Count);
                progressBar.transform.localScale = new Vector3(progress * 1920, 10);
            }
            else
            {
                UpdateRealtime();
                float progress = (Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration;
                progressBar.transform.localScale = new Vector3(progress * 1920, 10);
            }

            if (remainingBatDataVisualsToDisplay.Count == 0)
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
            if (displayedBatDataVisuals.Count == 0)
            {
                ResetVisual();
                return;
            }

            HideSomeVisuals(disappearingRate);
            GC.Collect();
        }

        private void ReturnDataVisualToCorrectPool(RilDataVisual dataVisual)
        {
            BatVisualPool visualpool = dataVisual.Data.Raw == "future" ? debugBatVisualPool : batVisualPool;
            visualpool.Return(dataVisual.Visual);
        }

        private void ApplyDataToTransform(RilData rilData, Transform transform)
        {
            Vector3 currentPosition = new Vector3(
                FlattenCurve.GetFlattenedOneDimensionPoint(rilData.X, new[] {centerX, centerZ}),
                FlattenCurve.GetFlattenedOneDimensionPoint(rilData.Y, new[] {centerY, centerZ}),
                (float) VisualPlanner.Layers.Ril
            );

            transform.parent =
                gameObject.transform; // to make the visual affected by the parent gameobject 

            //currentPosition = transform.rotation * Vector3.Scale(currentPosition, transform.localScale);
            transform.localPosition = currentPosition;
            transform.localRotation = new Quaternion(0, 0, rilData.T * 90, 0);
            //currentRilData.SetX(batVisual.transform.position.x);
            //currentRilData.SetY(batVisual.transform.position.y);

            transform.localScale = new Vector3(
                minBatSize + rilData.NOMBRE_LOG * batSizeCoeff,
                minBatSize + rilData.NOMBRE_LOG * batSizeCoeff
            );
        }

        private void InitDrawing()
        {
            allData = GetDataToDisplay();
            logger.Log("#" + nbIteration++ + " = " + allData.Count);
            //
            // var centerVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // centerVis.transform.localPosition = new Vector3(centerX, centerY, -15);
            // centerVis.transform.localScale = new Vector3(20,20,20);
            // centerVis.transform.parent = gameObject.transform;
            //
            foreach (RilData currentRilData in allData)
            {
                BatVisualPool visualpool = currentRilData.Raw == "future" ? debugBatVisualPool : batVisualPool;
                GameObject batVisual = visualpool.GetOne();

                if (!batVisual)
                    continue;

                batVisual.tag = "bat:tmp";
                batVisual.SetActive(false);
                ApplyDataToTransform(currentRilData, batVisual.transform);

                //restriction to line
                if (restrictor.IsPointInPoly(batVisual.transform.position, restrictor.restrictionLine))
                {
                    remainingBatDataVisualsToDisplay.Enqueue(new RilDataVisual(currentRilData, batVisual));
                }
                else
                {
                    visualpool.Return(batVisual);
                }
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
            var config = Configuration.GetConfig();
            if (currentIterationStartTimestamp == 0f || allData.Count > this.nbDataBeforeRestart)
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
            GameObject[] visualsToDestroy = displayedBatDataVisuals.Select(x => x.Visual).ToArray();

            foreach (RilDataVisual dataVisual in displayedBatDataVisuals)
            {
                ReturnDataVisualToCorrectPool(dataVisual);
            }

            while (remainingBatDataVisualsToDisplay.Count > 0)
            {
                var dataVisual = remainingBatDataVisualsToDisplay.Dequeue();
                ReturnDataVisualToCorrectPool(dataVisual);
            }

            displayedBatDataVisuals = new List<RilDataVisual>();
            remainingBatDataVisualsToDisplay = new Queue<RilDataVisual>();
            currentIterationStartTimestamp = Time.realtimeSinceStartup;

            //specific to updateControlled FrameRate
            currentIterationTime = 0;

            //logger 
            //logger.Reset();
        }

        public void HideSomeVisuals(float disappearingRate)
        {
            int nbToTake = (int) Math.Max(Math.Round(displayedBatDataVisuals.Count * disappearingRate), 50);
            RilDataVisual[] dataVisualsToDestroy = displayedBatDataVisuals.Select(x => x).Take(nbToTake).ToArray();
            int nbTook = dataVisualsToDestroy.Length;
            foreach (var dataVisualToDestroy in dataVisualsToDestroy)
            {
                ReturnDataVisualToCorrectPool(dataVisualToDestroy);
            }

            displayedBatDataVisuals =
                displayedBatDataVisuals.GetRange(nbTook, displayedBatDataVisuals.Count() - nbTook);
        }

        void UpdateControlledFrameRate()
        {
            ICollection<RilDataVisual> hatchedData =
                rilEventHatcher.HatchEvents(remainingBatDataVisualsToDisplay, currentIterationTime);
            currentIterationTime += controlledFramerateStep;

            DefinitiveUpdateAction(hatchedData);
        }

        void UpdateRealtime()
        {
            ICollection<RilDataVisual> hatchedData = rilEventHatcher
                .HatchEvents(remainingBatDataVisualsToDisplay,
                    (Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration);

            DefinitiveUpdateAction(hatchedData);
        }

        void DefinitiveUpdateAction(ICollection<RilDataVisual> hatchedData)
        {
            float progress = Convert.ToSingle(displayedBatDataVisuals.Count) / Convert.ToSingle(allData.Count);
            pureData.SendOscMessage("/data_clock", progress);
            Renderer batVisualRenderer;

            foreach (RilDataVisual rilDataVisual in hatchedData)
            {
                displayedBatDataVisuals.Add(rilDataVisual);

                pureData.SendOscMessage("/data_bang", 1);
            }

            foreach (RilDataVisual rilDataVisual in displayedBatDataVisuals)
            {
                rilDataVisual.Visual.TryGetComponent<Renderer>(out batVisualRenderer);
                batVisualRenderer.material.SetFloat("_Clock", progress);
            }
        }
    }
}