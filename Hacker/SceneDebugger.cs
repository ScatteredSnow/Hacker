using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Hacker
{
    public class SceneDebugger : MonoBehaviour
    {
        private int ProjectWindowId
        {
            get
            {
                return HierarchyWindowId + 1;
            }
        }

        private int InspectorWindowId
        {
            get
            {
                return ProjectWindowId + 1;
            }
        }

        public void OnEnable()
        {
            HierarchyWindowId = GetHashCode();
            HierarchyWindow = new Rect((float)(Screen.width - HierarchyWidth - Margin), (float)Margin, (float)HierarchyWidth, (float)(Screen.height - Margin * 2));
            ProjectWindow = new Rect(HierarchyWindow.x - (float)Margin - (float)ProjectWidth, (float)Margin, (float)ProjectWidth, (float)(Screen.height - Margin * 2));
            InspectorWindow = new Rect(ProjectWindow.x - (float)Margin - (float)InspectorWidth, (float)Margin, (float)InspectorWidth, (float)(Screen.height - Margin * 2));
        }

        public void OnGUI()
        {
            HierarchyWindow = GUILayout.Window(HierarchyWindowId, HierarchyWindow, new GUI.WindowFunction(HierarchyWindowMethod), "Hierarchy", new GUILayoutOption[0]);
            ProjectWindow = GUILayout.Window(ProjectWindowId, ProjectWindow, new GUI.WindowFunction(ProjectWindowMethod), "Project", new GUILayoutOption[0]);
        }

        private void DisplayGameObject(GameObject gameObj, int level)
        {
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Space((float)(level * 20));
            Color color = GUI.color;
            if (SelectedGameObject == gameObj.transform)
            {
                GUI.color = Color.green;
            }
            if (!gameObj.activeSelf && gameObj.transform.childCount == 0)
            {
                GUI.color = Color.magenta;
            }
            else if (gameObj.transform.childCount == 0)
            {
                GUI.color = Color.yellow;
            }
            else if (!gameObj.activeSelf)
            {
                GUI.color = Color.red;
            }
            if (GUILayout.Toggle(ExpandedObjs.Contains(gameObj.name), gameObj.name, new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(false)
            }))
            {
                if (!ExpandedObjs.Contains(gameObj.name))
                {
                    ExpandedObjs.Add(gameObj.name);
                    SelectedGameObject = gameObj.transform;
                }
            }
            else if (ExpandedObjs.Contains(gameObj.name))
            {
                ExpandedObjs.Remove(gameObj.name);
                SelectedGameObject = gameObj.transform;
            }
            GUI.color = color;
            GUILayout.EndHorizontal();
            if (ExpandedObjs.Contains(gameObj.name))
            {
                for (int i = 0; i < gameObj.transform.childCount; i++)
                {
                    DisplayGameObject(gameObj.transform.GetChild(i).gameObject, level + 1);
                }
            }
        }

        private void HierarchyWindowMethod(int id)
        {
            GUILayout.BeginVertical(GUIContent.none, GUI.skin.box, new GUILayoutOption[0]);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            SearchText = GUILayout.TextField(SearchText, new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true)
            });
            GUILayout.Button("Search", new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(false)
            });
            GUILayout.EndHorizontal();
            List<GameObject> list = new List<GameObject>();
            foreach (Transform transform in FindObjectsOfType<Transform>())
            {
                if (transform.parent == null)
                {
                    list.Add(transform.gameObject);
                }
            }
            if (SelectedGameObject == null)
            {
                SelectedGameObject = list.First<GameObject>().transform;
            }
            HierarchyScrollPos = GUILayout.BeginScrollView(HierarchyScrollPos, new GUILayoutOption[]
            {
                GUILayout.Height(HierarchyWindow.height / 3f),
                GUILayout.ExpandWidth(true)
            });
            foreach (GameObject gameObj in list)
            {
                DisplayGameObject(gameObj, 0);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUIContent.none, GUI.skin.box, new GUILayoutOption[0]);
            PropertiesScrollPos = GUILayout.BeginScrollView(PropertiesScrollPos, new GUILayoutOption[0]);
            string text = SelectedGameObject.name;
            Transform parent = SelectedGameObject.parent;
            while (parent != null)
            {
                text = parent.name + "/" + text;
                parent = parent.parent;
            }
            GUILayout.Label(text, new GUILayoutOption[0]);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Label(SelectedGameObject.gameObject.layer.ToString() + " : " + LayerMask.LayerToName(SelectedGameObject.gameObject.layer), new GUILayoutOption[0]);
            GUILayout.FlexibleSpace();
            SelectedGameObject.gameObject.SetActive(GUILayout.Toggle(SelectedGameObject.gameObject.activeSelf, "Active", new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(false)
            }));
            if (GUILayout.Button("?", new GUILayoutOption[0]))
            {
                Console.WriteLine("?");
            }
            if (GUILayout.Button("X", new GUILayoutOption[0]))
            {
                Destroy(SelectedGameObject.gameObject);
            }
            GUILayout.EndHorizontal();
            foreach (Component component in SelectedGameObject.GetComponents<Component>())
            {
                GUILayout.BeginHorizontal(GUIContent.none, GUI.skin.box, new GUILayoutOption[0]);
                if (component is Behaviour)
                {
                    (component as Behaviour).enabled = GUILayout.Toggle((component as Behaviour).enabled, "", new GUILayoutOption[]
                    {
                        GUILayout.ExpandWidth(false)
                    });
                }
                GUILayout.Label(component.GetType().Name + " : " + component.GetType().Namespace, new GUILayoutOption[0]);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("?", new GUILayoutOption[0]))
                {
                    Console.WriteLine("?");
                }
                if (!(component is Transform) && GUILayout.Button("X", new GUILayoutOption[0]))
                {
                    Destroy(component);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void ProjectWindowMethod(int id)
        {
            GUILayout.BeginVertical(GUIContent.none, GUI.skin.box, new GUILayoutOption[0]);
            ProjectScrollPos = GUILayout.BeginScrollView(ProjectScrollPos, new GUILayoutOption[]
            {
                GUILayout.Height(ProjectWindow.height / 3f),
                GUILayout.ExpandWidth(true)
            });
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                ExpandedObjects[assembly] = GUILayout.Toggle(ExpandedObjects.ContainsKey(assembly) && ExpandedObjects[assembly], assembly.GetName().Name, new GUILayoutOption[]
                {
                    GUILayout.ExpandWidth(false)
                });
                if (ExpandedObjects[assembly])
                {
                    foreach (Type type in (from t in assembly.GetTypes()
                                           where t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters
                                           select t).ToList<Type>())
                    {
                        if (type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Count((FieldInfo f) => f.Name != "OffsetOfInstanceIDInCPlusPlusObject") != 0)
                        {
                            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                            Color color = GUI.color;
                            GUILayout.Space(20f);
                            ExpandedObjects[type] = GUILayout.Toggle(ExpandedObjects.ContainsKey(type) && ExpandedObjects[type], type.Name, new GUILayoutOption[]
                            {
                                GUILayout.ExpandWidth(false)
                            });
                            GUI.color = color;
                            GUILayout.EndHorizontal();
                            if (ExpandedObjects[type])
                            {
                                foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
                                {
                                    if (!(fieldInfo.Name == "OffsetOfInstanceIDInCPlusPlusObject"))
                                    {
                                        GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                                        GUILayout.Space(40f);
                                        ConcurrentDictionary<object, bool> expandedObjects = ExpandedObjects;
                                        object key = fieldInfo;
                                        bool flag = ExpandedObjects.ContainsKey(fieldInfo) && ExpandedObjects[fieldInfo];
                                        string name = fieldInfo.Name;
                                        string str = " : ";
                                        Type fieldType = fieldInfo.FieldType;
                                        expandedObjects[key] = GUILayout.Toggle(flag, name + str + ((fieldType != null) ? fieldType.ToString() : null), GUI.skin.label, new GUILayoutOption[]
                                        {
                                            GUILayout.ExpandWidth(false)
                                        });
                                        GUILayout.EndHorizontal();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private int HierarchyWindowId;

        private int Margin = 50;

        private Rect HierarchyWindow;

        private int HierarchyWidth = 400;

        private Vector2 HierarchyScrollPos;

        private string SearchText = "";

        private Vector2 PropertiesScrollPos;

        private Transform SelectedGameObject;

        private List<string> ExpandedObjs = new List<string>();

        private Rect ProjectWindow;

        private int ProjectWidth = 400;

        private Vector2 ProjectScrollPos;

        private ConcurrentDictionary<object, bool> ExpandedObjects = new ConcurrentDictionary<object, bool>();

        private Rect InspectorWindow;

        private int InspectorWidth = 350;
    }
}
