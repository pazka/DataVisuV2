using System.Collections.Generic;
using DataProcessing.Ril;
using Packages.Rider.Editor.UnitTesting;
using UnityEngine;

namespace Visuals
{
    public class RilDrawer : MonoBehaviour
    {
        RilDataManager _rilDataManager;
        private int currentRilIndex = 0;
        private List<RilData> allData;
        private Stack<GameObject> allBatVisuals = new Stack<GameObject>();
        private GameObject currentBatVisual;

        public GameObject batRessource;
        
        void Start()
        {
            //Prepare entities        
            _rilDataManager = (RilDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.RIL);
            _rilDataManager.Init(Screen.width, Screen.height);
        }

        public void FillWithData()
        {
            allData = (List<RilData>) _rilDataManager.GetAllData();
            
            foreach (RilData currentRil in allData)
            {
                GameObject batVisual = 
                    GameObject.Instantiate(batRessource) as GameObject;
            
                if (!batVisual)
                    break;

                Vector3 currentPosition = new Vector3(currentRil.X, currentRil.Y, 0);
                batVisual.transform.position = currentPosition;
                
                allBatVisuals.Push(batVisual);
            }
        }
        
        void Update()
        {
        }
    }
}