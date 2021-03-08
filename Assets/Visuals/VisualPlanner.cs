﻿using Assets.Visuals;
using UnityEngine;
using Visuals;
using Visuals.Ril;

namespace Visuals
{
    public class VisualPlanner : MonoBehaviour
    {
        [SerializeField]
        public enum Layers
        {
            City = 3,
            Ril = 4,
            Density = 5
        }
        
        // Start is called before the first frame update
        [SerializeField]
        bool activateCityDrawer;
        [SerializeField]
        bool activateDensityDrawer;
        [SerializeField]
        bool activateRilDrawer;
        [SerializeField]
        bool activateTest;

        void Start()
        {
            CityDrawer cityDrawer = GameObject.Find("CityDrawer").GetComponent<CityDrawer>();
            DensityDrawer densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();
            RilDrawer rilDrawer = GameObject.Find("RilDrawer").GetComponent<RilDrawer>();
            Test test = GameObject.Find("Test").GetComponent<Test>();
        
            if (activateCityDrawer){
                cityDrawer.FillWithData();
            }

            if (activateDensityDrawer){
                densityDrawer.FillWithData();
            }

            if (activateRilDrawer){
                rilDrawer.FillWithData();
            }

            if (activateTest){
                test.DrawTest();
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
