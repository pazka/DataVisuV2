using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IData
{
}

public abstract class Data : IData
{
    public float RawX { get; }
    public float RawY { get; }

    public Data(float x, float y)
    {
        this.RawX = x;
        this.RawY = y;
    }
    
    public abstract Vector3 GetPosition();
}
