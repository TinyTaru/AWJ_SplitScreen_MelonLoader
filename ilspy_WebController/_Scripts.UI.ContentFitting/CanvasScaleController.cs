using System;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Singletons;

namespace _Scripts.UI.ContentFitting;

public class CanvasScaleController : MonoBehaviour
{
	private enum ScreenOrientation
	{
		Horizontal,
		Vertical
	}

	[SerializeField]
	private ScreenOrientation screenOrientation;

	[SerializeField]
	private float width = 1920f;

	[SerializeField]
	private float height = 1080f;

	private void OnEnable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
	}

	private void OnDisable()
	{
		if (Singleton<SettingsController>.Instance != null)
		{
			SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		}
	}

	private void Start()
	{
		SetScaleMode();
	}

	private void SetScaleMode()
	{
		if (!this)
		{
			return;
		}
		CanvasScaler component = GetComponent<CanvasScaler>();
		if ((bool)component)
		{
			component.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			component.referenceResolution = new Vector2(width, height);
			float num = 0f;
			float num2 = 0f;
			switch (screenOrientation)
			{
			case ScreenOrientation.Horizontal:
				num2 = width / height;
				num = (float)Screen.width / (float)Screen.height / num2;
				break;
			case ScreenOrientation.Vertical:
				num2 = height / width;
				num = (float)Screen.height / (float)Screen.width / num2;
				break;
			}
			float matchWidthOrHeight = ((num < 1f) ? 0f : 1f);
			base.gameObject.GetComponent<CanvasScaler>().matchWidthOrHeight = matchWidthOrHeight;
		}
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		SetScaleMode();
	}
}
