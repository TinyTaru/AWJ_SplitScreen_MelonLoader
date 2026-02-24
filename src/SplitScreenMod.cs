using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

[assembly: MelonInfo(typeof(AWJSplitScreen.SplitScreenMod), "AWJ Split Screen + P2 Inject", "0.2.2", "ChatGPT")]
[assembly: MelonGame(null, "A Webbing Journey")]

namespace AWJSplitScreen
{
    public sealed class SplitScreenMod : MelonMod
    {
        // Shared config/state for patches
        internal static bool P2UseGamepad = true;
        internal static int P2GamepadIndex = 1;        // second controller
        internal static float P2Deadzone = 0.15f;
        internal static float P2TriggerThreshold = 0.35f;
        internal static bool FilterP1FromP2Gamepad = true;
        internal static float P2CameraDistance = 5.6f;
        internal static bool DebugCameraInput = false;

        internal static bool P2ShootHeld;              // computed each frame & from WebController.Update prefix
        internal static bool InP2WebContext;           // one-shot actions
        internal static Transform P2InputTransform;
        internal static Camera P2Camera;

        internal static FieldInfo BodyMove_MoveInputField;
        internal static FieldInfo BodyMove_MoveVectorField;

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

        private float _p2CamYaw;
        private float _p2CamPitch;
        private float _p2CamDistance;
        private bool _p2CamRigInited;

        // WebController calls
        private Component _webController;
        private MethodInfo _wcMobileShootWebBool;
        private MethodInfo _wcMobileDeleteWeb;
        private MethodInfo _wcAttachWeb;
        private MethodInfo _wcReleaseWebBool;

        private bool _p2ShootHeldPrev;

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
            LoggerInstance.Msg("F9 split, F10 orientation | P2 Move: IJKL or Gamepad LStick | P2 Look: N/M or RStickX | P2 Web: U/RT shoot, P/A attach, O/B delete, RightCtrl/RB release.");
            LoggerInstance.Msg("Tip: If both controllers still move P1, ensure FilterP1FromP2Gamepad=true and P2_GamepadIndex is the second pad (usually 1).");
        }

        private void ApplyPrefsToStatics()
        {
            P2UseGamepad = _p2UseGamepadPref.Value;
            P2GamepadIndex = _p2GamepadIndexPref.Value;
            P2Deadzone = _p2DeadzonePref.Value;
            P2TriggerThreshold = _p2TriggerThresholdPref.Value;
            FilterP1FromP2Gamepad = _filterP1FromP2PadPref.Value;
            P2CameraDistance = Mathf.Clamp(_p2CameraDistancePref.Value, 5.5f, 14f);
            DebugCameraInput = _debugCameraInputPref.Value;
        }

        public override void OnDeinitializeMelon()
        {
        }

