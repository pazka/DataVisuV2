using System;
using System.Collections.Generic;
using System.IO;
using DataProcessing.Generic;
using UnityEngine;

namespace DataProcessing.Density
{
    public class DensityDataReader : IDataReader
    {
        readonly string FilePath = "Assets/DataAsset/Strasbourg/Density/res_density.json";
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
            public float area;
            public float pop;
            public float rev;
            public float m25ans;
            public float p65ans;
            public float men_basr;
            public float men;
            public float men_coll;
            public float men_prop;
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
                json.area,
                json.pop,
                json.rev,
                json.m25ans,
                json.p65ans,
                json.men_basr,
                json.men,
                json.men_coll,
                json.men_prop,
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