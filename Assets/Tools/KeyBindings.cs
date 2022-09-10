using System.Reflection;
using UnityEngine;
    
namespace Tools
{
    public static class KeyBindings
    {
        public static readonly KeyCode ToggleDebugText= KeyCode.F1;
        public static readonly KeyCode ToggleCityLine  = KeyCode.F2;
        public static readonly KeyCode ToggleDensity = KeyCode.F3;
        public static readonly KeyCode ToggleRilDrawing = KeyCode.F4;
        public static readonly KeyCode PauseRilDrawing = KeyCode.F5;
        public static readonly KeyCode TogglePureData = KeyCode.F6;
        public static readonly KeyCode Quit = KeyCode.Escape;
        
        public static string GetBindingStrings()
        {
            string res = "";
            PropertyInfo[] props = typeof(KeyBindings).GetProperties();
            foreach (var prop in props)
            {
                res += prop.Name + " = " + (prop.GetValue(null,null)) + "\n";
            }

            return res;
        }
    }

}