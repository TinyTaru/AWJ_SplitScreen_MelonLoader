using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.Rendering;

namespace AWJSplitScreenUpdateFix
{
    // Compatibility work for the updated game.  This is hosted by the main
    // split-screen MelonMod so the release remains one mod and one DLL.
    internal static class UpdateFixMod
    {
        private static HarmonyLib.Harmony harmony;
        private static Action<string> logInfo;
        private static Action<string> logError;
        private static Type bodyMovementType;
        private static FieldInfo rootField;
        private static FieldInfo ballField;
        private static FieldInfo rigidbodyField;
        private static FieldInfo visualRootField;
        private static FieldInfo npcSleepTimerField;
        private static FieldInfo isPlayerField;
        private static FieldInfo jumpTimerField;
        private static Type simpleShellType;
        private static Type spiderCustomizationType;
        private static FieldInfo[] spiderAppearanceSettings;
        private static MethodInfo refreshCustomizationMethod;
        private static FieldInfo[] simpleShellSettings;
        private static FieldInfo shellLengthField;
        private static FieldInfo shellCountField;
        private static FieldInfo shellColorField;
        private static FieldInfo shellRenderersField;
        private static MethodInfo setShellLengthMethod;
        private static MethodInfo setActiveShellCountMethod;
        private static MethodInfo updateShellColorsMethod;
        private static Type spiderFluffLodType;
        private static FieldInfo p2CameraField;
        private static FieldInfo p2InputTransformField;
        private static Type cameraControllerType;
        private static FieldInfo cameraInputTransformField;
        private static FieldInfo cameraLookInputField;
        private static Type carpetType;
        private static FieldInfo carpetWorldReferenceSizeField;
        private static FieldInfo carpetFullFluffScreenSizeField;
        private static FieldInfo carpetCullScreenSizeField;
        private static Type windParticleSystemType;
        private static FieldInfo windDeactivateThresholdField;
        private static FieldInfo windActivateThresholdField;
        private static FieldInfo windMaxVelocityField;
        private static Type splitScreenType;
        private static FieldInfo p2UseGamepadField;
        private static FieldInfo p2GamepadIndexField;
        private static FieldInfo p2DeadzoneField;
        private static FieldInfo debugSpeedLogField;
        private static Type inputCompatType;
        private static MethodInfo inputIsCallbackFromP2Method;
        private static MethodInfo inputGetP2RightStickMethod;
        private static PropertyInfo bodyStateProperty;
        private static Type webControllerType;
        private static FieldInfo webActiveField;
        private static FieldInfo webSpringJointField;
        private static FieldInfo webBodyMovementField;
        private static Component webControllerInstance;
        private static FieldInfo p2WebActiveField;
        private static Transform p2AirInputProxy;
        private static float lastP2AirDiagnosticTime = -999f;
        private static int p2BodyUpdateCount;
        private static int p2BodyFixedUpdateCount;
        private static int p2CameraRenderCount;
        private static int p1CameraRenderCount;
        private static int lastFrameDiagnosticFrame;
        private static int lastFrameDiagnosticP2Updates;
        private static int lastFrameDiagnosticP2FixedUpdates;
        private static int lastFrameDiagnosticP2Renders;
        private static int lastFrameDiagnosticP1Renders;
        private static float lastFrameDiagnosticTime = -999f;
        private static float lastSharedPhysicsDiagnosticTime = -999f;
        private static readonly List<Material> p2ShellMaterials = new List<Material>();
        private static int preparedP2Id;
        private static int preparedP2WindId;
        private static int preparedP2WindPlayerId;
        // Runtime testing proved that cycling WindParticleSystem_P2 itself is NOT
        // sufficient, while cycling PlayerSpider_P2 is.  That means the missing
        // initialization lives somewhere in the cloned player hierarchy (or in a
        // component that observes its OnDisable/OnEnable), not in the ParticleSystem.
        // Give each newly-created P2 root one clean inactive -> active lifecycle after
        // the split-screen mod has finished rewriting the clone.
        private static GameObject pendingP2PlayerLifecycle;
        private static int pendingP2PlayerDisableFrame = -1;
        private static int pendingP2PlayerEnableFrame = -1;
        private static int completedP2PlayerLifecycleId;
        // The P2 effect may be detached from the cloned camera (or parented to P2 in
        // the fallback path), so retain the exact object rather than rediscovering it.
        private static Component p2WindSystem;
        private static int diagnosticP2Id;
        private static int lastDistanceBand = -1;
        private static string lastVisualState;
        private static readonly int PlayerPositionShaderId = Shader.PropertyToID("_PlayerPosition");
        private static readonly int ShellCountShaderId = Shader.PropertyToID("_ShellCount");
        private static readonly int ShellIndexShaderId = Shader.PropertyToID("_ShellIndex");
        private static readonly int ShellColorShaderId = Shader.PropertyToID("_ShellColor");
        private static readonly int ColorShaderId = Shader.PropertyToID("_Color");
        private static readonly int BodyColorShaderId = Shader.PropertyToID("_BodyColor");
        private static readonly int LegColorShaderId = Shader.PropertyToID("_LegColor");
        private static readonly int JointColorShaderId = Shader.PropertyToID("_JointColor");

