using Assets.Visuals;
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

        void Start()
        {
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
            if (Input.GetKeyDown(KeyCode.F1))
            {
                activateCityDrawer = !activateCityDrawer;
                CityDrawer cityDrawer = GameObject.Find("CityDrawer").GetComponent<CityDrawer>();
                cityDrawer.SetActive(activateCityDrawer);
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                activateDensityDrawer = !activateDensityDrawer;
                DensityDrawer densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();
                densityDrawer.SetActive(activateDensityDrawer);
            }

            if (Input.GetKeyDown(KeyCode.F3))
            {
                activateRilDrawer = !activateRilDrawer;
                RilDrawer rilDrawer = GameObject.Find("RilDrawer").GetComponent<RilDrawer>();
                rilDrawer.SetActive(activateRilDrawer);
            }
        }
    }
}
