using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using DataProcessing;
using DataProcessing.Ril;
using UnityEngine;

namespace Visuals.Ril
{
    public class RilDrawer : MonoBehaviour
    {
        public Tools.Logger logger;
        RilDataConverter rilDataConverter;
        private RilDataExtrapolator rilDataExtrapolator;
        [SerializeField] private float timelapseDuration = 30;
        [SerializeField] private bool controlledUpdateTime = false;
        private List<RilData> allData;
        private Queue<RilDataVisual> remainingBatDataVisuals = new Queue<RilDataVisual>();
        private RilEventHatcher rilEventHatcher = RilEventHatcher.Instance;
        private float currentIterationStartTimestamp = 0f;
        private bool isActive = false;

        private float myTime = 0f;
        private float step = 0.0005f ;

        public GameObject batRessource;
        public GameObject batFutureRessource;
        public GameObject progressBar;

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

        public void SetActive()
        {
            InitDrawing();
            isActive = true;
        }

        public void ClearVisuals()
        {
            GameObject[] visualsToDestroy = GameObject.FindGameObjectsWithTag("bat:tmp");
            foreach (var visualToDestroy in visualsToDestroy)
            {
                Destroy(visualToDestroy);
            }
            
            remainingBatDataVisuals = new Queue<RilDataVisual>();
            currentIterationStartTimestamp = Time.realtimeSinceStartup;
            
            //specific to updateControlled FrameRate
            myTime = 0;
            
            //logger 
            logger.Reset();
        }

        private List<RilData> GetAllData()
        {
            List<RilData> tmpAllData;
            if (currentIterationStartTimestamp == 0f)
            {
                tmpAllData = (List<RilData>) rilDataConverter.GetAllData();
                rilDataExtrapolator.InitExtrapolation(tmpAllData,true);
            }
            
            tmpAllData = (List<RilData>) rilDataExtrapolator.RetrieveExtrapolation();
            
            rilDataExtrapolator.InitExtrapolation(tmpAllData,false);
            step = timelapseDuration / tmpAllData.Count ;

            return tmpAllData;
        }

        private void InitDrawing()
        {
            allData = GetAllData();

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
                    2 + currentRilData.NOMBRE_LOG * 30,
                    2 + currentRilData.NOMBRE_LOG * 30);

                remainingBatDataVisuals.Enqueue(new RilDataVisual(currentRilData, batVisual));
            }
        }

        void Update()
        {
            if (!isActive) return;
            
            progressBar.transform.localScale = new Vector3((Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration * 1920, 10);
            
            if(controlledUpdateTime)
                UpdateControlledFrameRate();
            else
                UpdateRealtime();
            
            if (remainingBatDataVisuals.Count == 0)
            {
                ClearVisuals();
                InitDrawing();
            }
        }
        
        void UpdateControlledFrameRate()
        {
            int hatched = rilEventHatcher.HatchEvents(remainingBatDataVisuals, myTime).Count;
            //logger.Log("Hatched : " + hatched);
            myTime += step;
        }

        void UpdateRealtime()
        {
            int hatched = rilEventHatcher
                .HatchEvents(remainingBatDataVisuals, (Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration).Count;
                //logger.Log("Hatched : " + hatched);
        }
    }
}