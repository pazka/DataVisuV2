﻿using System;
using System.Collections.Generic;
using DataProcessing.City;
using DataProcessing.Density;
using UnityEngine;

namespace Visuals
{
    public class DensityDrawer : MonoBehaviour
    {
        // Creates a line renderer that follows a Sin() function
        // and animates it.
        CityDataManager _cityDataManager;
        DensityDataManager _densityDataManager;
        List<DensityData> _densityData = new List<DensityData>();
        DensityData[] _dataBounds;


        //visual vars
        int scaleGradientDetail = 5;
        GUIStyle[] colorScales = new GUIStyle[5];
        public Color[] gradientColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.white };


        Material globalmaterialForMesh;
        Mesh meshInstance;
        Matrix4x4[] matricesOfDensity;

        Vector4[] colors;
        MaterialPropertyBlock colorBlockShader;

        void Start()
        {
            //Prepare entities        
            _densityDataManager = (DensityDataManager)FactoryDataManager.GetInstance(FactoryDataManager.AvailableDataManagerTypes.DENSITY);

            _densityDataManager.Init(Screen.width, Screen.height);

            colorBlockShader = new MaterialPropertyBlock();
            globalmaterialForMesh = new Material(Shader.Find("Customs/InstancedColor"));

        }

        public void FillWithData()
        {
            //Getting our Data
            _densityData = (List<DensityData>)_densityDataManager.GetAllData();
            _dataBounds = (DensityData[])_densityDataManager.getDataBounds();

            //Getting our visuals
            DensityData firstDensityData = _densityData[0];
            this.meshInstance = CreateQuad(
                0,0,
                firstDensityData.X1 - firstDensityData.X2, firstDensityData.Y1  - firstDensityData.Y2,
                firstDensityData.X1 - firstDensityData.X3, firstDensityData.Y1  - firstDensityData.Y3,
                firstDensityData.X1 - firstDensityData.X4, firstDensityData.Y1  - firstDensityData.Y4
                );

            this.matricesOfDensity = new Matrix4x4[_densityData.Count];
            colors = new Vector4[_densityData.Count];

            for (int i = 0; i < _densityData.Count; i++)
            {
                DensityData densityData = _densityData[i];

                float tmpPop = densityData.Pop - _dataBounds[0].Pop;
                int indexSlice = (int)Math.Floor(tmpPop / (((_dataBounds[1].Pop + 1f) - _dataBounds[0].Pop) / scaleGradientDetail));


                Vector3 position = new Vector3(
                    (_densityData[i].X1 + _densityData[i].X3) / 2,
                    (_densityData[i].Y1 + _densityData[i].Y2) / 2, 0);
                Quaternion rotation = Quaternion.Euler(0, 0, 0);
                Vector3 scale = Vector3.one;

                matricesOfDensity[i] = Matrix4x4.TRS(position, rotation, scale);
                colors[i] = this.gradientColors[indexSlice];

                // GUILayout.BeginArea(new Rect(densityData.X1, densityData.Y1, densityData.X1 - densityData.X3, densityData.Y3 - densityData.Y1), colorScales[indexSlice]);

            }

            //creation of custom shader
            colorBlockShader.SetVectorArray("_Colors", colors);
            globalmaterialForMesh.enableInstancing = true;
        }

        private Mesh CreateQuad(
            float x1, float y1,
            float x2, float y2,
            float x3, float y3,
            float x4, float y4){

            // Create a quad mesh.
            var mesh = new Mesh();

            var vertices = new Vector3[4] {
                new Vector3(x4, y4, 0),
                new Vector3(x3, y3, 0),
                new Vector3(x2, y2, 0),
                new Vector3(x1, y1, 0)
            };

            var tris = new int[6] {
                // lower left tri.
                0, 2, 1,
                // lower right tri
                0, 3, 2
            };

            var normals = new Vector3[4] {
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
            };

            var uv = new Vector2[4] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
            };

            mesh.vertices = vertices;
            mesh.triangles = tris;
            mesh.normals = normals;
            mesh.uv = uv;

            return mesh;
        }

        void Update()
        {
            if (_densityData.Count > 0)
            {
                Graphics.DrawMeshInstanced(meshInstance, 0, globalmaterialForMesh, matricesOfDensity, _densityData.Count, colorBlockShader);
            }
        }

        void OnGUI()
        {
         
        }
    }
}