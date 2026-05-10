using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FlatKit;
using PixelCrushers.DialogueSystem;
using Tayx.Graphy;
using Tayx.Graphy.Utils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UnityConsent;
using _Scripts.General;
using _Scripts.Utils;

namespace _Scripts.Singletons;

public class SettingsController : Singleton<SettingsController>
{
	[SerializeField]
	private bool wipeSaveDataOnNextGameStart;

	[SerializeField]
	private VolumeProfile[] globalVolumeProfiles;

	[SerializeField]
	private UniversalRenderPipelineAsset[] universalRenderPipelineAssets;

	[SerializeField]
	private UniversalRendererData rendererData;

	[SerializeField]
	private SystemLanguage[] availableLanguages;

	[SerializeField]
	private OutlineSettings outlineSettingsDefault;

	[SerializeField]
	private OutlineSettings outlineSettingsMobile;

	private static readonly Vector3 defaultGravity = new Vector3(0f, -40f, 0f);

	private const float defaultMouseSensitivity = 0.5f;

	private const float defaultGamepadSensitivity = 0.25f;

	private const float defaultTouchSensitivity = 0.5f;

	private const float defaultMobileJoystickSensitivity = 0.25f;

	private const float defaultVolume = 1f;

	private const float minFieldOfView = 90f;

	private const float maxFieldOfView = 120f;

	private static readonly int[] availableFrameRatesMobile = new int[4] { 30, 60, 90, 120 };

	private static List<string> options;

	private static List<Vector2Int> resolutions;

	private static Resolution maxResolution;

	private static float outlineEdgeColorAlpha;

	private static bool outlineUseDepth;

	private static bool outlineUseNormals;

	public static Vector3 DefaultGravity => defaultGravity;

	public static float MinFieldOfView => 90f;

	public static float MaxFieldOfView => 120f;

	public static bool AutoZoom { get; private set; }

	public static HoldOrToggle SprintMode { get; private set; }

	public static HoldOrToggle WebShootMode { get; private set; }

	public static bool WaterWalking { get; private set; }

	public static bool AllowAnalytics { get; private set; }

	public static float Brightness { get; private set; }

	public static float Saturation { get; private set; }

	public static float Contrast { get; private set; }

	public static float Outline { get; private set; }

	public static bool Fullscreen { get; private set; }

	public static int ResolutionIndex { get; private set; }

	public static int QualityIndex { get; private set; }

	public static bool VSync { get; private set; }

	public static bool GrassShaderEnabled { get; private set; }

	public static bool DepthOfField { get; private set; }

	public static float RenderScale { get; private set; }

	public static bool SMAA { get; private set; }

	public static int MSAA { get; private set; }

	public static float FieldOfView { get; private set; }

	public static int TargetFrameRate { get; private set; }

	public static float DialogueVolume { get; private set; }

	public static float MasterVolume { get; private set; }

	public static float MusicVolume { get; private set; }

	public static float SoundVolume { get; private set; }

	public static float UiVolume { get; private set; }

	public static float TrailerVolume { get; private set; }

	public static bool ArachnophobiaMode { get; private set; }

	public static CollectibleStyleType CollectibleStyle { get; private set; }

	public static CoinDetectorModeEnum CoinDetectorMode { get; private set; }

	public static CoinDetectorSoundEnum CoinDetectorSound { get; private set; }

	public static bool Snowflakes { get; private set; }

	public static SystemLanguage Language { get; private set; }

	public static float QuestMarkerSize { get; private set; }

	public static bool EventMode { get; private set; }

	public static SystemLanguage EventLanguage { get; private set; }

	public static ControlOverlay ShowControls { get; private set; }

	public static float MouseSensitivityX { get; private set; }

	public static float MouseSensitivityY { get; private set; }

	public static float GamepadSensitivityX { get; private set; }

	public static float GamepadSensitivityY { get; private set; }

	public static bool MouseInvertXAxis { get; private set; }

	public static bool MouseInvertYAxis { get; private set; }

	public static bool GamepadInvertXAxis { get; private set; }

	public static bool GamepadInvertYAxis { get; private set; }

	public static bool AutoStopSprint { get; private set; }

	public static bool MultiTouch { get; private set; }

	public static bool AdvanceBuildingMode { get; private set; }

	public static bool GraphyEnabled { get; private set; }

	public static bool AlternativeQuickBuildButton { get; private set; }

	public static float touchSensitivityX { get; private set; }

	public static float touchSensitivityY { get; private set; }

	public static float mobileJoystickSensitivityX { get; private set; }

	public static float mobileJoystickSensitivityY { get; private set; }

	public static bool TouchInvertXAxis { get; private set; }

	public static bool TouchInvertYAxis { get; private set; }

	public static bool mobileJoystickInvertXAxis { get; private set; }

	public static bool mobileJoystickInvertYAxis { get; private set; }

	public static float UISize { get; private set; }

	public static float ButtonTransparency { get; private set; }

	public static event EventHandler OnSettingsUpdated;

	public static event EventHandler OnLanguageChanged;

	protected override void Awake()
	{
		base.Awake();
		if (Singleton<SettingsController>.Instance == this)
		{
			string text = SaveController.LoadString("Version", string.Empty, SaveData.Settings);
			if (text == string.Empty || (wipeSaveDataOnNextGameStart && text != BuildInfo.FullVersionString))
			{
				Debug.Log("Wiping Save Data");
				ResetGame(resetSystemLanguage: true);
			}
			CalculateMaxSettings();
			LoadSettings();
		}
	}

	private void Start()
	{
		StartCoroutine(DefaultSettingsCor());
		SceneController.OnSceneLoaded -= SceneController_OnSceneLoaded;
		SceneController.OnSceneLoaded += SceneController_OnSceneLoaded;
	}

