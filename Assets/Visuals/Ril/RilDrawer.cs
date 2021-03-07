using System.Collections.Generic;
using System.Linq;
using DataProcessing.Ril;
using UnityEngine;

namespace Visuals.Ril
{
    
    public class RilDrawer : MonoBehaviour
    {
        RilDataManager rilDataManager;
        [SerializeField] private float timelapseDuration = 60;
        private List<RilData> allData;
        private Stack<RilDataVisual> allBatDataVisuals = new Stack<RilDataVisual>();
        private GameObject currentBatVisual;
        private RilEventHatcher rilEventHatcher = RilEventHatcher.Instance;
        
        public GameObject batRessource;
        
        void Start()
        {
            //Prepare entities        
            rilDataManager = (RilDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.RIL);
            rilDataManager.Init(Screen.width, Screen.height);
        }

        public void FillWithData()
        {
            allData = (List<RilData>) rilDataManager.GetAllData();
            allData = allData.OrderBy(rd => rd.T).Reverse().ToList();
            
            foreach (RilData currentRilData in allData)
            {
                GameObject batVisual = Instantiate(batRessource);
                batVisual.SetActive(false);
                
                if (!batVisual)
                    break;

                Vector3 currentPosition = new Vector3(currentRilData.X, currentRilData.Y, 0);
                batVisual.transform.position = currentPosition;
                
                allBatDataVisuals.Push(new RilDataVisual(currentRilData,batVisual));
            }
        }
        
        void Update()
        {
            rilEventHatcher.HatchEvents(allBatDataVisuals, Time.realtimeSinceStartup / timelapseDuration);
        }
    }
}