using System.Collections.Generic;
using System.Linq;
using DataProcessing;
using DataProcessing.Ril;
using UnityEngine;

namespace Visuals.Ril
{
    
    public class RilDrawer : MonoBehaviour
    {
        RilDataConverter rilDataConverter;
        private RilDataExtrapolator rilDataExtrapolator;
        [SerializeField] private float timelapseDuration = 60;
        private List<RilData> allData;
        private Stack<RilDataVisual> allBatDataVisuals;
        private GameObject currentBatVisual;
        private RilEventHatcher rilEventHatcher = RilEventHatcher.Instance;
        private int totalEventHatched;
        private bool firstTime = true;
        
        public GameObject batRessource;
        public GameObject progressBar;
        
        void Start()
        {
            //Prepare entities        
            rilDataConverter = (RilDataConverter)FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes.RIL);
            rilDataConverter.Init(Screen.width, Screen.height);
            
            rilDataExtrapolator = (RilDataExtrapolator)FactoryDataExtrapolator.GetInstance(FactoryDataExtrapolator.AvailableDataExtrapolatorTypes.RIL);
        }

        private List<RilData> GetAllData()
        {
            List<RilData> tmpAllData;
            if(firstTime)
            {
                tmpAllData = (List<RilData>) rilDataConverter.GetAllData();
                firstTime = false;
            }
            else
            {
                tmpAllData = (List<RilData>) rilDataExtrapolator.RetreiveExtrapolation();
            }
            
            tmpAllData = tmpAllData.OrderBy(r => r.T).Reverse().ToList();

            rilDataExtrapolator.InitExtrapolation(tmpAllData);
            return tmpAllData;
        }

        public void InitDrawing()
        {
            allData = GetAllData();
            
            totalEventHatched = 0;
            allBatDataVisuals = new Stack<RilDataVisual>();
            
            foreach (RilData currentRilData in allData)
            {
                GameObject batVisual = Instantiate(batRessource);
                batVisual.SetActive(false);
                
                if (!batVisual)
                    break;

                Vector3 currentPosition = new Vector3(currentRilData.X, currentRilData.Y, (float)VisualPlanner.Layers.Ril);
                batVisual.transform.position = currentPosition;
                batVisual.transform.localScale = new Vector3(2 + currentRilData.NOMBRE_LOG *30 ,2 + currentRilData.NOMBRE_LOG*30);
                
                allBatDataVisuals.Push(new RilDataVisual(currentRilData,batVisual));
            }
        }

        void Update()
        {
            //UpdateControlledFrameRate();
            UpdateRealtime();
        }
        
        private float myTime = 0f;
        private float step = 0.0005f;
        
        void UpdateControlledFrameRate()
        {
            myTime += step;
            rilEventHatcher.HatchEvents(allBatDataVisuals, myTime);
        }
        
        void UpdateRealtime()
        {
            progressBar.transform.localScale = new Vector3(Time.realtimeSinceStartup / timelapseDuration * 1920, 10);
            totalEventHatched += rilEventHatcher.HatchEvents(allBatDataVisuals, Time.realtimeSinceStartup / timelapseDuration).Count;

            if (totalEventHatched == allData.Count)
            {
                InitDrawing();
            }
        }
    }
}