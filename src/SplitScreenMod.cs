using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

[assembly: MelonInfo(typeof(AWJSplitScreen.SplitScreenMod), "AWJ Split Screen", "0.2.4", "TinyTaru", "https://github.com")]
[assembly: MelonGame("Fire Totem Games", "A Webbing Journey")]

namespace AWJSplitScreen
{
    public sealed class SplitScreenMod : MelonMod
    {
        internal static bool SkipCallbackContextPatches;

        // Shared config/state for patches
        internal static bool P2UseGamepad = true;
        internal static int P2GamepadIndex = 1;        // second controller
        internal static float P2Deadzone = 0.15f;
        internal static float P2TriggerThreshold = 0.35f;
        internal static bool FilterP1FromP2Gamepad = true;
        internal static float P2CameraDistance = 5.6f;
        internal static bool DebugCameraInput = false;

        internal static bool P2ShootHeld;              // computed each frame & from WebController.Update prefix
        internal static bool P2JumpPressed;            // set in OnUpdate, consumed in FixedUpdate
        internal static bool InP2WebContext;           // one-shot actions
        internal static Transform P2InputTransform;
        internal static Camera P2Camera;

        internal static FieldInfo BodyMove_MoveInputField;
        internal static FieldInfo BodyMove_MoveVectorField;
        internal static FieldInfo BodyMove_JumpInputField;
        internal static MethodInfo BodyMove_InitializeJumpMethod;

        private const string Cat = "AWJ_SplitScreen";
        private static MelonPreferences_Category _prefs;
        private static MelonPreferences_Entry<bool> _enabled;
        private static MelonPreferences_Entry<string> _splitMode;
        private static MelonPreferences_Entry<bool> _spawnSecondSpider;
        private static MelonPreferences_Entry<float> _p2LookSpeed;
        private static MelonPreferences_Entry<bool> _p2KeepPlayerTag;

        // Controller prefs
        private static MelonPreferences_Entry<bool> _p2UseGamepadPref;
        private static MelonPreferences_Entry<int> _p2GamepadIndexPref;
        private static MelonPreferences_Entry<float> _p2DeadzonePref;
        private static MelonPreferences_Entry<float> _p2TriggerThresholdPref;
        private static MelonPreferences_Entry<bool> _filterP1FromP2PadPref;
        private static MelonPreferences_Entry<float> _p2CameraDistancePref;
        private static MelonPreferences_Entry<bool> _debugCameraInputPref;

        // P2 keyboard fallback keys
        private const string P2JumpKeyProp = "spaceKey";
        private const KeyCode P2JumpKeyFallback = KeyCode.Space;
        private const string P2ShootKeyProp = "uKey";
        private const KeyCode P2ShootKeyFallback = KeyCode.U;
        private const string P2DeleteKeyProp = "oKey";
        private const KeyCode P2DeleteKeyFallback = KeyCode.O;
        private const string P2AttachKeyProp = "pKey";
        private const KeyCode P2AttachKeyFallback = KeyCode.P;
        private const string P2ReleaseKeyProp = "rightCtrlKey";
        private const KeyCode P2ReleaseKeyFallback = KeyCode.RightControl;

        private Camera _camLeftOrTop;
        private Camera _camRightOrBottom;

        private GameObject _p1Spider;
        internal static GameObject _p2Spider;
        private Transform _p1InputTransform;

        private Vector3 _p2CamDir;        // direction from pivot to camera (normalized)
        private Vector3 _p2SmoothUp;      // smoothed spider surface up (lerped each frame)
        private float _p2CamDistance;
        private bool _p2CamRigInited;
        private float _p2CamBaseHeight;   // local Y offset (up from InputTransform)
        private float _p2CamBaseDist;     // horizontal distance behind
        private bool _p2CamBasePoseCached;
        private int _p2CamRecaptureFrame;  // frame at which to re-read P1 camera distance
        private float _p2CamDebugNextLog;

        private Component _webController;
        private P2WebManager _p2WebManager;

        public override void OnInitializeMelon()
        {
            _prefs = MelonPreferences.CreateCategory(Cat);

            _enabled = _prefs.CreateEntry("Enabled", true, "Enable split-screen");
            _splitMode = _prefs.CreateEntry("SplitMode", "Vertical", "Split mode: Vertical or Horizontal");
            _spawnSecondSpider = _prefs.CreateEntry("SpawnSecondSpider", true, "Clone PlayerSpider to create Player 2 (experimental)");
            _p2LookSpeed = _prefs.CreateEntry("P2_LookSpeed", 90.0f, "P2 camera yaw speed (deg/sec) using N/M keys");
            _p2KeepPlayerTag = _prefs.CreateEntry("P2_KeepPlayerTag", false, "Keep Tag=Player on P2 clone (may confuse single-player code)");

            _p2UseGamepadPref = _prefs.CreateEntry("P2_UseGamepad", true, "Allow P2 to use a gamepad");
            _p2GamepadIndexPref = _prefs.CreateEntry("P2_GamepadIndex", 1, "Which gamepad index to use for P2 (0=first pad, 1=second pad, etc.)");
            _p2DeadzonePref = _prefs.CreateEntry("P2_GamepadDeadzone", 0.15f, "Deadzone for sticks");
            _p2TriggerThresholdPref = _prefs.CreateEntry("P2_TriggerThreshold", 0.35f, "Trigger threshold for shooting");
            _filterP1FromP2PadPref = _prefs.CreateEntry("FilterP1FromP2Gamepad", true, "Prevent P1 from reacting to P2's gamepad (recommended for 2-controller play)");
            _p2CameraDistancePref = _prefs.CreateEntry("P2_CameraDistance", 8.0f, "P2 third-person camera distance");
            _debugCameraInputPref = _prefs.CreateEntry("DebugCameraInput", false, "Enable camera input debug logs");

            ApplyPrefsToStatics();

            InputCompat.Init(LoggerInstance);
            InstallHarmonyPatches();

            SceneManager.sceneLoaded += (_, __) => MelonCoroutines.Start(DeferredSetup());

            LoggerInstance.Msg("AWJ Split Screen + P2 Inject v0.2.2 loaded.");
            LoggerInstance.Msg("F9 split, F10 orientation | P2 Move: IJKL or Gamepad LStick | P2 Look: N/M or RStickX | P2 Jump: A | P2 Web: U/RT shoot, P/LT attach, O/B delete, RightCtrl/RB release.");
            LoggerInstance.Msg("Tip: If both controllers still move P1, ensure FilterP1FromP2Gamepad=true and P2_GamepadIndex is the second pad (usually 1).");
        }

        private void ApplyPrefsToStatics()
        {
            P2UseGamepad = _p2UseGamepadPref.Value;
            P2GamepadIndex = _p2GamepadIndexPref.Value;
            P2Deadzone = _p2DeadzonePref.Value;
            P2TriggerThreshold = _p2TriggerThresholdPref.Value;
            FilterP1FromP2Gamepad = _filterP1FromP2PadPref.Value;
            P2CameraDistance = Mathf.Clamp(_p2CameraDistancePref.Value, 1.0f, 14f);
            DebugCameraInput = _debugCameraInputPref.Value;
        }

        public override void OnDeinitializeMelon()
        {
        }

        private void InstallHarmonyPatches()
        {
            var h = new HarmonyLib.Harmony("AWJ.SplitScreen.P2Inject.v022");
            SkipCallbackContextPatches = IsIl2CppRuntime();
            if (SkipCallbackContextPatches)
                LoggerInstance.Warning("Detected IL2CPP runtime. CallbackContext patches are disabled for stability.");

            // BodyMovement patches
            try
            {
                var bodyMoveType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                if (bodyMoveType != null)
                {
                    BodyMove_MoveInputField = FindBestMoveInputField(bodyMoveType);
                    BodyMove_MoveVectorField = FindFieldByName(bodyMoveType, "moveVector");
                    BodyMove_JumpInputField = FindFieldByName(bodyMoveType, "jumpInput");
                    BodyMove_InitializeJumpMethod = bodyMoveType.GetMethod("InitializeJump",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    if (BodyMove_InitializeJumpMethod != null)
                        h.Patch(BodyMove_InitializeJumpMethod,
                            prefix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.InitializeJump_Prefix)));

                    var performJumping = bodyMoveType.GetMethod("PerformJumping",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (performJumping != null)
                        h.Patch(performJumping,
                            prefix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.PerformJumping_Prefix)));

                    var fixedUpdate = AccessTools.Method(bodyMoveType, "FixedUpdate");
                    if (fixedUpdate != null)
                        h.Patch(fixedUpdate,
                            prefix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.FixedUpdate_Prefix)));

