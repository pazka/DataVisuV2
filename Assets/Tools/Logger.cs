using System;
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
            debugLines = str + '\n' + debugLines ;
        }

        public void Error(string str)
        {
            debugLines += "ERROR : " + str + '\n';
        }
    
        public void Update()
        {
            DebugText.SetText(debugLines);
        }
    }
}