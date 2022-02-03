using System;
using UnityEngine;

namespace Hacker.UI
{
    public static class GUIComboBox
    {
        public static int Box(int itemIndex, string[] items, string callerId)
        {
            GUIStyle guistyle = new GUIStyle();
            guistyle.normal.textColor = Color.cyan;
            guistyle.alignment = (TextAnchor)7;
            int num = items.Length;
            if (num == 0)
            {
                return -1;
            }
            if (num != 1)
            {
                int num2;
                if (GUIComboBox.popupWindow != null && callerId == GUIComboBox.popupWindow.OwnerId && GUIComboBox.popupWindow.CloseAndGetSelection(out num2))
                {
                    itemIndex = num2;
                    UnityEngine.Object.Destroy(GUIComboBox.popupWindow);
                    GUIComboBox.popupWindow = null;
                }
                Vector2 popupDimensions = GUIComboBox.GetPopupDimensions(items);
                GUILayout.Box(items[itemIndex], guistyle, new GUILayoutOption[]
                {
                    GUILayout.Width(popupDimensions.x),
                    GUILayout.Height(22f)
                });
                Vector2 position = GUIUtility.GUIToScreenPoint(GUILayoutUtility.GetLastRect().position);
                if (GUILayout.Button(" ▼ ", new GUILayoutOption[]
                {
                    GUILayout.Width(24f)
                }) && GUIComboBox.EnsurePopupWindow())
                {
                    GUIComboBox.popupWindow.Show(callerId, items, itemIndex, position, popupDimensions);
                }
                return itemIndex;
            }
            GUILayout.Label(items[0], guistyle, Array.Empty<GUILayoutOption>());
            return 0;
        }

        private static bool EnsurePopupWindow()
        {
            if (GUIComboBox.popupWindow != null)
            {
                return true;
            }
            TrainerMenu trainerMenu = UnityEngine.Object.FindObjectOfType<TrainerMenu>();
            if (trainerMenu == null)
            {
                return false;
            }
            if (trainerMenu.GetComponent<GUIComboBox.PopupWindow>() == null)
            {
                GUIComboBox.popupWindow = trainerMenu.gameObject.AddComponent<GUIComboBox.PopupWindow>();
            }
            return GUIComboBox.popupWindow != null;
        }

        private static Vector2 GetPopupDimensions(string[] items)
        {
            float num = 250f;
            float num2 = 0f;
            for (int i = 0; i < items.Length; i++)
            {
                Vector2 vector = GUI.skin.button.CalcSize(new GUIContent(items[i]));
                if (vector.x > num)
                {
                    num = vector.x;
                }
                num2 += vector.y;
            }
            return new Vector2(num + 36f, num2 + 36f);
        }

        private const string ExpandDownButtonText = " ▼ ";

        private static GUIComboBox.PopupWindow popupWindow;

        private class PopupWindow : MonoBehaviour
        {
            public string OwnerId { get; private set; }

            public PopupWindow()
            {
                hoverStyle = GUIComboBox.PopupWindow.CreateHoverStyle();
            }

            public void Show(string ownerId, string[] items, int currentIndex, Vector2 position, Vector2 popupSize)
            {
                OwnerId = ownerId;
                popupItems = items;
                selectedIndex = currentIndex;
                popupRect = new Rect(position, new Vector2(popupSize.x, Mathf.Min(400f, popupSize.y)));
                popupScrollPosition = default(Vector2);
                mouseClickPoint = null;
                readyToClose = false;
            }

            public bool CloseAndGetSelection(out int currentIndex)
            {
                if (readyToClose)
                {
                    currentIndex = selectedIndex;
                    Close();
                    return true;
                }
                currentIndex = -1;
                return false;
            }

            public void OnGUI()
            {
                if (OwnerId != null)
                {
                    GUI.ModalWindow(popupWindowId, popupRect, new GUI.WindowFunction(WindowFunction), string.Empty, GUIComboBox.PopupWindow.WindowStyle);
                }
            }

            public void Update()
            {
                if (OwnerId != null)
                {
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                    {
                        Vector3 mousePosition = Input.mousePosition;
                        mousePosition.y = (float)Screen.height - mousePosition.y;
                        mouseClickPoint = new Vector2?(mousePosition);
                        return;
                    }
                    mouseClickPoint = null;
                }
            }

            private static GUIStyle CreateHoverStyle()
            {
                GUIStyle guistyle = new GUIStyle(GUI.skin.label);
                guistyle.hover.textColor = Color.yellow;
                Texture2D texture2D = new Texture2D(1, 1);
                texture2D.SetPixel(0, 0, default(Color));
                texture2D.Apply();
                guistyle.hover.background = texture2D;
                guistyle.font = GUI.skin.font;
                guistyle.alignment = TextAnchor.MiddleCenter;
                return guistyle;
            }

            private static GUIStyle CreateWindowStyle()
            {
                Texture2D texture2D = new Texture2D(16, 16, (TextureFormat)4, false)
                {
                    wrapMode = TextureWrapMode.Repeat
                };
                for (int i = 0; i < texture2D.width; i++)
                {
                    for (int j = 0; j < texture2D.height; j++)
                    {
                        if (i == 0 || i == texture2D.width - 1 || j == 0 || j == texture2D.height - 1)
                        {
                            texture2D.SetPixel(i, j, new Color(0f, 0f, 0f, 1f));
                        }
                        else
                        {
                            texture2D.SetPixel(i, j, new Color(0.05f, 0.05f, 0.05f, 0.95f));
                        }
                    }
                }
                texture2D.Apply();
                GUIStyle guistyle = new GUIStyle(GUI.skin.window);
                guistyle.normal.background = texture2D;
                guistyle.onNormal.background = texture2D;
                guistyle.border.top = guistyle.border.bottom;
                guistyle.padding.top = guistyle.padding.bottom;
                return guistyle;
            }

            private void WindowFunction(int windowId)
            {
                if (OwnerId != null)
                {
                    popupScrollPosition = GUILayout.BeginScrollView(popupScrollPosition, false, false, Array.Empty<GUILayoutOption>());
                    int num = selectedIndex;
                    selectedIndex = GUILayout.SelectionGrid(selectedIndex, popupItems, 1, hoverStyle, Array.Empty<GUILayoutOption>());
                    GUILayout.EndScrollView();
                    if (num != selectedIndex || (mouseClickPoint != null && !popupRect.Contains(mouseClickPoint.Value)))
                    {
                        readyToClose = true;
                    }
                }
            }

            private void Close()
            {
                OwnerId = null;
                popupItems = null;
                selectedIndex = -1;
                mouseClickPoint = null;
            }

            private const float MaxPopupHeight = 400f;

            private static readonly GUIStyle WindowStyle = GUIComboBox.PopupWindow.CreateWindowStyle();

            private readonly int popupWindowId = GUIUtility.GetControlID(FocusType.Passive);

            private readonly GUIStyle hoverStyle;

            private Vector2 popupScrollPosition = Vector2.zero;

            private Rect popupRect;

            private Vector2? mouseClickPoint;

            private bool readyToClose;

            private int selectedIndex;

            private string[] popupItems;
        }
    }
}
