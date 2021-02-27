﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataProcessing.Ril
{
    public class RilData : Data
    {
        private float X, Y;
        string Raw;
        public float T { get; private set;}
        public string ACTUALITE { get; private set;}
        public string ANNEE_CONS { get; private set;}
        public string CANTON { get; private set;}
        public string CATEGORIE { get; private set;}
        public string COMMENTAIR { get; private set;}
        public string COMPLEMENT { get; private set;}
        public string COM_ANC_ID { get; private set;}
        public string COM_CAPACI { get; private set;}
        public string COM_DATE_C { get; private set;}
        public string COM_NB_LOG { get; private set;}
        public string COM_NOM { get; private set;}
        public string COM_SOUS_C { get; private set;}
        public string COM_STATUT { get; private set;}
        public string DATEMAJ_EA { get; private set;}
        public string DEPCOM { get; private set;}
        public string DERNIER_TI { get; private set;}
        public string ECHANTILLO { get; private set;}
        public string ENSEIGNE { get; private set;}
        public string GRP_ROTATI { get; private set;}
        public string ID_EA { get; private set;}
        public string ID_RP { get; private set;}
        public string IRIS { get; private set;}
        public string LIBELLE { get; private set;}
        public string LIEN_CMT { get; private set;}
        public string LISTE_INSE { get; private set;}
        public string NOMBRE_IMM { get; private set;}
        public string NOMBRE_LOG { get; private set;}
        public string NOMBRE_NIV { get; private set;}
        public string NUMERO { get; private set;}
        public string NUMERO_PAR { get; private set;}
        public string NUMERO_PER { get; private set;}
        public string PRINCIPAL { get; private set;}
        public string QP { get; private set;}
        public string REPETITION { get; private set;}
        public string RIVOLI { get; private set;}
        public string SOUS_TYPE { get; private set;}
        public string TYPE { get; private set;}
        public string TYPE_LOCAL { get; private set;}
        public string TYPE_VOIE { get; private set;}
        public string apic_obj00 { get; private set;}
        public string apic_obj01 { get; private set;}
        public string apic_obj02 { get; private set;}
        public string apic_obj03 { get; private set;}
        public string apic_obj04 { get; private set;}
        public string apic_obj05 { get; private set;}
        public string apic_obj06 { get; private set;}
        public string apic_obj07 { get; private set;}
        public string apic_obj08 { get; private set;}
        public string apic_objec { get; private set;}


        public RilData(string raw, float rawX, float rawY, string actualite, string anneeCons,
            string canton, string categorie, string commentair, string complement, string comAncID, string comCapaci,
            string comDateC, string comNbLog, string comNom, string comSousC, string comStatut, string datemajEa,
            string depcom, string dernierTi, string echantillo, string enseigne, string grpRotati, string idEa,
            string idRP, string iris, string libelle, string lienCmt, string listeInse, string nombreImm,
            string nombreLog, string nombreNiv, string numero, string numeroPar, string numeroPer, string principal,
            string qp, string repetition, string rivoli, string sousType, string type, string typeLocal,
            string typeVoie, string apicObj00, string apicObj01, string apicObj02, string apicObj03, string apicObj04,
            string apicObj05, string apicObj06, string apicObj07, string apicObj08, string apicObjec) : base(rawX, rawY)
        {
            Raw = raw;
            X = rawX;
            Y = rawY;
            T = string.IsNullOrEmpty(anneeCons) ? 1850 : float.Parse(anneeCons);
            ACTUALITE = actualite;
            ANNEE_CONS = anneeCons;
            CANTON = canton;
            CATEGORIE = categorie;
            COMMENTAIR = commentair;
            COMPLEMENT = complement;
            COM_ANC_ID = comAncID;
            COM_CAPACI = comCapaci;
            COM_DATE_C = comDateC;
            COM_NB_LOG = comNbLog;
            COM_NOM = comNom;
            COM_SOUS_C = comSousC;
            COM_STATUT = comStatut;
            DATEMAJ_EA = datemajEa;
            DEPCOM = depcom;
            DERNIER_TI = dernierTi;
            ECHANTILLO = echantillo;
            ENSEIGNE = enseigne;
            GRP_ROTATI = grpRotati;
            ID_EA = idEa;
            ID_RP = idRP;
            IRIS = iris;
            LIBELLE = libelle;
            LIEN_CMT = lienCmt;
            LISTE_INSE = listeInse;
            NOMBRE_IMM = nombreImm;
            NOMBRE_LOG = nombreLog;
            NOMBRE_NIV = nombreNiv;
            NUMERO = numero;
            NUMERO_PAR = numeroPar;
            NUMERO_PER = numeroPer;
            PRINCIPAL = principal;
            QP = qp;
            REPETITION = repetition;
            RIVOLI = rivoli;
            SOUS_TYPE = sousType;
            TYPE = type;
            TYPE_LOCAL = typeLocal;
            TYPE_VOIE = typeVoie;
            apic_obj00 = apicObj00;
            apic_obj01 = apicObj01;
            apic_obj02 = apicObj02;
            apic_obj03 = apicObj03;
            apic_obj04 = apicObj04;
            apic_obj05 = apicObj05;
            apic_obj06 = apicObj06;
            apic_obj07 = apicObj07;
            apic_obj08 = apicObj08;
            apic_objec = apicObjec;
        }

        public override void SetX(float x)
        {
            base.SetX(x);
            this.X = x;
        }

        public override void SetY(float y)
        {
            base.SetY(y);
            this.Y = y;
        }

        public void SetT(float t)
        {
            this.T = t;
        }

        public override Vector3 GetPosition()
        {
            return new Vector3(RawX, RawY);
        }
    }

}