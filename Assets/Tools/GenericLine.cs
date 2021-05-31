﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

namespace Tools
{
    public class GenericLine : MonoBehaviour
    {
        public LineRenderer lineRenderer;
        public float depth = 10;
        private int index = 0;
        private Queue<Vector3> pointsToAdd = new Queue<Vector3>();
        
        public void Start()
        { 
            lineRenderer.sortingOrder = 2;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = Color.white;
            lineRenderer.useWorldSpace = true;
            
            lineRenderer.startWidth = 2f;
            lineRenderer.endWidth = 2f;
        }

        public void AddPoint(float x, float y)
        {
            pointsToAdd.Enqueue(new Vector3(x,y,depth));
        }

        public void Clear()
        {
            index = 0;
            lineRenderer.SetPositions(new Vector3[]{});
        }
        
        public void Update()
        {
            if (pointsToAdd.Count > 0)
            {
                lineRenderer.positionCount = index + 1;
                lineRenderer.SetPosition(index++, pointsToAdd.Dequeue());
            }
                
        }
    }
}
