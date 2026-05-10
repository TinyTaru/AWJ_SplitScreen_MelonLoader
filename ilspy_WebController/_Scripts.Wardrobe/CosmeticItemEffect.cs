using System;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;

namespace _Scripts.Wardrobe;

[Serializable]
public class CosmeticItemEffect
{
	[SerializeField]
	private string name;

	[Space(10f)]
	[SerializeField]
	private CosmeticItemEffectType type;

	[SerializeField]
	private float defaultValue;

	[SerializeField]
	private float minValue;

	[SerializeField]
	private float maxValue = 1f;

	[SerializeField]
	private bool integerValuesOnly;

	[SerializeField]
	private float sliderMoveStep = -1f;

	[Space(10f)]
	[SerializeField]
	private SpiderMode mode;

	[Space(10f)]
	[SerializeField]
	private UnityEvent<float> onValueChangedEvent;

	public string Name => name;

	public CosmeticItemEffectType GetCosmeticItemEffectType => type;

	public float DefaultValue => defaultValue;

	public float MinValue => minValue;

	public float MaxValue => maxValue;

	public float SliderMoveStep => sliderMoveStep;

	public bool IntegerValuesOnly => integerValuesOnly;

	public SpiderMode Mode => mode;

	public void SetValue(float newValue)
	{
		float num = newValue;
		switch (type)
		{
		case CosmeticItemEffectType.Toggle:
			num = Mathf.Clamp01(num);
			break;
		case CosmeticItemEffectType.Slider:
			num = Mathf.Clamp(newValue, minValue, maxValue);
			if (integerValuesOnly)
			{
				num = Mathf.RoundToInt(num);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case CosmeticItemEffectType.None:
			break;
		}
		onValueChangedEvent?.Invoke(num);
	}
}
