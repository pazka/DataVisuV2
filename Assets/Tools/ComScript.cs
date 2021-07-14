using UnityEngine;

namespace Tools
{
    public class ComScript : MonoBehaviour
    {
        public bool isDisplayed = true;
        private bool state = false;

        // Start is called before the first frame update
        void Start()
        {
            state = !isDisplayed;
        }

        void ChangeChildrenStateRecusively(Transform rootTransform)
        {
            rootTransform.gameObject.SetActive(state);

            foreach (Transform child in rootTransform)
            {
                ChangeChildrenStateRecusively(child);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyBindings.ToggleDebugText))
            {
                isDisplayed = !isDisplayed;
            }

            if (state != isDisplayed)
            {
                state = isDisplayed;
                foreach (Transform child in transform)
                {
                    ChangeChildrenStateRecusively(child);
                }
            }
        }
    }
}