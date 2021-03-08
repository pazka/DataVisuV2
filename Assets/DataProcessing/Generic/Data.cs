﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataProcessing.Generic
{
    public interface IData
    {
        void SetX(float x);
        void SetY(float y);
        float[] GetPosition();
    }

    public abstract class Data : IData
    {
        public string Raw;
        public float RawX { get; private set; }
        public float RawY { get; private set; }
        public float X { get; protected set; }
        public float Y { get; protected set; }

        public Data(float x, float y)
        {
            this.RawX = x;
            this.RawY = y;
            this.X = x;
            this.Y = y;
        }
        public Data(string raw,float x, float y) : this(x,y)
        {
            this.Raw = raw;
        }

        public virtual void SetX(float x)
        {
            this.X = x;
        }

        public virtual void SetY(float y)
        {
            this.Y = y;
        }

        public virtual float[] GetPosition()
        {
            return new float[]{RawX, RawY};
        }
    }
}