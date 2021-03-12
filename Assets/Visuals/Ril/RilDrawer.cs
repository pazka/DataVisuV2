using System.Collections.Generic;
using System.Linq;
using DataProcessing.Ril;
using UnityEngine;

namespace Visuals.Ril
{
    
    public class RilDrawer : MonoBehaviour
    {
        RilDataConverter rilDataConverter;
        [SerializeField] private float timelapseDuration = 60;
        private List<RilData> allData;
        private Stack<RilDataVisual> allBatDataVisuals = new Stack<RilDataVisual>();
        private GameObject currentBatVisual;
        private RilEventHatcher rilEventHatcher = RilEventHatcher.Instance;
        
        public GameObject batRessource;
        public GameObject progressBar;
        
        void Start()
        {
            //Prepare entities        
            rilDataConverter = (RilDataConverter)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.RIL);
            rilDataConverter.Init(Screen.width, Screen.height);
        }

        public void FillWithData()
        {
            allData = (List<RilData>) rilDataConverter.GetAllData();
            allData = allData.OrderBy(r => r.T).Reverse().ToList();
                
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
            rilEventHatcher.HatchEvents(allBatDataVisuals, myTime);
            myTime += step;
        }
        
        void UpdateRealtime()
        {
            progressBar.transform.localScale = new Vector3(Time.realtimeSinceStartup / timelapseDuration * 1920, 10);
            rilEventHatcher.HatchEvents(allBatDataVisuals, Time.realtimeSinceStartup / timelapseDuration);
        }
    }
}