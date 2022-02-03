using Hacker.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Hacker
{
    public class TrainerMenu : MonoBehaviour
    {
        public static GameObject[] GetDontDestroyOnLoadObjects()
        {
            GameObject gameObject = null;
            GameObject[] rootGameObjects;
            try
            {
                gameObject = new GameObject();
                DontDestroyOnLoad(gameObject);
                Scene scene = gameObject.scene;
                DestroyImmediate(gameObject);
                gameObject = null;
                rootGameObjects = scene.GetRootGameObjects();
            }
            finally
            {
                if (gameObject != null)
                {
                    DestroyImmediate(gameObject);
                }
            }
            return rootGameObjects;
        }

        public static List<T> GetAllComponentsFromScene<T>(Scene scene) where T : Component
        {
            List<T> list = new List<T>();
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootGameObjects.Length; i++)
            {
                T[] componentsInChildren = rootGameObjects[i].GetComponentsInChildren<T>(true);
                list.AddRange(componentsInChildren);
            }
            return list;
        }

        public static string GetHierarchyDebugString(GameObject gameObject, int maxDepth)
        {
            if (gameObject == null)
            {
                return "null";
            }
            StringBuilder stringBuilder = new StringBuilder();
            Transform transform = gameObject.transform;
            stringBuilder.Append(transform.name);
            transform = transform.parent;
            int num = 0;
            while (num < maxDepth && transform != null)
            {
                stringBuilder.Append("<- ");
                stringBuilder.Append(transform.name);
                transform = transform.parent;
                num++;
            }
            if (transform != null)
            {
                stringBuilder.Append("<- ...");
            }
            return stringBuilder.ToString();
        }

        public static GameObject FindInActiveObjectByName(string name)
        {
            Transform[] array = Resources.FindObjectsOfTypeAll(typeof(Transform)) as Transform[];
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].hideFlags == null && array[i].name == name)
                {
                    return array[i].gameObject;
                }
            }
            return null;
        }

        public static List<GameObject> FindInActiveObjectsByNamePartial(string name)
        {
            List<GameObject> list = new List<GameObject>();
            Transform[] array = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].hideFlags == null && array[i].name.ToUpper().Contains(name.ToUpper()))
                {
                    list.Add(array[i].gameObject);
                }
            }
            return list;
        }

        public static T FindInActiveObjectByComp<T>() where T : MonoBehaviour
        {
            Transform[] array = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].hideFlags == null && array[i].gameObject.HasComponent<T>())
                {
                    return array[i].GetComponent<T>();
                }
            }
            return default(T);
        }

        public static List<T> FindInActiveObjectsByComp<T>() where T : MonoBehaviour
        {
            List<T> list = new List<T>();
            Transform[] array = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].hideFlags == null && array[i].gameObject.HasComponent<T>())
                {
                    list.Add(array[i].GetComponent<T>());
                }
            }
            return list;
        }

        public static List<T> FindInActiveObjectsByCompSO<T>() where T : ScriptableObject
        {
            List<T> list = new List<T>();
            ScriptableObject[] array = Resources.FindObjectsOfTypeAll<ScriptableObject>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].hideFlags == null && array[i].GetType() == typeof(T))
                {
                    T item = array[i] as T;
                    list.Add(item);
                }
            }
            return list;
        }

        public static GameObject FindByName(string name)
        {
            foreach (GameObject gameObject in FindObjectsOfType(typeof(GameObject)))
            {
                if (gameObject != null && gameObject.name == name)
                {
                    return gameObject;
                }
            }
            return null;
        }

        public static GameObject FindByName(GameObject root, string name)
        {
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                if (transform != null && transform.name == name)
                {
                    return transform.gameObject;
                }
            }
            return null;
        }

        public static Type GetTypeByName(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse<Assembly>())
            {
                Type type = assembly.GetType(name);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        public static bool IsNullableEnum(Type t)
        {
            Type underlyingType = Nullable.GetUnderlyingType(t);
            return underlyingType != null && underlyingType.IsEnum;
        }

        public static void ForEachInHierarchy(Transform t, Action<Transform> action)
        {
            action(t);
            foreach (object obj in t)
            {
                ForEachInHierarchy((Transform)obj, action);
            }
        }

        private void Start()
        {
            Debug.unityLogger.logEnabled = true;
            MainWindow = new Rect((float)(Screen.width / 2 - 100), (float)(Screen.height / 2 - 350), 260f, 50f);
            MiscWindow1 = new Rect(MainWindow.x + 260f, (float)(Screen.height / 2 - 250), 250f, 150f);
            MiscWindow2 = new Rect(MainWindow.x + 260f, (float)(Screen.height / 2 - 400), 250f, 225f);
            MiscWindow3 = new Rect(MainWindow.x + 260f, (float)(Screen.height / 2 - 250), 250f, 150f);
            origHeight = Screen.height;
            origWidth = Screen.width;
            SceneManager.activeSceneChanged += new UnityAction<Scene, Scene>(SceneManager_activeSceneChanged);
            InsertHarmonyPatches();
            instance = this;
            itemDB = Xml2Object.Deserialize<Items>(baseDirectory + "\\Data\\Config\\items.xml");
            BuildLists();
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
        }

        private void Update()
        {
            if (!Debug.unityLogger.logEnabled)
            {
                Debug.unityLogger.logEnabled = true;
            }
            if (Input.GetKeyDown(KeyCode.Home))
            {
                MainWindowVisible = !MainWindowVisible;
            }
            if (!sceneTracker.ContainsKey(sceneName))
            {
                sceneTracker.Add(sceneName, sceneIndex);
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                foreach (KeyValuePair<string, int> keyValuePair in sceneTracker)
                {
                    Debug.LogWarning("Scene: " + keyValuePair.Key + " Index: " + keyValuePair.Value.ToString());
                }
            }
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                toggleCursor = !toggleCursor;
                GameManager.Instance.Pause(toggleCursor);
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                killZombiesTrigger = true;
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
                godmodeTrigger = true;
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                flymodeTrigger = true;
            }
            if (Input.GetKeyDown(KeyCode.F8))
            {
                invisibleTrigger = true;
            }
        }

        private void OnGUI()
        {
            if (!MainWindowVisible)
            {
                return;
            }
            if (Event.current.type == EventType.Layout)
            {
                GUISkin customSkin = InterfaceMaker.CustomSkin;
                GUI.backgroundColor = new Color(62f, 62f, 66f);
                GUIStyle guistyle = new GUIStyle(customSkin.window);
                guistyle.normal.textColor = Color.green;
                GUI.backgroundColor = Color.black;
                new GUIStyle(GUI.skin.window).normal.textColor = Color.green;
                MainWindow = new Rect(MainWindow.x, MainWindow.y, 260f, 50f);
                MainWindow = GUILayout.Window(777, MainWindow, new GUI.WindowFunction(RenderUI), "7DTD Hacker", guistyle, new GUILayoutOption[0]);
                if (MiscWindowVisible)
                {
                    MiscWindow1 = new Rect(MiscWindow1.x, MiscWindow1.y, 250f, 50f);
                    MiscWindow1 = GUILayout.Window(778, MiscWindow1, new GUI.WindowFunction(RenderUI), "杂项", guistyle, new GUILayoutOption[0]);
                }
                if (MiscWindow2Visible)
                {
                    MiscWindow2 = GUILayout.Window(779, MiscWindow2, new GUI.WindowFunction(RenderUI), "物品和资源", guistyle, new GUILayoutOption[0]);
                }
                if (MiscWindow3Visible)
                {
                    MiscWindow3 = GUILayout.Window(780, MiscWindow3, new GUI.WindowFunction(RenderUI), "其它", guistyle, new GUILayoutOption[0]);
                }
            }
        }

        private void RenderUI(int id)
        {
            GUIStyle guistyle = new GUIStyle();
            guistyle.normal.background = Texture2D.whiteTexture;
            guistyle.alignment = TextAnchor.MiddleCenter;
            GUIStyle guistyle2 = new GUIStyle();
            guistyle2.normal.textColor = Color.green;
            guistyle2.alignment = TextAnchor.MiddleCenter;
            GUIStyle guistyle3 = new GUIStyle();
            guistyle3.normal.textColor = Color.cyan;
            guistyle3.alignment = TextAnchor.MiddleCenter;
            GUIStyle guistyle4 = new GUIStyle();
            guistyle4.normal.textColor = Color.cyan;
            guistyle4.alignment = TextAnchor.MiddleLeft;
            GUIStyle guistyle5 = new GUIStyle();
            guistyle5.normal.textColor = Color.white;
            guistyle5.alignment = TextAnchor.MiddleCenter;
            GUIStyle guistyle6 = new GUIStyle();
            guistyle6.normal.textColor = Color.yellow;
            guistyle6.alignment = TextAnchor.MiddleCenter;
            GUIStyle guistyle7 = new GUIStyle();
            guistyle7.normal.textColor = Color.red;
            guistyle7.alignment = TextAnchor.MiddleCenter;
            sceneName = SceneManager.GetActiveScene().name;
            sceneIndex = SceneManager.GetActiveScene().buildIndex;
            bool flag = true;
            switch (id)
            {
                case 777:
                    GUILayout.Label("显示/隐藏: Home", guistyle5, new GUILayoutOption[0]);
                    GUILayout.Space(10f);
                    if (flag)
                    {
                        GUILayout.Label(MakeEnable("[PgUp]", " 显隐光标               ", toggleCursor), guistyle4, Array.Empty<GUILayoutOption>());
                        GUILayout.Label(MakeEnable("[PgDwn]", " 无限弹药         ", Cheat.infiniteAmmo), guistyle4, Array.Empty<GUILayoutOption>());
                        GUILayout.Label(MakeEnable("[F2]", "        加速               ", Cheat.speed), guistyle4, Array.Empty<GUILayoutOption>());
                        GUILayout.Space(5f);
                        GUI.color = Color.white;
                        if (GUILayout.Button("升级 (最大: 300)", new GUILayoutOption[0]) && Objects.PlayerLocal)
                        {
                            Progression progression = Objects.PlayerLocal.Progression;
                            progression.AddLevelExp(progression.ExpToNextLevel, "_xpOther", Progression.XPTypes.Other, true);
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("添加50技能点", new GUILayoutOption[0]) && Objects.PlayerLocal)
                        {
                            Objects.PlayerLocal.Progression.SkillPoints += 50;
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("完成当前任务", new GUILayoutOption[0]))
                        {
                            try
                            {
                                if (Objects.questJournal != null)
                                {
                                    Quest trackedQuest = Objects.questJournal.TrackedQuest;
                                    if (trackedQuest != null)
                                    {
                                        for (int l = 0; l < trackedQuest.Objectives.Count; l++)
                                        {
                                            trackedQuest.Objectives.ElementAt(l).GetType().GetMethod("ChangeStatus", defaultBindingFlags).Invoke(trackedQuest.Objectives.ElementAt(l), new object[]
                                            {
                                            true
                                            });
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning("[Trainer] TrackedQuest is NULL!");
                                    }
                                    Quest activeQuest = Objects.questJournal.ActiveQuest;
                                    if (activeQuest != null)
                                    {
                                        for (int j = 0; j < activeQuest.Objectives.Count; j++)
                                        {
                                            activeQuest.Objectives.ElementAt(j).GetType().GetMethod("ChangeStatus", defaultBindingFlags).Invoke(activeQuest.Objectives.ElementAt(j), new object[]
                                            {
                                            true
                                            });
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogWarning("[Trainer] TrackedQuest is NULL!");
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("[Trainer] QuestJournal is NULL!");
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("ERROR: " + ex.Message);
                            }
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("添加20K金钱", new GUILayoutOption[0]))
                        {
                            try
                            {
                                GiveItem("casinoCoin", 20000);
                                Debug.Log("Added 20K Money...");
                            }
                            catch (Exception ex2)
                            {
                                Debug.LogError("ERROR: " + ex2.Message);
                            }
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("杀死僵尸 (范围50内的所有目标) [F11]", new GUILayoutOption[0]) || killZombiesTrigger)
                        {
                            try
                            {
                                foreach (EntityZombie entityZombie in Objects.ZombieObj)
                                {
                                    if (Vector3.Distance(Objects.PlayerLocal.position, entityZombie.position) <= 50f)
                                    {
                                        Entity entity = entityZombie;
                                        DamageResponse damageResponse = default(DamageResponse);
                                        damageResponse.Fatal = true;
                                        entity.Kill(damageResponse);
                                    }
                                }
                                killZombiesTrigger = false;
                                Debug.Log("Killed all Zombies...");
                            }
                            catch (Exception ex3)
                            {
                                Debug.LogError("ERROR: " + ex3.Message);
                            }
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("时间跳跃 (+6小时)", new GUILayoutOption[0]))
                        {
                            try
                            {
                                ulong worldTime = GameManager.Instance.World.GetWorldTime();
                                GameManager.Instance.World.SetTimeJump(worldTime + 6000UL, false);
                                Debug.Log("Added 6hr to World Time...");
                            }
                            catch (Exception ex4)
                            {
                                Debug.LogError("ERROR: " + ex4.Message);
                            }
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("治疗", new GUILayoutOption[0]))
                        {
                            try
                            {
                                Objects.PlayerLocal.AddHealth(Objects.PlayerLocal.GetMaxHealth());
                                Debug.Log("Healing...");
                            }
                            catch (Exception ex5)
                            {
                                Debug.LogError("ERROR: " + ex5.Message);
                            }
                        }
                        GUI.color = Color.white;
                        if (MiscWindowVisible)
                        {
                            GUI.color = Color.green;
                        }
                        else
                        {
                            GUI.color = Color.white;
                        }
                        if (GUILayout.Button("杂项菜单", new GUILayoutOption[0]))
                        {
                            MiscWindowVisible = !MiscWindowVisible;
                        }
                        GUI.color = Color.white;
                        if (MiscWindow2Visible)
                        {
                            GUI.color = Color.green;
                        }
                        else
                        {
                            GUI.color = Color.white;
                        }
                        if (GUILayout.Button("物品和资源", new GUILayoutOption[0]))
                        {
                            MiscWindow2Visible = !MiscWindow2Visible;
                        }
                        GUI.color = Color.white;
                        if (MiscWindow3Visible)
                        {
                            GUI.color = Color.green;
                        }
                        else
                        {
                            GUI.color = Color.white;
                        }
                        if (GUILayout.Button("其它菜单", new GUILayoutOption[0]))
                        {
                            MiscWindow3Visible = !MiscWindow3Visible;
                        }
                        //GUILayout.Space(5f);
                        //GUI.color = Color.white;
                        //GUILayout.Label("96辅助游戏论坛 www.steamcom.cn", guistyle3, Array.Empty<GUILayoutOption>());
                        //GUILayout.Label("By.卢大侠", guistyle3, Array.Empty<GUILayoutOption>());
                    }
                    else
                    {
                        GUILayout.Label("请等待游戏加载完毕...", guistyle6, Array.Empty<GUILayoutOption>());
                        //GUILayout.Space(10f);
                        //GUILayout.Label("96辅助游戏论坛 www.steamcom.cn", guistyle3, Array.Empty<GUILayoutOption>());
                    }
                    break;
                case 778:
                    GUI.color = Color.white;
                    if (GUILayout.Button("调试菜单帮助", new GUILayoutOption[0]))
                    {
                        try
                        {
                            Application.OpenURL("https://7daystodie.fandom.com/wiki/Debug_Mode");
                        }
                        catch (Exception ex6)
                        {
                            Debug.LogError("ERROR: " + ex6.Message);
                        }
                    }
                    if (toggleTelnet)
                    {
                        GUI.color = Color.green;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }
                    if (GUILayout.Button("开启Telnet (P: 8099)", new GUILayoutOption[0]))
                    {
                        try
                        {
                            toggleTelnet = !toggleTelnet;
                            if (toggleTelnet)
                            {
                                GamePrefs.GetInt(EnumGamePrefs.TelnetPort);
                                GamePrefs.Set(EnumGamePrefs.TelnetPort, 8099);
                                telnet = new TelnetConsole();
                            }
                            else
                            {
                                telnet.Disconnect();
                                telnet = null;
                            }
                            Debug.Log("Started Telnet Server");
                        }
                        catch (Exception ex7)
                        {
                            Debug.LogError("ERROR: " + ex7.Message);
                        }
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("显示控制台", new GUILayoutOption[0]))
                    {
                        Objects.gameManager.SetConsoleWindowVisible(true);
                        Debug.LogWarning("[Trainer] Opening Console...");
                    }
                    if (cmDm)
                    {
                        GUI.color = Color.green;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }
                    if (GUILayout.Button("创造模式和调试模式", new GUILayoutOption[0]))
                    {
                        cmDm = !cmDm;
                        ToggleCmDm();
                    }
                    if (toggleGodMode)
                    {
                        GUI.color = Color.green;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }
                    if (GUILayout.Button("无敌模式 [F10]", new GUILayoutOption[0]) || godmodeTrigger)
                    {
                        try
                        {
                            toggleGodMode = !Objects.PlayerLocal.IsGodMode.Value;
                            Objects.PlayerLocal.IsGodMode.Value = !Objects.PlayerLocal.IsGodMode.Value;
                            Objects.PlayerLocal.IsNoCollisionMode.Value = Objects.PlayerLocal.IsGodMode.Value;
                            Objects.PlayerLocal.IsFlyMode.Value = Objects.PlayerLocal.IsGodMode.Value;
                            if (Objects.PlayerLocal.IsGodMode.Value)
                            {
                                Objects.PlayerLocal.Buffs.AddBuff("god", -1, true, false, false);
                            }
                            else
                            {
                                Objects.PlayerLocal.Buffs.RemoveBuff("god", true);
                            }
                            godmodeTrigger = false;
                            Debug.Log("Toggled God Mode: " + toggleGodMode.ToString());
                        }
                        catch (Exception ex8)
                        {
                            Debug.LogError("ERROR: " + ex8.Message);
                        }
                    }
                    if (toggleFlyMode)
                    {
                        GUI.color = Color.green;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }
                    if (GUILayout.Button("飞行模式 [F9]", new GUILayoutOption[0]) || flymodeTrigger)
                    {
                        try
                        {
                            toggleFlyMode = !Objects.PlayerLocal.IsFlyMode.Value;
                            Objects.PlayerLocal.IsFlyMode.Value = !Objects.PlayerLocal.IsFlyMode.Value;
                            flymodeTrigger = false;
                            Debug.Log("Toggled Fly Mode: " + toggleFlyMode.ToString());
                        }
                        catch (Exception ex9)
                        {
                            Debug.LogError("ERROR: " + ex9.Message);
                        }
                    }
                    if (toggleInvisible)
                    {
                        GUI.color = Color.green;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }
                    if (GUILayout.Button("隐身模式 [F8]", new GUILayoutOption[0]) || invisibleTrigger)
                    {
                        try
                        {
                            toggleInvisible = !Objects.PlayerLocal.IsSpectator;
                            Objects.PlayerLocal.IsSpectator = !Objects.PlayerLocal.IsSpectator;
                            invisibleTrigger = false;
                            Debug.Log("Toggled Invisible: " + toggleInvisible.ToString());
                        }
                        catch (Exception ex10)
                        {
                            Debug.LogError("ERROR: " + ex10.Message);
                        }
                    }
                    GUI.color = Color.white;
                    GUILayout.BeginVertical("选项", GUI.skin.box, new GUILayoutOption[0]);
                    GUILayout.Space(20f);
                    GUILayout.BeginHorizontal(new GUILayoutOption[]
                    {
                    GUILayout.Width(250f)
                    });
                    Cheat.magicBullet = GUILayout.Toggle(Cheat.magicBullet, "魔法子弹", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new GUILayoutOption[]
                    {
                    GUILayout.Width(250f)
                    });
                    Cheat.noWeaponBob = GUILayout.Toggle(Cheat.noWeaponBob, "武器加强", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    Cheat.aimbot = GUILayout.Toggle(Cheat.aimbot, "自瞄", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new GUILayoutOption[]
                    {
                    GUILayout.Width(250f)
                    });
                    ESP.crosshair = GUILayout.Toggle(ESP.crosshair, "十字准星", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    ESP.fovCircle = GUILayout.Toggle(ESP.fovCircle, "显示自瞄范围", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new GUILayoutOption[]
                    {
                    GUILayout.Width(250f)
                    });
                    ESP.playerBox = GUILayout.Toggle(ESP.playerBox, "玩家方框", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    ESP.playerName = GUILayout.Toggle(ESP.playerName, "玩家名称", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new GUILayoutOption[]
                    {
                    GUILayout.Width(250f)
                    });
                    ESP.playerHealth = GUILayout.Toggle(ESP.playerHealth, "玩家血量", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    ESP.zombieBox = GUILayout.Toggle(ESP.zombieBox, "僵尸方框", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new GUILayoutOption[]
                    {
                    GUILayout.Width(250f)
                    });
                    ESP.zombieName = GUILayout.Toggle(ESP.zombieName, "僵尸名称", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    ESP.zombieHealth = GUILayout.Toggle(ESP.zombieHealth, "僵尸血量", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal(new GUILayoutOption[]
                    {
                    GUILayout.Width(250f)
                    });
                    ESP.playerCornerBox = GUILayout.Toggle(ESP.playerCornerBox, "玩家边缘方框", new GUILayoutOption[]
                    {
                    GUILayout.Width(125f)
                    });
                    ESP.zombieCornerBox = GUILayout.Toggle(ESP.zombieCornerBox, "僵尸边缘方框", new GUILayoutOption[]
                    {
                    GUILayout.Width(130f)
                    });
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    GUI.color = Color.yellow;
                    GUILayout.BeginVertical("传送到航点", GUI.skin.box, new GUILayoutOption[0]);
                    GUILayout.Space(20f);
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[]
                    {
                    GUILayout.MaxWidth(260f)
                    });
                    if (Objects.waypointCollection != null && Objects.waypointCollection.List.Count > 0)
                    {
                        foreach (Waypoint waypoint in Objects.waypointCollection.List)
                        {
                            GUI.color = Color.white;
                            if (GUILayout.Button(waypoint.name, Array.Empty<GUILayoutOption>()))
                            {
                                Objects.PlayerLocal.TeleportToPosition(waypoint.pos, false, null);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    GUI.color = Color.yellow;
                    GUILayout.BeginVertical("传送到僵尸", GUI.skin.box, new GUILayoutOption[0]);
                    GUILayout.Space(20f);
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[]
                    {
                    GUILayout.MaxWidth(260f)
                    });

                    foreach (EntityZombie entityZombie2 in Objects.ZombieObj)
                    {
                        if (entityZombie2 && !(entityZombie2 == Objects.PlayerLocal) && entityZombie2.IsAlive())
                        {
                            float num = Vector3.Distance(Objects.PlayerLocal.position, entityZombie2.position);
                            GUI.color = Color.white;
                            if (GUILayout.Button(entityZombie2.EntityName + " - 距离: " + num.ToString(), Array.Empty<GUILayoutOption>()))
                            {
                                Objects.PlayerLocal.TeleportToPosition(entityZombie2.GetPosition(), false, null);
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                    break;
                case 779:
                    {
                        GUILayout.Space(10f);
                        GUI.color = Color.white;
                        if (GUILayout.Button("秒制作", new GUILayoutOption[0]))
                        {
                            try
                            {
                                foreach (Recipe recipe in CraftingManager.GetAllRecipes())
                                {
                                    CraftingManager.UnlockRecipe(recipe, Objects.PlayerLocal);
                                    recipe.ingredients.Clear();
                                    recipe.craftingTime = 0.1f;
                                }
                            }
                            catch (Exception ex11)
                            {
                                Debug.LogError("ERROR: " + ex11.Message);
                            }
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("获得最大技能", new GUILayoutOption[0]))
                        {
                            try
                            {
                                if (Objects.PlayerLocal)
                                {
                                    foreach (KeyValuePair<int, ProgressionValue> keyValuePair in Objects.PlayerLocal.Progression.ProgressionValues.Dict)
                                    {
                                        keyValuePair.Value.Level = keyValuePair.Value.ProgressionClass.MaxLevel;
                                    }
                                }
                                Debug.Log("...");
                            }
                            catch (Exception ex12)
                            {
                                Debug.LogError("ERROR: " + ex12.Message);
                            }
                        }
                        GUI.color = Color.white;
                        if (GUILayout.Button("获取所有军事物品", new GUILayoutOption[0]))
                        {
                            try
                            {
                                foreach (Item item in from i in itemDB.Item
                                                      where i.Name.Contains("Military") || i.Name.Contains("Ghillie")
                                                      select i)
                                {
                                    GiveItem(item.Name, 1);
                                }
                                Debug.Log("...");
                            }
                            catch (Exception ex13)
                            {
                                Debug.LogError("ERROR: " + ex13.Message);
                            }
                        }
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items = resourceNames.ToArray();
                        int num2 = GUIComboBox.Box(selectedresource, items, "SelectResource");
                        if (num2 != selectedresource && num2 >= 0)
                        {
                            selectedresource = num2;
                            if (selectedresource > 0)
                            {
                                Debug.Log("Giving Resource Item: " + resourceNames.ElementAt(selectedresource));
                                GiveItem("resource" + resourceNames.ElementAt(selectedresource), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items2 = vehicleNames.ToArray();
                        int num3 = GUIComboBox.Box(selectedvehicle, items2, "SelectVehicle");
                        if (num3 != selectedvehicle && num3 >= 0)
                        {
                            selectedvehicle = num3;
                            if (selectedvehicle > 0)
                            {
                                Debug.Log("Giving Vehicle Item: " + vehicleNames.ElementAt(selectedvehicle));
                                GiveItem("vehicle" + vehicleNames.ElementAt(selectedvehicle), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items3 = medicalNames.ToArray();
                        int num4 = GUIComboBox.Box(selectedmedical, items3, "SelectMedical");
                        if (num4 != selectedmedical && num4 >= 0)
                        {
                            selectedmedical = num4;
                            if (selectedmedical > 0)
                            {
                                Debug.Log("Giving Medical Item: " + medicalNames.ElementAt(selectedmedical));
                                string itemname = medicalDic[medicalNames.ElementAt(selectedmedical)];
                                GiveItem(itemname, 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items4 = foodNames.ToArray();
                        int num5 = GUIComboBox.Box(selectedfood, items4, "SelectFood");
                        if (num5 != selectedfood && num5 >= 0)
                        {
                            selectedfood = num5;
                            if (selectedfood > 0)
                            {
                                Debug.Log("Giving Food/Drink Item: " + foodNames.ElementAt(selectedfood));
                                string itemname2 = foodDic[foodNames.ElementAt(selectedfood)];
                                GiveItem(itemname2, 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items5 = miscNames.ToArray();
                        int num6 = GUIComboBox.Box(selectedmisc, items5, "SelectMisc");
                        if (num6 != selectedmisc && num6 >= 0)
                        {
                            selectedmisc = num6;
                            if (selectedmisc > 0)
                            {
                                Debug.Log("Giving Misc Item: " + miscNames.ElementAt(selectedmisc));
                                GiveItem(miscNames.ElementAt(selectedmisc), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items6 = schematicNames.ToArray();
                        int num7 = GUIComboBox.Box(selectedschematic, items6, "SelectSchematic");
                        if (num7 != selectedschematic && num7 >= 0)
                        {
                            selectedschematic = num7;
                            if (selectedschematic > 0)
                            {
                                Debug.Log("Giving Schematic: " + schematicNames.ElementAt(selectedschematic));
                                string itemname3 = schematicDic[schematicNames.ElementAt(selectedschematic)];
                                GiveItem(itemname3, 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items7 = bookNames.ToArray();
                        int num8 = GUIComboBox.Box(selectedbook, items7, "SelectBook");
                        if (num8 != selectedbook && num8 >= 0)
                        {
                            selectedbook = num8;
                            if (selectedbook > 0)
                            {
                                Debug.Log("Giving Book: " + bookNames.ElementAt(selectedbook));
                                GiveItem("book" + bookNames.ElementAt(selectedbook), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items8 = ammoNames.ToArray();
                        int num9 = GUIComboBox.Box(selectedammo, items8, "SelectAmmo");
                        if (num9 != selectedammo && num9 >= 0)
                        {
                            selectedammo = num9;
                            if (selectedammo > 0)
                            {
                                Debug.Log("Giving Ammo: " + ammoNames.ElementAt(selectedammo));
                                GiveItem("ammo" + ammoNames.ElementAt(selectedammo), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items9 = apparelNames.ToArray();
                        int num10 = GUIComboBox.Box(selectedapparel, items9, "SelectApparel");
                        if (num10 != selectedapparel && num10 >= 0)
                        {
                            selectedapparel = num10;
                            if (selectedapparel > 0)
                            {
                                Debug.Log("Giving Apparel: " + apparelNames.ElementAt(selectedapparel));
                                GiveItem("apparel" + apparelNames.ElementAt(selectedapparel), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items10 = armorNames.ToArray();
                        int num11 = GUIComboBox.Box(selectedarmor, items10, "SelectArmor");
                        if (num11 != selectedarmor && num11 >= 0)
                        {
                            selectedarmor = num11;
                            if (selectedarmor > 0)
                            {
                                Debug.Log("Giving Armor: " + armorNames.ElementAt(selectedarmor));
                                GiveItem("armor" + armorNames.ElementAt(selectedarmor), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items11 = weaponNames.ToArray();
                        int num12 = GUIComboBox.Box(selectedweapon, items11, "SelectWeapon");
                        if (num12 != selectedweapon && num12 >= 0)
                        {
                            selectedweapon = num12;
                            if (selectedweapon > 0)
                            {
                                Debug.Log("Giving Weapon: " + weaponNames.ElementAt(selectedweapon));
                                string itemname4 = weaponDic[weaponNames.ElementAt(selectedweapon)];
                                GiveItem(itemname4, 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items12 = loadoutNames.ToArray();
                        int num13 = GUIComboBox.Box(selectedloadout, items12, "SelectLoadout");
                        if (num13 != selectedloadout && num13 >= 0)
                        {
                            selectedloadout = num13;
                            if (selectedloadout > 0)
                            {
                                Debug.Log("Giving Loadout: " + loadoutNames.ElementAt(selectedloadout));
                                GiveItem("Loadout" + loadoutNames.ElementAt(selectedloadout), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items13 = meleeNames.ToArray();
                        int num14 = GUIComboBox.Box(selectedmelee, items13, "SelectMelee");
                        if (num14 != selectedmelee && num14 >= 0)
                        {
                            selectedmelee = num14;
                            if (selectedmelee > 0)
                            {
                                Debug.Log("Giving Melee Item: " + meleeNames.ElementAt(selectedmelee));
                                GiveItem("melee" + meleeNames.ElementAt(selectedmelee), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                        GUILayout.FlexibleSpace();
                        string[] items14 = questNames.ToArray();
                        int num15 = GUIComboBox.Box(selectedquest, items14, "SelectQuest");
                        if (num15 != selectedquest && num15 >= 0)
                        {
                            selectedquest = num15;
                            if (selectedquest > 0)
                            {
                                Debug.Log("Giving Quest Item: " + questNames.ElementAt(selectedquest));
                                GiveItem("quest" + questNames.ElementAt(selectedquest), 1);
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10f);
                        break;
                    }
                case 780:
                    GUILayout.Space(10f);
                    GUI.color = Color.white;
                    if (GUILayout.Button("生成空投", new GUILayoutOption[0]))
                    {
                        try
                        {
                            DoCommand("spawnairdrop", "");
                            Debug.Log("Spawned Airdrop...");
                        }
                        catch (Exception ex14)
                        {
                            Debug.LogError("ERROR: " + ex14.Message);
                        }
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("生成供给箱", new GUILayoutOption[0]))
                    {
                        try
                        {
                            DoCommand("spawnsupplycrate", "");
                            Debug.Log("Spawned Supply Crate...");
                        }
                        catch (Exception ex15)
                        {
                            Debug.LogError("ERROR: " + ex15.Message);
                        }
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("生成僵尸侦察兵", new GUILayoutOption[0]))
                    {
                        try
                        {
                            DoCommand("spawnscouts", "");
                            Debug.Log("Spawned Zombie Scouts...");
                        }
                        catch (Exception ex16)
                        {
                            Debug.LogError("ERROR: " + ex16.Message);
                        }
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("生成僵尸部落", new GUILayoutOption[0]))
                    {
                        try
                        {
                            DoCommand("spawnwh", "");
                            Debug.Log("Spawned Zombie Horde...");
                        }
                        catch (Exception ex17)
                        {
                            Debug.LogError("ERROR: " + ex17.Message);
                        }
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("天气 - " + weatherPresets[currentWeatherPreset], new GUILayoutOption[0]))
                    {
                        try
                        {
                            currentWeatherPreset++;
                            if (currentWeatherPreset > 5)
                            {
                                currentWeatherPreset = 0;
                            }
                            DoCommand("spectrum", weatherPresets[currentWeatherPreset]);
                            switch (currentWeatherPreset)
                            {
                                case 0:
                                    DoCommand("weather", "Fog 0");
                                    DoCommand("weather", "Clouds 0");
                                    DoCommand("weather", "Rain 0");
                                    DoCommand("weather", "Wet 0");
                                    DoCommand("weather", "Snow 0");
                                    DoCommand("weather", "SnowFall 0");
                                    break;
                                case 2:
                                    DoCommand("weather", "Fog 0.25");
                                    DoCommand("weather", "FogColor 1");
                                    break;
                                case 3:
                                case 4:
                                    DoCommand("weather", "Fog 0.1");
                                    DoCommand("weather", "Clouds 1");
                                    DoCommand("weather", "Rain 1");
                                    DoCommand("weather", "Wet 1");
                                    DoCommand("weather", "Snow 0");
                                    DoCommand("weather", "SnowFall 0");
                                    break;
                                case 5:
                                    DoCommand("weather", "Fog 0.1");
                                    DoCommand("weather", "Rain 0");
                                    DoCommand("weather", "Wet 0");
                                    DoCommand("weather", "Clouds 1");
                                    DoCommand("weather", "Snow 1");
                                    DoCommand("weather", "SnowFall 1");
                                    break;
                            }
                            Debug.Log("Cycled Weather: " + weatherPresets[currentWeatherPreset]);
                        }
                        catch (Exception ex18)
                        {
                            Debug.LogError("ERROR: " + ex18.Message);
                        }
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("刷新饥饿/饥渴", new GUILayoutOption[0]))
                    {
                        try
                        {
                            DoCommand("food", "100");
                            DoCommand("thirsty", "100");
                            Debug.Log("Refreshed Hunger/Thirst...");
                        }
                        catch (Exception ex19)
                        {
                            Debug.LogError("ERROR: " + ex19.Message);
                        }
                    }
                    if (toggleTrees)
                    {
                        GUI.color = Color.green;
                    }
                    else
                    {
                        GUI.color = Color.white;
                    }
                    if (GUILayout.Button("隐藏树木", new GUILayoutOption[0]))
                    {
                        try
                        {
                            toggleTrees = !toggleTrees;
                            DoCommand("trees", "");
                            Debug.Log("Toggled Tree's: " + toggleTrees.ToString());
                        }
                        catch (Exception ex20)
                        {
                            Debug.LogError("ERROR: " + ex20.Message);
                        }
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("获取工作站材料", new GUILayoutOption[0]))
                    {
                        try
                        {
                            for (int k = 0; k < 5; k++)
                            {
                                DoCommand("wsmats", " " + k.ToString() + " 100");
                            }
                            Debug.Log("Gave Workstation Materials...");
                        }
                        catch (Exception ex21)
                        {
                            Debug.LogError("ERROR: " + ex21.Message);
                        }
                    }
                    GUI.color = Color.white;
                    if (GUILayout.Button("去除伤害", new GUILayoutOption[0]))
                    {
                        try
                        {
                            DoCommand("debuff", "buffInfectionCatch");
                            DoCommand("debuff", "buffAbrasionCatch");
                            DoCommand("debuff", "buffLegSprainedCHTrigger");
                            DoCommand("debuff", "buffLegBroken");
                            DoCommand("debuff", "buffArmSprainedCHTrigger");
                            DoCommand("debuff", "buffArmBroken");
                            DoCommand("debuff", "buffFatiguedTrigger");
                            DoCommand("debuff", "buffInjuryBleedingTwo");
                            DoCommand("debuff", "buffLaceration");
                            DoCommand("debuff", "buffInjuryStunned01CHTrigger");
                            DoCommand("debuff", "buffInjuryStunned01Cooldown");
                            DoCommand("debuff", "buffInjuryConcussion");
                            DoCommand("debuff", "buffInjuryBleedingOne");
                            DoCommand("debuff", "buffInjuryBleedingBarbedWire");
                            DoCommand("debuff", "buffInjuryBleeding");
                            DoCommand("debuff", "buffInjuryBleedingParticle");
                            DoCommand("debuff", "buffInjuryAbrasion");
                            DoCommand("debuff", "buffPlayerFallingDamage");
                            DoCommand("debuff", "buffFatigued");
                            DoCommand("debuff", "buffStayDownKO");
                            DoCommand("debuff", "buffInjuryCrippled01");
                            DoCommand("debuff", "buffInjuryUnconscious");
                            DoCommand("debuff", "buffBatterUpSlowDown");
                            DoCommand("debuff", "buffRadiation03");
                            DoCommand("debuff", "buffNearDeathTrauma");
                            DoCommand("debuff", "buffDysenteryCatchFood");
                            DoCommand("debuff", "buffDysenteryCatchDrink");
                            DoCommand("debuff", "buffDysenteryMain");
                            DoCommand("debuff", "buffIllPneumonia00");
                            DoCommand("debuff", "buffIllPneumonia01");
                            DoCommand("debuff", "buffInfectionCatch");
                            DoCommand("debuff", "buffInfectionMain");
                            DoCommand("debuff", "buffInfection04");
                            DoCommand("debuff", "buffStatusHungry01");
                            DoCommand("debuff", "buffStatusHungry02");
                            DoCommand("debuff", "buffStatusHungry03");
                            DoCommand("debuff", "buffStatusThirsty01");
                            DoCommand("debuff", "buffStatusThirsty02");
                            DoCommand("debuff", "buffStatusThirsty03");
                            Debug.Log("...");
                        }
                        catch (Exception ex22)
                        {
                            Debug.LogError("ERROR: " + ex22.Message);
                        }
                    }
                    GUILayout.Space(10f);
                    break;
            }
            GUI.DragWindow();
        }

        private string MakeEnable(string keycode, string label, bool toggle)
        {
            string text = "<color=yellow>" + keycode + "</color>";
            string text2 = toggle ? "<color=green>ON</color>" : "<color=red>OFF</color>";
            return string.Concat(new string[]
            {
                text,
                " ",
                label,
                " ",
                text2
            });
        }

        private void ToggleCmDm()
        {
            GameStats.Set(EnumGameStats.ShowSpawnWindow, cmDm);
            GameStats.Set(EnumGameStats.IsCreativeMenuEnabled, cmDm);
            GamePrefs.Set(EnumGamePrefs.DebugMenuEnabled, cmDm);
        }

        private void GiveItem(string itemname, int amount = 1)
        {
            string text = string.Concat(new string[]
            {
                "giveself ",
                itemname,
                " 6 ",
                amount.ToString(),
                " true"
            });
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(text, null);
                return;
            }
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(text), false);
        }

        private void DoCommand(string command, string args = "")
        {
            string text = command + " " + args;
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(text, null);
                return;
            }
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(text), false);
        }

        private void BuildLists()
        {
            schematicNames.Add("Select Schematic");
            foreach (Item item in from i in itemDB.Item
                                  where i.Name.EndsWith("Schematic")
                                  select i)
            {
                string text = item.Name.Replace("Schematic", "");
                if (text.StartsWith("mod"))
                {
                    text = text.Replace("mod", "");
                }
                else if (text.StartsWith("melee"))
                {
                    text = text.Replace("melee", "");
                }
                else if (text.StartsWith("gun"))
                {
                    text = text.Replace("gun", "");
                }
                else if (text.StartsWith("armor"))
                {
                    text = text.Replace("armor", "");
                }
                else if (text.StartsWith("resource"))
                {
                    text = text.Replace("resource", "");
                }
                else if (text.StartsWith("thrown"))
                {
                    text = text.Replace("thrown", "");
                }
                else if (text.StartsWith("medical"))
                {
                    text = text.Replace("medical", "");
                }
                else if (text.StartsWith("tool"))
                {
                    text = text.Replace("tool", "");
                }
                else if (text.StartsWith("vehicle"))
                {
                    text = text.Replace("vehicle", "");
                }
                else if (text.StartsWith("drink"))
                {
                    text = text.Replace("drink", "");
                }
                else if (text.StartsWith("food"))
                {
                    text = text.Replace("food", "");
                }
                else if (text.StartsWith("planted"))
                {
                    text = text.Replace("planted", "");
                }
                schematicDic.Add(text, item.Name);
                schematicNames.Add(text);
            }
            medicalNames.Add("Select Medical Item");
            foreach (Item item2 in from w in itemDB.Item
                                   where (w.Name.StartsWith("medical") || w.Name.StartsWith("drug")) && !w.Name.EndsWith("Schematic")
                                   select w)
            {
                string text2 = item2.Name;
                if (text2.StartsWith("medical"))
                {
                    text2 = text2.Replace("medical", "");
                }
                else if (text2.StartsWith("drug"))
                {
                    text2 = text2.Replace("drug", "");
                }
                medicalDic.Add(text2, item2.Name);
                medicalNames.Add(text2);
            }
            foodNames.Add("Select Food/Drink Item");
            foreach (Item item3 in from w in itemDB.Item
                                   where (w.Name.StartsWith("food") || w.Name.StartsWith("drink")) && !w.Name.EndsWith("Schematic")
                                   select w)
            {
                string text3 = item3.Name;
                if (text3.StartsWith("food"))
                {
                    text3 = text3.Replace("food", "");
                }
                else if (text3.StartsWith("drink"))
                {
                    text3 = text3.Replace("drink", "");
                }
                foodDic.Add(text3, item3.Name);
                foodNames.Add(text3);
            }
            resourceNames.Add("Select Resource");
            foreach (Item item4 in from w in itemDB.Item
                                   where w.Name.StartsWith("resource") && !w.Name.EndsWith("Schematic")
                                   select w)
            {
                resourceNames.Add(item4.Name.Replace("resource", ""));
            }
            miscNames.Add("Select Misc. Item");
            foreach (Item item5 in from w in itemDB.Item
                                   where !w.Name.StartsWith("ammo") && !w.Name.StartsWith("apparel") && !w.Name.StartsWith("armor") && !w.Name.StartsWith("book") && !w.Name.StartsWith("gun") && !w.Name.StartsWith("thrown") && !w.Name.StartsWith("Loadout") && !w.Name.StartsWith("melee") && !w.Name.StartsWith("quest") && !w.Name.StartsWith("resource") && !w.Name.StartsWith("vehicle") && !w.Name.StartsWith("medical") && !w.Name.StartsWith("drug") && !w.Name.StartsWith("drink") && !w.Name.StartsWith("food") && !w.Name.EndsWith("Schematic")
                                   select w)
            {
                miscNames.Add(item5.Name);
            }
            bookNames.Add("Select Book");
            foreach (Item item6 in from b in itemDB.Item
                                   where b.Name.StartsWith("book")
                                   select b)
            {
                bookNames.Add(item6.Name.Replace("book", ""));
            }
            ammoNames.Add("Select Ammo");
            foreach (Item item7 in from a in itemDB.Item
                                   where a.Name.StartsWith("ammo")
                                   select a)
            {
                ammoNames.Add(item7.Name.Replace("ammo", ""));
            }
            apparelNames.Add("Select Apparel");
            foreach (Item item8 in from a in itemDB.Item
                                   where a.Name.StartsWith("apparel")
                                   select a)
            {
                apparelNames.Add(item8.Name.Replace("apparel", ""));
            }
            armorNames.Add("Select Armor");
            foreach (Item item9 in from a in itemDB.Item
                                   where a.Name.StartsWith("armor") && !a.Name.EndsWith("Schematic")
                                   select a)
            {
                armorNames.Add(item9.Name.Replace("armor", ""));
            }
            weaponNames.Add("Select Weapon");
            foreach (Item item10 in from w in itemDB.Item
                                    where (w.Name.StartsWith("gun") || w.Name.StartsWith("thrown")) && !w.Name.EndsWith("Schematic")
                                    select w)
            {
                string text4 = item10.Name;
                if (text4.StartsWith("gun"))
                {
                    text4 = text4.Replace("gun", "");
                }
                else if (text4.StartsWith("thrown"))
                {
                    text4 = text4.Replace("thrown", "");
                }
                weaponDic.Add(text4, item10.Name);
                weaponNames.Add(text4);
            }
            loadoutNames.Add("Select Loadout");
            foreach (Item item11 in from w in itemDB.Item
                                    where w.Name.StartsWith("Loadout") && !w.Name.EndsWith("Schematic")
                                    select w)
            {
                loadoutNames.Add(item11.Name.Replace("Loadout", ""));
            }
            meleeNames.Add("Select Melee Item");
            foreach (Item item12 in from w in itemDB.Item
                                    where w.Name.StartsWith("melee") && !w.Name.EndsWith("Schematic")
                                    select w)
            {
                meleeNames.Add(item12.Name.Replace("melee", ""));
            }
            questNames.Add("Select Quest Item");
            foreach (Item item13 in from w in itemDB.Item
                                    where w.Name.StartsWith("quest") && !w.Name.EndsWith("Schematic")
                                    select w)
            {
                questNames.Add(item13.Name.Replace("quest", ""));
            }
            vehicleNames.Add("Select Vehicle Item");
            foreach (Item item14 in from w in itemDB.Item
                                    where w.Name.StartsWith("vehicle") && !w.Name.EndsWith("Schematic")
                                    select w)
            {
                vehicleNames.Add(item14.Name.Replace("vehicle", ""));
            }
        }

        public static void InsertHarmonyPatches()
        {
            try
            {
                Debug.Log("Inserting Hooks...");
                Harmony harmony = new Harmony("wh0am15533.trainer");
                MethodInfo methodInfo = AccessTools.Method(typeof(Recipe), "CanCraft", null, null);
                MethodInfo methodInfo2 = AccessTools.Method(typeof(TrainerMenu), "CanCraft", null, null);
                harmony.Patch(methodInfo, new HarmonyMethod(methodInfo2), null, null, null);
                MethodInfo methodInfo3 = AccessTools.Method(typeof(GameManager), "isAnyCursorWindowOpen", null, null);
                MethodInfo methodInfo4 = AccessTools.Method(typeof(TrainerMenu), "isAnyCursorWindowOpen", null, null);
                harmony.Patch(methodInfo3, new HarmonyMethod(methodInfo4), null, null, null);
                Debug.Log("Runtime Hooks's Applied");
            }
            catch (Exception ex)
            {
                Debug.LogError("FAILED to Apply Hooks's! Error: " + ex.Message);
            }
        }

        [HarmonyPrefix]
        public static bool CanCraft(ref bool __result, IList<ItemStack> _itemStack, EntityAlive _ea)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        public static bool isAnyCursorWindowOpen(ref bool __result)
        {
            if (toggleCursor)
            {
                __result = true;
                return false;
            }
            return true;
        }

        public static TrainerMenu instance = null;

        public static string baseDirectory = Environment.CurrentDirectory;

        private static Dictionary<string, int> sceneTracker = new Dictionary<string, int>();

        public static bool trainerReInitFlag = false;

        public static string sceneName = "";

        public static int sceneIndex = 0;

        public static BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static BindingFlags fieldBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField;

        public static BindingFlags propertyBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty;

        private Rect MainWindow;

        private bool MainWindowVisible = true;

        private Rect MiscWindow1;

        private bool MiscWindowVisible;

        private Rect MiscWindow2;

        private bool MiscWindow2Visible;

        private Rect MiscWindow3;

        private bool MiscWindow3Visible;

        private static bool toggleLogConsole = false;

        private static bool toggleCursor = false;

        private bool pause;

        private bool forceWindowMode;

        private int origHeight;

        private int origWidth;

        private static bool toggleSceneDebugger = false;

        private static bool toggleTelnet = false;

        private static bool toggleGodMode = false;

        private static bool toggleFlyMode = false;

        private static bool toggleInvisible = false;

        private static bool toggleTrees = false;

        private static bool killZombiesTrigger = false;

        private static bool godmodeTrigger = false;

        private static bool flymodeTrigger = false;

        private static bool invisibleTrigger = false;

        private static TelnetConsole telnet = null;

        private static string[] weatherPresets = new string[]
        {
            "Biome",
            "BloodMoon",
            "Foggy",
            "Rainy",
            "Stormy",
            "Snowy"
        };

        private static int currentWeatherPreset = 0;

        private static Items itemDB = null;

        private static int selectedschematic = 0;

        private static List<string> schematicNames = new List<string>();

        private static Dictionary<string, string> schematicDic = new Dictionary<string, string>();

        private static int selectedbook = 0;

        private static List<string> bookNames = new List<string>();

        private static int selectedammo = 0;

        private static List<string> ammoNames = new List<string>();

        private static int selectedapparel = 0;

        private static List<string> apparelNames = new List<string>();

        private static int selectedarmor = 0;

        private static List<string> armorNames = new List<string>();

        private static int selectedweapon = 0;

        private static List<string> weaponNames = new List<string>();

        private static Dictionary<string, string> weaponDic = new Dictionary<string, string>();

        private static int selectedloadout = 0;

        private static List<string> loadoutNames = new List<string>();

        private static int selectedmelee = 0;

        private static List<string> meleeNames = new List<string>();

        private static int selectedquest = 0;

        private static List<string> questNames = new List<string>();

        private static int selectedresource = 0;

        private static List<string> resourceNames = new List<string>();

        private static int selectedvehicle = 0;

        private static List<string> vehicleNames = new List<string>();

        private static int selectedmedical = 0;

        private static List<string> medicalNames = new List<string>();

        private static Dictionary<string, string> medicalDic = new Dictionary<string, string>();

        private static int selectedfood = 0;

        private static List<string> foodNames = new List<string>();

        private static Dictionary<string, string> foodDic = new Dictionary<string, string>();

        private static int selectedmisc = 0;

        private static List<string> miscNames = new List<string>();

        private Vector2 scrollPosition;

        private bool cmDm;
    }
}