        internal static void Initialize(Action<string> info, Action<string> error)
        {
            logInfo = info ?? delegate { };
            logError = error ?? delegate { };
            harmony = new HarmonyLib.Harmony("AWJ.SplitScreen.UpdateFix.v230");
            bodyMovementType = AccessTools.TypeByName("_Scripts.Spider.BodyMovement");
            if (bodyMovementType == null)
            {
                logError("Could not find BodyMovement; the update fix is inactive.");
                return;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            rootField = bodyMovementType.GetField("root", flags);
            ballField = bodyMovementType.GetField("ball", flags);
            rigidbodyField = bodyMovementType.GetField("rb", flags);
            visualRootField = bodyMovementType.GetField("visualRoot", flags);
            npcSleepTimerField = bodyMovementType.GetField("npcSleepTimer", flags);
            isPlayerField = bodyMovementType.GetField("isPlayer", flags);
            jumpTimerField = bodyMovementType.GetField("jumpTimer", flags);
            bodyStateProperty = bodyMovementType.GetProperty("State", flags);
            // WebController became a forwarding facade in the multiplayer update.
            // The live swing state is now owned by WebBuilder; using the facade here
            // leaves webActive/springJoint null and makes P2 fall as if unattached.
            webControllerType = AccessTools.TypeByName("_Scripts.Web.WebBuilder")
                ?? AccessTools.TypeByName("_Scripts.Singletons.WebController");
            webActiveField = webControllerType == null ? null : webControllerType.GetField("webActive", flags);
            webSpringJointField = webControllerType == null ? null : webControllerType.GetField("springJoint", flags);
            webBodyMovementField = webControllerType == null ? null : webControllerType.GetField("bodyMovement", flags);

            windParticleSystemType = AccessTools.TypeByName("_Scripts.Effects.WindParticleSystem");
            if (windParticleSystemType != null)
            {
                windDeactivateThresholdField = windParticleSystemType.GetField("windDeactivateThreshold", flags);
                windActivateThresholdField = windParticleSystemType.GetField("windActivateThreshold", flags);
                windMaxVelocityField = windParticleSystemType.GetField("maxWindVelocity", flags);
            }

            simpleShellType = AccessTools.TypeByName("SimpleShell");
            if (simpleShellType != null)
            {
                simpleShellSettings = simpleShellType.GetFields(flags);
                shellLengthField = simpleShellType.GetField("shellLength", flags);
                shellCountField = simpleShellType.GetField("shellCount", flags);
                shellColorField = simpleShellType.GetField("shellColor", flags);
                shellRenderersField = simpleShellType.GetField("mrs", flags);
                setShellLengthMethod = simpleShellType.GetMethod("SetLength", flags);
                setActiveShellCountMethod = simpleShellType.GetMethod("SetActiveShellCount", flags);
                updateShellColorsMethod = simpleShellType.GetMethod("UpdateColors", flags);
            }
            spiderFluffLodType = AccessTools.TypeByName("_Scripts.Fluffy.SpiderFluffLod");

            spiderCustomizationType = AccessTools.TypeByName("_Scripts.Spider.SpiderCustomization");
            if (spiderCustomizationType != null)
            {
                spiderAppearanceSettings = new[]
                {
                    spiderCustomizationType.GetField("bodyEnabled", flags),
                    spiderCustomizationType.GetField("bodyFluffiness", flags),
                    spiderCustomizationType.GetField("bodyColor", flags),
                    spiderCustomizationType.GetField("abdomenEnabled", flags),
                    spiderCustomizationType.GetField("abdomenFluffiness", flags),
                    spiderCustomizationType.GetField("abdomenColor", flags),
                    spiderCustomizationType.GetField("legSegmentFluffiness", flags),
                    spiderCustomizationType.GetField("legColors", flags),
                    spiderCustomizationType.GetField("legsEnabled", flags),
                    spiderCustomizationType.GetField("jointSegmentFluffiness", flags),
                    spiderCustomizationType.GetField("jointColors", flags),
                    spiderCustomizationType.GetField("eyeIndex", flags),
                    spiderCustomizationType.GetField("eyeColorBase", flags),
                    spiderCustomizationType.GetField("eyeColorLeft", flags),
                    spiderCustomizationType.GetField("eyeColorRight", flags),
                    spiderCustomizationType.GetField("eyeEffects", flags),
                    spiderCustomizationType.GetField("hatIndex", flags),
                    spiderCustomizationType.GetField("hatColors", flags),
                    spiderCustomizationType.GetField("hatEffects", flags),
                    spiderCustomizationType.GetField("accessoryIndex", flags),
                    spiderCustomizationType.GetField("accessoryColors", flags),
                    spiderCustomizationType.GetField("accessoryEffects", flags),
                    spiderCustomizationType.GetField("shoeIndex", flags),
                    spiderCustomizationType.GetField("shoeColors", flags),
                    spiderCustomizationType.GetField("shoeEffects", flags)
                };
                refreshCustomizationMethod = spiderCustomizationType.GetMethod("Refresh", flags);
            }

            splitScreenType = AccessTools.TypeByName("AWJSplitScreen.SplitScreenMod");
            if (splitScreenType != null)
            {
                p2CameraField = splitScreenType.GetField("P2Camera", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                p2InputTransformField = splitScreenType.GetField("P2InputTransform", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                p2UseGamepadField = splitScreenType.GetField("P2UseGamepad", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                p2GamepadIndexField = splitScreenType.GetField("P2GamepadIndex", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                p2DeadzoneField = splitScreenType.GetField("P2Deadzone", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                debugSpeedLogField = splitScreenType.GetField("DebugSpeedLog", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                p2WebActiveField = splitScreenType.GetField("P2WebActive", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                inputCompatType = AccessTools.TypeByName("AWJSplitScreen.InputCompat");
                if (inputCompatType != null)
                {
                    inputIsCallbackFromP2Method = AccessTools.Method(inputCompatType, "IsCallbackContextFromP2Gamepad");
                    inputGetP2RightStickMethod = AccessTools.Method(inputCompatType, "GetP2RightStick");
                }
            }

            cameraControllerType = AccessTools.TypeByName("_Scripts.Singletons.CameraController");
            cameraInputTransformField = cameraControllerType == null ? null : cameraControllerType.GetField("inputTransform", flags);
            carpetType = AccessTools.TypeByName("_Scripts.Office.Carpet");
            if (carpetType != null)
            {
                carpetWorldReferenceSizeField = carpetType.GetField("worldReferenceSize", flags);
                carpetFullFluffScreenSizeField = carpetType.GetField("fullFluffScreenSize", flags);
                carpetCullScreenSizeField = carpetType.GetField("cullScreenSize", flags);
                MethodInfo computeFluffFactor = AccessTools.Method(carpetType, "ComputeFluffFactor");
                if (computeFluffFactor != null)
                {
                    harmony.Patch(computeFluffFactor, postfix: new HarmonyMethod(typeof(UpdateFixMod), "CarpetComputeFluffFactorPostfix"));
                }
            }

            harmony.Patch(
                AccessTools.Method(bodyMovementType, "FixedUpdate"),
                prefix: new HarmonyMethod(typeof(UpdateFixMod), "BodyMovementFixedUpdatePrefix"),
                postfix: new HarmonyMethod(typeof(UpdateFixMod), "BodyMovementFixedUpdatePostfix"));
            harmony.Patch(
                AccessTools.Method(bodyMovementType, "Update"),
                postfix: new HarmonyMethod(typeof(UpdateFixMod), "BodyMovementUpdatePostfix"));

            Type cameraMouseLookType = AccessTools.TypeByName("_Scripts.Camera.CameraMouseLook");
            cameraLookInputField = cameraMouseLookType == null ? null : cameraMouseLookType.GetField("lookInput", flags);
            MethodInfo cameraMouseLookOnLook = cameraMouseLookType == null ? null : AccessTools.Method(cameraMouseLookType, "OnLook");
            if (cameraMouseLookOnLook != null)
            {
                HarmonyMethod p2LookIsolation = new HarmonyMethod(typeof(UpdateFixMod), "CameraMouseLookOnLookPrefix");
                p2LookIsolation.priority = HarmonyLib.Priority.First;
                harmony.Patch(cameraMouseLookOnLook, prefix: p2LookIsolation);
            }
            MethodInfo performJumping = AccessTools.Method(bodyMovementType, "PerformJumping");
            if (performJumping != null)
            {
                // The bundled split-screen mod replaces this entire routine with a copy from
                // the previous game version.  Remove only that obsolete prefix and let the
                // updated game routine run with P2's context temporarily substituted.
                harmony.Unpatch(performJumping, HarmonyPatchType.Prefix, "AWJ.SplitScreen.P2Inject.v022");
                harmony.Patch(
                    performJumping,
                    prefix: new HarmonyMethod(typeof(UpdateFixMod), "BodyMovementPerformJumpingPrefix"),
                    postfix: new HarmonyMethod(typeof(UpdateFixMod), "BodyMovementPerformJumpingPostfix"),
                    finalizer: new HarmonyMethod(typeof(UpdateFixMod), "BodyMovementPerformJumpingFinalizer"));
            }

            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;

            logInfo("Loaded: P2 visuals/physics compatibility, isolated input/web state, smooth P2 visuals, two-camera carpet LOD, and P2 wind-trail patch.");
        }

        internal static void Update()
        {
            SyncP2VisualMode();
            EnsureP2MotionVisuals();
            EnsureP2WindTrail();
            UpdateP2ShellMaterialPosition();
            if (ReadStaticBool(debugSpeedLogField))
            {
                LogP2FrameCadence();
                LogSharedPhysicsState();
            }
        }

        internal static void LateUpdate()
        {
            AlignP2CameraWithSmoothedSpider();
        }

        internal static void Deinitialize()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            ResetP2VisualResources();
            if (p2AirInputProxy != null)
            {
                UnityEngine.Object.Destroy(p2AirInputProxy.gameObject);
                p2AirInputProxy = null;
            }
            if (harmony != null)
            {
                harmony.UnpatchSelf();
                harmony = null;
            }
        }

        internal static void ResetP2VisualResources()
        {
            ResetP2ShellMaterials();
            preparedP2Id = 0;
            preparedP2WindId = 0;
            preparedP2WindPlayerId = 0;
            pendingP2PlayerLifecycle = null;
            pendingP2PlayerDisableFrame = -1;
            pendingP2PlayerEnableFrame = -1;
            completedP2PlayerLifecycleId = 0;
            // The wind object is detached from the cloned camera so its FollowTarget can
            // track P2 without inheriting either camera transform. Destroy it explicitly
            // during teardown so it cannot survive into the next split-screen session.
            if (p2WindSystem != null)
            {
                UnityEngine.Object.Destroy(p2WindSystem.gameObject);
            }
            p2WindSystem = null;
            diagnosticP2Id = 0;
            lastDistanceBand = -1;
        }

        private static void AlignP2CameraWithSmoothedSpider()
        {
            if (p2CameraField == null || bodyMovementType == null || visualRootField == null || bodyStateProperty == null)
            {
                return;
            }

            GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
            Camera p2Camera = p2CameraField.GetValue(null) as Camera;
            if (playerTwo == null || p2Camera == null || !p2Camera.isActiveAndEnabled)
            {
                return;
            }

            try
            {
                Component p2Body = playerTwo.GetComponentInChildren(bodyMovementType, true);
                if (p2Body == null || !string.Equals(Convert.ToString(bodyStateProperty.GetValue(p2Body, null)), "Jumping", StringComparison.Ordinal))
                {
                    return;
                }

                Transform visualRoot = visualRootField.GetValue(p2Body) as Transform;
                if (visualRoot == null)
                {
                    return;
                }

                // The split-screen camera follows PlayerSpider's rigidbody root directly,
                // while BodyMovement renders the spider from visualRoot between physics ticks.
                // Use the same small visual offset for P2's camera after the original mod has
                // placed it, keeping P2's own view locked to the smooth rendered spider.
                Vector3 visualOffset = visualRoot.position - playerTwo.transform.position;
                if (visualOffset.sqrMagnitude < 4f)
                {
                    p2Camera.transform.position += visualOffset;
                }
            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[UpdateFix] Could not align P2 camera to visual root: " + exception.Message);
            }
        }

        public static void BodyMovementFixedUpdatePrefix(object __instance)
        {
            if (!IsPlayerTwo(__instance))
            {
                return;
            }

            // The game update treats P2 as an NPC because the split-screen mod
            // deliberately clears isPlayer. NPCs are allowed to sleep, which
            // leaves P2 kinematic and permanently unresponsive.
            try
            {
                if (npcSleepTimerField != null)
                {
                    npcSleepTimerField.SetValue(__instance, 0f);
                }

                Rigidbody rigidbody = rigidbodyField == null ? null : rigidbodyField.GetValue(__instance) as Rigidbody;
                if (rigidbody != null && (rigidbody.isKinematic || rigidbody.IsSleeping()))
                {
                    // A P2 that was idle while far from P1 is put to sleep by the new NPC
                    // code.  The old input patch can then put it into Jumping without ever
                    // waking its rigidbody, leaving the visible spider gliding at a stale pose.
                    bool wasKinematic = rigidbody.isKinematic;
                    rigidbody.isKinematic = false;
                    rigidbody.WakeUp();
                    if (wasKinematic)
                    {
                        MelonLogger.Msg("[UpdateFix] Woke a sleeping P2 rigidbody.");
                    }
                }

            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[UpdateFix] Could not wake P2: " + exception.Message);
            }
        }

        public static void BodyMovementFixedUpdatePostfix(object __instance)
        {
            if (!IsPlayerTwo(__instance))
            {
                return;
            }

            p2BodyFixedUpdateCount++;
            LogP2AirPhysics(__instance);
        }

        public static void BodyMovementUpdatePostfix(object __instance)
        {
            if (IsPlayerTwo(__instance))
            {
                p2BodyUpdateCount++;
            }
        }

        private static void LogP2AirPhysics(object p2BodyObject)
        {
            if (!ReadStaticBool(debugSpeedLogField))
            {
                return;
            }
            try
            {
                string state = bodyStateProperty == null ? "?" : Convert.ToString(bodyStateProperty.GetValue(p2BodyObject, null));
                bool p2WebActive = ReadStaticBool(p2WebActiveField);
                if (!string.Equals(state, "Jumping", StringComparison.Ordinal) && !p2WebActive)
                {
                    return;
                }
                if (Time.unscaledTime - lastP2AirDiagnosticTime < 0.5f)
                {
                    return;
                }
                lastP2AirDiagnosticTime = Time.unscaledTime;

                Component p2Body = p2BodyObject as Component;
                Rigidbody p2Rigidbody = p2Body == null || rigidbodyField == null ? null : rigidbodyField.GetValue(p2BodyObject) as Rigidbody;
                GameObject playerOne = GameObject.Find("PlayerSpider");
                Component p1Body = playerOne == null ? null : playerOne.GetComponentInChildren(bodyMovementType, true);
                Rigidbody p1Rigidbody = p1Body == null ? null : rigidbodyField.GetValue(p1Body) as Rigidbody;
                Transform p2VisualRoot = visualRootField == null ? null : visualRootField.GetValue(p2BodyObject) as Transform;
                Transform p1VisualRoot = p1Body == null || visualRootField == null ? null : visualRootField.GetValue(p1Body) as Transform;

                Component webController = webControllerType == null ? null : UnityEngine.Object.FindObjectOfType(webControllerType) as Component;
                bool p1WebActive = webController != null && webActiveField != null && webActiveField.GetValue(webController) is bool active && active;
                float jumpTimer = jumpTimerField != null && jumpTimerField.GetValue(p2BodyObject) is float timer ? timer : -1f;
                float p2VisualGap = p2Rigidbody == null || p2VisualRoot == null ? -1f : Vector3.Distance(p2Rigidbody.position, p2VisualRoot.position);
                float p1VisualGap = p1Rigidbody == null || p1VisualRoot == null ? -1f : Vector3.Distance(p1Rigidbody.position, p1VisualRoot.position);

                MelonLogger.Msg(
                    "[UpdateFixDiag] P2Air state=" + state +
                    " p2Web=" + p2WebActive +
                    " p1Web=" + p1WebActive +
                    " jumpTimer=" + jumpTimer.ToString("F2") +
                    " | P2 vel=" + (p2Rigidbody == null ? "?" : p2Rigidbody.linearVelocity.ToString("F2")) +
                    " grav=" + (p2Rigidbody != null && p2Rigidbody.useGravity) +
                    " drag=" + (p2Rigidbody == null ? "?" : p2Rigidbody.linearDamping.ToString("F2")) +
                    " interp=" + (p2Rigidbody == null ? "?" : p2Rigidbody.interpolation.ToString()) +
                    " kin=" + (p2Rigidbody != null && p2Rigidbody.isKinematic) +
                    " visualGap=" + p2VisualGap.ToString("F3") +
                    " | P1 vel=" + (p1Rigidbody == null ? "?" : p1Rigidbody.linearVelocity.ToString("F2")) +
                    " visualGap=" + p1VisualGap.ToString("F3"));
            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[UpdateFixDiag] P2 airborne diagnostic failed: " + exception.Message);
            }
        }

        private static void LogSharedPhysicsState()
        {
            try
            {
                if (Time.unscaledTime - lastSharedPhysicsDiagnosticTime < 0.5f || bodyMovementType == null)
                    return;

                // Advance the throttle before inspecting scene state. Otherwise an idle
                // game repeats these global searches every frame indefinitely.
                lastSharedPhysicsDiagnosticTime = Time.unscaledTime;

                GameObject p1Object = GameObject.Find("PlayerSpider");
                GameObject p2Object = GameObject.Find("PlayerSpider_P2");
                Component p1Body = p1Object == null ? null : p1Object.GetComponentInChildren(bodyMovementType, true);
                Component p2Body = p2Object == null ? null : p2Object.GetComponentInChildren(bodyMovementType, true);
                string p1State = p1Body == null || bodyStateProperty == null ? "?" : Convert.ToString(bodyStateProperty.GetValue(p1Body, null));
                string p2State = p2Body == null || bodyStateProperty == null ? "-" : Convert.ToString(bodyStateProperty.GetValue(p2Body, null));

                if (webControllerInstance == null && webControllerType != null)
                    webControllerInstance = UnityEngine.Object.FindObjectOfType(webControllerType) as Component;

                bool p1WebActive = webControllerInstance != null && webActiveField != null && webActiveField.GetValue(webControllerInstance) is bool active && active;
                bool p2WebActive = ReadStaticBool(p2WebActiveField);
                if (!string.Equals(p1State, "Jumping", StringComparison.Ordinal) &&
                    !string.Equals(p2State, "Jumping", StringComparison.Ordinal) &&
                    !p1WebActive && !p2WebActive)
                    return;

                Rigidbody p1Rb = p1Body == null || rigidbodyField == null ? null : rigidbodyField.GetValue(p1Body) as Rigidbody;
                Rigidbody p2Rb = p2Body == null || rigidbodyField == null ? null : rigidbodyField.GetValue(p2Body) as Rigidbody;
                object liveJointObject = webControllerInstance == null || webSpringJointField == null ? null : webSpringJointField.GetValue(webControllerInstance);
                SpringJoint liveJoint = liveJointObject as SpringJoint;
                object liveBody = webControllerInstance == null || webBodyMovementField == null ? null : webBodyMovementField.GetValue(webControllerInstance);

                int jointsToP1 = 0;
                int jointsToP2 = 0;
                int totalJoints = 0;
                SpringJoint[] joints = UnityEngine.Object.FindObjectsOfType<SpringJoint>(true);
                if (joints != null)
                {
                    totalJoints = joints.Length;
                    for (int i = 0; i < joints.Length; i++)
                    {
                        SpringJoint joint = joints[i];
                        if (joint == null) continue;
                        if (p1Rb != null && joint.connectedBody == p1Rb) jointsToP1++;
                        if (p2Rb != null && joint.connectedBody == p2Rb) jointsToP2++;
                    }
                }

                MelonLogger.Msg("[SharedPhysicsDiag] gravity=" + Physics.gravity.ToString("F2") +
                    " timeScale=" + Time.timeScale.ToString("F2") +
                    " | P1 state=" + p1State + " web=" + p1WebActive +
                    " vel=" + (p1Rb == null ? "?" : p1Rb.linearVelocity.ToString("F2")) +
                    " grav=" + (p1Rb != null && p1Rb.useGravity) +
                    " | P2 state=" + p2State + " web=" + p2WebActive +
                    " vel=" + (p2Rb == null ? "-" : p2Rb.linearVelocity.ToString("F2")) +
                    " grav=" + (p2Rb != null && p2Rb.useGravity) +
                    " | liveWebBody=" + (ReferenceEquals(liveBody, p1Body) ? "P1" : ReferenceEquals(liveBody, p2Body) ? "P2" : "other") +
                    " liveJoint=" + (liveJoint == null ? "null" : liveJoint.name + "->" + (liveJoint.connectedBody == null ? "null" : liveJoint.connectedBody.name)) +
                    " joints(total/toP1/toP2)=" + totalJoints + "/" + jointsToP1 + "/" + jointsToP2);
            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[SharedPhysicsDiag] failed: " + exception.Message);
            }
        }

        public static bool CameraMouseLookOnLookPrefix(object __instance, object __0)
        {
            if (!AWJSplitScreen.SplitScreenMod.IsSplitScreenActive)
            {
                return true;
            }
            if (!ReadStaticBool(p2UseGamepadField))
            {
                return true;
            }

            int gamepadIndex = ReadStaticInt(p2GamepadIndexField, 1);
            try
            {
                Vector2 retainedBefore = Vector2.zero;
                if (cameraLookInputField != null && cameraLookInputField.GetValue(__instance) is Vector2)
                    retainedBefore = (Vector2)cameraLookInputField.GetValue(__instance);
                float deadzone = ReadStaticFloat(p2DeadzoneField, 0.15f);
                Vector2 rightStick = Vector2.zero;
                object rightStickValue = inputGetP2RightStickMethod == null
                    ? null
                    : inputGetP2RightStickMethod.Invoke(null, new object[] { gamepadIndex, deadzone });
                if (rightStickValue is Vector2)
                    rightStick = (Vector2)rightStickValue;

                // Prefer the exact callback-device test.  The fallback below is for the
                // game's updated composite binding, which no longer always reports P2's
                // device on the callback passed to the old split-screen patch.
                if (inputIsCallbackFromP2Method != null && inputIsCallbackFromP2Method.Invoke(null, new object[] { __0, gamepadIndex }) is bool fromP2 && fromP2)
                {
                    AWJSplitScreen.CameraIsolationDiagnostics.LogLookCallback(__instance, __0, rightStick, true, true, retainedBefore);
                    ClearCameraLookInput(__instance);
                    return false;
                }

                if (rightStick.sqrMagnitude > 0f)
                {
                    // P2's camera is driven directly by the split-screen camera rig.
                    // Never also feed that stick into P1's game action in the same frame.
                    // CameraMouseLook reuses its last value every Update, so clear any
                    // sample that slipped through before callback ownership was known.
                    AWJSplitScreen.CameraIsolationDiagnostics.LogLookCallback(__instance, __0, rightStick, true, true, retainedBefore);
                    ClearCameraLookInput(__instance);
                    return false;
                }

                AWJSplitScreen.CameraIsolationDiagnostics.LogLookCallback(__instance, __0, rightStick, false, false, retainedBefore);
            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[UpdateFix] Could not isolate P2 look input: " + exception.Message);
            }

            return true;
        }

        private static void ClearCameraLookInput(object instance)
        {
            try
            {
                if (instance != null && cameraLookInputField != null)
                    cameraLookInputField.SetValue(instance, Vector2.zero);
            }
            catch { }
        }

        private static bool ReadStaticBool(FieldInfo field)
        {
            try
            {
                return field != null && field.GetValue(null) is bool value && value;
            }
            catch
            {
                return false;
            }
        }

        private static int ReadStaticInt(FieldInfo field, int fallback)
        {
            try
            {
                return field != null && field.GetValue(null) is int value ? value : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private static float ReadStaticFloat(FieldInfo field, float fallback)
        {
            try
            {
                return field != null && field.GetValue(null) is float value ? value : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private sealed class JumpContext
        {
            public object CameraController;
            public Transform OriginalInputTransform;
            public bool OriginalIsPlayer;
            public object WebController;
            public bool OriginalWebActive;
            public bool WebStateSwapped;
            public bool Restored;
        }

        private static void BodyMovementPerformJumpingPrefix(object __instance, ref JumpContext __state)
        {
            if (!IsPlayerTwo(__instance))
            {
                return;
            }

            JumpContext state = new JumpContext();
            try
            {
                Transform inputTransform = GetP2AirInputTransform(__instance);
                if (cameraControllerType != null && cameraInputTransformField != null && inputTransform != null)
                {
                    state.CameraController = UnityEngine.Object.FindObjectOfType(cameraControllerType);
                    if (state.CameraController != null)
                    {
                        state.OriginalInputTransform = cameraInputTransformField.GetValue(state.CameraController) as Transform;
                        cameraInputTransformField.SetValue(state.CameraController, inputTransform);
                    }
                }

                if (isPlayerField != null)
                {
                    state.OriginalIsPlayer = (bool)isPlayerField.GetValue(__instance);
                    isPlayerField.SetValue(__instance, true);
                }

                if (webControllerType != null && webActiveField != null)
                {
                    if (webControllerInstance == null)
                    {
                        webControllerInstance = UnityEngine.Object.FindObjectOfType(webControllerType) as Component;
                    }
                    Component webController = webControllerInstance;
                    if (webController != null)
                    {
                        state.WebController = webController;
                        state.OriginalWebActive = webActiveField.GetValue(webController) is bool value && value;
                        // The updated jumping routine reads the singleton controller.  For
                        // P2 that is P1's state, so P2 swings were evaluated as ordinary
                        // free-falls.  Scope P2's own value to this one physics call only.
                        webActiveField.SetValue(webController, ReadStaticBool(p2WebActiveField));
                        state.WebStateSwapped = true;
                    }
                }

                __state = state;
            }
            catch (Exception exception)
            {
                RestoreJumpContext(__instance, state);
                MelonLogger.Warning("[UpdateFix] Could not prepare P2's updated jump routine: " + exception.Message);
            }
        }

        private static Transform GetP2AirInputTransform(object instance)
        {
            Transform source = p2InputTransformField == null ? null : p2InputTransformField.GetValue(null) as Transform;
            Camera p2Camera = p2CameraField == null ? null : p2CameraField.GetValue(null) as Camera;
            Vector3 forward = p2Camera != null ? p2Camera.transform.forward : source != null ? source.forward : Vector3.forward;
            forward = Vector3.ProjectOnPlane(forward, Vector3.up);

            Component body = instance as Component;
            if (forward.sqrMagnitude < 0.0001f && body != null)
                forward = Vector3.ProjectOnPlane(body.transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            if (p2AirInputProxy == null)
            {
                GameObject proxyObject = new GameObject("AWJ_P2AirInputProxy");
                proxyObject.hideFlags = HideFlags.HideAndDontSave;
                p2AirInputProxy = proxyObject.transform;
            }

            p2AirInputProxy.position = body != null ? body.transform.position : source != null ? source.position : Vector3.zero;
            p2AirInputProxy.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            return p2AirInputProxy;
        }

        private static void BodyMovementPerformJumpingPostfix(object __instance, JumpContext __state)
        {
            RestoreJumpContext(__instance, __state);
        }

        private static Exception BodyMovementPerformJumpingFinalizer(object __instance, JumpContext __state, Exception __exception)
        {
            RestoreJumpContext(__instance, __state);
            return __exception;
        }

        private static void RestoreJumpContext(object instance, JumpContext state)
        {
            if (state == null)
            {
                return;
            }

            if (state.Restored)
            {
                return;
            }
            state.Restored = true;

            try
            {
                if (state.CameraController != null && cameraInputTransformField != null)
                {
                    cameraInputTransformField.SetValue(state.CameraController, state.OriginalInputTransform);
                }
                if (isPlayerField != null)
                {
                    isPlayerField.SetValue(instance, state.OriginalIsPlayer);
                }
                if (state.WebStateSwapped && state.WebController != null && webActiveField != null)
                {
                    webActiveField.SetValue(state.WebController, state.OriginalWebActive);
                }
            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[UpdateFix] Could not restore shared jump context: " + exception.Message);
            }
        }

        private static void CarpetComputeFluffFactorPostfix(object __instance, ref float __result)
        {
            if (p2CameraField == null || carpetWorldReferenceSizeField == null || carpetFullFluffScreenSizeField == null || carpetCullScreenSizeField == null)
            {
                return;
            }

            GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
            Camera p2Camera = p2CameraField.GetValue(null) as Camera;
            Component carpet = __instance as Component;
            if (playerTwo == null || p2Camera == null || carpet == null || !p2Camera.isActiveAndEnabled)
            {
                return;
            }

            try
            {
                float worldReferenceSize = (float)carpetWorldReferenceSizeField.GetValue(__instance);
                float fullFluffScreenSize = (float)carpetFullFluffScreenSizeField.GetValue(__instance);
                float cullScreenSize = (float)carpetCullScreenSizeField.GetValue(__instance);
                float screenSize;
                if (p2Camera.orthographic)
                {
                    screenSize = worldReferenceSize / Mathf.Max(0.0001f, 2f * p2Camera.orthographicSize);
                }
                else
                {
                    float distance = Mathf.Max(0.0001f, Vector3.Distance(p2Camera.transform.position, carpet.transform.position));
                    float halfFovTangent = Mathf.Tan(Mathf.Deg2Rad * p2Camera.fieldOfView * 0.5f);
                    screenSize = worldReferenceSize / (distance * 2f * halfFovTangent);
                }

                float p2Factor = Mathf.Clamp01(Mathf.InverseLerp(cullScreenSize, fullFluffScreenSize, screenSize));
                __result = Mathf.Max(__result, p2Factor);
            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[UpdateFix] Could not evaluate carpet detail for P2: " + exception.Message);
            }
        }

        private static void SyncP2VisualMode()
        {
            if (bodyMovementType == null)
            {
                return;
            }

            GameObject playerOne = GameObject.Find("PlayerSpider");
            GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
            if (playerOne == null || playerTwo == null)
            {
                ResetP2ShellMaterials();
                preparedP2Id = 0;
                diagnosticP2Id = 0;
                lastDistanceBand = -1;
                return;
            }

            Component p1Body = playerOne.GetComponentInChildren(bodyMovementType, true);
            Component p2Body = playerTwo.GetComponentInChildren(bodyMovementType, true);
            if (p1Body == null || p2Body == null)
            {
                return;
            }

            try
            {
                Transform p1Root = rootField == null ? null : rootField.GetValue(p1Body) as Transform;
                Transform p1Ball = ballField == null ? null : ballField.GetValue(p1Body) as Transform;
                Transform p2Root = rootField == null ? null : rootField.GetValue(p2Body) as Transform;
                Transform p2Ball = ballField == null ? null : ballField.GetValue(p2Body) as Transform;

                bool normalSpiderActive = p1Root != null && p1Root.gameObject.activeSelf;
                bool alternateVisualActive = p1Ball != null && p1Ball.gameObject.activeSelf;

                if (p2Root != null && p2Root.gameObject.activeSelf != normalSpiderActive)
                {
                    p2Root.gameObject.SetActive(normalSpiderActive);
                }
                if (p2Ball != null && p2Ball.gameObject.activeSelf != alternateVisualActive)
                {
                    p2Ball.gameObject.SetActive(alternateVisualActive);
                }

                SyncP2Appearance(playerOne, playerTwo);

                string state = "normal=" + normalSpiderActive + ", alternate=" + alternateVisualActive;
                if (state != lastVisualState)
                {
                    lastVisualState = state;
                    MelonLogger.Msg("[UpdateFix] Synced P2 visual mode with P1 (" + state + ").");
                }
            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[UpdateFix] Could not sync P2 visuals: " + exception.Message);
            }
        }

        private static void SyncP2Appearance(GameObject playerOne, GameObject playerTwo)
        {
            if (preparedP2Id == playerTwo.GetInstanceID())
            {
                return;
            }

            Component p2Body = bodyMovementType == null ? null : playerTwo.GetComponentInChildren(bodyMovementType, true);
            Rigidbody p2Rigidbody = p2Body == null || rigidbodyField == null ? null : rigidbodyField.GetValue(p2Body) as Rigidbody;
            if (p2Rigidbody != null)
            {
                p2Rigidbody.isKinematic = false;
                p2Rigidbody.WakeUp();
            }

            if (spiderCustomizationType != null && refreshCustomizationMethod != null)
            {
                Component p1Customization = playerOne.GetComponentInChildren(spiderCustomizationType, true);
                Component p2Customization = playerTwo.GetComponentInChildren(spiderCustomizationType, true);
                if (p1Customization != null && p2Customization != null)
                {
                    foreach (FieldInfo setting in spiderAppearanceSettings)
                    {
                        if (setting == null)
                        {
                            continue;
                        }

                        object value = setting.GetValue(p1Customization);
                        Array arrayValue = value as Array;
                        setting.SetValue(p2Customization, arrayValue == null ? value : arrayValue.Clone());
                    }
                    refreshCustomizationMethod.Invoke(p2Customization, null);
                }
            }

            PrepareP2Shells(playerOne, playerTwo);
        }

        private static void PrepareP2Shells(GameObject playerOne, GameObject playerTwo)
        {
            if (simpleShellType == null || setShellLengthMethod == null || shellLengthField == null)
            {
                return;
            }

            int p2Id = playerTwo.GetInstanceID();
            if (preparedP2Id == p2Id)
            {
                return;
            }

            Component[] p1Shells = playerOne.GetComponentsInChildren(simpleShellType, true);
            Component[] p2Shells = playerTwo.GetComponentsInChildren(simpleShellType, true);
            if (p1Shells.Length == 0 || p2Shells.Length == 0)
            {
                return;
            }

            int shellPairs = Mathf.Min(p1Shells.Length, p2Shells.Length);
            for (int index = 0; index < shellPairs; index++)
            {
                Component source = p1Shells[index];
                Component destination = p2Shells[index];
                foreach (FieldInfo setting in simpleShellSettings)
                {
                    if (!setting.IsStatic && setting.IsPublic)
                    {
                        setting.SetValue(destination, setting.GetValue(source));
                    }
                }

                // Calling the public methods makes the game create P2's missing shell layers
                // and applies all copied properties, including the orange colour and base-mesh hiding.
                float shellLength = (float)shellLengthField.GetValue(destination);
                setShellLengthMethod.Invoke(destination, new object[] { shellLength });
                if (updateShellColorsMethod != null && shellColorField != null)
                {
                    Color shellColor = (Color)shellColorField.GetValue(destination);
                    updateShellColorsMethod.Invoke(destination, new object[] { shellColor });
                }
            }

            int staleShellsRemoved = RemoveStaleClonedShells(p2Shells);
            int mirroredRenderers = MirrorRendererState(playerOne, playerTwo);
            int disabledLodControllers = DisableP2FluffLod(playerTwo, p2Shells);
            CreateP2ShellMaterialInstances(p2Shells);

            preparedP2Id = p2Id;
            MelonLogger.Msg("[UpdateFix] Restored " + shellPairs + " P2 shell groups, removed " + staleShellsRemoved + " stale clones, mirrored " + mirroredRenderers + " renderer layers, and disabled " + disabledLodControllers + " P1-camera fluff LOD controller(s).");
        }

        private static int DisableP2FluffLod(GameObject playerTwo, Component[] p2Shells)
        {
            int disabled = 0;
            if (spiderFluffLodType != null)
            {
                Component[] lodControllers = playerTwo.GetComponentsInChildren(spiderFluffLodType, true);
                foreach (Component component in lodControllers)
                {
                    Behaviour behaviour = component as Behaviour;
                    if (behaviour != null)
                    {
                        behaviour.enabled = false;
                        disabled++;
                    }
                }
            }

            if (setActiveShellCountMethod != null && shellCountField != null)
            {
                foreach (Component shell in p2Shells)
                {
                    int maximum = (int)shellCountField.GetValue(shell);
                    setActiveShellCountMethod.Invoke(shell, new object[] { maximum });
                }
            }
            return disabled;
        }

        private static void DumpRendererDiagnostics(GameObject playerOne, GameObject playerTwo, Component[] p2Shells)
        {
            int p2Id = playerTwo.GetInstanceID();
            if (diagnosticP2Id == p2Id)
            {
                return;
            }
            diagnosticP2Id = p2Id;

            Renderer[] p1Renderers = playerOne.GetComponentsInChildren<Renderer>(true);
            Renderer[] p2Renderers = playerTwo.GetComponentsInChildren<Renderer>(true);
            Dictionary<string, Renderer> p1ByPath = new Dictionary<string, Renderer>();
            foreach (Renderer renderer in p1Renderers)
            {
                p1ByPath[GetRelativeTransformKey(renderer.transform, playerOne.transform)] = renderer;
            }

            int p1Visible = CountVisibleRenderers(p1Renderers);
            int p2Visible = CountVisibleRenderers(p2Renderers);
            MelonLogger.Msg("[UpdateFixDiag] SPAWN renderer totals: P1=" + p1Renderers.Length + " (visible=" + p1Visible + "), P2=" + p2Renderers.Length + " (visible=" + p2Visible + "), shellGroups=" + p2Shells.Length + ".");

            foreach (Component shell in p2Shells)
            {
                MeshRenderer[] tracked = shellRenderersField == null ? null : shellRenderersField.GetValue(shell) as MeshRenderer[];
                HashSet<int> trackedIds = new HashSet<int>();
                int trackedVisible = 0;
                if (tracked != null)
                {
                    foreach (MeshRenderer renderer in tracked)
                    {
                        if (renderer != null)
                        {
                            trackedIds.Add(renderer.GetInstanceID());
                            if (IsRendererVisible(renderer))
                            {
                                trackedVisible++;
                            }
                        }
                    }
                }

                int directShells = 0;
                int untracked = 0;
                for (int childIndex = 0; childIndex < shell.transform.childCount; childIndex++)
                {
                    Transform child = shell.transform.GetChild(childIndex);
                    if (!child.name.StartsWith("Shell ", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    directShells++;
                    MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                    if (renderer != null && !trackedIds.Contains(renderer.GetInstanceID()))
                    {
                        untracked++;
                    }
                }

                if (directShells != (tracked == null ? 0 : tracked.Length) || untracked > 0)
                {
                    MelonLogger.Msg("[UpdateFixDiag] SHELL MISMATCH " + GetRelativeTransformKey(shell.transform, playerTwo.transform) + ": direct=" + directShells + ", tracked=" + (tracked == null ? 0 : tracked.Length) + ", trackedVisible=" + trackedVisible + ", untracked=" + untracked + ".");
                }
            }

            foreach (Renderer renderer in p2Renderers)
            {
                if (!IsRendererVisible(renderer) || renderer.gameObject.name.StartsWith("Shell ", StringComparison.Ordinal))
                {
                    continue;
                }

                string key = GetRelativeTransformKey(renderer.transform, playerTwo.transform);
                Renderer p1Renderer;
                bool matched = p1ByPath.TryGetValue(key, out p1Renderer);
                MelonLogger.Msg("[UpdateFixDiag] P2 VISIBLE " + key + " | " + DescribeRenderer(renderer) + " | P1match=" + matched + (matched ? " P1visible=" + IsRendererVisible(p1Renderer) : string.Empty));
            }
        }

        private static void UpdateDistanceDiagnostics()
        {
            GameObject playerOne = GameObject.Find("PlayerSpider");
            GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
            if (playerOne == null || playerTwo == null)
            {
                lastDistanceBand = -1;
                return;
            }

            float distance = Vector3.Distance(playerOne.transform.position, playerTwo.transform.position);
            int band = distance < 25f ? 0 : distance < 75f ? 1 : distance < 150f ? 2 : distance < 220f ? 3 : 4;
            if (band == lastDistanceBand)
            {
                return;
            }
            lastDistanceBand = band;

            Renderer[] renderers = playerTwo.GetComponentsInChildren<Renderer>(true);
            int visible = CountVisibleRenderers(renderers);
            int visibleShells = 0;
            foreach (Renderer renderer in renderers)
            {
                if (renderer.gameObject.name.StartsWith("Shell ", StringComparison.Ordinal) && IsRendererVisible(renderer))
                {
                    visibleShells++;
                }
            }

            Vector4 globalPosition = Shader.GetGlobalVector(PlayerPositionShaderId);
            string materialPosition = "none";
            if (p2ShellMaterials.Count > 0 && p2ShellMaterials[0] != null)
            {
                Material material = p2ShellMaterials[0];
                materialPosition = material.HasProperty(PlayerPositionShaderId) ? material.GetVector(PlayerPositionShaderId).ToString("F2") : "property-missing";
            }
            MelonLogger.Msg("[UpdateFixDiag] DISTANCE band=" + band + " distance=" + distance.ToString("F1") + " P2visible=" + visible + " visibleShells=" + visibleShells + " globalPlayerPos=" + globalPosition.ToString("F2") + " p2MaterialPlayerPos=" + materialPosition + ".");
        }

        private static int CountVisibleRenderers(Renderer[] renderers)
        {
            int count = 0;
            foreach (Renderer renderer in renderers)
            {
                if (IsRendererVisible(renderer))
                {
                    count++;
                }
            }
            return count;
        }

        private static bool IsRendererVisible(Renderer renderer)
        {
            return renderer != null && renderer.gameObject.activeInHierarchy && renderer.enabled && !renderer.forceRenderingOff && renderer.shadowCastingMode != ShadowCastingMode.ShadowsOnly;
        }

        private static string DescribeRenderer(Renderer renderer)
        {
            StringBuilder description = new StringBuilder();
            description.Append(renderer.GetType().Name);
            description.Append(" mesh=");
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            SkinnedMeshRenderer skinned = renderer as SkinnedMeshRenderer;
            Mesh mesh = filter != null ? filter.sharedMesh : skinned != null ? skinned.sharedMesh : null;
            description.Append(mesh == null ? "null" : mesh.name);
            description.Append(" materials=");
            Material[] materials = renderer.sharedMaterials;
            for (int index = 0; index < materials.Length; index++)
            {
                if (index > 0)
                {
                    description.Append(",");
                }
                Material material = materials[index];
                description.Append(material == null ? "null" : material.name + "[" + (material.shader == null ? "no-shader" : material.shader.name) + "]");
            }
            description.Append(" size=");
            description.Append(renderer.bounds.size.ToString("F2"));

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            if (!block.isEmpty)
            {
                description.Append(" blockColor=");
                description.Append(block.GetColor(ColorShaderId).ToString());
                description.Append(" body=");
                description.Append(block.GetColor(BodyColorShaderId).ToString());
                description.Append(" leg=");
                description.Append(block.GetColor(LegColorShaderId).ToString());
                description.Append(" joint=");
                description.Append(block.GetColor(JointColorShaderId).ToString());
                description.Append(" shellColor=");
                description.Append(block.GetColor(ShellColorShaderId).ToString());
                description.Append(" shell=");
                description.Append(block.GetFloat(ShellIndexShaderId).ToString("F0") + "/" + block.GetFloat(ShellCountShaderId).ToString("F0"));
            }
            return description.ToString();
        }

        private static int RemoveStaleClonedShells(Component[] p2Shells)
        {
            if (shellRenderersField == null)
            {
                return 0;
            }

            int removed = 0;
            foreach (Component shell in p2Shells)
            {
                MeshRenderer[] trackedRenderers = shellRenderersField.GetValue(shell) as MeshRenderer[];
                if (trackedRenderers == null)
                {
                    continue;
                }

                HashSet<int> trackedIds = new HashSet<int>();
                foreach (MeshRenderer tracked in trackedRenderers)
                {
                    if (tracked != null)
                    {
                        trackedIds.Add(tracked.GetInstanceID());
                    }
                }

                for (int childIndex = shell.transform.childCount - 1; childIndex >= 0; childIndex--)
                {
                    Transform child = shell.transform.GetChild(childIndex);
                    if (!child.name.StartsWith("Shell ", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                    if (renderer != null && !trackedIds.Contains(renderer.GetInstanceID()))
                    {
                        child.gameObject.SetActive(false);
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                        removed++;
                    }
                }
            }
            return removed;
        }

        private static int MirrorRendererState(GameObject playerOne, GameObject playerTwo)
        {
            Renderer[] p2Renderers = playerTwo.GetComponentsInChildren<Renderer>(true);
            Dictionary<string, Renderer> p2ByPath = new Dictionary<string, Renderer>();
            foreach (Renderer renderer in p2Renderers)
            {
                p2ByPath[GetRelativeTransformKey(renderer.transform, playerTwo.transform)] = renderer;
            }

            int mirrored = 0;
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            Renderer[] p1Renderers = playerOne.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer source in p1Renderers)
            {
                Renderer destination;
                if (!p2ByPath.TryGetValue(GetRelativeTransformKey(source.transform, playerOne.transform), out destination))
                {
                    continue;
                }

                destination.gameObject.SetActive(source.gameObject.activeSelf);
                destination.gameObject.layer = source.gameObject.layer;
                destination.enabled = source.enabled;
                destination.forceRenderingOff = source.forceRenderingOff;
                destination.sharedMaterials = source.sharedMaterials;
                destination.shadowCastingMode = source.shadowCastingMode;
                destination.receiveShadows = source.receiveShadows;
                source.GetPropertyBlock(propertyBlock);
                destination.SetPropertyBlock(propertyBlock);

                MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
                MeshFilter destinationFilter = destination.GetComponent<MeshFilter>();
                if (sourceFilter != null && destinationFilter != null)
                {
                    destinationFilter.sharedMesh = sourceFilter.sharedMesh;
                }

                SkinnedMeshRenderer sourceSkinned = source as SkinnedMeshRenderer;
                SkinnedMeshRenderer destinationSkinned = destination as SkinnedMeshRenderer;
                if (sourceSkinned != null && destinationSkinned != null)
                {
                    destinationSkinned.sharedMesh = sourceSkinned.sharedMesh;
                }
                mirrored++;
            }
            return mirrored;
        }

        private static string GetRelativeTransformKey(Transform transform, Transform root)
        {
            string key = string.Empty;
            Transform cursor = transform;
            while (cursor != null && cursor != root)
            {
                string segment = cursor.name + "#" + cursor.GetSiblingIndex();
                key = key.Length == 0 ? segment : segment + "/" + key;
                cursor = cursor.parent;
            }
            return key;
        }

        private static void CreateP2ShellMaterialInstances(Component[] p2Shells)
        {
            ResetP2ShellMaterials();
            Dictionary<Material, Material> materialInstances = new Dictionary<Material, Material>();
            foreach (Component shell in p2Shells)
            {
                Renderer[] renderers = shell.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    if (!renderer.gameObject.name.StartsWith("Shell ", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    Material source = renderer.sharedMaterial;
                    if (source == null)
                    {
                        continue;
                    }

                    Material instance;
                    if (!materialInstances.TryGetValue(source, out instance))
                    {
                        instance = new Material(source);
                        instance.name = source.name + "_P2";
                        materialInstances.Add(source, instance);
                        p2ShellMaterials.Add(instance);
                    }
                    renderer.sharedMaterial = instance;
                }
            }
            UpdateP2ShellMaterialPosition();
        }

        private static void ResetP2ShellMaterials()
        {
            foreach (Material material in p2ShellMaterials)
            {
                if (material != null)
                    UnityEngine.Object.Destroy(material);
            }
            p2ShellMaterials.Clear();
        }

        private static void EnsureP2MotionVisuals()
        {
            GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
            if (playerTwo == null || bodyMovementType == null || rigidbodyField == null)
            {
                return;
            }

            Component p2Body = playerTwo.GetComponentInChildren(bodyMovementType, true);
            Rigidbody p2Rigidbody = p2Body == null ? null : rigidbodyField.GetValue(p2Body) as Rigidbody;
            if (p2Rigidbody != null && p2Rigidbody.interpolation == RigidbodyInterpolation.None)
            {
                p2Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                MelonLogger.Msg("[UpdateFix] Restored Rigidbody interpolation for P2.");
            }

        }

        private static void EnsureP2WindTrail()
        {
            // Exact runtime reproduction of the Unity Explorer workaround.  Cycling
            // only WindParticleSystem_P2 does nothing; cycling PlayerSpider_P2 fixes the
            // trail.  Pump that player-root lifecycle before normal discovery, because
            // GameObject.Find cannot see PlayerSpider_P2 while it is intentionally off.
            if (PumpPendingP2PlayerLifecycleRefresh())
            {
                return;
            }

            if (windParticleSystemType == null || p2CameraField == null || bodyMovementType == null || rigidbodyField == null)
            {
                return;
            }

            GameObject playerOne = GameObject.Find("PlayerSpider");
            GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
            Camera p2Camera = p2CameraField.GetValue(null) as Camera;
            if (playerOne == null || playerTwo == null || p2Camera == null)
            {
                preparedP2WindId = 0;
                preparedP2WindPlayerId = 0;
                if (playerTwo == null && pendingP2PlayerLifecycle == null)
                {
                    completedP2PlayerLifecycleId = 0;
                }
                return;
            }

            Component p2Body = playerTwo.GetComponentInChildren(bodyMovementType, true);
            Rigidbody p2Rigidbody = p2Body == null ? null : rigidbodyField.GetValue(p2Body) as Rigidbody;
            if (p2Rigidbody == null)
            {
                return;
            }

            Component p1Wind = FindP1WindSystem(p2Camera);
            if (p1Wind == null)
            {
                return;
            }

            Component p2Wind = p2WindSystem;
            if (p2Wind == null)
            {
                // SetupCameras clones the complete P1 camera hierarchy, including the
                // real WindParticleSystem GameObject. Prefer that exact serialized clone:
                // it already contains the Trails module, renderer trail material, vertex
                // streams, shape/noise/etc. with no hand-authored reconstruction.
                Component[] cameraWindSystems = p2Camera.GetComponentsInChildren(windParticleSystemType, true);
                if (cameraWindSystems.Length > 0)
                {
                    p2Wind = cameraWindSystems[0];
                }
                else
                {
                    // Fallback for camera-prefab changes: clone the live P1 wind object
                    // itself. This still copies the real ParticleSystem and renderer state.
                    GameObject clone = UnityEngine.Object.Instantiate(p1Wind.gameObject);
                    clone.name = p1Wind.gameObject.name + "_P2";
                    p2Wind = clone.GetComponent(windParticleSystemType);
                }

                p2WindSystem = p2Wind;
            }

            if (p2Wind == null || p2Wind == p1Wind)
            {
                return;
            }

            int windId = p2Wind.GetInstanceID();
            int playerId = playerTwo.GetInstanceID();
            // The wind object can survive a P2 despawn when it was detached from the
            // camera/player hierarchy.  A newly spawned P2 therefore also requires all
            // target/simulation-space references to be rebound even if windId is unchanged.
            bool firstPreparation = preparedP2WindId != windId || preparedP2WindPlayerId != playerId;
            if (firstPreparation)
            {
                // The cloned game driver is hard-wired to Singleton<GameController>.Player,
                // so it must not drive P2. Disabling it invokes its OnDisable (which stops
                // the global loop), therefore restore P1's audio immediately afterwards if
                // P1 is currently above the wind threshold.
                Behaviour originalWindDriver = p2Wind as Behaviour;
                if (originalWindDriver != null && originalWindDriver.enabled)
                {
                    originalWindDriver.enabled = false;
                    RestoreP1WindAudioIfFast(p1Wind, playerOne);
                }

                p2Wind.gameObject.SetActive(true);

                // The camera clone's _Scripts.Camera.FollowTarget is disabled by
                // DisableCameraDriverBehaviours(). P1's wind object relies on that
                // component for its authored spider-relative placement/orientation.
                // Detach from the camera and re-enable the SAME FollowTarget after
                // retargeting its `target` field to P2's corresponding transform.
                p2Wind.transform.SetParent(null, true);
                bool retargetedFollow = RetargetP2WindFollowTarget(p2Wind, playerOne, playerTwo, p2Rigidbody.transform);
                if (!retargetedFollow)
                {
                    // Safe fallback if a future game build removes FollowTarget: preserve
                    // the live P1 wind object's pose relative to P1's rigidbody and apply
                    // that same pose relative to P2's rigidbody.
                    AttachP2WindUsingP1RelativePose(p1Wind, p2Wind, playerOne, playerTwo, p2Rigidbody);
                }

                MirrorCriticalWindRendererState(p1Wind, p2Wind);
                RemapP2WindCustomSimulationSpace(p1Wind, p2Wind, playerOne, playerTwo, p2Camera);

                // Keep the original wind layer and make sure P2's output camera sees it.
                p2Wind.gameObject.layer = p1Wind.gameObject.layer;
                int windLayer = p2Wind.gameObject.layer;
                if (windLayer >= 0 && windLayer < 32)
                {
                    p2Camera.cullingMask |= 1 << windLayer;
                }
            }

            P2WindTrail trail = p2Wind.GetComponent<P2WindTrail>();
            if (trail == null)
            {
                trail = p2Wind.gameObject.AddComponent<P2WindTrail>();
            }

            float activateThreshold = ReadFloat(windActivateThresholdField, p1Wind, 12f);
            float deactivateThreshold = ReadFloat(windDeactivateThresholdField, p1Wind, activateThreshold * 0.8f);
            float maxVelocity = ReadFloat(windMaxVelocityField, p1Wind, activateThreshold * 2f);
            trail.Configure(p2Rigidbody, activateThreshold, deactivateThreshold, maxVelocity);

            if (firstPreparation)
            {
                preparedP2WindId = windId;
                preparedP2WindPlayerId = playerId;
                MelonLogger.Msg("[UpdateFix] Connected P2's real wind-trail clone to P2 velocity (activate=" +
                    activateThreshold.ToString("F1") + ", deactivate=" + deactivateThreshold.ToString("F1") + ").");
                LogWindParticleConfiguration("P1", p1Wind, Camera.main);
                LogWindParticleConfiguration("P2", p2Wind, p2Camera);

                // Decisive runtime finding (second test): cycling this wind GameObject
                // alone does NOT fix the trail, but cycling PlayerSpider_P2 does.  The
                // dependency is therefore in the cloned player lifecycle.  Reproduce the
                // exact proven action once per P2 instance, after all clone rewrites and
                // wind retargeting have completed.
                if (completedP2PlayerLifecycleId != playerId)
                {
                    ScheduleP2PlayerLifecycleRefresh(playerTwo);
                }
            }
        }

        private static void ScheduleP2PlayerLifecycleRefresh(GameObject playerTwo)
        {
            if (playerTwo == null)
            {
                return;
            }

            int playerId = playerTwo.GetInstanceID();
            if (completedP2PlayerLifecycleId == playerId)
            {
                return;
            }
            if (pendingP2PlayerLifecycle != null &&
                pendingP2PlayerLifecycle.GetInstanceID() == playerId)
            {
                return;
            }

            pendingP2PlayerLifecycle = playerTwo;
            // Wait one frame before disabling.  This keeps the pulse out of the frame in
            // which SetupSecondSpider / UpdateFix first finish mutating the active clone.
            pendingP2PlayerDisableFrame = Time.frameCount + 1;
            pendingP2PlayerEnableFrame = -1;
            MelonLogger.Msg("[UpdateFix] Scheduled one-time PlayerSpider_P2 lifecycle refresh for wind-trail initialization.");
        }

        // Returns true while P2 is intentionally inactive and normal wind maintenance
        // must be skipped.  On the re-enable frame it returns false so EnsureP2WindTrail
        // immediately rebinds the wind system against the freshly-enabled player.
        private static bool PumpPendingP2PlayerLifecycleRefresh()
        {
            GameObject playerTwo = pendingP2PlayerLifecycle;
            if (playerTwo == null)
            {
                pendingP2PlayerLifecycle = null;
                pendingP2PlayerDisableFrame = -1;
                pendingP2PlayerEnableFrame = -1;
                return false;
            }

            if (pendingP2PlayerEnableFrame >= 0)
            {
                if (Time.frameCount < pendingP2PlayerEnableFrame)
                {
                    return true;
                }

                int playerId = playerTwo.GetInstanceID();
                if (!playerTwo.activeSelf)
                {
                    playerTwo.SetActive(true);
                }

                completedP2PlayerLifecycleId = playerId;
                pendingP2PlayerLifecycle = null;
                pendingP2PlayerDisableFrame = -1;
                pendingP2PlayerEnableFrame = -1;

                // Re-run the wind reference/renderer mirroring immediately after all of
                // P2's OnEnable callbacks.  Do not schedule another pulse: the completed
                // player instance ID above prevents recursion.
                preparedP2WindId = 0;
                preparedP2WindPlayerId = 0;

                MelonLogger.Msg("[UpdateFix] PlayerSpider_P2 lifecycle refresh: re-enabled player; forcing post-OnEnable wind rebind.");
                return false;
            }

            if (Time.frameCount < pendingP2PlayerDisableFrame)
            {
                return false;
            }

            if (playerTwo.activeSelf)
            {
                playerTwo.SetActive(false);
            }
            pendingP2PlayerEnableFrame = Time.frameCount + 1;
            MelonLogger.Msg("[UpdateFix] PlayerSpider_P2 lifecycle refresh: disabled player for one frame.");
            return true;
        }

        private static Component FindP1WindSystem(Camera p2Camera)
        {
            UnityEngine.Object[] allWindSystems = UnityEngine.Object.FindObjectsOfType(windParticleSystemType, true);
            Component fallback = null;
            for (int i = 0; allWindSystems != null && i < allWindSystems.Length; i++)
            {
                Component candidate = allWindSystems[i] as Component;
                if (candidate == null || candidate == p2WindSystem)
                {
                    continue;
                }

                if (p2Camera != null && candidate.transform.IsChildOf(p2Camera.transform))
                {
                    continue;
                }

                if (candidate.gameObject.name.EndsWith("_P2", StringComparison.Ordinal))
                {
                    continue;
                }

                Camera ownerCamera = candidate.GetComponentInParent<Camera>();
                if (ownerCamera != null && ownerCamera != p2Camera)
                {
                    return candidate;
                }

                if (fallback == null)
                {
                    fallback = candidate;
                }
            }
            return fallback;
        }

        private static bool RetargetP2WindFollowTarget(Component p2Wind, GameObject playerOne, GameObject playerTwo, Transform fallbackTarget)
        {
            Behaviour[] behaviours = p2Wind.GetComponents<Behaviour>();
            for (int i = 0; behaviours != null && i < behaviours.Length; i++)
            {
                Behaviour behaviour = behaviours[i];
                if (behaviour == null)
                {
                    continue;
                }

                Type type = behaviour.GetType();
                string fullName = type.FullName ?? type.Name;
                if (!string.Equals(type.Name, "FollowTarget", StringComparison.Ordinal) &&
                    !fullName.EndsWith(".FollowTarget", StringComparison.Ordinal))
                {
                    continue;
                }

                FieldInfo targetField = AccessTools.Field(type, "target");
                if (targetField == null)
                {
                    continue;
                }

                Transform priorTarget = targetField.GetValue(behaviour) as Transform;
                Transform p2Target = null;
                if (priorTarget != null)
                {
                    if (IsTransformWithin(priorTarget, playerTwo.transform))
                    {
                        p2Target = priorTarget;
                    }
                    else
                    {
                        p2Target = FindMirroredTransform(priorTarget, playerOne.transform, playerTwo.transform);
                    }
                }
                if (p2Target == null)
                {
                    p2Target = fallbackTarget;
                }

                targetField.SetValue(behaviour, p2Target);
                behaviour.enabled = true;
                MelonLogger.Msg("[UpdateFix] Retargeted P2 wind FollowTarget from " +
                    DescribeTransform(priorTarget) + " to " + DescribeTransform(p2Target) + ".");
                return true;
            }
            return false;
        }

        private static void AttachP2WindUsingP1RelativePose(Component p1Wind, Component p2Wind, GameObject playerOne, GameObject playerTwo, Rigidbody p2Rigidbody)
        {
            Transform p1Anchor = null;
            Component p1Body = playerOne.GetComponentInChildren(bodyMovementType, true);
            Rigidbody p1Rigidbody = p1Body == null ? null : rigidbodyField.GetValue(p1Body) as Rigidbody;
            if (p1Rigidbody != null)
            {
                p1Anchor = p1Rigidbody.transform;
            }

            Vector3 localPosition = Vector3.zero;
            Quaternion localRotation = Quaternion.identity;
            if (p1Anchor != null && p1Wind != null)
            {
                localPosition = p1Anchor.InverseTransformPoint(p1Wind.transform.position);
                localRotation = Quaternion.Inverse(p1Anchor.rotation) * p1Wind.transform.rotation;
            }

            p2Wind.transform.SetParent(p2Rigidbody.transform, false);
            p2Wind.transform.localPosition = localPosition;
            p2Wind.transform.localRotation = localRotation;
            MelonLogger.Warning("[UpdateFix] P2 wind FollowTarget was unavailable; using P1's rigidbody-relative wind pose instead.");
        }

        private static void MirrorCriticalWindRendererState(Component p1Wind, Component p2Wind)
        {
            ParticleSystem p1System = p1Wind == null ? null : p1Wind.GetComponent<ParticleSystem>();
            ParticleSystem p2System = p2Wind == null ? null : p2Wind.GetComponent<ParticleSystem>();
            ParticleSystemRenderer sourceRenderer = p1System == null ? null : p1System.GetComponent<ParticleSystemRenderer>();
            ParticleSystemRenderer destinationRenderer = p2System == null ? null : p2System.GetComponent<ParticleSystemRenderer>();
            if (sourceRenderer == null || destinationRenderer == null)
            {
                return;
            }

            // Instantiate already copies the complete renderer. Re-assert only the
            // properties that decide whether the authored trail can be drawn. In
            // particular, RenderMode=None is intentional: it hides particle sprites while
            // the Trails module renders through Wind_Line.
            destinationRenderer.enabled = sourceRenderer.enabled;
            destinationRenderer.forceRenderingOff = sourceRenderer.forceRenderingOff;
            destinationRenderer.renderMode = sourceRenderer.renderMode;
            destinationRenderer.trailMaterial = sourceRenderer.trailMaterial;
            destinationRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            destinationRenderer.sortingOrder = sourceRenderer.sortingOrder;
            destinationRenderer.renderingLayerMask = sourceRenderer.renderingLayerMask;

            // Wind_Line can depend on trail-specific vertex streams. Camera/GameObject
            // cloning should preserve them, but copying the live P1 list makes that
            // dependency explicit and guards against clone-time renderer mutations.
            List<ParticleSystemVertexStream> trailStreams = new List<ParticleSystemVertexStream>();
            sourceRenderer.GetActiveTrailVertexStreams(trailStreams);
            destinationRenderer.SetActiveTrailVertexStreams(trailStreams);
        }

        private static void RemapP2WindCustomSimulationSpace(Component p1Wind, Component p2Wind, GameObject playerOne, GameObject playerTwo, Camera p2Camera)
        {
            ParticleSystem sourceSystem = p1Wind == null ? null : p1Wind.GetComponent<ParticleSystem>();
            ParticleSystem destinationSystem = p2Wind == null ? null : p2Wind.GetComponent<ParticleSystem>();
            if (sourceSystem == null || destinationSystem == null)
            {
                return;
            }

            ParticleSystem.MainModule sourceMain = sourceSystem.main;
            ParticleSystem.MainModule destinationMain = destinationSystem.main;
            if (sourceMain.simulationSpace != ParticleSystemSimulationSpace.Custom)
            {
                return;
            }

            Transform sourceSpace = sourceMain.customSimulationSpace;
            if (sourceSpace == null)
            {
                return;
            }

            Transform destinationSpace = null;
            if (sourceSpace == p1Wind.transform)
            {
                destinationSpace = p2Wind.transform;
            }
            if (destinationSpace == null)
            {
                destinationSpace = FindMirroredTransform(sourceSpace, playerOne.transform, playerTwo.transform);
            }

            Camera p1Camera = p1Wind.GetComponentInParent<Camera>();
            if (destinationSpace == null && p1Camera != null && p2Camera != null)
            {
                destinationSpace = FindMirroredTransform(sourceSpace, p1Camera.transform, p2Camera.transform);
            }

            if (destinationSpace != null)
            {
                destinationMain.customSimulationSpace = destinationSpace;
            }
            else
            {
                MelonLogger.Warning("[UpdateFix] P1 wind uses Custom simulation space " + DescribeTransform(sourceSpace) +
                    "; no P2 mirror was found, so the cloned reference was left unchanged.");
            }
        }

        private static Transform FindMirroredTransform(Transform source, Transform sourceRoot, Transform destinationRoot)
        {
            if (source == null || sourceRoot == null || destinationRoot == null)
            {
                return null;
            }
            if (source == sourceRoot)
            {
                return destinationRoot;
            }

            List<int> path = new List<int>();
            List<string> pathNames = new List<string>();
            Transform cursor = source;
            while (cursor != null && cursor != sourceRoot)
            {
                path.Add(cursor.GetSiblingIndex());
                pathNames.Add(cursor.name);
                cursor = cursor.parent;
            }
            if (cursor != sourceRoot)
            {
                return null;
            }

            Transform destination = destinationRoot;
            for (int i = path.Count - 1; i >= 0; i--)
            {
                int childIndex = path[i];
                string expectedName = pathNames[i];
                Transform next = childIndex >= 0 && childIndex < destination.childCount
                    ? destination.GetChild(childIndex)
                    : null;

                // P2 visual cleanup can remove/rebuild siblings after the initial spider
                // clone. Prefer the original sibling index, but fall back to the matching
                // child name if that index no longer describes the same transform.
                if (next == null || !string.Equals(next.name, expectedName, StringComparison.Ordinal))
                {
                    next = null;
                    for (int child = 0; child < destination.childCount; child++)
                    {
                        Transform candidate = destination.GetChild(child);
                        if (string.Equals(candidate.name, expectedName, StringComparison.Ordinal))
                        {
                            next = candidate;
                            break;
                        }
                    }
                }

                if (next == null)
                {
                    return null;
                }
                destination = next;
            }
            return destination;
        }

        private static bool IsTransformWithin(Transform transform, Transform root)
        {
            if (transform == null || root == null)
            {
                return false;
            }
            return transform == root || transform.IsChildOf(root);
        }

        private static string DescribeTransform(Transform transform)
        {
            return transform == null ? "null" : transform.name + "@" + transform.position.ToString("F2");
        }

        private static void LogWindParticleConfiguration(string label, Component wind, Camera camera)
        {
            try
            {
                ParticleSystem system = wind == null ? null : wind.GetComponent<ParticleSystem>();
                ParticleSystemRenderer renderer = system == null ? null : system.GetComponent<ParticleSystemRenderer>();
                if (system == null)
                {
                    MelonLogger.Warning("[UpdateFix] " + label + " wind has no ParticleSystem.");
                    return;
                }

                ParticleSystem.MainModule main = system.main;
                ParticleSystem.TrailModule trails = system.trails;
                Material trailMaterial = renderer == null ? null : renderer.trailMaterial;
                List<ParticleSystemVertexStream> trailStreams = new List<ParticleSystemVertexStream>();
                if (renderer != null)
                {
                    renderer.GetActiveTrailVertexStreams(trailStreams);
                }
                bool cameraSeesLayer = camera != null && (camera.cullingMask & (1 << system.gameObject.layer)) != 0;
                MelonLogger.Msg("[UpdateFix] " + label + " wind config: sim=" + main.simulationSpace +
                    " custom=" + DescribeTransform(main.customSimulationSpace) +
                    " scaling=" + main.scalingMode +
                    " trailsEnabled=" + trails.enabled +
                    " trailMode=" + trails.mode +
                    " ratio=" + trails.ratio.ToString("F2") +
                    " worldSpace=" + trails.worldSpace +
                    " lifetimeMul=" + trails.lifetimeMultiplier.ToString("F2") +
                    " minVertex=" + trails.minVertexDistance.ToString("F3") +
                    " widthMul=" + trails.widthOverTrailMultiplier.ToString("F3") +
                    " rendererMode=" + (renderer == null ? "null" : renderer.renderMode.ToString()) +
                    " rendererEnabled=" + (renderer != null && renderer.enabled) +
                    " forceOff=" + (renderer != null && renderer.forceRenderingOff) +
                    " trailMat=" + (trailMaterial == null ? "null" : trailMaterial.name) +
                    " trailStreams=" + trailStreams.Count +
                    " layer=" + system.gameObject.layer +
                    " cameraSeesLayer=" + cameraSeesLayer + ".");
            }
            catch (Exception ex)
            {
                MelonLogger.Warning("[UpdateFix] Could not log " + label + " wind configuration: " + ex.Message);
            }
        }

        private static void RestoreP1WindAudioIfFast(Component p1Wind, GameObject playerOne)
        {
            try
            {
                Component p1Body = playerOne == null ? null : playerOne.GetComponentInChildren(bodyMovementType, true);
                Rigidbody p1Rigidbody = p1Body == null ? null : rigidbodyField.GetValue(p1Body) as Rigidbody;
                float activateThreshold = ReadFloat(windActivateThresholdField, p1Wind, 12f);
                float maxVelocity = ReadFloat(windMaxVelocityField, p1Wind, activateThreshold * 2f);
                float deactivateThreshold = ReadFloat(windDeactivateThresholdField, p1Wind, activateThreshold * 0.8f);
                if (p1Rigidbody == null || p1Rigidbody.linearVelocity.magnitude <= activateThreshold)
                {
                    return;
                }

                Type musicControllerType = AccessTools.TypeByName("_Scripts.Singletons.MusicController");
                Type singletonType = musicControllerType == null ? null : musicControllerType.BaseType;
                PropertyInfo instanceProperty = singletonType == null ? null : singletonType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                object controller = instanceProperty == null ? null : instanceProperty.GetValue(null, null);
                MethodInfo start = musicControllerType == null ? null : AccessTools.Method(musicControllerType, "StartWindLoop");
                MethodInfo setSpeed = musicControllerType == null ? null : AccessTools.Method(musicControllerType, "SetWindLoopSpeed");
                if (controller != null && start != null)
                {
                    start.Invoke(controller, null);
                    if (setSpeed != null)
                    {
                        float speed = p1Rigidbody.linearVelocity.magnitude;
                        setSpeed.Invoke(controller, new object[] { Mathf.InverseLerp(deactivateThreshold, maxVelocity, speed) });
                    }
                }
            }
            catch { }
        }

        private static float ReadFloat(FieldInfo field, object instance, float fallback)
        {
            try
            {
                return field != null && field.GetValue(instance) is float value ? value : fallback;
            }
            catch
            {
                return fallback;
            }
        }

        private static void UpdateP2ShellMaterialPosition()
        {
            if (p2ShellMaterials.Count == 0)
            {
                return;
            }

            GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
            if (playerTwo == null)
            {
                ResetP2ShellMaterials();
                return;
            }

            Vector3 position = playerTwo.transform.position;
            foreach (Material material in p2ShellMaterials)
            {
                if (material != null)
                {
                    material.SetVector(PlayerPositionShaderId, position);
                }
            }
        }

        private static void LogP2FrameCadence()
        {
            if (Time.unscaledTime - lastFrameDiagnosticTime < 1f)
            {
                return;
            }

            GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
            if (playerTwo == null)
            {
                return;
            }

            lastFrameDiagnosticTime = Time.unscaledTime;
            int renderedFrames = Time.frameCount - lastFrameDiagnosticFrame;
            int bodyUpdates = p2BodyUpdateCount - lastFrameDiagnosticP2Updates;
            int bodyFixedUpdates = p2BodyFixedUpdateCount - lastFrameDiagnosticP2FixedUpdates;
            int p2Renders = p2CameraRenderCount - lastFrameDiagnosticP2Renders;
            int p1Renders = p1CameraRenderCount - lastFrameDiagnosticP1Renders;
            lastFrameDiagnosticFrame = Time.frameCount;
            lastFrameDiagnosticP2Updates = p2BodyUpdateCount;
            lastFrameDiagnosticP2FixedUpdates = p2BodyFixedUpdateCount;
            lastFrameDiagnosticP2Renders = p2CameraRenderCount;
            lastFrameDiagnosticP1Renders = p1CameraRenderCount;

            MelonLogger.Msg("[UpdateFixDiag] FrameCadence frames=" + renderedFrames + " p2BodyUpdate=" + bodyUpdates + " p2Fixed=" + bodyFixedUpdates + " p1Renders=" + p1Renders + " p2Renders=" + p2Renders + ".");
        }

        private static void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == null || p2CameraField == null)
            {
                return;
            }

            try
            {
                Camera p2Camera = p2CameraField.GetValue(null) as Camera;
                if (p2Camera != null && camera == p2Camera)
                {
                    p2CameraRenderCount++;
                }
                else if (camera != null && camera.name == "Main Camera")
                {
                    p1CameraRenderCount++;
                }
                GameObject playerTwo = GameObject.Find("PlayerSpider_P2");
                if (p2Camera != null && camera == p2Camera && playerTwo != null)
                {
                    // The game only updates this shader value for the real player.  P2's
                    // shell material uses it for distance fading, so set it before P2's camera draws.
                    Shader.SetGlobalVector(PlayerPositionShaderId, playerTwo.transform.position);
                }
            }
            catch (Exception exception)
            {
                MelonLogger.Warning("[UpdateFix] Could not set P2 render-distance anchor: " + exception.Message);
            }
        }

        private static bool IsPlayerTwo(object instance)
        {
            Component component = instance as Component;
            if (component == null)
            {
                return false;
            }

            Transform cursor = component.transform;
            while (cursor != null)
            {
                if (string.Equals(cursor.gameObject.name, "PlayerSpider_P2", StringComparison.Ordinal))
                {
                    return true;
                }
                cursor = cursor.parent;
            }
            return false;
        }

        private sealed class P2VisualSmoother : MonoBehaviour
        {
            private Rigidbody playerRigidbody;
            private Transform visualRoot;
            private Vector3 previousPosition;
            private Vector3 currentPosition;
            private bool hasPhysicsPose;

            public void Configure(Rigidbody rigidbody, Transform root)
            {
                playerRigidbody = rigidbody;
                visualRoot = root;
                if (!hasPhysicsPose && playerRigidbody != null)
                {
                    previousPosition = currentPosition = playerRigidbody.position;
                    hasPhysicsPose = true;
                }
            }

            public void CapturePhysicsPose()
            {
                if (playerRigidbody == null)
                {
                    return;
                }

                if (!hasPhysicsPose)
                {
                    previousPosition = currentPosition = playerRigidbody.position;
                    hasPhysicsPose = true;
                    return;
                }

                previousPosition = currentPosition;
                currentPosition = playerRigidbody.position;
            }

            private void LateUpdate()
            {
                if (!hasPhysicsPose || visualRoot == null)
                {
                    return;
                }

                float alpha = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
                // BodyMovement already owns the special airborne visual rotation.  Only
                // replace the fixed-step position, leaving that authored rotation intact.
                visualRoot.position = Vector3.Lerp(previousPosition, currentPosition, alpha);
            }
        }

        private sealed class P2WindTrail : MonoBehaviour
        {
            private Rigidbody playerRigidbody;
            private ParticleSystem[] particleSystems;
            private float activateThreshold;
            private float deactivateThreshold;
            private float maxVelocity;
            private bool active;
            private bool reportedParticleSetup;
            private float nextDiagnosticTime;
            private static object musicController;
            private static MethodInfo startWindLoopMethod;
            private static MethodInfo stopWindLoopMethod;
            private static MethodInfo setWindLoopSpeedMethod;

            public void Configure(Rigidbody rigidbody, float activateAt, float deactivateAt, float maxWindVelocity)
            {
                playerRigidbody = rigidbody;
                activateThreshold = Mathf.Max(0f, activateAt);
                deactivateThreshold = Mathf.Clamp(deactivateAt, 0f, activateThreshold);
                maxVelocity = Mathf.Max(activateThreshold + 0.01f, maxWindVelocity);
                if (particleSystems == null || particleSystems.Length == 0)
                {
                    particleSystems = GetComponentsInChildren<ParticleSystem>(true);
                    foreach (ParticleSystem system in particleSystems)
                    {
                        if (system == null) continue;
                        ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
                        if (renderer != null)
                        {
                            renderer.enabled = true;
                            renderer.forceRenderingOff = false;
                        }
                        system.gameObject.SetActive(true);
                        ParticleSystem.EmissionModule emission = system.emission;
                        emission.enabled = active;
                        // Vanilla WindParticleSystem never restarts the simulation when
                        // speed crosses the threshold; it only toggles emission. Keep the
                        // ParticleSystem running so its authored Trails module owns trail
                        // creation/lifetime exactly as on P1.
                        if (!system.isPlaying)
                        {
                            system.Play(true);
                        }
                    }
                }

                if (!reportedParticleSetup)
                {
                    reportedParticleSetup = true;
                    MelonLogger.Msg("[UpdateFix] P2 wind trail prepared with " + (particleSystems == null ? 0 : particleSystems.Length) + " particle system(s).");
                }
            }

            private void Update()
            {
                if (playerRigidbody == null || particleSystems == null)
                {
                    return;
                }

                float speed = playerRigidbody.linearVelocity.magnitude;
                if (!active && speed > activateThreshold)
                {
                    SetActive(true);
                }
                else if (active && speed < deactivateThreshold)
                {
                    SetActive(false);
                }

                // The game's wind loop is global and its original driver only watches
                // P1. Keep it alive while P2 is moving fast; P1's driver still owns
                // stopping it when neither player is above the threshold.
                if (active)
                {
                    UpdateWindSound(speed);
                }
                else
                {
                    StopWindSoundIfNeitherPlayerIsFast();
                }

                if (Time.unscaledTime >= nextDiagnosticTime)
                {
                    nextDiagnosticTime = Time.unscaledTime + 2f;
                    ParticleSystem system = particleSystems.Length > 0 ? particleSystems[0] : null;
                    MelonLogger.Msg("[UpdateFix] P2 wind state: active=" + active + " speed=" + speed.ToString("F1") +
                        " emitting=" + (system != null && system.isEmitting) + " playing=" + (system != null && system.isPlaying) +
                        " particles=" + (system == null ? 0 : system.particleCount) + " layer=" + (system == null ? -1 : system.gameObject.layer));
                }
            }

            private void OnEnable()
            {
                // The first OnEnable happens immediately when this component is added,
                // before Configure has supplied its Rigidbody/system cache.  Keep this
                // defensive initialization for any later wind-object activation, although
                // the proven startup fix now cycles PlayerSpider_P2 rather than this object.
                if (playerRigidbody == null || particleSystems == null)
                {
                    return;
                }

                foreach (ParticleSystem system in particleSystems)
                {
                    if (system == null) continue;
                    ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = true;
                        renderer.forceRenderingOff = false;
                    }
                    if (!system.isPlaying)
                    {
                        system.Play(true);
                    }
                }

                // Restore the same threshold state immediately after the activation pulse;
                // otherwise OnDisable intentionally left emission off until Update runs.
                SetActive(playerRigidbody.linearVelocity.magnitude > activateThreshold);
            }

            private void OnDisable()
            {
                SetActive(false);
            }

            private void SetActive(bool value)
            {
                active = value;
                if (particleSystems == null)
                {
                    return;
                }
                foreach (ParticleSystem system in particleSystems)
                {
                    if (system == null)
                    {
                        continue;
                    }

                    ParticleSystem.EmissionModule emission = system.emission;
                    // Match the game's WindParticleSystem literally: do not Emit(),
                    // Clear(), change RenderMode, or restart the system here. The hidden
                    // particles are only anchors for the real Trails module.
                    emission.enabled = value;
                }
            }

            private void UpdateWindSound(float speed)
            {
                try
                {
                    EnsureMusicController();

                    if (musicController != null)
                    {
                        if (startWindLoopMethod != null) startWindLoopMethod.Invoke(musicController, null);
                        if (setWindLoopSpeedMethod != null)
                            setWindLoopSpeedMethod.Invoke(musicController, new object[] { Mathf.InverseLerp(deactivateThreshold, maxVelocity, speed) });
                    }
                }
                catch { }
            }

            private static void EnsureMusicController()
            {
                if (musicController != null) return;

                Type musicControllerType = AccessTools.TypeByName("_Scripts.Singletons.MusicController");
                Type singletonType = musicControllerType == null ? null : musicControllerType.BaseType;
                PropertyInfo instanceProperty = singletonType == null ? null : singletonType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                musicController = instanceProperty == null ? null : instanceProperty.GetValue(null, null);
                startWindLoopMethod = musicControllerType == null ? null : AccessTools.Method(musicControllerType, "StartWindLoop");
                stopWindLoopMethod = musicControllerType == null ? null : AccessTools.Method(musicControllerType, "StopWindLoop");
                setWindLoopSpeedMethod = musicControllerType == null ? null : AccessTools.Method(musicControllerType, "SetWindLoopSpeed");
            }

            private void StopWindSoundIfNeitherPlayerIsFast()
            {
                try
                {
                    if (musicController == null)
                    {
                        EnsureMusicController();
                    }

                    GameObject playerOne = GameObject.Find("PlayerSpider");
                    Rigidbody p1Rigidbody = playerOne == null ? null : playerOne.GetComponentInChildren<Rigidbody>(true);
                    if (p1Rigidbody == null || p1Rigidbody.linearVelocity.magnitude < deactivateThreshold)
                    {
                        if (musicController != null && stopWindLoopMethod != null)
                            stopWindLoopMethod.Invoke(musicController, null);
                    }
                }
                catch { }
            }
        }
    }
}
