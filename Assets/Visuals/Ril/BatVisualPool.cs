using Tools;
using UnityEngine;

namespace Visuals.Ril
{
    public class BatVisualPool : ObjectPool<GameObject>
    {
        public GameObject batRessource;

        protected override GameObject CreateOneObject()
        {
            return Instantiate(batRessource);
        }

        protected override void DeactivateOneObject(GameObject obj)
        {
            obj.SetActive(false);

            Vector3 currentPosition = new Vector3(-100, -100, (float) VisualPlanner.Layers.Hidden);

            //currentPosition = transform.rotation * Vector3.Scale(currentPosition, transform.localScale);
            obj.transform.localPosition = currentPosition;
        }

        protected override void RemoveOneObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
}