using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.Density
{
    public class DensityData : Generic.Data
    {
        public float RawX1 { get; }
        public float RawY1 { get; }
        public float RawX2 { get; }
        public float RawY2 { get; }
        public float RawX3 { get; }
        public float RawY3 { get; }
        public float RawX4 { get; }
        public float RawY4 { get; }
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }
        public float X3 { get; set; }
        public float Y3 { get; set; }
        public float X4 { get; set; }
        public float Y4 { get; set; }
        public float W { get; set; }
        public float H { get; set; }
        public float Individuals { get; set; }
        public float Households { get; set; }
        public string Raw { get; set; }

        //X == 0 left side
        //Y == 0 is top
        public DensityData(
            float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4,
            float individuals       ,
            float households       ,
            string raw      
        ) : base(x1, y1)
        {
            X1          = x1;
            Y1          = y1;
            X2          = x2;
            Y2          = y2;
            X3          = x3;
            Y3          = y3;
            X4          = x4;
            Y4          = y4;
            RawX1       = X1;
            RawY1       = Y1;
            RawX2       = X2;
            RawY2       = Y2;
            RawX3       = X3;
            RawY3       = Y3;
            RawX4       = X4;
            RawY4       = Y4;
            Individuals         = individuals;
            Households         = households;
            Raw         = raw;
        }

        public DensityData(DensityData densityData) : this(
            densityData.X1,
            densityData.Y1,
            densityData.X2,
            densityData.Y2,
            densityData.X3,
            densityData.Y3,
            densityData.X4,
            densityData.Y4,
            densityData.Individuals     ,
            densityData.Households     ,
            densityData.Raw
        )
        {

        }

        public override void SetX(float x)
        {
            base.SetX(x);
            this.X1 = x;
        }

        public override void SetY(float y)
        {
            base.SetY(y);
            this.Y1 = y;
        }

        public void SetX1(float x)
        {
            this.SetX(x);
        }

        public void SetY1(float y)
        {
            this.SetY(y);
        }


        public void SetX2(float x)
        {
            this.X2 = x;
        }

        public void SetY2(float y)
        {
            this.Y2 = y;
        }
        public void SetX3(float x)
        {
            this.X3 = x;
        }

        public void SetY3(float y)
        {
            this.Y3 = y;
        }
        public void SetX4(float x)
        {
            this.X4 = x;
        }

        public void SetY4(float y)
        {
            this.Y4 = y;
        }

    }
}

