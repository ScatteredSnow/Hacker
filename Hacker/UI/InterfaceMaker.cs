using Hacker.Utils;
using System;
using UnityEngine;

namespace Hacker.UI
{
    public static class InterfaceMaker
    {
        public static void EatInputInRect(Rect eatRect)
        {
            if (eatRect.Contains(new Vector2(Input.mousePosition.x, (float)Screen.height - Input.mousePosition.y)))
            {
                Input.ResetInputAxes();
            }
        }

        public static GUISkin CustomSkin
        {
            get
            {
                if (InterfaceMaker._customSkin == null)
                {
                    try
                    {
                        InterfaceMaker._customSkin = InterfaceMaker.CreateSkin();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("Could not load custom GUISkin - " + ex.Message);
                        InterfaceMaker._customSkin = GUI.skin;
                    }
                }
                return InterfaceMaker._customSkin;
            }
        }

        private static GUISkin CreateSkin()
        {
            GUISkin guiskin = UnityEngine.Object.Instantiate<GUISkin>(GUI.skin);
            UnityEngine.Object.DontDestroyOnLoad(guiskin);
            InterfaceMaker._boxBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("NeoModTest.UI.guisharp-box.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker._boxBackground);
            guiskin.box.onNormal.background = null;
            guiskin.box.normal.background = InterfaceMaker._boxBackground;
            guiskin.box.normal.textColor = Color.white;
            InterfaceMaker._winBackground = ResourceUtils.LoadTexture(ResourceUtils.GetEmbeddedResource("NeoModTest.UI.guisharp-window.png", null));
            UnityEngine.Object.DontDestroyOnLoad(InterfaceMaker._winBackground);
            guiskin.window.onNormal.background = null;
            guiskin.window.normal.background = InterfaceMaker._winBackground;
            guiskin.window.padding = new RectOffset(6, 6, 22, 6);
            guiskin.window.border = new RectOffset(10, 10, 20, 10);
            guiskin.window.normal.textColor = Color.white;
            guiskin.button.padding = new RectOffset(4, 4, 3, 3);
            guiskin.button.normal.textColor = Color.white;
            guiskin.textField.normal.textColor = Color.white;
            guiskin.label.normal.textColor = Color.white;
            return guiskin;
        }

        private static Texture2D _boxBackground;

        private static Texture2D _winBackground;

        private static GUISkin _customSkin;
    }
}
