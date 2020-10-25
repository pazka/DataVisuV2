using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IData
{
}

public abstract class Data : IData
{
    public float RawX { get; private set; }
    public float RawY { get; private set; }

    public Data(float x, float y)
    {
        this.RawX = x;
        this.RawY = y;
    }

    public virtual void SetX(float x)
    {
        this.RawX = x;
    }

    public virtual void SetY(float y)
    {
        this.RawY = y;
    }

    public abstract Vector3 GetPosition();
}
