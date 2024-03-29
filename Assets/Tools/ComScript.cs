﻿using UnityEngine;

namespace Tools
{
    public class ComScript : MonoBehaviour
    {
        public bool isDisplayed = true;
        private bool state;

        // Start is called before the first frame update
        private void Start()
        {
            var config = Configuration.GetConfig();
            if (!config.isDev)
            {
                isDisplayed = config.debugVisual;
            }

            state = !isDisplayed;
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyBindings.Quit)) Application.Quit();

            if (Input.GetKeyDown(KeyBindings.ToggleDebugText)) isDisplayed = !isDisplayed;

            if (state != isDisplayed)
            {
                state = isDisplayed;
                foreach (Transform child in transform) ChangeChildrenStateRecusively(child);
            }
        }

        private void ChangeChildrenStateRecusively(Transform rootTransform)
        {
            rootTransform.gameObject.SetActive(state);

            foreach (Transform child in rootTransform) ChangeChildrenStateRecusively(child);
        }
    }
}