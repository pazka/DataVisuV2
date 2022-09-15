using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour
{
    private List<float[]> vals = new List<float[]>();
    private List<GameObject> objs = new List<GameObject>();
    [SerializeField] private GameObject vis;
    [SerializeField] private float centerX = 0.5f;
    [SerializeField] private float centerY = 0.5f;
    [SerializeField] private float centerZ = 0.5f;

    private GameObject centerVis;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                vals.Add(new[] {Random.value, Random.value});
            }
        }

        for (int i = 0; i < 100; i++)
        {
            objs.Add(Instantiate(vis));
        }

        centerVis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    }

    // Update is called once per frame
    void Update()
    {
        float[] centerPosX = new float[] {this.centerX, centerZ};
        float[] centerPosY = new float[] {this.centerY, centerZ};
        for (int i = 0; i < 100; i++)
        {
            objs[i].transform.position = new Vector3(
                FlattenCurve.GetFlattenedOneDimensionPoint(vals[i][0], centerPosX) * 1920,
                FlattenCurve.GetFlattenedOneDimensionPoint(vals[i][1], centerPosY) * 1080,
                0
            );
        }

        centerVis.transform.position = new Vector3(
            centerX * 1920,
            centerY * 1080,
            centerZ * 100
        );
    }
}