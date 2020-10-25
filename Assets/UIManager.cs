using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public bool displayPopulationDensity;
    public bool displayMap;

    // Start is called before the first frame update
    void Start()
    {
        displayPopulationDensity = false;
        displayMap = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            displayMap = !displayMap;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            displayPopulationDensity = !displayPopulationDensity;
        }
    }
}
