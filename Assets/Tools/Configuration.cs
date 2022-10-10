using System.IO;
using UnityEngine;

namespace Tools
{
    public class JsonConfiguration
    {
        public bool cityVisual;
        public string comPort;
        public bool debugVisual;
        public float[] densityGradiant;
        public bool densityVisual;
        public float disappearingRate;
        public float extrapolationRate;
        public int inPort;
        public bool isDev;
        public int nbDataBeforeRestart;
        public int offsetX;
        public int offsetY;
        public string outIp;
        public int outPort;
        public bool rilVisual;
        public float scaleX;
        public float scaleY;
        public float timelapseDuration;
        public int targetFrameRate;

        public JsonConfiguration(
            int outPort,
            string outIp,
            int inPort,
            int offsetX,
            int offsetY,
            float scaleX,
            float scaleY,
            float timelapseDuration,
            float extrapolationRate,
            float disappearingRate,
            string comPort,
            float[] densityGradiant,
            int nbDataBeforeRestart,
            int targetFrameRate,
            bool debugVisual,
            bool cityVisual,
            bool densityVisual,
            bool rilVisual,
            bool isDev
        )
        {
            this.outPort = outPort;
            this.outIp = outIp;
            this.inPort = inPort;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            this.timelapseDuration = timelapseDuration;
            this.extrapolationRate = extrapolationRate;
            this.disappearingRate = disappearingRate;
            this.targetFrameRate = targetFrameRate;
            this.comPort = comPort;
            this.densityGradiant = densityGradiant;
            this.nbDataBeforeRestart = nbDataBeforeRestart;
            this.debugVisual = debugVisual;
            this.cityVisual = cityVisual;
            this.densityVisual = densityVisual;
            this.rilVisual = rilVisual;
            this.isDev = isDev;
        }
    }

    public static class Configuration
    {
        private static readonly string ConfigPath = Application.dataPath + "/StreamingAssets/DataVisuConfig.json";
        public static string ConfigContent;

        public static JsonConfiguration GetConfig()
        {
            if (ConfigContent == null) ConfigContent = File.ReadAllText(ConfigPath);
            var config = JsonUtility.FromJson<JsonConfiguration>(ConfigContent);
            
            if(Debug.isDebugBuild)
            {
                config.isDev = true;
            }

            return config;
        }
    }
}