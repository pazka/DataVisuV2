using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using DataProcessing.Generic;

namespace DataProcessing.Ril
{
    public class RilDataReader : IDataReader
    {
        readonly string FilePath = "Assets/DataAsset/Strasbourg/Ril/RIL_2018.json";
        int Cursor;
        List<IntermediateJsonObject> AllDataRead;
        public bool streamEnd;


        [Serializable]
        private class JsonArrayWrapper
        {
            public List<IntermediateJsonObject> array;
        }
        
        [Serializable]
        private class IntermediateJsonObject
        {
            public RootJsonObject features;
            public RootGeoJsonObject geometry;
        }
        
        [Serializable]
        private class RootGeoJsonObject
        {
            public string type;
            public float[] coordinates;
        }

        [Serializable]
        private class RootJsonObject
        {
            public string ACTUALITE;  
            public string ANNEE_CONS  ; 
            public string CANTON      ;     
            public string CATEGORIE   ;  
            public string COMMENTAIR  ; 
            public string COMPLEMENT  ; 
            public string COM_ANC_ID  ; 
            public string COM_CAPACI  ; 
            public string COM_DATE_C  ; 
            public string COM_NB_LOG  ; 
            public string COM_NOM     ;    
            public string COM_SOUS_C  ; 
            public string COM_STATUT  ; 
            public string DATEMAJ_EA  ; 
            public string DEPCOM      ;     
            public string DERNIER_TI  ; 
            public string ECHANTILLO  ; 
            public string ENSEIGNE    ;   
            public string GRP_ROTATI  ; 
            public string ID_EA       ;      
            public string ID_RP       ;      
            public string IRIS        ;       
            public string LIBELLE     ;    
            public string LIEN_CMT    ;   
            public string LISTE_INSE  ; 
            public string NOMBRE_IMM  ; 
            public string NOMBRE_LOG  ; 
            public string NOMBRE_NIV  ; 
            public string NUMERO      ;     
            public string NUMERO_PAR  ; 
            public string NUMERO_PER  ; 
            public string PRINCIPAL   ;  
            public string QP          ;         
            public string REPETITION  ; 
            public string RIVOLI      ;     
            public string SOUS_TYPE   ;  
            public string TYPE        ;       
            public string TYPE_LOCAL  ; 
            public string TYPE_VOIE   ;  
            public string apic_obj00  ; 
            public string apic_obj01  ; 
            public string apic_obj02  ; 
            public string apic_obj03  ; 
            public string apic_obj04  ; 
            public string apic_obj05  ; 
            public string apic_obj06  ; 
            public string apic_obj07  ; 
            public string apic_obj08  ; 
            public string apic_objec  ;
        }

        public RilDataReader()
        {
            Init();
        }

        public void Init()
        {
            Cursor = 0;
            streamEnd = false;

            using (StreamReader r = new StreamReader(this.FilePath))
            {
                string json = "{\"array\":" + r.ReadToEnd() + "}";
                AllDataRead = JsonUtility.FromJson<JsonArrayWrapper>(json).array;
            }
        }

        public void Clean()
        {
            AllDataRead = new List<IntermediateJsonObject>();
            streamEnd = false;
        }

        public IData GetData()
        {
            IntermediateJsonObject json = AllDataRead[Cursor];

            return new RilData(
                JsonUtility.ToJson(json),
                json.geometry.coordinates[0],
                json.geometry.coordinates[1],
                json.features.ACTUALITE ,
            json.features.ANNEE_CONS ,
            json.features.CANTON ,
            json.features.CATEGORIE ,
            json.features.COMMENTAIR ,
            json.features.COMPLEMENT ,
            json.features.COM_ANC_ID ,
            json.features.COM_CAPACI ,
            json.features.COM_DATE_C ,
            json.features.COM_NB_LOG ,
            json.features.COM_NOM ,
            json.features.COM_SOUS_C ,
            json.features.COM_STATUT ,
            json.features.DATEMAJ_EA ,
            json.features.DEPCOM ,
            json.features.DERNIER_TI ,
            json.features.ECHANTILLO ,
            json.features.ENSEIGNE ,
            json.features.GRP_ROTATI ,
            json.features.ID_EA ,
            json.features.ID_RP ,
            json.features.IRIS ,
            json.features.LIBELLE ,
            json.features.LIEN_CMT ,
            json.features.LISTE_INSE ,
            json.features.NOMBRE_IMM ,
            json.features.NOMBRE_LOG ,
            json.features.NOMBRE_NIV ,
            json.features.NUMERO ,
            json.features.NUMERO_PAR ,
            json.features.NUMERO_PER ,
            json.features.PRINCIPAL ,
            json.features.QP ,
            json.features.REPETITION ,
            json.features.RIVOLI ,
            json.features.SOUS_TYPE ,
            json.features.TYPE ,
            json.features.TYPE_LOCAL ,
            json.features.TYPE_VOIE ,
            json.features.apic_obj00 ,
            json.features.apic_obj01 ,
            json.features.apic_obj02 ,
            json.features.apic_obj03 ,
            json.features.apic_obj04 ,
            json.features.apic_obj05 ,
            json.features.apic_obj06 ,
            json.features.apic_obj07 ,
            json.features.apic_obj08 ,
            json.features.apic_objec );
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