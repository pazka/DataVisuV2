using System;
using System.Collections.Generic;
using System.IO;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.Density
{
    public class DensityDataReader : IDataReader
    {
        readonly string FilePath = Application.dataPath + "/StreamingAssets/SeineSaintDenis/Density/carreaux_200m_SSD_wgs84.json";
        int Cursor;
        List<RootJsonObject> AllDataRead;
        public bool EndOfStream;


        [Serializable]
        private class JsonArrayWrapper
        {
            public List<RootJsonObject> array;
        }

        [Serializable]
        private class RootJsonObject
        {
            public float x1, y1, x2, y2, x3, y3, x4, y4;
            public float individuals;
            public float households;
            public string raw;
        }

        public DensityDataReader()
        {
            Init();
        }

        public void Init()
        {
            Cursor = 0;
            EndOfStream = false;

            using (StreamReader r = new StreamReader(this.FilePath))
            {
                string json = "{\"array\":" + r.ReadToEnd() + "}";
                AllDataRead = JsonUtility.FromJson<JsonArrayWrapper>(json).array;
            }
        }

        public void Clean()
        {
            AllDataRead = new List<RootJsonObject>();
            EndOfStream = false;
        }

        public IData GetData()
        {
            RootJsonObject json = AllDataRead[Cursor];

            return new DensityData(
                json.x1,
                json.y1,
                json.x2,
                json.y2,
                json.x3,
                json.y3,
                json.x4,
                json.y4,
                json.individuals,
                json.households,
                json.raw);
        }

        public void GoToNextData()
        {
            if (EndOfStream)
                return;

            Cursor++;

            if (Cursor == AllDataRead.Count)
            {
                EndOfStream = true;
            }
        }
    }
}