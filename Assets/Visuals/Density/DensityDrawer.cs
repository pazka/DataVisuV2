using System;
using System.Collections.Generic;
using DataProcessing.City;
using DataProcessing.Density;
using Tools;
using UnityEngine;

namespace Visuals
{
    public class DensityDrawer : MonoBehaviour
    {
        // Creates a line renderer that follows a Sin() function
        // and animates it.
        CityDataConverter cityDataConverter;
        DensityDataConverter densityDataConverter;
        List<DensityData> _densityData = new List<DensityData>();
        DensityData[] _dataBounds;
        private bool isActive = false;
        
        //visual vars
        int scaleGradientDetail = 5;
        private float[] scaleGradientSteps = new[] {0f, 0.2f, 0.4f, 0.6f, 0.8f, 1f};
        GUIStyle[] colorScales = new GUIStyle[5];
        public Color[] gradientColors = {Color.red, Color.green, Color.blue, Color.yellow, Color.white};


        Material globalmaterialForMesh;
        Mesh meshInstance;
        List<Matrix4x4> matricesOfDensity;

        Vector4[] colors;
        MaterialPropertyBlock colorBlockShader;
        private bool isOnError = false;

        struct CityAlign
        {
            public static Vector3 position = new Vector3(-968.5f, -525.04f, 10);
            public static Quaternion rotation = Quaternion.Euler(0, 0, 0.73f);
            public static Vector3 localScale = new Vector3(1f, 1f, 1f);
        }

        void Start()
        {
            var config = Configuration.GetConfig();

            transform.position += Vector3.Scale(CityAlign.position, new Vector3(config.scaleX, config.scaleY, 1f));
            transform.rotation *= CityAlign.rotation;
            transform.localScale = Vector3.Scale(transform.localScale, CityAlign.localScale);

            // Prepare entities
            densityDataConverter =
                (DensityDataConverter) FactoryDataConverter.GetInstance(FactoryDataConverter.AvailableDataManagerTypes
                    .DENSITY);

            densityDataConverter.Init(
                (int) (Screen.width),
                (int) (Screen.height));

            colorBlockShader = new MaterialPropertyBlock();
            globalmaterialForMesh = new Material(Shader.Find("Customs/InstancedColor"));


            //prepare square to use for GUI
            for (Int16 i = 0; i < scaleGradientDetail; i++)
            {
                Texture2D square = new Texture2D(1, 1);
                square.SetPixel(0, 0, gradientColors[i]);
                square.wrapMode = TextureWrapMode.Repeat;
                square.Apply();

                colorScales[i] = new GUIStyle();
                colorScales[i].normal.background = square;
            }
        }

        public void SetActive(bool state)
        {
            isActive = state;
            if (state)
                TryInitDrawing();
        }
        public void TryInitDrawing()
        {
            if (isOnError) return;
            
            try
            {
                InitDrawing();
            }
            catch (Exception e)
            {
                Debug.LogError("Error while drawing density data: " + e.Message);
                isOnError = true;
            }
        }

        public void InitDrawing()
        {
            var config = Configuration.GetConfig();
            var parentTransform = transform;
            //Getting our Data
            _densityData = (List<DensityData>) densityDataConverter.GetAllData();
            _dataBounds = (DensityData[]) densityDataConverter.GetDataBounds();

            scaleGradientSteps = config.densityGradiant;

            //Getting our visuals
            DensityData firstDensityData = _densityData[0];
            this.meshInstance = CreateQuad(
                0, 0,
                config.scaleX * (firstDensityData.X1 - firstDensityData.X2),
                config.scaleY * (firstDensityData.Y1 - firstDensityData.Y2),
                config.scaleX * (firstDensityData.X1 - firstDensityData.X3),
                config.scaleY * (firstDensityData.Y1 - firstDensityData.Y3),
                config.scaleX * (firstDensityData.X1 - firstDensityData.X4),
                config.scaleY * (firstDensityData.Y1 - firstDensityData.Y4)
            );

            this.matricesOfDensity = new List<Matrix4x4>(_densityData.Count);
            colors = new Vector4[_densityData.Count];

            for (int i = 0; i < _densityData.Count; i++)
            {
                DensityData densityData = _densityData[i];

                float tmpPop = densityData.Individuals - _dataBounds[0].Individuals;
                float uvPop = tmpPop / (((_dataBounds[1].Individuals+ 1f) - _dataBounds[0].Individuals));
                int indexSlice = 0;
                while ((indexSlice + 1) < gradientColors.Length && uvPop > scaleGradientSteps[indexSlice + 1])
                {
                    indexSlice++;
                }

                Vector3 rectPosition = new Vector3(
                    (_densityData[i].X1 + _densityData[i].X3) / 2,
                    (_densityData[i].Y1 + _densityData[i].Y2) / 2,
                    (float) VisualPlanner.Layers.Density
                );

                Vector3 position = transform.position +
                                   Vector3.Scale(rectPosition, new Vector3(config.scaleX, config.scaleY, 1f));

                matricesOfDensity.Add(Matrix4x4.TRS(position, transform.rotation, transform.localScale));
                colors[i] = this.gradientColors[indexSlice];
            }

            //creation of custom shader
            colorBlockShader.SetVectorArray("_Colors", colors);
            globalmaterialForMesh.enableInstancing = true;
        }

        private Mesh CreateQuad(
            float x1, float y1,
            float x2, float y2,
            float x3, float y3,
            float x4, float y4)
        {
            // Create a quad mesh.
            var mesh = new Mesh();

            var vertices = new Vector3[4]
            {
                new Vector3(x4, y4, 0),
                new Vector3(x3, y3, 0),
                new Vector3(x2, y2, 0),
                new Vector3(x1, y1, 0)
            };

            var tris = new int[6]
            {
                // lower left tri.
                0, 2, 1,
                // lower right tri
                0, 3, 2
            };

            var normals = new Vector3[4]
            {
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
                Vector3.forward,
            };

            var uv = new Vector2[4]
            {
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
            if (_densityData.Count < 0 || !isActive) return;

            if (transform.hasChanged)
            {
                TryInitDrawing();
                transform.hasChanged = false;
            }

            for (int i = 0; i < _densityData.Count; i += 1023)
            {
                DrawDensityChunk(i);
            }
        }
        
        private void DrawDensityChunk(int startIndex)
        {
            var sizeOfChunk = Math.Min(1023, _densityData.Count - startIndex);
            List<Matrix4x4> subMatriceOfDensity = matricesOfDensity.GetRange(startIndex, sizeOfChunk);
            
            Graphics.DrawMeshInstanced(meshInstance, 0, globalmaterialForMesh, subMatriceOfDensity.ToArray(), sizeOfChunk,
                colorBlockShader);
        }

        private void OnGUI()
        {
            if (!isActive) return;

            int i;
            float legendWidth = 50;
            for (i = 0; i < colorScales.Length; i++)
            {
                GUI.Label(new Rect(i * legendWidth, 5, legendWidth, legendWidth), "" + (i + 1), colorScales[i]);
            }
        }
    }
}