using System;
using System.Collections.Generic;
using System.Linq;
using DataProcessing;
using DataProcessing.Generic;
using DataProcessing.Sirene;
using DataProcessing.VisualRestrictor;
using SoundProcessing;
using Tools;
using UnityEngine;
using Logger = Tools.Logger;

namespace Visuals.Sirene
{
    enum DrawingState
    {
        Drawing,
        Destroying,
        Inactive,
        Active
    }

    public class SireneDrawer : MonoBehaviour
    {
        public Logger logger;
        public PureDataConnector pureData;
        IDataConverter sireneDataConverter;
        private IDataExtrapolator sireneDataExtrapolator;
        private EventHatcher<SireneDataVisual> sireneEventHatcher = SireneEventHatcher.Instance;

        public VisualRestrictor restrictor;

        [SerializeField] private float timelapseDuration = 30;
        [SerializeField] private float extrapolationRate = .1f;
        [SerializeField] private float disappearingRate = .01f;
        [SerializeField] private int nbDataBeforeRestart = 90000;
        
        [SerializeField] private bool updateRealTime = true;
        [SerializeField] private int targetFrameRate = 30;
        [SerializeField] private int debugStart = 0;

        [SerializeField] private int minBatSize = 5;
        [SerializeField] private int batSizeCoeff = 25;
        [SerializeField] private float centerX = -50f;
        [SerializeField] private float centerY = 0f;
        [SerializeField] private float centerZ = 5000f;

        private JsonConfiguration config;
        private List<SireneData> allData = new List<SireneData>();
        private Queue<SireneDataVisual> remainingBatDataVisualsToDisplay = new Queue<SireneDataVisual>();
        private List<SireneDataVisual> displayedBatDataVisuals = new List<SireneDataVisual>();

        private float currentIterationStartTimestamp = 0f;
        private int nbIteration = 1;
        private float currentIterationTime = 0f;
        private float controlledFramerateStep = 0.2f;
        private DrawingState drawingState = DrawingState.Inactive;
        private DrawingState previousDrawingState = DrawingState.Inactive;

        public BatVisualPool batVisualPool;
        public BatVisualPool debugBatVisualPool;
        public GameObject progressBar;

        public struct OldCityAlign
        {
            public static Vector3 position = new Vector3(1024, -279.7f, -12);
            public static Quaternion rotation = Quaternion.Euler(0, 0, 18f);
            public static Vector3 localScale = new Vector3(0.8f, 0.8f, 1);
        }
        
        struct CityAlign
        {
            public static Vector3 position = new Vector3(-741.24f, -595.2f, 10);
            public static Quaternion rotation = Quaternion.Euler(0, 0, 0.73f);
            public static Vector3 localScale = new Vector3(0.7f, 0.7f, 1f);
        }

        public void SetActive(bool state)
        {
            drawingState = state ? DrawingState.Active : DrawingState.Inactive;

            if (state)
            {
                logger.Log("Starting Sirene Draw");
                ClearVisuals();
                InitData();
                InitDrawing();
            }
            else
            {
                logger.Log("Stopping Sirene Draw");
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
                this.targetFrameRate = config.targetFrameRate;
                this.batSizeCoeff = config.batSizeCoeff;
                this.debugStart = config.debugStart;
            }

            transform.position += Vector3.Scale(CityAlign.position, new Vector3(config.scaleX, config.scaleY, 1f));
            transform.rotation = CityAlign.rotation;
            transform.localScale = Vector3.Scale(transform.localScale, CityAlign.localScale);

            //Prepare entities
            sireneDataConverter =
                (SireneDataConverter) FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes.SIRENE);
            sireneDataConverter.Init(
                (int) (Screen.width),
                (int) (Screen.height)
            );

            sireneDataExtrapolator = FactoryDataExtrapolator.GetInstance(FactoryDataExtrapolator
                .AvailableDataExtrapolatorTypes.SIRENE);
            
