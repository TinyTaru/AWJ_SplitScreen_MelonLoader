using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Emotes;

[Serializable]
public class WheelSlice : MonoBehaviour
{
	[SerializeField]
	private Image backgroundImage;

	protected bool interactable = true;

	public bool Interactable => interactable;

	public virtual void Setup()
	{
	}

	public void SetBackgroundImageColor(Color color)
	{
		backgroundImage.color = color;
	}
}