                    var npcWalk = AccessTools.Method(bodyMoveType, "NpcWalk");
                    if (npcWalk != null)
                        h.Patch(npcWalk,
                            postfix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.NpcWalk_Postfix)));

                    int callbackCount = 0;
                    if (!SkipCallbackContextPatches)
                    {
                        var bms = bodyMoveType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        for (int i = 0; i < bms.Length; i++)
                        {
                            var m = bms[i];
                            var ps = m.GetParameters();
                            if (ps.Length != 1) continue;
                            var pt = ps[0].ParameterType;
                            var pname = pt != null ? pt.Name : "";
                            if (string.Equals(pname, "CallbackContext", StringComparison.Ordinal) ||
                                (pt != null && pt.FullName != null && pt.FullName.IndexOf("CallbackContext", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                h.Patch(m, prefix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.CallbackContextFilter_Prefix)));
                                callbackCount++;
                            }
                        }
                    }

                    LoggerInstance.Msg("Patched BodyMovement: FixedUpdate + NpcWalk + CallbackContext filters=" + callbackCount + ".");
                    LoggerInstance.Msg("BodyMovement moveVector field: " + (BodyMove_MoveVectorField != null ? BodyMove_MoveVectorField.Name : "null"));
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("BodyMovement patch block failed (non-fatal): " + ex);
            }

            // LegController patches
            try
            {
                var legType = AccessTools.TypeByName("_Scripts.Spider.LegController");
                if (legType != null)
                {
                    var legFixed = AccessTools.Method(legType, "FixedUpdate");
                    if (legFixed != null)
                        h.Patch(legFixed, prefix: new HarmonyMethod(typeof(LegControllerPatches), nameof(LegControllerPatches.FixedUpdate_Prefix)));

                    LoggerInstance.Msg("Patched LegController: FixedUpdate (P2 parent-guard suppression).");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("LegController patch block failed (non-fatal): " + ex);
            }

            // WebController patches
            try
            {
                var webType = AccessTools.TypeByName("_Scripts.Singletons.WebController");
                if (webType != null)
                {
                    // Dump all properties and methods for diagnostics
                    var allProps = webType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    LoggerInstance.Msg("[WebController] Properties (" + allProps.Length + "):");
                    for (int i = 0; i < allProps.Length; i++)
                        LoggerInstance.Msg("  prop: " + allProps[i].Name + " -> " + allProps[i].PropertyType.Name);

                    var allMethods = webType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    LoggerInstance.Msg("[WebController] Methods (" + allMethods.Length + "):");
                    for (int i = 0; i < allMethods.Length; i++)
                    {
                        var prms = allMethods[i].GetParameters();
                        string pStr = "";
                        for (int j = 0; j < prms.Length; j++)
                            pStr += (j > 0 ? ", " : "") + prms[j].ParameterType.Name + " " + prms[j].Name;
                        LoggerInstance.Msg("  method: " + allMethods[i].Name + "(" + pStr + ") -> " + allMethods[i].ReturnType.Name);
                    }

                    // NOTE: We intentionally do NOT patch Update/FixedUpdate with a P2ShootHeld setter.
                    // P2WebManager sets context flags only around its own explicit invocations
                    // so P1's targeting is never corrupted.

                    // Try property getter first, then direct method
                    var getStart = AccessTools.PropertyGetter(webType, "WebStartPoint");
                    if (getStart == null) getStart = AccessTools.Method(webType, "get_WebStartPoint");
                    LoggerInstance.Msg("[WebController] get_WebStartPoint: " + (getStart != null) + (getStart != null ? " ret=" + getStart.ReturnType.Name : ""));
                    if (getStart != null)
                    {
                        if (getStart.ReturnType == typeof(Transform))
                            h.Patch(getStart, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebStartPointTransform_Prefix)));
                        else if (getStart.ReturnType == typeof(Vector3))
                            h.Patch(getStart, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebStartPointVector3_Prefix)));
                    }

                    var getDir = AccessTools.PropertyGetter(webType, "WebDirection");
                    if (getDir == null) getDir = AccessTools.Method(webType, "get_WebDirection");
                    LoggerInstance.Msg("[WebController] get_WebDirection: " + (getDir != null) + (getDir != null ? " ret=" + getDir.ReturnType.Name : ""));
                    if (getDir != null && getDir.ReturnType == typeof(Vector3))
                        h.Patch(getDir, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebDirectionVector3_Prefix)));

                    int callbackCount = 0;
                    if (!SkipCallbackContextPatches)
                    {
                        var wms = webType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        for (int i = 0; i < wms.Length; i++)
                        {
                            var m = wms[i];
                            var ps = m.GetParameters();
                            if (ps.Length != 1) continue;

                            var pt = ps[0].ParameterType;
                            var pname = pt != null ? pt.Name : "";
                            if (string.Equals(pname, "CallbackContext", StringComparison.Ordinal) ||
                                (pt != null && pt.FullName != null && pt.FullName.IndexOf("CallbackContext", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                h.Patch(m, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.CallbackContextFilter_Prefix)));
                                callbackCount++;
                            }
                        }
                    }

                    LoggerInstance.Msg("Patched WebController: Update/Fixed + WebStartPoint/WebDirection + " + callbackCount + " CallbackContext filters.");

                    // CheckForWebTarget — separate try/catch because signature is void(float)
                    try
                    {
                        var checkForWebTarget = AccessTools.Method(webType, "CheckForWebTarget");
                        LoggerInstance.Msg("[WebController] CheckForWebTarget: " + (checkForWebTarget != null)
                            + (checkForWebTarget != null ? " ret=" + checkForWebTarget.ReturnType.Name + " params=" + checkForWebTarget.GetParameters().Length : ""));
                        if (checkForWebTarget != null)
                        {
                            h.Patch(checkForWebTarget, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.CheckForWebTarget_Prefix)));
                            LoggerInstance.Msg("Patched WebController.CheckForWebTarget.");
                        }
                    }
                    catch (Exception exCfwt)
                    {
                        LoggerInstance.Warning("CheckForWebTarget patch failed (non-fatal): " + exCfwt);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("WebController patch block failed (non-fatal): " + ex);
            }

            // CameraController InputTransform getter + CallbackContext filter
            try
            {
                var camType = AccessTools.TypeByName("_Scripts.Singletons.CameraController");
                if (camType != null)
                {
                    var getter = AccessTools.PropertyGetter(camType, "InputTransform");
                    if (getter == null) getter = AccessTools.Method(camType, "get_InputTransform");
                    if (getter != null)
                        h.Patch(getter, prefix: new HarmonyMethod(typeof(CameraControllerPatches), nameof(CameraControllerPatches.InputTransform_Prefix)));

                    int callbackCount = 0;
                    if (!SkipCallbackContextPatches)
                    {
                        var cms = camType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        for (int i = 0; i < cms.Length; i++)
                        {
                            var m = cms[i];
                            var ps = m.GetParameters();
                            if (ps.Length != 1) continue;

                            var pt = ps[0].ParameterType;
                            var pname = pt != null ? pt.Name : "";
                            if (string.Equals(pname, "CallbackContext", StringComparison.Ordinal) ||
                                (pt != null && pt.FullName != null && pt.FullName.IndexOf("CallbackContext", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                h.Patch(m, prefix: new HarmonyMethod(typeof(CameraControllerPatches), nameof(CameraControllerPatches.CallbackContextFilter_Prefix)));
                                callbackCount++;
                            }
                        }
                    }

                    LoggerInstance.Msg("Patched CameraController.InputTransform + CallbackContext filters=" + callbackCount + " for P2.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("CameraController patch block failed (non-fatal): " + ex);
            }

            // CameraMouseLook.OnLook patch — blocks P2 gamepad right-stick from reaching P1 camera
            try
            {
                var mouseLookType = AccessTools.TypeByName("_Scripts.Camera.CameraMouseLook");
                if (mouseLookType != null)
                {
                    var onLook = AccessTools.Method(mouseLookType, "OnLook");
                    if (onLook != null)
                    {
                        if (!SkipCallbackContextPatches)
                        {
                            h.Patch(onLook, prefix: new HarmonyMethod(typeof(CameraMouseLookPatches), nameof(CameraMouseLookPatches.OnLook_Prefix)));
                            LoggerInstance.Msg("Patched CameraMouseLook.OnLook to block P2 gamepad input.");
                        }
                        else
                        {
                            LoggerInstance.Warning("Skipped CameraMouseLook.OnLook patch due to IL2CPP callback compatibility mode.");
                        }
                    }
                    else
                    {
                        LoggerInstance.Warning("CameraMouseLook.OnLook not found.");
                    }
                }
                else
                {
                    LoggerInstance.Warning("CameraMouseLook type not found.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("CameraMouseLook patch failed (non-fatal): " + ex);
            }

            // Camera.main getter patch
            try
            {
                var camMainGetter = AccessTools.PropertyGetter(typeof(Camera), "main");
                if (camMainGetter != null)
                {
                    h.Patch(camMainGetter, prefix: new HarmonyMethod(typeof(UnityCameraPatches), nameof(UnityCameraPatches.CameraMain_Prefix)));
                    LoggerInstance.Msg("Patched Camera.main getter for P2.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("Camera.main patch failed (non-fatal): " + ex);
            }
        }

        private static FieldInfo FindFieldByName(Type t, string name)
        {
            try { return t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); }
            catch { return null; }
        }

        private static bool IsIl2CppRuntime()
        {
            try
            {
                var direct = Type.GetType("Il2CppInterop.Runtime.Il2CppClassPointerStore`1, Il2CppInterop.Runtime", false);
                if (direct != null) return true;

                var loaded = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < loaded.Length; i++)
                {
                    var n = loaded[i].GetName().Name;
                    if (string.IsNullOrEmpty(n)) continue;
                    if (n.IndexOf("Il2Cpp", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }
            }
            catch { }

            return false;
        }

        private static FieldInfo FindBestMoveInputField(Type bodyMoveType)
        {
            try
            {
                var fs = bodyMoveType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                for (int i = 0; i < fs.Length; i++)
                {
                    var f = fs[i];
                    if (f.FieldType != typeof(Vector2)) continue;
                    var n = f.Name.ToLowerInvariant();
                    if (n.Contains("move") || n.Contains("input") || n.Contains("dir"))
                        return f;
                }

                for (int i = 0; i < fs.Length; i++)
                    if (fs[i].FieldType == typeof(Vector2))
                        return fs[i];

                return null;
            }
            catch { return null; }
        }

        private System.Collections.IEnumerator DeferredSetup()
        {
            yield return null;
            Teardown();

            if (_enabled != null && !_enabled.Value)
                yield break;

            SetupCameras();
            CacheWebController();

            if (_spawnSecondSpider != null && _spawnSecondSpider.Value)
                SetupSecondSpider();
        }

        public override void OnUpdate()
        {
            ApplyPrefsToStatics();

            if (InputCompat.Down_F9())
            {
                _enabled.Value = !_enabled.Value;
                if (_enabled.Value)
                {
                    MelonCoroutines.Start(DeferredSetup());
                    LoggerInstance.Msg("Split-screen enabled.");
                }
                else
                {
                    Teardown();
                    LoggerInstance.Msg("Split-screen disabled.");
                }
            }

            if (InputCompat.Down_F10())
            {
                _splitMode.Value = string.Equals(_splitMode.Value, "Vertical", StringComparison.OrdinalIgnoreCase)
                    ? "Horizontal"
                    : "Vertical";
                ApplyCameraRects();
                LoggerInstance.Msg("Split mode: " + _splitMode.Value);
            }

            if (_enabled.Value)
            {
                if (InputCompat.IsP2JumpPressedNow(P2UseGamepad, P2GamepadIndex))
                    P2JumpPressed = true;

                // Drive P2's independent web system
                if (_p2WebManager != null)
                    _p2WebManager.DriveInput();
            }
        }

        public override void OnLateUpdate()
        {
            if (_enabled == null || !_enabled.Value)
                return;

            UpdateP2CameraLook();
        }


        private void SetupCameras()
        {
            var main = Camera.main;
            if (main == null)
            {
                var cams = UnityEngine.Object.FindObjectsOfType<Camera>(true);
                if (cams != null && cams.Length > 0) main = cams[0];
            }

            if (main == null)
            {
                LoggerInstance.Warning("No camera found; can't set up split-screen.");
                return;
            }

            _camLeftOrTop = main;

            _camRightOrBottom = UnityEngine.Object.Instantiate(_camLeftOrTop, _camLeftOrTop.transform.parent);
            _camRightOrBottom.name = _camLeftOrTop.name + "_P2";

            var al = _camRightOrBottom.GetComponent<AudioListener>();
            if (al != null) al.enabled = false;

            DisableComponentByTypeName(_camRightOrBottom.gameObject, "Cinemachine.CinemachineBrain");
            DisableCameraDriverBehaviours(_camRightOrBottom.gameObject);

            // DEBUG: dump every component still on the P2 camera clone
            try
            {
                var allComps = _camRightOrBottom.GetComponentsInChildren<Component>(true);
                LoggerInstance.Msg("[P2CamDebug] Components on P2 camera clone (" + allComps.Length + "):");
                for (int i = 0; i < allComps.Length; i++)
                {
                    if (allComps[i] == null) continue;
                    var b = allComps[i] as Behaviour;
                    string en = b != null ? (b.enabled ? "ON" : "OFF") : "n/a";
                    LoggerInstance.Msg("  [" + i + "] " + allComps[i].GetType().FullName + " enabled=" + en + " go=" + allComps[i].gameObject.name);
                }
            }
            catch (Exception ex) { LoggerInstance.Warning("[P2CamDebug] component dump failed: " + ex); }

            P2Camera = _camRightOrBottom;

            ApplyCameraRects();
        }

        private void ApplyCameraRects()
        {
            if (_camLeftOrTop == null) return;

            var vertical = string.Equals(_splitMode.Value, "Vertical", StringComparison.OrdinalIgnoreCase);

            if (vertical)
            {
                _camLeftOrTop.rect = new Rect(0f, 0f, 0.5f, 1f);
                if (_camRightOrBottom != null) _camRightOrBottom.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            }
            else
            {
                _camLeftOrTop.rect = new Rect(0f, 0.5f, 1f, 0.5f);
                if (_camRightOrBottom != null) _camRightOrBottom.rect = new Rect(0f, 0f, 1f, 0.5f);
            }
        }

        private void CacheWebController()
        {
            _webController = null;

            var t = AccessTools.TypeByName("_Scripts.Singletons.WebController");
            if (t == null) return;

            var all = UnityEngine.Object.FindObjectsOfType(t, true);
            if (all != null && all.Length > 0)
                _webController = all[0] as Component;

            LoggerInstance.Msg("WebController cached: " + (_webController != null));
        }

        private void SetupSecondSpider()
        {
            _p1Spider = FindPlayerSpider();
            if (_p1Spider == null)
            {
                LoggerInstance.Warning("Couldn't find PlayerSpider. Make sure you're in a scene where it exists.");
                return;
            }

            _p2Spider = UnityEngine.Object.Instantiate(_p1Spider);
            _p2Spider.name = _p1Spider.name + "_P2";
            _p2Spider.transform.position += new Vector3(3f, 0f, 3f);

            _p1InputTransform = FindChildTransform(_p1Spider.transform, "InputTransform");

            _p2Spider.AddComponent<P2Marker>();

            // Destroy P2's LegController + Animation Rigging components.
            // Binary search confirmed LegController on P2 causes P1's leg glitch.
            // Also destroy RigBuilder/Rig so the IK system doesn't fight with our
            // direct bone-driving replacement (P2LegDriver).
            try
            {
                var legType3 = AccessTools.TypeByName("_Scripts.Spider.LegController");
                var mlcType3 = AccessTools.TypeByName("_Scripts.Spider.MasterLegController");
                if (legType3 != null)
                {
                    var targetF3     = legType3.GetField("target",        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var offsetF3     = legType3.GetField("startingOffset", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var centerF3     = legType3.GetField("center",         BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var opposingF3   = legType3.GetField("opposingLegs",   BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var targetJumpF3 = legType3.GetField("targetJump",     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    // Get params from MasterLegController
                    LayerMask whatIsGround = default;
                    float sphereRadius = 0.1f, rayUpOffset = 0.5f, rayLength = 5f, stepDist = 0.3f;
                    float stepTime = 0.15f, stepHeight = 0.3f, tipHeight = 0.02f, newTargetDist = 0.3f;
                    if (mlcType3 != null)
                    {
                        var p2Mlc = _p2Spider.GetComponentInChildren(mlcType3, true);
                        if (p2Mlc != null)
                        {
                            var wigP = mlcType3.GetProperty("WhatIsGround", BindingFlags.Instance | BindingFlags.Public);
                            if (wigP != null) whatIsGround = (LayerMask)wigP.GetValue(p2Mlc);
                            var srP = mlcType3.GetProperty("SphereCastRadius", BindingFlags.Instance | BindingFlags.Public);
                            if (srP != null) sphereRadius = (float)srP.GetValue(p2Mlc);
                            var ruoP = mlcType3.GetProperty("RayCastOriginUpOffset", BindingFlags.Instance | BindingFlags.Public);
                            if (ruoP != null) rayUpOffset = (float)ruoP.GetValue(p2Mlc);
                            var rlP = mlcType3.GetProperty("RayCastLength", BindingFlags.Instance | BindingFlags.Public);
                            if (rlP != null) rayLength = (float)rlP.GetValue(p2Mlc);
                            var stP = mlcType3.GetProperty("StepTime", BindingFlags.Instance | BindingFlags.Public);
                            if (stP != null) stepTime = (float)stP.GetValue(p2Mlc);
                            var shP = mlcType3.GetProperty("StepHeight", BindingFlags.Instance | BindingFlags.Public);
                            if (shP != null) stepHeight = (float)shP.GetValue(p2Mlc);
                            var thP = mlcType3.GetProperty("TipHeight", BindingFlags.Instance | BindingFlags.Public);
                            if (thP != null) tipHeight = (float)thP.GetValue(p2Mlc);
                            var ntdP = mlcType3.GetProperty("NewTargetDistance", BindingFlags.Instance | BindingFlags.Public);
                            if (ntdP != null) newTargetDist = (float)ntdP.GetValue(p2Mlc);
                            var sdP = mlcType3.GetProperty("StepDistance", BindingFlags.Instance | BindingFlags.Public);
                            if (sdP != null) stepDist = (float)sdP.GetValue(p2Mlc);
                        }
                    }

                    // Get P2's BodyMovement component (for MoveVector and forward anticipation)
                    var bodyMoveType3 = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                    Transform p2BodyTransform = null;
                    Component p2BodyMovement = null;
                    FieldInfo moveVecField = null;
                    if (bodyMoveType3 != null)
                    {
                        p2BodyMovement = _p2Spider.GetComponentInChildren(bodyMoveType3, true) as Component;
                        if (p2BodyMovement != null)
                        {
                            p2BodyTransform = p2BodyMovement.transform;
                            moveVecField = SplitScreenMod.BodyMove_MoveVectorField
                                ?? bodyMoveType3.GetField("moveVector", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        }
                    }

                    // First pass: collect raw LegController data, destroy, create drivers
                    var p2Legs = _p2Spider.GetComponentsInChildren(legType3, true);
                    var drivers = new P2LegDriver[p2Legs.Length];
                    for (int i = 0; i < p2Legs.Length; i++)
                    {
                        var lc = p2Legs[i];
                        var go = (lc as Component).gameObject;
                        var target3     = targetF3?.GetValue(lc) as Transform;
                        var offset3     = offsetF3 != null ? (Vector3)offsetF3.GetValue(lc) : Vector3.zero;
                        var center3     = centerF3?.GetValue(lc) as Transform;
                        var targetJump3 = targetJumpF3?.GetValue(lc) as Transform;

                        UnityEngine.Object.DestroyImmediate(lc);

                        if (target3 != null && p2BodyTransform != null)
                        {
                            var driver = go.AddComponent<P2LegDriver>();
                            driver.Init(target3, offset3, center3, p2BodyTransform,
                                p2BodyMovement, moveVecField,
                                whatIsGround, sphereRadius,
                                rayUpOffset, rayLength, stepDist,
                                stepTime, stepHeight, tipHeight, newTargetDist,
                                targetJump3);
                            drivers[i] = driver;
                        }
                    }

                    // Second pass: wire up opposing-leg pairs (alternate-gait)
                    for (int i = 0; i < drivers.Length; i++)
                    {
                        if (drivers[i] == null) continue;
                        int partner = (i % 2 == 0) ? i + 1 : i - 1;
                        if (partner >= 0 && partner < drivers.Length && drivers[partner] != null)
                            drivers[i].SetOpposingLegs(new P2LegDriver[] { drivers[partner] });
                    }
                    LoggerInstance.Msg("Replaced " + p2Legs.Length + " LegController(s) with P2LegDriver on P2.");
                }

                // Keep RigBuilder/Rig alive — the IK system drives visual bones toward
                // target positions. P2LegDriver updates targets without raycasts.
                // Rebuild RigBuilder so IK binds to P2's own bones.
                Type rigBuilderType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    rigBuilderType = asm.GetType("UnityEngine.Animations.Rigging.RigBuilder");
                    if (rigBuilderType != null) break;
                }
                if (rigBuilderType != null)
                {
                    var rigBuilders = _p2Spider.GetComponentsInChildren(rigBuilderType, true);
                    MethodInfo buildMethod = null;
                    foreach (var m in rigBuilderType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        if (m.Name == "Build" && m.GetParameters().Length == 0) { buildMethod = m; break; }
                    for (int ri = 0; ri < rigBuilders.Length; ri++)
                        buildMethod?.Invoke(rigBuilders[ri], null);
                    LoggerInstance.Msg("Rebuilt " + rigBuilders.Length + " RigBuilder(s) on P2.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("Failed to replace P2 LegControllers (non-fatal): " + ex);
            }

            // Critical: set isPlayer=false on all BodyMovement components on P2 clone.
            // GameController.Start() does FindObjectsByType<BodyMovement> and sets player=
            // whichever has isPlayer==true. If P2 clone also has isPlayer=true, P1 loses
            // the player slot and can no longer move.
            try
            {
                var bodyMoveType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                if (bodyMoveType != null)
                {
                    var isPlayerField = bodyMoveType.GetField("isPlayer",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (isPlayerField != null)
                    {
                        var bms = _p2Spider.GetComponentsInChildren(bodyMoveType, true);
                        for (int i = 0; i < bms.Length; i++)
                            isPlayerField.SetValue(bms[i], false);
                        LoggerInstance.Msg("Set isPlayer=false on " + bms.Length + " P2 BodyMovement(s).");

                        // Null out followTarget on P2 to prevent drift toward P1
                        var ftField = bodyMoveType.GetField("followTarget",
                            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (ftField != null)
                        {
                            for (int i = 0; i < bms.Length; i++)
                                ftField.SetValue(bms[i], null);
                            LoggerInstance.Msg("Nulled followTarget on P2 BodyMovement(s).");
                        }
                    }
                    else
                    {
                        LoggerInstance.Warning("isPlayer field not found on BodyMovement — P1 movement may break.");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("Failed to set isPlayer=false on P2 spider (non-fatal): " + ex);
            }

            // Clear P2's MasterLegController.legs list — when cloned, it inherits P1's
            // LegController instances. P2's own LegController.Start() will re-populate it.
            // Without this, P2's MasterLegController drives P1's legs (ResetAllLegs, etc.)
            // causing P1's legs to glitch when P2 exists.
            try
            {
                var masterLegType = AccessTools.TypeByName("_Scripts.Spider.MasterLegController");
                if (masterLegType != null)
                {
                    var legsField = masterLegType.GetField("legs",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    if (legsField != null)
                    {
                        var p2Masters = _p2Spider.GetComponentsInChildren(masterLegType, true);
                        for (int i = 0; i < p2Masters.Length; i++)
                        {
                            var list = legsField.GetValue(p2Masters[i]);
                            if (list != null)
                            {
                                var clearMethod = list.GetType().GetMethod("Clear");
                                clearMethod?.Invoke(list, null);
                            }
                        }
                        LoggerInstance.Msg("Cleared legs list on " + p2Masters.Length + " P2 MasterLegController(s).");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("Failed to clear P2 MasterLegController.legs (non-fatal): " + ex);
            }

            // Move ALL P2 GameObjects to layer 2 (Ignore Raycast).
            // Physics.IgnoreCollision does NOT affect SphereCast/Raycast — only layer
            // masks do. whatIsGround (21268800) excludes layer 2, so P1's leg raycasts
            // will never hit P2's colliders.
            try
            {
                var allP2Transforms = _p2Spider.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < allP2Transforms.Length; i++)
                    allP2Transforms[i].gameObject.layer = 2; // Ignore Raycast
                LoggerInstance.Msg("Set " + allP2Transforms.Length + " P2 GameObjects to layer 2 (Ignore Raycast).");
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("Failed to set P2 layers (non-fatal): " + ex);
            }

            DestroyComponentByTypeName(_p2Spider, "_Scripts.General.DontDestroyMe");
            DestroyComponentByTypeName(_p2Spider, "_Scripts.LevelSaving.UniqueID");
            DestroyComponentByTypeName(_p2Spider, "_Scripts.NPCs.FollowerManager");

            if (_p2KeepPlayerTag != null && !_p2KeepPlayerTag.Value)
            {
                try { _p2Spider.tag = "Untagged"; } catch { }
            }

            if (_camRightOrBottom != null)
            {
                _camRightOrBottom.transform.SetParent(null, true);
                var it = FindChildTransform(_p2Spider.transform, "InputTransform");
                if (it == null) it = _p2Spider.transform;

                P2InputTransform = it;
                _p2CamRigInited = false;
                InitP2CameraRig();
            }

            // Initialize independent P2 web system
            try
            {
                if (_webController != null && P2Camera != null && P2InputTransform != null)
                {
                    _p2WebManager = _p2Spider.AddComponent<P2WebManager>();
                    _p2WebManager.Init(_webController, P2Camera, P2InputTransform, _p2Spider, LoggerInstance, _p1InputTransform);
                }
                else
                {
                    LoggerInstance.Warning("Cannot init P2WebManager: webController=" + (_webController != null) +
                        " P2Camera=" + (P2Camera != null) + " P2InputTransform=" + (P2InputTransform != null));
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("P2WebManager setup failed (non-fatal): " + ex);
            }

            LoggerInstance.Msg("Spawned P2: " + _p2Spider.name +
                               " | P2InputTransform=" + (P2InputTransform != null ? P2InputTransform.name : "null") +
                               " | P2WebManager=" + (_p2WebManager != null));
        }

        private void InitP2CameraRig()
        {
            if (_camRightOrBottom == null || (P2InputTransform == null && _p2Spider == null))
            {
                LoggerInstance.Msg("[P2CamDebug] InitP2CameraRig BAIL: camR=" + (_camRightOrBottom != null)
                    + " P2IT=" + (P2InputTransform != null) + " p2Spider=" + (_p2Spider != null));
                return;
            }

            // Only read P1's camera offset on the very first init (at spawn).
            if (!_p2CamBasePoseCached)
            {
                _p2CamBaseHeight = 1.0f;
                _p2CamBaseDist = P2CameraDistance;

                if (_camLeftOrTop != null && _p1InputTransform != null)
                {
                    var delta = _camLeftOrTop.transform.position - _p1InputTransform.position;
                    var hDist = new Vector3(delta.x, 0f, delta.z).magnitude;
                    LoggerInstance.Msg("[P2CamDebug] P1 camera offset: delta=" + delta
                        + " hDist=" + hDist.ToString("F2") + " height=" + delta.y.ToString("F2"));
                    if (hDist > 0.3f && hDist < 60f)
                    {
                        _p2CamBaseDist = hDist;
                        _p2CamBaseHeight = delta.y;
                    }
                }
                _p2CamBasePoseCached = true;
            }

            _p2CamDistance = _p2CamBaseDist;

            // Initialize camera direction: behind and slightly above the spider
            var p2Anchor = P2InputTransform != null ? P2InputTransform : _p2Spider.transform;
            _p2SmoothUp = p2Anchor.up;
            var surfUp = _p2SmoothUp;
            var behind = -Vector3.ProjectOnPlane(p2Anchor.forward, surfUp).normalized;
            if (behind.sqrMagnitude < 0.001f) behind = -p2Anchor.forward;
            // Tilt upward slightly (15 degrees worth)
            _p2CamDir = (behind + surfUp * 0.27f).normalized;

            _p2CamRigInited = true;
            // Schedule a re-read of P1's camera distance after Cinemachine settles (~1.5s at 60fps)
            _p2CamRecaptureFrame = Time.frameCount + 90;

            ApplyP2CameraTransform();

            LoggerInstance.Msg("[P2CamDebug] InitP2CameraRig done: dist=" + _p2CamDistance.ToString("F2")
                + " height=" + _p2CamBaseHeight.ToString("F2")
                + " camDir=" + _p2CamDir
                + " camPos=" + _camRightOrBottom.transform.position + " camRot=" + _camRightOrBottom.transform.rotation.eulerAngles);
        }

        private void ApplyP2CameraTransform()
        {
            var p2Anchor = P2InputTransform != null ? P2InputTransform : _p2Spider.transform;

            // Pivot above spider along smoothed surface up
            var pivot = p2Anchor.position + _p2SmoothUp * _p2CamBaseHeight;

            // Camera position along the orbit direction
            var desiredPos = pivot + _p2CamDir * _p2CamDistance;

            // Look ABOVE the pivot so the spider sits in the lower portion of the screen,
            // matching P1's Cinemachine framing. Screen center then points at the horizon/walls
            // ahead, which is where the target dot ray (camera.forward) should hit.
            float lookAbove = _p2CamDistance * 0.15f;
            var lookTarget = pivot + Vector3.up * lookAbove;
            var lookDir = lookTarget - desiredPos;
            if (lookDir.sqrMagnitude > 0.001f)
                _camRightOrBottom.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            _camRightOrBottom.transform.position = desiredPos;
        }

        private void UpdateP2CameraLook()
        {
            if (_camRightOrBottom == null) return;
            if (P2InputTransform == null && _p2Spider == null) return;

            if (!_p2CamRigInited)
                InitP2CameraRig();

            // Delayed recapture: re-read P1's camera distance after Cinemachine settles
            if (_p2CamRecaptureFrame > 0 && Time.frameCount >= _p2CamRecaptureFrame)
            {
                _p2CamRecaptureFrame = 0;
                if (_camLeftOrTop != null && _p1InputTransform != null)
                {
                    var delta = _camLeftOrTop.transform.position - _p1InputTransform.position;
                    var hDist = new Vector3(delta.x, 0f, delta.z).magnitude;
                    if (hDist > 0.3f && hDist < 60f)
                    {
                        _p2CamBaseDist = hDist;
                        _p2CamBaseHeight = delta.y;
                        _p2CamDistance = _p2CamBaseDist;
                        LoggerInstance.Msg("[P2CamDebug] Recaptured P1 camera offset: hDist=" + hDist.ToString("F2")
                            + " height=" + delta.y.ToString("F2")
                            + " (settled after " + 90 + " frames)");
                    }
                }
            }

            var prePos = _camRightOrBottom.transform.position;

            float yawInput = 0f;
            if (InputCompat.Held_N()) yawInput -= 1f;
            if (InputCompat.Held_M()) yawInput += 1f;

            float pitchInput = 0f;

            if (P2UseGamepad)
            {
                var rs = InputCompat.GetP2RightStick(P2GamepadIndex, P2Deadzone);
                yawInput += rs.x;
                pitchInput += rs.y;
            }

            var speed = _p2LookSpeed != null ? _p2LookSpeed.Value : 90f;
            var dt = Time.deltaTime;
            float yawDelta = yawInput * speed * dt;
            float pitchDelta = -pitchInput * speed * dt;

            // --- Incremental orbit ---
            // Orbit uses a smoothed version of the spider's surface normal as the "up" axis.
            // On flat ground this is ~world-up; on walls it smoothly becomes the wall normal.
            var p2Anchor = P2InputTransform != null ? P2InputTransform : _p2Spider.transform;
            _p2SmoothUp = Vector3.Slerp(_p2SmoothUp, p2Anchor.up, dt * 3f).normalized;
            var surfUp = _p2SmoothUp;

            // Yaw: rotate camera direction around the surface normal
            if (Mathf.Abs(yawDelta) > 0.001f)
                _p2CamDir = Quaternion.AngleAxis(yawDelta, surfUp) * _p2CamDir;

            // Pitch: rotate around the local right axis (perpendicular to surfUp and camDir)
            if (Mathf.Abs(pitchDelta) > 0.001f)
            {
                var right = Vector3.Cross(surfUp, _p2CamDir).normalized;
                if (right.sqrMagnitude > 0.001f)
                {
                    var newDir = Quaternion.AngleAxis(pitchDelta, right) * _p2CamDir;
                    // Clamp pitch: don't let camera go below surface plane or directly overhead
                    float angleFromUp = Vector3.Angle(newDir, surfUp);
                    if (angleFromUp > 15f && angleFromUp < 165f)
                        _p2CamDir = newDir.normalized;
                }
            }

            _p2CamDir = _p2CamDir.normalized;

            ApplyP2CameraTransform();

            // Throttled debug log every 2 seconds
            if (Time.unscaledTime >= _p2CamDebugNextLog)
            {
                _p2CamDebugNextLog = Time.unscaledTime + 2f;
                var wantPos = _camRightOrBottom.transform.position;
                var delta = (prePos - wantPos).magnitude;
                LoggerInstance.Msg("[P2CamDebug] UpdateP2CameraLook:"
                    + " anchor=" + p2Anchor.name + " anchorPos=" + p2Anchor.position
                    + " surfUp=" + surfUp + " camDir=" + _p2CamDir
                    + " dist=" + _p2CamDistance.ToString("F2") + " height=" + _p2CamBaseHeight.ToString("F2")
                    + " | prePos=" + prePos + " wantPos=" + wantPos + " preDelta=" + delta.ToString("F3")
                    + " | wantRot=" + _camRightOrBottom.transform.rotation.eulerAngles
                    + " | p1CamPos=" + (_camLeftOrTop != null ? _camLeftOrTop.transform.position.ToString() : "null"));
            }
        }

        private static float NormalizeSignedAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }

        private static GameObject FindPlayerSpider()
        {
            try
            {
                var tagged = GameObject.FindGameObjectsWithTag("Player");
                if (tagged != null)
                {
                    for (int i = 0; i < tagged.Length; i++)
                    {
                        var go = tagged[i];
                        if (go != null && go.name == "PlayerSpider")
                            return go;
                    }
                }
            }
            catch { }

            var all = UnityEngine.Object.FindObjectsOfType<GameObject>(true);
            if (all != null)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    var go = all[i];
                    if (go == null) continue;
                    if (!go.scene.IsValid()) continue;
                    if (go.name == "PlayerSpider") return go;
                }
            }
            return null;
        }

        private static Transform FindChildTransform(Transform root, string name)
        {
            var all = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t != null && string.Equals(t.name, name, StringComparison.Ordinal))
                    return t;
            }
            return null;
        }

        private static void DisableComponentByTypeName(GameObject go, string typeName)
        {
            var t = Type.GetType(typeName + ", Cinemachine") ?? Type.GetType(typeName);
            if (t == null) return;
            var c = go.GetComponent(t) as Behaviour;
            if (c != null) c.enabled = false;
        }

        private void DisableCameraDriverBehaviours(GameObject root)
        {
            if (root == null)
                return;

            int disabled = 0;
            var behaviours = root.GetComponentsInChildren<Behaviour>(true);
            if (behaviours == null)
                return;

            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i];
                if (behaviour == null) continue;
                if (behaviour is Camera) continue;
                if (behaviour is AudioListener) continue;

                var fullName = behaviour.GetType().FullName;
                if (string.IsNullOrEmpty(fullName)) continue;

                if (fullName.StartsWith("_Scripts.Camera.", StringComparison.Ordinal) ||
                    fullName.StartsWith("_Scripts.Singletons.", StringComparison.Ordinal) ||
                    fullName.IndexOf("Camera", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    string.Equals(fullName, "Cinemachine.CinemachineBrain", StringComparison.Ordinal))
                {
                    behaviour.enabled = false;
                    disabled++;
                    LoggerInstance.Msg("[P2CamDebug] DISABLED behaviour: " + fullName + " on " + behaviour.gameObject.name);
                }
            }

            LoggerInstance.Msg("[P2CamDebug] Disabled " + disabled + " camera driver behaviour(s) on P2 camera clone.");
        }

        private static void DestroyComponentByTypeName(GameObject root, string fullTypeName)
        {
            var t = AccessTools.TypeByName(fullTypeName);
            if (t == null) return;

            var comps = root.GetComponentsInChildren(t, true);
            if (comps == null) return;

            for (int i = 0; i < comps.Length; i++)
            {
                var comp = comps[i] as UnityEngine.Object;
                if (comp != null)
                    UnityEngine.Object.Destroy(comp);
            }
        }

        private void Teardown()
        {
            if (_camLeftOrTop != null) _camLeftOrTop.rect = new Rect(0f, 0f, 1f, 1f);

            if (_camRightOrBottom != null)
            {
                UnityEngine.Object.Destroy(_camRightOrBottom.gameObject);
                _camRightOrBottom = null;
            }

            // Clean up P2 web system before destroying the spider
            if (_p2WebManager != null)
            {
                _p2WebManager.Cleanup();
                _p2WebManager = null;
            }

            if (_p2Spider != null)
            {
                UnityEngine.Object.Destroy(_p2Spider);
                _p2Spider = null;
            }

            _webController = null;

            _p1InputTransform = null;
            _p2CamRigInited = false;
            _p2CamBasePoseCached = false;

            P2InputTransform = null;
            P2Camera = null;
            InP2WebContext = false;
            P2ShootHeld = false;
            P2JumpPressed = false;
        }
    }

    public sealed class P2Marker : MonoBehaviour { }

    /// <summary>
    /// Manages P2's web actions independently.
    /// - Shoot/Delete web: invokes P1's WebController with P2 context flags
    ///   so Harmony patches redirect WebStartPoint/WebDirection/Camera.main.
    /// - Grapple (attach/release): handled directly via SpringJoint on P2's rigidbody.
    /// - Target dot: simple unlit sphere primitive (guaranteed visible).
    /// </summary>
    public sealed class P2WebManager : MonoBehaviour
    {
        private Component _p1WebController;
        private Type _wcType;

        // Cached methods on P1's WebController (for shoot/delete only)
        private MethodInfo _mShootWeb;      // MobileShootWeb(bool)
        private MethodInfo _mDeleteWeb;     // MobileDeleteWeb()
        private MethodInfo _mCheckForWebTarget; // CheckForWebTarget(float)

        private Camera _p2Camera;
        private Transform _p2InputTransform;
        private Rigidbody _p2Rigidbody;

        // Simple sphere target dot
        private GameObject _p2TargetDot;
        private float _p2DotScale = 0.5f;
        private float _p2NormalOffset = 0.05f;

        // P2 grapple state
        private SpringJoint _grappleJoint;
        private LineRenderer _grappleLine;
        private LineRenderer _shootLine;   // preview line while shoot button held (mirrors P1's IndicateWeb)
        private Vector3 _grapplePoint;
        private bool _grappleActive;
        private float _grappleMaxDist = 50f;
        private static readonly float GrappleSpring = 500f;
        private static readonly float GrappleDamper = 15f;

        // P1-derived parameters (read via reflection/logging)
        private float _p1MaxDistance = 50f;
        private float _p1TargetScale = 0.5f;
        private float _webStartHeightOffset = 1.0f; // height of WebStartPoint above InputTransform

        // Input edge detection
        private bool _shootHeldPrev;
        private bool _attachHeldPrev;

        private bool _inited;
        private MelonLogger.Instance _logger;
        private float _nextDebugLog;
        private int _driveCallCount;

        public void Init(Component p1WebController, Camera p2Camera, Transform p2InputTransform, GameObject p2Spider, MelonLogger.Instance logger, Transform p1InputTransform = null)
        {
            _logger = logger;
            if (_logger != null) _logger.Msg("[P2WebManager] Init begin");

            try
            {
                if (p1WebController == null)
                {
                    logger.Warning("[P2WebManager] P1 WebController is null, cannot initialize.");
                    return;
                }

                _p1WebController = p1WebController;
                _wcType = p1WebController.GetType();

                // Cache methods (non-fatal)
                try { CacheWebControllerMethods(); } catch (Exception ex) { logger.Warning("[P2WebManager] CacheWebControllerMethods failed during init: " + ex); }

                // Read P1 params (non-fatal, skip if dependencies missing)
                try { TryReadP1TargetParams(p1WebController); } catch (System.IO.FileNotFoundException fnf) { logger.Warning("[P2WebManager] Skip TryReadP1TargetParams (missing dependency): " + fnf.Message); } catch (Exception ex) { logger.Warning("[P2WebManager] TryReadP1TargetParams failed during init: " + ex); }

                _p2Camera = p2Camera;
                _p2InputTransform = p2InputTransform;
                _p2Rigidbody = p2Spider != null ? p2Spider.GetComponent<Rigidbody>() : null;

                // Read P1's WebStartPoint height offset (isolated call to avoid MonoMod crash)
                try { ReadWebStartPointOffset(p1WebController, p2InputTransform, p1InputTransform); }
                catch (Exception ex) { logger.Msg("[P2WebManager] WebStartPoint offset read failed (using default " + _webStartHeightOffset + "): " + ex.Message); }

                // Use P1-derived scale/offset if found
                _p2DotScale = _p1TargetScale > 0.01f ? _p1TargetScale : _p2DotScale;
                _grappleMaxDist = _p1MaxDistance > 0f ? _p1MaxDistance : _grappleMaxDist;

                try { CreateTargetDot(); } catch (Exception ex) { logger.Warning("[P2WebManager] CreateTargetDot failed: " + ex); }
                try { CreateGrappleLine(p2Spider); } catch (Exception ex) { logger.Warning("[P2WebManager] CreateGrappleLine failed: " + ex); }

                logger.Msg("[P2WebManager] Initialized." +
                    " | ShootWeb=" + (_mShootWeb != null) +
                    " | DeleteWeb=" + (_mDeleteWeb != null) +
                    " | CheckForWebTarget=" + (_mCheckForWebTarget != null) +
                    " | Rigidbody=" + (_p2Rigidbody != null) +
                    " | TargetDot=" + (_p2TargetDot != null) +
                    " | P1MaxDist=" + _p1MaxDistance + " | P1Scale=" + _p1TargetScale + " | P2Offset=" + _p2NormalOffset +
                    " | WebStartHeight=" + _webStartHeightOffset.ToString("F2"));
            }
            finally
            {
                // Even if parts failed, mark inited when core references exist so DriveInput can run
                if (_p2Camera != null && _p2Rigidbody != null)
                    _inited = true;
            }
        }

        private void CreateTargetDot()
        {
            _p2TargetDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _p2TargetDot.name = "P2_WebTargetDot";
            _p2TargetDot.transform.localScale = Vector3.one * _p2DotScale;

            // Remove collider so it doesn't interfere with raycasts
            var col = _p2TargetDot.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);

            // Bright unlit red material — visible at distance
            var rend = _p2TargetDot.GetComponent<Renderer>();
            if (rend != null)
            {
                // Try Unlit/Color first (solid color), fallback to Sprites/Default
                var shader = Shader.Find("Unlit/Color");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = rend.material.shader;
                var mat = new Material(shader);
                mat.color = new Color(1f, 0f, 0f, 1f);
                // Try to set _Color property directly for Standard shader
                try { mat.SetColor("_Color", new Color(1f, 0f, 0f, 1f)); } catch { }
                try { mat.SetColor("_EmissionColor", new Color(1f, 0f, 0f, 1f)); } catch { }

                // Force ZTest=Always and ZWrite Off so the dot draws over any occluding geometry
                // (including P2's own body which sits between the camera and the target).
                try { mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always); } catch { }
                try { mat.SetInt("_ZWrite", 0); } catch { }
                // Render in a very late queue so it draws last, on top of everything
                mat.renderQueue = 5000;
                rend.material = mat;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rend.receiveShadows = false;
            }

            _p2TargetDot.SetActive(false);
            _logger.Msg("[P2WebManager] Created target dot sphere.");
        }

        private bool _webLineMatCached;

        private void CreateGrappleLine(GameObject p2Spider)
        {
            var lineGo = new GameObject("P2_GrappleLine");
            lineGo.transform.SetParent(p2Spider.transform, false);
            _grappleLine = lineGo.AddComponent<LineRenderer>();
            _grappleLine.useWorldSpace = true;
            _grappleLine.positionCount = 2;
            _grappleLine.startWidth = 0.15f;
            _grappleLine.endWidth = 0.15f;

            // Try to copy P1's web line material/settings from existing WebThread LineRenderers
            TryCopyWebLineMaterial();

            // Fallback if no web material found yet
            if (!_webLineMatCached)
            {
                var lineMat = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard"));
                lineMat.color = new Color(0.9f, 0.9f, 1f, 0.8f);
                _grappleLine.material = lineMat;
            }
            _grappleLine.enabled = false;

            // Shoot preview line — visible while holding the shoot button, mimics P1's IndicateWeb animation
            var shootLineGo = new GameObject("P2_ShootLine");
            shootLineGo.transform.SetParent(p2Spider.transform, false);
            _shootLine = shootLineGo.AddComponent<LineRenderer>();
            _shootLine.useWorldSpace = true;
            _shootLine.positionCount = 2;
            _shootLine.startWidth = 0.15f;
            _shootLine.endWidth = 0.05f;   // taper toward tip for a "shooting" look
            _shootLine.enabled = false;

            // Share the fallback material with the shoot line if web material wasn't found yet
            if (!_webLineMatCached && _grappleLine.material != null)
                _shootLine.material = new Material(_grappleLine.material);
        }

        private void TryCopyWebLineMaterial()
        {
            if (_grappleLine == null) return;
            try
            {
                // Search for any LineRenderer that belongs to a WebThread in the scene
                var allRenderers = UnityEngine.Object.FindObjectsOfType<LineRenderer>(true);
                LineRenderer bestLR = null;
                for (int i = 0; i < allRenderers.Length; i++)
                {
                    var lr = allRenderers[i];
                    if (lr == _grappleLine) continue;
                    if (lr == _shootLine) continue;
                    // Check if parent/self has a component with "WebThread" in its type name
                    var comps = lr.GetComponents<Component>();
                    bool isWebThread = false;
                    for (int c = 0; c < comps.Length; c++)
                    {
                        if (comps[c] != null && comps[c].GetType().Name.Contains("WebThread"))
                        {
                            isWebThread = true;
                            break;
                        }
                    }
                    if (isWebThread && lr.material != null)
                    {
                        bestLR = lr;
                        break;
                    }
                }

                if (bestLR != null)
                {
                    _grappleLine.material = new Material(bestLR.material);
                    _grappleLine.startWidth = bestLR.startWidth;
                    _grappleLine.endWidth = bestLR.endWidth;
                    _grappleLine.widthMultiplier = bestLR.widthMultiplier;
                    _grappleLine.colorGradient = bestLR.colorGradient;
                    _grappleLine.textureMode = bestLR.textureMode;
                    _grappleLine.numCapVertices = bestLR.numCapVertices;
                    _grappleLine.numCornerVertices = bestLR.numCornerVertices;

                    // Apply the same material/settings to the shoot preview line
                    if (_shootLine != null)
                    {
                        _shootLine.material = new Material(bestLR.material);
                        _shootLine.startWidth = bestLR.startWidth;
                        _shootLine.endWidth = bestLR.endWidth * 0.35f; // taper to tip
                        _shootLine.widthMultiplier = bestLR.widthMultiplier;
                        _shootLine.colorGradient = bestLR.colorGradient;
                        _shootLine.textureMode = bestLR.textureMode;
                        _shootLine.numCapVertices = bestLR.numCapVertices;
                        _shootLine.numCornerVertices = bestLR.numCornerVertices;
                    }

                    _webLineMatCached = true;
                    if (_logger != null)
                        _logger.Msg("[P2WebManager] Copied web line material from: " + bestLR.gameObject.name
                            + " width=" + bestLR.startWidth.ToString("F3") + " mat=" + bestLR.material.name);
                }
                else
                {
                    if (_logger != null)
                        _logger.Msg("[P2WebManager] No WebThread LineRenderer found yet for material copy.");
                }
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.Warning("[P2WebManager] TryCopyWebLineMaterial failed: " + ex.Message);
            }
        }

        private void TryReadP1TargetParams(Component wc)
        {
            if (wc == null) return;
            try
            {
                var t = wc.GetType();
                float maxDist = -1f;
                float scale = -1f;
                float normalOffset = -1f;
                string maxDistField = null;
                string scaleField = null;
                string offsetField = null;

                // Scan fields for plausible values
                var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                for (int i = 0; i < fields.Length; i++)
                {
                    var f = fields[i];
                    try
                    {
                        if (f.FieldType == typeof(float))
                        {
                            float v = (float)f.GetValue(wc);
                            string name = f.Name.ToLowerInvariant();

                            if (v > 0f && v < 1000f && (name.Contains("distance") || name.Contains("dist")))
                            {
                                maxDist = v;
                                maxDistField = f.Name;
                            }

                            if (v > 0f && v < 10f && (name.Contains("scale") || name.Contains("radius")))
                            {
                                scale = v;
                                scaleField = f.Name;
                            }

                            if (v > 0f && v < 1f && (name.Contains("offset") || name.Contains("normal")))
                            {
                                normalOffset = v;
                                offsetField = f.Name;
                            }
                        }
                        else if (typeof(GameObject).IsAssignableFrom(f.FieldType))
                        {
                            var go = f.GetValue(wc) as GameObject;
                            if (go != null && f.Name.ToLowerInvariant().Contains("target"))
                            {
                                var los = go.transform.localScale;
                                float s = (los.x + los.y + los.z) / 3f;
                                if (s > 0f && s < 10f)
                                {
                                    scale = s;
                                    scaleField = f.Name + ".transform.localScale";
                                }
                            }
                        }
                    }
                    catch { }
                }

                // Scan properties for max distance
                var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                for (int i = 0; i < props.Length; i++)
                {
                    var p = props[i];
                    if (!p.CanRead) continue;
                    try
                    {
                        if (p.PropertyType == typeof(float))
                        {
                            float v = (float)p.GetValue(wc, null);
                            string name = p.Name.ToLowerInvariant();
                            if (v > 0f && v < 1000f && (name.Contains("distance") || name.Contains("dist")))
                            {
                                maxDist = v;
                                maxDistField = p.Name + "(prop)";
                            }
                            if (v > 0f && v < 10f && (name.Contains("scale") || name.Contains("radius")))
                            {
                                scale = v;
                                scaleField = p.Name + "(prop)";
                            }
                        }
                    }
                    catch { }
                }

                if (maxDist > 0f) _p1MaxDistance = maxDist;
                if (scale > 0f) _p1TargetScale = scale;
                if (normalOffset > 0f) _p2NormalOffset = normalOffset;

                // Try to read WebStartPoint/WebDirection to infer ray origin/direction
                Vector3 origin = Vector3.zero;
                Vector3 direction = Vector3.zero;
                string originSource = null;
                string dirSource = null;

                try
                {
                    var mStart = t.GetMethod("get_WebStartPoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (mStart != null)
                    {
                        var val = mStart.Invoke(wc, null);
                        if (val is Vector3 v)
                        {
                            origin = v;
                            originSource = "WebStartPoint(Vector3)";
                        }
                        else if (val is Transform tr && tr != null)
                        {
                            origin = tr.position;
                            originSource = "WebStartPoint(Transform.position)";
                        }
                    }

                    var mDir = t.GetMethod("get_WebDirection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (mDir != null)
                    {
                        var val = mDir.Invoke(wc, null);
                        if (val is Vector3 v2)
                        {
                            direction = v2;
                            dirSource = "WebDirection(Vector3)";
                        }
                    }
                }
                catch { }

                if (_logger != null)
                    _logger.Msg($"[P2WebManager] P1 target params: maxDist={_p1MaxDistance}({maxDistField}) scale={_p1TargetScale}({scaleField}) normalOffset={_p2NormalOffset}({offsetField}) origin={origin}({originSource}) dir={direction}({dirSource})");
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.Warning("[P2WebManager] TryReadP1TargetParams failed: " + ex);
            }
        }

        private void ReadWebStartPointOffset(Component p1WebController, Transform p2InputTransform, Transform p1InputTransform)
        {
            // Safely read just the WebStartPoint getter to find how high above InputTransform the web origin is.
            // This is isolated from TryReadP1TargetParams which crashes on MonoMod.Backports.
            var t = p1WebController.GetType();
            var getter = t.GetMethod("get_WebStartPoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (getter == null)
            {
                if (_logger != null) _logger.Msg("[P2WebManager] WebStartPoint getter not found, using default offset " + _webStartHeightOffset);
                return;
            }

            var val = getter.Invoke(p1WebController, null);
            Vector3 wsPos = Vector3.zero;
            bool found = false;

            if (val is Transform tr && tr != null)
            {
                wsPos = tr.position;
                found = true;
                if (_logger != null) _logger.Msg("[P2WebManager] P1 WebStartPoint Transform: " + tr.name + " pos=" + wsPos);
            }
            else if (val is Vector3 v)
            {
                wsPos = v;
                found = true;
                if (_logger != null) _logger.Msg("[P2WebManager] P1 WebStartPoint Vector3: " + wsPos);
            }

            if (found)
            {
                // Calculate height difference between WebStartPoint and P1's InputTransform
                var refPos = p1InputTransform != null ? p1InputTransform.position : p2InputTransform.position;
                float heightDiff = wsPos.y - refPos.y;
                if (heightDiff > 0.05f && heightDiff < 5f)
                {
                    _webStartHeightOffset = heightDiff;
                    if (_logger != null) _logger.Msg("[P2WebManager] WebStartPoint height offset: " + _webStartHeightOffset.ToString("F3"));
                }
                else
                {
                    if (_logger != null) _logger.Msg("[P2WebManager] WebStartPoint height " + heightDiff.ToString("F3") + " out of range, using default " + _webStartHeightOffset);
                }
            }
        }

        private void CacheWebControllerMethods()
        {
            try
            {
                if (_wcType == null) return;
                _mShootWeb = _wcType.GetMethod("MobileShootWeb", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(bool) }, null);
                _mDeleteWeb = _wcType.GetMethod("MobileDeleteWeb", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                _mCheckForWebTarget = _wcType.GetMethod("CheckForWebTarget", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(float) }, null);

                if (_logger != null)
                    _logger.Msg("[P2WebManager] CacheWebControllerMethods: shoot=" + (_mShootWeb != null) + " delete=" + (_mDeleteWeb != null) + " check=" + (_mCheckForWebTarget != null));
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.Warning("[P2WebManager] CacheWebControllerMethods failed: " + ex);
            }
        }

        public void DriveInput()
        {
            _driveCallCount++;

            if (!_inited || _p1WebController == null)
            {
                if (Time.unscaledTime >= _nextDebugLog)
                {
                    _nextDebugLog = Time.unscaledTime + 3f;
                    if (_logger != null)
                        _logger.Warning("[P2WebManager] DriveInput SKIPPED: inited=" + _inited + " wc=" + (_p1WebController != null));
                }
                return;
            }

            try
            {
                // --- Read P2 input ---
                bool shootHeld = InputCompat.IsP2ShootHeldNow(
                    SplitScreenMod.P2UseGamepad,
                    SplitScreenMod.P2GamepadIndex,
                    SplitScreenMod.P2TriggerThreshold,
                    "uKey", KeyCode.U);
                bool shootDown = shootHeld && !_shootHeldPrev;
                bool shootUp   = !shootHeld && _shootHeldPrev;
                _shootHeldPrev = shootHeld;

                // LT uses hysteresis: higher threshold to start, lower to release
                // This prevents rapid on/off flickering when LT hovers near threshold
                float attachThresh = _attachHeldPrev ? (SplitScreenMod.P2TriggerThreshold * 0.5f) : SplitScreenMod.P2TriggerThreshold;
                bool attachHeld = InputCompat.IsP2AttachHeldNow(
                    SplitScreenMod.P2UseGamepad,
                    SplitScreenMod.P2GamepadIndex,
                    attachThresh,
                    "pKey", KeyCode.P);
                bool attachDown = attachHeld && !_attachHeldPrev;
                bool attachUp  = !attachHeld && _attachHeldPrev;
                _attachHeldPrev = attachHeld;

                bool delDown = InputCompat.IsP2DeletePressedNow(
                    SplitScreenMod.P2UseGamepad,
                    SplitScreenMod.P2GamepadIndex,
                    "oKey", KeyCode.O);

                bool releaseDown = InputCompat.IsP2ReleasePressedNow(
                    SplitScreenMod.P2UseGamepad,
                    SplitScreenMod.P2GamepadIndex,
                    "rightCtrlKey", KeyCode.RightControl);

                // --- Periodic debug dump ---
                if (Time.unscaledTime >= _nextDebugLog)
                {
                    _nextDebugLog = Time.unscaledTime + 2f;
                    if (_logger != null)
                        _logger.Msg("[P2WebManager] TICK #" + _driveCallCount +
                            " | shootHeld=" + shootHeld + " attachHeld=" + attachHeld +
                            " | cam=" + (_p2Camera != null) + " rb=" + (_p2Rigidbody != null) +
                            " | dot=" + (_p2TargetDot != null ? _p2TargetDot.activeSelf.ToString() : "NULL") +
                            " | grapple=" + _grappleActive + " joint=" + (_grappleJoint != null) +
                            " | line=" + (_grappleLine != null ? _grappleLine.enabled.ToString() : "NULL"));
                }

                // --- Target dot (always visible, like P1's) ---
                UpdateTargetDot(true);

                // --- Shoot/Delete web via P1's WebController with P2 context ---
                if (shootDown || shootUp || delDown)
                {
                    if (_logger != null)
                        _logger.Msg("[P2WebManager] WEB ACTION: shootDown=" + shootDown + " shootUp=" + shootUp + " delDown=" + delDown);

                    SplitScreenMod.InP2WebContext = true;
                    SplitScreenMod.P2ShootHeld = true;
                    try
                    {
                        if (shootHeld && _mCheckForWebTarget != null)
                            _mCheckForWebTarget.Invoke(_p1WebController, new object[] { 1f });

                        if (shootDown && _mShootWeb != null)
                            _mShootWeb.Invoke(_p1WebController, new object[] { true });
                        if (shootUp && _mShootWeb != null)
                            _mShootWeb.Invoke(_p1WebController, new object[] { false });

                        if (delDown && _mDeleteWeb != null)
                            _mDeleteWeb.Invoke(_p1WebController, null);
                    }
                    finally
                    {
                        SplitScreenMod.InP2WebContext = false;
                        SplitScreenMod.P2ShootHeld = false;
                    }
                }

                // --- Grapple (attach/release) handled directly on P2's rigidbody ---
                if (attachDown)
                {
                    if (_logger != null)
                        _logger.Msg("[P2WebManager] GRAPPLE ATTACH triggered");
                    TryStartGrapple();
                }

                if (attachUp || releaseDown)
                {
                    if (_logger != null)
                        _logger.Msg("[P2WebManager] GRAPPLE RELEASE: attachUp=" + attachUp + " releaseDown=" + releaseDown);
                    StopGrapple();
                }

                // Update shoot preview line (visible while holding shoot, like P1's IndicateWeb)
                UpdateShootLine(shootHeld);

                // Update grapple line visual
                UpdateGrappleLine();
            }
            catch (Exception ex)
            {
                SplitScreenMod.InP2WebContext = false;
                SplitScreenMod.P2ShootHeld = false;
                if (_logger != null)
                    _logger.Warning("[P2WebManager] DriveInput error: " + ex);
            }
        }

        private void UpdateShootLine(bool shootHeld)
        {
            if (_shootLine == null) return;

            // Lazy material copy — web materials may not exist at init time
            if (!_webLineMatCached)
                TryCopyWebLineMaterial();

            if (!shootHeld || _p2Camera == null)
            {
                _shootLine.enabled = false;
                return;
            }

            var startPos = _p2InputTransform != null
                ? _p2InputTransform.position + Vector3.up * _webStartHeightOffset
                : (_p2Rigidbody != null ? _p2Rigidbody.position : _p2Camera.transform.position);

            // Ray from camera center — same origin as target dot, so line always ends exactly on the dot
            var ray = new Ray(_p2Camera.transform.position, _p2Camera.transform.forward);
            RaycastHit hit;
            Vector3 endPos;
            if (Physics.Raycast(ray, out hit, _grappleMaxDist))
                endPos = hit.point;
            else
                endPos = _p2Camera.transform.position + _p2Camera.transform.forward * _grappleMaxDist;

            _shootLine.SetPosition(0, startPos);
            _shootLine.SetPosition(1, endPos);
            _shootLine.enabled = true;
        }

        private void UpdateTargetDot(bool show)
        {
            if (_p2TargetDot == null || _p2Camera == null) return;

            // P1's dot always appears at screen center.
            // Cast directly from camera in camera.forward direction.
            // P2's spider is on layer 2 (Ignore Raycast) so the ray passes through it.
            // The dot material has ZTest=Always so it renders on top of everything.
            var ray = new Ray(_p2Camera.transform.position, _p2Camera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _grappleMaxDist))
            {
                _p2TargetDot.SetActive(true);
                _p2TargetDot.transform.position = hit.point + hit.normal * _p2NormalOffset;
            }
            else
            {
                _p2TargetDot.SetActive(false);
            }
        }

        private void TryStartGrapple()
        {
            if (_p2Rigidbody == null || _p2Camera == null)
            {
                if (_logger != null)
                    _logger.Warning("[P2WebManager] TryStartGrapple FAILED: rb=" + (_p2Rigidbody != null) + " cam=" + (_p2Camera != null));
                return;
            }

            var ray = BuildP2Ray();
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, _grappleMaxDist))
            {
                if (_logger != null)
                    _logger.Msg("[P2WebManager] TryStartGrapple: no raycast hit from " + ray.origin + " dir " + ray.direction);
                return;
            }

            // Lazy retry: if we didn't find a web material at init, try again now
            if (!_webLineMatCached)
                TryCopyWebLineMaterial();

            _grapplePoint = hit.point;
            _grappleActive = true;

            // Remove existing joint if any
            if (_grappleJoint != null)
                UnityEngine.Object.Destroy(_grappleJoint);

            _grappleJoint = _p2Rigidbody.gameObject.AddComponent<SpringJoint>();
            _grappleJoint.autoConfigureConnectedAnchor = false;
            _grappleJoint.anchor = Vector3.zero;

            // If the hit object has a Rigidbody, connect to it (enables two-way pull / tugging physics objects)
            var hitRb = hit.collider != null ? hit.collider.attachedRigidbody : null;
            if (hitRb != null)
            {
                _grappleJoint.connectedBody = hitRb;
                // Convert hit point to the target's local space for the connected anchor
                _grappleJoint.connectedAnchor = hitRb.transform.InverseTransformPoint(_grapplePoint);
            }
            else
            {
                _grappleJoint.connectedAnchor = _grapplePoint;
            }

            float dist = Vector3.Distance(_p2Rigidbody.position, _grapplePoint);
            _grappleJoint.maxDistance = dist * 0.8f;
            _grappleJoint.minDistance = 0f;
            _grappleJoint.spring = GrappleSpring;
            _grappleJoint.damper = GrappleDamper;
            _grappleJoint.breakForce = Mathf.Infinity;
            _grappleJoint.tolerance = 0.01f;

            if (_logger != null)
                _logger.Msg("[P2WebManager] Grapple CREATED: point=" + _grapplePoint +
                    " dist=" + dist.ToString("F1") +
                    " rbPos=" + _p2Rigidbody.position +
                    " rbGO=" + _p2Rigidbody.gameObject.name +
                    " hitRb=" + (hitRb != null ? hitRb.gameObject.name : "null") +
                    " joint=" + (_grappleJoint != null));
        }

        private void StopGrapple()
        {
            if (_grappleJoint != null)
            {
                UnityEngine.Object.Destroy(_grappleJoint);
                _grappleJoint = null;
            }
            _grappleActive = false;

            if (_grappleLine != null)
                _grappleLine.enabled = false;
        }

        private Ray BuildP2Ray()
        {
            // Use the same camera-center ray as the target dot so the grapple fires
            // exactly where the dot is pointing. This ensures visual consistency.
            return new Ray(_p2Camera.transform.position, _p2Camera.transform.forward);
        }

        private void UpdateGrappleLine()
        {
            if (_grappleLine == null) return;

            if (!_grappleActive || _grappleJoint == null || _p2Rigidbody == null)
            {
                _grappleLine.enabled = false;
                return;
            }

            _grappleLine.enabled = true;

            // Start from P2's body (InputTransform), not rigidbody center
            var startPos = _p2InputTransform != null
                ? _p2InputTransform.position + Vector3.up * _webStartHeightOffset
                : _p2Rigidbody.position;

            // End at connected body (tracks moving targets) or fixed point
            Vector3 endPos;
            if (_grappleJoint != null && _grappleJoint.connectedBody != null)
                endPos = _grappleJoint.connectedBody.transform.TransformPoint(_grappleJoint.connectedAnchor);
            else
                endPos = _grapplePoint;

            // Straight line between start and end — matches P1's web visual
            _grappleLine.SetPosition(0, startPos);
            _grappleLine.SetPosition(1, endPos);
        }

        public void Cleanup()
        {
            StopGrapple();

            if (_p2TargetDot != null)
            {
                UnityEngine.Object.Destroy(_p2TargetDot);
                _p2TargetDot = null;
            }
            if (_grappleLine != null)
            {
                UnityEngine.Object.Destroy(_grappleLine.gameObject);
                _grappleLine = null;
            }
            if (_shootLine != null)
            {
                UnityEngine.Object.Destroy(_shootLine.gameObject);
                _shootLine = null;
            }
            _p1WebController = null;
            _p2Camera = null;
            _p2InputTransform = null;
            _p2Rigidbody = null;
            _inited = false;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private static MethodInfo FindMethod_Bool(Type t, string name)
        {
            var ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < ms.Length; i++)
            {
                if (ms[i].Name != name) continue;
                var ps = ms[i].GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(bool)) return ms[i];
            }
            return null;
        }

        private static MethodInfo FindMethod_NoArgs(Type t, string name)
        {
            var ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < ms.Length; i++)
            {
                if (ms[i].Name != name) continue;
                if (ms[i].GetParameters().Length == 0) return ms[i];
            }
            return null;
        }

        private static MethodInfo FindMethod_Float(Type t, string name)
        {
            var ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < ms.Length; i++)
            {
                if (ms[i].Name != name) continue;
                var ps = ms[i].GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(float)) return ms[i];
            }
            return null;
        }
    }

    /// <summary>
    /// Faithful replacement for LegController on P2.
    /// Replicates: surface-parented targetLocal, center-based distance check,
    /// StepDistance anticipation, alternating gait (opposingLegs), multi-radius
    /// SphereCast chain, and surface-normal driven rotation.
    /// Raycasts only on step (in Update not FixedUpdate) to avoid P1 interference.
    /// </summary>
    public sealed class P2LegDriver : MonoBehaviour
    {
        // IK target written by this driver
        private Transform _target;
        // Local "foot anchor" parented to the hit surface so feet track moving geometry
        private Transform _targetLocal;
        // Center transform — rest position of this leg, used for step-distance check
        private Transform _center;
        // Spider body transform (provides forward/up for anticipatory cast)
        private Transform _bodyTransform;
        // BodyMovement component + moveVector field for anticipation
        private Component _bodyMovement;
        private FieldInfo _moveVecField;

        private LayerMask _whatIsGround;
        private float _sphereRadius;
        private float _rayUpOffset;
        private float _rayLength;
        private float _stepDist;
        private float _stepTime;
        private float _stepHeight;
        private float _tipHeight;
        private float _newTargetDist;

        private Vector3 _oldTarget;
        private float _lerp = 1f;
        private float _scale = 1f;
        private bool _inited;
        private bool _resetLeg;
        private bool _instantReset;

        private P2LegDriver[] _opposingLegs;

        // BodyMovement.State property cached for jump detection
        private PropertyInfo _bodyMovStateProp;
        private object _bodyMovJumpingVal;
        private bool _bodyMovStateCached;

        public bool IsAnimating => _lerp < 1f;

        public void SetOpposingLegs(P2LegDriver[] legs) { _opposingLegs = legs; }

        // Jump pose transform (mirrors LegController.targetJump)
        private Transform _targetJump;

        public void Init(Transform target, Vector3 startingOffset, Transform center,
            Transform bodyTransform, Component bodyMovement, FieldInfo moveVecField,
            LayerMask whatIsGround, float sphereRadius,
            float rayUpOffset, float rayLength, float stepDist,
            float stepTime, float stepHeight, float tipHeight, float newTargetDist,
            Transform targetJump = null)
        {
            _target = target;
            _center = center;
            _bodyTransform = bodyTransform;
            _bodyMovement = bodyMovement;
            _targetJump = targetJump;
            _moveVecField = moveVecField;
            _whatIsGround = whatIsGround;
            _sphereRadius = sphereRadius;
            _rayUpOffset = rayUpOffset;
            _rayLength = rayLength;
            _stepDist = stepDist;
            _stepTime = Mathf.Max(stepTime, 0.05f);
            _stepHeight = stepHeight;
            _tipHeight = tipHeight;
            _newTargetDist = Mathf.Max(newTargetDist, 0.05f);
            _scale = Mathf.Max(transform.lossyScale.x, 0.01f);

            // Create a dedicated targetLocal child GameObject (mirrors original LegController)
            var tlGo = new GameObject("P2LegTargetLocal_" + gameObject.name);
            _targetLocal = tlGo.transform;
            _targetLocal.SetParent(null, false);

            // Initial placement: try raycast downward from leg root
            var origin = transform.position + _bodyTransform.up * _rayUpOffset * _scale;
            var ray = new Ray(origin, -_bodyTransform.up);
            RaycastHit hit;
            if (CheckLegSphereCast(ray, out hit))
            {
                PlaceTargetLocal(hit);
                _lerp = 1f;
                if (_target != null)
                {
                    _target.position = _targetLocal.position;
                    _target.rotation = _targetLocal.rotation;
                }
            }
            else
            {
                _targetLocal.position = transform.position - _bodyTransform.up * _scale * 0.5f;
                _targetLocal.rotation = Quaternion.LookRotation(
                    Vector3.Cross(transform.right, _bodyTransform.up), _bodyTransform.up);
                _lerp = 1f;
            }
            _oldTarget = _targetLocal.position;
            _inited = true;
        }

        private void OnDestroy()
        {
            if (_targetLocal != null)
                UnityEngine.Object.Destroy(_targetLocal.gameObject);
        }

        private bool IsBodyJumping()
        {
            if (_bodyMovement == null) return false;
            if (!_bodyMovStateCached)
            {
                _bodyMovStateCached = true;
                _bodyMovStateProp = _bodyMovement.GetType().GetProperty("State",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                if (_bodyMovStateProp != null)
                    try { _bodyMovJumpingVal = Enum.Parse(_bodyMovStateProp.PropertyType, "Jumping"); } catch { }
            }
            if (_bodyMovStateProp == null || _bodyMovJumpingVal == null) return false;
            try { return _bodyMovStateProp.GetValue(_bodyMovement).Equals(_bodyMovJumpingVal); }
            catch { return false; }
        }

        private bool _wasJumping;

        private void Update()
        {
            if (!_inited || _target == null || _targetLocal == null) return;
            bool jumping = IsBodyJumping();

            if (jumping)
            {
                // Mirror LegController.PerformJumpAnimation() exactly
                if (_targetJump != null)
                {
                    _targetLocal.position = _targetJump.position;
                    _oldTarget = _targetLocal.position;
                    _target.position = _targetJump.position;
                    _target.rotation = _targetJump.rotation;
                }
                else
                {
                    _targetLocal.position = transform.position - transform.up * 1f * _scale;
                    _oldTarget = _targetLocal.position;
                    _target.position = _targetLocal.position;
                    _target.rotation = transform.rotation;
                }
                _wasJumping = true;
                return;
            }

            // Landing transition: snap legs to ground immediately
            if (_wasJumping)
            {
                _wasJumping = false;
                _resetLeg = true;
                _instantReset = true;
                _lerp = 1f;
            }

            PerformLegAnimation(Time.deltaTime);
            PerformWalking();
        }

        private void PerformLegAnimation(float dt)
        {
            if (_lerp < 1f)
            {
                _target.rotation = _targetLocal.rotation;
                var pos = Vector3.Lerp(_oldTarget, _targetLocal.position, _lerp);
                pos += _target.up * Mathf.Sin(_lerp * Mathf.PI) * _stepHeight * _scale;
                _target.position = pos;
                _lerp += dt / (_stepTime * _scale);
            }
            else
            {
                _target.position = _targetLocal.position;
                _target.rotation = _targetLocal.rotation;
            }
        }

        private void PerformWalking()
        {
            if (_lerp >= 1f)
                CheckLegPosition();
        }

        private void CheckLegPosition()
        {
            // Compute ray origin with optional move-vector anticipation
            Vector3 rayOrigin;
            if (_resetLeg)
            {
                rayOrigin = transform.position
                    + transform.forward * 0f
                    + _bodyTransform.up * _rayUpOffset * _scale;
            }
            else
            {
                var moveY = GetMoveVectorY();
                rayOrigin = transform.position
                    + _bodyTransform.up * _rayUpOffset * _scale
                    + _bodyTransform.forward * moveY * _stepDist * _scale;
            }

            var ray = new Ray(rayOrigin, -_bodyTransform.up);

            // Alternating gait: don't step if any opposing leg is mid-animation
            if (!AllOpposingIdle()) return;

            if (_resetLeg)
            {
                RaycastHit hit;
                if (CheckLegSphereCast(ray, out hit) || _instantReset)
                {
                    StartLegAnimation(hit);
                }
            }
            else
            {
                // Use center transform if available, otherwise fall back to targetLocal
                var refPos = (_center != null) ? _center.position : _targetLocal.position;
                var dist = (refPos - _targetLocal.position).magnitude;
                if (dist > _newTargetDist * _scale)
                {
                    RaycastHit hit;
                    if (CheckLegSphereCast(ray, out hit))
                        StartLegAnimation(hit);
                }
            }
        }

        private void StartLegAnimation(RaycastHit hit)
        {
            if (hit.transform == null && !_instantReset) return;

            _oldTarget = _targetLocal.position;

            if (hit.transform != null)
            {
                _targetLocal.position = hit.point + hit.normal * _tipHeight * _scale;
                var fwd = Vector3.Cross(transform.right, hit.normal);
                _targetLocal.rotation = Quaternion.LookRotation(fwd, hit.normal);
                // Parent to hit surface so feet track moving geometry (webs, platforms)
                _targetLocal.SetParent(hit.transform, true);
            }

            if (_instantReset)
            {
                _lerp = 1f;
                if (_target != null)
                {
                    _target.position = _targetLocal.position;
                    _target.rotation = _targetLocal.rotation;
                }
                _resetLeg = false;
                _instantReset = false;
            }
            else
            {
                _lerp = 0f;
            }
        }

        private bool AllOpposingIdle()
        {
            if (_opposingLegs == null || _opposingLegs.Length == 0) return true;
            for (int i = 0; i < _opposingLegs.Length; i++)
                if (_opposingLegs[i] != null && _opposingLegs[i].IsAnimating)
                    return false;
            return true;
        }

        private float GetMoveVectorY()
        {
            if (_bodyMovement == null || _moveVecField == null) return 0f;
            try
            {
                var v = _moveVecField.GetValue(_bodyMovement);
                if (v is Vector2) return ((Vector2)v).y;
                if (v is Vector3) return ((Vector3)v).y;
            }
            catch { }
            return 0f;
        }

        private bool CheckLegSphereCast(Ray ray, out RaycastHit hit)
        {
            // Mirrors original: Raycast first, then 4 SphereCasts with increasing radius
            if (Physics.Raycast(ray, out hit, _rayLength * _scale, _whatIsGround))
                return true;
            for (int i = 1; i <= 4; i++)
            {
                float r = _sphereRadius * _scale * i * 0.25f;
                if (Physics.SphereCast(ray, r, out hit, _rayLength * _scale, _whatIsGround))
                    return true;
            }
            hit = default;
            return false;
        }

        private void PlaceTargetLocal(RaycastHit hit)
        {
            _targetLocal.position = hit.point + hit.normal * _tipHeight * _scale;
            var fwd = Vector3.Cross(transform.right, hit.normal);
            _targetLocal.rotation = Quaternion.LookRotation(fwd, hit.normal);
            _targetLocal.SetParent(hit.transform, true);
        }
    }

    internal static class CameraMouseLookPatches
    {
        public static bool OnLook_Prefix(object __0)
        {
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            if (InputCompat.IsP2LookActiveNow(SplitScreenMod.P2GamepadIndex, SplitScreenMod.P2Deadzone))
            {
                if (SplitScreenMod.DebugCameraInput)
                    MelonLogger.Msg("[CameraMouseLook] Blocked P2 gamepad OnLook.");
                return false;
            }

            return true;
        }
    }

    internal static class BodyMovementPatches
    {
        private static bool IsP2(object __instance)
        {
            var mb = __instance as MonoBehaviour;
            if (mb == null) return false;
            return mb.GetComponentInParent<P2Marker>() != null;
        }

        // PerformJumping cached fields
        private static bool _pjFieldsCached;
        private static FieldInfo _fPjJumpTimer, _fPjRb, _fPjState, _fPjLastRotation;
        private static FieldInfo _fPjMoveInput, _fPjPitchAngle;
        private static FieldInfo _fPjLandingRotSmooth, _fPjJumpingRotSmooth;
        private static FieldInfo _fPjLandingOffset, _fPjLandingRadius;
        private static FieldInfo _fPjAerialThresh, _fPjAerialSpeedLR, _fPjAerialSpeedFB;
        private static FieldInfo _fPjWhatIsGround;
        private static FieldInfo _fPjMovementTimer, _fPjMovementStopTime;
        private static MethodInfo _mPjPerformLanding;
        private static object _walkingStateValue;

        private static void CachePjFields(object instance)
        {
            if (_pjFieldsCached) return;
            _pjFieldsCached = true;
            var t = instance.GetType();
            _fPjJumpTimer      = t.GetField("jumpTimer",                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjRb             = t.GetField("rb",                           BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjState          = t.GetField("state",                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjLastRotation   = t.GetField("lastRotation",                 BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjMoveInput      = t.GetField("moveInput",                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjPitchAngle     = t.GetField("pitchAngle",                   BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjLandingRotSmooth  = t.GetField("landingRotationSmoothness", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjJumpingRotSmooth  = t.GetField("jumpingRotationSmoothness", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjLandingOffset  = t.GetField("landingTriggerOffset",         BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjLandingRadius  = t.GetField("landingTriggerRadius",         BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjAerialThresh   = t.GetField("aerialAccelerationThreshold",  BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjAerialSpeedLR  = t.GetField("aerialControlSpeedLeftRight",  BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjAerialSpeedFB  = t.GetField("aerialControlSpeedForwardBackwards", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjWhatIsGround   = t.GetField("whatIsGround",                 BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                               ?? t.GetField("WhatIsGround",                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjMovementTimer     = t.GetField("movementTimer",     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _fPjMovementStopTime  = t.GetField("movementStopTime",  BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            _mPjPerformLanding = t.GetMethod("PerformLanding",              BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (_fPjState != null)
                try { _walkingStateValue = Enum.Parse(_fPjState.FieldType, "Walking"); } catch { }
        }

        private static T PjGet<T>(FieldInfo f, object inst, T fallback = default)
        {
            if (f == null) return fallback;
            try { return (T)f.GetValue(inst); } catch { return fallback; }
        }

        public static bool PerformJumping_Prefix(object __instance)
        {
            if (!IsP2(__instance)) return true; // P1 uses original

            CachePjFields(__instance);

            try
            {
                var mb = __instance as MonoBehaviour;
                if (mb == null) return false;

                // --- Tick jump timer + keep movementTimer alive (mirrors original) ---
                float jumpTimer = PjGet<float>(_fPjJumpTimer, __instance);
                jumpTimer -= Time.fixedDeltaTime;
                _fPjJumpTimer?.SetValue(__instance, jumpTimer);
                float movStopTime = PjGet<float>(_fPjMovementStopTime, __instance, 0f);
                _fPjMovementTimer?.SetValue(__instance, movStopTime);

                var rb = PjGet<Rigidbody>(_fPjRb, __instance);
                if (rb == null) return false;

                // --- SphereCast toward velocity (same as original) ---
                bool hitGround = false;
                RaycastHit hitInfo = default;
                Vector3 vel = rb.linearVelocity;
                if (vel.sqrMagnitude > 0.001f)
                    hitGround = Physics.SphereCast(new Ray(mb.transform.position, vel.normalized),
                        0.5f, out hitInfo, 5f,
                        PjGet<LayerMask>(_fPjWhatIsGround, __instance, Physics.DefaultRaycastLayers));

                // --- Air control: build a flat yaw-only reference (matches CameraController.InputTransform) ---
                // Project camera forward onto horizontal plane to strip pitch — camera height won't affect threshold
                Vector3 flatFwd = Vector3.zero, flatRight = Vector3.zero;
                bool hasCamRef = false;
                if (SplitScreenMod.P2Camera != null)
                {
                    flatFwd   = Vector3.ProjectOnPlane(SplitScreenMod.P2Camera.transform.forward, Vector3.up);
                    flatRight = Vector3.ProjectOnPlane(SplitScreenMod.P2Camera.transform.right,   Vector3.up);
                    if (flatFwd.sqrMagnitude > 0.0001f && flatRight.sqrMagnitude > 0.0001f)
                    {
                        flatFwd.Normalize(); flatRight.Normalize();
                        hasCamRef = true;
                    }
                }
                // Project velocity onto flat camera axes for threshold check
                Vector3 inputTrans = hasCamRef
                    ? new Vector3(Vector3.Dot(vel, flatRight), 0f, Vector3.Dot(vel, flatFwd))
                    : Vector3.zero;
                Vector2 moveIn = PjGet<Vector2>(_fPjMoveInput, __instance);
                float aerialThresh  = PjGet<float>(_fPjAerialThresh,  __instance, 5f);
                float aerialSpeedLR = PjGet<float>(_fPjAerialSpeedLR, __instance, 2f);
                float aerialSpeedFB = PjGet<float>(_fPjAerialSpeedFB, __instance, 2f);
                if (!hitGround && hasCamRef)
                {
                    Vector3 airForce = Vector3.zero;
                    if (Mathf.Abs(inputTrans.x) < aerialThresh ||
                        (inputTrans.x < -aerialThresh && moveIn.x > 0f) ||
                        (inputTrans.x >  aerialThresh && moveIn.x < 0f))
                        airForce += flatRight * moveIn.x * aerialSpeedLR;
                    if (Mathf.Abs(inputTrans.z) < aerialThresh ||
                        (inputTrans.z < -aerialThresh && moveIn.y > 0f) ||
                        (inputTrans.z >  aerialThresh && moveIn.y < 0f))
                        airForce += flatFwd * moveIn.y * aerialSpeedFB;
                    rb.linearVelocity += Vector3.ClampMagnitude(airForce, aerialSpeedFB) * Time.fixedDeltaTime;
                    vel = rb.linearVelocity; // refresh after air control
                }

                // --- Rotation (mirror original math) ---
                Vector3 normalized = vel.sqrMagnitude > 0.001f
                    ? Vector3.Cross(Vector3.up, vel.normalized).normalized
                    : mb.transform.right;
                float pitchAngle = PjGet<float>(_fPjPitchAngle, __instance, 0f);
                Vector3 forward2  = Quaternion.AngleAxis(-pitchAngle, normalized) * vel.normalized;
                Vector3 upwards   = Vector3.Cross(normalized, -forward2);

                bool landing = jumpTimer <= 0f && hitGround;
                if (landing)
                {
                    upwards  = hitInfo.normal;
                    forward2 = Vector3.Cross(normalized, hitInfo.normal);
                }

                if (vel.sqrMagnitude > 0f)
                {
                    float smoothness = landing
                        ? PjGet<float>(_fPjLandingRotSmooth,  __instance, 2f)
                        : PjGet<float>(_fPjJumpingRotSmooth, __instance, 4f);
                    Quaternion targetRot = Quaternion.LookRotation(forward2, upwards);
                    Quaternion lastRot   = PjGet<Quaternion>(_fPjLastRotation, __instance, mb.transform.rotation);
                    mb.transform.rotation = Quaternion.Slerp(lastRot, targetRot, 1f / (1f + smoothness));
                }
                _fPjLastRotation?.SetValue(__instance, mb.transform.rotation);

                // --- Early return while still airborne ---
                if (jumpTimer > 0f) return false;

                // --- Landing trigger (mirror original) ---
                float lOffset = PjGet<float>(_fPjLandingOffset, __instance, 0f);
                float lRadius = PjGet<float>(_fPjLandingRadius, __instance, 0.5f);
                LayerMask groundMask = PjGet<LayerMask>(_fPjWhatIsGround, __instance, Physics.DefaultRaycastLayers);
                LayerMask landMask   = (int)groundMask | LayerMask.GetMask("Movable");
                if (Physics.CheckSphere(mb.transform.position + mb.transform.up * lOffset, lRadius, landMask))
                {
                    if (_mPjPerformLanding != null)
                        try { _mPjPerformLanding.Invoke(__instance, null); } catch { }
                    else
                    {
                        // Fallback: manually set state + MLC
                        if (_fPjState != null && _walkingStateValue != null)
                            _fPjState.SetValue(__instance, _walkingStateValue);
                        SetMlcState(__instance, _mlcWalkingState);
                    }
                }
            }
            catch { }

            return false; // Always skip original for P2
        }

        private static FieldInfo _jumpCheckDistField;
        private static bool _jumpCheckDistSearched;
        private static bool _inInitializeJumpOverride;
        // MLC state fields cached for leg pose during jump
        private static FieldInfo _fMlcRef;
        private static PropertyInfo _pMlcState;
        private static object _mlcJumpingState;
        private static object _mlcWalkingState;
        private static bool _mlcCached;

        private static void CacheMlcFields(object bodyMovInstance)
        {
            if (_mlcCached) return;
            _mlcCached = true;
            var t = bodyMovInstance.GetType();
            _fMlcRef = t.GetField("masterLegController",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (_fMlcRef == null) return;
            var mlc = _fMlcRef.GetValue(bodyMovInstance);
            if (mlc == null) return;
            _pMlcState = mlc.GetType().GetProperty("State",
                BindingFlags.Instance | BindingFlags.Public);
            if (_pMlcState == null) return;
            var stateType = _pMlcState.PropertyType;
            try { _mlcJumpingState = Enum.Parse(stateType, "Jumping"); } catch { }
            try { _mlcWalkingState = Enum.Parse(stateType, "Walking"); } catch { }
        }

        private static void SetMlcState(object bodyMovInstance, object state)
        {
            if (_fMlcRef == null || _pMlcState == null || state == null) return;
            var mlc = _fMlcRef.GetValue(bodyMovInstance);
            if (mlc != null)
                try { _pMlcState.SetValue(mlc, state); } catch { }
        }

        public static bool InitializeJump_Prefix(object __instance)
        {
            // Skip if we're already inside our own override call (re-entrance guard)
            if (_inInitializeJumpOverride) return true;
            if (!IsP2(__instance)) return true;

            if (!_jumpCheckDistSearched)
            {
                _jumpCheckDistSearched = true;
                _jumpCheckDistField = __instance.GetType().GetField("jumpCheckDistance",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }
            CacheMlcFields(__instance);

            float original = 0f;
            bool patched = false;
            if (_jumpCheckDistField != null)
            {
                try { original = (float)_jumpCheckDistField.GetValue(__instance); patched = true; } catch { }
                try { _jumpCheckDistField.SetValue(__instance, 0f); } catch { }
            }

            // Call original with zeroed jumpCheckDistance so SphereCast never blocks the jump
            _inInitializeJumpOverride = true;
            try { SplitScreenMod.BodyMove_InitializeJumpMethod.Invoke(__instance, null); } catch { }
            _inInitializeJumpOverride = false;

            if (patched && _jumpCheckDistField != null)
                try { _jumpCheckDistField.SetValue(__instance, original); } catch { }

            // Force leg jump pose — original sets masterLegController.State=Jumping inside
            // InitializeJump, but P2's MLC may be null if it got destroyed. Set it explicitly.
            SetMlcState(__instance, _mlcJumpingState);

            return false;
        }

        public static bool CallbackContextFilter_Prefix(object __instance, ref UnityEngine.InputSystem.InputAction.CallbackContext __0)
        {
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            if (IsP2(__instance)) return true;

            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex))
                return false;

            return true;
        }

        // State field cache for jump ground-check
        private static bool _stateFieldCached;
        private static FieldInfo _fStateForJump;
        private static object _jumpingStateForJump;

        private static bool IsAlreadyJumping(object instance)
        {
            if (!_stateFieldCached)
            {
                _stateFieldCached = true;
                _fStateForJump = instance.GetType().GetField("state",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (_fStateForJump != null)
                    try { _jumpingStateForJump = Enum.Parse(_fStateForJump.FieldType, "Jumping"); } catch { }
            }
            if (_fStateForJump == null || _jumpingStateForJump == null) return false;
            try { return _fStateForJump.GetValue(instance).Equals(_jumpingStateForJump); } catch { return false; }
        }

        public static void FixedUpdate_Prefix(object __instance)
        {
            if (IsP2(__instance))
            {
                // Consume the jump flag — guard against mid-air jumps
                if (SplitScreenMod.P2JumpPressed)
                {
                    SplitScreenMod.P2JumpPressed = false;
                    // Only jump if currently Walking (not already airborne)
                    if (!IsAlreadyJumping(__instance))
                    {
                        // Trigger InitializeJump via the existing prefix patch
                        // (InitializeJump_Prefix zeros jumpCheckDistance so it always fires)
                        if (SplitScreenMod.BodyMove_InitializeJumpMethod != null)
                            try { SplitScreenMod.BodyMove_InitializeJumpMethod.Invoke(__instance, null); } catch { }
                    }
                }

                // While airborne, inject raw stick as moveInput so PerformJumping_Prefix can apply aerial forces
                if (IsAlreadyJumping(__instance))
                {
                    var fi = SplitScreenMod.BodyMove_MoveInputField;
                    if (fi != null)
                    {
                        Vector2 raw = Vector2.zero;
                        if (InputCompat.Held_J()) raw.x -= 1f;
                        if (InputCompat.Held_L()) raw.x += 1f;
                        if (InputCompat.Held_K()) raw.y -= 1f;
                        if (InputCompat.Held_I()) raw.y += 1f;
                        if (SplitScreenMod.P2UseGamepad)
                        {
                            var gp = InputCompat.GetP2LeftStick(SplitScreenMod.P2GamepadIndex, SplitScreenMod.P2Deadzone);
                            raw += new Vector2(gp.x, gp.y);
                        }
                        if (raw.sqrMagnitude > 1f) raw.Normalize();
                        try { fi.SetValue(__instance, raw); } catch { }
                    }
                }
            }
            else if (SplitScreenMod.FilterP1FromP2Gamepad && SplitScreenMod.P2UseGamepad)
            {
                var p2ls = InputCompat.GetP2LeftStick(SplitScreenMod.P2GamepadIndex, SplitScreenMod.P2Deadzone);
                if (p2ls.sqrMagnitude > 0f)
                {
                    var p1ls = InputCompat.GetP1LeftStick(SplitScreenMod.P2Deadzone);
                    if (p1ls.sqrMagnitude < SplitScreenMod.P2Deadzone * SplitScreenMod.P2Deadzone)
                    {
                        var fi = SplitScreenMod.BodyMove_MoveInputField;
                        if (fi != null)
                            try { fi.SetValue(__instance, Vector2.zero); } catch { }
                    }
                }
            }
        }

        public static void NpcWalk_Postfix(object __instance)
        {
            if (!IsP2(__instance)) return;

            var fv = SplitScreenMod.BodyMove_MoveVectorField;
            if (fv == null) return;

            Vector2 v = Vector2.zero;
            float x = 0f, y = 0f;
            if (InputCompat.Held_J()) x -= 1f;
            if (InputCompat.Held_L()) x += 1f;
            if (InputCompat.Held_K()) y -= 1f;
            if (InputCompat.Held_I()) y += 1f;
            v += new Vector2(x, y);

            if (SplitScreenMod.P2UseGamepad)
            {
                var gp = InputCompat.GetP2LeftStick(SplitScreenMod.P2GamepadIndex, SplitScreenMod.P2Deadzone);
                v += new Vector2(gp.x, gp.y);
            }

            if (v.sqrMagnitude > 1f) v.Normalize();

            // Camera-relative remap:
            // raw v.x/v.y means strafe/forward in camera space.
            // Convert that desired world direction into the spider body's local move axes.
            var mb = __instance as MonoBehaviour;
            var p2Cam = SplitScreenMod.P2Camera;
            if (mb != null && p2Cam != null && v.sqrMagnitude > 0.0001f)
            {
                var body = mb.transform;
                var up = body.up;

                var camForward = Vector3.ProjectOnPlane(p2Cam.transform.forward, up);
                var camRight = Vector3.ProjectOnPlane(p2Cam.transform.right, up);

                // Fallback if projection degenerates (e.g., camera aligned with up).
                if (camForward.sqrMagnitude < 0.0001f || camRight.sqrMagnitude < 0.0001f)
                {
                    camForward = Vector3.ProjectOnPlane(p2Cam.transform.forward, Vector3.up);
                    camRight = Vector3.ProjectOnPlane(p2Cam.transform.right, Vector3.up);
                }

                camForward = camForward.sqrMagnitude > 0.0001f ? camForward.normalized : body.forward;
                camRight = camRight.sqrMagnitude > 0.0001f ? camRight.normalized : body.right;

                var desiredWorld = (camRight * v.x) + (camForward * v.y);
                if (desiredWorld.sqrMagnitude > 0.0001f)
                    desiredWorld.Normalize();

                v = new Vector2(
                    Vector3.Dot(desiredWorld, body.right),
                    Vector3.Dot(desiredWorld, body.forward));

                if (v.sqrMagnitude > 1f) v.Normalize();
            }

            try { fv.SetValue(__instance, v); } catch { }
        }
    }

    internal static class LegControllerPatches
    {
        private static FieldInfo _targetLocalField;
        private static bool _targetLocalFieldSearched;

        private static Transform GetTargetLocal(object instance)
        {
            if (!_targetLocalFieldSearched)
            {
                _targetLocalFieldSearched = true;
                _targetLocalField = instance.GetType().GetField("targetLocal",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }
            if (_targetLocalField == null) return null;
            try { return _targetLocalField.GetValue(instance) as Transform; } catch { return null; }
        }

        public static void FixedUpdate_Prefix(object __instance)
        {
            var mb = __instance as MonoBehaviour;
            if (mb == null) return;
            if (mb.GetComponentInParent<P2Marker>() == null) return;

            var targetLocal = GetTargetLocal(__instance);
            if (targetLocal == null) return;

            if (targetLocal.parent == mb.transform.parent)
                targetLocal.SetParent(null, true);
        }
    }

    internal static class WebControllerPatches
    {
        private static bool ShouldUseP2Now()
        {
            return SplitScreenMod.P2ShootHeld || SplitScreenMod.InP2WebContext;
        }

        private static float _nextFilterLog;

        public static bool CallbackContextFilter_Prefix(object __instance, ref UnityEngine.InputSystem.InputAction.CallbackContext __0)
        {
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex))
            {
                if (Time.unscaledTime >= _nextFilterLog)
                {
                    _nextFilterLog = Time.unscaledTime + 2f;
                    MelonLogger.Msg("[WebControllerPatches] BLOCKED P2 callback on " + __instance.GetType().Name);
                }
                return false;
            }

            return true;
        }

        public static bool WebStartPointTransform_Prefix(ref Transform __result)
        {
            if (!ShouldUseP2Now()) return true;
            if (SplitScreenMod.P2InputTransform == null) return true;
            __result = SplitScreenMod.P2InputTransform;
            return false;
        }

        public static bool WebStartPointVector3_Prefix(ref Vector3 __result)
        {
            if (!ShouldUseP2Now()) return true;
            if (SplitScreenMod.P2InputTransform == null) return true;
            __result = SplitScreenMod.P2InputTransform.position;
            return false;
        }

        public static bool WebDirectionVector3_Prefix(ref Vector3 __result)
        {
            if (!ShouldUseP2Now()) return true;

            if (SplitScreenMod.P2Camera != null)
            {
                __result = SplitScreenMod.P2Camera.transform.forward;
                return false;
            }

            if (SplitScreenMod.P2InputTransform != null)
            {
                __result = SplitScreenMod.P2InputTransform.forward;
                return false;
            }

            return true;
        }

        public static bool CheckForWebTarget_Prefix(object __instance, float raycastRadiusFactor)
        {
            if (!ShouldUseP2Now()) return true;
            // When in P2 context, still run the original method — the WebDirection and
            // Camera.main patches will redirect the raycast origin/direction to P2's camera.
            // We just need to let it run with P2's redirected data.
            return true;
        }
    }

    internal static class CameraControllerPatches
    {
        private static float _nextDebugLogAt;
        private static int _callbackHits;
        private static int _callbackBlockedP2;
        private static int _pollingBlockedP2;
        private static int _nonControllerPathSuspicions;

        public static bool CallbackContextFilter_Prefix(ref UnityEngine.InputSystem.InputAction.CallbackContext __0)
        {
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            bool sawAnyCallbackArg = true;
            _callbackHits++;

            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex))
            {
                _callbackBlockedP2++;
                return false;
            }

            if (InputCompat.IsP2LookActiveNow(SplitScreenMod.P2GamepadIndex, SplitScreenMod.P2Deadzone))
            {
                MaybeDebugLog("callback-path no P2 context; likely polling look path active", sawAnyCallbackArg);
                return false;
            }

            MaybeDebugLog("callback-path pass", sawAnyCallbackArg);

            return true;
        }

        // Polling look guard intentionally disabled due to crash risk in unknown camera lifecycle methods.

        private static void MaybeDebugLog(string reason, bool sawAnyCallbackArg)
        {
            if (!SplitScreenMod.DebugCameraInput) return;

            float now = Time.unscaledTime;
            if (now < _nextDebugLogAt) return;
            _nextDebugLogAt = now + 1.0f;

            var rs = InputCompat.GetP2RightStick(SplitScreenMod.P2GamepadIndex, 0f);
            MelonLogger.Msg("[CameraDebug] " + reason +
                " | rs=(" + rs.x.ToString("F3") + "," + rs.y.ToString("F3") + ")" +
                " | callbackArgs=" + sawAnyCallbackArg +
                " | callbackHits=" + _callbackHits +
                " | callbackBlockedP2=" + _callbackBlockedP2 +
                " | pollingBlockedP2=" + _pollingBlockedP2 +
                " | nonControllerPathSuspicions=" + _nonControllerPathSuspicions);
        }

        public static bool InputTransform_Prefix(ref Transform __result)
        {
            if (!(SplitScreenMod.P2ShootHeld || SplitScreenMod.InP2WebContext)) return true;
            if (SplitScreenMod.P2InputTransform == null) return true;
            __result = SplitScreenMod.P2InputTransform;
            return false;
        }
    }

    internal static class UnityCameraPatches
    {
        public static bool CameraMain_Prefix(ref Camera __result)
        {
            if (!(SplitScreenMod.P2ShootHeld || SplitScreenMod.InP2WebContext)) return true;
            if (SplitScreenMod.P2Camera == null) return true;
            __result = SplitScreenMod.P2Camera;
            return false;
        }
    }

    internal static class InputCompat
    {
        private static bool _inited;
        private static bool _usingNewInput;

        // Keyboard reflection
        private static Type _keyboardType;
        private static PropertyInfo _keyboardCurrentProp;
        private static readonly Dictionary<string, PropertyInfo> _keyboardKeyProps = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
        private static PropertyInfo _keyControlWasPressedThisFrame;
        private static PropertyInfo _keyControlIsPressed;

        // Gamepad reflection
        private static Type _gamepadType;
        private static PropertyInfo _gamepadAllProp;
        private static MethodInfo _readOnlyArrayGetItem;
        private static PropertyInfo _readOnlyArrayCountProp;

        private static PropertyInfo _leftStickProp;
        private static PropertyInfo _rightStickProp;
        private static PropertyInfo _rightTriggerProp;
        private static PropertyInfo _buttonSouthProp;
        private static PropertyInfo _buttonNorthProp;
        private static PropertyInfo _buttonEastProp;
        private static PropertyInfo _rightShoulderProp;
        private static PropertyInfo _leftTriggerProp;
        private static PropertyInfo _leftShoulderProp;
        private static PropertyInfo _buttonWestProp;

        private static MethodInfo _controlReadValueVec2;
        private static MethodInfo _controlReadValueFloat;
        private static PropertyInfo _buttonWasPressedThisFrame;

        // CallbackContext reflection
        private static PropertyInfo _ctxControlProp;
        private static PropertyInfo _controlDeviceProp;
        private static PropertyInfo _inputDeviceDeviceIdProp;

        public static void Init(MelonLogger.Instance logger)
        {
            if (_inited) return;
            _inited = true;

            _keyboardType = Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
            if (_keyboardType == null) { _usingNewInput = false; return; }

            _keyboardCurrentProp = _keyboardType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
            if (_keyboardCurrentProp == null) { _usingNewInput = false; return; }

            var keyControlType = Type.GetType("UnityEngine.InputSystem.Controls.KeyControl, Unity.InputSystem");
            if (keyControlType == null) { _usingNewInput = false; return; }

            _keyControlWasPressedThisFrame = keyControlType.GetProperty("wasPressedThisFrame", BindingFlags.Public | BindingFlags.Instance);
            _keyControlIsPressed = keyControlType.GetProperty("isPressed", BindingFlags.Public | BindingFlags.Instance);

            _usingNewInput = _keyControlWasPressedThisFrame != null && _keyControlIsPressed != null;

            _gamepadType = Type.GetType("UnityEngine.InputSystem.Gamepad, Unity.InputSystem");
            if (_gamepadType != null)
            {
                _gamepadAllProp = _gamepadType.GetProperty("all", BindingFlags.Public | BindingFlags.Static);

                var roType = Type.GetType("UnityEngine.InputSystem.Utilities.ReadOnlyArray`1, Unity.InputSystem");
                if (roType != null)
                {
                    _readOnlyArrayGetItem = roType.GetMethod("get_Item", BindingFlags.Public | BindingFlags.Instance);
                    _readOnlyArrayCountProp = roType.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
                }

                _leftStickProp = _gamepadType.GetProperty("leftStick", BindingFlags.Public | BindingFlags.Instance);
                _rightStickProp = _gamepadType.GetProperty("rightStick", BindingFlags.Public | BindingFlags.Instance);
                _rightTriggerProp = _gamepadType.GetProperty("rightTrigger", BindingFlags.Public | BindingFlags.Instance);
                _buttonSouthProp = _gamepadType.GetProperty("buttonSouth", BindingFlags.Public | BindingFlags.Instance);
                _buttonNorthProp = _gamepadType.GetProperty("buttonNorth", BindingFlags.Public | BindingFlags.Instance);
                _buttonEastProp = _gamepadType.GetProperty("buttonEast", BindingFlags.Public | BindingFlags.Instance);
                _rightShoulderProp = _gamepadType.GetProperty("rightShoulder", BindingFlags.Public | BindingFlags.Instance);
                _leftTriggerProp = _gamepadType.GetProperty("leftTrigger", BindingFlags.Public | BindingFlags.Instance);
                _leftShoulderProp = _gamepadType.GetProperty("leftShoulder", BindingFlags.Public | BindingFlags.Instance);
                _buttonWestProp = _gamepadType.GetProperty("buttonWest", BindingFlags.Public | BindingFlags.Instance);

                var vec2Control = Type.GetType("UnityEngine.InputSystem.Controls.Vector2Control, Unity.InputSystem");
                if (vec2Control != null) _controlReadValueVec2 = vec2Control.GetMethod("ReadValue", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

                var axisControl = Type.GetType("UnityEngine.InputSystem.Controls.AxisControl, Unity.InputSystem");
                if (axisControl != null) _controlReadValueFloat = axisControl.GetMethod("ReadValue", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);

                var buttonControl = Type.GetType("UnityEngine.InputSystem.Controls.ButtonControl, Unity.InputSystem");
                if (buttonControl != null)
                    _buttonWasPressedThisFrame = buttonControl.GetProperty("wasPressedThisFrame", BindingFlags.Public | BindingFlags.Instance);
            }

            var ctxType = Type.GetType("UnityEngine.InputSystem.InputAction+CallbackContext, Unity.InputSystem");
            if (ctxType != null)
                _ctxControlProp = ctxType.GetProperty("control", BindingFlags.Public | BindingFlags.Instance);

            var inputControlType = Type.GetType("UnityEngine.InputSystem.InputControl, Unity.InputSystem");
            if (inputControlType != null)
                _controlDeviceProp = inputControlType.GetProperty("device", BindingFlags.Public | BindingFlags.Instance);

            var inputDeviceType = Type.GetType("UnityEngine.InputSystem.InputDevice, Unity.InputSystem");
            if (inputDeviceType != null)
                _inputDeviceDeviceIdProp = inputDeviceType.GetProperty("deviceId", BindingFlags.Public | BindingFlags.Instance);
        }

        private static object KeyboardCurrent()
        {
            try { return _keyboardCurrentProp != null ? _keyboardCurrentProp.GetValue(null, null) : null; }
            catch { return null; }
        }

        private static object GetKeyControl(object keyboard, string propName)
        {
            PropertyInfo pi;
            if (!_keyboardKeyProps.TryGetValue(propName, out pi))
            {
                pi = _keyboardType != null ? _keyboardType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance) : null;
                if (pi != null) _keyboardKeyProps[propName] = pi;
            }
            return pi != null ? pi.GetValue(keyboard, null) : null;
        }

        private static bool WasPressedThisFrame(object keyControl)
        {
            try { return _keyControlWasPressedThisFrame != null && (bool)_keyControlWasPressedThisFrame.GetValue(keyControl, null); }
            catch { return false; }
        }

        private static bool IsPressed(object keyControl)
        {
            try { return _keyControlIsPressed != null && (bool)_keyControlIsPressed.GetValue(keyControl, null); }
            catch { return false; }
        }

        public static bool Down(string propName, KeyCode fallback)
        {
            if (_usingNewInput)
            {
                var kb = KeyboardCurrent();
                if (kb == null) return false;
                var key = GetKeyControl(kb, propName);
                if (key == null) return false;
                return WasPressedThisFrame(key);
            }

            try { return UnityEngine.Input.GetKeyDown(fallback); }
            catch { return false; }
        }

        public static bool Held(string propName, KeyCode fallback)
        {
            if (_usingNewInput)
            {
                var kb = KeyboardCurrent();
                if (kb == null) return false;
                var key = GetKeyControl(kb, propName);
                if (key == null) return false;
                return IsPressed(key);
            }

            try { return UnityEngine.Input.GetKey(fallback); }
            catch { return false; }
        }

        public static bool Down_F9() { return Down("f9Key", KeyCode.F9); }
        public static bool Down_F10() { return Down("f10Key", KeyCode.F10); }

        public static bool Held_I() { return Held("iKey", KeyCode.I); }
        public static bool Held_J() { return Held("jKey", KeyCode.J); }
        public static bool Held_K() { return Held("kKey", KeyCode.K); }
        public static bool Held_L() { return Held("lKey", KeyCode.L); }

        public static bool Held_N() { return Held("nKey", KeyCode.N); }
        public static bool Held_M() { return Held("mKey", KeyCode.M); }

        private static object GetGamepadAtIndex(int index)
        {
            try
            {
                if (_gamepadType == null || _gamepadAllProp == null) return null;

                var ro = _gamepadAllProp.GetValue(null, null);
                if (ro == null) return null;

                var roRuntimeType = ro.GetType();
                var countProp = _readOnlyArrayCountProp;
                if (countProp == null || !countProp.DeclaringType.IsAssignableFrom(roRuntimeType))
                    countProp = roRuntimeType.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);

                var itemGetter = _readOnlyArrayGetItem;
                if (itemGetter == null || !itemGetter.DeclaringType.IsAssignableFrom(roRuntimeType))
                    itemGetter = roRuntimeType.GetMethod("get_Item", BindingFlags.Public | BindingFlags.Instance);

                if (countProp == null || itemGetter == null) return null;

                var countObj = countProp.GetValue(ro, null);
                int count = countObj is int ? (int)countObj : 0;
                if (index < 0 || index >= count) return null;

                return itemGetter.Invoke(ro, new object[] { index });
            }
            catch { return null; }
        }

        private static Vector2 ReadStick(object gamepad, PropertyInfo stickProp)
        {
            try
            {
                if (gamepad == null || stickProp == null || _controlReadValueVec2 == null) return Vector2.zero;
                var stick = stickProp.GetValue(gamepad, null);
                if (stick == null) return Vector2.zero;
                var v = _controlReadValueVec2.Invoke(stick, null);
                if (v is Vector2) return (Vector2)v;
                return Vector2.zero;
            }
            catch { return Vector2.zero; }
        }

        private static float ReadAxis(object gamepad, PropertyInfo axisProp)
        {
            try
            {
                if (gamepad == null || axisProp == null || _controlReadValueFloat == null) return 0f;
                var axis = axisProp.GetValue(gamepad, null);
                if (axis == null) return 0f;
                var v = _controlReadValueFloat.Invoke(axis, null);
                if (v is float) return (float)v;
                return 0f;
            }
            catch { return 0f; }
        }

        private static bool ReadButtonDown(object gamepad, PropertyInfo buttonProp)
        {
            try
            {
                if (gamepad == null || buttonProp == null || _buttonWasPressedThisFrame == null) return false;
                var btn = buttonProp.GetValue(gamepad, null);
                if (btn == null) return false;
                var v = _buttonWasPressedThisFrame.GetValue(btn, null);
                return v is bool ? (bool)v : false;
            }
            catch { return false; }
        }

        public static Vector2 GetP2LeftStick(int index, float deadzone)
        {
            var gp = GetGamepadAtIndex(index);
            var v = ReadStick(gp, _leftStickProp);
            if (v.magnitude < deadzone) return Vector2.zero;
            return v;
        }

        public static float GetP2RightStickX(int index)
        {
            var v = GetP2RightStick(index, 0f);
            return v.x;
        }

        public static Vector2 GetP2RightStick(int index, float deadzone)
        {
            var gp = GetGamepadAtIndex(index);
            var v = ReadStick(gp, _rightStickProp);
            if (v.magnitude < deadzone) return Vector2.zero;
            return v;
        }

        public static bool IsP2LookActiveNow(int index, float deadzone)
        {
            var v = GetP2RightStick(index, deadzone);
            return v.sqrMagnitude > 0f;
        }

        public static Vector2 GetP1RightStick(float deadzone)
        {
            var gp = GetGamepadAtIndex(0);
            var v = ReadStick(gp, _rightStickProp);
            if (v.magnitude < deadzone) return Vector2.zero;
            return v;
        }

        public static Vector2 GetP1LeftStick(float deadzone)
        {
            var gp = GetGamepadAtIndex(0);
            var v = ReadStick(gp, _leftStickProp);
            if (v.magnitude < deadzone) return Vector2.zero;
            return v;
        }

        public static bool IsAnyCallbackContextArg(object arg)
        {
            if (arg == null) return false;

            try
            {
                var t = arg.GetType();
                if (string.Equals(t.Name, "CallbackContext", StringComparison.Ordinal)) return true;

                var fn = t.FullName;
                if (!string.IsNullOrEmpty(fn) && fn.IndexOf("CallbackContext", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            catch { }

            return false;
        }

        public static bool IsP2ShootHeldNow(bool useGamepad, int index, float triggerThreshold, string kbProp, KeyCode kbFallback)
        {
            bool kb = Held(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetGamepadAtIndex(index);
            float rt = ReadAxis(gp, _rightTriggerProp);
            return kb || (rt >= triggerThreshold);
        }

        public static bool IsP2JumpPressedNow(bool useGamepad, int index)
        {
            bool kb = Down("backslashKey", KeyCode.Backslash);
            if (!useGamepad) return kb;
            var gp = GetGamepadAtIndex(index);
            bool south = ReadButtonDown(gp, _buttonSouthProp);
            return kb || south;
        }

        public static bool IsP2AttachHeldNow(bool useGamepad, int index, float triggerThreshold, string kbProp, KeyCode kbFallback)
        {
            bool kb = Held(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetGamepadAtIndex(index);
            float lt = ReadAxis(gp, _leftTriggerProp);
            return kb || (lt >= triggerThreshold);
        }

        public static bool IsP2AttachPressedNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = Down(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetGamepadAtIndex(index);
            float lt = ReadAxis(gp, _leftTriggerProp);
            // Treat trigger as "pressed" when it crosses threshold — callers track edge themselves
            return kb || (lt >= 0.35f);
        }

        public static bool IsP2DeletePressedNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = Down(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetGamepadAtIndex(index);
            bool b = ReadButtonDown(gp, _buttonEastProp);
            return kb || b;
        }

        public static bool IsP2ReleasePressedNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = Down(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetGamepadAtIndex(index);
            bool rb = ReadButtonDown(gp, _rightShoulderProp);
            return kb || rb;
        }

        public static bool IsCallbackContextFromP2Gamepad(object ctx, int p2Index)
        {
            try
            {
                if (ctx == null) return false;

                var gp = GetGamepadAtIndex(p2Index);
                if (gp == null) return false;

                var ctxControlProp = _ctxControlProp;
                if (ctxControlProp == null || !ctxControlProp.DeclaringType.IsAssignableFrom(ctx.GetType()))
                    ctxControlProp = ctx.GetType().GetProperty("control", BindingFlags.Public | BindingFlags.Instance);
                if (ctxControlProp == null) return false;

                var control = ctxControlProp.GetValue(ctx, null);
                if (control == null) return false;

                var controlDeviceProp = _controlDeviceProp;
                if (controlDeviceProp == null || !controlDeviceProp.DeclaringType.IsAssignableFrom(control.GetType()))
                    controlDeviceProp = control.GetType().GetProperty("device", BindingFlags.Public | BindingFlags.Instance);
                if (controlDeviceProp == null) return false;

                var device = controlDeviceProp.GetValue(control, null);
                if (device == null) return false;

                if (object.ReferenceEquals(device, gp)) return true;

                var devIdProp = _inputDeviceDeviceIdProp;
                if (devIdProp == null || !devIdProp.DeclaringType.IsAssignableFrom(device.GetType()))
                    devIdProp = device.GetType().GetProperty("deviceId", BindingFlags.Public | BindingFlags.Instance);

                object deviceId = devIdProp != null ? devIdProp.GetValue(device, null) : null;
                object gpId = devIdProp != null ? devIdProp.GetValue(gp, null) : null;

                if (deviceId != null && gpId != null)
                    return string.Equals(deviceId.ToString(), gpId.ToString(), StringComparison.Ordinal);

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
