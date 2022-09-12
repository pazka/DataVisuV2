using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;

namespace Tools
{
    public class Logger : MonoBehaviour
    {
        public int LineLimit = 0;
        public TextMeshPro DebugText;
        static string LogPath;
        public int lastIndex = 0;
        public FileStream fileDump;

        public Queue<string> logLines = new Queue<string>();
        public Queue<string> logLinesToDump = new Queue<string>();
        private int lineCounter = 0;
        private int position = 0;

        public FileStream GetFile()
        {
            if (fileDump == null)
            {
                LogPath = Application.persistentDataPath + "/../Logs." + GetTimeString()+".txt";
            }

            return fileDump;
        }

        public string GetTimeString()
        {
            var t = DateTime.Now;
            return t.Month + "_" + t.Day + "." + t.Hour + "." + t.Minute + "." + t.Second + "_" + t.Millisecond;
        }

        public void Start()
        {
            lineCounter = 0;
            LogPath = Application.persistentDataPath + "/../Logs." + GetTimeString()+".txt";
        }

        public void Log(string str)
        {
            logLinesToDump.Enqueue(GetTimeString() + " : " + str);

            if (logLines.Count > LineLimit)
            {
                logLines.Dequeue();
            }

            Console.WriteLine(str);
            logLines.Enqueue(str);
            lineCounter++;
        }

        public void Error(string str)
        {
            Log("ERROR : " + str);
        }

        public void Reset()
        {
            this.logLines.Clear();
            lineCounter = 0;
        }

        public void Update()
        {
            if (lastIndex != lineCounter)
            {
                File.WriteAllText(LogPath,String.Join("\n",logLinesToDump));
                logLinesToDump.Clear();
                
                DebugText.SetText(String.Join("\n", logLines));
                
                lastIndex = lineCounter;
            }
            
        }
    }
}