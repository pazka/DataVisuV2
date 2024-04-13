using System;
using System.Collections;
using System.Collections.Generic;
using DataProcessing.Generic;
using Tools;
using UnityEngine;
using Random = System.Random;

namespace DataProcessing.Sirene
{
    public class SireneData : TimedData
    {
        public string Sirene { get; private set; }
        public DateTime DateCreation { get; private set; }
        public string Name { get; private set; }
        public bool IsOnePerson { get; private set; }
        
        public float EntityCount { get; set; }


        public SireneData(string raw, float rawX, float rawY, float t) : base(raw, rawX, rawY, t)
        {
        }

        public SireneData(string raw, float rawX, float rawY, string sirene, DateTime dateCreation, string name,
            bool isOnePerson,int entityCount) : base(raw, rawX, rawY)
        {
            this.T = (float)(dateCreation - new DateTime(1900, 1, 1, 0, 0, 0)).TotalDays;
            this.Sirene = sirene;
            this.DateCreation = dateCreation;
            this.Name = name;
            this.IsOnePerson = isOnePerson;
            this.EntityCount = entityCount;
        }
    }


    public class FutureSireneData : SireneData
    {
        private static Random _rnd = new Random();

        public FutureSireneData(float rawX, float rawY, float t) : base("future", rawX, rawY, t)
        {
        }

        public void Randomize(int posRnd = 100, int perlinRnd = 500, float maxBatSize = 1f, float[] bias = null)
        {
            if (bias == null)
            {
                bias = new float[] { 0, 0 };
            }

            //get random point around a existing point
            float newX = this.X + (_rnd.Next(0, posRnd * 2) - posRnd) + bias[0];
            float newY = this.Y + (_rnd.Next(0, posRnd * 2) - posRnd) + bias[1];

            //amplification of the bat size
            float ampl = Mathf.PerlinNoise(
                _rnd.Next(0, 500) - 250 + newX,
                _rnd.Next(0, perlinRnd * 2) - perlinRnd + newY
            );
            ampl = 1 + 10 * ampl * ampl * ampl;


            this.SetX(newX);
            this.SetY(newY);
            this.EntityCount = (int) Math.Min(this.EntityCount * ampl, maxBatSize);
        }
    }
}