using EasyAntiCheat.Client;
using EasyAntiCheat.Client.Hydra;
using EasyAntiCheat.Server.Hydra;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Hacker
{
    public static class Loader
    {
        public static void Init()
        {
            Loader.Load = new GameObject("TestTrainer");
            Loader.Load.transform.parent = null;
            Loader.Load.AddComponent<Cheat>();
            Loader.Load.AddComponent<Objects>();
            Loader.Load.AddComponent<ESP>();
            Loader.Load.AddComponent<TrainerMenu>();
            Loader.scDebugger = Loader.Load.AddComponent<SceneDebugger>();
            Loader.scDebugger.enabled = false;
            UnityEngine.Object.DontDestroyOnLoad(Loader.Load);
        }

        public static void InsertHarmonyPatches()
        {
            try
            {
                UnityEngine.Debug.Log("Inserting Hooks...");
                new Harmony("wh0am15533.trainer");
                AccessTools.Method(typeof(Runtime), "PollStatus", null, null);
                AccessTools.Method(typeof(Loader), "PollStatus", null, null);
                AccessTools.Method(typeof(Runtime), "OnIntegrityViolation", null, null);
                AccessTools.Method(typeof(Loader), "OnIntegrityViolation", null, null);
                AccessTools.Method(typeof(Runtime), "OnHostValidation", null, null);
                AccessTools.Method(typeof(Loader), "OnHostValidation", null, null);
                UnityEngine.Debug.Log("Runtime Hooks's Applied");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("FAILED to Apply Hooks's! Error: " + ex.Message);
            }
        }

        [HarmonyPrefix]
        public static bool PollStatus()
        {
            return !Loader.pollStatusFired;
        }

        [HarmonyPrefix]
        public static bool OnIntegrityViolation(ref uint __result, ClientIntegrityViolationType ViolationType, string ViolationCause, string DefaultMessage, IntPtr CallbackParameter)
        {
            __result = 0U;
            typeof(Runtime).GetField("<Integrity>k__BackingField", Loader.bindflags).SetValue(typeof(Runtime), new IntegrityDescriptor(false, "Trainer Override"));
            StateChangedEventArgs stateChangedEventArgs = (StateChangedEventArgs)typeof(StateChangedEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single<ConstructorInfo>().Invoke(new object[]
            {
                1,
                "Trainer Override"
            });
            EventHandler<StateChangedEventArgs> eventHandler = (EventHandler<StateChangedEventArgs>)typeof(Runtime).GetField("onStateChanged", Loader.bindflags).GetValue(typeof(Runtime));
            return false;
        }

        [HarmonyPrefix]
        public static bool OnHostValidation(ref bool __result, HostValidationResult Result, string Message, IntPtr CallbackParameter)
        {
            UnityEngine.Debug.LogWarning("OnHostValidation Fired!");
            __result = true;
            UnityEngine.Debug.LogWarning("HostValidationResult: " + Result.ToString() + " Message: " + Message);
            typeof(Runtime).GetField("<HostValidation>k__BackingField", Loader.bindflags).SetValue(typeof(Runtime), 0);
            StateChangedEventArgs stateChangedEventArgs = (StateChangedEventArgs)typeof(StateChangedEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single<ConstructorInfo>().Invoke(new object[]
            {
                2,
                "Trainer Override"
            });
            EventHandler<StateChangedEventArgs> eventHandler = (EventHandler<StateChangedEventArgs>)typeof(Runtime).GetField("onStateChanged", Loader.bindflags).GetValue(typeof(Runtime));
            return false;
        }

        [HarmonyPrefix]
        public static bool StateChangedEventArgs(StateChangeType stateChangeType, string message)
        {
            return true;
        }

        [HarmonyPrefix]
        public static bool UserStatusHandler(ClientStatusUpdate<ClientInfo> _userStatus)
        {
            UnityEngine.Debug.LogWarning("UserStatusHandler Fired! Require AC: " + _userStatus.Client.requiresAntiCheat.ToString());
            return true;
        }

        public static GameObject Load;

        private static BindingFlags bindflags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static bool pollStatusFired = false;

        public static SceneDebugger scDebugger = null;
    }
}
