using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tools
{
    public class Logger : MonoBehaviour
    {
        public int LineLimit = 0;
        public TextMeshPro DebugText;
        
        private int lineCounter = 0;
        private String debugLines;
        
        public void Start()
        {
            lineCounter = 0;
            debugLines = "";
        }
        
        public void Log(string str)
        {
            debugLines = str + '\n' +  debugLines;
            lineCounter++;
        }

        public void Error(string str)
        {
            Log("ERROR : " + str);
        }

        public void Reset()
        {
            this.debugLines = "";
        }
    
        public void Update()
        {
            Queue<String> res = new Queue<string>();
            
            foreach (var line in debugLines)
            {
                DebugText.SetText(debugLines);
            }
        }
    }
}