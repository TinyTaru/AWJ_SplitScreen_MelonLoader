using System;
using PixelCrushers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Settings;

public class SettingsSlider : MonoBehaviour
{
	[SerializeField]
	private SliderType sliderType;

	[Header("Labels")]
	[SerializeField]
	private string settingName;

	[SerializeField]
	private string labelLeft;

	[SerializeField]
	private string labelRight;

	[Header("Localization")]
	[SerializeField]
	private string settingNameLocalization;

	[SerializeField]
	private string labelLeftLocalization;

	[SerializeField]
	private string labelRightLocalization;

	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI settingNameText;

	[SerializeField]
	private TextMeshProUGUI labelLeftText;

	[SerializeField]
	private TextMeshProUGUI labelRightText;

	private Slider slider;

	private void Awake()
	{
		slider = GetComponentInChildren<Slider>();
		slider.onValueChanged.AddListener(SetValue);
	}

	public void OnEnable()
	{
		switch (sliderType)
		{
		case SliderType.MasterVolume:
			slider.SetValueWithoutNotify(SettingsController.MasterVolume);
			break;
		case SliderType.MusicVolume:
			slider.SetValueWithoutNotify(SettingsController.MusicVolume);
			break;
		case SliderType.SoundVolume:
			slider.SetValueWithoutNotify(SettingsController.SoundVolume);
			break;
		case SliderType.UiVolume:
			slider.SetValueWithoutNotify(SettingsController.UiVolume);
			break;
		case SliderType.DialogueVolume:
			slider.SetValueWithoutNotify(SettingsController.DialogueVolume);
			break;
		case SliderType.Brightness:
			slider.SetValueWithoutNotify(SettingsController.Brightness);
			break;
		case SliderType.Contrast:
			slider.SetValueWithoutNotify(SettingsController.Contrast);
			break;
		case SliderType.Saturation:
			slider.SetValueWithoutNotify(SettingsController.Saturation);
			break;
		case SliderType.TrailerVolume:
			slider.SetValueWithoutNotify(SettingsController.TrailerVolume);
			break;
		case SliderType.Outline:
			slider.SetValueWithoutNotify(SettingsController.Outline);
			break;
		case SliderType.FieldOfView:
			slider.SetValueWithoutNotify(SettingsController.FieldOfView);
			break;
		case SliderType.UISize:
			slider.SetValueWithoutNotify(SettingsController.UISize);
			break;
		case SliderType.MouseX:
			slider.SetValueWithoutNotify(SettingsController.MouseSensitivityX);
			break;
		case SliderType.MouseY:
			slider.SetValueWithoutNotify(SettingsController.MouseSensitivityY);
			break;
		case SliderType.GamepadX:
			slider.SetValueWithoutNotify(SettingsController.GamepadSensitivityX);
			break;
		case SliderType.GamepadY:
			slider.SetValueWithoutNotify(SettingsController.GamepadSensitivityY);
			break;
		case SliderType.MobileJoystickX:
			slider.SetValueWithoutNotify(SettingsController.mobileJoystickSensitivityX);
			break;
		case SliderType.MobileJoystickY:
			slider.SetValueWithoutNotify(SettingsController.mobileJoystickSensitivityY);
			break;
		case SliderType.TouchX:
			slider.SetValueWithoutNotify(SettingsController.touchSensitivityX);
			break;
		case SliderType.TouchY:
			slider.SetValueWithoutNotify(SettingsController.touchSensitivityY);
			break;
		case SliderType.QuestMarkerSize:
			slider.SetValueWithoutNotify(SettingsController.QuestMarkerSize);
			break;
		case SliderType.RenderScale:
			slider.SetValueWithoutNotify(SettingsController.RenderScale);
			break;
		case SliderType.ButtonTransparency:
			slider.SetValueWithoutNotify(SettingsController.ButtonTransparency);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void UpdateTexts()
	{
		settingNameText.text = settingName;
		labelLeftText.text = labelLeft;
		labelRightText.text = labelRight;
		LocalizeUI component = settingNameText.GetComponent<LocalizeUI>();
		if (component != null)
		{
			component.fieldName = settingNameLocalization;
		}
		component = labelLeftText.GetComponent<LocalizeUI>();
		if (component != null)
		{
			component.fieldName = labelLeftLocalization;
		}
		component = labelRightText.GetComponent<LocalizeUI>();
		if (component != null)
		{
			component.fieldName = labelRightLocalization;
		}
	}

	private void SetValue(float value)
	{
		switch (sliderType)
		{
		case SliderType.MasterVolume:
			SettingsController.SetMasterVolume(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.MusicVolume:
			SettingsController.SetMusicVolume(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.SoundVolume:
			SettingsController.SetSoundVolume(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.UiVolume:
			SettingsController.SetUiVolume(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.DialogueVolume:
			SettingsController.SetDialogueVolume(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.Brightness:
			SettingsController.SetBrightness(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.Contrast:
			SettingsController.SetContrast(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.Saturation:
			SettingsController.SetSaturation(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.TrailerVolume:
			SettingsController.SetTrailerVolume(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.Outline:
			SettingsController.SetOutlineEdgeColorAlpha(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.FieldOfView:
			SettingsController.SetFieldOfView(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.UISize:
			SettingsController.SetUISize(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.MouseX:
			SettingsController.SetMouseSensitivityX(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.MouseY:
			SettingsController.SetMouseSensitivityY(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.GamepadX:
			SettingsController.SetGamepadSensitivityX(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.GamepadY:
			SettingsController.SetGamepadSensitivityY(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.MobileJoystickX:
			SettingsController.SetMobileJoystickSensitivityX(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.MobileJoystickY:
			SettingsController.SetMobileJoystickSensitivityY(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.TouchX:
			SettingsController.SetTouchSensitivityX(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.TouchY:
			SettingsController.SetTouchSensitivityY(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.QuestMarkerSize:
			SettingsController.SetQuestMarkerSize(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.RenderScale:
			SettingsController.SetRenderScale(value);
			slider.SetValueWithoutNotify(value);
			break;
		case SliderType.ButtonTransparency:
			SettingsController.SetButtonTransparency(value);
			slider.SetValueWithoutNotify(value);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
