using System;
using UnityEngine;
using System.Collections;
using System.IO.Ports;
using Tools;
using Logger = Tools.Logger;

namespace Tools
{
    public class SerialReader : MonoBehaviour
    {
        private SerialPort stream;
        int echoTime = 0;

        public Tools.Logger logger;
        JsonConfiguration config;

        SerialPort TryOpenSerialPort()
        {
            config = Configuration.GetConfig();
            var stream = new SerialPort(config.comPort,
                9600); //Set the port (com4) and the baud rate (9600, is standard on most devices)
            stream.ReadTimeout = 100;
            stream.Open(); //Open the Serial Stream.

            return stream;
        }
        
        void Start()
        {
            stream = TryOpenSerialPort();
        }

        // Update is called once per frame
        void Update()
        {
            if (!stream.IsOpen)
            {
                stream = TryOpenSerialPort();
                echoTime = -1;
                return;
            }

            string reading = stream.ReadTo("\n"); //Read the information
            Console.WriteLine(reading);
            int.TryParse(reading, out echoTime);
            if (echoTime < 1000)
            {
                logger.Log("CLOSE OBJECT !! ");
            }
        }

        void OnGUI()
        {
            string newString = "sensor : " + echoTime;
            GUI.Label(new Rect(Screen.width - 100, 200, 300, 100), newString); //Display new values
            // Though, it seems that it outputs the value in percentage O-o I don't know why.
        }
    }
}