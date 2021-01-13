using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualPlanner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CityDrawer cityDrawer = GameObject.Find("CityDrawer").GetComponent<CityDrawer>();
        DensityDrawer densityDrawer = GameObject.Find("DensityDrawer").GetComponent<DensityDrawer>();

        cityDrawer.FillWithData();
        densityDrawer.FillWithData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
