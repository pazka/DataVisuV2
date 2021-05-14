using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace Tools
{
    public class Logger : MonoBehaviour
    {
        public int LineLimit = 0;
        public TextMeshPro DebugText;
        string debugFile = "Assets/Resources/screenCityLogs.txt";
        
        private int lineCounter = 0;
        private String displayText;
        private StreamWriter fileDumper;
        
        public void Start()
        {
            fileDumper = new StreamWriter(debugFile);
            lineCounter = 0;
            displayText = "";
        }
        
        public void Log(string str)
        {
            //fileDumper.WriteLine(displayText);
            displayText = str + '\n' +  displayText;
            lineCounter++;
        }

        public void LogFile(string str)
        {
            fileDumper.WriteLine(str);
        }

        public void Error(string str)
        {
            Log("ERROR : " + str);
        }

        public void Reset()
        {
            this.displayText = "";
        }
    
        public void Update()
        {
            Queue<String> res = new Queue<string>();
            
            foreach (var line in displayText)
            {
                DebugText.SetText(displayText);
            }
        }

        public void OnDestroy()
        {
            fileDumper.Close();
        }
    }
}