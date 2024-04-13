using DataProcessing.Sirene;
using UnityEngine;

namespace Visuals.Sirene
{
    public class SireneDataVisual
    {
        public SireneData Data { get; private set; }
        public GameObject Visual { get; private set; }

        public SireneDataVisual(SireneData data, GameObject visual)
        {
            this.Data = data;
            this.Visual = visual;
        }
    }
}