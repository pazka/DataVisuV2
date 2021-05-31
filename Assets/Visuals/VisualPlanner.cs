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
            DensityDrawer densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();
            RilDrawer rilDrawer = GameObject.Find("RilDrawer").GetComponent<RilDrawer>();
        
            if (activateCityDrawer){
                cityDrawer.InitDrawing();
            }

            if (activateDensityDrawer){
                densityDrawer.InitDrawing();
            }

            if (activateRilDrawer){
                rilDrawer.SetActive();
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
