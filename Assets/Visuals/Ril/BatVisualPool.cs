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
        }

        protected override void RemoveOneObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
}