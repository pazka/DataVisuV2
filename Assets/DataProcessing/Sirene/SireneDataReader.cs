﻿using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using DataProcessing.Generic;

namespace DataProcessing.Sirene
{
    public class SireneDataReader : IDataReader
    {
        private readonly string filePath = Application.dataPath +
                                              "/StreamingAssets/SeineSaintDenis/Sirene/etablissements_geoloc_cleaned_with_count_in_ssd.csv";
        private readonly string ffilePath = Application.dataPath +
                                                "/StreamingAssets/SeineSaintDenis/Sirene/small_etablissements_geoloc_cleaned_with_count_in_ssd.csv";
        private readonly string fffilePath = Application.dataPath +
                                             "/StreamingAssets/SeineSaintDenis/Sirene/verysmall_etablissements_geoloc_cleaned_with_count_in_ssd.csv";

        private int cursor;
        private List<SireneData> allDataRead;
        public bool EndOfStream;


        public SireneDataReader()
        {
            Init();
        }

        public void Init()
        {
            cursor = 0;
            EndOfStream = false;

            using (StreamReader r = new StreamReader(this.filePath))
            {
                //HEADER : siren,dateCreationEtablissement,denominationUniteLegale,isOnePerson,X,Y
                string line;
                allDataRead = new List<SireneData>();
                r.ReadLine(); // Skip the first line

                while ((line = r.ReadLine()) != null && line != "")
                {
                    var data = line.Split(',');

                    if (data.Length < 5)
                        continue;

                    var y = float.Parse(data[4], CultureInfo.InvariantCulture);
                    var x = float.Parse(data[5], CultureInfo.InvariantCulture);
                    //parse YYYY-MM-DD to DateTime
                    var dateCreation = DateTime.ParseExact(data[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    var entityCount = int.Parse(data[6]);

                    var sireneData = new SireneData(line, x, y, data[0], dateCreation, data[2], data[3] == "True",
                        entityCount);

                    allDataRead.Add(sireneData);
                }
            }
        }

        public void Clean()
        {
            allDataRead = new List<SireneData>();
            EndOfStream = false;
        }

        public IData GetData()
        {
            SireneData data = allDataRead[cursor];

            return data;
        }

        public void GoToNextData()
        {
            if (EndOfStream)
                return;

            cursor++;

            if (cursor == allDataRead.Count)
            {
                EndOfStream = true;
            }
        }
    }
}