	private void OnDestroy()
	{
		UniversalRenderPipelineAsset[] array = universalRenderPipelineAssets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].renderScale = 1f;
		}
		SceneController.OnSceneLoaded -= SceneController_OnSceneLoaded;
	}

	private IEnumerator DefaultSettingsCor()
	{
		yield return null;
		if (UnityEngine.Object.FindObjectsByType<GraphyManager>(FindObjectsInactive.Include, FindObjectsSortMode.None) != null && G_Singleton<GraphyManager>.Instance != null)
		{
			if (GraphyEnabled)
			{
				G_Singleton<GraphyManager>.Instance.Enable();
			}
			else
			{
				G_Singleton<GraphyManager>.Instance.Disable();
			}
		}
		SetVSync(VSync);
		SetOutlineEdgeColorAlpha(Outline);
		UpdateRenderScale();
		UpdateMSAA();
		InvertColors(value: false);
	}

	private static void CalculateMaxSettings()
	{
		Resolution resolution = default(Resolution);
		resolution.width = Display.main.renderingWidth;
		resolution.height = Display.main.renderingHeight;
		maxResolution = resolution;
	}

	private static void LoadSettings()
	{
		LoadSettingsGameplay();
		LoadSettingsGraphics();
		LoadSettingsAudio();
		LoadSettingsAccessibility();
		LoadSettingsControls();
		LoadSettingsMobile();
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void ResetGame(bool resetSystemLanguage = false)
	{
		SaveController.DeleteAllFiles();
		SetDefaultSettings(resetSystemLanguage);
	}

	private static void SetDefaultSettings(bool resetSystemLanguage = false)
	{
		SystemLanguage language = Language;
		SetSprintMode(HoldOrToggle.Hold);
		SetWebShootMode(HoldOrToggle.Hold);
		SetAutoZoom(value: false);
		SetWaterWalking(value: false);
		SetAllowAnalytics(value: false);
		SetQualityIndex(3);
		SetVSync(value: true);
		SetRenderScale(1f);
		SetSMAA(value: true);
		SetMSAAIndex(0);
		SetBrightness(0.5f);
		SetContrast(0.5f);
		SetSaturation(0.5f);
		SetOutlineEdgeColorAlpha(0.5f);
		SetDepthOfField(value: false);
		SetGrassShaderEnabled(value: false);
		SetFullscreen(value: true);
		SetMasterVolume(1f);
		SetMusicVolume(0.5f);
		SetSoundVolume(1f);
		SetDialogueVolume(0.5f);
		SetUiVolume(1f);
		SetArachnophobiaMode(value: false);
		SetCollectibleStyle(0);
		SetCoinDetectorMode(2);
		SetCoinDetectorSound(1);
		if (resetSystemLanguage)
		{
			SetLanguage(LoadSystemLanguage());
		}
		else
		{
			SetLanguage(language);
		}
		SetQuestMarkerSize(0.5f);
		SetSnowflakes(value: true);
		SetMouseSensitivityX(0.5f);
		SetMouseSensitivityY(0.5f);
		SetGamepadSensitivityX(0.25f);
		SetGamepadSensitivityY(0.25f);
		SetMouseInvertXAxis(value: false);
		SetMouseInvertYAxis(value: false);
		SetGamepadInvertXAxis(value: false);
		SetGamepadInvertYAxis(value: false);
		SetAutoStopSprint(value: false);
		SetMultiTouch(value: false);
		SetAdvancedBuildingMode(value: true);
		SetGraphyEnabled(value: false);
		SetAlternativeQuickBuildButton(value: false);
		SetTouchSensitivityX(0.5f);
		SetTouchSensitivityY(0.5f);
		SetMobileJoystickSensitivityX(0.25f);
		SetMobileJoystickSensitivityY(0.25f);
		SetTouchInvertX(value: false);
		SetTouchInvertY(value: false);
		SetMobileJoystickInvertX(value: false);
		SetMobileJoystickInvertY(value: false);
		SetUISize(1f);
		SetButtonTransparency(0f);
		if (Singleton<CosmeticItemsController>.Instance != null)
		{
			Singleton<CosmeticItemsController>.Instance.ResetSpiderCustomization();
		}
		SaveController.Save("Version", BuildInfo.FullVersionString, SaveData.Settings);
	}

	public static void ResetTutorial()
	{
		if (EventMode)
		{
			SystemLanguage eventLanguage = EventLanguage;
			SaveController.DeleteAllFiles();
			if (Singleton<CosmeticItemsController>.Instance != null)
			{
				Singleton<CosmeticItemsController>.Instance.UnlockAllDemoItems();
			}
			SetAutoZoom(value: false);
			SetSprintMode(HoldOrToggle.Hold);
			SetWaterWalking(value: false);
			SetWebShootMode(HoldOrToggle.Hold);
			SetAllowAnalytics(value: false);
			SetQualityIndex(3);
			SetBrightness(0.5f);
			SetContrast(0.5f);
			SetSaturation(0.5f);
			SetOutlineEdgeColorAlpha(0.5f);
			SetGrassShaderEnabled(value: false);
			SetDepthOfField(value: false);
			SetVSync(value: true);
			SetSMAA(value: true);
			SetMSAAIndex(0);
			SetMasterVolume(1f);
			SetMusicVolume(0.5f);
			SetSoundVolume(1f);
			SetDialogueVolume(0.5f);
			SetUiVolume(1f);
			SetArachnophobiaMode(value: false);
			SetCollectibleStyle(CollectibleStyleType.Button);
			SetCoinDetectorMode(2);
			SetCoinDetectorSound(1);
			SetLanguage(EventLanguage);
			SetQuestMarkerSize(0.5f);
			SetSnowflakes(value: true);
			SetEventLanguage((int)eventLanguage);
			SetMouseSensitivityX(0.5f);
			SetMouseSensitivityY(0.5f);
			SetGamepadSensitivityX(0.25f);
			SetGamepadSensitivityY(0.25f);
			SetMouseInvertXAxis(value: false);
			SetMouseInvertYAxis(value: false);
			SetGamepadInvertXAxis(value: false);
			SetGamepadInvertYAxis(value: false);
			if (Singleton<CosmeticItemsController>.Instance != null)
			{
				Singleton<CosmeticItemsController>.Instance.ResetSpiderCustomization();
			}
			SaveController.Save("Version", BuildInfo.FullVersionString, SaveData.Settings);
		}
	}

	public static int GetDefaultValue(DefaultSettingType type)
	{
		int result = 0;
		switch (type)
		{
		case DefaultSettingType.Language:
			result = GetLanguage();
			break;
		case DefaultSettingType.Phobia:
			result = (ArachnophobiaMode ? 1 : 0);
			break;
		case DefaultSettingType.CollectibleStyle:
			result = (int)CollectibleStyle;
			break;
		case DefaultSettingType.Fullscreen:
			result = (Fullscreen ? 1 : 0);
			break;
		case DefaultSettingType.VSync:
			result = (VSync ? 1 : 0);
			break;
		case DefaultSettingType.Resolution:
			result = ResolutionIndex;
			break;
		case DefaultSettingType.Quality:
			result = QualityIndex;
			break;
		case DefaultSettingType.MouseInvertX:
			result = (MouseInvertXAxis ? 1 : 0);
			break;
		case DefaultSettingType.MouseInvertY:
			result = (MouseInvertYAxis ? 1 : 0);
			break;
		case DefaultSettingType.GamepadInvertX:
			result = (GamepadInvertXAxis ? 1 : 0);
			break;
		case DefaultSettingType.GamepadInvertY:
			result = (GamepadInvertYAxis ? 1 : 0);
			break;
		case DefaultSettingType.SprintMode:
			result = ((SprintMode == HoldOrToggle.Toggle) ? 1 : 0);
			break;
		case DefaultSettingType.AutoZoom:
			result = (AutoZoom ? 1 : 0);
			break;
		case DefaultSettingType.WebShootMode:
			result = ((WebShootMode == HoldOrToggle.Toggle) ? 1 : 0);
			break;
		case DefaultSettingType.WaterWalking:
			result = (WaterWalking ? 1 : 0);
			break;
		case DefaultSettingType.Grass:
			result = (GrassShaderEnabled ? 1 : 0);
			break;
		case DefaultSettingType.DepthOfField:
			result = (DepthOfField ? 1 : 0);
			break;
		case DefaultSettingType.EventMode:
			result = (EventMode ? 1 : 0);
			break;
		case DefaultSettingType.EventLanguage:
			result = GetEventLanguage();
			break;
		case DefaultSettingType.AutoStopSprint:
			result = (AutoStopSprint ? 1 : 0);
			break;
		case DefaultSettingType.MultiTouch:
			result = (MultiTouch ? 1 : 0);
			break;
		case DefaultSettingType.Graphy:
			result = (GraphyEnabled ? 1 : 0);
			break;
		case DefaultSettingType.TouchInvertX:
			result = (TouchInvertXAxis ? 1 : 0);
			break;
		case DefaultSettingType.TouchInvertY:
			result = (TouchInvertYAxis ? 1 : 0);
			break;
		case DefaultSettingType.MobileJoystickInvertX:
			result = (mobileJoystickInvertXAxis ? 1 : 0);
			break;
		case DefaultSettingType.MobileJoystickInvertY:
			result = (mobileJoystickInvertYAxis ? 1 : 0);
			break;
		case DefaultSettingType.AlternativeQuickBuildButton:
			result = (AlternativeQuickBuildButton ? 1 : 0);
			break;
		case DefaultSettingType.Snowflakes:
			result = (Snowflakes ? 1 : 0);
			break;
		case DefaultSettingType.SMAA:
			result = (SMAA ? 1 : 0);
			break;
		case DefaultSettingType.MSAA:
			result = MSAA;
			break;
		case DefaultSettingType.CoinDetectorMode:
			result = (int)CoinDetectorMode;
			break;
		case DefaultSettingType.CoinDetectorSound:
			result = (int)CoinDetectorSound;
			break;
		case DefaultSettingType.TargetFrameRate:
		{
			for (int i = 0; i < availableFrameRatesMobile.Length; i++)
			{
				if (availableFrameRatesMobile[i] == TargetFrameRate)
				{
					result = i;
					break;
				}
			}
			break;
		}
		case DefaultSettingType.ShowControls:
			result = (int)ShowControls;
			break;
		case DefaultSettingType.AllowAnalytics:
			result = (AllowAnalytics ? 1 : 0);
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		case DefaultSettingType.None:
			break;
		}
		return result;
	}

	public static List<string> GetFields(DefaultSettingType type)
	{
		List<string> list = new List<string>();
		switch (type)
		{
		case DefaultSettingType.Language:
		case DefaultSettingType.EventLanguage:
			return new List<string>
			{
				"English", "Deutsch", "Français", "Español", "Português", "Polski", "Türkçe", "Русский", "简体中文", "Nederlands",
				"日本語"
			};
		case DefaultSettingType.Resolution:
			return options;
		case DefaultSettingType.Quality:
			return QualitySettings.names.ToList();
		case DefaultSettingType.SprintMode:
		case DefaultSettingType.WebShootMode:
			return new List<string> { "Setting_Hold", "Setting_Toggle" };
		case DefaultSettingType.Reserved1:
			return new List<string> { "50%", "75%", "100%" };
		case DefaultSettingType.AlternativeQuickBuildButton:
			return new List<string> { "Setting_Bottom", "Setting_Top" };
		case DefaultSettingType.CollectibleStyle:
			return new List<string>
			{
				"Setting_Collectible Style_Button", "Setting_Collectible Style_Coin", "Setting_Collectible Style_Hexagon", "Setting_Collectible Style_Flower", "Setting_Collectible Style_Cookie", "Setting_Collectible Style_Banana", "Setting_Collectible Style_Spider Web", "Setting_Collectible Style_Rubber Duck", "Setting_Collectible Style_Heart", "Setting_Collectible Style_Star",
				"Setting_Collectible Style_Invisible"
			};
		case DefaultSettingType.MSAA:
			return new List<string> { "Setting_Off", "2x", "4x", "8x" };
		case DefaultSettingType.CoinDetectorMode:
			return new List<string> { "Setting_Coin Detector Mode_Sound", "Setting_Coin Detector Mode_Visual", "Setting_Coin Detector Mode_Both" };
		case DefaultSettingType.CoinDetectorSound:
			return new List<string>
			{
				"Setting_Coin Detector Sound_Beeping", "Setting_Coin Detector Sound_Clicking", "Setting_Coin Detector Sound_Clicking2", "Setting_Coin Detector Sound_Rubber Duck", "Setting_Coin Detector Sound_Fly", "Setting_Coin Detector Sound_Guitar", "Setting_Coin Detector Sound_Bouncy Ball", "Setting_Coin Detector Sound_Look", "Setting_Coin Detector Sound_Car Horn", "Setting_Coin Detector Sound_Tapping Glass",
				"Setting_Coin Detector Sound_Shmoop", "Setting_Coin Detector Sound_Keyboard"
			};
		case DefaultSettingType.TargetFrameRate:
			return availableFrameRatesMobile.Select((int t) => t.ToString()).ToList();
		case DefaultSettingType.ShowControls:
			return new List<string> { "Setting_Show Controls_None", "Setting_Show Controls_Minimal", "Setting_Show Controls_Full" };
		case DefaultSettingType.AllowAnalytics:
			return new List<string> { "Setting_No", "Setting_Yes" };
		default:
			return new List<string> { "Setting_Off", "Setting_On" };
		}
	}

	public static void SetValue(int index, DefaultSettingType type)
	{
		switch (type)
		{
		case DefaultSettingType.Language:
			SetLanguage(index);
			break;
		case DefaultSettingType.Phobia:
			SetPhobia(index);
			break;
		case DefaultSettingType.CollectibleStyle:
			SetCollectibleStyle(index);
			break;
		case DefaultSettingType.Fullscreen:
			SetFullscreenIndex(index);
			break;
		case DefaultSettingType.VSync:
			SetVSyncIndex(index);
			break;
		case DefaultSettingType.Resolution:
			SetResolutionIndex(index);
			break;
		case DefaultSettingType.Quality:
			SetQualityIndex(index);
			break;
		case DefaultSettingType.MouseInvertX:
			SetMouseInvertXIndex(index);
			break;
		case DefaultSettingType.MouseInvertY:
			SetMouseInvertYIndex(index);
			break;
		case DefaultSettingType.GamepadInvertX:
			SetGamepadInvertXIndex(index);
			break;
		case DefaultSettingType.GamepadInvertY:
			SetGamepadInvertYIndex(index);
			break;
		case DefaultSettingType.SprintMode:
			SetSprintIndex(index);
			break;
		case DefaultSettingType.AutoZoom:
			SetAutoZoomIndex(index);
			break;
		case DefaultSettingType.WebShootMode:
			SetWebShootIndex(index);
			break;
		case DefaultSettingType.WaterWalking:
			SetWaterWalkingIndex(index);
			break;
		case DefaultSettingType.Grass:
			SetGrassIndex(index);
			break;
		case DefaultSettingType.DepthOfField:
			SetDepthOfFieldIndex(index);
			break;
		case DefaultSettingType.EventMode:
			SetEventMode(index);
			break;
		case DefaultSettingType.EventLanguage:
			SetEventLanguage(index);
			break;
		case DefaultSettingType.AutoStopSprint:
			SetAutoStopSprintIndex(index);
			break;
		case DefaultSettingType.MultiTouch:
			SetMultiTouchIndex(index);
			break;
		case DefaultSettingType.Graphy:
			SetGraphyEnabledIndex(index);
			break;
		case DefaultSettingType.TouchInvertX:
			SetTouchInvertXIndex(index);
			break;
		case DefaultSettingType.TouchInvertY:
			SetTouchInvertYIndex(index);
			break;
		case DefaultSettingType.MobileJoystickInvertX:
			SetMobileJoystickInvertXIndex(index);
			break;
		case DefaultSettingType.MobileJoystickInvertY:
			SetMobileJoystickInvertYIndex(index);
			break;
		case DefaultSettingType.AlternativeQuickBuildButton:
			SetAlternativeQuickBuildButtonIndex(index);
			break;
		case DefaultSettingType.Snowflakes:
			SetSnowflakes(index);
			break;
		case DefaultSettingType.SMAA:
			SetSMAAIndex(index);
			break;
		case DefaultSettingType.MSAA:
			SetMSAAIndex(index);
			break;
		case DefaultSettingType.CoinDetectorMode:
			SetCoinDetectorMode(index);
			break;
		case DefaultSettingType.CoinDetectorSound:
			SetCoinDetectorSound(index);
			break;
		case DefaultSettingType.TargetFrameRate:
			SetTargetFrameRateIndex(index);
			break;
		case DefaultSettingType.ShowControls:
			SetShowControls(index);
			break;
		case DefaultSettingType.AllowAnalytics:
			SetAllowAnalyticsIndex(index);
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		case DefaultSettingType.None:
			break;
		}
	}

	private static void LoadSettingsGameplay()
	{
		SprintMode = SaveController.Load("SprintMode", HoldOrToggle.Hold, SaveData.Settings);
		WebShootMode = SaveController.Load("WebShootMode", HoldOrToggle.Hold, SaveData.Settings);
		AutoZoom = SaveController.Load("AutoZoom", defaultValue: false, SaveData.Settings);
		WaterWalking = SaveController.Load("WaterWalking", defaultValue: false, SaveData.Settings);
		SetAllowAnalytics(SaveController.Load("AllowAnalytics", defaultValue: false, SaveData.Settings));
	}

	private static void SetSprintIndex(int index)
	{
		SetSprintMode((HoldOrToggle)index);
	}

	private static void SetSprintMode(HoldOrToggle value)
	{
		SprintMode = value;
		SaveController.Save("SprintMode", SprintMode, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetWebShootIndex(int index)
	{
		SetWebShootMode((HoldOrToggle)index);
	}

	private static void SetWebShootMode(HoldOrToggle value)
	{
		WebShootMode = value;
		SaveController.Save("WebShootMode", WebShootMode, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetAutoZoomIndex(int index)
	{
		SetAutoZoom(index == 1);
	}

	private static void SetAutoZoom(bool value)
	{
		AutoZoom = value;
		SaveController.Save("AutoZoom", AutoZoom, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetWaterWalkingIndex(int index)
	{
		SetWaterWalking(index == 1);
	}

	private static void SetWaterWalking(bool value)
	{
		WaterWalking = value;
		SaveController.Save("WaterWalking", WaterWalking, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetAllowAnalyticsIndex(int index)
	{
		SetAllowAnalytics(index == 1);
	}

	public static void SetAllowAnalytics(bool value)
	{
		AllowAnalytics = value;
		ConsentState consentState = EndUserConsent.GetConsentState();
		consentState.AnalyticsIntent = (AllowAnalytics ? ConsentStatus.Granted : ConsentStatus.Denied);
		EndUserConsent.SetConsentState(consentState);
		SaveController.Save("AllowAnalytics", AllowAnalytics, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void LoadSettingsGraphics()
	{
		GrassShaderEnabled = SaveController.Load("GrassShaderEnabled", defaultValue: false, SaveData.Settings);
		Brightness = SaveController.Load("Brightness", 0.5f, SaveData.Settings);
		Saturation = SaveController.Load("Saturation", 0.5f, SaveData.Settings);
		Contrast = SaveController.Load("Contrast", 0.5f, SaveData.Settings);
		Outline = SaveController.Load("Outline", 0.5f, SaveData.Settings);
		ResolutionIndex = SaveController.Load("ResolutionIndex", -1, SaveData.Settings);
		QualityIndex = SaveController.Load("QualityIndex", 3, SaveData.Settings);
		TargetFrameRate = -1;
		Application.targetFrameRate = TargetFrameRate;
		VSync = SaveController.Load("VSync", defaultValue: true, SaveData.Settings);
		Fullscreen = SaveController.Load("Fullscreen", defaultValue: true, SaveData.Settings);
		DepthOfField = SaveController.Load("DepthOfField", defaultValue: true, SaveData.Settings);
		RenderScale = 1f;
		SMAA = SaveController.Load("SMAA", defaultValue: true, SaveData.Settings);
		MSAA = SaveController.Load("MSAA", 0, SaveData.Settings);
		Resolution[] array = Screen.resolutions;
		resolutions = new List<Vector2Int>();
		Resolution[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Resolution resolution = array2[i];
			Vector2Int item = new Vector2Int(resolution.width, resolution.height);
			if (!resolutions.Contains(item))
			{
				resolutions.Add(item);
			}
		}
		if (ResolutionIndex >= resolutions.Count)
		{
			ResolutionIndex = -1;
		}
		options = new List<string>();
		for (int j = 0; j < resolutions.Count; j++)
		{
			string item2 = $"{resolutions[j].x} x {resolutions[j].y}";
			options.Add(item2);
			if (ResolutionIndex < 0 && resolutions[j].x == Screen.currentResolution.width && resolutions[j].y == Screen.currentResolution.height)
			{
				SetResolutionIndex(j);
			}
		}
		float value = _Scripts.Utils.Utils.ConvertVerticalToHorizontalFOV(60f, GetResolutionRatio());
		float defaultValue = Mathf.InverseLerp(90f, 120f, value);
		FieldOfView = SaveController.Load("FieldOfView", defaultValue, SaveData.Settings);
		SaveController.Save("FieldOfView", FieldOfView, SaveData.Settings);
		QualitySettings.SetQualityLevel(QualityIndex);
		VolumeProfile[] array3 = Singleton<SettingsController>.Instance.globalVolumeProfiles;
		foreach (VolumeProfile obj in array3)
		{
			if (obj.TryGet<ColorAdjustments>(out var component))
			{
				component.postExposure.value = (Brightness - 0.5f) * 2f;
				component.contrast.value = (Contrast - 0.5f) * 100f;
				component.saturation.value = (Saturation - 0.5f) * 200f;
			}
			if (obj.TryGet<DepthOfField>(out var component2))
			{
				component2.active = DepthOfField;
			}
		}
		Screen.fullScreen = Fullscreen;
	}

	public static void SetBrightness(float value)
	{
		Brightness = value;
		VolumeProfile[] array = Singleton<SettingsController>.Instance.globalVolumeProfiles;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGet<ColorAdjustments>(out var component))
			{
				component.postExposure.value = (Brightness - 0.5f) * 2f;
			}
		}
		SaveController.Save("Brightness", Brightness, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetSaturation(float value)
	{
		Saturation = value;
		VolumeProfile[] array = Singleton<SettingsController>.Instance.globalVolumeProfiles;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGet<ColorAdjustments>(out var component))
			{
				component.saturation.value = (Saturation - 0.5f) * 200f;
			}
		}
		SaveController.Save("Saturation", Saturation, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetContrast(float value)
	{
		Contrast = value;
		VolumeProfile[] array = Singleton<SettingsController>.Instance.globalVolumeProfiles;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGet<ColorAdjustments>(out var component))
			{
				component.contrast.value = (Contrast - 0.5f) * 100f;
			}
		}
		SaveController.Save("Contrast", Contrast, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetFullscreenIndex(int index)
	{
		SetFullscreen(index == 1);
	}

	private static void SetFullscreen(bool value)
	{
		Fullscreen = value;
		Screen.fullScreen = Fullscreen;
		SaveController.Save("Fullscreen", Fullscreen, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetGrassIndex(int index)
	{
		SetGrassShaderEnabled(index == 1);
	}

	private static void SetGrassShaderEnabled(bool value)
	{
		GrassShaderEnabled = value;
		SaveController.Save("GrassShaderEnabled", GrassShaderEnabled, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetDepthOfFieldIndex(int index)
	{
		SetDepthOfField(index == 1);
	}

	private static void SetDepthOfField(bool value)
	{
		DepthOfField = value;
		VolumeProfile[] array = Singleton<SettingsController>.Instance.globalVolumeProfiles;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGet<DepthOfField>(out var component))
			{
				component.active = DepthOfField;
			}
		}
		SaveController.Save("DepthOfField", DepthOfField, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetResolutionIndex(int value)
	{
		ResolutionIndex = value;
		Vector2Int vector2Int = resolutions[ResolutionIndex];
		Screen.SetResolution(vector2Int.x, vector2Int.y, Fullscreen);
		SaveController.Save("ResolutionIndex", ResolutionIndex, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static float GetResolutionRatio()
	{
		Vector2Int vector2Int = resolutions[ResolutionIndex];
		return (float)vector2Int.x / (float)vector2Int.y;
	}

	private static void SetQualityIndex(int value)
	{
		QualityIndex = value;
		QualitySettings.SetQualityLevel(QualityIndex);
		QualitySettings.vSyncCount = (VSync ? 1 : 0);
		SaveController.Save("QualityIndex", QualityIndex, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetVSyncIndex(int index)
	{
		SetVSync(index == 1);
	}

	private static void SetVSync(bool value)
	{
		VSync = value;
		QualitySettings.vSyncCount = (VSync ? 1 : 0);
		SaveController.Save("VSync", VSync, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetFieldOfView(float value)
	{
		FieldOfView = value;
		SaveController.Save("FieldOfView", FieldOfView, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetSMAAIndex(int index)
	{
		SetSMAA(index == 1);
	}

	private static void SetSMAA(bool value)
	{
		SMAA = value;
		SaveController.Save("SMAA", SMAA, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetMSAAIndex(int index)
	{
		MSAA = index;
		UpdateMSAA();
		SaveController.Save("MSAA", MSAA, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void UpdateMSAA()
	{
		int msaaSampleCount = 0;
		if (MSAA == 0)
		{
			msaaSampleCount = 1;
		}
		else if (MSAA == 1)
		{
			msaaSampleCount = 2;
		}
		else if (MSAA == 2)
		{
			msaaSampleCount = 4;
		}
		else if (MSAA == 3)
		{
			msaaSampleCount = 8;
		}
		UniversalRenderPipelineAsset[] array = Singleton<SettingsController>.Instance.universalRenderPipelineAssets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].msaaSampleCount = msaaSampleCount;
		}
	}

	private static void SetTargetFrameRateIndex(int index)
	{
		SetTargetFrameRate(availableFrameRatesMobile[index]);
	}

	private static void SetTargetFrameRate(int value)
	{
		TargetFrameRate = value;
		Application.targetFrameRate = TargetFrameRate;
		SaveController.Save("TargetFrameRate", TargetFrameRate, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void GetDefaultOutline()
	{
		outlineEdgeColorAlpha = Singleton<SettingsController>.Instance.outlineSettingsDefault.edgeColor.a;
	}

	private static void ApplyDefaultOutline()
	{
		Singleton<SettingsController>.Instance.outlineSettingsDefault.edgeColor.a = outlineEdgeColorAlpha;
		Singleton<SettingsController>.Instance.outlineSettingsMobile.edgeColor.a = outlineEdgeColorAlpha;
	}

	public static void SetOutlineEdgeColorAlpha(float value)
	{
		Outline = value;
		FlatKitOutline flatKitOutline = ScriptableObject.CreateInstance<FlatKitOutline>();
		foreach (ScriptableRendererFeature rendererFeature in Singleton<SettingsController>.Instance.rendererData.rendererFeatures)
		{
			if (rendererFeature is FlatKitOutline flatKitOutline2)
			{
				flatKitOutline = flatKitOutline2;
				break;
			}
		}
		if (!(flatKitOutline == null))
		{
			OutlineSettings settings = flatKitOutline.settings;
			settings.edgeColor.a = value;
			flatKitOutline.settings = settings;
			Singleton<SettingsController>.Instance.rendererData.SetDirty();
			SaveController.Save("Outline", Outline, SaveData.Settings);
			SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
		}
	}

	private static void SetOutlineSettings(OutlineSettings outlineSettings)
	{
		FlatKitOutline flatKitOutline = ScriptableObject.CreateInstance<FlatKitOutline>();
		foreach (ScriptableRendererFeature rendererFeature in Singleton<SettingsController>.Instance.rendererData.rendererFeatures)
		{
			if (rendererFeature is FlatKitOutline flatKitOutline2)
			{
				flatKitOutline = flatKitOutline2;
				break;
			}
		}
		if (!(flatKitOutline == null))
		{
			flatKitOutline.settings = outlineSettings;
			Singleton<SettingsController>.Instance.rendererData.SetDirty();
		}
	}

	public static void InvertColors(bool value)
	{
		foreach (ScriptableRendererFeature rendererFeature in Singleton<SettingsController>.Instance.rendererData.rendererFeatures)
		{
			if (rendererFeature is FullScreenPassRendererFeature)
			{
				rendererFeature.SetActive(value);
				break;
			}
		}
	}

	public static void SetRenderScale(float value)
	{
		RenderScale = value;
		UpdateRenderScale();
		SaveController.Save("RenderScale", RenderScale, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void UpdateRenderScale()
	{
		float renderScale = 0.5f + RenderScale * 0.5f;
		UniversalRenderPipelineAsset[] array = Singleton<SettingsController>.Instance.universalRenderPipelineAssets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].renderScale = renderScale;
		}
	}

	private static void LoadSettingsAudio()
	{
		MasterVolume = SaveController.Load("MasterVolume", 1f, SaveData.Settings);
		DialogueVolume = SaveController.Load("DialogueVolume", 0.5f, SaveData.Settings);
		MusicVolume = SaveController.Load("MusicVolume", 0.5f, SaveData.Settings);
		SoundVolume = SaveController.Load("SoundVolume", 1f, SaveData.Settings);
		UiVolume = SaveController.Load("UiVolume", 1f, SaveData.Settings);
		TrailerVolume = SaveController.Load("TrailerVolume", 1f, SaveData.Settings);
	}

	public static void SetMasterVolume(float value)
	{
		MasterVolume = value;
		SaveController.Save("MasterVolume", MasterVolume, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetMusicVolume(float value)
	{
		MusicVolume = value;
		SaveController.Save("MusicVolume", MusicVolume, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetSoundVolume(float value)
	{
		SoundVolume = value;
		SaveController.Save("SoundVolume", SoundVolume, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetDialogueVolume(float value)
	{
		DialogueVolume = value;
		SaveController.Save("DialogueVolume", DialogueVolume, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetUiVolume(float value)
	{
		UiVolume = value;
		SaveController.Save("UiVolume", UiVolume, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetTrailerVolume(float value)
	{
		TrailerVolume = value;
		SaveController.Save("TrailerVolume", TrailerVolume, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void LoadSettingsAccessibility()
	{
		ArachnophobiaMode = SaveController.Load("ArachnophobiaMode", defaultValue: false, SaveData.Settings);
		CollectibleStyle = SaveController.Load("CollectibleStyle", CollectibleStyleType.Button, SaveData.Settings);
		Snowflakes = SaveController.Load("Snowflakes", defaultValue: true, SaveData.Settings);
		QuestMarkerSize = SaveController.Load("QuestMarkerSize", 0.5f, SaveData.Settings);
		Language = LoadSystemLanguage();
		EventLanguage = SaveController.Load("EventLanguage", SystemLanguage.English, SaveData.Settings);
		CoinDetectorMode = SaveController.Load("CoinDetectorMode", CoinDetectorModeEnum.Both, SaveData.Settings);
		CoinDetectorSound = SaveController.Load("CoinDetectorSound", CoinDetectorSoundEnum.Clicking, SaveData.Settings);
		ShowControls = SaveController.Load("ShowControls", ControlOverlay.TaskList, SaveData.Settings);
	}

	private static SystemLanguage LoadSystemLanguage()
	{
		SystemLanguage systemLanguage = Application.systemLanguage;
		SystemLanguage systemLanguage2 = SaveController.Load("Language", systemLanguage, SaveData.Settings);
		if (!Singleton<SettingsController>.Instance.availableLanguages.Contains(systemLanguage2))
		{
			systemLanguage2 = SystemLanguage.English;
		}
		return systemLanguage2;
	}

	public static void SetQuestMarkerSize(float value)
	{
		QuestMarkerSize = value;
		SaveController.Save("QuestMarkerSize", QuestMarkerSize, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetPhobia(int index)
	{
		SetArachnophobiaMode(index == 1);
	}

	public static void SetArachnophobiaMode(bool value)
	{
		ArachnophobiaMode = value;
		SaveController.Save("ArachnophobiaMode", ArachnophobiaMode, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetCollectibleStyle(int index)
	{
		SetCollectibleStyle((CollectibleStyleType)index);
	}

	private static void SetCollectibleStyle(CollectibleStyleType value)
	{
		CollectibleStyle = value;
		SaveController.Save("CollectibleStyle", CollectibleStyle, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetCoinDetectorMode(int index)
	{
		SetCoinDetectorMode((CoinDetectorModeEnum)index);
	}

	private static void SetCoinDetectorMode(CoinDetectorModeEnum value)
	{
		CoinDetectorMode = value;
		SaveController.Save("CoinDetectorMode", CoinDetectorMode, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetCoinDetectorSound(int index)
	{
		SetCoinDetectorSound((CoinDetectorSoundEnum)index);
	}

	private static void SetCoinDetectorSound(CoinDetectorSoundEnum value)
	{
		CoinDetectorSound = value;
		SaveController.Save("CoinDetectorSound", CoinDetectorSound, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetSnowflakes(int index)
	{
		SetSnowflakes(index == 1);
	}

	private static void SetSnowflakes(bool value)
	{
		Snowflakes = value;
		SaveController.Save("Snowflakes", Snowflakes, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static int GetLanguage()
	{
		return Language switch
		{
			SystemLanguage.English => 0, 
			SystemLanguage.German => 1, 
			SystemLanguage.French => 2, 
			SystemLanguage.Spanish => 3, 
			SystemLanguage.Portuguese => 4, 
			SystemLanguage.Polish => 5, 
			SystemLanguage.Turkish => 6, 
			SystemLanguage.Russian => 7, 
			SystemLanguage.ChineseSimplified => 8, 
			SystemLanguage.Dutch => 9, 
			SystemLanguage.Japanese => 10, 
			_ => 0, 
		};
	}

	public string GetLanguageName()
	{
		return Language switch
		{
			SystemLanguage.English => "en", 
			SystemLanguage.German => "de", 
			SystemLanguage.French => "fr", 
			SystemLanguage.Spanish => "es_spain", 
			SystemLanguage.Portuguese => "pt_brazil", 
			SystemLanguage.Polish => "pl", 
			SystemLanguage.Turkish => "tr", 
			SystemLanguage.Russian => "ru", 
			SystemLanguage.ChineseSimplified => "ch_simplified", 
			SystemLanguage.Dutch => "nl", 
			SystemLanguage.Japanese => "jp", 
			_ => "en", 
		};
	}

	public static void SetLanguage(SystemLanguage value)
	{
		Language = value;
		switch (Language)
		{
		default:
			DialogueManager.SetLanguage("");
			break;
		case SystemLanguage.Chinese:
			DialogueManager.SetLanguage("ch");
			break;
		case SystemLanguage.French:
			DialogueManager.SetLanguage("fr");
			break;
		case SystemLanguage.German:
			DialogueManager.SetLanguage("de");
			break;
		case SystemLanguage.Italian:
			DialogueManager.SetLanguage("it");
			break;
		case SystemLanguage.Japanese:
			DialogueManager.SetLanguage("jp");
			break;
		case SystemLanguage.Russian:
			DialogueManager.SetLanguage("ru");
			break;
		case SystemLanguage.Spanish:
			DialogueManager.SetLanguage("es_spain");
			break;
		case SystemLanguage.ChineseSimplified:
			DialogueManager.SetLanguage("ch_simplified");
			break;
		case SystemLanguage.Polish:
			DialogueManager.SetLanguage("pl");
			break;
		case SystemLanguage.Portuguese:
			DialogueManager.SetLanguage("pt_brazil");
			break;
		case SystemLanguage.Turkish:
			DialogueManager.SetLanguage("tr");
			break;
		case SystemLanguage.Dutch:
			DialogueManager.SetLanguage("nl");
			break;
		}
		DialogueManager.SendUpdateTracker();
		SaveController.Save("Language", Language, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
		SettingsController.OnLanguageChanged?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetLanguage(int index)
	{
		switch (index)
		{
		case 0:
			SetLanguage(SystemLanguage.English);
			break;
		case 1:
			SetLanguage(SystemLanguage.German);
			break;
		case 2:
			SetLanguage(SystemLanguage.French);
			break;
		case 3:
			SetLanguage(SystemLanguage.Spanish);
			break;
		case 4:
			SetLanguage(SystemLanguage.Portuguese);
			break;
		case 5:
			SetLanguage(SystemLanguage.Polish);
			break;
		case 6:
			SetLanguage(SystemLanguage.Turkish);
			break;
		case 7:
			SetLanguage(SystemLanguage.Russian);
			break;
		case 8:
			SetLanguage(SystemLanguage.ChineseSimplified);
			break;
		case 9:
			SetLanguage(SystemLanguage.Dutch);
			break;
		case 10:
			SetLanguage(SystemLanguage.Japanese);
			break;
		}
	}

	private static int GetEventLanguage()
	{
		return EventLanguage switch
		{
			SystemLanguage.English => 0, 
			SystemLanguage.German => 1, 
			SystemLanguage.French => 2, 
			SystemLanguage.Spanish => 3, 
			SystemLanguage.Portuguese => 4, 
			SystemLanguage.Polish => 5, 
			SystemLanguage.Turkish => 6, 
			SystemLanguage.Russian => 7, 
			SystemLanguage.ChineseSimplified => 8, 
			SystemLanguage.Dutch => 9, 
			SystemLanguage.Japanese => 10, 
			_ => 0, 
		};
	}

	private static void SetEventLanguage(int index)
	{
		EventLanguage = index switch
		{
			0 => SystemLanguage.English, 
			1 => SystemLanguage.German, 
			2 => SystemLanguage.French, 
			3 => SystemLanguage.Spanish, 
			4 => SystemLanguage.Portuguese, 
			5 => SystemLanguage.Polish, 
			6 => SystemLanguage.Turkish, 
			7 => SystemLanguage.Russian, 
			8 => SystemLanguage.ChineseSimplified, 
			9 => SystemLanguage.Dutch, 
			10 => SystemLanguage.Japanese, 
			_ => EventLanguage, 
		};
		DialogueManager.SendUpdateTracker();
		SaveController.Save("EventLanguage", EventLanguage, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
		SettingsController.OnLanguageChanged?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetEventMode(int index)
	{
		EventMode = index == 1;
		DialogueLua.SetVariable("EventMode", EventMode);
		SaveController.Save("EventMode", EventMode, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetShowControls(int index)
	{
		ShowControls = (ControlOverlay)index;
		SaveController.Save("ShowControls", ShowControls, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void LoadSettingsControls()
	{
		MouseSensitivityX = SaveController.Load("MouseSensitivityX", 0.5f, SaveData.Controls);
		MouseSensitivityY = SaveController.Load("MouseSensitivityY", 0.5f, SaveData.Controls);
		GamepadSensitivityX = SaveController.Load("GamepadSensitivityX", 0.25f, SaveData.Controls);
		GamepadSensitivityY = SaveController.Load("GamepadSensitivityY", 0.25f, SaveData.Controls);
		MouseInvertXAxis = SaveController.Load("MouseInvertXAxis", defaultValue: false, SaveData.Controls);
		MouseInvertYAxis = SaveController.Load("MouseInvertYAxis", defaultValue: false, SaveData.Controls);
		GamepadInvertXAxis = SaveController.Load("GamepadInvertXAxis", defaultValue: false, SaveData.Controls);
		GamepadInvertYAxis = SaveController.Load("GamepadInvertYAxis", defaultValue: false, SaveData.Controls);
	}

	public static void SetMouseSensitivityX(float value)
	{
		MouseSensitivityX = value;
		SaveController.Save("MouseSensitivityX", MouseSensitivityX, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetMouseSensitivityY(float value)
	{
		MouseSensitivityY = value;
		SaveController.Save("MouseSensitivityY", MouseSensitivityY, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetGamepadSensitivityX(float value)
	{
		GamepadSensitivityX = value;
		SaveController.Save("GamepadSensitivityX", GamepadSensitivityX, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetGamepadSensitivityY(float value)
	{
		GamepadSensitivityY = value;
		SaveController.Save("GamepadSensitivityY", GamepadSensitivityY, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetMouseInvertXIndex(int index)
	{
		SetMouseInvertXAxis(index == 1);
	}

	private static void SetMouseInvertXAxis(bool value)
	{
		MouseInvertXAxis = value;
		SaveController.Save("MouseInvertXAxis", MouseInvertXAxis, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetMouseInvertYIndex(int index)
	{
		SetMouseInvertYAxis(index == 1);
	}

	private static void SetMouseInvertYAxis(bool value)
	{
		MouseInvertYAxis = value;
		SaveController.Save("MouseInvertYAxis", MouseInvertYAxis, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetGamepadInvertXIndex(int index)
	{
		SetGamepadInvertXAxis(index == 1);
	}

	private static void SetGamepadInvertXAxis(bool value)
	{
		GamepadInvertXAxis = value;
		SaveController.Save("GamepadInvertXAxis", GamepadInvertXAxis, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetGamepadInvertYIndex(int index)
	{
		SetGamepadInvertYAxis(index == 1);
	}

	private static void SetGamepadInvertYAxis(bool value)
	{
		GamepadInvertYAxis = value;
		SaveController.Save("GamepadInvertYAxis", GamepadInvertYAxis, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void LoadSettingsMobile()
	{
		AutoStopSprint = SaveController.Load("AutoStopSprint", defaultValue: false, SaveData.Settings);
		MultiTouch = SaveController.Load("MultiTouch", defaultValue: false, SaveData.Settings);
		AdvanceBuildingMode = SaveController.Load("AdvanceBuildingMode", defaultValue: true, SaveData.Settings);
		GraphyEnabled = SaveController.Load("GraphyEnabled", defaultValue: false, SaveData.Settings);
		AlternativeQuickBuildButton = SaveController.Load("AlternativeQuickBuildButton", defaultValue: false, SaveData.Settings);
		touchSensitivityX = SaveController.Load("touchSensitivityX", 0.5f, SaveData.Controls);
		touchSensitivityY = SaveController.Load("touchSensitivityY", 0.5f, SaveData.Controls);
		mobileJoystickSensitivityX = SaveController.Load("mobileJoystickSensitivityX", 0.25f, SaveData.Controls);
		mobileJoystickSensitivityY = SaveController.Load("mobileJoystickSensitivityY", 0.25f, SaveData.Controls);
		TouchInvertXAxis = SaveController.Load("TouchInvertXAxis", defaultValue: false, SaveData.Controls);
		TouchInvertYAxis = SaveController.Load("TouchInvertYAxis", defaultValue: false, SaveData.Controls);
		mobileJoystickInvertXAxis = SaveController.Load("mobileJoystickInvertXAxis", defaultValue: false, SaveData.Controls);
		mobileJoystickInvertYAxis = SaveController.Load("mobileJoystickInvertYAxis", defaultValue: false, SaveData.Controls);
		UISize = SaveController.Load("UISize", 1f, SaveData.Settings);
		ButtonTransparency = SaveController.Load("ButtonTransparency", 0f, SaveData.Settings);
	}

	private static void SetAutoStopSprintIndex(int index)
	{
		SetAutoStopSprint(index == 1);
	}

	private static void SetAutoStopSprint(bool value)
	{
		AutoStopSprint = value;
		SaveController.Save("AutoStopSprint", AutoStopSprint, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetMultiTouchIndex(int index)
	{
		SetMultiTouch(index == 1);
	}

	private static void SetMultiTouch(bool value)
	{
		MultiTouch = value;
		SaveController.Save("MultiTouch", MultiTouch, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetAdvancedBuildingModeIndex(int index)
	{
		SetAdvancedBuildingMode(index == 1);
	}

	private static void SetAdvancedBuildingMode(bool value)
	{
		AdvanceBuildingMode = value;
		SaveController.Save("AdvanceBuildingMode", AdvanceBuildingMode, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetAlternativeQuickBuildButtonIndex(int index)
	{
		SetAlternativeQuickBuildButton(index == 1);
	}

	private static void SetAlternativeQuickBuildButton(bool value)
	{
		AlternativeQuickBuildButton = value;
		SaveController.Save("AlternativeQuickBuildButton", AlternativeQuickBuildButton, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetGraphyEnabledIndex(int index)
	{
		SetGraphyEnabled(index == 1);
	}

	private static void SetGraphyEnabled(bool value)
	{
		GraphyEnabled = value;
		SaveController.Save("GraphyEnabled", GraphyEnabled, SaveData.Settings);
		if (UnityEngine.Object.FindObjectsByType<GraphyManager>(FindObjectsInactive.Include, FindObjectsSortMode.None) != null && G_Singleton<GraphyManager>.Instance != null)
		{
			if (GraphyEnabled)
			{
				G_Singleton<GraphyManager>.Instance.Enable();
			}
			else
			{
				G_Singleton<GraphyManager>.Instance.Disable();
			}
		}
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetTouchSensitivityX(float value)
	{
		touchSensitivityX = value;
		SaveController.Save("touchSensitivityX", touchSensitivityX, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetTouchSensitivityY(float value)
	{
		touchSensitivityY = value;
		SaveController.Save("touchSensitivityY", touchSensitivityY, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetMobileJoystickSensitivityX(float value)
	{
		mobileJoystickSensitivityX = value;
		SaveController.Save("mobileJoystickSensitivityX", mobileJoystickSensitivityX, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetMobileJoystickSensitivityY(float value)
	{
		mobileJoystickSensitivityY = value;
		SaveController.Save("mobileJoystickSensitivityY", mobileJoystickSensitivityY, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetTouchInvertXIndex(int index)
	{
		SetTouchInvertX(index == 1);
	}

	private static void SetTouchInvertX(bool value)
	{
		TouchInvertXAxis = value;
		SaveController.Save("TouchInvertXAxis", TouchInvertXAxis, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetTouchInvertYIndex(int index)
	{
		SetTouchInvertY(index == 1);
	}

	private static void SetTouchInvertY(bool value)
	{
		TouchInvertYAxis = value;
		SaveController.Save("TouchInvertYAxis", TouchInvertYAxis, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetMobileJoystickInvertXIndex(int index)
	{
		SetMobileJoystickInvertX(index == 1);
	}

	private static void SetMobileJoystickInvertX(bool value)
	{
		mobileJoystickInvertXAxis = value;
		SaveController.Save("mobileJoystickInvertXAxis", mobileJoystickInvertXAxis, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private static void SetMobileJoystickInvertYIndex(int index)
	{
		SetMobileJoystickInvertY(index == 1);
	}

	private static void SetMobileJoystickInvertY(bool value)
	{
		mobileJoystickInvertYAxis = value;
		SaveController.Save("mobileJoystickInvertYAxis", mobileJoystickInvertYAxis, SaveData.Controls);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetUISize(float value)
	{
		UISize = value;
		SaveController.Save("UISize", UISize, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	public static void SetButtonTransparency(float value)
	{
		ButtonTransparency = value;
		SaveController.Save("ButtonTransparency", ButtonTransparency, SaveData.Settings);
		SettingsController.OnSettingsUpdated?.Invoke(Singleton<SettingsController>.Instance, EventArgs.Empty);
	}

	private void SceneController_OnSceneLoaded(object sender, EventArgs e)
	{
		StartCoroutine(DefaultSettingsCor());
	}
}
