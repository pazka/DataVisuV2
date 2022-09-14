using Tools;
using UnityEngine;
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
            Density = 5,
            Hidden = -100
        }

        [SerializeField] private Logger logger;

        // Start is called before the first frame update
        [SerializeField] private bool activateCityDrawer;
        [SerializeField] private bool activateDensityDrawer;
        [SerializeField] private bool activateRilDrawer;

        private void Start()
        {
            //logger.Log(KeyBindings.GetBindingStrings());
            var config = Configuration.GetConfig();

            if (!config.isDev)
            {
                activateCityDrawer = config.cityVisual;
                activateDensityDrawer = config.densityVisual;
                activateRilDrawer = config.rilVisual;
            }


            var cityDrawer = GameObject.Find("CityDrawer").GetComponent<CityDrawer>();
            if (activateCityDrawer) cityDrawer.SetActive(true);

            var densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();
            if (activateDensityDrawer) densityDrawer.SetActive(true);

            var rilDrawer = GameObject.Find("RilDrawer").GetComponent<RilDrawer>();
            if (activateRilDrawer) rilDrawer.SetActive(true);
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyBindings.ToggleCityLine))
            {
                activateCityDrawer = !activateCityDrawer;
                var cityDrawer = GameObject.Find("CityDrawer").GetComponent<CityDrawer>();
                cityDrawer.SetActive(activateCityDrawer);
            }

            if (Input.GetKeyDown(KeyBindings.ToggleDensity))
            {
                activateDensityDrawer = !activateDensityDrawer;
                var densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();
                densityDrawer.SetActive(activateDensityDrawer);
            }

            if (Input.GetKeyDown(KeyBindings.ToggleRilDrawing))
            {
                activateRilDrawer = !activateRilDrawer;
                var rilDrawer = GameObject.Find("RilDrawer").GetComponent<RilDrawer>();
                rilDrawer.SetActive(activateRilDrawer);
            }
        }
    }
}