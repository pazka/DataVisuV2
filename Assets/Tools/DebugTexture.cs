using UnityEngine;

namespace Tools
{
    public class DebugTexture : MonoBehaviour
    {
        void Start()
        {
            return;
            Texture2D texture = new Texture2D(128, 128);
            GetComponent<Renderer>().material.mainTexture = texture;

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = ((x & y) != 0 ? Color.white : Color.gray);
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();}

        void Update()
        {
        }
    }
}
