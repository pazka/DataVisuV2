using System;
using System.IO;
using Tools;
using UnityEngine;

namespace SoundProcessing
{
    class JsonConfigWrapper
    {
        public int inPort;
        public string outIp;
        public int outPort;

        public JsonConfigWrapper(int outPort, string outIp, int inPort)
        {
            this.outPort = outPort;
            this.outIp = outIp;
            this.inPort = inPort;
        }
    }
    
    public class PureDataConnector : MonoBehaviour
    {
        public Tools.Logger logger;
        private OSC osc;
        private string configFile;
        private JsonConfigWrapper config;
        private void Start()
        {
            configFile = Application.dataPath + "/StreamingAssets/PureDataConfig.json";
            OpenConnection();
        }

        private void Update()
        {
            
            if (Input.GetKeyDown(KeyBindings.TogglePureData))
            {
                if(!IsOpen())
                {
                    logger.Log($"Trying to connect to Pure Data : {config.outIp}:{config.outPort} ");
                    OpenConnection();
                }
                else
                {
                    logger.Log($"Closing Pure Data connection ");
                    osc.Close();
                }
            }
            
            if (!IsOpen())
                return;
            
            osc.Update();
        }

        public void OnDestroy()
        {
            if (!IsOpen())
                return;
            
            osc.OnDestroy();
        }
        
        void OpenConnection()
        {
            string configText = File.ReadAllText(configFile);
            config = JsonUtility.FromJson<JsonConfigWrapper>(configText);

            try
            {
                osc = new OSC(config.inPort,config.outIp,config.outPort);
                logger.Log("Connected to Pure Data client !");
                
                OscMessage oscMess = new OscMessage();
                oscMess.address = "/Test";
                oscMess.values.Add("Hello");
                oscMess.values.Add(DateTime.Now.Millisecond);
                Send(oscMess);
            }
            catch (Exception e)
            {
                osc = null;
                logger.Log(e.Message);
                throw;
            }
        }
        public bool IsOpen() {
            return osc != null && osc.IsOpen();
        }

        public void Send(OscMessage message)
        {
            if(!IsOpen())
                logger.Log("Error when trying to send a message, the connection is not open")
                    ;
            osc.Send(message);
        }

        public void SendOscMessage(string address, dynamic value)
        {
            Send(new OscMessage(address, value));
        }
    }
}
