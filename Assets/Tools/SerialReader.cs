using System;
using System.IO.Ports;
using UnityEngine;

namespace Tools
{
    public class SerialReader : MonoBehaviour
    {
        public Logger logger;
        private JsonConfiguration config;
        private int echoTime;
        private SerialPort stream;

        private void Start()
        {
            stream = TryOpenSerialPort();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!stream.IsOpen)
            {
                stream = TryOpenSerialPort();
                echoTime = -1;
                return;
            }

            var reading = stream.ReadTo("\n"); //Read the information
            Console.WriteLine(reading);
            int.TryParse(reading, out echoTime);
            if (echoTime < 1000) logger.Log("CLOSE OBJECT !! ");
        }

        private void OnGUI()
        {
            var newString = "sensor : " + echoTime;
            GUI.Label(new Rect(Screen.width - 100, 200, 300, 100), newString); //Display new values
            // Though, it seems that it outputs the value in percentage O-o I don't know why.
        }

        private SerialPort TryOpenSerialPort()
        {
            config = Configuration.GetConfig();
            var stream = new SerialPort(config.comPort,
                9600); //Set the port (com4) and the baud rate (9600, is standard on most devices)
            stream.ReadTimeout = 100;
            stream.Open(); //Open the Serial Stream.

            return stream;
        }
    }
}