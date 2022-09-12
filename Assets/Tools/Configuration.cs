using System.ComponentModel;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public class JsonConfiguration
    {
        public int inPort;
        public string outIp;
        public int outPort;
        public int offsetX;
        public int offsetY;
        public float scaleX;
        public float scaleY;
        public float timelapseDuration;
        public float extrapolationRate;
        public float disappearingRate;
        public string comPort;
        public float[] densityGradiant;
        public int nbDataBeforeRestart;
        public bool debugVisual;
        public bool cityVisual;
        public bool densityVisual;
        public bool rilVisual;
        public bool isDev;

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
        static string ConfigPath = Application.dataPath + "/StreamingAssets/DataVisuConfig.json";
        public static string ConfigContent;

        public static JsonConfiguration GetConfig()
        {
            if (ConfigContent == null)
            {
                ConfigContent = File.ReadAllText(ConfigPath);
            }
            var config = JsonUtility.FromJson<JsonConfiguration>(ConfigContent);
            // I would love to have something like this:
#if DEVELOPMENT_BUILD
            config.isDev = true;
#endif

            return config;
        }
    }
}