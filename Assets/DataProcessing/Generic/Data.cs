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
    public float X { get; private set; }
    public float Y { get; private set; }

    public Data(float x, float y)
    {
        this.RawX = x;
        this.RawY = y;
        this.X = x;
        this.Y = y;
    }

    public virtual void SetX(float x)
    {
        this.X = x;
    }

    public virtual void SetY(float y)
    {
        this.Y = y;
    }

    public abstract Vector3 GetPosition();
}
