using Assets.Visuals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Visuals.Ril;

public class VisualPlanner : MonoBehaviour
{
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


        if(activateCityDrawer)
            cityDrawer.FillWithData();

        if (activateDensityDrawer)
            densityDrawer.FillWithData();

        if (activateRilDrawer)
            rilDrawer.FillWithData();

        if (activateDensityDrawer)
            test.DrawTest();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
