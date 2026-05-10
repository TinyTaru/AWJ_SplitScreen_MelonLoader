using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Wardrobe;

[Serializable]
public class CosmeticItemColor
{
	[SerializeField]
	private string name;

	[SerializeField]
	private Color defaultColor;

	[SerializeField]
	private int[] colorMaterialIndices;

	[Space(5f)]
	[SerializeField]
	private UnityEvent<Color> onColorChangedEvent;

	public Color DefaultColor
	{
		get
		{
			return defaultColor;
		}
		set
		{
			defaultColor = value;
		}
	}

	public int[] ColorMaterialIndices => colorMaterialIndices;

	public string Name => name;

	public void SetColor(Color newColor)
	{
		defaultColor = newColor;
		onColorChangedEvent?.Invoke(newColor);
	}
}
