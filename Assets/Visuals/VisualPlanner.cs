using Assets.Visuals;
using Tools;
using UnityEngine;
using Visuals;
using Visuals.Ril;
using Logger = Tools.Logger;

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

        [SerializeField]
        private Logger logger;
        
        // Start is called before the first frame update
        [SerializeField]
        bool activateCityDrawer;
        [SerializeField]
        bool activateDensityDrawer;
        [SerializeField]
        bool activateRilDrawer;

        void Start()
        {
            //logger.Log(KeyBindings.GetBindingStrings());
            var config = Configuration.GetConfig();

            if (!config.isDev)
            {
                activateCityDrawer = config.cityVisual;
                activateDensityDrawer = config.densityVisual;
                activateRilDrawer = config.rilVisual;
            }
            
            
            CityDrawer cityDrawer = GameObject.Find("CityDrawer").GetComponent<CityDrawer>();
            if (activateCityDrawer){
                cityDrawer.SetActive(true);
            }  
            
            DensityDrawer densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();
            if (activateDensityDrawer){
                densityDrawer.SetActive(true);
            }
            
            RilDrawer rilDrawer = GameObject.Find("RilDrawer").GetComponent<RilDrawer>();
            if (activateRilDrawer){
                rilDrawer.SetActive(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyBindings.ToggleCityLine))
            {
                activateCityDrawer = !activateCityDrawer;
                CityDrawer cityDrawer = GameObject.Find("CityDrawer").GetComponent<CityDrawer>();
                cityDrawer.SetActive(activateCityDrawer);
            }

            if (Input.GetKeyDown(KeyBindings.ToggleDensity))
            {
                activateDensityDrawer = !activateDensityDrawer;
                DensityDrawer densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();
                densityDrawer.SetActive(activateDensityDrawer);
            }

            if (Input.GetKeyDown(KeyBindings.ToggleRilDrawing))
            {
                activateRilDrawer = !activateRilDrawer;
                RilDrawer rilDrawer = GameObject.Find("RilDrawer").GetComponent<RilDrawer>();
                rilDrawer.SetActive(activateRilDrawer);
            }
        }
    }
}
