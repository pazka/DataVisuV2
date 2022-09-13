using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public abstract class ObjectPool<T> : MonoBehaviour where T : new()
    {
        private readonly Queue<T> availableObjects = new Queue<T>();


        public void Return(T obj)
        {
            DeactivateOneObject(obj);
            availableObjects.Enqueue(obj);
        }

        public T GetOne()
        {
            if (availableObjects.Count == 0) availableObjects.Enqueue(CreateOneObject());

            return availableObjects.Dequeue();
        }

        protected abstract T CreateOneObject();
        protected abstract void DeactivateOneObject(T obj);
        protected abstract void RemoveOneObject(T obj);
    }
}