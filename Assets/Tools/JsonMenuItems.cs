using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tools
{
    public class JsonMenuItems : MonoBehaviour
    {
        private Dictionary<string, string> allValues = new Dictionary<string, string>();

        private void Start()
        {
            allValues.Add("Test1", "ok");
            allValues.Add("Test2", "koko");
            allValues.Add("Test3", "ouioui");
            
            int i = 0;
            foreach (var value in allValues)
            {
                TextMeshPro textInput = gameObject.AddComponent<TextMeshPro>() as TextMeshPro;
                textInput.transform.position = new Vector3(10, 100 * i, 10);
                textInput.SetText(value.Key);

                i++;
            }
            
            var config = Configuration.GetConfig();
            var content = Configuration.ConfigContent;
        }

        void OnGUI()
        {
            // Make a text field that modifies stringToEdit.
            var content = Configuration.ConfigContent;

            int i = 0;
            foreach (var value in allValues)
            {
                GUI.TextField(new Rect(50, 100 * i, 400, 100), value.Value, 200);

                i++;
            }
        }
    }
}