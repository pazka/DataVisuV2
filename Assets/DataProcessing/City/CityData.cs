using UnityEngine;

namespace DataProcessing.City
{
    public class CityData : Data
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        public CityData(float x, float y) : base(x, y)
        {
            X = x;
            Y = y;
        }

        override public void SetX(float x)
        {
            X = x;
        }

        override public void SetY(float y)
        {
            Y = y;
        }

        public override Vector3 GetPosition()
        {
            return new Vector3(RawX, RawY);
        }
    }
}