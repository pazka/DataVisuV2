using System.ComponentModel;
using System.IO;
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
            int nbDataBeforeRestart
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
        }
    }

    public static class Configuration
    {
        static string ConfigPath = Application.dataPath + "/StreamingAssets/DataVisuConfig.json";
        public static string ConfigContent;

        public static JsonConfiguration GetConfig()
        {
            ConfigContent = File.ReadAllText(ConfigPath);
            var config = JsonUtility.FromJson<JsonConfiguration>(ConfigContent);
            return config;
        }
    }
}