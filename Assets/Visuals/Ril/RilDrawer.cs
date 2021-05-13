using System.Collections.Generic;
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
        private List<RilData> allData;
        private Stack<RilDataVisual> remainingBatDataVisuals = new Stack<RilDataVisual>();
        private RilEventHatcher rilEventHatcher = RilEventHatcher.Instance;
        private float currentIterationStartTimestamp = 0f;
        

        private float myTime = 0f;
        private float step = 0.0005f ;

        public GameObject batRessource;
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

        public void ClearVisuals()
        {
            GameObject[] visualsToDestroy = GameObject.FindGameObjectsWithTag("bat:tmp");
            foreach (var visualToDestroy in visualsToDestroy)
            {
                Destroy(visualToDestroy);
            }
            
            remainingBatDataVisuals = new Stack<RilDataVisual>();
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
            }
            else
            {
                tmpAllData = (List<RilData>) rilDataExtrapolator.RetrieveExtrapolation();
            }

            tmpAllData = tmpAllData.OrderBy(r => r.T).Reverse().ToList();

            step = timelapseDuration / tmpAllData.Count ;
            rilDataExtrapolator.InitExtrapolation(tmpAllData);
            return tmpAllData;
        }

        public void InitDrawing()
        {
            allData = GetAllData();

            foreach (RilData currentRilData in allData)
            {
                GameObject batVisual = Instantiate(batRessource);
                batVisual.tag = "bat:tmp";
                batVisual.SetActive(false);

                if (!batVisual)
                    break;

                Vector3 currentPosition =
                    new Vector3(currentRilData.X, currentRilData.Y, (float) VisualPlanner.Layers.Ril);
                batVisual.transform.position = currentPosition;
                batVisual.transform.localScale = new Vector3(2 + currentRilData.NOMBRE_LOG * 30,
                    2 + currentRilData.NOMBRE_LOG * 30);

                remainingBatDataVisuals.Push(new RilDataVisual(currentRilData, batVisual));
            }
        }

        void Update()
        {
            UpdateFrame();
        }

        void UpdateFrame()
        {
            progressBar.transform.localScale = new Vector3((Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration * 1920, 10);
            //UpdateControlledFrameRate();
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
            logger.Log("Hatched : " + hatched);
            myTime += step;
        }

        void UpdateRealtime()
        {

            int hatched = rilEventHatcher
                .HatchEvents(remainingBatDataVisuals, (Time.realtimeSinceStartup - currentIterationStartTimestamp) / timelapseDuration).Count;
                if (hatched != 0) logger.Log("Hatched : " + hatched);
        }
    }
}