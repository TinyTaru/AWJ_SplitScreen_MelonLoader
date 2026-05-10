using System;
using UnityEngine;

namespace _Scripts.LivingRoom;

public class AquariumLightControl : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer colorIndicator;

	[SerializeField]
	private int colorIndicatorMaterialIndex;

	[SerializeField]
	private Transform saturationKnob;

	[SerializeField]
	private Transform valueKnob;

	[SerializeField]
	private Color[] buttonColors;

	[SerializeField]
	private float knobAngleLimit = 135f;

	[SerializeField]
	private float saturationAndValueChangeThreshold = 0.05f;

	[SerializeField]
	private Vector2 saturationRange;

	[SerializeField]
	private Vector2 valueRange;

	[SerializeField]
	private float defaultSaturationKnobAngle = -90f;

	[SerializeField]
	private float defaultValueKnobAngle = 90f;

	private Color color;

	private float hue;

	private float saturation;

	private float value;

	private float oldSaturationValue;

	private float oldValueValue;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	public event Action<Color> OnColorChanged;

	private void Start()
	{
		saturationKnob.transform.localRotation = Quaternion.Euler(0f, 0f, defaultSaturationKnobAngle);
		valueKnob.transform.localRotation = Quaternion.Euler(0f, 0f, defaultValueKnobAngle);
		OnButtonPressed(4);
		HandleSaturationUpdate(isInitialize: true);
		HandleValueUpdate(isInitialize: true);
		UpdateColor();
	}

	private void Update()
	{
		HandleSaturationUpdate();
		HandleValueUpdate();
	}

	private void UpdateColor()
	{
		color = Color.HSVToRGB(hue, saturation, value);
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		colorIndicator.GetPropertyBlock(mpb, colorIndicatorMaterialIndex);
		mpb.SetColor(colorId, color);
		colorIndicator.SetPropertyBlock(mpb, colorIndicatorMaterialIndex);
		Debug.Log($"New aquarium color: {color} hue:{hue:F4} saturation:{saturation:F4} value:{value:F4}");
		this.OnColorChanged?.Invoke(color);
	}

	private void HandleSaturationUpdate(bool isInitialize = false)
	{
		float num = saturationKnob.localRotation.eulerAngles.z;
		if (num > 180f)
		{
			num -= 360f;
		}
		float num2 = Mathf.InverseLerp(0f - knobAngleLimit, knobAngleLimit, num);
		if (Mathf.Abs(num2 - oldSaturationValue) > saturationAndValueChangeThreshold || isInitialize)
		{
			saturation = Mathf.Lerp(saturationRange.x, saturationRange.y, num2);
			UpdateColor();
			oldSaturationValue = num2;
		}
	}

	private void HandleValueUpdate(bool isInitialize = false)
	{
		float num = valueKnob.localRotation.eulerAngles.z;
		if (num > 180f)
		{
			num -= 360f;
		}
		float num2 = Mathf.InverseLerp(0f - knobAngleLimit, knobAngleLimit, num);
		if (Mathf.Abs(num2 - oldValueValue) > saturationAndValueChangeThreshold || isInitialize)
		{
			value = Mathf.Lerp(valueRange.x, valueRange.y, num2);
			UpdateColor();
			oldValueValue = num2;
		}
	}

	public void OnButtonPressed(int index)
	{
		Color.RGBToHSV(buttonColors[index], out hue, out var _, out var _);
		Debug.Log($"Hue set to {hue}");
		UpdateColor();
	}
}
