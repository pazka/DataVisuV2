using Tools;
using UnityEngine;
using Visuals.Sirene;
using Logger = Tools.Logger;

namespace Visuals
{
    public class VisualPlanner : MonoBehaviour
    {
        [SerializeField]
        public enum Layers
        {
            City = 3,
            Sirene = 4,
            Density = 5,
            Hidden = -100
        }

        [SerializeField] private Logger logger;

        // Start is called before the first frame update
        [SerializeField] private bool activateCityDrawer;
        [SerializeField] private bool activateDensityDrawer;
        [SerializeField] private bool activateSireneDrawer;

        private void Start()
        {
            //logger.Log(KeyBindings.GetBindingStrings());
            var config = Configuration.GetConfig();

            if (!config.isDev)
            {
                activateCityDrawer = config.cityVisual;
                activateDensityDrawer = config.densityVisual;
                activateSireneDrawer = config.sireneVisual;
            }

            var cityDrawer = GameObject.Find("CityDrawer").GetComponent<CityDrawer>();
            if (activateCityDrawer) cityDrawer.SetActive(true);

            var densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();
            if (activateDensityDrawer) densityDrawer.SetActive(true);

            var SireneDrawer = GameObject.Find("SireneDrawer").GetComponent<SireneDrawer>();
            if (activateSireneDrawer) SireneDrawer.SetActive(true);
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
            

            if (Input.GetKeyDown(KeyBindings.PauseSireneDrawing))
            {
                activateSireneDrawer = !activateSireneDrawer;
                var SireneDrawer = GameObject.Find("SireneDrawer").GetComponent<SireneDrawer>();
                SireneDrawer.SetActive(activateSireneDrawer);
            }

            if (Input.GetKeyDown(KeyBindings.PauseSireneDrawing))
            {
                activateSireneDrawer = !activateSireneDrawer;
                var SireneDrawer = GameObject.Find("SireneDrawer").GetComponent<SireneDrawer>();
                SireneDrawer.SetActive(activateSireneDrawer);
            }
        }
    }
}