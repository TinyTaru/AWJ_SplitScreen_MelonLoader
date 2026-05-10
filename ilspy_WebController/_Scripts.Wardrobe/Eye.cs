using System;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;

namespace _Scripts.Wardrobe;

public class Eye : MonoBehaviour
{
	[SerializeField]
	private GameObject normalEyes;

	[SerializeField]
	private GameObject arachnophobiaEyes;

	[SerializeField]
	private SpriteRenderer[] eyeBaseSpriteRenderers;

	[SerializeField]
	private SpriteRenderer[] eyeLeftSpriteRenderers;

	[SerializeField]
	private SpriteRenderer[] eyeRightSpriteRenderers;

	[SerializeField]
	[Range(0f, 3f)]
	private int numberOfColors = 3;

	[SerializeField]
	private Color defaultColorBase;

	[SerializeField]
	private Color defaultColorLeft;

	[SerializeField]
	private Color defaultColorRight;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent<Color> onEyeColorBaseChangedEvent;

	[SerializeField]
	private UnityEvent<Color> onEyeColorLeftChangedEvent;

	[SerializeField]
	private UnityEvent<Color> onEyeColorRightChangedEvent;

	[SerializeField]
	private CosmeticItemEffect[] effects;

	public Color DefaultColorBase => defaultColorBase;

	public Color DefaultColorLeft => defaultColorLeft;

	public Color DefaultColorRight => defaultColorRight;

	public int NumberOfColors => numberOfColors;

	public int NumberOfEffects => effects.Length;

	private void Start()
	{
		ActivateCorrectEyes();
	}

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
			SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		}
	}

	private void ActivateCorrectEyes()
	{
		bool arachnophobiaMode = SettingsController.ArachnophobiaMode;
		if (normalEyes != null)
		{
			normalEyes.SetActive(!arachnophobiaMode);
		}
		if (arachnophobiaEyes != null)
		{
			arachnophobiaEyes.SetActive(arachnophobiaMode);
		}
	}

	private void OnColorChanged()
	{
		SetEyeColor(0, defaultColorBase);
		SetEyeColor(1, defaultColorLeft);
		SetEyeColor(2, defaultColorRight);
	}

	public void SetEyeColor(int index, Color color)
	{
		if (numberOfColors == 0)
		{
			return;
		}
		switch (index)
		{
		case 0:
			if (eyeBaseSpriteRenderers != null)
			{
				SpriteRenderer[] array = eyeBaseSpriteRenderers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = color;
				}
				onEyeColorBaseChangedEvent?.Invoke(color);
			}
			break;
		case 1:
			if (eyeLeftSpriteRenderers != null)
			{
				SpriteRenderer[] array = eyeLeftSpriteRenderers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = color;
				}
				onEyeColorLeftChangedEvent?.Invoke(color);
			}
			break;
		case 2:
			if (eyeRightSpriteRenderers != null)
			{
				SpriteRenderer[] array = eyeRightSpriteRenderers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = color;
				}
				onEyeColorRightChangedEvent?.Invoke(color);
			}
			break;
		}
	}

	public CosmeticItemEffect GetEffect(int index)
	{
		if (index < NumberOfEffects)
		{
			return effects[index];
		}
		return null;
	}

	public float GetDefaultEffect(int index)
	{
		if (index < NumberOfEffects)
		{
			return effects[index].DefaultValue;
		}
		return 0f;
	}

	public void SetEyeEffect(int index, float newValue)
	{
		if (NumberOfEffects != 0 && index < NumberOfEffects)
		{
			effects[index].SetValue(newValue);
		}
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		ActivateCorrectEyes();
	}
}