            debugBatVisualPool.PreloadNObjects(200000);
            batVisualPool.PreloadNObjects(300000);
            Application.targetFrameRate = this.targetFrameRate;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyBindings.PauseSireneDrawing))
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
            if (updateRealTime)
            {
                currentIterationTime += controlledFramerateStep;
            }
            else
            {
                currentIterationTime = (Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration;
            }
            
            ICollection<SireneDataVisual> hatchedData =
                sireneEventHatcher.HatchEvents(remainingBatDataVisualsToDisplay, currentIterationTime);
            
            PropagateUpdatedData(hatchedData);

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
                //all displayed bat visuals have been hidden
                ResetVisual();
                return;
            }

            HideSomeVisuals(disappearingRate);
        }

        private void ReturnDataVisualToCorrectPool(SireneDataVisual dataVisual)
        {
            BatVisualPool visualpool = dataVisual.Data.Raw == "future" ? debugBatVisualPool : batVisualPool;
            visualpool.Return(dataVisual.Visual);
        }

        private void ApplyDataToTransform(SireneData sireneData, Transform transform)
        {
            Vector3 currentPosition = new Vector3(
                FlattenCurve.GetFlattenedOneDimensionPoint(sireneData.X, new[] {centerX, centerZ}),
                FlattenCurve.GetFlattenedOneDimensionPoint(sireneData.Y, new[] {centerY, centerZ}),
                (float) VisualPlanner.Layers.Sirene
            );

            transform.parent =
                gameObject.transform; // to make the visual affected by the parent gameobject 

            //currentPosition = transform.rotation * Vector3.Scale(currentPosition, transform.localScale);
            transform.localPosition = currentPosition;
            transform.localRotation = new Quaternion(0, 0, sireneData.T * 90, 0);
            //currentSireneData.SetX(batVisual.transform.position.x);
            //currentSireneData.SetY(batVisual.transform.position.y);

            transform.localScale = new Vector3(
                minBatSize + sireneData.EntityCount * batSizeCoeff,
                minBatSize + sireneData.EntityCount * batSizeCoeff
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
            foreach (SireneData currentSireneData in allData)
            {
                BatVisualPool visualpool = currentSireneData.Raw == "future" ? debugBatVisualPool : batVisualPool;
                GameObject batVisual = visualpool.GetOne();

                if (!batVisual)
                    continue;

                batVisual.tag = "bat:tmp";
                batVisual.SetActive(false);
                ApplyDataToTransform(currentSireneData, batVisual.transform);

                //restriction to line ignored for the moment
                if (true || restrictor.IsPointInPoly(batVisual.transform.position, restrictor.restrictionLine))
                {
                    remainingBatDataVisualsToDisplay.Enqueue(new SireneDataVisual(currentSireneData, batVisual));
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
            List<SireneData> initialDataToExtrapolate;
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

            initialDataToExtrapolate = (List<SireneData>) sireneDataConverter.GetAllData();

            sireneDataExtrapolator.InitExtrapolation(initialDataToExtrapolate, new SireneExtrapolationParameters()
            {
                isOnlyFutureExtrapolating = true,
                extrapolationRate = extrapolationRate
            });
        }

        private List<SireneData> GetDataToDisplay()
        {
            var config = Configuration.GetConfig();
            
            if (currentIterationStartTimestamp == 0f || allData.Count > this.nbDataBeforeRestart)
            {
                InitData();
            }

            List<SireneData> tmpAllData = (List<SireneData>) sireneDataExtrapolator.RetrieveExtrapolation();

            sireneDataExtrapolator.InitExtrapolation(tmpAllData, new SireneExtrapolationParameters()
            {
                isOnlyFutureExtrapolating = false,
                extrapolationRate = extrapolationRate
            });

            if (debugStart > 0 && debugStart < nbDataBeforeRestart)
            {
                //Option to extrapolate until the desired data step
                
                while (tmpAllData.Count < debugStart)
                {
                    tmpAllData = (List<SireneData>) sireneDataExtrapolator.RetrieveExtrapolation();

                    sireneDataExtrapolator.InitExtrapolation(tmpAllData, new SireneExtrapolationParameters()
                    {
                        isOnlyFutureExtrapolating = false,
                        extrapolationRate = extrapolationRate
                    });
                }
            }

            controlledFramerateStep = (timelapseDuration * Application.targetFrameRate) / tmpAllData.Count;

            return tmpAllData;
        }

        public void ClearVisuals()
        {
            GameObject[] visualsToDestroy = displayedBatDataVisuals.Select(x => x.Visual).ToArray();

            foreach (SireneDataVisual dataVisual in displayedBatDataVisuals)
            {
                ReturnDataVisualToCorrectPool(dataVisual);
            }

            while (remainingBatDataVisualsToDisplay.Count > 0)
            {
                var dataVisual = remainingBatDataVisualsToDisplay.Dequeue();
                ReturnDataVisualToCorrectPool(dataVisual);
            }

            displayedBatDataVisuals = new List<SireneDataVisual>();
            remainingBatDataVisualsToDisplay = new Queue<SireneDataVisual>();
            currentIterationStartTimestamp = Time.realtimeSinceStartup;

            //specific to updateControlled FrameRate
            currentIterationTime = 0;

            //logger 
            //logger.Reset();
        }

        public void HideSomeVisuals(float disappearingRate)
        {
            int nbToTake = (int) Math.Max(Math.Round(displayedBatDataVisuals.Count * disappearingRate), 50);
            SireneDataVisual[] dataVisualsToDestroy = displayedBatDataVisuals.Select(x => x).Take(nbToTake).ToArray();
            int nbTook = dataVisualsToDestroy.Length;
            foreach (var dataVisualToDestroy in dataVisualsToDestroy)
            {
                ReturnDataVisualToCorrectPool(dataVisualToDestroy);
            }

            displayedBatDataVisuals =
                displayedBatDataVisuals.GetRange(nbTook, displayedBatDataVisuals.Count() - nbTook);
        }

        void PropagateUpdatedData(ICollection<SireneDataVisual> hatchedData)
        {
            progressBar.transform.localScale = new Vector3(currentIterationTime * 1920, 10);
            pureData.SendOscMessage("/data_clock", currentIterationTime);

            foreach (SireneDataVisual sireneDataVisual in hatchedData)
            {
                displayedBatDataVisuals.Add(sireneDataVisual);

                pureData.SendOscMessage("/data_bang", 1);
            }
            
            Renderer batVisualRenderer;
            foreach (SireneDataVisual sireneDataVisual in displayedBatDataVisuals)
            {
                sireneDataVisual.Visual.TryGetComponent<Renderer>(out batVisualRenderer);
                batVisualRenderer.material.SetFloat("_Clock", currentIterationTime);
            }
        }
    }
}