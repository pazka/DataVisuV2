﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DensityData : Data
{
    public float X { get; set; }
    public float Y { get; set; }
    public float W { get; set; }
    public float H { get; set; }
    public float Area { get; set; }
    public float Pop { get; set; }
    public float Rev { get; set; }
    public float M25ans { get; set; }
    public float P65ans { get; set; }
    public float Men_basr { get; set; }
    public float Men { get; set; }
    public float Men_coll { get; set; }
    public float Men_prop { get; set; }
    public string Raw { get; set; }

    //X == 0 left side
    //Y == 0 is top
    public DensityData(
        float x,
        float y,
        float w,
        float h,
        float area      ,
        float pop       ,
        float rev       ,
        float m25ans    ,
        float p65ans    ,
        float men_basr  ,
        float men       ,
        float men_coll  ,
        float men_prop  ,
        string raw      
        ) : base(x, y)
    {
        X           = x;
        Y           = y;
        W           = w;
        H           = h;
        Area        = area;
        Pop         = pop;
        Rev         = rev;
        M25ans      = m25ans;
        P65ans      = p65ans;
        Men_basr    = men_basr;
        Men         = men;
        Men_coll    = men_coll;
        Men_prop    = men_prop;
        Raw         = raw;
    }

    public DensityData(DensityData densityData) : this(
        densityData.X       ,
        densityData.Y       ,
        densityData.W       ,
        densityData.H       ,
        densityData.Area    ,
        densityData.Pop     ,
        densityData.Rev     ,
        densityData.M25ans  ,
        densityData.P65ans  ,
        densityData.Men_basr,
        densityData.Men     ,
        densityData.Men_coll,
        densityData.Men_prop,
        densityData.Raw
        )
    {

    }

    public override void SetX(float x) 
    {
        base.SetX(x);
        this.X = x;
    }

    public override void SetY(float y)
    {
        base.SetY(Y);
        this.Y = y;
    }
    public void SetH(float h)
    {
        this.H = h;
    }
    public void SetW(float w)
    {
        this.W = w;
    }


    public override Vector3 GetPosition()
    {
        return new Vector3(RawX, RawY);
    }
}