        private void InstallHarmonyPatches()
        {
            var h = new HarmonyLib.Harmony("AWJ.SplitScreen.P2Inject.v022");

            // BodyMovement patches
            try
            {
                var bodyMoveType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                if (bodyMoveType != null)
                {
                    BodyMove_MoveInputField = FindBestMoveInputField(bodyMoveType);
                    BodyMove_MoveVectorField = FindFieldByName(bodyMoveType, "moveVector");

                    var fixedUpdate = AccessTools.Method(bodyMoveType, "FixedUpdate");
                    if (fixedUpdate != null)
                        h.Patch(fixedUpdate,
                            prefix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.FixedUpdate_Prefix)));

                    var npcWalk = AccessTools.Method(bodyMoveType, "NpcWalk");
                    if (npcWalk != null)
                        h.Patch(npcWalk,
                            postfix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.NpcWalk_Postfix)));

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
                        }
                    }

                    LoggerInstance.Msg("Patched BodyMovement: FixedUpdate + NpcWalk + CallbackContext filter.");
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
                    var wcUpdate = AccessTools.Method(webType, "Update");
                    if (wcUpdate != null)
                        h.Patch(wcUpdate, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebControllerUpdate_Prefix)));

                    var wcFixed = AccessTools.Method(webType, "FixedUpdate");
                    if (wcFixed != null)
                        h.Patch(wcFixed, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebControllerUpdate_Prefix)));

                    var getStart = AccessTools.Method(webType, "get_WebStartPoint");
                    if (getStart != null)
                    {
                        if (getStart.ReturnType == typeof(Transform))
                            h.Patch(getStart, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebStartPointTransform_Prefix)));
                        else if (getStart.ReturnType == typeof(Vector3))
                            h.Patch(getStart, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebStartPointVector3_Prefix)));
                    }

                    var getDir = AccessTools.Method(webType, "get_WebDirection");
                    if (getDir != null && getDir.ReturnType == typeof(Vector3))
                        h.Patch(getDir, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebDirectionVector3_Prefix)));

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
                        }
                    }

                    LoggerInstance.Msg("Patched WebController: Update sync + WebStartPoint/WebDirection + CallbackContext filter.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("WebController patch block failed (non-fatal): " + ex);
            }

            // CameraController InputTransform getter
            try
            {
                var camType = AccessTools.TypeByName("_Scripts.Singletons.CameraController");
                if (camType != null)
                {
                    var getter = AccessTools.PropertyGetter(camType, "InputTransform");
                    if (getter == null) getter = AccessTools.Method(camType, "get_InputTransform");
                    if (getter != null)
                        h.Patch(getter, prefix: new HarmonyMethod(typeof(CameraControllerPatches), nameof(CameraControllerPatches.InputTransform_Prefix)));

                    var cms = camType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    for (int i = 0; i < cms.Length; i++)
                    {
                        var m = cms[i];
                        var ps = m.GetParameters();
                        bool hasCallbackContext = false;
                        for (int p = 0; p < ps.Length; p++)
                        {
                            var pt = ps[p].ParameterType;
                            var pname = pt != null ? pt.Name : "";
                            if (string.Equals(pname, "CallbackContext", StringComparison.Ordinal) ||
                                (pt != null && pt.FullName != null && pt.FullName.IndexOf("CallbackContext", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                hasCallbackContext = true;
                                break;
                            }
                        }

                        if (hasCallbackContext)
                        {
                            h.Patch(m, prefix: new HarmonyMethod(typeof(CameraControllerPatches), nameof(CameraControllerPatches.CallbackContextFilter_Prefix)));
                        }
                    }

                    LoggerInstance.Msg("Patched CameraController.InputTransform + CallbackContext filter for P2.");
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
                        h.Patch(onLook, prefix: new HarmonyMethod(typeof(CameraMouseLookPatches), nameof(CameraMouseLookPatches.OnLook_Prefix)));
                        LoggerInstance.Msg("Patched CameraMouseLook.OnLook to block P2 gamepad input.");
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
                P2ShootHeld = InputCompat.IsP2ShootHeldNow(P2UseGamepad, P2GamepadIndex, P2TriggerThreshold, P2ShootKeyProp, P2ShootKeyFallback);

                UpdateP2CameraLook();
                InjectP2Web();
            }
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
            _wcMobileShootWebBool = null;
            _wcMobileDeleteWeb = null;
            _wcAttachWeb = null;
            _wcReleaseWebBool = null;

            var t = AccessTools.TypeByName("_Scripts.Singletons.WebController");
            if (t == null) return;

            var all = UnityEngine.Object.FindObjectsOfType(t, true);
            if (all != null && all.Length > 0)
                _webController = all[0] as Component;

            if (_webController == null) return;

            var mt = _webController.GetType();
            _wcMobileShootWebBool = FindMethod_Bool(mt, "MobileShootWeb");
            _wcMobileDeleteWeb = FindMethod_NoArgs(mt, "MobileDeleteWeb");
            _wcAttachWeb = FindMethod_NoArgs(mt, "AttachWeb");
            _wcReleaseWebBool = FindMethod_Bool(mt, "ReleaseWeb");

            LoggerInstance.Msg("WebController cached: " + (_webController != null) +
                               " | MobileShootWeb=" + (_wcMobileShootWebBool != null) +
                               " | MobileDeleteWeb=" + (_wcMobileDeleteWeb != null) +
                               " | AttachWeb=" + (_wcAttachWeb != null) +
                               " | ReleaseWeb=" + (_wcReleaseWebBool != null));
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
                    var targetF3 = legType3.GetField("target", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var offsetF3 = legType3.GetField("startingOffset", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    // Get params from MasterLegController
                    LayerMask whatIsGround = default;
                    float sphereRadius = 0.1f, rayUpOffset = 0.5f, rayLength = 5f;
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
                        }
                    }

                    // Get P2's BodyMovement transform + targetTransform (ground level)
                    var bodyMoveType3 = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                    Transform p2BodyTransform = null;
                    Transform p2GroundTransform = null;
                    if (bodyMoveType3 != null)
                    {
                        var p2Bm = _p2Spider.GetComponentInChildren(bodyMoveType3, true);
                        if (p2Bm != null)
                        {
                            p2BodyTransform = (p2Bm as Component).transform;
                            var ttProp = bodyMoveType3.GetProperty("TargetTransform",
                                BindingFlags.Instance | BindingFlags.Public);
                            p2GroundTransform = ttProp?.GetValue(p2Bm) as Transform;
                        }
                    }

                    // Save leg data, destroy LegController, add P2LegDriver
                    var p2Legs = _p2Spider.GetComponentsInChildren(legType3, true);
                    for (int i = 0; i < p2Legs.Length; i++)
                    {
                        var lc = p2Legs[i];
                        var go = (lc as Component).gameObject;
                        var target3 = targetF3?.GetValue(lc) as Transform;
                        var offset3 = offsetF3 != null ? (Vector3)offsetF3.GetValue(lc) : Vector3.zero;

                        UnityEngine.Object.DestroyImmediate(lc);

                        if (target3 != null && p2BodyTransform != null)
                        {
                            var driver = go.AddComponent<P2LegDriver>();
                            driver.Init(target3, offset3, p2BodyTransform,
                                p2GroundTransform, whatIsGround, sphereRadius,
                                rayUpOffset, rayLength,
                                stepTime, stepHeight, tipHeight, newTargetDist);
                        }
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
            }

            LoggerInstance.Msg("Spawned P2: " + _p2Spider.name +
                               " | P2InputTransform=" + (P2InputTransform != null ? P2InputTransform.name : "null"));
        }

        private void UpdateP2CameraLook()
        {
            if (_camRightOrBottom == null) return;
            if (P2InputTransform == null) return;

            var pivot = P2InputTransform.position + (Vector3.up * 1.2f);

            if (!_p2CamRigInited)
            {
                var rel = _camRightOrBottom.transform.position - pivot;
                if (rel.sqrMagnitude < 0.25f)
                    rel = new Vector3(0f, 1.9f, -5.6f);

                _p2CamDistance = Mathf.Clamp(P2CameraDistance, 3.5f, 12f);
                _p2CamPitch = Mathf.Clamp(Mathf.Atan2(rel.y, Mathf.Max(0.01f, _p2CamDistance)) * Mathf.Rad2Deg, -10f, 55f);
                _p2CamYaw = Mathf.Atan2(rel.x, rel.z) * Mathf.Rad2Deg;
                _p2CamRigInited = true;
            }

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

            _p2CamYaw += yawInput * speed * dt;
            _p2CamPitch = Mathf.Clamp(_p2CamPitch - (pitchInput * speed * dt), -25f, 65f);

            var rot = Quaternion.Euler(_p2CamPitch, _p2CamYaw, 0f);
            var camPos = pivot - (rot * Vector3.forward * _p2CamDistance);

            _camRightOrBottom.transform.position = camPos;
            _camRightOrBottom.transform.rotation = rot;
        }

        private static float NormalizeSignedAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }

        private void InjectP2Web()
        {
            if (_p2Spider == null || _webController == null) return;

            bool shootHeld = P2ShootHeld;
            bool shootDown = shootHeld && !_p2ShootHeldPrev;
            bool shootUp = !shootHeld && _p2ShootHeldPrev;
            _p2ShootHeldPrev = shootHeld;

            bool delDown = InputCompat.IsP2DeletePressedNow(P2UseGamepad, P2GamepadIndex, P2DeleteKeyProp, P2DeleteKeyFallback);
            bool attachDown = InputCompat.IsP2AttachPressedNow(P2UseGamepad, P2GamepadIndex, P2AttachKeyProp, P2AttachKeyFallback);
            bool releaseDown = InputCompat.IsP2ReleasePressedNow(P2UseGamepad, P2GamepadIndex, P2ReleaseKeyProp, P2ReleaseKeyFallback);

            try
            {
                InP2WebContext = attachDown || delDown || releaseDown;

                if (_wcMobileShootWebBool != null)
                {
                    if (shootDown) _wcMobileShootWebBool.Invoke(_webController, new object[] { true });
                    if (shootUp) _wcMobileShootWebBool.Invoke(_webController, new object[] { false });
                }

                if (delDown && _wcMobileDeleteWeb != null)
                    _wcMobileDeleteWeb.Invoke(_webController, null);

                if (attachDown && _wcAttachWeb != null)
                    _wcAttachWeb.Invoke(_webController, null);

                if (releaseDown && _wcReleaseWebBool != null)
                    _wcReleaseWebBool.Invoke(_webController, new object[] { true });
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("P2 web invoke error (non-fatal): " + ex);
            }
            finally
            {
                InP2WebContext = false;
            }
        }

        private static MethodInfo FindMethod_Bool(Type t, string name)
        {
            var ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < ms.Length; i++)
            {
                var m = ms[i];
                if (m.Name != name) continue;
                var ps = m.GetParameters();
                if (ps.Length == 1 && ps[0].ParameterType == typeof(bool))
                    return m;
            }
            return null;
        }

        private static MethodInfo FindMethod_NoArgs(Type t, string name)
        {
            var ms = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < ms.Length; i++)
            {
                var m = ms[i];
                if (m.Name != name) continue;
                var ps = m.GetParameters();
                if (ps.Length == 0) return m;
            }
            return null;
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

            if (_p2Spider != null)
            {
                UnityEngine.Object.Destroy(_p2Spider);
                _p2Spider = null;
            }

            _webController = null;
            _p2ShootHeldPrev = false;

            _p1InputTransform = null;
            _p2CamRigInited = false;

            P2InputTransform = null;
            P2Camera = null;
            InP2WebContext = false;
            P2ShootHeld = false;
        }
    }

    public sealed class P2Marker : MonoBehaviour { }

    /// <summary>
    /// Lightweight replacement for LegController on P2.
    /// The original LegController's continuous FixedUpdate raycasts interfere with P1's legs.
    /// This version only raycasts when a step is actually needed (in Update, not FixedUpdate),
    /// which minimizes interference. Falls back to plane projection if raycast misses.
    /// </summary>
    public sealed class P2LegDriver : MonoBehaviour
    {
        private Transform _target;
        private Transform _bodyTransform;
        private Transform _groundTransform;
        private Vector3 _startingOffset;
        private LayerMask _whatIsGround;
        private float _sphereRadius;
        private float _rayUpOffset;
        private float _rayLength;
        private float _stepTime;
        private float _stepHeight;
        private float _tipHeight;
        private float _newTargetDist;

        private Vector3 _goalPos;
        private Vector3 _goalNormal = Vector3.up;
        private Vector3 _oldTarget;
        private float _lerp = 1f;
        private float _scale = 1f;
        private bool _inited;

        public void Init(Transform target, Vector3 startingOffset, Transform bodyTransform,
            Transform groundTransform, LayerMask whatIsGround, float sphereRadius,
            float rayUpOffset, float rayLength,
            float stepTime, float stepHeight, float tipHeight, float newTargetDist)
        {
            _target = target;
            _startingOffset = startingOffset;
            _bodyTransform = bodyTransform;
            _groundTransform = groundTransform;
            _whatIsGround = whatIsGround;
            _sphereRadius = sphereRadius;
            _rayUpOffset = rayUpOffset;
            _rayLength = rayLength;
            _stepTime = Mathf.Max(stepTime, 0.05f);
            _stepHeight = stepHeight;
            _tipHeight = tipHeight;
            _newTargetDist = Mathf.Max(newTargetDist, 0.1f);
            _scale = transform.lossyScale.x;
            _inited = true;

            // Initial placement via single raycast (proven not to interfere)
            var origin = transform.position + transform.forward * _startingOffset.z
                       + _bodyTransform.up * _rayUpOffset * _scale;
            if (Physics.Raycast(origin, -_bodyTransform.up, out var hit, _rayLength * _scale, _whatIsGround))
            {
                _goalPos = hit.point + hit.normal * _tipHeight * _scale;
                _goalNormal = hit.normal;
            }
            else
            {
                _goalPos = ComputeFallbackPosition();
            }
            if (_target != null) _target.position = _goalPos;
            _lerp = 1f;
        }

        private Vector3 ComputeFallbackPosition()
        {
            if (_bodyTransform == null) return transform.position;
            var body = _bodyTransform;
            var up = body.up;
            var pos = transform.position + body.forward * _startingOffset.z * _scale;
            Vector3 groundPoint = _groundTransform != null
                ? _groundTransform.position
                : body.position - up * 0.5f * _scale;
            float d = Vector3.Dot(pos - groundPoint, up);
            return pos - up * d;
        }

        private void Update()
        {
            if (!_inited || _target == null) return;

            // Step animation
            if (_lerp < 1f)
            {
                var pos = Vector3.Lerp(_oldTarget, _goalPos, _lerp);
                pos += _goalNormal * Mathf.Sin(_lerp * Mathf.PI) * _stepHeight * _scale;
                _target.position = pos;
                _target.rotation = Quaternion.LookRotation(
                    Vector3.Cross(transform.right, _goalNormal), _goalNormal);
                _lerp += Time.deltaTime / (_stepTime * _scale);
            }
            else
            {
                _target.position = _goalPos;

                // Check if step needed (only when idle, not every frame during animation)
                if (_bodyTransform == null) return;
                var checkPos = transform.position + _bodyTransform.forward * _startingOffset.z * _scale;
                var dist = (checkPos - _goalPos).magnitude;
                if (dist <= _newTargetDist * _scale) return;

                // Need to step — do ONE SphereCast to find ground (catches thin web strands)
                var origin = checkPos + _bodyTransform.up * _rayUpOffset * _scale;
                Vector3 newGoal;
                Vector3 newNormal;
                if (Physics.SphereCast(origin, _sphereRadius * _scale, -_bodyTransform.up, out var hit, _rayLength * _scale, _whatIsGround))
                {
                    newGoal = hit.point + hit.normal * _tipHeight * _scale;
                    newNormal = hit.normal;
                }
                else
                {
                    newGoal = ComputeFallbackPosition();
                    newNormal = _bodyTransform.up;
                }

                _oldTarget = _goalPos;
                _goalPos = newGoal;
                _goalNormal = newNormal;
                _lerp = 0f;
            }
        }
    }

    internal static class CameraMouseLookPatches
    {
        public static bool OnLook_Prefix(object __0)
        {
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex))
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

        public static bool CallbackContextFilter_Prefix(object __instance, object __0)
        {
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            if (IsP2(__instance)) return true;

            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex))
                return false;

            return true;
        }

        public static void FixedUpdate_Prefix(object __instance)
        {
            if (!IsP2(__instance) && SplitScreenMod.FilterP1FromP2Gamepad && SplitScreenMod.P2UseGamepad)
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

        public static void WebControllerUpdate_Prefix()
        {
            SplitScreenMod.P2ShootHeld = InputCompat.IsP2ShootHeldNow(
                SplitScreenMod.P2UseGamepad,
                SplitScreenMod.P2GamepadIndex,
                SplitScreenMod.P2TriggerThreshold,
                "uKey",
                KeyCode.U
            );
        }

        public static bool CallbackContextFilter_Prefix(object __instance, object __0)
        {
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex))
                return false;

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
    }

    internal static class CameraControllerPatches
    {
        private static float _nextDebugLogAt;
        private static int _callbackHits;
        private static int _callbackBlockedP2;
        private static int _pollingBlockedP2;
        private static int _nonControllerPathSuspicions;

        public static bool CallbackContextFilter_Prefix(object[] __args)
        {
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            var args = __args;
            bool sawAnyCallbackArg = false;
            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (InputCompat.IsAnyCallbackContextArg(args[i]))
                    {
                        sawAnyCallbackArg = true;
                        _callbackHits++;
                    }

                    if (InputCompat.IsCallbackContextFromP2Gamepad(args[i], SplitScreenMod.P2GamepadIndex))
                    {
                        _callbackBlockedP2++;
                        return false;
                    }
                }
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
        private static PropertyInfo _buttonEastProp;
        private static PropertyInfo _rightShoulderProp;

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
                _buttonEastProp = _gamepadType.GetProperty("buttonEast", BindingFlags.Public | BindingFlags.Instance);
                _rightShoulderProp = _gamepadType.GetProperty("rightShoulder", BindingFlags.Public | BindingFlags.Instance);

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

        public static bool IsP2AttachPressedNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = Down(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetGamepadAtIndex(index);
            bool a = ReadButtonDown(gp, _buttonSouthProp);
            return kb || a;
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
