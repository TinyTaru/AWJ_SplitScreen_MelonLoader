using System;
using System.Collections.Generic;
using PixelCrushers;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Settings;

public class SettingsSelection : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private DefaultSettingType settingType;

	[SerializeField]
	private string settingName;

	[SerializeField]
	private string settingNameLocalization;

	[SerializeField]
	private bool playClickSoundOnValueChanged = true;

	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI settingNameText;

	[SerializeField]
	private TextMeshProUGUI textField;

	[SerializeField]
	private GameObject leftArrow;

	[SerializeField]
	private GameObject rightArrow;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference navLeft;

	[SerializeField]
	private InputActionReference navRight;

	[SerializeField]
	private InputActionReference enter;

	[Header("Events")]
	[SerializeField]
	private UnityEvent<int> onValueChangedEvent;

	private Selectable selectable;

	private List<string> fields;

	private int currIndex;

	private void Awake()
	{
		selectable = GetComponentInChildren<Selectable>();
	}

	private void OnEnable()
	{
		if (!(Singleton<SettingsController>.Instance == null))
		{
			currIndex = SettingsController.GetDefaultValue(settingType);
			fields = SettingsController.GetFields(settingType);
			UpdateTextField();
			UpdateArrows();
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
	}

	private void OnDisable()
	{
		if (!(Singleton<SettingsController>.Instance == null))
		{
			SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		}
	}

	private void Update()
	{
		if (navLeft.action.WasPerformedThisFrame() && EventSystem.current.currentSelectedGameObject == selectable.gameObject && currIndex > 0)
		{
			currIndex--;
			SetValue();
		}
		if (navRight.action.WasPerformedThisFrame() && EventSystem.current.currentSelectedGameObject == selectable.gameObject && currIndex < fields.Count - 1)
		{
			currIndex++;
			SetValue();
		}
		if (enter.action.WasPerformedThisFrame() && EventSystem.current.currentSelectedGameObject == selectable.gameObject)
		{
			currIndex = (currIndex + 1) % fields.Count;
			SetValue();
		}
	}

	private void SetSettingName()
	{
		settingNameText.text = settingName;
		LocalizeUI component = settingNameText.GetComponent<LocalizeUI>();
		if (component != null)
		{
			component.fieldName = settingNameLocalization;
		}
	}

	private void SetValue()
	{
		SettingsController.SetValue(currIndex, settingType);
		if (playClickSoundOnValueChanged)
		{
			Singleton<MusicController>.Instance.ClickSound();
		}
		UpdateTextField();
		UpdateArrows();
		onValueChangedEvent?.Invoke(currIndex);
	}

	private void UpdateArrows()
	{
		if (fields != null)
		{
			if (currIndex == 0)
			{
				leftArrow.SetActive(value: false);
				rightArrow.SetActive(value: true);
			}
			else if (currIndex == fields.Count - 1)
			{
				rightArrow.SetActive(value: false);
				leftArrow.SetActive(value: true);
			}
			else
			{
				leftArrow.SetActive(value: true);
				rightArrow.SetActive(value: true);
			}
		}
	}

	private void UpdateTextField()
	{
		if (textField != null && fields != null)
		{
			textField.text = DialogueManager.GetLocalizedText(fields[currIndex]);
		}
	}

	public void ChangeIndex(int val)
	{
		selectable.Select();
		selectable.OnSelect(null);
		if (currIndex > 0 && val == -1)
		{
			currIndex--;
			SetValue();
		}
		else if (currIndex < fields.Count - 1 && val == 1)
		{
			currIndex++;
			SetValue();
		}
	}

	public void SetValueWithoutNotify(int newIndex)
	{
		currIndex = newIndex;
		UpdateTextField();
		UpdateArrows();
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateTextField();
	}
}
