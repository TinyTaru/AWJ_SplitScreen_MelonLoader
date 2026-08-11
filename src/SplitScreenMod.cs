using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
        private static SplitScreenMod _instance;
        private const string HarmonyId = "AWJ.SplitScreen.P2Inject.v022";
        private HarmonyLib.Harmony _harmony;
        internal static bool SkipCallbackContextPatches;

        // Shared config/state for patches
        internal static bool P2UseGamepad = true;
        internal static int P2GamepadIndex = 1;        // second controller
        internal static float P2Deadzone = 0.15f;
        internal static float P2TriggerThreshold = 0.35f;
        internal static bool FilterP1FromP2Gamepad = true;
        internal static float P2CameraDistance = 5.6f;
        internal static bool DebugSpeedLog;
        internal static bool P2SprintDesired;          // authoritative P2 sprint state, held by FixedUpdate_Prefix

        internal static bool P2ShootHeld;              // computed each frame & from WebController.Update prefix
        internal static bool P2JumpPressed;            // set in OnUpdate, consumed in FixedUpdate
        internal static bool P1JumpPressed;            // bypass for shared jumpInputAction phase blocking when P2 holds South
        internal static bool InP2WebContext;           // one-shot actions
        internal static bool P2WebActive;              // last published P2 grapple/web state for movement patches
        internal static bool P2WebTargetActive;        // published P2 target-in-range state for HUD crosshair
        internal static Transform P2InputTransform;
        internal static Camera P2Camera;
        internal static Component P1BodyMovementInstance;
        internal static Component P2BodyMovementInstance;

        internal static FieldInfo BodyMove_MoveInputField;
        internal static FieldInfo BodyMove_MoveVectorField;
        internal static FieldInfo BodyMove_JumpInputField;
        internal static FieldInfo BodyMove_SprintInputField;
        internal static FieldInfo BodyMove_IsSprintingField;
        internal static FieldInfo BodyMove_MovementSpeedField;
        internal static FieldInfo BodyMove_BoostFactorField;
        internal static FieldInfo BodyMove_IsUnderwaterField;
        internal static FieldInfo BodyMove_UnderwaterFactorField;
        internal static FieldInfo BodyMove_PotionMultField;
        internal static FieldInfo BodyMove_TargetTransformField;
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
        private static MelonPreferences_Entry<bool> _debugSpeedLogPref;
        private static bool _swapPlayerControllers;

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
        private const string P2InteractKeyProp = "hKey";
        private const KeyCode P2InteractKeyFallback = KeyCode.H;

        private Camera _camLeftOrTop;
        private Camera _camRightOrBottom;

        private GameObject _p1Spider;
        internal static GameObject _p2Spider;
        private Transform _p1InputTransform;
        private readonly List<ReanchoredTransformParent> _p1CloneReanchors = new List<ReanchoredTransformParent>();
        private readonly Dictionary<Camera, int> _globalEffectCameraMasks = new Dictionary<Camera, int>();

        // Scene-load setup can fire before PlayerSpider's Start/leg-rig initialization has
        // finished.  P2 is cloned from P1, so cloning that transient state can preserve a
        // bad left/right leg pose.  A generation token also prevents overlapping
        // sceneLoaded coroutines (e.g. additive scene loads) from tearing down/rebuilding
        // split-screen on top of each other.
        private int _setupGeneration;
        private bool _currentSetupFromSceneLoad;

        private Vector3 _p2CamDir;        // direction from pivot to camera (normalized, derived from yaw/pitch)
        private Vector3 _p2SmoothUp;      // smoothed spider surface up (lerped each frame)
        private float _p2CamDistance;     // current dynamic distance (mirrors Cinemachine3rdPersonFollow.CameraDistance)
        private bool _p2CamRigInited;
        private float _p2CamYaw;
        private float _p2CamLookY;

        // --- Dynamic camera offset (mirrors P1's _Scripts.Camera.CameraZoom logic) ---
        // P1 FollowTarget pins the v-cam follow target at (spider.position + spider.up * 1f)
        // each FixedUpdate. Cinemachine3rdPersonFollow then applies an additional
        // shoulder offset above that target. P2 mirrors both pieces.
        private const float P2CamPivotOffset = 1.0f;

        // P1 CameraZoom reflection cache (settings are shared between P1 and P2).
        private object _p1CameraZoom;
        private float _p1MinZoom = 3.0f;
        private float _p1MaxZoom = 12.0f;
        private int _p1ZoomSteps = 6;
        private bool _p1ZoomInWhenLookingUp;
        private bool _p1CameraZoomCached;
        private bool _p1CameraMouseLookCached;
        private bool _p1ClampLookY = true;
        private float _p1MinLookY = -80f;
        private float _p1MaxLookY = 80f;

        // P2 manual zoom mirrors CameraZoom.MobileZoom / OnZoom(button path):
        // CameraZoom.cs builds `zoomArray[zoomSteps]` where each element is:
        //   minZoom + i * (maxZoom - minZoom) / (zoomSteps - 1)
        // and advances `zoomIndex` on each button press. P2 keeps its own
        // independent manual zoom/index but uses the same exact step values.
        private float[] _p2ZoomArray;
        private int _p2ZoomIndex = -1;
        private float _p2ManualZoom = 5.6f;   // always re-seeded from P2CameraDistance before use

        // P2 BodyMovement reflection cache (for velocity / state input to the zoom curve).
        private Component _p2BodyMovement;
        private PropertyInfo _bmRbProp, _bmStateProp, _bmWebTouchedProp;
        private object _bmWalkingState;

        // SettingsController.AutoZoom static accessor.
        private PropertyInfo _settingsAutoZoomProp;

        // Exponentially-decayed zoom (mirrors CameraZoom.cameraDistance private field).
        private float _p2CamSmoothedZoom = -1f;

        // --- Camera collision (mirrors Cinemachine3rdPersonFollow built-in collision) ---
        // P1 has no scripted collision; it relies entirely on Cinemachine3rdPersonFollow's
        // CameraRadius / CameraCollisionFilter / IgnoreTag / Damping(Into|From)Collision.
        // We replicate that with a SphereCast + asymmetric exponential damping for P2.
        private bool _p1FollowCached;
        private float _p2CamRadius = 0.2f;
        private LayerMask _p2CamCollisionMask = ~0;          // default everything
        private string _p2CamIgnoreTag = "";
        private float _p2CamDampingIn = 0.1f;                // snappy when obstacle pushes us in
        private float _p2CamDampingOut = 0.5f;               // smooth when obstacle clears
        private float _p2CamCollidedDistance = -1f;          // damped collision-clamped distance
        private Collider[] _p2SelfColliders;                 // cached spider colliders to ignore
        private int _p2SelfColliderRefreshFrame = -1;
        private float _p2CamShoulderHeight = 3f;
        private float _p2CamVerticalArm = 0f;

        private Component _webController;
        private P2WebManager _p2WebManager;
        private Component _p2SpiderInteraction;
        private MethodInfo _p2SpiderMobileInteractMethod;
        private MethodInfo _p1MobileShootWebMethod;
        private FieldInfo _p1WebActiveField;
        private FieldInfo _p1SpringJointField;
        private MethodInfo _p1DeactivateSpringJointMethod;
        private MethodInfo _p1ReleaseWebMethod;
        private bool _p1ShootHeldPrev;

        internal static bool IsSplitScreenActive
        {
            get
            {
                return _instance != null
                    && _enabled != null
                    && _enabled.Value
                    && _instance._camLeftOrTop != null
                    && _instance._camRightOrBottom != null;
            }
        }

        internal static Camera P1Camera
        {
            get { return _instance != null ? _instance._camLeftOrTop : null; }
        }

        public override void OnInitializeMelon()
        {
            _instance = this;
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
            _p2CameraDistancePref = _prefs.CreateEntry("P2_CameraDistance", 14.0f, "P2 third-person camera distance (near P1's typical distance)");
            _debugSpeedLogPref = _prefs.CreateEntry("Debug_SpeedLog", false, "Log movement plus detailed P1/P2 camera input, driver, pose and zoom diagnostics");

            // One-time default correction: the log showed P1 sits far (~16) while the
            // old 8.0 / interim 5.6 defaults left P2 too close. A per-value marker keeps
            // a deliberately-chosen distance untouched.
            var p2CamDistMigrated = _prefs.CreateEntry("P2_CameraDistance_Migrated", false,
                "Internal: one-time marker for the P2_CameraDistance default correction");
            if (!p2CamDistMigrated.Value)
            {
                float cur = _p2CameraDistancePref.Value;
                if (Mathf.Approximately(cur, 8.0f) || Mathf.Approximately(cur, 5.6f))
                {
                    _p2CameraDistancePref.Value = 14.0f;
                    LoggerInstance.Msg("P2_CameraDistance corrected to 14.0 (closer to P1's typical distance).");
                }
                p2CamDistMigrated.Value = true;
            }

            ApplyPrefsToStatics();

            InputCompat.Init(LoggerInstance);
            InstallHarmonyPatches();
            AWJSplitScreenUpdateFix.UpdateFixMod.Initialize(
                message => LoggerInstance.Msg(message),
                message => LoggerInstance.Error(message));

            SceneManager.sceneLoaded += OnSceneLoaded;

            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

            LoggerInstance.Msg("AWJ Split Screen + P2 Inject v0.2.2 loaded.");
            LoggerInstance.Msg("F8 swap controllers, F9 split, F10 orientation | P2 Move: IJKL or Gamepad LStick | P2 Sprint: LStick click (toggles on/off in all modes) | P2 Look: N/M or RStickX | P2 Zoom: RStick press | P2 Jump: A | P2 Interact: H/X | P2 Web: RT shoot/release, LT quick build, LB fixed anchor, RB moving anchor, B delete/cancel.");
            LoggerInstance.Msg("Diagnostics: set Debug_SpeedLog=true in MelonPreferences.cfg to log movement and detailed camera input/driver/pose state. Press F7 to dump all task/quest states.");
            LoggerInstance.Msg("Tip: If both controllers still move P1, ensure FilterP1FromP2Gamepad=true and P2_GamepadIndex is the second pad (usually 1).");
        }

        public static bool P1CameraUnderwater = false;
        public static bool P2CameraUnderwater = false;
        public static VolumeProfile[] TrackedWaterProfiles = null;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            int generation = ++_setupGeneration;
            MelonCoroutines.Start(DeferredSetup(true, generation));
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (TrackedWaterProfiles == null) return;
            bool isP2 = (camera != null && camera == _camRightOrBottom);
            bool isP1 = (camera != null && camera == _camLeftOrTop);
            
            // Default to false for UI/other cameras?
            bool waterState = false;
            if (isP1) waterState = P1CameraUnderwater;
            else if (isP2) waterState = P2CameraUnderwater;
            // if neither, it might be a UI camera, skip or apply false. We'll apply false.

            ApplyWaterProfileState(waterState);
        }

        private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (TrackedWaterProfiles == null) return;
            // Revert state so other effects don't get the filter accidentally,
            // though the next camera's OnBegin will set it again.
            ApplyWaterProfileState(false);
        }

        private static void ApplyWaterProfileState(bool value)
        {
            if (TrackedWaterProfiles == null)
                return;

            foreach (var profile in TrackedWaterProfiles)
            {
                if (profile == null) continue;
                if (profile.TryGet<ColorAdjustments>(out var ca))
                    ca.colorFilter.overrideState = value;
                if (profile.TryGet<PaniniProjection>(out var pp))
                    pp.active = value;
            }
        }

        private static void ResetWaterRenderingState()
        {
            ApplyWaterProfileState(false);
            P1CameraUnderwater = false;
            P2CameraUnderwater = false;
            TrackedWaterProfiles = null;
        }

        private void ApplyPrefsToStatics()
        {
            P2UseGamepad = _p2UseGamepadPref.Value;
            P2GamepadIndex = ResolveP2GamepadIndex(_p2GamepadIndexPref.Value);
            P2Deadzone = _p2DeadzonePref.Value;
            P2TriggerThreshold = _p2TriggerThresholdPref.Value;
            FilterP1FromP2Gamepad = _filterP1FromP2PadPref.Value;
            P2CameraDistance = Mathf.Clamp(_p2CameraDistancePref.Value, 1.0f, 14f);
            DebugSpeedLog = _debugSpeedLogPref.Value;
        }

        private int ResolveP2GamepadIndex(int configuredIndex)
        {
            int normalized = Mathf.Max(0, configuredIndex);
            if (!_swapPlayerControllers)
                return normalized;

            int count = InputCompat.GetConnectedGamepadCount();
            if (count < 2)
                return normalized;

            if (normalized != 0)
                return 0;

            return count > 1 ? 1 : 0;
        }

        private void ToggleControllerAssignments()
        {
            if (!P2UseGamepad)
            {
                LoggerInstance.Warning("Can't swap controllers while P2 gamepad input is disabled.");
                return;
            }

            int count = InputCompat.GetConnectedGamepadCount();
            if (count < 2)
            {
                LoggerInstance.Warning("Need at least two connected gamepads to swap controller ownership.");
                return;
            }

            _swapPlayerControllers = !_swapPlayerControllers;
            ApplyPrefsToStatics();

            LoggerInstance.Msg(_swapPlayerControllers
                ? "Controllers swapped. P2 now uses the primary gamepad."
                : "Controllers restored. P2 is back on its configured gamepad.");
        }

        public override void OnDeinitializeMelon()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            Teardown();
            AWJSplitScreenUpdateFix.UpdateFixMod.Deinitialize();
            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
                _harmony = null;
            }
            _instance = null;
        }

        private void InstallHarmonyPatches()
        {
            _harmony = new HarmonyLib.Harmony(HarmonyId);
            var h = _harmony;
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
                    BodyMove_SprintInputField = FindFieldByName(bodyMoveType, "sprintInput");
                    BodyMove_IsSprintingField = FindFieldByName(bodyMoveType, "isSprinting");
                    BodyMove_MovementSpeedField = FindFieldByName(bodyMoveType, "movementSpeed");
                    BodyMove_BoostFactorField = FindFieldByName(bodyMoveType, "movementBoostFactor");
                    BodyMove_IsUnderwaterField = FindFieldByName(bodyMoveType, "isUnderwater");
                    BodyMove_UnderwaterFactorField = FindFieldByName(bodyMoveType, "movementUnderwaterFactor");
                    BodyMove_PotionMultField = FindFieldByName(bodyMoveType, "currentAncientPotionSpeedMultiplier");
                    BodyMove_TargetTransformField = FindFieldByName(bodyMoveType, "targetTransform");
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
                            prefix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.FixedUpdate_Prefix)),
                            postfix: new HarmonyMethod(typeof(BodyMovementPatches), nameof(BodyMovementPatches.FixedUpdate_Postfix)));

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

            // SpiderInteraction patches (P2 interaction/collectible support while keeping isPlayer=false globally)
            try
            {
                var spiderInteractionType = AccessTools.TypeByName("_Scripts.Spider.SpiderInteraction");
                if (spiderInteractionType != null)
                {
                    var start = AccessTools.Method(spiderInteractionType, "Start");
                    if (start != null)
                        h.Patch(start,
                            prefix: new HarmonyMethod(typeof(SpiderInteractionPatches), nameof(SpiderInteractionPatches.TemporarilyEnableIsPlayer_Prefix)),
                            postfix: new HarmonyMethod(typeof(SpiderInteractionPatches), nameof(SpiderInteractionPatches.TemporarilyEnableIsPlayer_Postfix)));

                    var onTriggerEnter = spiderInteractionType.GetMethod("OnTriggerEnter",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                        null,
                        new Type[] { typeof(Collider) },
                        null);
                    if (onTriggerEnter != null)
                        h.Patch(onTriggerEnter,
                            prefix: new HarmonyMethod(typeof(SpiderInteractionPatches), nameof(SpiderInteractionPatches.TemporarilyEnableIsPlayer_Prefix)),
                            postfix: new HarmonyMethod(typeof(SpiderInteractionPatches), nameof(SpiderInteractionPatches.TemporarilyEnableIsPlayer_Postfix)));

                    var onTriggerExit = spiderInteractionType.GetMethod("OnTriggerExit",
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                        null,
                        new Type[] { typeof(Collider) },
                        null);
                    if (onTriggerExit != null)
                        h.Patch(onTriggerExit,
                            prefix: new HarmonyMethod(typeof(SpiderInteractionPatches), nameof(SpiderInteractionPatches.TemporarilyEnableIsPlayer_Prefix)),
                            postfix: new HarmonyMethod(typeof(SpiderInteractionPatches), nameof(SpiderInteractionPatches.TemporarilyEnableIsPlayer_Postfix)));

                    var mobileInteract = AccessTools.Method(spiderInteractionType, "MobileInteract");
                    if (mobileInteract != null)
                        h.Patch(mobileInteract,
                            prefix: new HarmonyMethod(typeof(SpiderInteractionPatches), nameof(SpiderInteractionPatches.TemporarilyEnableIsPlayer_Prefix)),
                            postfix: new HarmonyMethod(typeof(SpiderInteractionPatches), nameof(SpiderInteractionPatches.TemporarilyEnableIsPlayer_Postfix)));

                    LoggerInstance.Msg("Patched SpiderInteraction: Start + TriggerEnter/Exit + MobileInteract for P2.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("SpiderInteraction patch block failed (non-fatal): " + ex);
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
                    // NOTE: We intentionally do NOT patch Update/FixedUpdate with a P2ShootHeld setter.
                    // P2WebManager sets context flags only around its own explicit invocations
                    // so P1's targeting is never corrupted.

                    // Try property getter first, then direct method
                    var getStart = AccessTools.PropertyGetter(webType, "WebStartPoint");
                    if (getStart == null) getStart = AccessTools.Method(webType, "get_WebStartPoint");
                    if (getStart != null)
                    {
                        if (getStart.ReturnType == typeof(Transform))
                            h.Patch(getStart, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebStartPointTransform_Prefix)));
                        else if (getStart.ReturnType == typeof(Vector3))
                            h.Patch(getStart, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.WebStartPointVector3_Prefix)));
                    }

                    var getDir = AccessTools.PropertyGetter(webType, "WebDirection");
                    if (getDir == null) getDir = AccessTools.Method(webType, "get_WebDirection");
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
                        if (checkForWebTarget != null)
                            h.Patch(checkForWebTarget, prefix: new HarmonyMethod(typeof(WebControllerPatches), nameof(WebControllerPatches.CheckForWebTarget_Prefix)));
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

            // GameplayUI crosshair patch
            try
            {
                var gameplayUiType = AccessTools.TypeByName("_Scripts.UI.HUD.GameplayUI");
                if (gameplayUiType != null)
                {
                    var update = AccessTools.Method(gameplayUiType, "Update");
                    if (update != null)
                        h.Patch(update, postfix: new HarmonyMethod(typeof(GameplayUIPatches), nameof(GameplayUIPatches.Update_Postfix)));

                    LoggerInstance.Msg("Patched GameplayUI.Update for split-screen crosshair placement.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("GameplayUI patch block failed (non-fatal): " + ex);
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

            // CameraZoom.OnZoom patch — blocks P2 gamepad right-stick click from reaching P1 zoom
            try
            {
                var cameraZoomType = AccessTools.TypeByName("_Scripts.Camera.CameraZoom");
                if (cameraZoomType != null)
                {
                    var onZoom = AccessTools.Method(cameraZoomType, "OnZoom");
                    if (onZoom != null)
                    {
                        if (!SkipCallbackContextPatches)
                        {
                            h.Patch(onZoom, prefix: new HarmonyMethod(typeof(CameraZoomPatches), nameof(CameraZoomPatches.OnZoom_Prefix)));
                            LoggerInstance.Msg("Patched CameraZoom.OnZoom to block P2 gamepad input.");
                        }
                        else
                        {
                            LoggerInstance.Warning("Skipped CameraZoom.OnZoom patch due to IL2CPP callback compatibility mode.");
                        }
                    }
                    else
                    {
                        LoggerInstance.Warning("CameraZoom.OnZoom not found.");
                    }
                }
                else
                {
                    LoggerInstance.Warning("CameraZoom type not found.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("CameraZoom patch failed (non-fatal): " + ex);
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

            // CameraController.MainCamera getter / GetCameraDistance() — web targeting reads both.
            try
            {
                var camType = AccessTools.TypeByName("_Scripts.Singletons.CameraController");
                if (camType != null)
                {
                    var mainCamGetter = AccessTools.PropertyGetter(camType, "MainCamera");
                    if (mainCamGetter == null) mainCamGetter = AccessTools.Method(camType, "get_MainCamera");
                    if (mainCamGetter != null)
                    {
                        h.Patch(mainCamGetter, prefix: new HarmonyMethod(typeof(CameraControllerMainCameraPatches), nameof(CameraControllerMainCameraPatches.MainCamera_Prefix)));
                        LoggerInstance.Msg("Patched CameraController.MainCamera getter for P2.");
                    }
                    else
                    {
                        LoggerInstance.Warning("CameraController.MainCamera getter not found.");
                    }

                    var getCameraDistance = AccessTools.Method(camType, "GetCameraDistance");
                    if (getCameraDistance != null)
                    {
                        h.Patch(getCameraDistance, prefix: new HarmonyMethod(typeof(CameraControllerMainCameraPatches), nameof(CameraControllerMainCameraPatches.GetCameraDistance_Prefix)));
                        LoggerInstance.Msg("Patched CameraController.GetCameraDistance for P2.");
                    }
                    else
                    {
                        LoggerInstance.Warning("CameraController.GetCameraDistance not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("CameraController patch failed (non-fatal): " + ex);
            }

            // MainWebVisuals event handlers — suppress P1's reaction while in P2 web context.
            try
            {
                var asm = typeof(UnityEngine.GameObject).Assembly; // not the right one
                Type mwvType = null;
                try { mwvType = AccessTools.TypeByName("_Scripts.Web.MainWebVisuals"); } catch { }
                if (mwvType == null)
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        try
                        {
                            var t = a.GetType("_Scripts.Web.MainWebVisuals", false);
                            if (t != null) { mwvType = t; break; }
                        }
                        catch { }
                    }
                }

                if (mwvType != null)
                {
                    var actMethod = AccessTools.Method(mwvType, "WebController_OnMainWebActivated");
                    var deactMethod = AccessTools.Method(mwvType, "WebController_OnMainWebDeactivated");

                    if (actMethod != null)
                    {
                        h.Patch(actMethod, prefix: new HarmonyMethod(typeof(MainWebVisualsPatches), nameof(MainWebVisualsPatches.OnMainWebActivated_Prefix)));
                        LoggerInstance.Msg("Patched MainWebVisuals.WebController_OnMainWebActivated.");
                    }
                    else
                    {
                        LoggerInstance.Warning("MainWebVisuals.WebController_OnMainWebActivated not found.");
                    }

                    if (deactMethod != null)
                    {
                        h.Patch(deactMethod, prefix: new HarmonyMethod(typeof(MainWebVisualsPatches), nameof(MainWebVisualsPatches.OnMainWebDeactivated_Prefix)));
                        LoggerInstance.Msg("Patched MainWebVisuals.WebController_OnMainWebDeactivated.");
                    }
                    else
                    {
                        LoggerInstance.Warning("MainWebVisuals.WebController_OnMainWebDeactivated not found.");
                    }
                }
                else
                {
                    LoggerInstance.Warning("MainWebVisuals type not found — P1 visuals may bleed into P2 actions.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("MainWebVisuals patch failed (non-fatal): " + ex);
            }

            // BodyMovement.SetIsUnderwater — original early-returns when !isPlayer, so P2
            // entering water does nothing. Prefix detects P2's instance and drives our
            // own underwater counter + tells MusicController to start/stop the
            // underwater ambience loop based on whether *either* player is underwater.
            try
            {
                var bmType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                if (bmType != null)
                {
                    var setUw = bmType.GetMethod("SetIsUnderwater",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, new Type[] { typeof(bool) }, null);
                    if (setUw != null)
                    {
                        h.Patch(setUw,
                            prefix: new HarmonyMethod(typeof(BodyMovementUnderwaterPatches),
                                nameof(BodyMovementUnderwaterPatches.SetIsUnderwater_Prefix)));
                        LoggerInstance.Msg("Patched BodyMovement.SetIsUnderwater for P2 audio.");
                    }
                    BodyMovementUnderwaterPatches.IsUnderwaterField =
                        bmType.GetField("isUnderwater",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("BodyMovement.SetIsUnderwater patch failed (non-fatal): " + ex);
            }

            // MusicController.StopUnderwater — if P2 is still underwater when P1 exits
            // water, don't stop the loop. (And vice versa.)
            try
            {
                var mcType = AccessTools.TypeByName("_Scripts.Singletons.MusicController");
                if (mcType != null)
                {
                    var stopUw = mcType.GetMethod("StopUnderwater",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, Type.EmptyTypes, null);
                    if (stopUw != null)
                    {
                        h.Patch(stopUw,
                            prefix: new HarmonyMethod(typeof(MusicControllerUnderwaterPatches),
                                nameof(MusicControllerUnderwaterPatches.StopUnderwater_Prefix)));
                        LoggerInstance.Msg("Patched MusicController.StopUnderwater for split underwater state.");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("MusicController.StopUnderwater patch failed (non-fatal): " + ex);
            }
                        try
            {
                var cwtType = AccessTools.TypeByName("_Scripts.Camera.CameraWaterTrigger");
                if (cwtType != null)
                {
                    var m = cwtType.GetMethod("EnableUnderWaterPostProcessing", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (m != null)
                    {
                        h.Patch(m, prefix: new HarmonyMethod(typeof(CameraWaterTriggerPatches), nameof(CameraWaterTriggerPatches.EnableUnderWaterPostProcessing_Prefix)));
                        LoggerInstance.Msg("Patched CameraWaterTrigger for per-player filter.");
                    }
                    var startMethod = cwtType.GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (startMethod != null)
                    {
                        h.Patch(startMethod, postfix: new HarmonyMethod(typeof(CameraWaterTriggerPatches), nameof(CameraWaterTriggerPatches.Start_Postfix)));
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("CameraWaterTrigger patch failed (non-fatal): " + ex);
            }

            // WebThread.DeleteWebThread — when a WebThread is about to be destroyed,
            // detach any P2 transforms (spider, targetTransform) that may be parented
            // to it. BodyMovement.PerformWalking parents the spider to the surface it
            // walks on; if that surface is the WebThread being deleted, Unity would
            // otherwise destroy the P2 spider GameObject with it.
            try
            {
                var wtType = AccessTools.TypeByName("_Scripts.Web.WebThread");
                if (wtType != null)
                {
                    var delMethod = wtType.GetMethod("DeleteWebThread",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (delMethod != null)
                    {
                        h.Patch(delMethod,
                            prefix: new HarmonyMethod(typeof(WebThreadDeletePatches),
                                nameof(WebThreadDeletePatches.DeleteWebThread_Prefix)));
                        LoggerInstance.Msg("Patched WebThread.DeleteWebThread to detach P2 transforms.");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("WebThread.DeleteWebThread patch failed (non-fatal): " + ex);
            }

            // Immediate Unity hierarchy destruction — if P2 is parented to a surface being
            // destroyed right now, detach first so the spider is not destroyed with it.
            try
            {
                var destroyNow = typeof(UnityEngine.Object).GetMethod("Destroy",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(UnityEngine.Object), typeof(float) },
                    null);
                if (destroyNow != null)
                {
                    h.Patch(destroyNow,
                        prefix: new HarmonyMethod(typeof(UnityDestroyDetachPatches),
                            nameof(UnityDestroyDetachPatches.Destroy_Prefix)));
                }

                var destroyImmediate = typeof(UnityEngine.Object).GetMethod("DestroyImmediate",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(UnityEngine.Object), typeof(bool) },
                    null);
                if (destroyImmediate != null)
                {
                    h.Patch(destroyImmediate,
                        prefix: new HarmonyMethod(typeof(UnityDestroyDetachPatches),
                            nameof(UnityDestroyDetachPatches.DestroyImmediate_Prefix)));
                }

                if (destroyNow != null || destroyImmediate != null)
                    LoggerInstance.Msg("Patched immediate Unity destroy paths to detach grounded P2 before hierarchy teardown.");
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("Unity destroy detach patch failed (non-fatal): " + ex);
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

        private System.Collections.IEnumerator DeferredSetup(bool waitForP1Initialization, int generation)
        {
            // sceneLoaded is earlier than Start() for a number of gameplay components.
            // F9 is normally pressed long after that point, so only the automatic
            // scene-load path needs the stronger readiness gate.
            yield return null;

            // A newer sceneLoaded/F9 request supersedes this coroutine.  This matters for
            // additive scene loads: without a token, an older coroutine can wake up later
            // and tear down a P2 that a newer setup just created.
            if (generation != _setupGeneration)
                yield break;

            Teardown();

            // Scene loads destroy the P1 camera components these caches point at, so
            // drop them on every setup pass (scene load or F9-enable both come through
            // here) and let EnsureCameraDynamicsCached re-resolve once.
            _p1CameraZoom = null;
            _p1CameraZoomCached = false;
            _p1CameraMouseLookCached = false;
            _p1FollowCached = false;
            ResetP1ShoulderIsolationCache();

            if (_enabled != null && !_enabled.Value)
                yield break;

            if (waitForP1Initialization)
            {
                const float timeoutSeconds = 15f;
                const float requiredStableSeconds = 0.50f;
                const int requiredStableFrames = 8;

                float deadline = Time.realtimeSinceStartup + timeoutSeconds;
                float stableSince = -1f;
                int stableFrames = 0;
                int stablePlayerId = 0;
                string lastReason = "PlayerSpider has not spawned";
                bool ready = false;

                while (Time.realtimeSinceStartup < deadline)
                {
                    if (generation != _setupGeneration)
                        yield break;
                    if (_enabled != null && !_enabled.Value)
                        yield break;

                    GameObject p1 = FindPlayerSpider();
                    string reason;
                    bool frameReady = IsP1SafeToCloneForP2(p1, out reason);
                    int playerId = p1 != null ? p1.GetInstanceID() : 0;

                    if (frameReady)
                    {
                        if (stablePlayerId != playerId)
                        {
                            stablePlayerId = playerId;
                            stableSince = Time.realtimeSinceStartup;
                            stableFrames = 1;
                        }
                        else
                        {
                            stableFrames++;
                        }

                        if (stableSince < 0f)
                            stableSince = Time.realtimeSinceStartup;

                        if (stableFrames >= requiredStableFrames &&
                            Time.realtimeSinceStartup - stableSince >= requiredStableSeconds)
                        {
                            ready = true;
                            break;
                        }
                    }
                    else
                    {
                        stablePlayerId = playerId;
                        stableSince = -1f;
                        stableFrames = 0;
                        lastReason = reason;
                    }

                    yield return null;
                }

                if (!ready)
                {
                    LoggerInstance.Warning(
                        "Automatic P2 spawn skipped because P1 did not reach a safe clone-ready state within " +
                        timeoutSeconds.ToString("F0") + "s. Last state: " + lastReason +
                        ". F9 can still be used after the level finishes loading.");
                    yield break;
                }

                // One extra rendered frame after the stable window guarantees that any
                // Start/OnEnable work that made the readiness fields valid has returned
                // before Instantiate snapshots P1.
                yield return null;
                if (generation != _setupGeneration)
                    yield break;

                LoggerInstance.Msg(
                    "P1 is fully initialized for P2 cloning (leg targets + MasterLegController stable for " +
                    requiredStableSeconds.ToString("F2") + "s).");
            }
            else if (!CanUseSplitScreenInCurrentScene())
            {
                LoggerInstance.Msg("Split-screen setup deferred until PlayerSpider has spawned.");
                yield break;
            }

            // Re-check even after the readiness wait in case a scene transition destroyed
            // P1 between the final validation frame and setup.
            if (!CanUseSplitScreenInCurrentScene())
            {
                LoggerInstance.Msg("Split-screen setup deferred until PlayerSpider has spawned.");
                yield break;
            }

            if (generation != _setupGeneration)
                yield break;

            _currentSetupFromSceneLoad = waitForP1Initialization;
            SetupCameras();
            CacheWebController();

            if (_spawnSecondSpider != null && _spawnSecondSpider.Value)
                SetupSecondSpider();

            if (_spawnSecondSpider != null && _spawnSecondSpider.Value && _p2Spider == null)
            {
                LoggerInstance.Warning("Split-screen setup aborted because P2 could not be summoned.");
                if (_enabled != null) _enabled.Value = false;
                Teardown();
                yield break;
            }

            // Allow P2 (layer 2 = Ignore Raycast) into the snow/dust effect cameras'
            // culling masks so P2 also leaves snow trails / piano dust trails.
            ApplyP2LayerToGlobalEffectCameras();
        }

        /// <summary>
        /// Returns true only after P1's runtime spider/leg state is actually initialized,
        /// not merely after a GameObject named PlayerSpider exists.
        ///
        /// The key signal is LegController.targetLocal.  The game creates/populates those
        /// runtime foot anchors during leg initialization.  Cloning before they exist (or
        /// before MasterLegController has collected the legs) can snapshot a transient
        /// animation/IK state.  This is exactly the window sceneLoaded can hit but a later
        /// manual F9 spawn normally cannot.
        /// </summary>
        private static bool IsP1SafeToCloneForP2(GameObject p1, out string reason)
        {
            reason = null;
            if (p1 == null)
            {
                reason = "PlayerSpider missing";
                return false;
            }

            if (!p1.activeInHierarchy)
            {
                reason = "PlayerSpider inactive";
                return false;
            }

            if (!p1.scene.IsValid() || !p1.scene.isLoaded)
            {
                reason = "PlayerSpider scene not loaded";
                return false;
            }

            if (p1.GetComponent<Rigidbody>() == null)
            {
                reason = "root Rigidbody missing";
                return false;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            // BodyMovement.Start establishes runtime references used by the clone path.
            var bodyType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
            if (bodyType != null)
            {
                var body = p1.GetComponentInChildren(bodyType, true) as Component;
                if (body == null)
                {
                    reason = "BodyMovement missing";
                    return false;
                }

                var bodyBehaviour = body as Behaviour;
                if (bodyBehaviour != null && !bodyBehaviour.isActiveAndEnabled)
                {
                    reason = "BodyMovement not active";
                    return false;
                }

                var targetTransformField = bodyType.GetField("targetTransform", flags);
                if (targetTransformField != null)
                {
                    Transform targetTransform = null;
                    try { targetTransform = targetTransformField.GetValue(body) as Transform; } catch { }
                    if (targetTransform == null)
                    {
                        reason = "BodyMovement.targetTransform not initialized";
                        return false;
                    }
                }
            }

            var legType = AccessTools.TypeByName("_Scripts.Spider.LegController");
            if (legType == null)
            {
                reason = "LegController type unavailable";
                return false;
            }

            var legs = p1.GetComponentsInChildren(legType, true);
            if (legs == null || legs.Length < 8)
            {
                reason = "only " + (legs == null ? 0 : legs.Length) + "/8 LegControllers present";
                return false;
            }

            var targetField = legType.GetField("target", flags);
            var centerField = legType.GetField("center", flags);
            var targetLocalField = legType.GetField("targetLocal", flags);

            for (int i = 0; i < legs.Length; i++)
            {
                var leg = legs[i];
                var comp = leg as Component;
                if (comp == null)
                {
                    reason = "null LegController component";
                    return false;
                }

                if (targetField != null)
                {
                    Transform target = null;
                    try { target = targetField.GetValue(leg) as Transform; } catch { }
                    if (target == null)
                    {
                        reason = comp.name + ".target not initialized";
                        return false;
                    }
                }

                if (centerField != null)
                {
                    Transform center = null;
                    try { center = centerField.GetValue(leg) as Transform; } catch { }
                    if (center == null)
                    {
                        reason = comp.name + ".center not initialized";
                        return false;
                    }
                }

                // targetLocal is the strongest runtime-started signal. If this field exists
                // in this game version, require every leg to have created its anchor.
                if (targetLocalField != null)
                {
                    Transform targetLocal = null;
                    try { targetLocal = targetLocalField.GetValue(leg) as Transform; } catch { }
                    if (targetLocal == null)
                    {
                        reason = comp.name + ".targetLocal not initialized";
                        return false;
                    }
                }
            }

            // MasterLegController's list is populated by the individual leg startup path.
            var masterType = AccessTools.TypeByName("_Scripts.Spider.MasterLegController");
            if (masterType != null)
            {
                var master = p1.GetComponentInChildren(masterType, true) as Component;
                if (master == null)
                {
                    reason = "MasterLegController missing";
                    return false;
                }

                var legsField = masterType.GetField("legs", flags);
                if (legsField != null)
                {
                    int registeredCount = -1;
                    try
                    {
                        var collection = legsField.GetValue(master) as System.Collections.ICollection;
                        if (collection != null)
                            registeredCount = collection.Count;
                    }
                    catch { }

                    if (registeredCount >= 0 && registeredCount < legs.Length)
                    {
                        reason = "MasterLegController has only " + registeredCount + "/" + legs.Length + " registered legs";
                        return false;
                    }
                }
            }

            // The visual rig should also have passed Animator initialization before it is
            // cloned. Ignore inactive/disabled animators (alternate visual modes may keep
            // some around intentionally).
            var animators = p1.GetComponentsInChildren<Animator>(true);
            bool sawActiveAnimator = false;
            bool sawInitializedAnimator = false;
            for (int i = 0; i < animators.Length; i++)
            {
                var animator = animators[i];
                if (animator == null || !animator.enabled || !animator.gameObject.activeInHierarchy)
                    continue;

                sawActiveAnimator = true;
                if (animator.isInitialized)
                    sawInitializedAnimator = true;
            }

            if (sawActiveAnimator && !sawInitializedAnimator)
            {
                reason = "Animator not initialized";
                return false;
            }

            reason = "ready";
            return true;
        }

        private void ApplyP2LayerToGlobalEffectCameras()
        {
            const int p2LayerMask = 1 << 2; // Ignore Raycast — P2 spider is on this layer.
            try
            {
                var types = new[] {
                    AccessTools.TypeByName("_Scripts.Miscellaneous.Christmas.SnowController"),
                    AccessTools.TypeByName("_Scripts.LivingRoom.PianoDust")
                };
                foreach (var t in types)
                {
                    if (t == null) continue;
                    var comps = UnityEngine.Object.FindObjectsOfType(t, true);
                    if (comps == null) continue;
                    var camFields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (var c in comps)
                    {
                        foreach (var fi in camFields)
                        {
                            if (fi.FieldType != typeof(Camera)) continue;
                            try
                            {
                                var cam = fi.GetValue(c) as Camera;
                                if (cam != null && (cam.cullingMask & p2LayerMask) == 0)
                                {
                                    if (!_globalEffectCameraMasks.ContainsKey(cam))
                                        _globalEffectCameraMasks.Add(cam, cam.cullingMask);
                                    cam.cullingMask |= p2LayerMask;
                                    LoggerInstance.Msg("Added P2 layer to " + t.Name + "." + fi.Name + " cullingMask.");
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("ApplyP2LayerToGlobalEffectCameras failed (non-fatal): " + ex);
            }
        }

        public override void OnUpdate()
        {
            ApplyPrefsToStatics();

            if (InputCompat.Down_F9())
            {
                // Invalidate any scene-load setup coroutine before applying the manual
                // toggle.  Manual F9 occurs after gameplay is interactable, so it does not
                // need the scene-start stabilization delay.
                int generation = ++_setupGeneration;
                _enabled.Value = !_enabled.Value;
                if (_enabled.Value)
                {
                    if (!CanUseSplitScreenInCurrentScene())
                    {
                        _enabled.Value = false;
                        LoggerInstance.Warning("Split-screen can't be enabled here. Enter gameplay first.");
                    }
                    else
                    {
                        MelonCoroutines.Start(DeferredSetup(false, generation));
                        LoggerInstance.Msg("Split-screen enabled.");
                    }
                }
                else
                {
                    Teardown();
                    LoggerInstance.Msg("Split-screen disabled.");
                }
            }

            if (InputCompat.Down_F10())
            {
                if (_camRightOrBottom == null || _p2Spider == null)
                {
                    LoggerInstance.Warning("Can't switch split layout before P2 is summoned.");
                }
                else
                {
                    _splitMode.Value = string.Equals(_splitMode.Value, "Vertical", StringComparison.OrdinalIgnoreCase)
                        ? "Horizontal"
                        : "Vertical";
                    ApplyCameraRects();
                    LoggerInstance.Msg("Split mode: " + _splitMode.Value);
                }
            }

            if (InputCompat.Down_F8())
                ToggleControllerAssignments();

            if (InputCompat.Down_F7())
                DumpQuestStates();

            if (_enabled.Value)
            {
                if (InputCompat.IsP2JumpPressedNow(P2UseGamepad, P2GamepadIndex))
                    P2JumpPressed = true;

                if (InputCompat.IsP2SprintPressedNow(P2UseGamepad, P2GamepadIndex))
                    TriggerP2SprintToggle();

                // P1 jump bypass: shared jumpInputAction can get stuck in Performed
                // phase while P2 holds South, suppressing P1's `performed` callback.
                // Poll P1's input directly and force P1.jumpInput=true in FixedUpdate.
                if (InputCompat.IsP1JumpPressedNow(P2GamepadIndex))
                    P1JumpPressed = true;

                if (InputCompat.IsP2InteractPressedNow(P2UseGamepad, P2GamepadIndex, P2InteractKeyProp, P2InteractKeyFallback))
                    TriggerP2Interact();

                if (_p2Spider != null && _camRightOrBottom != null
                    && InputCompat.IsP2CameraZoomPressedNow(P2UseGamepad, P2GamepadIndex))
                {
                    CycleP2CameraZoom();
                }

                // Drive P2's independent web system
                RepairP1WebState();
                DriveP1ShootFallback();

                if (_p2WebManager != null)
                    _p2WebManager.DriveInput();
            }

            AWJSplitScreenUpdateFix.UpdateFixMod.Update();
        }

        // F7 diagnostic: dump every task list's tasks with their live PixelCrushers
        // QuestState. The game's web-based quest triggers (WebJointTrigger /
        // WebThreadTrigger) early-out unless their quest is Active, so a task sitting
        // at Unassigned silently ignores every web the player builds. Read-only.
        private void DumpQuestStates()
        {
            try
            {
                var qcType = AccessTools.TypeByName("_Scripts.Singletons.QuestController");
                if (qcType == null)
                {
                    LoggerInstance.Warning("[QuestDump] QuestController type not found.");
                    return;
                }

                var instProp = qcType.GetProperty("Instance",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                var qc = instProp != null ? instProp.GetValue(null, null) : null;
                if (qc == null)
                {
                    var found = UnityEngine.Object.FindObjectsOfType(qcType, true);
                    if (found != null && found.Length > 0) qc = found[0];
                }
                if (qc == null)
                {
                    LoggerInstance.Warning("[QuestDump] No QuestController instance in this scene.");
                    return;
                }

                // QuestLog.GetQuestState(string) -> QuestState (PixelCrushers Dialogue System)
                var questLogType = AccessTools.TypeByName("PixelCrushers.DialogueSystem.QuestLog");
                var getState = questLogType != null
                    ? questLogType.GetMethod("GetQuestState", BindingFlags.Static | BindingFlags.Public,
                        null, new Type[] { typeof(string) }, null)
                    : null;
                if (getState == null)
                    LoggerInstance.Warning("[QuestDump] QuestLog.GetQuestState not found — states will show as '?'.");

                const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var listNames = new[] { "taskListKitchen", "taskListOffice", "taskListKidsRoom", "taskListLivingRoom" };

                LoggerInstance.Msg("===== [QuestDump] task states =====");
                foreach (var listName in listNames)
                {
                    object taskList = null;
                    try
                    {
                        var lf = qcType.GetField(listName, F);
                        if (lf != null) taskList = lf.GetValue(qc);
                    }
                    catch { }

                    if (taskList == null)
                    {
                        LoggerInstance.Msg("  " + listName + ": (not set)");
                        continue;
                    }

                    Array tasks = null;
                    try
                    {
                        var tf = taskList.GetType().GetField("tasks", F);
                        if (tf != null) tasks = tf.GetValue(taskList) as Array;
                    }
                    catch { }

                    if (tasks == null || tasks.Length == 0)
                    {
                        LoggerInstance.Msg("  " + listName + ": (no tasks)");
                        continue;
                    }

                    LoggerInstance.Msg("  " + listName + " (" + tasks.Length + " tasks):");
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        var task = tasks.GetValue(i);
                        if (task == null) { LoggerInstance.Msg("    [" + i + "] (null)"); continue; }

                        string questName = "?", text = "?", state = "?";
                        try
                        {
                            var tt = task.GetType();
                            var qnF = tt.GetField("questName", F);
                            if (qnF != null) questName = (qnF.GetValue(task) as string) ?? "";
                            var txtF = tt.GetField("text", F);
                            if (txtF != null) text = (txtF.GetValue(task) as string) ?? "";
                        }
                        catch { }

                        try
                        {
                            if (getState != null && !string.IsNullOrEmpty(questName) && questName != "?")
                            {
                                var s = getState.Invoke(null, new object[] { questName });
                                state = s != null ? s.ToString() : "null";
                            }
                        }
                        catch (Exception ex) { state = "err:" + ex.Message; }

                        LoggerInstance.Msg("    [" + i + "] state=" + state
                            + " | quest=\"" + questName + "\" | text=\"" + text + "\"");
                    }
                }
                LoggerInstance.Msg("===== [QuestDump] end =====");
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("[QuestDump] failed: " + ex);
            }
        }

        // --- Debug_SpeedLog: clean horizontal ground-speed measurement ---
        // rb.linearVelocity is a bad proxy for walk speed (it includes moving-platform
        // velocity and vertical bob, and is sampled at render rate). Instead we integrate
        // each spider's horizontal displacement at physics rate and report the true
        // per-second ground speed for P1 vs P2. Test on flat, static ground.
        private Vector3 _p1PrevPos, _p2PrevPos;
        private bool _speedSampleInit;
        private float _p1AccumDist, _p2AccumDist, _speedAccumTime;

        public override void OnFixedUpdate()
        {
            if (!DebugSpeedLog || _enabled == null || !_enabled.Value || _p2Spider == null)
            {
                _speedSampleInit = false;
                return;
            }

            var p1 = P1BodyMovementInstance;
            var p2 = P2BodyMovementInstance != null ? P2BodyMovementInstance : _p2BodyMovement;
            if (p1 == null || p2 == null)
            {
                _speedSampleInit = false;
                return;
            }

            Vector3 p1Pos = p1.transform.position;
            Vector3 p2Pos = p2.transform.position;

            if (_speedSampleInit)
            {
                _p1AccumDist += Vector3.ProjectOnPlane(p1Pos - _p1PrevPos, Vector3.up).magnitude;
                _p2AccumDist += Vector3.ProjectOnPlane(p2Pos - _p2PrevPos, Vector3.up).magnitude;
                _speedAccumTime += Time.fixedDeltaTime;
            }
            _p1PrevPos = p1Pos;
            _p2PrevPos = p2Pos;
            _speedSampleInit = true;

            if (_speedAccumTime >= 1f)
            {
                float p1Sp = _p1AccumDist / _speedAccumTime;
                float p2Sp = _p2AccumDist / _speedAccumTime;
                float ratio = p1Sp > 0.01f ? p2Sp / p1Sp : 0f;
                LoggerInstance.Msg("[GroundSpeed/1s] P1=" + p1Sp.ToString("F2")
                    + " P2=" + p2Sp.ToString("F2")
                    + " ratio(P2/P1)=" + ratio.ToString("F2")
                    + " | P1 " + DescribeBody(p1)
                    + " | P2 " + DescribeBody(p2));
                _p1AccumDist = 0f;
                _p2AccumDist = 0f;
                _speedAccumTime = 0f;
            }
        }

        // Describes one BodyMovement's speed-relevant state for the diagnostic log
        // (context alongside the displacement-based [GroundSpeed/1s] numbers). The `v`
        // field is rb.linearVelocity.magnitude, which includes moving-platform velocity
        // and vertical motion — trust [GroundSpeed/1s] for actual walk speed.
        private string DescribeBody(Component bm)
        {
            if (bm == null) return "v=? velUp=? mv=? tgt=? sprint=? boost=? uw=? pot=? ms=? scale=?";
            string v = "?", velUp = "?", mv = "?", tgt = "?", sprint = "?", boost = "?", uw = "?", pot = "?", ms = "?", scale = "?";
            try
            {
                if (_bmRbProp != null)
                {
                    var rb = _bmRbProp.GetValue(bm, null) as Rigidbody;
                    if (rb != null)
                    {
                        var vel = rb.linearVelocity;
                        v = vel.magnitude.ToString("F2");
                        // Component of velocity along the body's up axis. If P2's velocity
                        // points into the ground (large negative velUp) while P1's is ~0,
                        // that's the wasted downward angle stealing horizontal speed.
                        velUp = Vector3.Dot(vel, bm.transform.up).ToString("F2");
                    }
                }
            }
            catch { }
            try
            {
                // Ground target position in body-local space. tgtY = ride offset (how far
                // the target sits below the body); tgtZ = fore/aft (negative = target is
                // BEHIND the body). Either being off vs P1 explains the down/back velocity.
                if (BodyMove_TargetTransformField != null)
                {
                    var tt = BodyMove_TargetTransformField.GetValue(bm) as Transform;
                    if (tt != null)
                    {
                        var local = bm.transform.InverseTransformPoint(tt.position);
                        tgt = "(y" + local.y.ToString("F2") + ",z" + local.z.ToString("F2") + ")";
                    }
                }
            }
            catch { }
            try
            {
                if (BodyMove_MoveVectorField != null)
                {
                    var m = (Vector2)BodyMove_MoveVectorField.GetValue(bm);
                    mv = "(" + m.x.ToString("F2") + "," + m.y.ToString("F2") + ")";
                }
            }
            catch { }
            try
            {
                if (BodyMove_IsSprintingField != null)
                    sprint = ((bool)BodyMove_IsSprintingField.GetValue(bm)) ? "T" : "F";
            }
            catch { }
            try
            {
                if (BodyMove_BoostFactorField != null)
                    boost = ((float)BodyMove_BoostFactorField.GetValue(bm)).ToString("F2");
            }
            catch { }
            try
            {
                // uw shows the effective underwater factor: 1.00 when dry, the game's
                // movementUnderwaterFactor (default 0.5) when flagged underwater.
                if (BodyMove_IsUnderwaterField != null)
                {
                    bool isUw = (bool)BodyMove_IsUnderwaterField.GetValue(bm);
                    float uwFactor = 1f;
                    if (isUw && BodyMove_UnderwaterFactorField != null)
                        uwFactor = (float)BodyMove_UnderwaterFactorField.GetValue(bm);
                    uw = uwFactor.ToString("F2");
                }
            }
            catch { }
            try
            {
                if (BodyMove_PotionMultField != null)
                    pot = ((float)BodyMove_PotionMultField.GetValue(bm)).ToString("F2");
            }
            catch { }
            try
            {
                if (BodyMove_MovementSpeedField != null)
                    ms = ((float)BodyMove_MovementSpeedField.GetValue(bm)).ToString("F2");
            }
            catch { }
            try
            {
                var s = bm.transform.lossyScale;
                scale = s.x.ToString("F2") + "/" + s.y.ToString("F2");
            }
            catch { }
            return "v=" + v + " velUp=" + velUp + " mv=" + mv + " tgt=" + tgt
                + " sprint=" + sprint + " boost=" + boost
                + " uw=" + uw + " pot=" + pot + " ms=" + ms + " scale=" + scale;
        }

        public override void OnLateUpdate()
        {
            if (_enabled == null || !_enabled.Value)
                return;

            UpdateP2CameraLook();
            AWJSplitScreenUpdateFix.UpdateFixMod.LateUpdate();
            EnforceP1CameraShoulderOffset();
        }

        private int _p1ShoulderCorrectionLogs;
        private Component _p1ShoulderController;
        private Component _p1ShoulderFollowTarget;
        private object _p1ShoulderFollowBody;
        private FieldInfo _p1ShoulderControllerField;
        private FieldInfo _p1ShoulderTargetField;
        private FieldInfo _p1ShoulderSpiderOffsetField;
        private FieldInfo _p1ShoulderFollowField;

        private void ResetP1ShoulderIsolationCache()
        {
            _p1ShoulderController = null;
            _p1ShoulderFollowTarget = null;
            _p1ShoulderFollowBody = null;
            _p1ShoulderControllerField = null;
            _p1ShoulderTargetField = null;
            _p1ShoulderSpiderOffsetField = null;
            _p1ShoulderFollowField = null;
        }

        private bool CacheP1ShoulderIsolation()
        {
            if (_p1ShoulderController != null && _p1ShoulderFollowTarget != null &&
                _p1ShoulderFollowBody != null && _p1ShoulderControllerField != null &&
                _p1ShoulderTargetField != null && _p1ShoulderSpiderOffsetField != null &&
                _p1ShoulderFollowField != null)
                return true;

            ResetP1ShoulderIsolationCache();
            const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            Type controllerType = AccessTools.TypeByName("_Scripts.Singletons.CameraController");
            if (controllerType == null) return false;

            UnityEngine.Object[] controllers = UnityEngine.Object.FindObjectsOfType(controllerType, true);
            _p1ShoulderController = controllers != null && controllers.Length > 0 ? controllers[0] as Component : null;
            if (_p1ShoulderController == null) return false;

            FieldInfo followTargetField = controllerType.GetField("followCameraFollowTarget", F);
            _p1ShoulderFollowTarget = followTargetField == null ? null : followTargetField.GetValue(_p1ShoulderController) as Component;
            _p1ShoulderControllerField = controllerType.GetField("shoulderOffset", F);
            if (_p1ShoulderFollowTarget == null || _p1ShoulderControllerField == null) return false;

            _p1ShoulderTargetField = AccessTools.Field(_p1ShoulderFollowTarget.GetType(), "target");
            _p1ShoulderSpiderOffsetField = AccessTools.Field(_p1ShoulderFollowTarget.GetType(), "spiderOffset");
            if (_p1ShoulderTargetField == null || _p1ShoulderSpiderOffsetField == null) return false;

            if (!TryGetP1Follow3rdPerson(out _p1ShoulderFollowBody, out Type followType)) return false;
            _p1ShoulderFollowField = followType.GetField("ShoulderOffset", F)
                ?? AccessTools.Field(followType, "m_ShoulderOffset");
            return _p1ShoulderFollowField != null;
        }

        private void EnforceP1CameraShoulderOffset()
        {
            try
            {
                if (!CacheP1ShoulderIsolation()) return;

                Transform target = _p1ShoulderTargetField.GetValue(_p1ShoulderFollowTarget) as Transform;
                object spiderOffsetValue = _p1ShoulderSpiderOffsetField.GetValue(_p1ShoulderFollowTarget);
                if (target == null || !(spiderOffsetValue is Vector3)) return;

                Vector3 spiderOffset = (Vector3)spiderOffsetValue;
                Vector3 authoritativeOffset = _p1ShoulderFollowTarget.transform.InverseTransformVector(target.up * spiderOffset.y);

                Vector3 priorControllerOffset = _p1ShoulderControllerField.GetValue(_p1ShoulderController) is Vector3
                    ? (Vector3)_p1ShoulderControllerField.GetValue(_p1ShoulderController)
                    : authoritativeOffset;
                _p1ShoulderControllerField.SetValue(_p1ShoulderController, authoritativeOffset);
                _p1ShoulderFollowField.SetValue(_p1ShoulderFollowBody, authoritativeOffset);

                if (DebugSpeedLog && (priorControllerOffset - authoritativeOffset).sqrMagnitude > 0.000001f &&
                    _p1ShoulderCorrectionLogs++ < 8)
                {
                    LoggerInstance.Msg("[CameraIsolation] Enforced P1 shoulder offset " +
                        priorControllerOffset.ToString("F3") + " -> " + authoritativeOffset.ToString("F3"));
                }
            }
            catch (Exception ex)
            {
                if (DebugSpeedLog && _p1ShoulderCorrectionLogs++ < 8)
                    LoggerInstance.Warning("[CameraIsolation] Direct shoulder enforcement failed: " + ex.Message);
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

            NeutralizeP2CameraPhysics(_camLeftOrTop, _camRightOrBottom);
            DisableComponentByTypeName(_camRightOrBottom.gameObject, "Cinemachine.CinemachineBrain");
            DisableCameraDriverBehaviours(_camRightOrBottom.gameObject);

            P2Camera = _camRightOrBottom;

            ApplyCameraRects();
        }

        private void NeutralizeP2CameraPhysics(Camera p1Camera, Camera p2Camera)
        {
            if (p2Camera == null) return;

            // The output camera prefab carries a SphereCollider + Rigidbody for
            // CameraWaterTrigger. Cloning the whole camera also clones that physical body.
            // Moving the P2 camera transform then makes it push P1's original camera body,
            // translating P1 without changing CameraMouseLook or Cinemachine state.
            // Remove the cloned physics participation entirely. P1's original water sensor
            // remains intact; P2 underwater sensing can be reintroduced as a query-based
            // probe without putting a collider into Cinemachine's obstruction world.
            Collider[] p2Colliders = p2Camera.GetComponentsInChildren<Collider>(true);
            for (int i = 0; p2Colliders != null && i < p2Colliders.Length; i++)
            {
                Collider collider = p2Colliders[i];
                if (collider != null)
                {
                    // Cinemachine3rdPersonFollow uses an explicit collision mask and can
                    // include both triggers and Unity's Ignore Raycast layer. Disable the
                    // cloned sensor completely so it cannot enter P1's obstruction query.
                    collider.enabled = false;
                }
            }

            Rigidbody[] p2Bodies = p2Camera.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; p2Bodies != null && i < p2Bodies.Length; i++)
            {
                Rigidbody body = p2Bodies[i];
                if (body == null) continue;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.useGravity = false;
                body.isKinematic = true;
                body.detectCollisions = false;
                body.constraints = RigidbodyConstraints.FreezeAll;
            }

            // Explicitly ignore the P1/P2 camera pair as an extra guard if another
            // component changes either collider's trigger state later.
            Collider[] p1Colliders = p1Camera == null ? null : p1Camera.GetComponentsInChildren<Collider>(true);
            for (int p1 = 0; p1Colliders != null && p1 < p1Colliders.Length; p1++)
            {
                if (p1Colliders[p1] == null) continue;
                for (int p2 = 0; p2Colliders != null && p2 < p2Colliders.Length; p2++)
                {
                    if (p2Colliders[p2] != null)
                        Physics.IgnoreCollision(p1Colliders[p1], p2Colliders[p2], true);
                }
            }

            LoggerInstance.Msg("Neutralized P2 camera physics: triggers=" +
                (p2Colliders == null ? 0 : p2Colliders.Length) + " rigidbodies=" +
                (p2Bodies == null ? 0 : p2Bodies.Length) + " collidersDisabled=true.");
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

            // Re-anchor any spider-owned transforms that the game parents to external
            // surfaces while grounded (BodyMovement.targetTransform, LegController.targetLocal).
            // Unity Instantiate only deep-copies references that live INSIDE the cloned
            // hierarchy. If targetTransform was reparented to the floor (line 1078 of
            // BodyMovement.PerformWalking), the clone's field would still point to P1's
            // targetTransform — both spiders would then share one move-target Transform,
            // each pulling the other toward themselves whenever they moved.
            // P1's spider is mid-air → targetTransform.parent == P1.transform → safe to clone.
            // P1's spider is grounded → targetTransform.parent == surface → must re-anchor.
            _p1CloneReanchors.Clear();
            ReanchorSharedTargets(_p1Spider, _p1CloneReanchors);
            try
            {
                _p2Spider = UnityEngine.Object.Instantiate(_p1Spider);
            }
            finally
            {
                // Reanchoring is only needed for the instant of cloning.  Leaving P1's
                // walking target under the spider changes the game's ground-following
                // physics and persists even after split-screen is disabled.
                RestoreReanchoredTargets(_p1CloneReanchors);
                _p1CloneReanchors.Clear();
            }

            if (_p2Spider == null)
            {
                LoggerInstance.Warning("Couldn't clone PlayerSpider for P2.");
                return;
            }
            _p2Spider.name = _p1Spider.name + "_P2";
            _p2Spider.transform.position += new Vector3(3f, 0f, 3f);

            _p1InputTransform = FindChildTransform(_p1Spider.transform, "InputTransform");

            _p2Spider.AddComponent<P2Marker>();

            // IMPORTANT: isolate P2 colliders BEFORE any P2LegDriver performs its first
            // ground cast. Previously this happened much later in SetupSecondSpider(), after
            // every P2 leg had already called Init(). On a level-entry spawn the cloned leg
            // pose can still be folded/settling, so a leg cast could hit P2's own body/leg
            // collider and parent its private foot anchor to that moving transform. Once the
            // rig moved, the anchor moved to the opposite side even though the IK target and
            // leg binding were correct. F9 is far less likely to reproduce because the pose
            // is already stable.
            try
            {
                var earlyP2Colliders = _p2Spider.GetComponentsInChildren<Collider>(true);
                int earlyLayered = 0;
                for (int i = 0; i < earlyP2Colliders.Length; i++)
                {
                    var c = earlyP2Colliders[i];
                    if (c == null) continue;
                    c.gameObject.layer = 2; // Ignore Raycast; excluded by whatIsGround.
                    earlyLayered++;
                }
                LoggerInstance.Msg("Pre-isolated " + earlyLayered +
                    " P2 collider GameObject(s) from leg ground casts before leg initialization.");
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("Failed early P2 collider isolation (non-fatal): " + ex.Message);
            }

            var p2RootRb = _p2Spider.GetComponent<Rigidbody>();
            if (p2RootRb != null && p2RootRb.interpolation == RigidbodyInterpolation.None)
            {
                p2RootRb.interpolation = RigidbodyInterpolation.Interpolate;
                LoggerInstance.Msg("Enabled Rigidbody interpolation on P2 spider root.");
            }

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

                    // Build the P2 leg drivers from P1's CANONICAL live references, not from the
                    // cloned LegController fields.  Instantiate() remaps serialized references, but these
                    // LegControllers also contain runtime Transform references.  During scene startup the
                    // active clone can run lifecycle code before we destroy it, and a transient/mis-remapped
                    // target is enough to make (for example) Left_1 drive the right-side IK target.
                    //
                    // Match each P2 LegController to its P1 counterpart by the exact child-index path in the
                    // cloned hierarchy, then map P1 target/center/jump/opposing references into P2 using the
                    // same path.  This makes leg identity deterministic regardless of clone timing.
                    var p2Legs = _p2Spider.GetComponentsInChildren(legType3, true);
                    var drivers = new P2LegDriver[p2Legs.Length];
                    var p1LegForIndex = new Component[p2Legs.Length];
                    var p1LegIndexById = new Dictionary<int, int>();
                    var p2LegIndexById = new Dictionary<int, int>();
                    int canonicalLegMappings = 0;

                    for (int i = 0; i < p2Legs.Length; i++)
                    {
                        var p2Leg = p2Legs[i] as Component;
                        if (p2Leg == null) continue;
                        p2LegIndexById[p2Leg.GetInstanceID()] = i;

                        Transform p1LegTransform = MapTransformBetweenCloneRoots(
                            _p2Spider.transform, _p1Spider.transform, p2Leg.transform);
                        Component p1Leg = p1LegTransform != null
                            ? p1LegTransform.GetComponent(legType3) as Component
                            : null;
                        p1LegForIndex[i] = p1Leg;
                        if (p1Leg != null)
                        {
                            p1LegIndexById[p1Leg.GetInstanceID()] = i;
                            canonicalLegMappings++;
                        }
                    }

                    var opposingIndices = new List<int>[p2Legs.Length];
                    for (int i = 0; i < p2Legs.Length; i++)
                    {
                        opposingIndices[i] = new List<int>();
                        Component canonicalLeg = p1LegForIndex[i] ?? (p2Legs[i] as Component);
                        bool sourceIsP1 = p1LegForIndex[i] != null;
                        if (canonicalLeg == null || opposingF3 == null) continue;

                        try
                        {
                            var authoredOpposing = opposingF3.GetValue(canonicalLeg) as System.Collections.IEnumerable;
                            if (authoredOpposing == null) continue;

                            foreach (var opposing in authoredOpposing)
                            {
                                var opposingComponent = opposing as Component;
                                if (opposingComponent == null) continue;

                                int partnerIndex;
                                bool found = sourceIsP1
                                    ? p1LegIndexById.TryGetValue(opposingComponent.GetInstanceID(), out partnerIndex)
                                    : p2LegIndexById.TryGetValue(opposingComponent.GetInstanceID(), out partnerIndex);
                                if (found && partnerIndex >= 0 && partnerIndex < p2Legs.Length && partnerIndex != i)
                                    opposingIndices[i].Add(partnerIndex);
                            }
                        }
                        catch { }
                    }

                    for (int i = 0; i < p2Legs.Length; i++)
                    {
                        var p2Leg = p2Legs[i] as Component;
                        if (p2Leg == null) continue;

                        var go = p2Leg.gameObject;
                        Component canonicalLeg = p1LegForIndex[i] ?? p2Leg;
                        bool sourceIsP1 = p1LegForIndex[i] != null;

                        Transform sourceTarget = targetF3?.GetValue(canonicalLeg) as Transform;
                        Transform sourceCenter = centerF3?.GetValue(canonicalLeg) as Transform;
                        Transform sourceTargetJump = targetJumpF3?.GetValue(canonicalLeg) as Transform;

                        Transform target3 = sourceIsP1
                            ? MapTransformBetweenCloneRoots(_p1Spider.transform, _p2Spider.transform, sourceTarget)
                            : sourceTarget;
                        Transform center3 = sourceIsP1
                            ? MapTransformBetweenCloneRoots(_p1Spider.transform, _p2Spider.transform, sourceCenter)
                            : sourceCenter;
                        Transform targetJump3 = sourceIsP1
                            ? MapTransformBetweenCloneRoots(_p1Spider.transform, _p2Spider.transform, sourceTargetJump)
                            : sourceTargetJump;

                        // If a particular runtime reference lives outside PlayerSpider, path mapping cannot
                        // clone it.  Fall back only that reference to the P2 clone's own field rather than
                        // discarding the whole canonical mapping.
                        if (target3 == null)
                            target3 = targetF3?.GetValue(p2Leg) as Transform;
                        if (center3 == null)
                            center3 = centerF3?.GetValue(p2Leg) as Transform;
                        if (targetJump3 == null)
                            targetJump3 = targetJumpF3?.GetValue(p2Leg) as Transform;

                        Vector3 offset3 = Vector3.zero;
                        if (offsetF3 != null)
                        {
                            try { offset3 = (Vector3)offsetF3.GetValue(canonicalLeg); }
                            catch
                            {
                                try { offset3 = (Vector3)offsetF3.GetValue(p2Leg); } catch { }
                            }
                        }

                        UnityEngine.Object.DestroyImmediate(p2Leg);

                        if (target3 != null && p2BodyTransform != null)
                        {
                            var driver = go.AddComponent<P2LegDriver>();
                            driver.Init(target3, offset3, center3, p2BodyTransform,
                                _p1Spider.transform, _p2Spider.transform,
                                p2BodyMovement, moveVecField,
                                whatIsGround, sphereRadius,
                                rayUpOffset, rayLength, stepDist,
                                stepTime, stepHeight, tipHeight, newTargetDist,
                                targetJump3);
                            drivers[i] = driver;
                        }
                        else
                        {
                            LoggerInstance.Warning("Could not map canonical P2 leg target for " + go.name +
                                " (P1 source=" + (sourceIsP1 ? "yes" : "no") + ").");
                        }
                    }

                    // Preserve P1's authored opposing-leg relationships after the deterministic mapping.
                    for (int i = 0; i < drivers.Length; i++)
                    {
                        if (drivers[i] == null) continue;

                        var mappedOpposing = new List<P2LegDriver>();
                        if (opposingIndices[i] != null)
                        {
                            for (int oi = 0; oi < opposingIndices[i].Count; oi++)
                            {
                                int partnerIndex = opposingIndices[i][oi];
                                if (partnerIndex >= 0 && partnerIndex < drivers.Length &&
                                    drivers[partnerIndex] != null && drivers[partnerIndex] != drivers[i])
                                {
                                    mappedOpposing.Add(drivers[partnerIndex]);
                                }
                            }
                        }

                        if (mappedOpposing.Count == 0)
                        {
                            int fallbackPartner = (i % 2 == 0) ? i + 1 : i - 1;
                            if (fallbackPartner >= 0 && fallbackPartner < drivers.Length && drivers[fallbackPartner] != null)
                                mappedOpposing.Add(drivers[fallbackPartner]);
                        }

                        drivers[i].SetOpposingLegs(mappedOpposing.ToArray());
                    }

                    LoggerInstance.Msg("Replaced " + p2Legs.Length +
                        " LegController(s) with P2LegDriver; canonical P1 hierarchy mappings=" +
                        canonicalLegMappings + "/" + p2Legs.Length + ".");
                }

                // Keep RigBuilder/Rig alive — the IK system drives visual bones toward
                // target positions. P2LegDriver updates targets without raycasts.
                // Before rebuilding the PlayableGraph, rewrite every Transform reference in
                // Animation Rigging constraint data from P1's canonical hierarchy into the
                // corresponding P2 transform.  This removes a second possible source of a
                // left-target/right-bone swap: a cloned constraint carrying a transient runtime
                // reference even though the P2LegDriver itself owns the correct target.
                int canonicalConstraintRefs = CanonicalizeP2AnimationRigConstraintReferences(_p1Spider, _p2Spider);
                int canonicalRigLayers = CanonicalizeP2RigBuilderLayerReferences(_p1Spider, _p2Spider);
                LoggerInstance.Msg("Canonicalized P2 Animation Rigging references from P1: constraintTransforms=" +
                    canonicalConstraintRefs + " rigLayers=" + canonicalRigLayers + ".");

                // Rebuild RigBuilder so IK binds to the canonical P2 bones/targets.
                Type rigBuilderType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    rigBuilderType = asm.GetType("UnityEngine.Animations.Rigging.RigBuilder");
                    if (rigBuilderType != null) break;
                }
                if (rigBuilderType != null)
                {
                    var rigBuilders = _p2Spider.GetComponentsInChildren(rigBuilderType, true);
                    MethodInfo clearMethod = null;
                    MethodInfo buildMethod = null;
                    foreach (var m in rigBuilderType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (m.GetParameters().Length != 0) continue;
                        if (m.Name == "Clear") clearMethod = m;
                        else if (m.Name == "Build") buildMethod = m;
                    }

                    int cleared = 0;
                    int built = 0;
                    for (int ri = 0; ri < rigBuilders.Length; ri++)
                    {
                        var rigBuilder = rigBuilders[ri];
                        if (rigBuilder == null) continue;
                        try
                        {
                            // A cloned active RigBuilder can already own a PlayableGraph created during
                            // Instantiate/OnEnable. Calling Build() again without Clear() can leave the
                            // clone using stale bindings. Destroy that graph first, then build once from
                            // P2's final cloned constraint/bone references.
                            if (clearMethod != null)
                            {
                                clearMethod.Invoke(rigBuilder, null);
                                cleared++;
                            }
                            if (buildMethod != null)
                            {
                                buildMethod.Invoke(rigBuilder, null);
                                built++;
                            }
                        }
                        catch (Exception rigEx)
                        {
                            LoggerInstance.Warning("P2 RigBuilder clear/build failed (non-fatal): " + rigEx.Message);
                        }
                    }
                    LoggerInstance.Msg("Reset P2 Animation Rigging graph(s): cleared=" + cleared + ", built=" + built + ".");
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

            // Speed-parity fix. Moving P2 to layer 2 (above) also turned ON physical
            // collision between P2's body and the walkable ground (the original spider
            // layer is excluded from ground collision in the project's collision matrix;
            // layer 2 is not). That collision held P2's body ~0.1 above where its scripted
            // velocity wants it and made it bounce vertically, wasting ~27% of its forward
            // speed (measured: velUp oscillated ±3.7, ratio 0.73). P1's body never
            // physically collides with what it walks on — it's positioned purely by script.
            // Match that by excluding the walkable-ground layers from P2's colliders so the
            // body settles flush (velUp≈0). Layer 2 still hides P2 from P1's leg raycasts.
            try
            {
                LayerMask groundMask = GetP2WhatIsGround();
                if (groundMask.value != 0)
                {
                    var p2Colliders = _p2Spider.GetComponentsInChildren<Collider>(true);
                    int excluded = 0;
                    for (int i = 0; i < p2Colliders.Length; i++)
                    {
                        var c = p2Colliders[i];
                        if (c == null) continue;
                        c.excludeLayers = c.excludeLayers | groundMask;
                        excluded++;
                    }
                    LoggerInstance.Msg("Excluded ground layers (" + groundMask.value + ") from " + excluded
                        + " P2 collider(s) so the body sits flush like P1 (speed-parity fix).");
                }
                else
                {
                    LoggerInstance.Warning("Could not resolve whatIsGround for the P2 ground-collision fix; P2 may walk slower than P1.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("P2 ground-collision exclusion failed (non-fatal): " + ex);
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

            CacheP2InteractionRefs();

            LoggerInstance.Msg("Spawned P2: " + _p2Spider.name +
                               " | P2InputTransform=" + (P2InputTransform != null ? P2InputTransform.name : "null") +
                               " | P2WebManager=" + (_p2WebManager != null) +
                               " | P2SpiderInteraction=" + (_p2SpiderInteraction != null));

            // Automatic scene spawning happens while unrelated level Start/OnEnable methods can
            // still run.  F9 spawning later does not have that race.  Re-assert the canonical
            // leg/constraint bindings after the wind fix's one-frame P2 lifecycle pulse and again
            // after scene startup has had time to settle.  The repair is idempotent and only rebuilds
            // the rig when the same P1/P2 instances are still alive.
            if (_currentSetupFromSceneLoad)
                MelonCoroutines.Start(DelayedP2LegBindingRepair(_p1Spider, _p2Spider));
        }

        private void CacheP2InteractionRefs()
        {
            _p2SpiderInteraction = null;
            _p2SpiderMobileInteractMethod = null;

            if (_p2Spider == null)
                return;

            try
            {
                var spiderInteractionType = AccessTools.TypeByName("_Scripts.Spider.SpiderInteraction");
                if (spiderInteractionType == null)
                    return;

                _p2SpiderInteraction = _p2Spider.GetComponentInChildren(spiderInteractionType, true) as Component;
                if (_p2SpiderInteraction == null)
                    return;

                _p2SpiderMobileInteractMethod = AccessTools.Method(_p2SpiderInteraction.GetType(), "MobileInteract");
            }
            catch { }
        }

        private void TriggerP2Interact()
        {
            if (_p2Spider == null)
                return;

            if (_p2SpiderInteraction == null || _p2SpiderMobileInteractMethod == null)
                CacheP2InteractionRefs();

            if (_p2SpiderInteraction == null || _p2SpiderMobileInteractMethod == null)
                return;

            try
            {
                _p2SpiderMobileInteractMethod.Invoke(_p2SpiderInteraction, null);
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("[P2Interact] MobileInteract invoke failed (non-fatal): " + ex.Message);
            }
        }

        private void TriggerP2SprintToggle()
        {
            if (_p2Spider == null || BodyMove_SprintInputField == null)
                return;

            if (_p2BodyMovement == null)
            {
                try
                {
                    var bodyMoveType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                    if (bodyMoveType != null)
                    {
                        _p2BodyMovement = _p2Spider.GetComponentInChildren(bodyMoveType, true) as Component;
                        P2BodyMovementInstance = _p2BodyMovement;
                    }
                }
                catch { }
            }

            if (_p2BodyMovement == null)
                return;

            P2SprintDesired = !P2SprintDesired;
            LoggerInstance.Msg("[P2Sprint] Sprint " + (P2SprintDesired ? "ON" : "OFF"));

            // If the branch-aware writer in FixedUpdate_Prefix can't run (missing
            // reflection targets), fall back to the legacy single pulse. The game's
            // toggle branch consumes it; the Hold+KB/M branch latches it (old bug),
            // but that's no worse than the previous behavior.
            bool managed = BodyMove_IsSprintingField != null && TryGetSprintBranch(out _);
            if (!managed)
            {
                try
                {
                    BodyMove_SprintInputField.SetValue(_p2BodyMovement, true);
                }
                catch (Exception ex)
                {
                    LoggerInstance.Warning("[P2Sprint] Failed to toggle sprint (non-fatal): " + ex.Message);
                }
            }
        }

        // Determines which sprint branch BodyMovement.PerformWalking will take
        // (ilspy BodyMovement.cs:739): Hold+KeyboardMouse copies sprintInput into
        // isSprinting every step; otherwise sprintInput acts as a consume-once toggle.
        // Values are read fresh every call (device/mode can change mid-session);
        // only the reflection handles are cached.
        private static PropertyInfo _sprintModeProp;
        private static object _sprintModeHoldValue;
        private static PropertyInfo _gcInstanceProp;
        private static PropertyInfo _gcInputIsKbmProp;
        private static bool _sprintBranchReflectionSearched;

        internal static bool TryGetSprintBranch(out bool holdKbm)
        {
            holdKbm = false;
            try
            {
                if (!_sprintBranchReflectionSearched)
                {
                    _sprintBranchReflectionSearched = true;
                    var sct = AccessTools.TypeByName("_Scripts.Singletons.SettingsController");
                    if (sct != null)
                    {
                        _sprintModeProp = sct.GetProperty("SprintMode",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        if (_sprintModeProp != null)
                            try { _sprintModeHoldValue = Enum.Parse(_sprintModeProp.PropertyType, "Hold"); } catch { }
                    }
                    var gct = AccessTools.TypeByName("_Scripts.Singletons.GameController");
                    if (gct != null)
                    {
                        _gcInstanceProp = gct.GetProperty("Instance",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        _gcInputIsKbmProp = gct.GetProperty("InputIsKeyboardMouse",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    }
                }

                if (_sprintModeProp == null || _sprintModeHoldValue == null ||
                    _gcInstanceProp == null || _gcInputIsKbmProp == null)
                    return false;

                var sprintMode = _sprintModeProp.GetValue(null, null);
                if (sprintMode == null) return false;

                var gc = _gcInstanceProp.GetValue(null, null);
                if (gc == null) return false;

                bool inputIsKbm = (bool)_gcInputIsKbmProp.GetValue(gc, null);
                holdKbm = sprintMode.Equals(_sprintModeHoldValue) && inputIsKbm;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // True only when the game is in normal gameplay (GameController.State == GameState.Running).
        // Defaults to true (sync) when reflection is unavailable, preserving prior always-sync
        // behavior. Used to keep P1's transient cutscene/dialogue/photo-mode FOV off P2's camera.
        private static PropertyInfo _gcStateInstanceProp;
        private static PropertyInfo _gcStateProp;
        private static object _gameStateRunningValue;
        private static bool _gcStateReflectionSearched;

        private static bool IsGameplayRunning()
        {
            try
            {
                if (!_gcStateReflectionSearched)
                {
                    _gcStateReflectionSearched = true;
                    var gct = AccessTools.TypeByName("_Scripts.Singletons.GameController");
                    if (gct != null)
                    {
                        _gcStateInstanceProp = gct.GetProperty("Instance",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        _gcStateProp = gct.GetProperty("State",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var gsType = gct.GetNestedType("GameState", BindingFlags.Public);
                        if (gsType != null)
                            try { _gameStateRunningValue = Enum.Parse(gsType, "Running"); } catch { }
                    }
                }

                if (_gcStateInstanceProp == null || _gcStateProp == null || _gameStateRunningValue == null)
                    return true; // reflection unavailable -> preserve always-sync behavior

                var gc = _gcStateInstanceProp.GetValue(null, null);
                if (gc == null) return true;

                var state = _gcStateProp.GetValue(gc, null);
                return state != null && state.Equals(_gameStateRunningValue);
            }
            catch
            {
                return true;
            }
        }

        private void InitP2CameraRig()
        {
            if (_camRightOrBottom == null || (P2InputTransform == null && _p2Spider == null))
                return;
            

            EnsureCameraDynamicsCached();

            // Initialize camera direction: behind and slightly above the spider.
            // Mirror P1: orbit is in WORLD space (FollowTarget LookAt's world up,
            // CameraMouseLook yaws on local Y ≈ world up). Only the pivot translates
            // along the spider's surface normal — the orbit itself stays world-aligned.
            var p2Anchor = GetP2CameraAnchor();
            _p2SmoothUp = p2Anchor.up;
            var behind = -Vector3.ProjectOnPlane(p2Anchor.forward, Vector3.up).normalized;
            if (behind.sqrMagnitude < 0.001f) behind = Vector3.back;
            // Tilt upward slightly (15 degrees worth) — also in world up.
            SeedP2CameraAngles((behind + Vector3.up * 0.27f).normalized);

            // Prime the dynamic distance from P2's own current manual zoom state.
            // The manual preset ladder uses the exact CameraZoom.cs formula, but the
            // chosen preset is independent from P1.
            SyncP2ZoomStateFromPrefs();
            float seed = Mathf.Clamp(_p2ManualZoom, _p1MinZoom, _p1MaxZoom);
            _p2CamSmoothedZoom = seed;
            _p2CamDistance = seed;

            _p2CamRigInited = true;
            ApplyP2CameraTransform();
        }

        private void SeedP2CameraAngles(Vector3 camDir)
        {
            if (camDir.sqrMagnitude < 0.001f) camDir = Vector3.back;
            camDir = camDir.normalized;

            var camForward = -camDir;
            var flatForward = Vector3.ProjectOnPlane(camForward, Vector3.up);
            if (flatForward.sqrMagnitude < 0.001f)
            {
                flatForward = Vector3.forward;
            }

            flatForward.Normalize();
            _p2CamYaw = Mathf.Atan2(flatForward.x, flatForward.z) * Mathf.Rad2Deg;

            var yawOnly = Quaternion.AngleAxis(_p2CamYaw, Vector3.up);
            var localForward = Quaternion.Inverse(yawOnly) * camForward;
            _p2CamLookY = -Vector3.SignedAngle(Vector3.forward, localForward, Vector3.right);
            if (_p1ClampLookY)
            {
                _p2CamLookY = Mathf.Clamp(_p2CamLookY, _p1MinLookY, _p1MaxLookY);
            }

            _p2CamDir = GetP2CameraRotation() * Vector3.back;
        }

        private Quaternion GetP2CameraRotation()
        {
            var yawRot = Quaternion.AngleAxis(_p2CamYaw, Vector3.up);
            var pitchRot = Quaternion.AngleAxis(-_p2CamLookY, Vector3.right);
            return yawRot * pitchRot;
        }

        private Transform GetP2CameraAnchor()
        {
            if (_p2Spider != null) return _p2Spider.transform;
            return P2InputTransform;
        }

        private void ApplyP2CameraTransform()
        {
            var p2Anchor = GetP2CameraAnchor();
            var camRot = GetP2CameraRotation();
            _p2CamDir = (camRot * Vector3.back).normalized;

            // P1's effective orbit center is the spider root lifted by FollowTarget's
            // +target.up and then by Cinemachine3rdPersonFollow's shoulder offset.
            var pivot = p2Anchor.position
                + _p2SmoothUp * (P2CamPivotOffset + _p2CamShoulderHeight)
                + Vector3.up * _p2CamVerticalArm;

            // Mirror Cinemachine3rdPersonFollow built-in collision: shorten _p2CamDistance
            // when an obstacle is in the way, with asymmetric damping (fast in, slow out).
            float effectiveDist = ApplyP2CameraCollision(pivot, _p2CamDir, _p2CamDistance, Time.deltaTime);

            // Camera position along the orbit direction
            var desiredPos = pivot + _p2CamDir * effectiveDist;

            _camRightOrBottom.transform.SetPositionAndRotation(desiredPos, camRot);
        }

        // Mirrors Cinemachine3rdPersonFollow's built-in collision: spherecast from the
        // pivot toward the camera; if an obstacle is in the way, clamp the distance to
        // the hit point. Apply asymmetric exponential damping so the camera snaps in
        // quickly (DampingIntoCollision) but eases back out slowly (DampingFromCollision).
        // Filters out trigger volumes (so CameraWaterTrigger and similar don't push the
        // camera), the IgnoreTag from P1's settings, and the P2 spider's own colliders.
        private float ApplyP2CameraCollision(Vector3 pivot, Vector3 camDir, float desiredDist, float dt)
        {
            float radius = _p2CamRadius;
            float targetDist = desiredDist;

            try
            {
                // Refresh self-collider cache periodically (spider can spawn/despawn parts).
                int frame = Time.frameCount;
                if (_p2SelfColliders == null || frame - _p2SelfColliderRefreshFrame > 240)
                {
                    if (_p2Spider != null)
                        _p2SelfColliders = _p2Spider.GetComponentsInChildren<Collider>(true);
                    _p2SelfColliderRefreshFrame = frame;
                }

                // SphereCastAll from pivot along camDir up to (desiredDist + radius).
                // Origin is pulled slightly back toward the pivot (-radius along dir) so
                // that we still detect obstacles whose surface is right at the pivot.
                Vector3 origin = pivot - camDir * radius;
                float castLen = desiredDist + radius;
                var hits = Physics.SphereCastAll(
                    origin, radius, camDir, castLen,
                    _p2CamCollisionMask, QueryTriggerInteraction.Ignore);

                float bestHitDist = float.PositiveInfinity;
                if (hits != null)
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        var h = hits[i];
                        if (h.collider == null) continue;
                        if (!string.IsNullOrEmpty(_p2CamIgnoreTag))
                        {
                            try { if (h.collider.CompareTag(_p2CamIgnoreTag)) continue; } catch { }
                        }
                        if (IsP2SelfCollider(h.collider)) continue;
                        // h.distance is from the (pulled-back) origin; convert to
                        // distance from pivot along camDir.
                        float distFromPivot = h.distance - radius;
                        if (distFromPivot < bestHitDist) bestHitDist = distFromPivot;
                    }
                }

                if (bestHitDist < desiredDist)
                {
                    // Keep a tiny minimum so the camera never lands inside the pivot.
                    targetDist = Mathf.Max(0.05f, bestHitDist);
                }
            }
            catch { /* ignore — fall through with damping */ }

            // First call: seed without damping.
            if (_p2CamCollidedDistance < 0f) { _p2CamCollidedDistance = targetDist; return targetDist; }

            // Asymmetric damping: shrinking → DampingIntoCollision; growing → DampingFromCollision.
            float damping = (targetDist < _p2CamCollidedDistance) ? _p2CamDampingIn : _p2CamDampingOut;
            if (damping <= 0.0001f || dt <= 0f)
                _p2CamCollidedDistance = targetDist;
            else
                _p2CamCollidedDistance = targetDist + (_p2CamCollidedDistance - targetDist) * Mathf.Exp(-dt / damping);

            return _p2CamCollidedDistance;
        }

        private bool IsP2SelfCollider(Collider c)
        {
            if (c == null || _p2SelfColliders == null) return false;
            for (int i = 0; i < _p2SelfColliders.Length; i++)
                if (_p2SelfColliders[i] == c) return true;
            // Fallback: walk the transform parent chain looking for the spider root.
            if (_p2Spider != null)
            {
                var t = c.transform;
                var rootT = _p2Spider.transform;
                while (t != null)
                {
                    if (t == rootT) return true;
                    t = t.parent;
                }
            }
            return false;
        }

        private void UpdateP2CameraLook()
        {
            if (_camRightOrBottom == null) return;
            if (P2InputTransform == null && _p2Spider == null) return;

            if (!_p2CamRigInited)
                InitP2CameraRig();

            EnsureCameraDynamicsCached();

            // Keep P2's FOV in lockstep with P1's settings-driven gameplay FOV. The game
            // recomputes P1's FOV live from settings/aspect (CameraController.UpdateFieldOfView);
            // P2's clone only inherited the value once at Instantiate. Only sync while the game
            // is in normal gameplay (GameState.Running) — otherwise P1's output camera carries a
            // transient cutscene/dialogue/photo-mode vcam FOV that we don't want on P2's half.
            if (_camLeftOrTop != null && !_camLeftOrTop.orthographic
                && IsGameplayRunning()
                && !Mathf.Approximately(_camRightOrBottom.fieldOfView, _camLeftOrTop.fieldOfView))
            {
                _camRightOrBottom.fieldOfView = _camLeftOrTop.fieldOfView;
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
            // Pitch sign matches P1's CameraMouseLook (line 97: AngleAxis(-mouseLook.y, right)
            // applied to a camera under a yaw-transform). Since our _p2CamDir is the
            // pivot→camera vector and `right = up × camDir`, a positive pitchInput
            // (stick up) needs to rotate camDir by a positive angle around `right` for
            // the camera-forward (which is -camDir) to tilt UP — i.e. NOT negated.
            float pitchDelta = pitchInput * speed * dt;

            // --- Incremental orbit ---
            // Mirror P1's CameraMouseLook + FollowTarget: yaw is around WORLD up
            // (FollowTarget does LookAt(player, Vector3.up); CameraMouseLook then
            // yaws localRot around Vector3.up of that follow-target, which equals
            // world up). Pitch is around the world-horizontal right axis. The orbit
            // stays world-aligned; on walls/ceilings the pivot translates with the
            // spider but the camera doesn't roll. _p2SmoothUp remains the smoothed
            // surface normal — used only for the pivot offset and the look-up-zoom
            // dot product (which uses spider.up in P1's CameraZoom).
            var p2Anchor = GetP2CameraAnchor();
            _p2SmoothUp = Vector3.Slerp(_p2SmoothUp, p2Anchor.up, dt * 3f).normalized;
            var surfUp = _p2SmoothUp;
            _p2CamYaw += yawDelta;
            _p2CamLookY += pitchDelta;
            if (_p1ClampLookY)
            {
                _p2CamLookY = Mathf.Clamp(_p2CamLookY, _p1MinLookY, _p1MaxLookY);
            }
            var camRot = GetP2CameraRotation();
            _p2CamDir = (camRot * Vector3.back).normalized;

            // --- True dynamic camera offset (mirrors P1 CameraZoom.HandleCameraZoom) ---
            _p2CamDistance = ComputeP2DynamicCameraDistance(camRot * Vector3.forward, surfUp, dt);

            ApplyP2CameraTransform();
        }

        // Lazily caches the P1 _Scripts.Camera.CameraZoom instance + private field info,
        // and the P2 _Scripts.Spider.BodyMovement instance + property accessors. Safe to
        // call every frame; once everything is found we just no-op.
        private void EnsureCameraDynamicsCached()
        {
            if (!_p1CameraZoomCached)
            {
                try
                {
                    var czType = AccessTools.TypeByName("_Scripts.Camera.CameraZoom");
                    if (czType != null)
                    {
                        var arr = UnityEngine.Object.FindObjectsOfType(czType, true);
                        if (arr != null && arr.Length > 0) _p1CameraZoom = arr[0];
                        if (_p1CameraZoom != null)
                        {
                            const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                            var minF = czType.GetField("minZoom", F);
                            var maxF = czType.GetField("maxZoom", F);
                            var stepsF = czType.GetField("zoomSteps", F);
                            var zupF = czType.GetField("zoomInWhenLookingUp", F);
                            if (minF != null) _p1MinZoom = (float)minF.GetValue(_p1CameraZoom);
                            if (maxF != null) _p1MaxZoom = (float)maxF.GetValue(_p1CameraZoom);
                            if (stepsF != null) _p1ZoomSteps = Mathf.Max(2, (int)stepsF.GetValue(_p1CameraZoom));
                            if (zupF != null) _p1ZoomInWhenLookingUp = (bool)zupF.GetValue(_p1CameraZoom);
                            RebuildP2ZoomArray();
                            _p1CameraZoomCached = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerInstance.Warning("[P2CamDyn] CameraZoom reflection failed: " + ex.Message);
                    _p1CameraZoomCached = true; // don't keep retrying
                }
            }

            if (!_p1CameraMouseLookCached)
            {
                try
                {
                    var cmlType = AccessTools.TypeByName("_Scripts.Camera.CameraMouseLook");
                    if (cmlType != null)
                    {
                        var arr = UnityEngine.Object.FindObjectsOfType(cmlType, true);
                        if (arr != null && arr.Length > 0)
                        {
                            const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                            var clampYF = cmlType.GetField("clampY", F);
                            var minYF = cmlType.GetField("minY", F);
                            var maxYF = cmlType.GetField("maxY", F);
                            var cameraMouseLook = arr[0];
                            if (clampYF != null) _p1ClampLookY = (bool)clampYF.GetValue(cameraMouseLook);
                            if (minYF != null) _p1MinLookY = (float)minYF.GetValue(cameraMouseLook);
                            if (maxYF != null) _p1MaxLookY = (float)maxYF.GetValue(cameraMouseLook);
                            if (_p2CamRigInited && _p1ClampLookY)
                            {
                                _p2CamLookY = Mathf.Clamp(_p2CamLookY, _p1MinLookY, _p1MaxLookY);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerInstance.Warning("[P2CamDyn] CameraMouseLook reflection failed: " + ex.Message);
                }

                _p1CameraMouseLookCached = true;
            }

            if (_settingsAutoZoomProp == null)
            {
                try
                {
                    var sct = AccessTools.TypeByName("_Scripts.Singletons.SettingsController");
                    if (sct != null)
                        _settingsAutoZoomProp = sct.GetProperty("AutoZoom", BindingFlags.Static | BindingFlags.Public);
                }
                catch { }
            }

            if (_p2Spider != null && _p2BodyMovement == null)
            {
                try
                {
                    var bmType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                    if (bmType != null)
                    {
                        _p2BodyMovement = _p2Spider.GetComponentInChildren(bmType, true) as Component;
                        P2BodyMovementInstance = _p2BodyMovement;
                        // P1 resolution: ONLY accept a BodyMovement where isPlayer==true.
                        // NPC spiders also have BodyMovement components, so a "first non-P2"
                        // fallback can poison the cache with an NPC. If isPlayer isn't set
                        // yet, leave P1BodyMovementInstance null and retry next frame — the
                        // identity check downstream (P1JumpBypass) tolerates null safely.
                        if (P1BodyMovementInstance == null)
                        {
                            var isPlayerField = bmType.GetField("isPlayer",
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            var p1bms = UnityEngine.Object.FindObjectsOfType(bmType, true);
                            for (int i = 0; i < (p1bms != null ? p1bms.Length : 0); i++)
                            {
                                var c = p1bms[i] as Component;
                                if (c == null || object.ReferenceEquals(c, _p2BodyMovement)) continue;
                                bool isP1 = false;
                                if (isPlayerField != null)
                                {
                                    try { isP1 = (bool)isPlayerField.GetValue(c); } catch { }
                                }
                                if (isP1)
                                {
                                    P1BodyMovementInstance = c;
                                    LoggerInstance.Msg("Resolved P1 BodyMovement: " + c.gameObject.name);
                                    break;
                                }
                            }
                            // No fallback. If isPlayer isn't set yet we'll retry next
                            // frame (this method is invoked every LateUpdate while
                            // _p2BodyMovement is non-null). The retry path below
                            // ensures lazy re-resolution if for some reason the cache
                            // was lost or wasn't set on the first attempt.
                        }
                        const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        _bmRbProp = bmType.GetProperty("Rb", F);
                        _bmStateProp = bmType.GetProperty("State", F);
                        _bmWebTouchedProp = bmType.GetProperty("WebTouched", F);
                        var msType = bmType.GetNestedType("MovementState", BindingFlags.Public);
                        if (msType != null)
                        {
                            try { _bmWalkingState = Enum.Parse(msType, "Walking"); } catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerInstance.Warning("[P2CamDyn] BodyMovement reflection failed: " + ex.Message);
                }
            }

            // Lazy P1 retry: if we cached P2 but never resolved P1 (e.g., isPlayer
            // wasn't true yet during the initial pass), keep looking each frame
            // until we find a BodyMovement with isPlayer==true that isn't P2.
            if (_p2BodyMovement != null && P1BodyMovementInstance == null)
            {
                try
                {
                    var bmType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                    if (bmType != null)
                    {
                        var isPlayerField = bmType.GetField("isPlayer",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        var p1bms = UnityEngine.Object.FindObjectsOfType(bmType, true);
                        for (int i = 0; i < (p1bms != null ? p1bms.Length : 0); i++)
                        {
                            var c = p1bms[i] as Component;
                            if (c == null || object.ReferenceEquals(c, _p2BodyMovement)) continue;
                            bool isP1 = false;
                            if (isPlayerField != null)
                            {
                                try { isP1 = (bool)isPlayerField.GetValue(c); } catch { }
                            }
                            if (isP1)
                            {
                                P1BodyMovementInstance = c;
                                LoggerInstance.Msg("Resolved P1 BodyMovement (lazy retry): " + c.gameObject.name);
                                break;
                            }
                        }
                    }
                }
                catch { }
            }

            if (!_p1FollowCached)
            {
                if (TryGetP1Follow3rdPerson(out var follow, out var followType))
                {
                    try
                    {
                        const BindingFlags F = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        var radF = followType.GetField("CameraRadius", F);
                        var maskF = followType.GetField("CameraCollisionFilter", F);
                        var tagF = followType.GetField("IgnoreTag", F);
                        var dInF = followType.GetField("DampingIntoCollision", F);
                        var dOutF = followType.GetField("DampingFromCollision", F);
                        var shoulderF = followType.GetField("ShoulderOffset", F) ?? followType.GetField("m_ShoulderOffset", F);
                        var shoulderP = followType.GetProperty("ShoulderOffset", F);
                        var verticalArmF = followType.GetField("VerticalArmLength", F) ?? followType.GetField("m_VerticalArmLength", F);
                        var verticalArmP = followType.GetProperty("VerticalArmLength", F);
                        if (radF != null) _p2CamRadius = Mathf.Max(0.01f, (float)radF.GetValue(follow));
                        if (maskF != null)
                        {
                            var v = maskF.GetValue(follow);
                            if (v is LayerMask lm) _p2CamCollisionMask = lm;
                            else if (v is int i) _p2CamCollisionMask = i;
                        }
                        if (tagF != null) _p2CamIgnoreTag = (tagF.GetValue(follow) as string) ?? "";
                        if (dInF != null) _p2CamDampingIn = Mathf.Max(0f, (float)dInF.GetValue(follow));
                        if (dOutF != null) _p2CamDampingOut = Mathf.Max(0f, (float)dOutF.GetValue(follow));
                        object shoulderObj = shoulderP != null ? shoulderP.GetValue(follow, null) : (shoulderF != null ? shoulderF.GetValue(follow) : null);
                        if (shoulderObj is Vector3 shoulderOffset)
                        {
                            _p2CamShoulderHeight = Mathf.Max(0f, shoulderOffset.magnitude);
                        }
                        object verticalArmObj = verticalArmP != null ? verticalArmP.GetValue(follow, null) : (verticalArmF != null ? verticalArmF.GetValue(follow) : null);
                        if (verticalArmObj is float verticalArm)
                        {
                            _p2CamVerticalArm = verticalArm;
                        }
                        _p1FollowCached = true;
                    }
                    catch (Exception ex)
                    {
                        LoggerInstance.Warning("[P2CamDyn] 3rdPersonFollow collision reflection failed: " + ex.Message);
                        _p1FollowCached = true;
                    }
                }
            }
        }

        private void RebuildP2ZoomArray()
        {
            int zoomSteps = Mathf.Max(2, _p1ZoomSteps);
            float minZoom = _p1MinZoom;
            float maxZoom = Mathf.Max(_p1MaxZoom, minZoom + 0.01f);

            if (_p2ZoomArray != null && _p2ZoomArray.Length == zoomSteps)
            {
                bool same = true;
                for (int i = 0; i < zoomSteps; i++)
                {
                    float want = minZoom + (float)i * (maxZoom - minZoom) / (float)(zoomSteps - 1);
                    if (Mathf.Abs(_p2ZoomArray[i] - want) > 0.0001f)
                    {
                        same = false;
                        break;
                    }
                }
                if (same) return;
            }

            _p2ZoomArray = new float[zoomSteps];
            for (int i = 0; i < zoomSteps; i++)
            {
                _p2ZoomArray[i] = minZoom + (float)i * (maxZoom - minZoom) / (float)(zoomSteps - 1);
            }
        }

        private int FindNearestP2ZoomIndex(float zoom)
        {
            RebuildP2ZoomArray();
            if (_p2ZoomArray == null || _p2ZoomArray.Length == 0) return 0;

            int best = 0;
            float bestDelta = Mathf.Abs(_p2ZoomArray[0] - zoom);
            for (int i = 1; i < _p2ZoomArray.Length; i++)
            {
                float delta = Mathf.Abs(_p2ZoomArray[i] - zoom);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    best = i;
                }
            }
            return best;
        }

        private void SyncP2ZoomStateFromPrefs()
        {
            RebuildP2ZoomArray();
            if (_p2ZoomArray == null || _p2ZoomArray.Length == 0)
            {
                _p2ManualZoom = Mathf.Clamp(_p2ManualZoom, _p1MinZoom, Mathf.Max(_p1MaxZoom, _p1MinZoom + 0.01f));
                _p2ZoomIndex = -1;
                return;
            }

            // Mirror vanilla CameraZoom more closely: the save/pref value is only used
            // to SEED runtime zoom state, then the live `zoom`/`zoomIndex` fields are
            // authoritative until the component is torn down. Re-reading the pref every
            // frame causes brief outward movement followed by a snap back to the old
            // distance if the pref path lags behind the runtime toggle.
            if (_p2ZoomIndex < 0 || _p2ZoomIndex >= _p2ZoomArray.Length)
            {
                float seed = Mathf.Clamp(P2CameraDistance, _p1MinZoom, Mathf.Max(_p1MaxZoom, _p1MinZoom + 0.01f));
                _p2ManualZoom = seed;
                _p2ZoomIndex = FindNearestP2ZoomIndex(seed);
            }
            else
            {
                _p2ManualZoom = Mathf.Clamp(_p2ManualZoom, _p1MinZoom, Mathf.Max(_p1MaxZoom, _p1MinZoom + 0.01f));
            }
        }

        private void CycleP2CameraZoom()
        {
            EnsureCameraDynamicsCached();
            SyncP2ZoomStateFromPrefs();
            if (_p2ZoomArray == null || _p2ZoomArray.Length == 0) return;

            _p2ZoomIndex++;
            if (_p2ZoomIndex > _p2ZoomArray.Length - 1)
                _p2ZoomIndex = 0;

            _p2ManualZoom = _p2ZoomArray[_p2ZoomIndex];
            P2CameraDistance = _p2ManualZoom;
            try
            {
                if (_p2CameraDistancePref != null)
                    _p2CameraDistancePref.Value = _p2ManualZoom;
            }
            catch { }

        }

        // Resolves P1's Cinemachine3rdPersonFollow body component on the follow vcam.
        // Centralized so both distance reads and collision-setting reads can share the
        // generic-method lookup, and so we don't need a compile-time Cinemachine reference.
        private bool TryGetP1Follow3rdPerson(out object follow, out Type followType)
        {
            follow = null; followType = null;
            try
            {
                var ccType = AccessTools.TypeByName("_Scripts.Singletons.CameraController");
                if (ccType == null) return false;
                var instProp = ccType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
                var cc = instProp != null ? instProp.GetValue(null) : null;
                if (cc == null)
                {
                    var arr = UnityEngine.Object.FindObjectsOfType(ccType, true);
                    if (arr != null && arr.Length > 0) cc = arr[0];
                }
                if (cc == null) return false;
                var camField = ccType.GetField("cinemachineFollowCamera", BindingFlags.Instance | BindingFlags.NonPublic);
                if (camField == null) return false;
                var vcam = camField.GetValue(cc);
                if (vcam == null) return false;

                followType = AccessTools.TypeByName("Cinemachine.Cinemachine3rdPersonFollow");
                if (followType == null) return false;

                MethodInfo generic = null;
                var methods = vcam.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    if (m.Name != "GetCinemachineComponent") continue;
                    if (!m.IsGenericMethodDefinition) continue;
                    if (m.GetParameters().Length != 0) continue;
                    generic = m; break;
                }
                if (generic == null) return false;

                follow = generic.MakeGenericMethod(followType).Invoke(vcam, null);
                return follow != null;
            }
            catch { }
            return false;
        }

        // Mirrors _Scripts.Camera.CameraZoom.HandleCameraZoom for P2:
        //   - autoZoom: target = clamp(minZoom + speed, max=maxZoom)
        //   - non-autoZoom: target = P2's independent manual zoom preset
        //   - smoothed via ExponentialDecay(current, target, decay=5, dt)
        //   - if zoomInWhenLookingUp && walking && !webTouched:
        //       finalDist = Lerp(smoothed, 0, max(dot(spider.up, cam.forward), 0) * 1.5)
        private float ComputeP2DynamicCameraDistance(Vector3 camForward, Vector3 surfUp, float dt)
        {
            float minZoom = _p1MinZoom;
            float maxZoom = Mathf.Max(_p1MaxZoom, minZoom + 0.01f);

            bool autoZoom = false;
            try
            {
                if (_settingsAutoZoomProp != null)
                    autoZoom = (bool)_settingsAutoZoomProp.GetValue(null);
            }
            catch { }

            float speed = 0f;
            bool walking = false;
            bool webTouched = false;
            try
            {
                if (_p2BodyMovement != null)
                {
                    if (_bmRbProp != null)
                    {
                        var rb = _bmRbProp.GetValue(_p2BodyMovement) as Rigidbody;
                        if (rb != null) speed = rb.linearVelocity.magnitude;
                    }
                    if (_bmStateProp != null && _bmWalkingState != null)
                    {
                        var st = _bmStateProp.GetValue(_p2BodyMovement);
                        walking = st != null && st.Equals(_bmWalkingState);
                    }
                    if (_bmWebTouchedProp != null)
                    {
                        var wt = _bmWebTouchedProp.GetValue(_p2BodyMovement);
                        if (wt is bool b) webTouched = b;
                    }
                }
            }
            catch { }

            SyncP2ZoomStateFromPrefs();
            float targetZoom = autoZoom
                ? Mathf.Min(minZoom + speed * 1f, maxZoom)
                : Mathf.Clamp(_p2ManualZoom, minZoom, maxZoom);

            if (_p2CamSmoothedZoom < 0f) _p2CamSmoothedZoom = targetZoom;
            _p2CamSmoothedZoom = ExponentialDecay(_p2CamSmoothedZoom, targetZoom, 5f, dt);

            float finalDist = _p2CamSmoothedZoom;
            if (_p1ZoomInWhenLookingUp && walking && !webTouched)
            {
                float dot = Mathf.Max(Vector3.Dot(surfUp, camForward.normalized), 0f);
                finalDist = Mathf.Lerp(_p2CamSmoothedZoom, 0f, dot * 1.5f);
            }
            return finalDist;
        }

        // Matches the standard signature of _Scripts.Utils.Utils.ExponentialDecay used by
        // CameraZoom: an exponential approach to `target` with decay rate `decay` over `dt`.
        private static float ExponentialDecay(float current, float target, float decay, float dt)
        {
            return target + (current - target) * Mathf.Exp(-decay * dt);
        }

        // Reads the current Cinemachine3rdPersonFollow.CameraDistance off P1's follow vcam
        // via reflection (the field is private in CameraController). Returns false if
        // anything is missing so callers can fall back gracefully.
        private bool TryReadP1CameraDistance(out float dist)
        {
            dist = 0f;
            try
            {
                if (!TryGetP1Follow3rdPerson(out var follow, out var followType)) return false;
                var distField = followType.GetField("CameraDistance", BindingFlags.Instance | BindingFlags.Public);
                if (distField == null) return false;
                dist = (float)distField.GetValue(follow);
                return dist > 0.01f;
            }
            catch { }
            return false;
        }

        private static float NormalizeSignedAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f) angle -= 360f;
            if (angle < -180f) angle += 360f;
            return angle;
        }

        // Reads P2's BodyMovement ground layer mask (the surfaces the spider walks on)
        // for the ground-collision-exclusion speed fix. Falls back to whatIsGroundDefault.
        private LayerMask GetP2WhatIsGround()
        {
            try
            {
                var bmType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
                if (bmType == null || _p2Spider == null) return default;
                var bm = _p2Spider.GetComponentInChildren(bmType, true) as Component;
                if (bm == null) return default;

                const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                foreach (var name in new[] { "whatIsGround", "whatIsGroundDefault" })
                {
                    var f = bmType.GetField(name, F);
                    if (f == null) continue;
                    try
                    {
                        var v = (LayerMask)f.GetValue(bm);
                        if (v.value != 0) return v;
                    }
                    catch { }
                }
            }
            catch { }
            return default;
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

        private static bool CanUseSplitScreenInCurrentScene()
        {
            return FindPlayerSpider() != null;
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

                if (string.Equals(fullName, "_Scripts.Camera.CameraWaterTrigger", StringComparison.Ordinal)) continue;

                if (fullName.StartsWith("_Scripts.Camera.", StringComparison.Ordinal) ||
                    fullName.StartsWith("_Scripts.Singletons.", StringComparison.Ordinal) ||
                    fullName.IndexOf("Camera", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    string.Equals(fullName, "Cinemachine.CinemachineBrain", StringComparison.Ordinal))
                    behaviour.enabled = false;
            }
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


        // Maps a Transform from one cloned PlayerSpider hierarchy to the corresponding
        // Transform in the other hierarchy.  We use name + same-name ordinal at every
        // level instead of raw sibling index so UpdateFix removing unrelated cloned
        // children cannot invalidate a delayed repair.
        private static Transform MapTransformBetweenCloneRoots(Transform sourceRoot, Transform destinationRoot, Transform source)
        {
            if (sourceRoot == null || destinationRoot == null || source == null)
                return null;
            if (object.ReferenceEquals(source, sourceRoot))
                return destinationRoot;

            var names = new List<string>();
            var ordinals = new List<int>();
            Transform cursor = source;
            while (cursor != null && !object.ReferenceEquals(cursor, sourceRoot))
            {
                Transform parent = cursor.parent;
                if (parent == null)
                    return null;

                int sameNameOrdinal = 0;
                int siblingIndex = cursor.GetSiblingIndex();
                for (int i = 0; i < siblingIndex; i++)
                {
                    Transform sibling = parent.GetChild(i);
                    if (sibling != null && string.Equals(sibling.name, cursor.name, StringComparison.Ordinal))
                        sameNameOrdinal++;
                }

                names.Add(cursor.name);
                ordinals.Add(sameNameOrdinal);
                cursor = parent;
            }

            if (!object.ReferenceEquals(cursor, sourceRoot))
                return null;

            Transform mapped = destinationRoot;
            for (int seg = names.Count - 1; seg >= 0; seg--)
            {
                string wantedName = names[seg];
                int wantedOrdinal = ordinals[seg];
                int seen = 0;
                Transform next = null;
                for (int c = 0; c < mapped.childCount; c++)
                {
                    Transform child = mapped.GetChild(c);
                    if (child == null || !string.Equals(child.name, wantedName, StringComparison.Ordinal))
                        continue;
                    if (seen == wantedOrdinal)
                    {
                        next = child;
                        break;
                    }
                    seen++;
                }
                if (next == null)
                    return null;
                mapped = next;
            }
            return mapped;
        }

        private static int GetComponentOrdinal(Component component)
        {
            if (component == null) return -1;
            Component[] siblings = component.gameObject.GetComponents(component.GetType());
            for (int i = 0; i < siblings.Length; i++)
                if (object.ReferenceEquals(siblings[i], component))
                    return i;
            return -1;
        }

        private static Component MapComponentBetweenCloneRoots(GameObject sourceRoot, GameObject destinationRoot, Component source)
        {
            if (sourceRoot == null || destinationRoot == null || source == null)
                return null;

            Transform mappedTransform = MapTransformBetweenCloneRoots(sourceRoot.transform, destinationRoot.transform, source.transform);
            if (mappedTransform == null)
                return null;

            Component[] candidates = mappedTransform.gameObject.GetComponents(source.GetType());
            int ordinal = GetComponentOrdinal(source);
            if (ordinal >= 0 && ordinal < candidates.Length)
                return candidates[ordinal];
            return candidates.Length > 0 ? candidates[0] : null;
        }

        // Rewrites direct Transform members in Animation Rigging constraint data from
        // P1 to the equivalent P2 hierarchy.  TwoBoneIKConstraintData exposes root,
        // mid, tip, target and hint as Transform properties; the generic reflection here
        // also covers other Animation Rigging constraints that use direct Transform refs.
        private static int CanonicalizeP2AnimationRigConstraintReferences(GameObject p1Root, GameObject p2Root)
        {
            if (p1Root == null || p2Root == null)
                return 0;

            int changedRefs = 0;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            MonoBehaviour[] p1Behaviours = p1Root.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < p1Behaviours.Length; i++)
            {
                MonoBehaviour p1Constraint = p1Behaviours[i];
                if (p1Constraint == null) continue;

                Type constraintType = p1Constraint.GetType();
                string ns = constraintType.Namespace ?? string.Empty;
                if (!ns.StartsWith("UnityEngine.Animations.Rigging", StringComparison.Ordinal) ||
                    constraintType.Name.IndexOf("Constraint", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                Component p2Constraint = MapComponentBetweenCloneRoots(p1Root, p2Root, p1Constraint);
                if (p2Constraint == null)
                    continue;

                PropertyInfo dataProperty = constraintType.GetProperty("data", flags);
                FieldInfo dataField = constraintType.GetField("m_Data", flags);
                object p1Data = null;
                object p2Data = null;
                try
                {
                    if (dataProperty != null && dataProperty.CanRead)
                    {
                        p1Data = dataProperty.GetValue(p1Constraint, null);
                        p2Data = dataProperty.GetValue(p2Constraint, null);
                    }
                    else if (dataField != null)
                    {
                        p1Data = dataField.GetValue(p1Constraint);
                        p2Data = dataField.GetValue(p2Constraint);
                    }
                }
                catch { continue; }

                if (p1Data == null || p2Data == null)
                    continue;

                bool dataChanged = false;
                Type dataType = p1Data.GetType();

                PropertyInfo[] properties = dataType.GetProperties(flags);
                for (int pi = 0; pi < properties.Length; pi++)
                {
                    PropertyInfo prop = properties[pi];
                    if (prop.PropertyType != typeof(Transform) || !prop.CanRead || !prop.CanWrite ||
                        prop.GetIndexParameters().Length != 0)
                        continue;
                    try
                    {
                        Transform p1Ref = prop.GetValue(p1Data, null) as Transform;
                        Transform mapped = MapTransformBetweenCloneRoots(p1Root.transform, p2Root.transform, p1Ref);
                        if (mapped == null) continue;
                        Transform current = prop.GetValue(p2Data, null) as Transform;
                        if (!object.ReferenceEquals(current, mapped))
                        {
                            prop.SetValue(p2Data, mapped, null);
                            changedRefs++;
                            dataChanged = true;
                        }
                    }
                    catch { }
                }

                FieldInfo[] fields = dataType.GetFields(flags);
                for (int fi = 0; fi < fields.Length; fi++)
                {
                    FieldInfo field = fields[fi];
                    if (field.FieldType != typeof(Transform) || field.IsInitOnly)
                        continue;
                    try
                    {
                        Transform p1Ref = field.GetValue(p1Data) as Transform;
                        Transform mapped = MapTransformBetweenCloneRoots(p1Root.transform, p2Root.transform, p1Ref);
                        if (mapped == null) continue;
                        Transform current = field.GetValue(p2Data) as Transform;
                        if (!object.ReferenceEquals(current, mapped))
                        {
                            field.SetValue(p2Data, mapped);
                            changedRefs++;
                            dataChanged = true;
                        }
                    }
                    catch { }
                }

                if (!dataChanged)
                    continue;

                try
                {
                    if (dataProperty != null && dataProperty.CanWrite)
                        dataProperty.SetValue(p2Constraint, p2Data, null);
                    else if (dataField != null)
                        dataField.SetValue(p2Constraint, p2Data);
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning("[P2LegBinding] Could not write canonical rig data for " +
                        constraintType.Name + ": " + ex.Message);
                }
            }
            return changedRefs;
        }


        private static int CanonicalizeP2RigBuilderLayerReferences(GameObject p1Root, GameObject p2Root)
        {
            if (p1Root == null || p2Root == null)
                return 0;

            Type rigBuilderType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                rigBuilderType = asm.GetType("UnityEngine.Animations.Rigging.RigBuilder");
                if (rigBuilderType != null) break;
            }
            if (rigBuilderType == null)
                return 0;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            int changed = 0;
            var p1Builders = p1Root.GetComponentsInChildren(rigBuilderType, true);
            for (int bi = 0; bi < p1Builders.Length; bi++)
            {
                Component p1Builder = p1Builders[bi] as Component;
                if (p1Builder == null) continue;
                Component p2Builder = MapComponentBetweenCloneRoots(p1Root, p2Root, p1Builder);
                if (p2Builder == null) continue;

                PropertyInfo layersProp = rigBuilderType.GetProperty("layers", flags);
                FieldInfo layersField = rigBuilderType.GetField("m_RigLayers", flags);
                object p1LayersObj = null;
                object p2LayersObj = null;
                try
                {
                    if (layersProp != null && layersProp.CanRead)
                    {
                        p1LayersObj = layersProp.GetValue(p1Builder, null);
                        p2LayersObj = layersProp.GetValue(p2Builder, null);
                    }
                    else if (layersField != null)
                    {
                        p1LayersObj = layersField.GetValue(p1Builder);
                        p2LayersObj = layersField.GetValue(p2Builder);
                    }
                }
                catch { continue; }

                var p1Layers = p1LayersObj as System.Collections.IList;
                var p2Layers = p2LayersObj as System.Collections.IList;
                if (p1Layers == null || p2Layers == null)
                    continue;

                int count = Math.Min(p1Layers.Count, p2Layers.Count);
                for (int li = 0; li < count; li++)
                {
                    object p1Layer = p1Layers[li];
                    object p2Layer = p2Layers[li];
                    if (p1Layer == null || p2Layer == null) continue;

                    Type layerType = p1Layer.GetType();
                    PropertyInfo rigProp = layerType.GetProperty("rig", flags);
                    FieldInfo rigField = layerType.GetField("rig", flags) ?? layerType.GetField("m_Rig", flags);
                    Component p1Rig = null;
                    Component p2Rig = null;
                    try
                    {
                        if (rigProp != null && rigProp.CanRead)
                        {
                            p1Rig = rigProp.GetValue(p1Layer, null) as Component;
                            p2Rig = rigProp.GetValue(p2Layer, null) as Component;
                        }
                        else if (rigField != null)
                        {
                            p1Rig = rigField.GetValue(p1Layer) as Component;
                            p2Rig = rigField.GetValue(p2Layer) as Component;
                        }
                    }
                    catch { continue; }

                    Component mappedRig = MapComponentBetweenCloneRoots(p1Root, p2Root, p1Rig);
                    if (mappedRig == null || object.ReferenceEquals(mappedRig, p2Rig))
                        continue;

                    try
                    {
                        if (rigProp != null && rigProp.CanWrite)
                            rigProp.SetValue(p2Layer, mappedRig, null);
                        else if (rigField != null && !rigField.IsInitOnly)
                            rigField.SetValue(p2Layer, mappedRig);
                        else
                            continue;
                        changed++;
                    }
                    catch { }
                }
            }
            return changed;
        }

        private static int RebindP2LegDriversFromP1(GameObject p1Root, GameObject p2Root)
        {
            if (p1Root == null || p2Root == null)
                return 0;

            Type legType = AccessTools.TypeByName("_Scripts.Spider.LegController");
            if (legType == null)
                return 0;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            FieldInfo targetField = legType.GetField("target", flags);
            FieldInfo centerField = legType.GetField("center", flags);
            FieldInfo jumpField = legType.GetField("targetJump", flags);
            if (targetField == null)
                return 0;

            int rebound = 0;
            P2LegDriver[] drivers = p2Root.GetComponentsInChildren<P2LegDriver>(true);
            for (int i = 0; i < drivers.Length; i++)
            {
                P2LegDriver driver = drivers[i];
                if (driver == null) continue;

                Transform p1LegTransform = MapTransformBetweenCloneRoots(p2Root.transform, p1Root.transform, driver.transform);
                Component p1Leg = p1LegTransform != null ? p1LegTransform.GetComponent(legType) as Component : null;
                if (p1Leg == null) continue;

                Transform p1Target = targetField.GetValue(p1Leg) as Transform;
                Transform p1Center = centerField != null ? centerField.GetValue(p1Leg) as Transform : null;
                Transform p1Jump = jumpField != null ? jumpField.GetValue(p1Leg) as Transform : null;

                Transform p2Target = MapTransformBetweenCloneRoots(p1Root.transform, p2Root.transform, p1Target);
                Transform p2Center = MapTransformBetweenCloneRoots(p1Root.transform, p2Root.transform, p1Center);
                Transform p2Jump = MapTransformBetweenCloneRoots(p1Root.transform, p2Root.transform, p1Jump);

                if (driver.RebindAuthoredTransforms(p2Target, p2Center, p2Jump))
                    rebound++;
            }
            return rebound;
        }

        private static void RebuildP2RigBuilders(GameObject p2Root, out int cleared, out int built)
        {
            cleared = 0;
            built = 0;
            if (p2Root == null) return;

            Type rigBuilderType = null;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                rigBuilderType = asm.GetType("UnityEngine.Animations.Rigging.RigBuilder");
                if (rigBuilderType != null) break;
            }
            if (rigBuilderType == null) return;

            MethodInfo clearMethod = rigBuilderType.GetMethod("Clear",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);
            MethodInfo buildMethod = rigBuilderType.GetMethod("Build",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);

            var builders = p2Root.GetComponentsInChildren(rigBuilderType, true);
            for (int i = 0; i < builders.Length; i++)
            {
                object builder = builders[i];
                if (builder == null) continue;
                try
                {
                    if (clearMethod != null)
                    {
                        clearMethod.Invoke(builder, null);
                        cleared++;
                    }
                    if (buildMethod != null)
                    {
                        buildMethod.Invoke(builder, null);
                        built++;
                    }
                }
                catch (Exception ex)
                {
                    MelonLogger.Warning("[P2LegBinding] RigBuilder clear/build failed: " + ex.Message);
                }
            }
        }

        private System.Collections.IEnumerator DelayedP2LegBindingRepair(GameObject p1Root, GameObject p2Root)
        {
            // Unity Explorer isolated the actual scene-entry failure:
            // - P2's Animator is normally disabled after cloning.
            // - Animator.Rebind() while it is disabled does not repair the legs.
            // - Temporarily enabling that Animator and disabling it again DOES immediately
            //   restore the correct underlying leg-bone pose.
            // - P1 only relaxes slightly when an IK constraint is removed, whereas the bad
            //   P2 leg crosses the body when its IK weight is set to zero.  Therefore the bad
            //   state exists below TwoBoneIK: P2 can inherit/freeze a transient base Animator
            //   pose during automatic level-entry cloning.
            //
            // Reproduce the proven Unity Explorer action once, after UpdateFix's required
            // PlayerSpider_P2 root lifecycle pulse has completed.  Keep the Animator enabled
            // for one real frame so Unity evaluates the skeleton, then restore its original
            // disabled state.  F9 spawning does not need this because it occurs after P1's
            // visual pose is already stable.
            float[] checkpoints = { 0.60f, 1.20f, 2.00f };
            float started = Time.realtimeSinceStartup;
            bool scenePoseSettled = false;
            bool animatorPoseRefreshed = false;

            for (int ci = 0; ci < checkpoints.Length; ci++)
            {
                float due = started + checkpoints[ci];
                while (Time.realtimeSinceStartup < due)
                {
                    if (p1Root == null || p2Root == null || !object.ReferenceEquals(_p2Spider, p2Root))
                        yield break;
                    yield return null;
                }

                if (p1Root == null || p2Root == null || !object.ReferenceEquals(_p2Spider, p2Root))
                    yield break;

                // Only automatic scene-entry spawning reaches this coroutine.  Pulse the
                // disabled Animator once at the first checkpoint where P2 is active.  Prefer
                // the root Animator (the one confirmed in Unity Explorer); only fall back to
                // active child Animators if the root has none.
                int animatorPulsed = 0;
                if (!animatorPoseRefreshed && p2Root.activeInHierarchy)
                {
                    var animatorsToPulse = new List<Animator>();
                    Animator rootAnimator = p2Root.GetComponent<Animator>();
                    if (rootAnimator != null)
                    {
                        if (!rootAnimator.enabled && rootAnimator.gameObject.activeInHierarchy)
                            animatorsToPulse.Add(rootAnimator);
                    }
                    else
                    {
                        Animator[] childAnimators = p2Root.GetComponentsInChildren<Animator>(true);
                        for (int ai = 0; ai < childAnimators.Length; ai++)
                        {
                            Animator animator = childAnimators[ai];
                            if (animator == null || animator.enabled || !animator.gameObject.activeInHierarchy)
                                continue;
                            animatorsToPulse.Add(animator);
                        }
                    }

                    if (animatorsToPulse.Count > 0)
                    {
                        for (int ai = 0; ai < animatorsToPulse.Count; ai++)
                        {
                            Animator animator = animatorsToPulse[ai];
                            if (animator == null) continue;
                            try
                            {
                                animator.enabled = true;
                                // A zero-delta evaluation makes the intent explicit while the
                                // component is enabled; the following real frame matches the
                                // manual Unity Explorer enable/disable test exactly.
                                animator.Update(0f);
                                animatorPulsed++;
                            }
                            catch (Exception ex)
                            {
                                LoggerInstance.Warning("[P2AnimatorPose] Failed to enable/evaluate " + animator.name + ": " + ex.Message);
                            }
                        }

                        LoggerInstance.Msg("[P2AnimatorPose] Enabled " + animatorPulsed +
                            " previously-disabled P2 Animator(s) for one frame to normalize the cloned bone pose.");

                        // Let Unity run one normal Animator evaluation / transform write pass.
                        yield return null;

                        // Always restore the mod/game's intended disabled state before doing
                        // any target settling or RigBuilder work.
                        for (int ai = 0; ai < animatorsToPulse.Count; ai++)
                        {
                            Animator animator = animatorsToPulse[ai];
                            if (animator == null) continue;
                            try
                            {
                                if (animator.enabled)
                                {
                                    animator.Update(0f);
                                    animator.enabled = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggerInstance.Warning("[P2AnimatorPose] Failed to restore disabled Animator " + animator.name + ": " + ex.Message);
                            }
                        }

                        LoggerInstance.Msg("[P2AnimatorPose] Restored P2 Animator disabled state after one-frame pose refresh.");
                        animatorPoseRefreshed = true;

                        if (p1Root == null || p2Root == null || !object.ReferenceEquals(_p2Spider, p2Root))
                            yield break;
                    }
                    else
                    {
                        // If no disabled active Animator exists, do not keep retrying forever.
                        // This also avoids touching alternate/inactive visual-mode animators.
                        animatorPoseRefreshed = true;
                        LoggerInstance.Msg("[P2AnimatorPose] No disabled active P2 Animator required a pose refresh.");
                    }
                }

                int reboundDrivers = RebindP2LegDriversFromP1(p1Root, p2Root);

                int forcedPoseSettles = 0;
                int playerAnchorsRepaired = 0;
                int crossedTargetsRepaired = 0;
                P2LegDriver[] drivers = p2Root.GetComponentsInChildren<P2LegDriver>(true);
                for (int di = 0; di < drivers.Length; di++)
                {
                    P2LegDriver driver = drivers[di];
                    if (driver == null) continue;

                    if (driver.RepairPlayerOwnedAnchorIfNeeded("post-scene t=" + checkpoints[ci].ToString("F2") + "s"))
                        playerAnchorsRepaired++;

                    if (!scenePoseSettled)
                    {
                        if (driver.ForceSceneSpawnSettle("post-scene t=" + checkpoints[ci].ToString("F2") + "s"))
                            forcedPoseSettles++;
                    }
                    else if (driver.RepairCrossedTargetIfNeeded("post-scene t=" + checkpoints[ci].ToString("F2") + "s"))
                    {
                        crossedTargetsRepaired++;
                    }
                }

                // All drivers share the same BodyMovement state, so a nonzero settle count
                // normally means all eight were re-seated. If P2 was still in Jumping state,
                // the count remains zero and the next checkpoint tries again.
                if (forcedPoseSettles > 0)
                    scenePoseSettled = true;

                int changedConstraintRefs = CanonicalizeP2AnimationRigConstraintReferences(p1Root, p2Root);
                int changedRigLayers = CanonicalizeP2RigBuilderLayerReferences(p1Root, p2Root);
                int cleared = 0, built = 0;

                // After the Animator pulse, rebuild once when the feet are force-settled so
                // Animation Rigging consumes the normalized base-bone pose plus canonical
                // targets. Subsequent no-op checkpoints leave the graph alone.
                if (reboundDrivers > 0 || changedConstraintRefs > 0 || changedRigLayers > 0 || forcedPoseSettles > 0)
                    RebuildP2RigBuilders(p2Root, out cleared, out built);

                LoggerInstance.Msg("[P2LegBinding] post-scene repair t=" + checkpoints[ci].ToString("F2") +
                    "s: animatorPulsed=" + animatorPulsed +
                    " driversRebound=" + reboundDrivers +
                    " forcedPoseSettles=" + forcedPoseSettles +
                    " playerAnchorsRepaired=" + playerAnchorsRepaired +
                    " crossedTargetsRepaired=" + crossedTargetsRepaired +
                    " constraintRefsChanged=" + changedConstraintRefs +
                    " rigLayersChanged=" + changedRigLayers +
                    " rigCleared=" + cleared + " rigBuilt=" + built + ".");
            }
        }

        // Pre-clone safety: re-parent transforms the game reassigns to external surfaces
        // (BodyMovement.targetTransform, LegController.targetLocal) back into the spider
        // hierarchy so Instantiate deep-copies them instead of leaving the clone pointing
        // at P1's instance. Without this, P1 and P2 share a movement target while grounded.
        private sealed class ReanchoredTransformParent
        {
            public Transform Transform;
            public Transform Parent;
            public int SiblingIndex;
        }

        // The game's shoot InputAction is shared by every gamepad. If P2 holds RT,
        // Input System can leave that action in Performed and P1's next RT press does
        // not produce a new callback. Poll P1's own pad and invoke the mobile path only
        // when the native callback did not already change the live P1 web state.
        private void DriveP1ShootFallback()
        {
            bool held = InputCompat.IsP1ShootRTHeldNow(P2GamepadIndex, P2TriggerThreshold);
            bool down = held && !_p1ShootHeldPrev;
            bool up = !held && _p1ShootHeldPrev;
            _p1ShootHeldPrev = held;

            if ((!down && !up) || _webController == null || InP2WebContext)
                return;

            try
            {
                var webType = _webController.GetType();
                if (_p1MobileShootWebMethod == null)
                    _p1MobileShootWebMethod = webType.GetMethod("MobileShootWeb",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, new[] { typeof(bool) }, null);
                if (_p1WebActiveField == null)
                    _p1WebActiveField = webType.GetField("webActive",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (_p1MobileShootWebMethod == null || _p1WebActiveField == null)
                    return;

                bool webActive = (bool)_p1WebActiveField.GetValue(_webController);
                if ((down && !webActive) || (up && webActive))
                {
                    _p1MobileShootWebMethod.Invoke(_webController, new object[] { down });
                    LoggerInstance.Msg("[P1WebFallback] Recovered a missed P1 " + (down ? "grapple press." : "grapple release."));
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("[P1WebFallback] Failed: " + ex.Message);
            }
        }

        // The P2 state capsule can expose a bad vanilla invariant: WebController says
        // P1's web is inactive while its old SpringJoint is still alive. Vanilla's next
        // AttachWeb then fails because ActivateSpringJoint refuses to create a second
        // joint. The orphan also continues participating in physics. Repair the live P1
        // state before processing either player's web input.
        private void RepairP1WebState()
        {
            if (_webController == null || InP2WebContext)
                return;

            try
            {
                var webType = _webController.GetType();
                if (_p1WebActiveField == null)
                    _p1WebActiveField = webType.GetField("webActive",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (_p1SpringJointField == null)
                    _p1SpringJointField = webType.GetField("springJoint",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (_p1DeactivateSpringJointMethod == null)
                    _p1DeactivateSpringJointMethod = webType.GetMethod("DeactivateSpringJoint",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, new[] { typeof(bool) }, null);
                if (_p1ReleaseWebMethod == null)
                    _p1ReleaseWebMethod = webType.GetMethod("ReleaseWeb",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        null, new[] { typeof(bool) }, null);

                if (_p1WebActiveField == null || _p1SpringJointField == null)
                    return;

                bool active = (bool)_p1WebActiveField.GetValue(_webController);
                SpringJoint joint = _p1SpringJointField.GetValue(_webController) as SpringJoint;
                if (!active && joint != null)
                {
                    if (_p1DeactivateSpringJointMethod != null)
                        _p1DeactivateSpringJointMethod.Invoke(_webController, new object[] { true });
                    else
                    {
                        UnityEngine.Object.Destroy(joint);
                        _p1SpringJointField.SetValue(_webController, null);
                    }
                    LoggerInstance.Msg("[P1WebRepair] Removed an orphaned P1 spring joint.");
                }
                else if (active && joint == null && _p1ReleaseWebMethod != null)
                {
                    _p1ReleaseWebMethod.Invoke(_webController, new object[] { false });
                    LoggerInstance.Msg("[P1WebRepair] Cleared P1 webActive after its joint disappeared.");
                }
            }
            catch (Exception ex)
            {
                LoggerInstance.Warning("[P1WebRepair] Failed: " + ex.Message);
            }
        }

        private static void ReanchorSharedTargets(GameObject p1Spider, List<ReanchoredTransformParent> reanchored)
        {
            if (p1Spider == null || reanchored == null) return;
            try
            {
                ReanchorField(p1Spider, "_Scripts.Spider.BodyMovement", "targetTransform", reanchored);
                // NOTE: do NOT reanchor LegController.targetLocal here. The cloned
                // LegControllers on P2 are destroyed synchronously right after
                // Instantiate (no Update runs on them), so they can't share P1's
                // targetLocal long enough to matter — and re-parenting targetLocal
                // away from the ground freezes P1's legs until the next jump.
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[ReanchorSharedTargets] non-fatal: " + ex);
            }
        }

        private static void ReanchorField(GameObject p1Spider, string typeName, string fieldName, List<ReanchoredTransformParent> reanchored)
        {
            var t = AccessTools.TypeByName(typeName);
            if (t == null) return;
            var f = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null) return;

            var comps = p1Spider.GetComponentsInChildren(t, true);
            if (comps == null) return;

            int reanchoredCount = 0;
            for (int i = 0; i < comps.Length; i++)
            {
                var comp = comps[i] as Component;
                if (comp == null) continue;

                Transform tt = null;
                try { tt = f.GetValue(comp) as Transform; } catch { }
                if (tt == null) continue;

                if (!tt.IsChildOf(p1Spider.transform))
                {
                    reanchored.Add(new ReanchoredTransformParent
                    {
                        Transform = tt,
                        Parent = tt.parent,
                        SiblingIndex = tt.GetSiblingIndex()
                    });
                    tt.SetParent(comp.transform, worldPositionStays: true);
                    reanchoredCount++;
                }
            }
            if (reanchoredCount > 0)
                MelonLogger.Msg("[ReanchorSharedTargets] temporarily re-parented " + reanchoredCount + " " + typeName + "." + fieldName + " for the P2 clone.");
        }

        private static void RestoreReanchoredTargets(List<ReanchoredTransformParent> reanchored)
        {
            if (reanchored == null) return;

            for (int i = reanchored.Count - 1; i >= 0; i--)
            {
                var state = reanchored[i];
                if (state == null || state.Transform == null) continue;

                state.Transform.SetParent(state.Parent, worldPositionStays: true);
                if (state.Parent != null)
                {
                    int maxIndex = Math.Max(0, state.Parent.childCount - 1);
                    state.Transform.SetSiblingIndex(Math.Min(state.SiblingIndex, maxIndex));
                }
            }
        }

        private void Teardown()
        {
            // Defensive cleanup if setup was interrupted between re-parenting P1's
            // target and cloning P2.  This must never survive into single-player.
            RestoreReanchoredTargets(_p1CloneReanchors);
            _p1CloneReanchors.Clear();

            foreach (var pair in _globalEffectCameraMasks)
            {
                if (pair.Key != null)
                    pair.Key.cullingMask = pair.Value;
            }
            _globalEffectCameraMasks.Clear();
            ResetWaterRenderingState();
            AWJSplitScreenUpdateFix.UpdateFixMod.ResetP2VisualResources();

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

            // Drop cached LegController identity mappings — they reference
            // instanceIDs that may belong to the destroyed P2 spider.
            try { LegControllerPatches.ClearCache(); } catch { }
            try { P2MovableCollisionHelper.Reset(); } catch { }

            _webController = null;
            _p1MobileShootWebMethod = null;
            _p1WebActiveField = null;
            _p1SpringJointField = null;
            _p1DeactivateSpringJointMethod = null;
            _p1ReleaseWebMethod = null;
            _p1ShootHeldPrev = false;
            _p2SpiderInteraction = null;
            _p2SpiderMobileInteractMethod = null;

            _p1InputTransform = null;
            _p2CamRigInited = false;
            _p2CamYaw = 0f;
            _p2CamLookY = 0f;
            _p2CamSmoothedZoom = -1f;
            _p2ZoomArray = null;
            _p2ZoomIndex = -1;
            _p2ManualZoom = Mathf.Clamp(P2CameraDistance, _p1MinZoom, Mathf.Max(_p1MaxZoom, _p1MinZoom + 0.01f));
            _p2CamCollidedDistance = -1f;
            _p2SelfColliders = null;
            _p2SelfColliderRefreshFrame = -1;
            _p2BodyMovement = null;
            P2BodyMovementInstance = null;
            // Note: P1BodyMovementInstance intentionally retained — P1 persists across toggles.
            _bmRbProp = null;
            _bmStateProp = null;
            _bmWebTouchedProp = null;
            _bmWalkingState = null;
            // Note: the P1 camera caches (_p1CameraZoom / _p1FollowCached / _p1CameraMouseLookCached)
            // are NOT reset here — Teardown runs first and its _p2ManualZoom clamp below relies on the
            // retained _p1MinZoom/_p1MaxZoom. DeferredSetup invalidates those caches immediately after
            // calling Teardown so a scene reload re-resolves them against the new P1 components.

            P2InputTransform = null;
            P2Camera = null;
            InP2WebContext = false;
            P2ShootHeld = false;
            P2JumpPressed = false;
            P1JumpPressed = false;
            P2SprintDesired = false;
            P2WebActive = false;
            P2WebTargetActive = false;
            BodyMovementUnderwaterPatches.Reset();
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
        private MethodInfo _mQuickBuild;        // MobileQuickBuild()
        private MethodInfo _mFixedAnchor;       // MobileFixedAnchor()
        private MethodInfo _mMovingAnchor;      // MobileMovingAnchor()
        private MethodInfo _mDeleteWebReleased; // MobileDeleteWebButtonReleased()

        // Reflection caches for WebController private fields used by the capsule swap.
        private FieldInfo _fBodyMovement;
        private FieldInfo _fWebStartPoint;
        private FieldInfo _fWebMode;          // enum WebMode
        private FieldInfo _fWebBuildingMode;  // enum WebBuildingMode
        private FieldInfo _fWebActive;
        private FieldInfo _fWebTargetActive;
        private FieldInfo _fWebAnchorActive;
        private FieldInfo _fSpringJoint;
        private FieldInfo _fWebTarget;
        private FieldInfo _fWebAnchor;
        private FieldInfo _fWebTargetObject;
        private FieldInfo _fOldWebTargetObject;
        private FieldInfo _fOldWebTargetObject1;
        private FieldInfo _fWebAnchorObject;
        private FieldInfo _fPlayerWebJoint;
        private FieldInfo _fDeleteWebPressed;
        private FieldInfo _fDeletePlayerWebsTimer;
        private FieldInfo _fWebTargetPrefab;
        private FieldInfo _fWebAnchorPrefab;
        // P1's visible target sphere (Transform with MeshRenderer + Mesh), the
        // dynamic-scale curve, and max web distance. We mirror these onto P2's
        // own target dot so it looks identical and scales with distance the
        // same way P1's does.
        private FieldInfo _fWebTargetGfx;
        private FieldInfo _fWebAnchorGfx;
        private FieldInfo _fWebIndicationLineRenderer;
        private FieldInfo _fWebTargetSize;
        private FieldInfo _fWebDistance;
        private FieldInfo _fWebTargetDefaultMaterial;
        private FieldInfo _fWebAnchorFixedAnchorMaterial;
        private FieldInfo _fWebTargetFixedAnchorMaterial;
        private FieldInfo _fWebAnchorMovingAnchorMaterial;
        private FieldInfo _fWebTargetMovingAnchorMaterial;
        // WebController caches these emitters from P1's WebTarget in AssignSounds.
        // They need to be part of the state capsule too, otherwise P2 actions play at
        // P1's target object (often outside the audible range of P2's camera).
        private FieldInfo _fBuildThreadSound;
        private FieldInfo _fAttachAnchorSound;
        private FieldInfo _fDeleteThreadSound;
        private FieldInfo _fCantBuildSound;
        private FieldInfo _fAttachToPlayerSound;
        private FieldInfo _fMusicThreadSound;
        private FieldInfo _fWebSound;
        private AnimationCurve _webTargetSizeCurve;
        private float _webDistanceVal = 50f;
        // CameraController.mainCamera private field — game systems often access this
        // field directly (Singleton<CameraController>.Instance.mainCamera) bypassing the
        // MainCamera property, so we must swap the field itself during P2 invocations.
        private static FieldInfo _fCcMainCamera;
        private static object _ccInstance;

        // Per-player P2 state hosting
        private Component _p2BodyMovement;
        private PropertyInfo _p2BodyMovementBallProp;
        private static PropertyInfo _settingsArachnophobiaModeProp;
        private Transform _p2Root;
        private Transform _p2WebTargetTr;
        private Transform _p2WebAnchorTr;
        private Transform _p2WebStartPoint;
        private Component _p2PlayerWebJoint;
        private WcCapsule _p2Capsule;
        private bool _p2HasWebTarget;
        private bool _p2HasWebAnchor;
        private bool _p2HasWebJoint;

        private Camera _p2Camera;
        private Transform _p2InputTransform;
        private Rigidbody _p2Rigidbody;

        // P2 reticle/anchor indicators cloned from P1's web gfx.
        private GameObject _p2TargetDot;
        private GameObject _p2AnchorDot;
        private LineRenderer _webIndicatorLine;
        private Material _p2TargetDotMaterial;
        private Material _p2AnchorDotMaterial;
        private Material _grappleLineMaterial;
        private float _p2DotScale = 0.5f;
        private float _p2NormalOffset = 0.05f;

        // P2 grapple/web visuals (driven by P2's WcCapsule state, not a custom SpringJoint)
        private LineRenderer _grappleLine;
        private float _grappleMaxDist = 50f;

        // P1-derived parameters (read via reflection/logging)
        private float _p1MaxDistance = 50f;
        private float _p1TargetScale = 0.5f;
        private float _webStartHeightOffset = 1.0f; // height of WebStartPoint above InputTransform

        // Input edge detection
        private bool _shootHeldPrev;
        private bool _attachHeldPrev;
        private bool _ltPrev;
        private bool _rbPrev;
        private bool _lbPrev;
        private bool _bPrev;

        // Hold-to-delete-all simulation (because WebController.Update only ticks P1's
        // capsule; P2's deletePlayerWebsTimer never advances naturally). Game uses 1f
        // hold; we mirror that here and invoke DestroyAllPlayerWebs once when reached.
        private float _p2DeleteHoldTimer;
        private bool _p2DeleteAllFired;
        private const float P2_DELETE_HOLD_DURATION = 1f;
        private MethodInfo _mDestroyAllPlayerWebs;

        // Cached members for the per-physics-step spring-joint anchor tick (FixedUpdate).
        private PropertyInfo _bmTargetRigidbodyProp;
        private bool _bmTargetRigidbodyPropCached;
        private PropertyInfo _moUseComAsWebAnchorProp;
        private Type _moUseComAsWebAnchorType;
        private Type _movableObjectType;
        private bool _movableObjectTypeCached;
        private Rigidbody _comAnchorCachedRb;
        private bool _comAnchorCachedValue;
        private SpringJoint _loggedAnchorTickJoint;

        private bool _inited;
        private MelonLogger.Instance _logger;
        private float _nextDebugLog;

        public void Init(Component p1WebController, Camera p2Camera, Transform p2InputTransform, GameObject p2Spider, MelonLogger.Instance logger, Transform p1InputTransform = null)
        {
            _logger = logger;
            SplitScreenMod.P2WebActive = false;
            SplitScreenMod.P2WebTargetActive = false;
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
                try { CreateAnchorDot(); } catch (Exception ex) { logger.Warning("[P2WebManager] CreateAnchorDot failed: " + ex); }
                try { CreateWebIndicatorLine(p2Spider); } catch (Exception ex) { logger.Warning("[P2WebManager] CreateWebIndicatorLine failed: " + ex); }
                try { CreateGrappleLine(p2Spider); } catch (Exception ex) { logger.Warning("[P2WebManager] CreateGrappleLine failed: " + ex); }

                // Set up P2-side WebController state (bodyMovement / Root / per-player webTarget+webAnchor / playerWebJoint).
                try { SetupP2WebState(p2Spider); }
                catch (Exception ex) { logger.Warning("[P2WebManager] SetupP2WebState failed (non-fatal): " + ex); }

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
            // Prefer cloning P1's own webTargetGfx so the visuals match exactly
            // (same mesh, same material). Falls back to a primitive sphere if
            // reflection didn't resolve the field for some reason.
            GameObject cloneSrc = null;
            try
            {
                if (_fWebTargetGfx != null && _p1WebController != null)
                {
                    var srcTr = _fWebTargetGfx.GetValue(_p1WebController) as Transform;
                    if (srcTr != null) cloneSrc = srcTr.gameObject;
                }
            }
            catch { }

            if (cloneSrc != null)
            {
                _p2TargetDot = UnityEngine.Object.Instantiate(cloneSrc);
                _p2TargetDot.name = "P2_WebTargetDot";
                _p2TargetDot.transform.SetParent(null, false);
                // Strip colliders so it doesn't interfere with raycasts.
                foreach (var c in _p2TargetDot.GetComponentsInChildren<Collider>(true))
                {
                    try { UnityEngine.Object.Destroy(c); } catch { }
                }
                // Match P1's render queue/shadow settings (its renderer is already
                // configured). Initial scale will be replaced every frame by the
                // dynamic-scale code in UpdateTargetDot.
                _p2TargetDot.transform.localScale = Vector3.one * _p2DotScale;
                _p2TargetDot.SetActive(false);
                _logger.Msg("[P2WebManager] Cloned target dot from P1 webTargetGfx.");
                return;
            }

            // Fallback: plain sphere (legacy path).
            _p2TargetDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _p2TargetDot.name = "P2_WebTargetDot";
            _p2TargetDot.transform.localScale = Vector3.one * _p2DotScale;

            var col = _p2TargetDot.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);

            var rend = _p2TargetDot.GetComponent<Renderer>();
            if (rend != null)
            {
                var shader = Shader.Find("Unlit/Color");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = rend.material.shader;
                var mat = new Material(shader);
                mat.color = new Color(1f, 0f, 0f, 1f);
                try { mat.SetColor("_Color", new Color(1f, 0f, 0f, 1f)); } catch { }
                try { mat.SetColor("_EmissionColor", new Color(1f, 0f, 0f, 1f)); } catch { }
                try { mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always); } catch { }
                try { mat.SetInt("_ZWrite", 0); } catch { }
                mat.renderQueue = 5000;
                rend.material = mat;
                _p2TargetDotMaterial = mat;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rend.receiveShadows = false;
            }

            _p2TargetDot.SetActive(false);
            _logger.Msg("[P2WebManager] Created target dot sphere (fallback).");
        }

        private void CreateAnchorDot()
        {
            GameObject cloneSrc = null;
            try
            {
                if (_fWebAnchorGfx != null && _p1WebController != null)
                {
                    var srcTr = _fWebAnchorGfx.GetValue(_p1WebController) as Transform;
                    if (srcTr != null) cloneSrc = srcTr.gameObject;
                }
            }
            catch { }

            if (cloneSrc != null)
            {
                _p2AnchorDot = UnityEngine.Object.Instantiate(cloneSrc);
                _p2AnchorDot.name = "P2_WebAnchorDot";
                _p2AnchorDot.transform.SetParent(null, false);
                foreach (var c in _p2AnchorDot.GetComponentsInChildren<Collider>(true))
                {
                    try { UnityEngine.Object.Destroy(c); } catch { }
                }
                _p2AnchorDot.transform.localScale = Vector3.one * _p2DotScale;
                _p2AnchorDot.SetActive(false);
                _logger.Msg("[P2WebManager] Cloned anchor dot from P1 webAnchorGfx.");
                return;
            }

            _p2AnchorDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _p2AnchorDot.name = "P2_WebAnchorDot";
            _p2AnchorDot.transform.localScale = Vector3.one * _p2DotScale;

            var col = _p2AnchorDot.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);

            var rend = _p2AnchorDot.GetComponent<Renderer>();
            if (rend != null)
            {
                var shader = Shader.Find("Unlit/Color");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = rend.material.shader;
                var mat = new Material(shader);
                mat.color = new Color(1f, 1f, 1f, 1f);
                try { mat.SetColor("_Color", new Color(1f, 1f, 1f, 1f)); } catch { }
                try { mat.SetColor("_EmissionColor", new Color(1f, 1f, 1f, 1f)); } catch { }
                try { mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always); } catch { }
                try { mat.SetInt("_ZWrite", 0); } catch { }
                mat.renderQueue = 5000;
                rend.material = mat;
                _p2AnchorDotMaterial = mat;
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                rend.receiveShadows = false;
            }

            _p2AnchorDot.SetActive(false);
            _logger.Msg("[P2WebManager] Created anchor dot sphere (fallback).");
        }

        private bool _webLineMatCached;

        private void CreateWebIndicatorLine(GameObject p2Spider)
        {
            var lineGo = new GameObject("P2_WebIndicatorLine");
            lineGo.transform.SetParent(p2Spider.transform, false);
            _webIndicatorLine = lineGo.AddComponent<LineRenderer>();
            _webIndicatorLine.useWorldSpace = true;
            _webIndicatorLine.positionCount = 2;
            _webIndicatorLine.enabled = false;
            CopyP1WebIndicatorLineSettings();
        }

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
                _grappleLineMaterial = new Material(Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard"));
                _grappleLineMaterial.color = new Color(0.9f, 0.9f, 1f, 0.8f);
                _grappleLine.material = _grappleLineMaterial;
            }
            _grappleLine.enabled = false;
        }

        private LineRenderer GetP1WebIndicatorLine()
        {
            if (_fWebIndicationLineRenderer == null || _p1WebController == null)
                return null;

            try
            {
                return _fWebIndicationLineRenderer.GetValue(_p1WebController) as LineRenderer;
            }
            catch
            {
                return null;
            }
        }

        private void CopyP1WebIndicatorLineSettings()
        {
            if (_webIndicatorLine == null)
                return;

            var source = GetP1WebIndicatorLine();
            if (source == null)
                return;

            _webIndicatorLine.sharedMaterial = source.sharedMaterial;
            _webIndicatorLine.startColor = source.startColor;
            _webIndicatorLine.endColor = source.endColor;
            _webIndicatorLine.widthMultiplier = source.widthMultiplier;
            _webIndicatorLine.widthCurve = source.widthCurve;
            _webIndicatorLine.startWidth = source.startWidth;
            _webIndicatorLine.endWidth = source.endWidth;
            _webIndicatorLine.colorGradient = source.colorGradient;
            _webIndicatorLine.textureMode = source.textureMode;
            _webIndicatorLine.alignment = source.alignment;
            _webIndicatorLine.numCapVertices = source.numCapVertices;
            _webIndicatorLine.numCornerVertices = source.numCornerVertices;
            _webIndicatorLine.shadowCastingMode = source.shadowCastingMode;
            _webIndicatorLine.receiveShadows = source.receiveShadows;
            _webIndicatorLine.generateLightingData = source.generateLightingData;
            _webIndicatorLine.motionVectorGenerationMode = source.motionVectorGenerationMode;
            _webIndicatorLine.lightProbeUsage = source.lightProbeUsage;
            _webIndicatorLine.reflectionProbeUsage = source.reflectionProbeUsage;
            _webIndicatorLine.sortingLayerID = source.sortingLayerID;
            _webIndicatorLine.sortingOrder = source.sortingOrder;
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
                    DestroyOwnedMaterial(ref _grappleLineMaterial);
                    _grappleLineMaterial = new Material(bestLR.material);
                    _grappleLine.material = _grappleLineMaterial;
                    _grappleLine.startWidth = bestLR.startWidth;
                    _grappleLine.endWidth = bestLR.endWidth;
                    _grappleLine.widthMultiplier = bestLR.widthMultiplier;
                    _grappleLine.colorGradient = bestLR.colorGradient;
                    _grappleLine.textureMode = bestLR.textureMode;
                    _grappleLine.numCapVertices = bestLR.numCapVertices;
                    _grappleLine.numCornerVertices = bestLR.numCornerVertices;

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
                _mQuickBuild = _wcType.GetMethod("MobileQuickBuild", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                _mFixedAnchor = _wcType.GetMethod("MobileFixedAnchor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                _mMovingAnchor = _wcType.GetMethod("MobileMovingAnchor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                _mDeleteWebReleased = _wcType.GetMethod("MobileDeleteWebButtonReleased", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                _mDestroyAllPlayerWebs = _wcType.GetMethod("DestroyAllPlayerWebs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

                // Field reflection caches for the state-capsule swap.
                _fBodyMovement = TryGetField("bodyMovement");
                _fWebStartPoint = TryGetField("webStartPoint");
                _fWebMode = TryGetField("webMode");
                _fWebBuildingMode = TryGetField("webBuildingMode");
                _fWebActive = TryGetField("webActive");
                _fWebTargetActive = TryGetField("webTargetActive");
                _fWebAnchorActive = TryGetField("webAnchorActive");
                _fSpringJoint = TryGetField("springJoint");
                _fWebTarget = TryGetField("webTarget");
                _fWebAnchor = TryGetField("webAnchor");
                _fWebTargetObject = TryGetField("webTargetObject");
                _fOldWebTargetObject = TryGetField("oldWebTargetObject");
                _fOldWebTargetObject1 = TryGetField("oldWebTargetObject1");
                _fWebAnchorObject = TryGetField("webAnchorObject");
                _fPlayerWebJoint = TryGetField("playerWebJoint");
                _fDeleteWebPressed = TryGetField("deleteWebPressed");
                _fDeletePlayerWebsTimer = TryGetField("deletePlayerWebsTimer");
                _fWebTargetPrefab = TryGetField("webTargetPrefab");
                _fWebAnchorPrefab = TryGetField("webAnchorPrefab");
                _fWebTargetGfx = TryGetField("webTargetGfx");
                _fWebAnchorGfx = TryGetField("webAnchorGfx");
                _fWebIndicationLineRenderer = TryGetField("webIndicationLineRenderer");
                _fWebTargetSize = TryGetField("webTargetSize");
                _fWebDistance = TryGetField("webDistance");
                _fWebTargetDefaultMaterial = TryGetField("webTargetDefaultMaterial");
                _fWebAnchorFixedAnchorMaterial = TryGetField("webAnchorFixedAnchorMaterial");
                _fWebTargetFixedAnchorMaterial = TryGetField("webTargetFixedAnchorMaterial");
                _fWebAnchorMovingAnchorMaterial = TryGetField("webAnchorMovingAnchorMaterial");
                _fWebTargetMovingAnchorMaterial = TryGetField("webTargetMovingAnchorMaterial");
                _fBuildThreadSound = TryGetField("buildThreadSound");
                _fAttachAnchorSound = TryGetField("attachAnchorSound");
                _fDeleteThreadSound = TryGetField("deleteThreadSound");
                _fCantBuildSound = TryGetField("cantBuildSound");
                _fAttachToPlayerSound = TryGetField("attachToPlayerSound");
                _fMusicThreadSound = TryGetField("musicThreadSound");
                _fWebSound = TryGetField("webSound");
                try
                {
                    if (_fWebTargetSize != null) _webTargetSizeCurve = _fWebTargetSize.GetValue(_p1WebController) as AnimationCurve;
                    if (_fWebDistance != null)
                    {
                        var v = _fWebDistance.GetValue(_p1WebController);
                        if (v is float f && f > 0f) _webDistanceVal = f;
                    }
                }
                catch { }

                // CameraController.mainCamera field + singleton instance for direct field swap.
                try
                {
                    var ccType = AccessTools.TypeByName("_Scripts.Singletons.CameraController");
                    if (ccType != null)
                    {
                        _fCcMainCamera = AccessTools.Field(ccType, "mainCamera");
                        // Singleton<CameraController>.Instance — resolve via the generic base class.
                        var singletonType = ccType.BaseType; // Singleton<CameraController>
                        var instProp = singletonType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                            ?? singletonType?.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static);
                        if (instProp != null) _ccInstance = instProp.GetValue(null, null);
                        else
                        {
                            var instField = singletonType?.GetField("Instance", BindingFlags.Public | BindingFlags.Static)
                                ?? singletonType?.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static);
                            if (instField != null) _ccInstance = instField.GetValue(null);
                        }
                        if (_logger != null)
                            _logger.Msg("[P2WebManager] CameraController.mainCamera field cached: field=" + (_fCcMainCamera != null) + " inst=" + (_ccInstance != null));
                    }
                }
                catch (Exception ex)
                {
                    if (_logger != null) _logger.Warning("[P2WebManager] CameraController.mainCamera cache failed: " + ex.Message);
                }

                if (_logger != null)
                    _logger.Msg("[P2WebManager] CacheWebControllerMethods: shoot=" + (_mShootWeb != null)
                        + " delete=" + (_mDeleteWeb != null)
                        + " check=" + (_mCheckForWebTarget != null)
                        + " quickBuild=" + (_mQuickBuild != null)
                        + " fixedAnchor=" + (_mFixedAnchor != null)
                        + " movingAnchor=" + (_mMovingAnchor != null)
                        + " deleteReleased=" + (_mDeleteWebReleased != null)
                        + " | fields: bm=" + (_fBodyMovement != null)
                        + " wsp=" + (_fWebStartPoint != null)
                        + " wm=" + (_fWebMode != null)
                        + " wbm=" + (_fWebBuildingMode != null)
                        + " wa=" + (_fWebActive != null)
                        + " sj=" + (_fSpringJoint != null)
                        + " wt=" + (_fWebTarget != null)
                        + " wan=" + (_fWebAnchor != null)
                        + " pwj=" + (_fPlayerWebJoint != null)
                        + " wtp=" + (_fWebTargetPrefab != null)
                        + " wap=" + (_fWebAnchorPrefab != null));
            }
            catch (Exception ex)
            {
                if (_logger != null)
                    _logger.Warning("[P2WebManager] CacheWebControllerMethods failed: " + ex);
            }
        }

        private FieldInfo TryGetField(string name)
        {
            try { return _wcType.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); }
            catch { return null; }
        }

        private T GetField<T>(FieldInfo f) where T : class
        {
            if (f == null || _p1WebController == null) return null;
            try { return f.GetValue(_p1WebController) as T; } catch { return null; }
        }

        private object GetFieldRaw(FieldInfo f)
        {
            if (f == null || _p1WebController == null) return null;
            try { return f.GetValue(_p1WebController); } catch { return null; }
        }

        private void SetFieldRaw(FieldInfo f, object value)
        {
            if (f == null || _p1WebController == null) return;
            try { f.SetValue(_p1WebController, value); } catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] SetField " + f.Name + " failed: " + ex.Message); }
        }

        private static bool IsArachnophobiaModeEnabled()
        {
            try
            {
                if (_settingsArachnophobiaModeProp == null)
                {
                    var settingsType = AccessTools.TypeByName("_Scripts.Singletons.SettingsController");
                    if (settingsType != null)
                        _settingsArachnophobiaModeProp = settingsType.GetProperty("ArachnophobiaMode", BindingFlags.Static | BindingFlags.Public);
                }

                if (_settingsArachnophobiaModeProp == null)
                    return false;

                object value = _settingsArachnophobiaModeProp.GetValue(null, null);
                return value is bool enabled && enabled;
            }
            catch
            {
                return false;
            }
        }

        private Transform ResolveP2WebStartPoint()
        {
            Transform start = null;

            if (_p2BodyMovement != null)
            {
                if (_p2BodyMovementBallProp == null)
                {
                    try
                    {
                        _p2BodyMovementBallProp = _p2BodyMovement.GetType().GetProperty("Ball", BindingFlags.Instance | BindingFlags.Public);
                    }
                    catch { }
                }

                if (_p2BodyMovementBallProp != null)
                {
                    try { start = _p2BodyMovementBallProp.GetValue(_p2BodyMovement, null) as Transform; }
                    catch { }
                }
            }

            if (!IsArachnophobiaModeEnabled())
                start = _p2Root != null ? _p2Root : start;

            if (start == null)
                start = _p2Root;

            if (start == null)
                start = _p2InputTransform;

            if (start == null && _p2Rigidbody != null)
                start = _p2Rigidbody.transform;

            return start;
        }

        private void RefreshP2WebStartPointReference()
        {
            _p2WebStartPoint = ResolveP2WebStartPoint();
            if (_p2Capsule != null)
                _p2Capsule.webStartPoint = _p2WebStartPoint;
        }

        private void SaveLive(WcCapsule c)
        {
            if (c == null || _p1WebController == null) return;
            try
            {
                if (_fBodyMovement != null) c.bodyMovement = _fBodyMovement.GetValue(_p1WebController);
                if (_fWebStartPoint != null) c.webStartPoint = _fWebStartPoint.GetValue(_p1WebController);
                if (_fWebMode != null) { var v = _fWebMode.GetValue(_p1WebController); c.webMode = v != null ? Convert.ToInt32(v) : 0; }
                if (_fWebBuildingMode != null) { var v = _fWebBuildingMode.GetValue(_p1WebController); c.webBuildingMode = v != null ? Convert.ToInt32(v) : 0; }
                if (_fWebActive != null) c.webActive = (bool)_fWebActive.GetValue(_p1WebController);
                if (_fWebTargetActive != null) c.webTargetActive = (bool)_fWebTargetActive.GetValue(_p1WebController);
                if (_fWebAnchorActive != null) c.webAnchorActive = (bool)_fWebAnchorActive.GetValue(_p1WebController);
                if (_fSpringJoint != null) c.springJoint = _fSpringJoint.GetValue(_p1WebController);
                if (_fWebTarget != null) c.webTarget = _fWebTarget.GetValue(_p1WebController);
                if (_fWebAnchor != null) c.webAnchor = _fWebAnchor.GetValue(_p1WebController);
                if (_fWebTargetObject != null) c.webTargetObject = _fWebTargetObject.GetValue(_p1WebController);
                if (_fOldWebTargetObject != null) c.oldWebTargetObject = _fOldWebTargetObject.GetValue(_p1WebController);
                if (_fOldWebTargetObject1 != null) c.oldWebTargetObject1 = _fOldWebTargetObject1.GetValue(_p1WebController);
                if (_fWebAnchorObject != null) c.webAnchorObject = _fWebAnchorObject.GetValue(_p1WebController);
                if (_fPlayerWebJoint != null) c.playerWebJoint = _fPlayerWebJoint.GetValue(_p1WebController);
                if (_fDeleteWebPressed != null) c.deleteWebPressed = (bool)_fDeleteWebPressed.GetValue(_p1WebController);
                if (_fDeletePlayerWebsTimer != null) c.deletePlayerWebsTimer = (float)_fDeletePlayerWebsTimer.GetValue(_p1WebController);
                if (_fBuildThreadSound != null) c.buildThreadSound = _fBuildThreadSound.GetValue(_p1WebController);
                if (_fAttachAnchorSound != null) c.attachAnchorSound = _fAttachAnchorSound.GetValue(_p1WebController);
                if (_fDeleteThreadSound != null) c.deleteThreadSound = _fDeleteThreadSound.GetValue(_p1WebController);
                if (_fCantBuildSound != null) c.cantBuildSound = _fCantBuildSound.GetValue(_p1WebController);
                if (_fAttachToPlayerSound != null) c.attachToPlayerSound = _fAttachToPlayerSound.GetValue(_p1WebController);
                if (_fMusicThreadSound != null) c.musicThreadSound = _fMusicThreadSound.GetValue(_p1WebController);
                if (_fWebSound != null) c.webSound = _fWebSound.GetValue(_p1WebController) as string;
            }
            catch (Exception ex)
            {
                if (_logger != null) _logger.Warning("[P2WebManager] SaveLive failed: " + ex);
            }
        }

        private void LoadLive(WcCapsule c)
        {
            if (c == null || _p1WebController == null) return;
            try
            {
                if (_fBodyMovement != null) _fBodyMovement.SetValue(_p1WebController, c.bodyMovement);
                if (_fWebStartPoint != null) _fWebStartPoint.SetValue(_p1WebController, c.webStartPoint);
                if (_fWebMode != null) _fWebMode.SetValue(_p1WebController, Enum.ToObject(_fWebMode.FieldType, c.webMode));
                if (_fWebBuildingMode != null) _fWebBuildingMode.SetValue(_p1WebController, Enum.ToObject(_fWebBuildingMode.FieldType, c.webBuildingMode));
                if (_fWebActive != null) _fWebActive.SetValue(_p1WebController, c.webActive);
                if (_fWebTargetActive != null) _fWebTargetActive.SetValue(_p1WebController, c.webTargetActive);
                if (_fWebAnchorActive != null) _fWebAnchorActive.SetValue(_p1WebController, c.webAnchorActive);
                if (_fSpringJoint != null) _fSpringJoint.SetValue(_p1WebController, c.springJoint);
                if (_fWebTarget != null) _fWebTarget.SetValue(_p1WebController, c.webTarget);
                if (_fWebAnchor != null) _fWebAnchor.SetValue(_p1WebController, c.webAnchor);
                if (_fWebTargetObject != null) _fWebTargetObject.SetValue(_p1WebController, c.webTargetObject);
                if (_fOldWebTargetObject != null) _fOldWebTargetObject.SetValue(_p1WebController, c.oldWebTargetObject);
                if (_fOldWebTargetObject1 != null) _fOldWebTargetObject1.SetValue(_p1WebController, c.oldWebTargetObject1);
                if (_fWebAnchorObject != null) _fWebAnchorObject.SetValue(_p1WebController, c.webAnchorObject);
                if (_fPlayerWebJoint != null) _fPlayerWebJoint.SetValue(_p1WebController, c.playerWebJoint);
                if (_fDeleteWebPressed != null) _fDeleteWebPressed.SetValue(_p1WebController, c.deleteWebPressed);
                if (_fDeletePlayerWebsTimer != null) _fDeletePlayerWebsTimer.SetValue(_p1WebController, c.deletePlayerWebsTimer);
                if (_fBuildThreadSound != null) _fBuildThreadSound.SetValue(_p1WebController, c.buildThreadSound);
                if (_fAttachAnchorSound != null) _fAttachAnchorSound.SetValue(_p1WebController, c.attachAnchorSound);
                if (_fDeleteThreadSound != null) _fDeleteThreadSound.SetValue(_p1WebController, c.deleteThreadSound);
                if (_fCantBuildSound != null) _fCantBuildSound.SetValue(_p1WebController, c.cantBuildSound);
                if (_fAttachToPlayerSound != null) _fAttachToPlayerSound.SetValue(_p1WebController, c.attachToPlayerSound);
                if (_fMusicThreadSound != null) _fMusicThreadSound.SetValue(_p1WebController, c.musicThreadSound);
                if (_fWebSound != null) _fWebSound.SetValue(_p1WebController, c.webSound);
            }
            catch (Exception ex)
            {
                if (_logger != null) _logger.Warning("[P2WebManager] LoadLive failed: " + ex);
            }
        }

        private void PublishP2WebState()
        {
            SplitScreenMod.P2WebActive = _p2Capsule != null && _p2Capsule.webActive;
            SplitScreenMod.P2WebTargetActive = _p2Capsule != null && _p2Capsule.webTargetActive;
        }

        private void InvokeAsP2(Action invoke, bool setShootHeld = false, bool refreshTarget = true)
        {
            if (_p2Capsule == null || _p1WebController == null) return;

            var savedP1 = new WcCapsule();
            SaveLive(savedP1);
            // Snapshot the shared webTargetGfx active state. P1 and P2 share the
            // same Transform/GameObject — when P2's CheckForWebTarget finds no
            // target in range, the game calls webTargetGfx.gameObject.SetActive(false),
            // which would hide P1's dot until P1 ran its own pass. Capture
            // P1's gfx visibility here and restore it in the finally block.
            GameObject p1GfxGo = null;
            bool p1GfxActive = false;
            Renderer p1GfxRenderer = null;
            Material p1GfxMaterial = null;
            GameObject p1AnchorGfxGo = null;
            bool p1AnchorGfxActive = false;
            Renderer p1AnchorGfxRenderer = null;
            Material p1AnchorGfxMaterial = null;
            try
            {
                if (_fWebTargetGfx != null)
                {
                    var gfxTr = _fWebTargetGfx.GetValue(_p1WebController) as Transform;
                    if (gfxTr != null)
                    {
                        p1GfxGo = gfxTr.gameObject;
                        p1GfxActive = p1GfxGo.activeSelf;
                        p1GfxRenderer = gfxTr.GetComponentInChildren<Renderer>(true);
                        if (p1GfxRenderer != null) p1GfxMaterial = p1GfxRenderer.sharedMaterial;
                    }
                }
                if (_fWebAnchorGfx != null)
                {
                    var anchTr = _fWebAnchorGfx.GetValue(_p1WebController) as Transform;
                    if (anchTr != null)
                    {
                        p1AnchorGfxGo = anchTr.gameObject;
                        p1AnchorGfxActive = p1AnchorGfxGo.activeSelf;
                        p1AnchorGfxRenderer = anchTr.GetComponentInChildren<Renderer>(true);
                        if (p1AnchorGfxRenderer != null) p1AnchorGfxMaterial = p1AnchorGfxRenderer.sharedMaterial;
                    }
                }
            }
            catch { }
            RefreshP2WebStartPointReference();
            LoadLive(_p2Capsule);

            // Mirror BodyMovement / WebStartPoint into the live state for P2 if available
            // (LoadLive already copied them from the capsule, which was set up at init time).

            // Swap CameraController.mainCamera field directly so that game code reading
            // the field (bypassing the patched property) sees the P2 camera.
            UnityEngine.Camera prevMainCam = null;
            bool swappedCam = false;
            if (_fCcMainCamera != null && _ccInstance != null && SplitScreenMod.P2Camera != null)
            {
                try
                {
                    prevMainCam = _fCcMainCamera.GetValue(_ccInstance) as UnityEngine.Camera;
                    _fCcMainCamera.SetValue(_ccInstance, SplitScreenMod.P2Camera);
                    swappedCam = true;
                }
                catch (Exception ex)
                {
                    if (_logger != null) _logger.Warning("[P2WebManager] mainCamera swap failed: " + ex.Message);
                }
            }

            SplitScreenMod.InP2WebContext = true;
            if (setShootHeld) SplitScreenMod.P2ShootHeld = true;

            try
            {
                if (refreshTarget && _mCheckForWebTarget != null)
                {
                    try { _mCheckForWebTarget.Invoke(_p1WebController, new object[] { 1f }); }
                    catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] CheckForWebTarget(P2) failed: " + ex.Message); }
                }

                if (invoke != null)
                {
                    try { invoke(); }
                    catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] InvokeAsP2 inner failed: " + ex.Message); }
                }
            }
            finally
            {
                // Capture P2's mutated state for next time.
                SaveLive(_p2Capsule);
                PublishP2WebState();
                // Restore P1.
                LoadLive(savedP1);
                // Restore the shared web target/anchor gfx visibility to what
                // P1 had before the swap. P2 maintains its own _p2TargetDot,
                // so toggling these only affects P1's reticle.
                try
                {
                    if (p1GfxGo != null && p1GfxGo.activeSelf != p1GfxActive)
                        p1GfxGo.SetActive(p1GfxActive);
                    if (p1AnchorGfxGo != null && p1AnchorGfxGo.activeSelf != p1AnchorGfxActive)
                        p1AnchorGfxGo.SetActive(p1AnchorGfxActive);
                    if (p1GfxRenderer != null && p1GfxRenderer.sharedMaterial != p1GfxMaterial)
                        p1GfxRenderer.sharedMaterial = p1GfxMaterial;
                    if (p1AnchorGfxRenderer != null && p1AnchorGfxRenderer.sharedMaterial != p1AnchorGfxMaterial)
                        p1AnchorGfxRenderer.sharedMaterial = p1AnchorGfxMaterial;
                }
                catch { }
                SplitScreenMod.InP2WebContext = false;
                SplitScreenMod.P2ShootHeld = false;
                // Restore CameraController.mainCamera field.
                if (swappedCam && _fCcMainCamera != null && _ccInstance != null)
                {
                    try { _fCcMainCamera.SetValue(_ccInstance, prevMainCam); }
                    catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] mainCamera restore failed: " + ex.Message); }
                }
            }
        }

        private static void DisableChildRenderers(Transform root)
        {
            if (root == null) return;
            try
            {
                var rends = root.GetComponentsInChildren<UnityEngine.Renderer>(true);
                if (rends != null)
                    foreach (var r in rends) { try { if (r != null) r.enabled = false; } catch { } }
            }
            catch { }
        }

        // Build P2's per-player WebController state and seed a capsule so InvokeAsP2 can swap it in.
        private void SetupP2WebState(GameObject p2Spider)
        {
            if (p2Spider == null || _p1WebController == null || _wcType == null) return;

            var asm = _wcType.Assembly;
            Type bodyMovementType = null;
            try { bodyMovementType = asm.GetType("_Scripts.Spider.BodyMovement", false); } catch { }
            if (bodyMovementType == null)
            {
                try { bodyMovementType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement"); } catch { }
            }

            // Resolve P2's BodyMovement
            if (bodyMovementType != null)
            {
                try
                {
                    var comps = p2Spider.GetComponentsInChildren(bodyMovementType, true);
                    if (comps != null && comps.Length > 0)
                        _p2BodyMovement = comps[0] as Component;
                }
                catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] Locate P2 BodyMovement failed: " + ex.Message); }
            }

            if (_p2BodyMovement != null)
            {
                try
                {
                    var rootProp = bodyMovementType.GetProperty("Root", BindingFlags.Public | BindingFlags.Instance);
                    _p2BodyMovementBallProp = bodyMovementType.GetProperty("Ball", BindingFlags.Public | BindingFlags.Instance);
                    if (rootProp != null) _p2Root = rootProp.GetValue(_p2BodyMovement, null) as Transform;
                    if (_p2Root == null)
                    {
                        var rootField = bodyMovementType.GetField("root", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (rootField != null) _p2Root = rootField.GetValue(_p2BodyMovement) as Transform;
                    }
                }
                catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] Read P2 Root failed: " + ex.Message); }
            }

            // Instantiate per-player webTarget / webAnchor (they host SpringJoint at attach time).
            try
            {
                var wcTr = _p1WebController is Component c ? c.transform : null;
                var targetPrefab = GetFieldRaw(_fWebTargetPrefab) as Transform;
                if (targetPrefab != null)
                {
                    _p2WebTargetTr = UnityEngine.Object.Instantiate(targetPrefab, wcTr);
                    _p2WebTargetTr.name = "P2_WebTarget";
                    // Must remain active so SpringJoint physics simulate when AttachWeb
                    // adds a SpringJoint to this GameObject.
                    _p2WebTargetTr.gameObject.SetActive(true);
                    // Hide the prefab's visible target dot (we render our own _targetDot);
                    // the engine re-enables this GameObject only when its capsule's
                    // webTargetActive flag is set, which only happens while P2 is aiming.
                    DisableChildRenderers(_p2WebTargetTr);
                    _p2HasWebTarget = true;
                }
                else if (_logger != null) _logger.Warning("[P2WebManager] webTargetPrefab not found — P2 grapple disabled.");

                var anchorPrefab = GetFieldRaw(_fWebAnchorPrefab) as Transform;
                if (anchorPrefab != null)
                {
                    _p2WebAnchorTr = UnityEngine.Object.Instantiate(anchorPrefab, wcTr);
                    _p2WebAnchorTr.name = "P2_WebAnchor";
                    _p2WebAnchorTr.gameObject.SetActive(true);
                    DisableChildRenderers(_p2WebAnchorTr);
                    _p2HasWebAnchor = true;
                }
                else if (_logger != null) _logger.Warning("[P2WebManager] webAnchorPrefab not found — P2 build disabled.");
            }
            catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] Instantiate P2 webTarget/Anchor failed: " + ex.Message); }

            // Create P2's playerWebJoint component on its body.
            try
            {
                Type webJointType = null;
                try { webJointType = asm.GetType("_Scripts.Web.WebJoint", false); } catch { }
                if (webJointType == null)
                {
                    try { webJointType = AccessTools.TypeByName("_Scripts.Web.WebJoint"); } catch { }
                }

                if (webJointType != null && _p2BodyMovement != null)
                {
                    _p2PlayerWebJoint = _p2BodyMovement.gameObject.AddComponent(webJointType) as Component;
                    if (_p2PlayerWebJoint != null)
                    {
                        var setupM = webJointType.GetMethod("SetupPlayerWebJoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                        var setAnchorM = webJointType.GetMethod("SetAnchor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(Transform) }, null);
                        try { if (setupM != null) setupM.Invoke(_p2PlayerWebJoint, null); }
                        catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] SetupPlayerWebJoint failed: " + ex.Message); }
                        try { if (setAnchorM != null && _p2Root != null) setAnchorM.Invoke(_p2PlayerWebJoint, new object[] { _p2Root }); }
                        catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] SetAnchor failed: " + ex.Message); }
                        _p2HasWebJoint = true;
                    }
                }
                else if (_logger != null)
                {
                    _logger.Warning("[P2WebManager] WebJoint type or P2 BodyMovement missing — P2 won't have a player web joint.");
                }
            }
            catch (Exception ex) { if (_logger != null) _logger.Warning("[P2WebManager] Create P2 playerWebJoint failed: " + ex.Message); }

            // Seed P2's capsule from current live state, then overwrite per-player parts.
            try
            {
                _p2Capsule = new WcCapsule();
                SaveLive(_p2Capsule);

                if (_p2BodyMovement != null) _p2Capsule.bodyMovement = _p2BodyMovement;
                if (_p2WebTargetTr != null) _p2Capsule.webTarget = _p2WebTargetTr;
                if (_p2WebAnchorTr != null) _p2Capsule.webAnchor = _p2WebAnchorTr;
                if (_p2PlayerWebJoint != null) _p2Capsule.playerWebJoint = _p2PlayerWebJoint;
                CacheP2WebSounds();

                // WebController uses BodyMovement.Root and switches to BodyMovement.Ball in
                // arachnophobia mode. Mirror that exact source transform for P2 so the
                // grapple originates from inside the spider instead of below it.
                RefreshP2WebStartPointReference();
                if (_p2WebStartPoint == null && _logger != null)
                    _logger.Warning("[P2WebManager] Couldn't resolve a P2 web start transform; falling back to InputTransform/camera math.");

                // P2 starts with no active web/joint.
                _p2Capsule.webActive = false;
                _p2Capsule.webTargetActive = false;
                _p2Capsule.webAnchorActive = false;
                _p2Capsule.springJoint = null;
                _p2Capsule.webMode = 0;          // WebMode.Default
                _p2Capsule.webBuildingMode = 0;  // WebBuildingMode.MovingAnchor
                _p2Capsule.deleteWebPressed = false;
                _p2Capsule.deletePlayerWebsTimer = 0f;
                _p2Capsule.webTargetObject = null;
                _p2Capsule.oldWebTargetObject = null;
                _p2Capsule.oldWebTargetObject1 = null;
                _p2Capsule.webAnchorObject = null;
                PublishP2WebState();

                if (_logger != null)
                    _logger.Msg("[P2WebManager] P2 web state ready. bm=" + (_p2BodyMovement != null)
                        + " root=" + (_p2Root != null)
                        + " wt=" + _p2HasWebTarget
                        + " wa=" + _p2HasWebAnchor
                        + " wj=" + _p2HasWebJoint);
            }
            catch (Exception ex)
            {
                if (_logger != null) _logger.Warning("[P2WebManager] Seed P2 capsule failed: " + ex);
            }
        }

        public void DriveInput()
        {
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
                // --- Read P2 inputs (gamepad-only; keyboard fallbacks intentionally disabled) ---
                bool rtHeld = InputCompat.IsP2ShootRTHeldNow(
                    SplitScreenMod.P2UseGamepad,
                    SplitScreenMod.P2GamepadIndex,
                    SplitScreenMod.P2TriggerThreshold,
                    null, KeyCode.None);
                bool rtDown = rtHeld && !_shootHeldPrev;
                bool rtUp   = !rtHeld && _shootHeldPrev;
                _shootHeldPrev = rtHeld;

                bool ltHeld = InputCompat.IsP2QuickBuildHeldNow(
                    SplitScreenMod.P2UseGamepad,
                    SplitScreenMod.P2GamepadIndex,
                    SplitScreenMod.P2TriggerThreshold,
                    null, KeyCode.None);
                bool ltDown = ltHeld && !_ltPrev;
                _ltPrev = ltHeld;

                bool rbHeld = InputCompat.IsP2FixedAnchorHeldNow(
                    SplitScreenMod.P2UseGamepad, SplitScreenMod.P2GamepadIndex, null, KeyCode.None);
                bool rbDown = rbHeld && !_rbPrev;
                _rbPrev = rbHeld;

                bool lbHeld = InputCompat.IsP2MovingAnchorHeldNow(
                    SplitScreenMod.P2UseGamepad, SplitScreenMod.P2GamepadIndex, null, KeyCode.None);
                bool lbDown = lbHeld && !_lbPrev;
                _lbPrev = lbHeld;

                bool bHeld = InputCompat.IsP2DeleteHeldNow(
                    SplitScreenMod.P2UseGamepad, SplitScreenMod.P2GamepadIndex, null, KeyCode.None);
                bool bDown = bHeld && !_bPrev;
                bool bUp   = !bHeld && _bPrev;
                _bPrev = bHeld;

                // --- Refresh P2's web target each frame so the dot snaps to webbable surfaces
                // exactly like P1's reticle. This swaps state for one CheckForWebTarget call.
                if (_mCheckForWebTarget != null)
                {
                    InvokeAsP2(null, refreshTarget: true);
                }

                // --- Target dot (always visible, like P1's) ---
                UpdateTargetDot(true);
                UpdateAnchorDot(true);
                UpdateWebIndicatorLine();

                // --- Hold-to-delete-all-webs simulation (P2's capsule timer can't tick
                // between input events because P2 state is only loaded transiently;
                // simulate it here and fire DestroyAllPlayerWebs when the hold completes). ---
                TickP2DeleteHold(bHeld, bDown, bUp);

                // --- Drive native WebController via state-capsule swap ---
                if (rtDown && _mShootWeb != null)
                {
                    bool hadWeb = _p2Capsule != null && _p2Capsule.webActive;
                    InvokeAsP2(() => _mShootWeb.Invoke(_p1WebController, new object[] { true }), setShootHeld: true);
                    if (!hadWeb && _p2Capsule != null && _p2Capsule.webActive)
                    {
                        // The WebController call plays this too, but invoke it directly
                        // on P2's relocated emitter as a reliable split-screen fallback.
                        PlayP2GrappleSound();
                    }
                }
                if (rtUp && _mShootWeb != null)
                {
                    bool guarded = _p2Capsule != null && _p2Capsule.webActive;
                    if (guarded)
                        InvokeAsP2(() => _mShootWeb.Invoke(_p1WebController, new object[] { false }), setShootHeld: false, refreshTarget: false);
                }

                if (ltDown && _mQuickBuild != null)
                {
                    InvokeAsP2(() => _mQuickBuild.Invoke(_p1WebController, null));
                }
                if (rbDown && _mFixedAnchor != null)
                {
                    InvokeAsP2(() => _mFixedAnchor.Invoke(_p1WebController, null), refreshTarget: false);
                }
                if (lbDown && _mMovingAnchor != null)
                {
                    InvokeAsP2(() => _mMovingAnchor.Invoke(_p1WebController, null), refreshTarget: false);
                }
                if (bDown && _mDeleteWeb != null)
                {
                    // refreshTarget: true so MobileDeleteWeb's CheckForWebThreadToDestroy
                    // sees the web P2 is currently aiming at (previous behavior used a stale
                    // capsule and frequently missed the target).
                    InvokeAsP2(() => _mDeleteWeb.Invoke(_p1WebController, null), refreshTarget: true);
                }
                if (bUp && _mDeleteWebReleased != null)
                {
                    InvokeAsP2(() => _mDeleteWebReleased.Invoke(_p1WebController, null), refreshTarget: false);
                }

                // Update grapple line visual (driven by P2's capsule state)
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

        private void TickP2DeleteHold(bool bHeld, bool bDown, bool bUp)
        {
            if (_mDestroyAllPlayerWebs == null) return;

            if (bDown)
            {
                _p2DeleteHoldTimer = 0f;
                _p2DeleteAllFired = false;
            }
            else if (bHeld && !_p2DeleteAllFired)
            {
                _p2DeleteHoldTimer += Time.deltaTime;
                if (_p2DeleteHoldTimer >= P2_DELETE_HOLD_DURATION)
                {
                    _p2DeleteAllFired = true;
                    InvokeAsP2(() => _mDestroyAllPlayerWebs.Invoke(_p1WebController, null), refreshTarget: false);
                }
            }
            else if (bUp || !bHeld)
            {
                _p2DeleteHoldTimer = 0f;
            }
        }

        private void FixedUpdate()
        {
            TickSpringJointAnchor();
        }

        // Mirror of WebController.UpdateSpringJointAnchor for P2.
        //
        // When the spider webs a movable object while standing on ANOTHER movable body
        // (e.g. a sliding-puzzle tile while standing on the puzzle board), the game
        // connects the SpringJoint to that body rather than to the spider, and then
        // rewrites springJoint.connectedAnchor every physics step to the spider's
        // current position in that body's local space. That per-step rewrite is what
        // makes the webbed object follow the player.
        //
        // WebController.FixedUpdate only ever runs against the live (P1) state, so for
        // P2 the anchor kept the Vector3.zero it was given at attach time — the local
        // origin of the board — and every tile P2 grabbed was dragged toward the board's
        // pivot instead of toward P2. Tick it here from P2's own capsule.
        private void TickSpringJointAnchor()
        {
            if (!_inited || _p2Capsule == null || !_p2Capsule.webActive || _p2BodyMovement == null) return;
            if (!(_p2Capsule.springJoint is SpringJoint sj) || sj == null) return;

            try
            {
                var connected = sj.connectedBody;
                if (connected == null) return;

                // Only when the joint is connected to the body P2 is standing on;
                // a joint connected to P2's own rigidbody needs no anchor update.
                var targetRb = GetP2TargetRigidbody();
                if (targetRb == null || connected != targetRb) return;

                Vector3 anchor = connected.transform.InverseTransformPoint(_p2BodyMovement.transform.position);
                sj.connectedAnchor = UsesCenterOfMassAsWebAnchor(targetRb)
                    ? targetRb.centerOfMass + new Vector3(anchor.x, 0f, anchor.z)
                    : anchor;

                if (!ReferenceEquals(sj, _loggedAnchorTickJoint))
                {
                    _loggedAnchorTickJoint = sj;
                    if (_logger != null)
                        _logger.Msg("[P2WebManager] Driving spring anchor for P2's web on '" + sj.name
                            + "' (connected to '" + connected.name + "' that P2 is standing on).");
                }
            }
            catch { }
        }

        private Rigidbody GetP2TargetRigidbody()
        {
            if (_p2BodyMovement == null) return null;
            if (!_bmTargetRigidbodyPropCached)
            {
                _bmTargetRigidbodyPropCached = true;
                try
                {
                    _bmTargetRigidbodyProp = _p2BodyMovement.GetType().GetProperty("TargetRigidbody",
                        BindingFlags.Instance | BindingFlags.Public);
                }
                catch { }
                if (_bmTargetRigidbodyProp == null && _logger != null)
                    _logger.Warning("[P2WebManager] BodyMovement.TargetRigidbody not found — P2 can't drag webbed objects while standing on a movable body.");
            }
            if (_bmTargetRigidbodyProp == null) return null;
            try { return _bmTargetRigidbodyProp.GetValue(_p2BodyMovement, null) as Rigidbody; }
            catch { return null; }
        }

        // Memoized per rigidbody: this is polled every physics step while P2 drags, and
        // the answer is a serialized flag on the object, so it can't change under us.
        private bool UsesCenterOfMassAsWebAnchor(Rigidbody rb)
        {
            if (rb == null) return false;
            if (ReferenceEquals(rb, _comAnchorCachedRb)) return _comAnchorCachedValue;

            _comAnchorCachedRb = rb;
            _comAnchorCachedValue = false;
            try
            {
                if (!_movableObjectTypeCached)
                {
                    _movableObjectTypeCached = true;
                    try { _movableObjectType = AccessTools.TypeByName("_Scripts.Objects.MovableObject"); } catch { }
                }
                if (_movableObjectType == null) return false;

                var movable = rb.GetComponent(_movableObjectType);
                if (movable == null) return false;

                if (!ReferenceEquals(_moUseComAsWebAnchorType, movable.GetType()))
                {
                    _moUseComAsWebAnchorType = movable.GetType();
                    _moUseComAsWebAnchorProp = _moUseComAsWebAnchorType.GetProperty("UseCenterOfMassAsWebAnchor",
                        BindingFlags.Instance | BindingFlags.Public);
                }
                if (_moUseComAsWebAnchorProp == null) return false;
                _comAnchorCachedValue = (bool)_moUseComAsWebAnchorProp.GetValue(movable, null);
            }
            catch { _comAnchorCachedValue = false; }
            return _comAnchorCachedValue;
        }

        private void UpdateTargetDot(bool show)
        {
            if (_p2TargetDot == null || _p2Camera == null) return;

            if (!show)
            {
                _p2TargetDot.SetActive(false);
                return;
            }

            bool placed = false;

            // Prefer the engine-resolved web target from CheckForWebTarget (refreshed
            // each frame via InvokeAsP2). This snaps the dot to webbable surfaces /
            // WebJoints / WebThreads exactly like P1's reticle, instead of the raw
            // camera-forward raycast which never snaps.
            try
            {
                if (_p2Capsule != null && _p2Capsule.webTargetActive)
                {
                    var webTr = _p2Capsule.webTarget as Transform;
                    if (webTr != null)
                    {
                        _p2TargetDot.SetActive(true);
                        _p2TargetDot.transform.position = webTr.position;
                        placed = true;
                    }
                }
            }
            catch { /* fall through to raw raycast */ }

            if (!placed)
            {
                // Fallback: raw camera-forward raycast (when nothing webbable is in front).
                // P2's spider is on layer 2 (Ignore Raycast) so the ray passes through it.
                // The dot material has ZTest=Always so it renders on top of everything.
                var ray = new Ray(_p2Camera.transform.position, _p2Camera.transform.forward);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, _grappleMaxDist))
                {
                    _p2TargetDot.SetActive(true);
                    _p2TargetDot.transform.position = hit.point + hit.normal * _p2NormalOffset;
                    placed = true;
                }
                else
                {
                    _p2TargetDot.SetActive(false);
                }
            }

            if (placed)
            {
                ApplyIndicatorScale(_p2TargetDot);
                ApplyIndicatorMaterial(_p2TargetDot, GetP2TargetIndicatorMaterial());
            }
        }

        private void UpdateAnchorDot(bool show)
        {
            if (_p2AnchorDot == null)
                return;

            if (!show || _p2Capsule == null || !_p2Capsule.webAnchorActive)
            {
                _p2AnchorDot.SetActive(false);
                return;
            }

            var anchorTr = _p2Capsule.webAnchor as Transform ?? _p2WebAnchorTr;
            if (anchorTr == null)
            {
                _p2AnchorDot.SetActive(false);
                return;
            }

            _p2AnchorDot.SetActive(true);
            _p2AnchorDot.transform.position = anchorTr.position;
            ApplyIndicatorScale(_p2AnchorDot);
            ApplyIndicatorMaterial(_p2AnchorDot, GetP2AnchorIndicatorMaterial());
        }

        private void UpdateWebIndicatorLine()
        {
            if (_webIndicatorLine == null)
                return;

            bool show = false;
            Vector3 anchorPos = default(Vector3);
            Vector3 targetPos = default(Vector3);

            try
            {
                if (_p2Capsule != null && _p2Capsule.webAnchorActive && _p2Capsule.webTargetActive)
                {
                    var anchorTr = _p2Capsule.webAnchor as Transform ?? _p2WebAnchorTr;
                    var targetTr = _p2Capsule.webTarget as Transform ?? _p2WebTargetTr;
                    if (anchorTr != null && targetTr != null)
                    {
                        anchorPos = anchorTr.position;
                        targetPos = targetTr.position;
                        show = true;
                    }
                }
            }
            catch { }

            if (!show)
            {
                _webIndicatorLine.enabled = false;
                return;
            }

            CopyP1WebIndicatorLineSettings();
            _webIndicatorLine.enabled = true;
            _webIndicatorLine.SetPosition(0, anchorPos);
            _webIndicatorLine.SetPosition(1, targetPos);
        }

        // Mirror P1's distance-based dot scaling so the on-screen reticle size
        // stays roughly constant regardless of how far the target is. P1 uses:
        //   webTargetGfx.localScale = Vector3.one * webTargetSize.Evaluate(
        //       Vector3.Distance(webTargetGfx.position, webStartPoint.position) / webDistance);
        // (see ilspy_WebController/_Scripts.Singletons/WebController.cs:505)
        private void ApplyIndicatorScale(GameObject indicator)
        {
            if (indicator == null) return;

            // Use P2's own web start point so distance reflects P2's view.
            // Fall back to the camera position if no start point has been
            // wired up yet (e.g., before InitializeP2WebState).
            Vector3 start;
            RefreshP2WebStartPointReference();

            if (_p2WebStartPoint != null) start = _p2WebStartPoint.position;
            else if (_p2InputTransform != null) start = _p2InputTransform.position;
            else if (_p2Camera != null) start = _p2Camera.transform.position;
            else return;

            float dist = Vector3.Distance(indicator.transform.position, start);
            float t = (_webDistanceVal > 0f) ? Mathf.Clamp01(dist / _webDistanceVal) : 0f;

            float s;
            if (_webTargetSizeCurve != null)
            {
                s = _webTargetSizeCurve.Evaluate(t);
            }
            else
            {
                // Reasonable linear fallback matching the spirit of P1's curve.
                s = Mathf.Lerp(0.15f, 1.0f, t);
            }
            if (s <= 0.0001f) s = 0.0001f;
            indicator.transform.localScale = Vector3.one * s;
        }

        private Material GetP2TargetIndicatorMaterial()
        {
            if (_p2Capsule != null && _p2Capsule.webAnchorActive)
            {
                if (_p2Capsule.webBuildingMode == 1)
                    return GetFieldRaw(_fWebTargetFixedAnchorMaterial) as Material;

                return GetFieldRaw(_fWebTargetMovingAnchorMaterial) as Material;
            }

            return GetFieldRaw(_fWebTargetDefaultMaterial) as Material;
        }

        private Material GetP2AnchorIndicatorMaterial()
        {
            if (_p2Capsule != null && _p2Capsule.webBuildingMode == 1)
                return GetFieldRaw(_fWebAnchorFixedAnchorMaterial) as Material;

            return GetFieldRaw(_fWebAnchorMovingAnchorMaterial) as Material;
        }

        private static void ApplyIndicatorMaterial(GameObject indicator, Material material)
        {
            if (indicator == null || material == null)
                return;

            try
            {
                var renderers = indicator.GetComponentsInChildren<Renderer>(true);
                if (renderers == null)
                    return;

                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    if (renderer != null && renderer.sharedMaterial != material)
                        renderer.sharedMaterial = material;
                }
            }
            catch { }
        }

        private void UpdateGrappleLine()
        {
            if (_grappleLine == null) return;

            // Driven by P2's capsule state — show line only when P2 has an active web with a spring joint.
            bool active = _p2Capsule != null && _p2Capsule.webActive && _p2Capsule.springJoint != null;
            if (!active)
            {
                _grappleLine.enabled = false;
                return;
            }

            // Lazy material copy — web materials may not exist at init time
            if (!_webLineMatCached)
                TryCopyWebLineMaterial();

            _grappleLine.enabled = true;

            RefreshP2WebStartPointReference();

            // Start: mirror WebController's live webStartPoint source (Root or Ball).
            Vector3 startPos;
            if (_p2WebStartPoint != null)
                startPos = _p2WebStartPoint.position;
            else if (_p2Root != null)
                startPos = _p2Root.position;
            else if (_p2InputTransform != null)
                startPos = _p2InputTransform.position + Vector3.up * _webStartHeightOffset;
            else if (_p2Rigidbody != null)
                startPos = _p2Rigidbody.position;
            else
                startPos = transform.position;

            // End: P2's webTarget transform (set by CheckForWebTarget when the joint was attached).
            Vector3 endPos = startPos;
            try
            {
                if (_p2WebTargetTr != null)
                    endPos = _p2WebTargetTr.position;
                else if (_p2Capsule.springJoint is SpringJoint sj && sj != null)
                {
                    if (sj.connectedBody != null)
                        endPos = sj.connectedBody.transform.TransformPoint(sj.connectedAnchor);
                    else
                        endPos = sj.connectedAnchor;
                }
            }
            catch { }

            _grappleLine.SetPosition(0, startPos);
            _grappleLine.SetPosition(1, endPos);
        }

        public void Cleanup()
        {
            // Try to release any active P2 web cleanly via the game's own logic.
            try
            {
                if (_p2Capsule != null && _p2Capsule.webActive && _mShootWeb != null)
                {
                    InvokeAsP2(() => _mShootWeb.Invoke(_p1WebController, new object[] { false }), setShootHeld: false, refreshTarget: false);
                }
            }
            catch (Exception ex)
            {
                if (_logger != null) _logger.Warning("[P2WebManager] Cleanup release-web failed: " + ex.Message);
            }

            if (_p2WebTargetTr != null)
            {
                try { UnityEngine.Object.Destroy(_p2WebTargetTr.gameObject); } catch { }
                _p2WebTargetTr = null;
            }
            if (_p2WebAnchorTr != null)
            {
                try { UnityEngine.Object.Destroy(_p2WebAnchorTr.gameObject); } catch { }
                _p2WebAnchorTr = null;
            }
            if (_p2PlayerWebJoint != null)
            {
                try { UnityEngine.Object.Destroy(_p2PlayerWebJoint); } catch { }
                _p2PlayerWebJoint = null;
            }
            if (_p2WebStartPoint != null)
            {
                _p2WebStartPoint = null;
            }

            if (_p2TargetDot != null)
            {
                UnityEngine.Object.Destroy(_p2TargetDot);
                _p2TargetDot = null;
            }
            if (_p2AnchorDot != null)
            {
                UnityEngine.Object.Destroy(_p2AnchorDot);
                _p2AnchorDot = null;
            }
            if (_grappleLine != null)
            {
                UnityEngine.Object.Destroy(_grappleLine.gameObject);
                _grappleLine = null;
            }
            if (_webIndicatorLine != null)
            {
                UnityEngine.Object.Destroy(_webIndicatorLine.gameObject);
                _webIndicatorLine = null;
            }
            DestroyOwnedMaterial(ref _p2TargetDotMaterial);
            DestroyOwnedMaterial(ref _p2AnchorDotMaterial);
            DestroyOwnedMaterial(ref _grappleLineMaterial);
            _p1WebController = null;
            _p2Camera = null;
            _p2InputTransform = null;
            _p2Rigidbody = null;
            _p2BodyMovementBallProp = null;
            _comAnchorCachedRb = null;
            _loggedAnchorTickJoint = null;
            SplitScreenMod.P2WebActive = false;
            SplitScreenMod.P2WebTargetActive = false;
            _inited = false;
        }

        private void CacheP2WebSounds()
        {
            if (_p2Capsule == null || _p2WebTargetTr == null) return;

            try
            {
                Type webTargetType = AccessTools.TypeByName("_Scripts.Web.WebTarget");
                Component webTarget = webTargetType == null ? null : _p2WebTargetTr.GetComponentInChildren(webTargetType, true) as Component;
                if (webTarget == null) return;

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                _p2Capsule.buildThreadSound = webTargetType.GetField("buildThreadSound", flags)?.GetValue(webTarget);
                _p2Capsule.attachAnchorSound = webTargetType.GetField("attachAnchorSound", flags)?.GetValue(webTarget);
                _p2Capsule.deleteThreadSound = webTargetType.GetField("deleteThreadSound", flags)?.GetValue(webTarget);
                _p2Capsule.cantBuildSound = webTargetType.GetField("cantBuildSound", flags)?.GetValue(webTarget);
                _p2Capsule.attachToPlayerSound = webTargetType.GetField("attachToPlayerSound", flags)?.GetValue(webTarget);
                _p2Capsule.musicThreadSound = webTargetType.GetField("musicThreadSound", flags)?.GetValue(webTarget);
                MoveP2WebSoundEmittersToPlayer();

                if (_logger != null)
                    _logger.Msg("[P2WebManager] Cached P2 web sound emitters: build=" + (_p2Capsule.buildThreadSound != null) + ", attach=" + (_p2Capsule.attachAnchorSound != null));
            }
            catch (Exception ex)
            {
                if (_logger != null) _logger.Warning("[P2WebManager] CacheP2WebSounds failed: " + ex.Message);
            }
        }

        private void MoveP2WebSoundEmittersToPlayer()
        {
            if (_p2BodyMovement == null || _p2WebTargetTr == null) return;

            object[] emitters =
            {
                _p2Capsule.buildThreadSound,
                _p2Capsule.attachAnchorSound,
                _p2Capsule.deleteThreadSound,
                _p2Capsule.cantBuildSound,
                _p2Capsule.attachToPlayerSound,
                _p2Capsule.musicThreadSound
            };
            var moved = new HashSet<Component>();
            foreach (object emitterObject in emitters)
            {
                Component emitter = emitterObject as Component;
                if (emitter == null || !moved.Add(emitter)) continue;

                // WebController's emitters are children of the target prefab. That
                // target sits at the grapple point, which can be well beyond FMOD's
                // attenuation range. Keep the event objects with P2 instead; their
                // references remain valid when WebController calls Play().
                if (emitter.transform != _p2WebTargetTr)
                    emitter.transform.SetParent(_p2BodyMovement.transform, false);

                // FMOD can select P1's listener in split-screen. Make the P2 web
                // events audible from either listener while keeping their normal
                // event content and volume.
                Type emitterType = emitter.GetType();
                emitterType.GetField("OverrideAttenuation", BindingFlags.Instance | BindingFlags.Public)?.SetValue(emitter, true);
                emitterType.GetField("OverrideMinDistance", BindingFlags.Instance | BindingFlags.Public)?.SetValue(emitter, 0f);
                emitterType.GetField("OverrideMaxDistance", BindingFlags.Instance | BindingFlags.Public)?.SetValue(emitter, 10000f);
            }
        }

        private void PlayP2GrappleSound()
        {
            try
            {
                object emitterObject = string.Equals(_p2Capsule.webSound, "Music", StringComparison.Ordinal)
                    ? _p2Capsule.musicThreadSound
                    : _p2Capsule.buildThreadSound;
                MethodInfo playMethod = emitterObject == null ? null : emitterObject.GetType().GetMethod("Play", BindingFlags.Instance | BindingFlags.Public);
                if (playMethod != null)
                {
                    playMethod.Invoke(emitterObject, null);
                    if (_logger != null) _logger.Msg("[P2WebManager] Played P2 grapple sound fallback.");
                }
                else if (_logger != null)
                {
                    _logger.Warning("[P2WebManager] P2 grapple sound emitter has no Play() method.");
                }
            }
            catch (Exception ex)
            {
                if (_logger != null) _logger.Warning("[P2WebManager] P2 grapple sound fallback failed: " + ex.Message);
            }
        }

        private static void DestroyOwnedMaterial(ref Material material)
        {
            if (material != null)
                UnityEngine.Object.Destroy(material);
            material = null;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        // Per-player snapshot of WebController's private state. Used to swap state
        // around each P2 input event so P1 and P2 can run web actions simultaneously.
        private sealed class WcCapsule
        {
            public object bodyMovement;
            public object webStartPoint;
            public int webMode;          // WebMode enum
            public int webBuildingMode;  // WebBuildingMode enum (0=MovingAnchor, 1=FixedAnchor)
            public bool webActive;
            public bool webTargetActive;
            public bool webAnchorActive;
            public object springJoint;
            public object webTarget;
            public object webAnchor;
            public object webTargetObject;
            public object oldWebTargetObject;
            public object oldWebTargetObject1;
            public object webAnchorObject;
            public object playerWebJoint;
            public bool deleteWebPressed;
            public float deletePlayerWebsTimer;
            public object buildThreadSound;
            public object attachAnchorSound;
            public object deleteThreadSound;
            public object cantBuildSound;
            public object attachToPlayerSound;
            public object musicThreadSound;
            public string webSound;
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
        // Center transform — authored rest position of this leg.  This is the most
        // reliable left/right identity for the leg, so it is also used as the ray origin.
        private Transform _center;
        // Vanilla LegController's authored offset.  Older P2 code read this value but
        // discarded it completely; keep it as a fallback when a leg has no center.
        private Vector3 _startingOffset;
        // Spider body transform (provides forward/up for anticipatory cast)
        private Transform _bodyTransform;
        // Player roots excluded from all leg ground casts. A planted foot may be parented
        // to real moving world geometry, but never to either spider's body/leg hierarchy.
        private Transform _p1Root;
        private Transform _p2Root;
        private readonly RaycastHit[] _castBuffer = new RaycastHit[64];
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
        private bool _postInitEnableRefreshed;
        private bool _loggedCrossSideReject;
        private bool _loggedPlayerSurfaceReject;
        private int _crossedTargetRepairCount;

        private P2LegDriver[] _opposingLegs;

        // BodyMovement.State property cached for jump detection
        private PropertyInfo _bodyMovStateProp;
        private object _bodyMovJumpingVal;
        private bool _bodyMovStateCached;

        public bool IsAnimating => _lerp < 1f;

        public void SetOpposingLegs(P2LegDriver[] legs) { _opposingLegs = legs; }

        internal bool RebindAuthoredTransforms(Transform target, Transform center, Transform targetJump)
        {
            bool changed = false;
            if (target != null && !object.ReferenceEquals(_target, target))
            {
                _target = target;
                changed = true;
            }
            if (center != null && !object.ReferenceEquals(_center, center))
            {
                _center = center;
                changed = true;
            }
            if (targetJump != null && !object.ReferenceEquals(_targetJump, targetJump))
            {
                _targetJump = targetJump;
                changed = true;
            }

            // If target identity changed, immediately put the newly-canonical IK target at
            // this driver's existing foot anchor so the repair itself does not cause a step.
            if (changed && _target != null && _targetLocal != null)
            {
                _target.position = _targetLocal.position;
                _target.rotation = _targetLocal.rotation;
            }
            return changed;
        }

        // Jump pose transform (mirrors LegController.targetJump)
        private Transform _targetJump;

        public void Init(Transform target, Vector3 startingOffset, Transform center,
            Transform bodyTransform, Transform p1Root, Transform p2Root,
            Component bodyMovement, FieldInfo moveVecField,
            LayerMask whatIsGround, float sphereRadius,
            float rayUpOffset, float rayLength, float stepDist,
            float stepTime, float stepHeight, float tipHeight, float newTargetDist,
            Transform targetJump = null)
        {
            _target = target;
            _center = center;
            _startingOffset = startingOffset;
            _bodyTransform = bodyTransform;
            _p1Root = p1Root;
            _p2Root = p2Root;
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

            // Create a dedicated targetLocal child GameObject (mirrors original LegController).
            var tlGo = new GameObject("P2LegTargetLocal_" + gameObject.name);
            _targetLocal = tlGo.transform;
            _targetLocal.SetParent(null, false);

            // Seed from the already-cloned IK target FIRST.  That pose was copied from the
            // correct P1 leg by Unity, so it preserves leg identity even if P2 is spawned
            // next to awkward geometry.  The previous implementation immediately replaced
            // this with a sphere-cast from the LegController GameObject, which could rarely
            // choose a surface on the opposite side of the spider.
            _targetLocal.position = _target.position;
            _targetLocal.rotation = _target.rotation;

            // If the cloned target itself is already across the body centerline, do not
            // preserve that bad pose.  This can happen if a cloned runtime reference was
            // carrying transient P1 leg state at exactly the wrong spawn moment.
            if (!IsPointOnOwnSide(_targetLocal.position))
            {
                LogCrossSideReject(_targetLocal.position);
                _targetLocal.position = GetHomePosition() - _bodyTransform.up * _scale * 0.5f;
                _targetLocal.rotation = transform.rotation;
            }

            // Then settle onto the surface below this leg's authored CENTER, not the
            // controller object's transform.  The center is unique to each of the eight
            // legs and therefore cannot silently swap left/right ownership.
            var origin = GetHomePosition() + _bodyTransform.up * _rayUpOffset * _scale;
            var ray = new Ray(origin, -_bodyTransform.up);
            RaycastHit hit;
            if (CheckLegSphereCast(ray, out hit))
                PlaceTargetLocal(hit);

            _lerp = 1f;
            _target.position = _targetLocal.position;
            _target.rotation = _targetLocal.rotation;
            _oldTarget = _targetLocal.position;
            _inited = true;
        }

        private void OnEnable()
        {
            // AddComponent invokes OnEnable before Init(), so the first useful OnEnable is
            // the later P2 root lifecycle refresh.  Re-settle once after that refresh using
            // this leg's own authored center.  This removes spawn-order dependence without
            // continually snapping feet during normal play.
            if (!_inited || _postInitEnableRefreshed || _targetLocal == null || _target == null)
                return;

            _postInitEnableRefreshed = true;
            if (IsBodyJumping())
                return;

            var origin = GetHomePosition() + _bodyTransform.up * _rayUpOffset * _scale;
            RaycastHit hit;
            if (CheckLegSphereCast(new Ray(origin, -_bodyTransform.up), out hit))
            {
                PlaceTargetLocal(hit);
                _target.position = _targetLocal.position;
                _target.rotation = _targetLocal.rotation;
                _oldTarget = _targetLocal.position;
                _lerp = 1f;
            }
        }

        private void OnDestroy()
        {
            if (_targetLocal != null)
                UnityEngine.Object.Destroy(_targetLocal.gameObject);
        }

        private Vector3 GetHomePosition()
        {
            if (_center != null)
                return _center.position;

            // startingOffset is only a fallback for unusual game builds where center is
            // missing.  TransformPoint preserves the original controller's local axes.
            if (_startingOffset.sqrMagnitude > 0.000001f)
                return transform.TransformPoint(_startingOffset);

            return transform.position;
        }

        /// <summary>
        /// Scene-entry-only hard reset used after P2's body/ground orientation has had a
        /// chance to settle. This does not copy a pose from P1 and does not guess a leg
        /// index: it raycasts from THIS leg's authored center using P2's CURRENT body axes.
        /// F9 spawning does not call this path.
        /// </summary>
        internal bool ForceSceneSpawnSettle(string reason)
        {
            if (!_inited || _target == null || _targetLocal == null || _bodyTransform == null)
                return false;
            if (IsBodyJumping())
                return false;

            ForceSettleFromOwnCenter();

            try
            {
                Vector3 homeLocal = _bodyTransform.InverseTransformPoint(GetHomePosition());
                Vector3 targetLocal = _bodyTransform.InverseTransformPoint(_target.position);
                MelonLogger.Msg("[P2LegDriver] Scene-spawn settle " + gameObject.name +
                    " reason=" + reason +
                    " homeLocal=" + homeLocal.ToString("F3") +
                    " targetLocal=" + targetLocal.ToString("F3") + ".");
            }
            catch { }
            return true;
        }

        /// <summary>
        /// Repairs the concrete bad-parent state caught by the scene-entry logs: a private
        /// foot anchor parented under P1/P2 instead of real world geometry. Such an anchor
        /// moves with the spider animation even when the IK target itself was initially right.
        /// </summary>
        internal bool RepairPlayerOwnedAnchorIfNeeded(string reason)
        {
            if (!_inited || _target == null || _targetLocal == null || _bodyTransform == null)
                return false;

            Transform parent = _targetLocal.parent;
            if (!IsPlayerOwnedSurface(parent))
                return false;

            string parentName = GetTransformPath(parent);
            ForceSettleFromOwnCenter();
            try
            {
                MelonLogger.Warning("[P2LegDriver] Repaired player-owned foot anchor for " +
                    gameObject.name + " reason=" + reason + " oldParent=" + parentName + ".");
            }
            catch { }
            return true;
        }

        /// <summary>
        /// Scene-start diagnostic/safety repair for a clearly cross-sided existing target.
        /// This is intentionally NOT run every frame during normal play because a planted
        /// world-space foot can legitimately cross the body's local center while turning.
        /// </summary>
        internal bool RepairCrossedTargetIfNeeded(string reason)
        {
            if (!_inited || _target == null || _targetLocal == null || _bodyTransform == null || _center == null)
                return false;
            if (IsBodyJumping())
                return false;

            bool anchorCrossed = !IsPointOnOwnSide(_targetLocal.position);
            bool ikTargetCrossed = !IsPointOnOwnSide(_target.position);
            if (!anchorCrossed && !ikTargetCrossed)
                return false;

            Vector3 oldAnchor = _targetLocal.position;
            Vector3 oldTarget = _target.position;

            // If only the IK target drifted/came out of the rig in a bad pose while our
            // private foot anchor is still valid, restore the target directly. Otherwise
            // re-seat the anchor from this leg's own center against current ground.
            if (!anchorCrossed)
            {
                _target.position = _targetLocal.position;
                _target.rotation = _targetLocal.rotation;
                _oldTarget = _targetLocal.position;
                _lerp = 1f;
                _resetLeg = false;
                _instantReset = false;
            }
            else
            {
                ForceSettleFromOwnCenter();
            }

            _crossedTargetRepairCount++;
            if (_crossedTargetRepairCount <= 4)
            {
                try
                {
                    Vector3 homeLocal = _bodyTransform.InverseTransformPoint(GetHomePosition());
                    Vector3 oldAnchorLocal = _bodyTransform.InverseTransformPoint(oldAnchor);
                    Vector3 oldTargetLocal = _bodyTransform.InverseTransformPoint(oldTarget);
                    Vector3 newTargetLocal = _bodyTransform.InverseTransformPoint(_target.position);
                    MelonLogger.Warning("[P2LegDriver] Repaired crossed leg target for " + gameObject.name +
                        " reason=" + reason +
                        " home=" + homeLocal.ToString("F3") +
                        " oldAnchor=" + oldAnchorLocal.ToString("F3") +
                        " oldIK=" + oldTargetLocal.ToString("F3") +
                        " newIK=" + newTargetLocal.ToString("F3") + ".");
                }
                catch { }
            }
            return true;
        }

        private void ForceSettleFromOwnCenter()
        {
            if (_targetLocal == null || _target == null || _bodyTransform == null)
                return;

            // Remove any old surface parent before computing the new world-space foot pose.
            _targetLocal.SetParent(null, true);

            Vector3 homePosition = GetHomePosition();
            Vector3 origin = homePosition + _bodyTransform.up * _rayUpOffset * _scale;
            RaycastHit hit;
            if (CheckLegSphereCast(new Ray(origin, -_bodyTransform.up), out hit))
            {
                PlaceTargetLocal(hit);
            }
            else
            {
                // No usable ground hit (spawn transition, midair, unusual geometry). Keep
                // the fallback on the leg's own side by deriving it from its authored center.
                _targetLocal.SetParent(null, true);
                _targetLocal.position = homePosition - _bodyTransform.up * Mathf.Max(0.25f * _scale, _tipHeight * _scale);
                _targetLocal.rotation = transform.rotation;
            }

            _target.position = _targetLocal.position;
            _target.rotation = _targetLocal.rotation;
            _oldTarget = _targetLocal.position;
            _lerp = 1f;
            _resetLeg = false;
            _instantReset = false;
        }

        private bool IsPointOnOwnSide(Vector3 worldPoint)
        {
            if (_bodyTransform == null || _center == null)
                return true;

            Vector3 homeLocal = _bodyTransform.InverseTransformPoint(_center.position);
            Vector3 pointLocal = _bodyTransform.InverseTransformPoint(worldPoint);
            float sideTolerance = Mathf.Max(0.02f, 0.05f * _scale);

            // A leg is allowed to approach the center line, but never initialize/step
            // clearly across it.  This is orientation independent because the comparison
            // is done in the spider body's local right/left axis, so walls and ceilings are
            // handled the same way as floors.
            if (Mathf.Abs(homeLocal.x) <= sideTolerance || Mathf.Abs(pointLocal.x) <= sideTolerance)
                return true;

            return Mathf.Sign(homeLocal.x) == Mathf.Sign(pointLocal.x);
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

        private bool IsBodySprinting()
        {
            try
            {
                var f = SplitScreenMod.BodyMove_IsSprintingField;
                if (f != null && _bodyMovement != null)
                    return (bool)f.GetValue(_bodyMovement);
            }
            catch { }
            return false;
        }

        private bool _wasJumping;

        private void Update()
        {
            if (!_inited || _target == null) return;
            // If a previous parent surface got destroyed (e.g., SplitWebThread destroyed
            // a WebThread that was holding this leg target as a child), Unity will have
            // destroyed _targetLocal with it. Recreate so IK doesn't break for the
            // life of the session.
            if (_targetLocal == null)
            {
                var tlGo = new GameObject("P2LegTargetLocal_" + gameObject.name);
                _targetLocal = tlGo.transform;
                _targetLocal.SetParent(null, false);
                _targetLocal.position = GetHomePosition() - _bodyTransform.up * _scale * 0.5f;
                _targetLocal.rotation = transform.rotation;
                _oldTarget = _targetLocal.position;
                _lerp = 1f;
            }
            bool jumping = IsBodyJumping();

            // Hard invariant: a planted foot anchor may never remain parented to either
            // PlayerSpider hierarchy. A surface-parented anchor is supposed to follow moving
            // WORLD geometry; if it is parented to a spider body/leg collider it will be
            // dragged around by that spider's animation and can appear on the wrong side.
            // Do not enforce left/right centerline every frame here: a correctly planted foot
            // can legitimately cross the body's local centerline while the spider rotates.
            if (!jumping)
                RepairPlayerOwnedAnchorIfNeeded("runtime player-surface guard");

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
                // Mirror MasterLegController.StepTime (ilspy MasterLegController.cs:71-79):
                // stepping is twice as fast while the body is sprinting. Without this,
                // P2's legs stay at the walking cadence and visibly drag behind the
                // faster body when sprinting.
                float stepTime = IsBodySprinting() ? _stepTime * 0.5f : _stepTime;
                _lerp += dt / (stepTime * _scale);
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
            // Compute the ray from this leg's authored home/center.  Using the
            // LegController GameObject itself here was subtly unsafe: depending on clone
            // lifecycle its transform can be shared/central while the center remains the
            // unambiguous per-leg rest position.
            Vector3 homePosition = GetHomePosition();
            Vector3 rayOrigin;
            if (_resetLeg)
            {
                rayOrigin = homePosition
                    + _bodyTransform.up * _rayUpOffset * _scale;
            }
            else
            {
                var moveY = GetMoveVectorY();
                rayOrigin = homePosition
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
                // Parent to hit surface so feet track moving geometry (webs, platforms).
                // EXCEPTION: never parent to a WebThread — they get destroyed/replaced by
                // SplitWebThread when *either* player builds a new web, and a destroyed
                // parent takes our leg-target child with it (which breaks IK and makes
                // the P2 spider visually disappear). Webs barely move anyway, so a
                // world-space target is fine.
                if (!IsWebSurface(hit.transform))
                {
                    _targetLocal.SetParent(hit.transform, true);
                }
                else
                {
                    _targetLocal.SetParent(null, true);
                }
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
            // Preserve vanilla's search order (ray first, then 4 increasing sphere radii),
            // but choose the nearest ACCEPTABLE hit rather than blindly accepting Unity's
            // first collider. P2 must never plant a foot on either spider hierarchy.
            // This is the key scene-entry fix: P2's first casts used to happen before its
            // collider layers were changed, so a folded startup pose could self-hit.
            float distance = _rayLength * _scale;
            int count = Physics.RaycastNonAlloc(ray, _castBuffer, distance, _whatIsGround);
            if (TrySelectGroundHit(count, out hit))
                return true;

            for (int i = 1; i <= 4; i++)
            {
                float r = _sphereRadius * _scale * i * 0.25f;
                count = Physics.SphereCastNonAlloc(ray, r, _castBuffer, distance, _whatIsGround);
                if (TrySelectGroundHit(count, out hit))
                    return true;
            }

            hit = default;
            return false;
        }

        private bool TrySelectGroundHit(int count, out RaycastHit hit)
        {
            bool found = false;
            float bestDistance = float.PositiveInfinity;
            RaycastHit best = default;

            int limit = Mathf.Min(count, _castBuffer.Length);
            for (int i = 0; i < limit; i++)
            {
                RaycastHit candidate = _castBuffer[i];
                Transform surface = candidate.transform;
                if (surface == null)
                    continue;

                if (IsPlayerOwnedSurface(surface))
                {
                    LogPlayerSurfaceReject(surface, candidate.point);
                    continue;
                }

                if (!IsPointOnOwnSide(candidate.point))
                {
                    LogCrossSideReject(candidate.point);
                    continue;
                }

                if (!found || candidate.distance < bestDistance)
                {
                    found = true;
                    bestDistance = candidate.distance;
                    best = candidate;
                }
            }

            hit = best;
            return found;
        }

        private bool IsPlayerOwnedSurface(Transform t)
        {
            if (t == null) return false;

            if (_p2Root != null && (object.ReferenceEquals(t, _p2Root) || t.IsChildOf(_p2Root)))
                return true;
            if (_p1Root != null && (object.ReferenceEquals(t, _p1Root) || t.IsChildOf(_p1Root)))
                return true;

            return false;
        }

        private void LogPlayerSurfaceReject(Transform surface, Vector3 point)
        {
            if (_loggedPlayerSurfaceReject) return;
            _loggedPlayerSurfaceReject = true;

            try
            {
                Vector3 pointLocal = _bodyTransform == null
                    ? Vector3.zero
                    : _bodyTransform.InverseTransformPoint(point);
                MelonLogger.Warning("[P2LegDriver] Rejected player-owned ground hit for " +
                    gameObject.name + " surface=" + GetTransformPath(surface) +
                    " pointLocal=" + pointLocal.ToString("F3") + ".");
            }
            catch { }
        }

        private static string GetTransformPath(Transform t)
        {
            if (t == null) return "null";
            try
            {
                string path = t.name;
                Transform p = t.parent;
                int guard = 0;
                while (p != null && guard++ < 12)
                {
                    path = p.name + "/" + path;
                    p = p.parent;
                }
                return path;
            }
            catch { return t.name; }
        }

        private void LogCrossSideReject(Vector3 point)
        {
            if (_loggedCrossSideReject)
                return;
            _loggedCrossSideReject = true;

            try
            {
                Vector3 homeLocal = _bodyTransform == null ? Vector3.zero : _bodyTransform.InverseTransformPoint(GetHomePosition());
                Vector3 pointLocal = _bodyTransform == null ? Vector3.zero : _bodyTransform.InverseTransformPoint(point);
                MelonLogger.Warning("[P2LegDriver] Rejected cross-side foot candidate for " + gameObject.name +
                    " homeLocal=" + homeLocal.ToString("F3") + " hitLocal=" + pointLocal.ToString("F3") + ".");
            }
            catch { }
        }

        private void PlaceTargetLocal(RaycastHit hit)
        {
            if (hit.transform == null || IsPlayerOwnedSurface(hit.transform))
            {
                if (hit.transform != null)
                    LogPlayerSurfaceReject(hit.transform, hit.point);
                return;
            }

            _targetLocal.position = hit.point + hit.normal * _tipHeight * _scale;
            var fwd = Vector3.Cross(transform.right, hit.normal);
            _targetLocal.rotation = Quaternion.LookRotation(fwd, hit.normal);
            if (!IsWebSurface(hit.transform))
                _targetLocal.SetParent(hit.transform, true);
            else
                _targetLocal.SetParent(null, true);
        }

        // Web layers ("Web" and "PlayerWeb") are destroyed/replaced by SplitWebThread,
        // and Unity destroys child Transforms with them. Avoid parenting to those.
        private static int _webLayerMask = -1;
        private static bool IsWebSurface(Transform t)
        {
            if (t == null) return false;
            if (_webLayerMask == -1)
            {
                int webLayer = LayerMask.NameToLayer("Web");
                int playerWebLayer = LayerMask.NameToLayer("PlayerWeb");
                int m = 0;
                if (webLayer >= 0) m |= 1 << webLayer;
                if (playerWebLayer >= 0) m |= 1 << playerWebLayer;
                _webLayerMask = m;
            }
            return ((1 << t.gameObject.layer) & _webLayerMask) != 0;
        }
    }

    internal static class CameraIsolationDiagnostics
    {
        private const float ActiveLogInterval = 0.25f;
        private const float IdleLogInterval = 1.0f;
        private static float _nextSampleTime;
        private static float _lastP2LookTime = -999f;
        private static Vector3 _lastP1Position;
        private static Quaternion _lastP1Rotation;
        private static bool _haveP1Pose;
        private static int _lookCallbackCount;
        private static int _blockedLookCallbackCount;
        private static float _nextCallbackLogTime;
        private static float _nextStageLogTime;

        internal static void TraceP2CameraUpdate(Camera p1Camera, Vector3 before, string stage)
        {
            if (!SplitScreenMod.DebugSpeedLog || p1Camera == null) return;

            Vector2 p2Stick = InputCompat.GetP2RightStick(SplitScreenMod.P2GamepadIndex, 0f);
            if (p2Stick.magnitude < SplitScreenMod.P2Deadzone) return;

            float now = Time.unscaledTime;
            if (now < _nextStageLogTime) return;
            _nextStageLogTime = now + ActiveLogInterval;

            Vector3 after = p1Camera.transform.position;
            MelonLogger.Msg("[CamDiag/Stage] frame=" + Time.frameCount +
                " stage=" + stage + " p2Stick=" + Format(p2Stick) +
                " p1Before=" + Format(before) + " p1After=" + Format(after) +
                " immediateDelta=" + Vector3.Distance(before, after).ToString("F6"));
        }

        internal static void LogLookCallback(object instance, object context, Vector2 p2Stick, bool fromP2, bool blocked, Vector2 retainedBefore)
        {
            if (!SplitScreenMod.DebugSpeedLog) return;

            _lookCallbackCount++;
            if (blocked) _blockedLookCallbackCount++;
            float now = Time.unscaledTime;
            if (now < _nextCallbackLogTime) return;
            _nextCallbackLogTime = now + 0.25f;

            MelonLogger.Msg("[CamDiag/LookCallback] frame=" + Time.frameCount +
                " owner=" + DescribeComponent(instance as Component) +
                " ctx=" + InputCompat.DescribeCallbackContext(context) +
                " p2Stick=" + Format(p2Stick) +
                " retainedBefore=" + Format(retainedBefore) +
                " fromP2=" + fromP2 + " blocked=" + blocked +
                " totals=" + _lookCallbackCount + "/" + _blockedLookCallbackCount);
        }

        internal static void Sample(Camera p1Camera, Camera p2Camera)
        {
            if (!SplitScreenMod.DebugSpeedLog || !SplitScreenMod.IsSplitScreenActive || p1Camera == null)
            {
                _haveP1Pose = false;
                return;
            }

            Vector2 p1Stick = InputCompat.GetP1RightStick(0f);
            Vector2 p2Stick = InputCompat.GetP2RightStick(SplitScreenMod.P2GamepadIndex, 0f);
            bool p2Active = p2Stick.magnitude >= SplitScreenMod.P2Deadzone;
            float now = Time.unscaledTime;
            if (p2Active) _lastP2LookTime = now;

            Vector3 p1Position = p1Camera.transform.position;
            Quaternion p1Rotation = p1Camera.transform.rotation;
            GameObject p1Spider = GameObject.Find("PlayerSpider");
            GameObject p2Spider = GameObject.Find("PlayerSpider_P2");
            float positionDelta = _haveP1Pose ? Vector3.Distance(_lastP1Position, p1Position) : 0f;
            float rotationDelta = _haveP1Pose ? Quaternion.Angle(_lastP1Rotation, p1Rotation) : 0f;
            _lastP1Position = p1Position;
            _lastP1Rotation = p1Rotation;
            _haveP1Pose = true;

            bool recentlyActive = now - _lastP2LookTime < 1.0f;
            bool p1Moved = positionDelta > 0.0005f || rotationDelta > 0.005f;
            float interval = recentlyActive ? ActiveLogInterval : IdleLogInterval;
            if (now < _nextSampleTime || (!recentlyActive && !p1Moved)) return;
            _nextSampleTime = now + interval;

            MelonLogger.Msg("[CamDiag/Pose] frame=" + Time.frameCount +
                " p2Index=" + SplitScreenMod.P2GamepadIndex +
                " pads=" + InputCompat.GetConnectedGamepadCount() +
                " p1Stick=" + Format(p1Stick) + " p2Stick=" + Format(p2Stick) +
                " p2Active=" + p2Active +
                " | p1Pos=" + Format(p1Position) + " dPos=" + positionDelta.ToString("F4") +
                " p1Rot=" + Format(p1Rotation.eulerAngles) + " dRot=" + rotationDelta.ToString("F3") +
                " p1Parent=" + DescribeTransform(p1Camera.transform.parent) +
                " p1Spider=" + (p1Spider == null ? "null" : Format(p1Spider.transform.position)) +
                " p1SpiderRot=" + (p1Spider == null ? "null" : Format(p1Spider.transform.eulerAngles)) +
                " | p2Pos=" + (p2Camera == null ? "null" : Format(p2Camera.transform.position)) +
                " p2Rot=" + (p2Camera == null ? "null" : Format(p2Camera.transform.rotation.eulerAngles)) +
                " p2Spider=" + (p2Spider == null ? "null" : Format(p2Spider.transform.position)) +
                " p2SpiderRot=" + (p2Spider == null ? "null" : Format(p2Spider.transform.eulerAngles)));

            DumpCameraDrivers();
            DumpCinemachineState();
        }

        private static void DumpCameraDrivers()
        {
            try
            {
                Type lookType = AccessTools.TypeByName("_Scripts.Camera.CameraMouseLook");
                if (lookType != null)
                {
                    const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    FieldInfo lookInput = lookType.GetField("lookInput", F);
                    FieldInfo mouseLook = lookType.GetField("mouseLook", F);
                    FieldInfo yawTransform = lookType.GetField("yawTransform", F);
                    UnityEngine.Object[] looks = UnityEngine.Object.FindObjectsOfType(lookType, true);
                    for (int i = 0; looks != null && i < looks.Length; i++)
                    {
                        Component component = looks[i] as Component;
                        Transform yaw = yawTransform == null ? null : yawTransform.GetValue(looks[i]) as Transform;
                        MelonLogger.Msg("[CamDiag/Driver] look#" + i + "=" + DescribeComponent(component) +
                            " enabled=" + DescribeEnabled(looks[i]) +
                            " lookInput=" + FormatObjectVector2(lookInput == null ? null : lookInput.GetValue(looks[i])) +
                            " mouseLook=" + FormatObjectVector2(mouseLook == null ? null : mouseLook.GetValue(looks[i])) +
                            " localRot=" + (component == null ? "null" : Format(component.transform.localEulerAngles)) +
                            " yaw=" + DescribeTransform(yaw) +
                            " yawLocalRot=" + (yaw == null ? "null" : Format(yaw.localEulerAngles)));
                    }
                }

                Type zoomType = AccessTools.TypeByName("_Scripts.Camera.CameraZoom");
                if (zoomType != null)
                {
                    const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    FieldInfo zoom = zoomType.GetField("zoom", F);
                    FieldInfo cameraDistance = zoomType.GetField("cameraDistance", F);
                    FieldInfo finalDistance = zoomType.GetField("finalCameraDistance", F);
                    UnityEngine.Object[] zooms = UnityEngine.Object.FindObjectsOfType(zoomType, true);
                    for (int i = 0; zooms != null && i < zooms.Length; i++)
                    {
                        Component component = zooms[i] as Component;
                        MelonLogger.Msg("[CamDiag/Zoom] zoom#" + i + "=" + DescribeComponent(component) +
                            " enabled=" + DescribeEnabled(zooms[i]) +
                            " zoom=" + FormatObjectFloat(zoom == null ? null : zoom.GetValue(zooms[i])) +
                            " smoothed=" + FormatObjectFloat(cameraDistance == null ? null : cameraDistance.GetValue(zooms[i])) +
                            " final=" + FormatObjectFloat(finalDistance == null ? null : finalDistance.GetValue(zooms[i])) +
                            " forward=" + (component == null ? "null" : Format(component.transform.forward)));
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[CamDiag] driver inspection failed: " + ex.Message);
            }
        }

        private static void DumpCinemachineState()
        {
            try
            {
                const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                Type controllerType = AccessTools.TypeByName("_Scripts.Singletons.CameraController");
                if (controllerType == null) return;

                UnityEngine.Object[] controllers = UnityEngine.Object.FindObjectsOfType(controllerType, true);
                if (controllers == null || controllers.Length == 0) return;
                object controller = controllers[0];

                FieldInfo mainCameraField = controllerType.GetField("mainCamera", F);
                FieldInfo shoulderField = controllerType.GetField("shoulderOffset", F);
                FieldInfo inputField = controllerType.GetField("inputTransform", F);
                FieldInfo targetField = controllerType.GetField("followCameraFollowTarget", F);
                FieldInfo vcamField = controllerType.GetField("cinemachineFollowCamera", F);

                Camera mainCamera = mainCameraField == null ? null : mainCameraField.GetValue(controller) as Camera;
                Transform input = inputField == null ? null : inputField.GetValue(controller) as Transform;
                Component followTarget = targetField == null ? null : targetField.GetValue(controller) as Component;
                Component vcam = vcamField == null ? null : vcamField.GetValue(controller) as Component;
                Transform followedTransform = null;
                if (followTarget != null)
                {
                    FieldInfo followedField = AccessTools.Field(followTarget.GetType(), "target");
                    followedTransform = followedField == null ? null : followedField.GetValue(followTarget) as Transform;
                }

                MelonLogger.Msg("[CamDiag/Controller] main=" + DescribeComponent(mainCamera) +
                    " shoulder=" + FormatObjectVector3(shoulderField == null ? null : shoulderField.GetValue(controller)) +
                    " input=" + DescribeTransform(input) +
                    " followTarget=" + DescribeComponent(followTarget) +
                    " followTargetPos=" + (followTarget == null ? "null" : Format(followTarget.transform.position)) +
                    " followTargetRot=" + (followTarget == null ? "null" : Format(followTarget.transform.eulerAngles)) +
                    " followed=" + DescribeTransform(followedTransform) +
                    " followedPos=" + (followedTransform == null ? "null" : Format(followedTransform.position)) +
                    " followedRot=" + (followedTransform == null ? "null" : Format(followedTransform.eulerAngles)) +
                    " vcam=" + DescribeComponent(vcam) +
                    " vcamPos=" + (vcam == null ? "null" : Format(vcam.transform.position)) +
                    " vcamRot=" + (vcam == null ? "null" : Format(vcam.transform.eulerAngles)));

                if (vcam == null) return;
                Type followType = AccessTools.TypeByName("Cinemachine.Cinemachine3rdPersonFollow");
                if (followType == null) return;

                MethodInfo getter = null;
                MethodInfo[] methods = vcam.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo method = methods[i];
                    if (method.Name == "GetCinemachineComponent" && method.IsGenericMethodDefinition &&
                        method.GetParameters().Length == 0)
                    {
                        getter = method;
                        break;
                    }
                }
                if (getter == null) return;

                object follow = getter.MakeGenericMethod(followType).Invoke(vcam, null);
                if (follow == null) return;

                MelonLogger.Msg("[CamDiag/Follow] shoulder=" + ReadVector3(followType, follow, "ShoulderOffset") +
                    " verticalArm=" + ReadFloat(followType, follow, "VerticalArmLength") +
                    " side=" + ReadFloat(followType, follow, "CameraSide") +
                    " distance=" + ReadFloat(followType, follow, "CameraDistance") +
                    " damping=" + ReadVector3(followType, follow, "Damping") +
                    " previousTarget=" + ReadVector3(followType, follow, "m_PreviousFollowTargetPosition") +
                    " dampingCorrection=" + ReadVector3(followType, follow, "m_DampingCorrection") +
                    " collisionCorrection=" + ReadVector3(followType, follow, "m_CamPosCollisionCorrection"));

                DumpBodyTarget("P1", SplitScreenMod.P1BodyMovementInstance);
                DumpBodyTarget("P2", SplitScreenMod.P2BodyMovementInstance);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[CamDiag] Cinemachine inspection failed: " + ex.Message);
            }
        }

        private static void DumpBodyTarget(string player, Component body)
        {
            if (body == null) return;
            FieldInfo targetField = AccessTools.Field(body.GetType(), "targetTransform");
            Transform target = targetField == null ? null : targetField.GetValue(body) as Transform;
            MelonLogger.Msg("[CamDiag/Body] player=" + player +
                " body=" + DescribeComponent(body) +
                " bodyPos=" + Format(body.transform.position) +
                " bodyRot=" + Format(body.transform.eulerAngles) +
                " target=" + DescribeTransform(target) +
                " targetPos=" + (target == null ? "null" : Format(target.position)) +
                " targetRot=" + (target == null ? "null" : Format(target.eulerAngles)));
        }

        private static string ReadVector3(Type type, object instance, string fieldName)
        {
            FieldInfo field = AccessTools.Field(type, fieldName);
            return field == null ? "?" : FormatObjectVector3(field.GetValue(instance));
        }

        private static string ReadFloat(Type type, object instance, string fieldName)
        {
            FieldInfo field = AccessTools.Field(type, fieldName);
            return field == null ? "?" : FormatObjectFloat(field.GetValue(instance));
        }

        private static string DescribeEnabled(object value)
        {
            Behaviour behaviour = value as Behaviour;
            return behaviour == null ? "n/a" : (behaviour.enabled ? "ON" : "OFF");
        }

        private static string DescribeComponent(Component component)
        {
            return component == null ? "null" : component.GetType().FullName + "@" + DescribeTransform(component.transform);
        }

        private static string DescribeTransform(Transform transform)
        {
            if (transform == null) return "null";
            string path = transform.name;
            Transform parent = transform.parent;
            int depth = 0;
            while (parent != null && depth++ < 6)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path + "#" + transform.GetInstanceID();
        }

        private static string Format(Vector2 value) { return "(" + value.x.ToString("F3") + "," + value.y.ToString("F3") + ")"; }
        private static string Format(Vector3 value) { return "(" + value.x.ToString("F3") + "," + value.y.ToString("F3") + "," + value.z.ToString("F3") + ")"; }
        private static string FormatObjectVector2(object value) { return value is Vector2 ? Format((Vector2)value) : "?"; }
        private static string FormatObjectVector3(object value) { return value is Vector3 ? Format((Vector3)value) : "?"; }
        private static string FormatObjectFloat(object value) { return value is float ? ((float)value).ToString("F3") : "?"; }
    }

    internal static class CameraMouseLookPatches
    {
        private static FieldInfo _lookInputField;

        public static bool OnLook_Prefix(object __instance, object __0)
        {
            if (!SplitScreenMod.IsSplitScreenActive) return true;
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            // Composite bindings in newer game builds do not always expose the originating
            // device through CallbackContext. Fall back to the dedicated P2 stick that drives
            // our camera rig so a missing device does not leak P2 look into P1.
            Vector2 retainedBefore = Vector2.zero;
            try
            {
                if (_lookInputField == null && __instance != null)
                    _lookInputField = AccessTools.Field(__instance.GetType(), "lookInput");
                if (_lookInputField != null && _lookInputField.GetValue(__instance) is Vector2)
                    retainedBefore = (Vector2)_lookInputField.GetValue(__instance);
            }
            catch { }

            bool fromP2 = InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex);
            Vector2 p2Look = InputCompat.GetP2RightStick(SplitScreenMod.P2GamepadIndex, SplitScreenMod.P2Deadzone);
            if (!fromP2)
            {
                fromP2 = p2Look.sqrMagnitude > 0f;
            }

            CameraIsolationDiagnostics.LogLookCallback(__instance, __0, p2Look, fromP2, fromP2, retainedBefore);

            if (fromP2)
            {
                // CameraMouseLook keeps its last performed value and applies it every Update.
                // Clear it as well as skipping this callback, otherwise one leaked sample can
                // continue nudging P1 after P2 releases the stick.
                try
                {
                    if (_lookInputField == null && __instance != null)
                        _lookInputField = AccessTools.Field(__instance.GetType(), "lookInput");
                    if (_lookInputField != null)
                        _lookInputField.SetValue(__instance, Vector2.zero);
                }
                catch { }
                return false;
            }

            return true;
        }
    }

    internal static class CameraZoomPatches
    {
        public static bool OnZoom_Prefix(object __0)
        {
            if (!SplitScreenMod.IsSplitScreenActive) return true;
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            // Prefer callback ownership, with a polling fallback for composite bindings that
            // report the shared action rather than P2's actual device.
            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex) ||
                InputCompat.IsP2CameraZoomPressedNow(true, SplitScreenMod.P2GamepadIndex))
                return false;

            return true;
        }
    }

    internal static class SpiderInteractionPatches
    {
        private static FieldInfo _bodyMovementField;
        private static FieldInfo _isPlayerField;
        private static Type _bodyMovementType;

        private static bool IsP2(object __instance)
        {
            // SpiderInteraction is on the spider root, not BodyMovement directly,
            // so identity-equality against P2BodyMovementInstance doesn't apply here.
            // Use hierarchy check, which is correct for SpiderInteraction (its
            // transform parent is not modified by InitializeJump).
            var mb = __instance as MonoBehaviour;
            if (mb == null) return false;
            return mb.GetComponentInParent<P2Marker>() != null;
        }

        private static void CacheFields(object __instance)
        {
            if (__instance == null) return;

            if (_bodyMovementField == null)
            {
                var t = __instance.GetType();
                _bodyMovementField = t.GetField("bodyMovement", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            if (_bodyMovementType == null)
                _bodyMovementType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");

            if (_isPlayerField == null && _bodyMovementType != null)
                _isPlayerField = _bodyMovementType.GetField("isPlayer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static object GetBodyMovement(object __instance)
        {
            if (__instance == null) return null;

            CacheFields(__instance);

            if (_bodyMovementField != null)
            {
                try
                {
                    var movement = _bodyMovementField.GetValue(__instance);
                    if (movement != null) return movement;
                }
                catch { }
            }

            var mb = __instance as MonoBehaviour;
            if (mb != null && _bodyMovementType != null)
            {
                try { return mb.GetComponentInParent(_bodyMovementType); }
                catch { }
            }

            return null;
        }

        public static void TemporarilyEnableIsPlayer_Prefix(object __instance, ref bool __state)
        {
            __state = false;

            if (!IsP2(__instance))
                return;

            var movement = GetBodyMovement(__instance);
            if (movement == null || _isPlayerField == null)
                return;

            try
            {
                var current = _isPlayerField.GetValue(movement);
                if (current is bool && !(bool)current)
                {
                    _isPlayerField.SetValue(movement, true);
                    __state = true;
                }
            }
            catch { }
        }

        public static void TemporarilyEnableIsPlayer_Postfix(object __instance, bool __state)
        {
            if (!__state)
                return;

            var movement = GetBodyMovement(__instance);
            if (movement == null || _isPlayerField == null)
                return;

            try { _isPlayerField.SetValue(movement, false); }
            catch { }
        }
    }

    internal static class BodyMovementPatches
    {
        private static bool IsP2(object __instance)
        {
            // Identity check first — robust against parent=null detachment
            // (P2's InitializeJump sets base.transform.parent=null, which can break
            // hierarchy-based GetComponentInParent<P2Marker>() if BodyMovement is on
            // a child of _p2Spider).
            if (SplitScreenMod.P2BodyMovementInstance != null &&
                ReferenceEquals(__instance, SplitScreenMod.P2BodyMovementInstance))
                return true;
            var mb = __instance as MonoBehaviour;
            if (mb == null) return false;
            return mb.GetComponentInParent<P2Marker>() != null;
        }

        private static bool _pjFieldsCached;
        private static FieldInfo _fPjJumpTimer, _fPjRb, _fPjState, _fPjLastRotation;
        private static FieldInfo _fPjMoveInput, _fPjPitchAngle;
        private static FieldInfo _fPjLandingRotSmooth, _fPjJumpingRotSmooth;
        private static FieldInfo _fPjLandingOffset, _fPjLandingRadius;
        private static FieldInfo _fPjAerialThresh, _fPjAerialSpeedLR, _fPjAerialSpeedFB;
        private static FieldInfo _fPjBounceMinVelocity;
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
            _fPjBounceMinVelocity = t.GetField("bounceMinimumVelocity",     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
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
                float bounceMinimumVelocity = Mathf.Max(0f, PjGet<float>(_fPjBounceMinVelocity, __instance, 10f));
                bool p2WebActive = SplitScreenMod.P2WebActive;

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

                // Mirror the walking path: the spider root orientation is driven
                // explicitly, so grapple torque must not keep rotating the body
                // between fixed steps and show up as camera-visible shake.
                rb.angularVelocity = Vector3.zero;

                // --- Rotation (mirror original math) ---
                float velMagnitude = vel.magnitude;
                Vector3 normalized = vel.sqrMagnitude > 0.001f
                    ? Vector3.Cross(Vector3.up, vel.normalized).normalized
                    : mb.transform.right;
                float pitchAngle = PjGet<float>(_fPjPitchAngle, __instance, 0f);
                Vector3 forward2  = Quaternion.AngleAxis(-pitchAngle, normalized) * vel.normalized;
                Vector3 upwards   = Vector3.Cross(normalized, -forward2);

                bool landing = jumpTimer <= 0f && hitGround && (!p2WebActive || velMagnitude < bounceMinimumVelocity);
                if (landing)
                {
                    upwards  = hitInfo.normal;
                    forward2 = Vector3.Cross(normalized, hitInfo.normal);
                }

                if (vel.sqrMagnitude > 0f)
                {
                    float smoothness = PjGet<float>(_fPjJumpingRotSmooth, __instance, 4f);
                    if (landing || (hitGround && (!p2WebActive || velMagnitude > bounceMinimumVelocity)))
                        smoothness = PjGet<float>(_fPjLandingRotSmooth, __instance, 2f);
                    Quaternion targetRot = Quaternion.LookRotation(forward2, upwards);
                    Quaternion lastRot   = PjGet<Quaternion>(_fPjLastRotation, __instance, mb.transform.rotation);
                    mb.transform.rotation = Quaternion.Slerp(lastRot, targetRot, 1f / (1f + smoothness));
                }
                _fPjLastRotation?.SetValue(__instance, mb.transform.rotation);

                // --- Early return while still airborne ---
                if (jumpTimer > 0f || (p2WebActive && velMagnitude > bounceMinimumVelocity)) return false;

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
            if (!SplitScreenMod.IsSplitScreenActive) return true;
            // P2 movement, jump and sprint are all driven by the dedicated polling
            // path. Letting the cloned BodyMovement also consume the shared game's
            // callbacks makes P1's controls act on P2 and can apply jump input twice.
            if (IsP2(__instance)) return false;

            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex))
                return false;

            return true;
        }

        // State field cache for jump ground-check
        private static bool _stateFieldCached;
        private static FieldInfo _fStateForJump;
        private static object _jumpingStateForJump;
        private static object _walkingStateForJump;
        private static bool IsAlreadyJumping(object instance)
        {
            if (!_stateFieldCached)
            {
                _stateFieldCached = true;
                _fStateForJump = instance.GetType().GetField("state",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (_fStateForJump != null)
                {
                    try { _jumpingStateForJump = Enum.Parse(_fStateForJump.FieldType, "Jumping"); } catch { }
                    try { _walkingStateForJump = Enum.Parse(_fStateForJump.FieldType, "Walking"); } catch { }
                }
            }
            if (_fStateForJump == null || _jumpingStateForJump == null) return false;
            try { return _fStateForJump.GetValue(instance).Equals(_jumpingStateForJump); } catch { return false; }
        }

        private static bool IsWalkingExact(object instance)
        {
            // Ensure cache (shares the _stateFieldCached path)
            if (!_stateFieldCached) IsAlreadyJumping(instance);
            if (_fStateForJump == null || _walkingStateForJump == null) return false;
            try { return _fStateForJump.GetValue(instance).Equals(_walkingStateForJump); } catch { return false; }
        }

        public static void FixedUpdate_Prefix(object __instance)
        {
            if (IsP2(__instance))
            {
                // Hold P2's sprint state authoritatively each physics step. The game's
                // sprint handling (ilspy BodyMovement.cs:739-747) runs in PerformWalking
                // right after this prefix: the Hold+KB/M branch copies sprintInput into
                // isSprinting (so we write the desired state every step), while the
                // toggle branch flips isSprinting once per sprintInput=true (so we pulse
                // only on mismatch — converges in one step, no oscillation).
                bool holdKbm;
                if (SplitScreenMod.BodyMove_SprintInputField != null &&
                    SplitScreenMod.BodyMove_IsSprintingField != null &&
                    SplitScreenMod.TryGetSprintBranch(out holdKbm))
                {
                    bool desired = SplitScreenMod.P2SprintDesired;
                    try
                    {
                        if (holdKbm)
                        {
                            SplitScreenMod.BodyMove_SprintInputField.SetValue(__instance, desired);
                        }
                        else
                        {
                            bool sprinting = (bool)SplitScreenMod.BodyMove_IsSprintingField.GetValue(__instance);
                            SplitScreenMod.BodyMove_SprintInputField.SetValue(__instance, sprinting != desired);
                        }
                    }
                    catch { }
                }

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
            else
            {
                // P1 jump bypass: consume P1JumpPressed flag set by direct polling.
                // CRITICAL: the else branch fires for ANY non-P2 BodyMovement,
                // including NPC spiders. We must only consume the flag for the
                // actual P1 instance — otherwise an NPC's FixedUpdate (which may
                // run before P1's in the same fixed step) eats the flag and P1
                // never jumps.
                if (!ReferenceEquals(__instance, SplitScreenMod.P1BodyMovementInstance))
                    return;

                if (SplitScreenMod.P1JumpPressed)
                {
                    SplitScreenMod.P1JumpPressed = false;
                    if (!IsAlreadyJumping(__instance) &&
                        SplitScreenMod.BodyMove_JumpInputField != null)
                        try { SplitScreenMod.BodyMove_JumpInputField.SetValue(__instance, true); } catch { }
                }
            }
        }

        public static void FixedUpdate_Postfix(object __instance)
        {
            if (!IsP2(__instance))
                return;

            P2MovableCollisionHelper.SyncForBodyMovement(__instance as Component);
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
        // Cache "is this LegController P2's?" by instanceID so we don't depend on
        // hierarchy walks every FixedUpdate. BodyMovement.InitializeJump detaches
        // the spider's transform.parent mid-jump (ilspy: BodyMovement.cs:952),
        // which would make GetComponentInParent<P2Marker>() return null and stop
        // applying the P2 leg-target reparent fix exactly when it's most needed.
        private static readonly Dictionary<int, bool> _isP2Cache = new Dictionary<int, bool>();

        internal static void ClearCache()
        {
            _isP2Cache.Clear();
        }

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

        private static bool IsP2LegController(MonoBehaviour mb)
        {
            int id = mb.GetInstanceID();
            if (_isP2Cache.TryGetValue(id, out bool cached)) return cached;

            bool isP2 = false;
            var p2Spider = SplitScreenMod._p2Spider;
            if (p2Spider != null)
            {
                // Walk up the transform chain looking for the P2 spider root.
                // Done once and cached, so the result survives parent detachment.
                for (var t = mb.transform; t != null; t = t.parent)
                {
                    if (object.ReferenceEquals(t.gameObject, p2Spider))
                    {
                        isP2 = true;
                        break;
                    }
                }
                // Fallback for components that have already been detached by the
                // time we first see them: identity-check the owning BodyMovement.
                if (!isP2)
                {
                    var p2bm = SplitScreenMod.P2BodyMovementInstance;
                    if (p2bm != null)
                    {
                        var ownBm = mb.GetComponentInParent(p2bm.GetType()) as Component;
                        if (ownBm != null && object.ReferenceEquals(ownBm, p2bm))
                            isP2 = true;
                    }
                }
            }
            _isP2Cache[id] = isP2;
            return isP2;
        }

        public static void FixedUpdate_Prefix(object __instance)
        {
            var mb = __instance as MonoBehaviour;
            if (mb == null) return;
            if (!IsP2LegController(mb)) return;

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

        public static bool CallbackContextFilter_Prefix(object __instance, ref UnityEngine.InputSystem.InputAction.CallbackContext __0)
        {
            if (!SplitScreenMod.IsSplitScreenActive) return true;
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
        public static bool CallbackContextFilter_Prefix(ref UnityEngine.InputSystem.InputAction.CallbackContext __0)
        {
            if (!SplitScreenMod.IsSplitScreenActive) return true;
            if (!SplitScreenMod.FilterP1FromP2Gamepad) return true;
            if (!SplitScreenMod.P2UseGamepad) return true;

            if (InputCompat.IsCallbackContextFromP2Gamepad(__0, SplitScreenMod.P2GamepadIndex))
                return false;

            return true;
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

    // Redirect CameraController.MainCamera to P2's camera while in P2 web context.
    internal static class CameraControllerMainCameraPatches
    {
        private static int _p2Hits;
        public static bool MainCamera_Prefix(ref Camera __result)
        {
            if (!(SplitScreenMod.P2ShootHeld || SplitScreenMod.InP2WebContext)) return true;
            if (SplitScreenMod.P2Camera == null) return true;
            __result = SplitScreenMod.P2Camera;
            if (_p2Hits < 5)
            {
                _p2Hits++;
                MelonLogger.Msg("[CameraControllerMainCamera_Prefix] redirected to P2 cam (#" + _p2Hits + ") name=" + SplitScreenMod.P2Camera.name);
            }
            return false;
        }

        public static bool GetCameraDistance_Prefix(ref float __result)
        {
            if (!(SplitScreenMod.P2ShootHeld || SplitScreenMod.InP2WebContext)) return true;
            if (SplitScreenMod.P2Camera == null) return true;

            var pivot = SplitScreenMod.P2InputTransform;
            if (pivot != null)
            {
                __result = Mathf.Max(Vector3.Distance(pivot.position, SplitScreenMod.P2Camera.transform.position), 0.01f);
                return false;
            }

            __result = Mathf.Max(SplitScreenMod.P2CameraDistance, 0.01f);
            return false;
        }
    }

    internal static class GameplayUIPatches
    {
        private static FieldInfo _crossHairsField;
        private static FieldInfo _crossHairsImageField;
        private static FieldInfo _webTargetActiveColorField;
        private static FieldInfo _noWebTargetActiveColorField;
        private static PropertyInfo _uiEnabledProperty;
        private static PropertyInfo _uiColorProperty;

        private static RectTransform _trackedCrossHairRect;
        private static Vector2 _originalAnchorMin;
        private static Vector2 _originalAnchorMax;
        private static Vector2 _originalPivot;
        private static Vector2 _originalAnchoredPosition;
        private static GameObject _p2CrossHairObject;
        private static RectTransform _p2CrossHairRect;
        private static Component _p2CrossHairImage;
        private static GameObject _trackedCrossHairObject;

        public static void Update_Postfix(object __instance)
        {
            if (__instance == null)
                return;

            RectTransform crossHairRect;
            GameObject crossHairObject;
            Component crossHairImage;
            if (!TryGetCrossHairParts(__instance, out crossHairRect, out crossHairObject, out crossHairImage) || crossHairRect == null)
                return;

            CaptureOriginalLayoutIfNeeded(crossHairRect);

            if (!SplitScreenMod.IsSplitScreenActive)
            {
                RestoreOriginalLayout(crossHairRect);
                DestroyP2CrossHair();
                return;
            }

            var p1Camera = SplitScreenMod.P1Camera;
            var p2Camera = SplitScreenMod.P2Camera;
            if (p1Camera == null || p2Camera == null)
                return;

            var parentRect = crossHairRect.parent as RectTransform;
            if (parentRect == null)
                return;

            PositionCrossHair(crossHairRect, parentRect, p1Camera.rect);

            EnsureP2CrossHair(crossHairObject, crossHairImage);
            if (_p2CrossHairRect == null)
                return;

            PositionCrossHair(_p2CrossHairRect, parentRect, p2Camera.rect);
            SyncP2CrossHairState(__instance, crossHairObject, crossHairImage);
        }

        private static bool TryGetCrossHairParts(object gameplayUi, out RectTransform rectTransform, out GameObject crossHairObject, out Component crossHairImage)
        {
            rectTransform = null;
            crossHairObject = null;
            crossHairImage = null;

            if (_crossHairsField == null)
                _crossHairsField = gameplayUi.GetType().GetField("crossHairs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (_crossHairsField != null)
                crossHairObject = _crossHairsField.GetValue(gameplayUi) as GameObject;

            if (crossHairObject != null)
                rectTransform = crossHairObject.GetComponent<RectTransform>();

            if (_crossHairsImageField == null)
                _crossHairsImageField = gameplayUi.GetType().GetField("crossHairsImage", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (_crossHairsImageField != null)
            {
                crossHairImage = _crossHairsImageField.GetValue(gameplayUi) as Component;
                if (crossHairImage != null)
                {
                    if (rectTransform == null)
                        rectTransform = crossHairImage.transform as RectTransform;
                }
            }

            if (crossHairObject != null)
            {
                if (crossHairImage == null)
                    crossHairImage = FindUiImageComponent(crossHairObject);
            }

            if (crossHairObject == null && rectTransform != null)
                crossHairObject = rectTransform.gameObject;

            return rectTransform != null && crossHairObject != null;
        }

        private static void CaptureOriginalLayoutIfNeeded(RectTransform rectTransform)
        {
            if (_trackedCrossHairRect == rectTransform)
                return;

            _trackedCrossHairRect = rectTransform;
            _originalAnchorMin = rectTransform.anchorMin;
            _originalAnchorMax = rectTransform.anchorMax;
            _originalPivot = rectTransform.pivot;
            _originalAnchoredPosition = rectTransform.anchoredPosition;
        }

        private static void RestoreOriginalLayout(RectTransform rectTransform)
        {
            if (_trackedCrossHairRect != rectTransform)
                return;

            rectTransform.anchorMin = _originalAnchorMin;
            rectTransform.anchorMax = _originalAnchorMax;
            rectTransform.pivot = _originalPivot;
            rectTransform.anchoredPosition = _originalAnchoredPosition;
        }

        private static void PositionCrossHair(RectTransform rectTransform, RectTransform parentRect, Rect cameraRect)
        {
            var normalizedCenter = new Vector2(
                cameraRect.x + cameraRect.width * 0.5f,
                cameraRect.y + cameraRect.height * 0.5f);
            var localPoint = new Vector2(
                (normalizedCenter.x - 0.5f) * parentRect.rect.width,
                (normalizedCenter.y - 0.5f) * parentRect.rect.height);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = localPoint;
        }

        private static void EnsureP2CrossHair(GameObject sourceCrossHairObject, Component sourceCrossHairImage)
        {
            if (sourceCrossHairObject == null)
                return;

            if (_p2CrossHairObject != null && _trackedCrossHairObject != sourceCrossHairObject)
                DestroyP2CrossHair();

            if (_p2CrossHairObject != null)
                return;

            _trackedCrossHairObject = sourceCrossHairObject;
            _p2CrossHairObject = UnityEngine.Object.Instantiate(sourceCrossHairObject, sourceCrossHairObject.transform.parent);
            _p2CrossHairObject.name = sourceCrossHairObject.name + "_P2";
            _p2CrossHairObject.transform.SetSiblingIndex(sourceCrossHairObject.transform.GetSiblingIndex() + 1);
            _p2CrossHairRect = _p2CrossHairObject.GetComponent<RectTransform>();
            if (_p2CrossHairRect == null)
                _p2CrossHairRect = _p2CrossHairObject.GetComponentInChildren<RectTransform>(true);
            _p2CrossHairImage = FindUiImageComponent(_p2CrossHairObject, sourceCrossHairImage != null ? sourceCrossHairImage.GetType() : null);
        }

        private static void SyncP2CrossHairState(object gameplayUi, GameObject sourceCrossHairObject, Component sourceCrossHairImage)
        {
            if (_p2CrossHairObject == null)
                return;

            _p2CrossHairObject.SetActive(sourceCrossHairObject != null && sourceCrossHairObject.activeSelf);

            bool enabled = GetUiComponentEnabled(sourceCrossHairImage);
            SetUiComponentEnabled(_p2CrossHairImage, enabled);

            var color = SplitScreenMod.P2WebTargetActive
                ? GetColorField(gameplayUi, ref _webTargetActiveColorField, "webTargetActiveColor")
                : GetColorField(gameplayUi, ref _noWebTargetActiveColorField, "noWebTargetActiveColor");
            SetUiComponentColor(_p2CrossHairImage, color);
        }

        private static void DestroyP2CrossHair()
        {
            if (_p2CrossHairObject != null)
                UnityEngine.Object.Destroy(_p2CrossHairObject);

            _p2CrossHairObject = null;
            _p2CrossHairRect = null;
            _p2CrossHairImage = null;
            _trackedCrossHairObject = null;
        }

        private static Component FindUiImageComponent(GameObject root, Type preferredType = null)
        {
            if (root == null)
                return null;

            if (preferredType != null)
            {
                var preferred = root.GetComponentInChildren(preferredType, true) as Component;
                if (preferred != null)
                    return preferred;
            }

            var components = root.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                if (component == null)
                    continue;

                var fullName = component.GetType().FullName;
                if (string.Equals(fullName, "UnityEngine.UI.Image", StringComparison.Ordinal))
                    return component;
            }

            return null;
        }

        private static bool GetUiComponentEnabled(Component component)
        {
            if (component == null)
                return false;

            if (_uiEnabledProperty == null || _uiEnabledProperty.DeclaringType != component.GetType())
                _uiEnabledProperty = component.GetType().GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public);

            if (_uiEnabledProperty != null && _uiEnabledProperty.CanRead)
                return (bool)_uiEnabledProperty.GetValue(component, null);

            return component.gameObject.activeSelf;
        }

        private static void SetUiComponentEnabled(Component component, bool enabled)
        {
            if (component == null)
                return;

            if (_uiEnabledProperty == null || _uiEnabledProperty.DeclaringType != component.GetType())
                _uiEnabledProperty = component.GetType().GetProperty("enabled", BindingFlags.Instance | BindingFlags.Public);

            if (_uiEnabledProperty != null && _uiEnabledProperty.CanWrite)
                _uiEnabledProperty.SetValue(component, enabled, null);
        }

        private static void SetUiComponentColor(Component component, Color color)
        {
            if (component == null)
                return;

            if (_uiColorProperty == null || _uiColorProperty.DeclaringType != component.GetType())
                _uiColorProperty = component.GetType().GetProperty("color", BindingFlags.Instance | BindingFlags.Public);

            if (_uiColorProperty != null && _uiColorProperty.CanWrite)
                _uiColorProperty.SetValue(component, color, null);
        }

        private static Color GetColorField(object gameplayUi, ref FieldInfo field, string fieldName)
        {
            if (field == null)
                field = gameplayUi.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field != null)
                return (Color)field.GetValue(gameplayUi);

            return Color.white;
        }
    }

    // While invoking WebController methods on behalf of P2, suppress P1's MainWebVisuals
    // event handlers so they don't draw a line from P1's body to P2's web target.
    internal static class MainWebVisualsPatches
    {
        public static bool OnMainWebActivated_Prefix()
        {
            return !SplitScreenMod.InP2WebContext;
        }

        public static bool OnMainWebDeactivated_Prefix()
        {
            return !SplitScreenMod.InP2WebContext;
        }
    }

    // Drives P2's underwater state. Original BodyMovement.SetIsUnderwater early-returns
    // when isPlayer==false (which P2 always is), so P2 entering water plays no audio.
    // The prefix runs custom logic for P2's BodyMovement instance, then lets the
    // original method continue (it'll early-return). For P1, the prefix is a no-op.
    internal static class BodyMovementUnderwaterPatches
    {
        public static FieldInfo IsUnderwaterField;

        private static int _p2Counter;
        private static bool _p2Underwater;

        public static bool P2IsUnderwater { get { return _p2Underwater; } }

        public static bool SetIsUnderwater_Prefix(object __instance, bool value)
        {
            try
            {
                var p2bm = SplitScreenMod.P2BodyMovementInstance;
                if (p2bm == null || !object.ReferenceEquals(__instance, p2bm))
                    return true; // not P2 — let original run normally

                // P2's water transition. Mirror the counter logic from the original.
                _p2Counter += (value ? 1 : -1);
                if (_p2Counter < 0) _p2Counter = 0;

                if (!_p2Underwater && _p2Counter > 0)
                {
                    _p2Underwater = true;
                    // Only start the ambience if it isn't already running because of P1.
                    if (!P1IsUnderwater())
                        InvokeMusicControllerMethod("StartUnderwater");
                }
                else if (_p2Underwater && _p2Counter == 0)
                {
                    _p2Underwater = false;
                    if (!P1IsUnderwater())
                        InvokeMusicControllerMethod("StopUnderwater");
                }
            }
            catch { }
            return true; // always let original run; it early-returns for non-player
        }

        private static bool P1IsUnderwater()
        {
            try
            {
                var p1bm = SplitScreenMod.P1BodyMovementInstance;
                if (p1bm == null || IsUnderwaterField == null) return false;
                var v = IsUnderwaterField.GetValue(p1bm);
                return v is bool b && b;
            }
            catch { return false; }
        }

        private static object SingletonInstance(Type singletonTargetType)
        {
            if (singletonTargetType == null) return null;
            try
            {
                var asm = singletonTargetType.Assembly;
                var generic = asm.GetType("_Scripts.Singletons.Singleton`1");
                if (generic == null) generic = AccessTools.TypeByName("_Scripts.Singletons.Singleton`1");
                if (generic == null) return null;
                var closed = generic.MakeGenericType(singletonTargetType);
                var prop = closed.GetProperty("Instance",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null) return prop.GetValue(null, null);
            }
            catch { }
            return null;
        }

        public static void Reset()
        {
            bool shouldStopUnderwater = _p2Underwater && !P1IsUnderwater();
            _p2Counter = 0;
            _p2Underwater = false;

            if (shouldStopUnderwater)
                InvokeMusicControllerMethod("StopUnderwater");
        }

        private static void InvokeMusicControllerMethod(string methodName)
        {
            try
            {
                var mcType = AccessTools.TypeByName("_Scripts.Singletons.MusicController");
                var inst = SingletonInstance(mcType);
                if (inst == null)
                    return;

                var method = mcType.GetMethod(methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null);
                if (method != null)
                    method.Invoke(inst, null);
            }
            catch { }
        }
    }

    internal static class MusicControllerUnderwaterPatches
    {
        public static bool StopUnderwater_Prefix()
        {
            // If P2 is still underwater, don't kill the loop just because P1 left water.
            if (BodyMovementUnderwaterPatches.P2IsUnderwater)
            {
                return false;
            }
            return true;
        }
    }

    // Prefix on WebThread.DeleteWebThread. Whenever a WebThread is about to be
    // destroyed (including via SplitWebThread/QuickBuild/FixedAnchor/MovingAnchor/
    // DeleteWeb/DestroyAll), make sure any P2 transforms parented to it are
    // re-parented away first. BodyMovement.PerformWalking parents the spider
    // GameObject and its targetTransform to the walked-on surface; if that surface
    // is a WebThread being destroyed, Unity would destroy the P2 spider with it.
    internal static class P2DestroyDetachHelper
    {
        private static FieldInfo _targetTransformField;
        private static PropertyInfo _targetTransformProp;
        private static FieldInfo _targetRigidbodyField;
        private static FieldInfo _targetMaterialField;
        private static FieldInfo _targetMovableObjectField;
        private static FieldInfo _oldTargetTransformParentField;
        private static Type _cachedBodyMovementType;

        private static void CacheBodyMovementMembers(Type bmType)
        {
            if (bmType == null || object.ReferenceEquals(_cachedBodyMovementType, bmType))
                return;

            _cachedBodyMovementType = bmType;
            _targetTransformProp = bmType.GetProperty("TargetTransform",
                BindingFlags.Public | BindingFlags.Instance);
            _targetTransformField = bmType.GetField("targetTransform",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _targetRigidbodyField = bmType.GetField("targetRigidbody",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _targetMaterialField = bmType.GetField("targetMaterial",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _targetMovableObjectField = bmType.GetField("targetMovableObject",
                BindingFlags.NonPublic | BindingFlags.Instance);
            _oldTargetTransformParentField = bmType.GetField("oldTargetTransformParent",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static Transform GetTargetTransform(Component p2bm)
        {
            if (p2bm == null)
                return null;

            var bmType = p2bm.GetType();
            CacheBodyMovementMembers(bmType);

            try
            {
                if (_targetTransformProp != null)
                    return _targetTransformProp.GetValue(p2bm, null) as Transform;
            }
            catch { }

            try
            {
                if (_targetTransformField != null)
                    return _targetTransformField.GetValue(p2bm) as Transform;
            }
            catch { }

            return null;
        }

        public static void DetachFromDestroyedRoot(Transform deletedRoot)
        {
            if (deletedRoot == null)
                return;

            try
            {
                var p2bm = SplitScreenMod.P2BodyMovementInstance;
                Transform spiderT = (p2bm != null) ? p2bm.transform : null;
                if (spiderT == null)
                    return;

                // Ignore teardown of P2's own hierarchy; this fix is only for external
                // surfaces that temporarily become the walking parent.
                if (object.ReferenceEquals(deletedRoot, spiderT) || deletedRoot.IsChildOf(spiderT))
                    return;

                var targetT = GetTargetTransform(p2bm);
                bool spiderWillBeDestroyed = spiderT.IsChildOf(deletedRoot);
                bool targetWillBeDestroyed = targetT != null && targetT.IsChildOf(deletedRoot);
                if (!spiderWillBeDestroyed && !targetWillBeDestroyed)
                    return;

                if (spiderWillBeDestroyed)
                    spiderT.SetParent(null, true);

                if (targetWillBeDestroyed)
                    targetT.SetParent(spiderT != null ? spiderT : null, true);

                var bmType = p2bm.GetType();
                CacheBodyMovementMembers(bmType);

                if (_targetRigidbodyField != null)
                    _targetRigidbodyField.SetValue(p2bm, null);
                if (_targetMaterialField != null)
                    _targetMaterialField.SetValue(p2bm, null);
                if (_targetMovableObjectField != null)
                    _targetMovableObjectField.SetValue(p2bm, null);
                if (_oldTargetTransformParentField != null)
                    _oldTargetTransformParentField.SetValue(p2bm, targetT != null ? targetT.parent : null);
            }
            catch
            {
                // Defensive: never block the original destroy path.
            }
        }

        public static object GetTargetMovableObject(Component bodyMovement)
        {
            if (bodyMovement == null)
                return null;

            var bmType = bodyMovement.GetType();
            CacheBodyMovementMembers(bmType);

            try
            {
                return _targetMovableObjectField != null ? _targetMovableObjectField.GetValue(bodyMovement) : null;
            }
            catch
            {
                return null;
            }
        }
    }

    internal static class P2MovableCollisionHelper
    {
        private static object _trackedMovableObject;
        private static Type _cachedMovableObjectType;
        private static FieldInfo _collidersField;
        private static int _p2Layer = int.MinValue;
        private static readonly Dictionary<Collider, int> _originalExcludeMasks = new Dictionary<Collider, int>();

        private static int GetP2ExcludeMask()
        {
            if (_p2Layer == int.MinValue)
                _p2Layer = LayerMask.NameToLayer("Ignore Raycast");

            if (_p2Layer < 0)
                return 0;

            return 1 << _p2Layer;
        }

        private static void CacheMovableMembers(object movableObject)
        {
            if (movableObject == null)
                return;

            var movableType = movableObject.GetType();
            if (ReferenceEquals(_cachedMovableObjectType, movableType))
                return;

            _cachedMovableObjectType = movableType;
            _collidersField = movableType.GetField("colliders",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static void ApplyP2CollisionExclusion(object movableObject, bool exclude)
        {
            int p2ExcludeMask = GetP2ExcludeMask();
            if (p2ExcludeMask == 0 || movableObject == null)
                return;

            CacheMovableMembers(movableObject);
            if (_collidersField == null)
                return;

            try
            {
                var colliders = _collidersField.GetValue(movableObject) as System.Collections.IEnumerable;
                if (colliders == null)
                    return;

                foreach (var entry in colliders)
                {
                    var collider = entry as Collider;
                    if (collider == null)
                        continue;

                    if (exclude)
                    {
                        if (!_originalExcludeMasks.ContainsKey(collider))
                            _originalExcludeMasks.Add(collider, collider.excludeLayers.value);
                        collider.excludeLayers = collider.excludeLayers | p2ExcludeMask;
                    }
                    else
                    {
                        int originalMask;
                        if (_originalExcludeMasks.TryGetValue(collider, out originalMask))
                        {
                            collider.excludeLayers = originalMask;
                            _originalExcludeMasks.Remove(collider);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static void SyncForBodyMovement(Component bodyMovement)
        {
            if (!SplitScreenMod.IsSplitScreenActive ||
                bodyMovement == null ||
                !ReferenceEquals(bodyMovement, SplitScreenMod.P2BodyMovementInstance))
            {
                Reset();
                return;
            }

            object currentMovableObject = P2DestroyDetachHelper.GetTargetMovableObject(bodyMovement);
            if (!ReferenceEquals(currentMovableObject, _trackedMovableObject))
            {
                ApplyP2CollisionExclusion(_trackedMovableObject, false);
                _trackedMovableObject = currentMovableObject;
            }

            ApplyP2CollisionExclusion(_trackedMovableObject, _trackedMovableObject != null);
        }

        public static void Reset()
        {
            foreach (var pair in _originalExcludeMasks)
            {
                if (pair.Key != null)
                    pair.Key.excludeLayers = pair.Value;
            }
            _originalExcludeMasks.Clear();
            _trackedMovableObject = null;
        }
    }

    internal static class WebThreadDeletePatches
    {
        public static void DeleteWebThread_Prefix(MonoBehaviour __instance)
        {
            if (__instance == null) return;
            P2DestroyDetachHelper.DetachFromDestroyedRoot(__instance.transform);
        }
    }

    internal static class UnityDestroyDetachPatches
    {
        private static Transform ResolveDestroyedRoot(UnityEngine.Object obj)
        {
            if (obj is GameObject go)
                return go.transform;

            if (obj is Transform tr)
                return tr;

            return null;
        }

        public static void Destroy_Prefix(UnityEngine.Object obj, float t)
        {
            if (t > 0f)
                return;

            var deletedRoot = ResolveDestroyedRoot(obj);
            if (deletedRoot != null)
                P2DestroyDetachHelper.DetachFromDestroyedRoot(deletedRoot);
        }

        public static void DestroyImmediate_Prefix(UnityEngine.Object obj, bool allowDestroyingAssets)
        {
            var deletedRoot = ResolveDestroyedRoot(obj);
            if (deletedRoot != null)
                P2DestroyDetachHelper.DetachFromDestroyedRoot(deletedRoot);
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
        private static PropertyInfo _leftStickButtonProp;
        private static PropertyInfo _rightStickButtonProp;

        private static MethodInfo _controlReadValueVec2;
        private static MethodInfo _controlReadValueFloat;
        private static PropertyInfo _buttonWasPressedThisFrame;

        // CallbackContext reflection
        private static PropertyInfo _ctxControlProp;
        private static PropertyInfo _controlDeviceProp;
        private static PropertyInfo _inputDeviceDeviceIdProp;

        private static Type _readOnlyArrayRuntimeType;
        private static PropertyInfo _readOnlyArrayRuntimeCountProp;
        private static MethodInfo _readOnlyArrayRuntimeGetItem;
        private static readonly object[] _readOnlyArrayIndexArgs = new object[1];
        private static object[] _gamepadSnapshot = Array.Empty<object>();
        private static int _gamepadSnapshotCount;
        private static int _gamepadSnapshotFrame = -1;

        private static int _boundP2Index = int.MinValue;
        private static int _boundP2DeviceId = int.MinValue;
        private static object _boundP2Gamepad;
        private static int _boundP2ResolvedFrame = -1;
        private static object _boundP2GamepadThisFrame;
        private static int _boundP2MissingFrames;
        private const int P2RebindAfterMissingFrames = 120;

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
                _leftStickButtonProp = _gamepadType.GetProperty("leftStickButton", BindingFlags.Public | BindingFlags.Instance);
                _rightStickButtonProp = _gamepadType.GetProperty("rightStickButton", BindingFlags.Public | BindingFlags.Instance);

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

        public static bool Down_F7() { return Down("f7Key", KeyCode.F7); }
        public static bool Down_F8() { return Down("f8Key", KeyCode.F8); }
        public static bool Down_F9() { return Down("f9Key", KeyCode.F9); }
        public static bool Down_F10() { return Down("f10Key", KeyCode.F10); }

        public static bool Held_I() { return Held("iKey", KeyCode.I); }
        public static bool Held_J() { return Held("jKey", KeyCode.J); }
        public static bool Held_K() { return Held("kKey", KeyCode.K); }
        public static bool Held_L() { return Held("lKey", KeyCode.L); }

        public static bool Held_N() { return Held("nKey", KeyCode.N); }
        public static bool Held_M() { return Held("mKey", KeyCode.M); }

        private static void RefreshGamepadSnapshot()
        {
            try
            {
                int frame = Time.frameCount;
                if (_gamepadSnapshotFrame == frame) return;

                _gamepadSnapshotFrame = frame;

                int oldCount = _gamepadSnapshotCount;
                _gamepadSnapshotCount = 0;

                if (_gamepadType == null || _gamepadAllProp == null) return;

                var ro = _gamepadAllProp.GetValue(null, null);
                if (ro == null) return;

                var roRuntimeType = ro.GetType();
                if (!object.ReferenceEquals(_readOnlyArrayRuntimeType, roRuntimeType))
                {
                    _readOnlyArrayRuntimeType = roRuntimeType;
                    _readOnlyArrayRuntimeCountProp = roRuntimeType.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
                    _readOnlyArrayRuntimeGetItem = roRuntimeType.GetMethod("get_Item", BindingFlags.Public | BindingFlags.Instance);
                }

                if (_readOnlyArrayRuntimeCountProp == null || _readOnlyArrayRuntimeGetItem == null) return;

                var countObj = _readOnlyArrayRuntimeCountProp.GetValue(ro, null);
                int count = countObj is int ? (int)countObj : 0;
                if (count <= 0)
                {
                    for (int i = 0; i < oldCount && i < _gamepadSnapshot.Length; i++)
                        _gamepadSnapshot[i] = null;
                    return;
                }

                if (_gamepadSnapshot.Length < count)
                    _gamepadSnapshot = new object[count];

                for (int i = 0; i < count; i++)
                {
                    _readOnlyArrayIndexArgs[0] = i;
                    _gamepadSnapshot[i] = _readOnlyArrayRuntimeGetItem.Invoke(ro, _readOnlyArrayIndexArgs);
                }

                for (int i = count; i < oldCount && i < _gamepadSnapshot.Length; i++)
                    _gamepadSnapshot[i] = null;

                _gamepadSnapshotCount = count;
            }
            catch
            {
                _gamepadSnapshotCount = 0;
            }
        }

        private static bool TryReadDeviceId(object device, out int deviceId)
        {
            deviceId = int.MinValue;

            try
            {
                if (device == null) return false;

                var devIdProp = _inputDeviceDeviceIdProp;
                if (devIdProp == null || !devIdProp.DeclaringType.IsAssignableFrom(device.GetType()))
                    devIdProp = device.GetType().GetProperty("deviceId", BindingFlags.Public | BindingFlags.Instance);
                if (devIdProp == null) return false;

                var raw = devIdProp.GetValue(device, null);
                if (raw is int)
                {
                    deviceId = (int)raw;
                    return true;
                }

                if (raw != null)
                    return int.TryParse(raw.ToString(), out deviceId);
            }
            catch { }

            return false;
        }

        private static bool SnapshotContainsReference(object gamepad)
        {
            if (gamepad == null) return false;

            for (int i = 0; i < _gamepadSnapshotCount; i++)
            {
                if (object.ReferenceEquals(_gamepadSnapshot[i], gamepad))
                    return true;
            }

            return false;
        }

        private static object FindSnapshotGamepadByDeviceId(int deviceId)
        {
            if (deviceId == int.MinValue) return null;

            for (int i = 0; i < _gamepadSnapshotCount; i++)
            {
                var gp = _gamepadSnapshot[i];
                int currentId;
                if (TryReadDeviceId(gp, out currentId) && currentId == deviceId)
                    return gp;
            }

            return null;
        }

        private static void ClearP2GamepadBinding()
        {
            _boundP2Index = int.MinValue;
            _boundP2DeviceId = int.MinValue;
            _boundP2Gamepad = null;
            _boundP2ResolvedFrame = -1;
            _boundP2GamepadThisFrame = null;
            _boundP2MissingFrames = 0;
        }

        private static object BindP2Gamepad(int index, object gamepad)
        {
            _boundP2Index = index;
            _boundP2Gamepad = gamepad;
            _boundP2DeviceId = int.MinValue;
            _boundP2MissingFrames = 0;

            int deviceId;
            if (TryReadDeviceId(gamepad, out deviceId))
                _boundP2DeviceId = deviceId;

            return gamepad;
        }

        private static object GetGamepadAtIndex(int index)
        {
            try
            {
                RefreshGamepadSnapshot();
                int count = _gamepadSnapshotCount;
                if (index < 0 || index >= count) return null;
                return _gamepadSnapshot[index];
            }
            catch { return null; }
        }

        public static int GetConnectedGamepadCount()
        {
            try
            {
                RefreshGamepadSnapshot();
                return _gamepadSnapshotCount;
            }
            catch { return 0; }
        }

        private static object GetP2Gamepad(int index)
        {
            try
            {
                // Bind P2 to the originally selected device so a temporary reshuffle of
                // Gamepad.all doesn't silently move P2 over to a different controller.
                if (_boundP2Index != index)
                    ClearP2GamepadBinding();

                int frame = Time.frameCount;
                if (_boundP2ResolvedFrame == frame)
                    return _boundP2GamepadThisFrame;

                RefreshGamepadSnapshot();

                object resolved = null;

                if (_boundP2DeviceId != int.MinValue)
                    resolved = FindSnapshotGamepadByDeviceId(_boundP2DeviceId);

                if (resolved == null && SnapshotContainsReference(_boundP2Gamepad))
                    resolved = _boundP2Gamepad;

                bool hasExistingBinding = _boundP2Gamepad != null || _boundP2DeviceId != int.MinValue;
                if (resolved == null)
                {
                    if (!hasExistingBinding)
                    {
                        resolved = BindP2Gamepad(index, GetGamepadAtIndex(index));
                    }
                    else if (++_boundP2MissingFrames >= P2RebindAfterMissingFrames)
                    {
                        resolved = BindP2Gamepad(index, GetGamepadAtIndex(index));
                    }
                }
                else
                {
                    _boundP2MissingFrames = 0;

                    int resolvedDeviceId;
                    if (_boundP2DeviceId == int.MinValue || !object.ReferenceEquals(resolved, _boundP2Gamepad))
                    {
                        if (TryReadDeviceId(resolved, out resolvedDeviceId))
                        {
                            _boundP2DeviceId = resolvedDeviceId;
                            _boundP2Gamepad = resolved;
                        }
                    }
                }

                _boundP2ResolvedFrame = frame;
                _boundP2GamepadThisFrame = resolved;
                return resolved;
            }
            catch
            {
                return null;
            }
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
            var gp = GetP2Gamepad(index);
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
            var gp = GetP2Gamepad(index);
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

            var gp = GetP2Gamepad(index);
            // RT is reserved for grapple/attach. Keep shoot on LT so the two systems
            // cannot fire simultaneously from the same trigger.
            float lt = ReadAxis(gp, _leftTriggerProp);
            return kb || (lt >= triggerThreshold);
        }

        public static bool IsP2JumpPressedNow(bool useGamepad, int index)
        {
            bool kb = Down("backslashKey", KeyCode.Backslash);
            if (!useGamepad) return kb;
            var gp = GetP2Gamepad(index);
            bool south = ReadButtonDown(gp, _buttonSouthProp);
            return kb || south;
        }

        // P1 jump bypass: Polls Keyboard.Space + any non-P2 gamepad's South button.
        // Needed because the shared jumpInputAction (used by both P1 and P2 BodyMovement
        // instances) gets stuck in the Performed phase while P2's South is actuated, so
        // P1's gamepad-South press doesn't fire a fresh `performed` callback.
        public static bool IsP1JumpPressedNow(int p2Index)
        {
            // Keyboard Space — vanilla path also covers this, but include for completeness
            // (it can never be blocked by gamepad phase issues anyway).
            if (Down("spaceKey", KeyCode.Space))
                return true;

            // Iterate gamepads 0..MAX via the proven GetGamepadAtIndex path
            // (don't reinvent ReadOnlyArray reflection — use the cached one).
            const int MaxGamepads = 8;
            var p2gp = GetP2Gamepad(p2Index);
            for (int i = 0; i < MaxGamepads; i++)
            {
                if (i == p2Index) continue;
                var gp = GetGamepadAtIndex(i);
                if (gp == null) continue;
                if (p2gp != null && object.ReferenceEquals(gp, p2gp)) continue;
                if (ReadButtonDown(gp, _buttonSouthProp)) return true;
            }
            return false;
        }

        public static bool IsP1ShootRTHeldNow(int p2Index, float triggerThreshold)
        {
            const int MaxGamepads = 8;
            var p2gp = GetP2Gamepad(p2Index);
            for (int i = 0; i < MaxGamepads; i++)
            {
                if (i == p2Index) continue;
                var gp = GetGamepadAtIndex(i);
                if (gp == null) continue;
                if (p2gp != null && object.ReferenceEquals(gp, p2gp)) continue;
                if (ReadAxis(gp, _rightTriggerProp) >= triggerThreshold)
                    return true;
            }
            return false;
        }

        public static bool IsP2InteractPressedNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = Down(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetP2Gamepad(index);
            bool west = ReadButtonDown(gp, _buttonWestProp);
            return kb || west;
        }

        public static bool IsP2AttachHeldNow(bool useGamepad, int index, float triggerThreshold, string kbProp, KeyCode kbFallback)
        {
            bool kb = Held(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetP2Gamepad(index);
            float rt = ReadAxis(gp, _rightTriggerProp);
            return kb || (rt >= triggerThreshold);
        }

        public static bool IsP2AttachPressedNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = Down(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetP2Gamepad(index);
            float rt = ReadAxis(gp, _rightTriggerProp);
            // Treat trigger as "pressed" when it crosses threshold — callers track edge themselves
            return kb || (rt >= 0.35f);
        }

        public static bool IsP2DeletePressedNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = Down(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetP2Gamepad(index);
            bool b = ReadButtonDown(gp, _buttonEastProp);
            return kb || b;
        }

        public static bool IsP2ReleasePressedNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = Down(kbProp, kbFallback);
            if (!useGamepad) return kb;

            var gp = GetP2Gamepad(index);
            bool rb = ReadButtonDown(gp, _rightShoulderProp);
            return kb || rb;
        }

        public static bool IsP2CameraZoomPressedNow(bool useGamepad, int index)
        {
            if (!useGamepad) return false;
            var gp = GetP2Gamepad(index);
            return ReadButtonDown(gp, _rightStickButtonProp);
        }

        public static bool IsP2SprintPressedNow(bool useGamepad, int index)
        {
            if (!useGamepad) return false;
            var gp = GetP2Gamepad(index);
            return ReadButtonDown(gp, _leftStickButtonProp);
        }

        // === New helpers for full P2 web abilities (Option B). All return "held" booleans;
        // callers track press/release edges. kbProp may be null to skip keyboard fallback. ===

        // RT (right trigger) → shoot/grapple (held)
        public static bool IsP2ShootRTHeldNow(bool useGamepad, int index, float triggerThreshold, string kbProp, KeyCode kbFallback)
        {
            bool kb = !string.IsNullOrEmpty(kbProp) && Held(kbProp, kbFallback);
            if (!useGamepad) return kb || (kbFallback != KeyCode.None && SafeKey(kbFallback));
            var gp = GetP2Gamepad(index);
            float rt = ReadAxis(gp, _rightTriggerProp);
            return kb || (rt >= triggerThreshold);
        }

        // LT (left trigger) → quick build (held)
        public static bool IsP2QuickBuildHeldNow(bool useGamepad, int index, float triggerThreshold, string kbProp, KeyCode kbFallback)
        {
            bool kb = !string.IsNullOrEmpty(kbProp) && Held(kbProp, kbFallback);
            if (!useGamepad) return kb || (kbFallback != KeyCode.None && SafeKey(kbFallback));
            var gp = GetP2Gamepad(index);
            float lt = ReadAxis(gp, _leftTriggerProp);
            return kb || (lt >= triggerThreshold);
        }

        // LB (left shoulder) → fixed anchor (held)
        public static bool IsP2FixedAnchorHeldNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = !string.IsNullOrEmpty(kbProp) && Held(kbProp, kbFallback);
            if (!useGamepad) return kb || (kbFallback != KeyCode.None && SafeKey(kbFallback));
            var gp = GetP2Gamepad(index);
            return kb || ReadButtonHeld(gp, _leftShoulderProp);
        }

        // RB (right shoulder) → moving anchor (held)
        public static bool IsP2MovingAnchorHeldNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = !string.IsNullOrEmpty(kbProp) && Held(kbProp, kbFallback);
            if (!useGamepad) return kb || (kbFallback != KeyCode.None && SafeKey(kbFallback));
            var gp = GetP2Gamepad(index);
            return kb || ReadButtonHeld(gp, _rightShoulderProp);
        }

        // B (buttonEast) → delete (held — P2WebManager tracks press/release edges itself)
        public static bool IsP2DeleteHeldNow(bool useGamepad, int index, string kbProp, KeyCode kbFallback)
        {
            bool kb = !string.IsNullOrEmpty(kbProp) && Held(kbProp, kbFallback);
            if (!useGamepad) return kb || (kbFallback != KeyCode.None && SafeKey(kbFallback));
            var gp = GetP2Gamepad(index);
            return kb || ReadButtonHeld(gp, _buttonEastProp);
        }

        private static bool SafeKey(KeyCode k)
        {
            try { return UnityEngine.Input.GetKey(k); } catch { return false; }
        }

        private static bool ReadButtonHeld(object gamepad, PropertyInfo buttonProp)
        {
            try
            {
                if (gamepad == null || buttonProp == null) return false;
                var btn = buttonProp.GetValue(gamepad, null);
                if (btn == null) return false;
                // ButtonControl.isPressed inherits from InputControl<float>; use AxisControl.ReadValue threshold
                if (_controlReadValueFloat != null)
                {
                    var v = _controlReadValueFloat.Invoke(btn, null);
                    if (v is float) return ((float)v) >= 0.5f;
                }
                return false;
            }
            catch { return false; }
        }

        public static bool IsCallbackContextFromP2Gamepad(object ctx, int p2Index)
        {
            try
            {
                if (ctx == null) return false;

                var gp = GetP2Gamepad(p2Index);
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

        public static string DescribeCallbackContext(object ctx)
        {
            try
            {
                if (ctx == null) return "null";

                PropertyInfo ctxControlProp = _ctxControlProp;
                if (ctxControlProp == null || !ctxControlProp.DeclaringType.IsAssignableFrom(ctx.GetType()))
                    ctxControlProp = ctx.GetType().GetProperty("control", BindingFlags.Public | BindingFlags.Instance);
                object control = ctxControlProp == null ? null : ctxControlProp.GetValue(ctx, null);
                if (control == null) return ctx.GetType().FullName + " control=null";

                PropertyInfo controlDeviceProp = _controlDeviceProp;
                if (controlDeviceProp == null || !controlDeviceProp.DeclaringType.IsAssignableFrom(control.GetType()))
                    controlDeviceProp = control.GetType().GetProperty("device", BindingFlags.Public | BindingFlags.Instance);
                object device = controlDeviceProp == null ? null : controlDeviceProp.GetValue(control, null);

                string controlPath = ReadStringProperty(control, "path");
                string controlName = ReadStringProperty(control, "name");
                string deviceName = device == null ? "null" : ReadStringProperty(device, "displayName");
                int deviceId;
                string id = TryReadDeviceId(device, out deviceId) ? deviceId.ToString() : "?";
                string value = TryReadContextVector2(ctx);

                return "control=" + controlName + " path=" + controlPath +
                    " device=" + deviceName + "#" + id + " value=" + value;
            }
            catch (Exception ex)
            {
                return "error:" + ex.GetType().Name + ":" + ex.Message;
            }
        }

        private static string ReadStringProperty(object instance, string propertyName)
        {
            if (instance == null) return "null";
            try
            {
                PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                object value = property == null ? null : property.GetValue(instance, null);
                return value == null ? "?" : value.ToString();
            }
            catch { return "?"; }
        }

        private static string TryReadContextVector2(object ctx)
        {
            try
            {
                MethodInfo[] methods = ctx.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo method = methods[i];
                    if (method.Name != "ReadValue" || !method.IsGenericMethodDefinition || method.GetParameters().Length != 0)
                        continue;
                    object value = method.MakeGenericMethod(typeof(Vector2)).Invoke(ctx, null);
                    if (value is Vector2)
                    {
                        Vector2 vector = (Vector2)value;
                        return "(" + vector.x.ToString("F3") + "," + vector.y.ToString("F3") + ")";
                    }
                }
            }
            catch { }
            return "?";
        }
    }

    internal static class CameraWaterTriggerPatches
    {
        public static void Start_Postfix(object __instance)
        {
            try
            {
                var type = __instance.GetType();
                var profilesField = AccessTools.Field(type, "globalVolumeProfiles");
                if (profilesField != null)
                {
                    var profiles = profilesField.GetValue(__instance) as UnityEngine.Rendering.VolumeProfile[];
                    if (profiles != null && profiles.Length > 0)
                    {
                        SplitScreenMod.TrackedWaterProfiles = profiles;
                    }
                }
            }
            catch { }
        }

        public static bool EnableUnderWaterPostProcessing_Prefix(object __instance, bool value)
        {
            var p2Bm = SplitScreenMod.P2BodyMovementInstance;
            if (p2Bm != null)
            {
                var p2Cam = SplitScreenMod.P2Camera;
                var trigger = __instance as UnityEngine.Component;
                if (trigger != null && p2Cam != null && trigger.gameObject == p2Cam.gameObject)
                {
                    SplitScreenMod.P2CameraUnderwater = value;
                    return false;
                }
                else
                {
                    SplitScreenMod.P1CameraUnderwater = value;
                    return false;
                }
            }
            return true;
        }
    }
}
