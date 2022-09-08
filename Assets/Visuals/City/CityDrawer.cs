using System;
using System.Collections.Generic;
using DataProcessing.City;
using Tools;
using UnityEngine;

namespace Visuals
{
    public class CityDrawer : MonoBehaviour
    {
        // Creates a line renderer that follows a Sin() function
        // and animates it.
        Mesh _cityBoundsMesh;
        CityDataConverter cityDataConverter;
        public LineRenderer _lineRenderer;
        Vector3[] cityData;

        private bool isActive;


        void Start()
        {
            cityDataConverter =
                (CityDataConverter) FactoryDataConverter.GetInstance(
                    FactoryDataConverter.AvailableDataManagerTypes.CITY);
            var config = Configuration.GetConfig();

            cityDataConverter.Init(
                (int) (Screen.width),
                (int) (Screen.height));

            _lineRenderer.transform.parent = gameObject.transform;
        }

        public void SetActive(bool state)
        {
            isActive = state;
            if (state)
                InitDrawing();
            else
                StopDrawing();
        }

        void StopDrawing()
        {
            _lineRenderer.positionCount = 0;
            _lineRenderer.SetPositions(new Vector3[] { });
            this.cityData = null;
        }

        public void InitDrawing()
        {
            //Prepare entities
            _cityBoundsMesh = new Mesh();

            this.LoadAllVectoredData();
            _lineRenderer.positionCount = cityData.Length;
            _lineRenderer.SetPositions(cityData);

            //link LineRenderer to Data
            //TODO : Bake when unity is less shitty
            //_lineRenderer.BakeMesh(_cityBoundsMesh, true);
        }


        public void LoadAllVectoredData()
        {
            if (!isActive || this.cityData != null)
                return;

            List<CityData> cityData = (List<CityData>) cityDataConverter.GetAllData();

            this.cityData = new Vector3[cityData.Count];
            for (int i = 0; i < cityData.Count; i++)
            {
                this.cityData[i] = new Vector3(cityData[i].X,cityData[i].Y, 1);
            }
        }

        void Update()
        {
            if (!isActive)
                return;

            if (transform.hasChanged)
            {
                StopDrawing();
                InitDrawing();
                transform.hasChanged = false;
            }

            //TODO : Bake when unity is less shitty
            //_lineRenderer.SetPositions(_cityBoundsMesh.vertices);
        }
    }
}