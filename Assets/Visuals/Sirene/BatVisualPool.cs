using Tools;
using UnityEngine;

namespace Visuals.Sirene
{
    public class BatVisualPool : ObjectPool<GameObject>
    {
        public GameObject batRessource;

        protected override GameObject CreateOneObject()
        {
            var obj = Instantiate(batRessource);
            obj.SetActive(false);
            
            return obj;
        }

        protected override void DeactivateOneObject(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.localPosition = new Vector3(10, 10, (float) VisualPlanner.Layers.Hidden);
        }

        protected override void RemoveOneObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
}