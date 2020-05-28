using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityData : Data
{
    public float X { get; private set; }
    public float Y { get; private set; }

    public CityData(float x, float y) : base(x, y) {
        X = x;
        Y = y;
    }

    public void SetX(int x)
    {
        X = x;
    }

    public void SetY(int y)
    {
        Y = y;
    }

    public void SetX(float x)
    {
        X = x;
    }

    public void SetY(float y)
    {
        Y = y;
    }

    public override Vector3 GetPosition()
    {
        return new Vector3(RawX, RawY);
    }
}

