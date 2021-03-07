using DataProcessing.Ril;
using UnityEngine;

namespace Visuals.Ril
{
    public class RilDataVisual
    {
        public RilData Data { get; private set; }
        public GameObject Visual { get; private set; }

        public RilDataVisual(RilData data, GameObject visual)
        {
            this.Data = data;
            this.Visual = visual;
        }
    }
}