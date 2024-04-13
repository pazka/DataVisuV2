using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using DataProcessing.Generic;

namespace DataProcessing.Sirene
{
    public class SireneDataReader : IDataReader
    {
        readonly string FilePath = Application.dataPath +
                                   "/StreamingAssets/SeineSaintDenis/Sirene/etablissements_geoloc_cleaned_with_count_in_ssd.csv";

        int Cursor;
        List<SireneData> AllDataRead;
        public bool streamEnd;


        public SireneDataReader()
        {
            Init();
        }

        public void Init()
        {
            Cursor = 0;
            streamEnd = false;

            using (StreamReader r = new StreamReader(this.FilePath))
            {
                //HEADER : siren,dateCreationEtablissement,denominationUniteLegale,isOnePerson,X,Y
                string line;
                AllDataRead = new List<SireneData>();
                r.ReadLine(); // Skip the first line

                while ((line = r.ReadLine()) != null && line != "")
                {
                    var data = line.Split(',');

                    if (data.Length < 5)
                        continue;

                    var x = float.Parse(data[4]);
                    var y = float.Parse(data[5]);
                    //parse YYYY-MM-DD to DateTime
                    var dateCreation = DateTime.Parse(data[1]);
                    
                    var entityCount = int.Parse(data[6]);

                    var sireneData = new SireneData(line, x, y, data[0], dateCreation, data[2], data[3] == "True",entityCount);

                    AllDataRead.Add(sireneData);
                }
            }
        }

        public void Clean()
        {
            AllDataRead = new List<SireneData>();
            streamEnd = false;
        }

        public IData GetData()
        {
            SireneData data = AllDataRead[Cursor];

            return data.Clone();
        }

        public void GoToNextData()
        {
            if (streamEnd)
                return;

            Cursor++;

            if (Cursor == AllDataRead.Count)
            {
                streamEnd = true;
            }
        }
    }
}