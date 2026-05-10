using System;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;

namespace _Scripts.UI.Wheel_Customization;

public class EmoteWheelSlot : MonoBehaviour
{
	[SerializeField]
	private int slotIndex;

	[SerializeField]
	private Button button;

	[SerializeField]
	private Image colorSwatch;

	public static event Action<int> OnSlotClicked;

	private void Start()
	{
		button.onClick.AddListener(delegate
		{
			EmoteWheelSlot.OnSlotClicked?.Invoke(slotIndex);
		});
		colorSwatch.transform.rotation = Quaternion.identity;
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	public void SetColor(CosmeticItemWebSo cosmeticItemWebSo)
	{
		if ((bool)colorSwatch)
		{
			colorSwatch.sprite = cosmeticItemWebSo.webSo.webSprite;
		}
	}

	public void SetSlotIndex(int index)
	{
		slotIndex = index;
	}

	public void Select()
	{
		button.Select();
	}

	public void Deselect()
	{
		button.OnDeselect(null);
	}
}
