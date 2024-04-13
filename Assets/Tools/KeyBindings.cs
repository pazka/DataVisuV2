﻿using UnityEngine;

namespace Tools
{
    public static class KeyBindings
    {
        public static readonly KeyCode ToggleDebugText = KeyCode.F1;
        public static readonly KeyCode ToggleCityLine = KeyCode.F2;
        public static readonly KeyCode ToggleDensity = KeyCode.F3;
        public static readonly KeyCode ToggleSireneDrawing = KeyCode.F4;
        public static readonly KeyCode PauseSireneDrawing = KeyCode.F5;
        public static readonly KeyCode TogglePureData = KeyCode.F6;
        public static readonly KeyCode ToggleRegisteringRestriction = KeyCode.R;
        public static readonly KeyCode AddRestrictionPoint = KeyCode.T;
        public static readonly KeyCode Quit = KeyCode.Escape;

        public static string GetBindingStrings()
        {
            var res = "";
            // Convert this class properties to a string
            foreach (var property in typeof(KeyBindings).GetFields())
            {
                res += property.Name + " : " + property.GetValue(null) + "\n";
            }
            
            return res;
        }
    }
}