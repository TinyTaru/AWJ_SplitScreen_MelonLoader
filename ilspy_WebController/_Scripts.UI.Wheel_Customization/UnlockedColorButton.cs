using System;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;

namespace _Scripts.UI.Wheel_Customization;

public class UnlockedColorButton : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Button button;

	[SerializeField]
	private Image colorSwatch;

	private CosmeticItemWebSo cosmeticItemWebSo;

	public static event Action<UnlockedColorButton, CosmeticItemWebSo> OnColorButtonPressed;

	private void SelectButton()
	{
		UnlockedColorButton.OnColorButtonPressed?.Invoke(this, cosmeticItemWebSo);
	}

	public void SetInteractable(bool interactable)
	{
		button.interactable = interactable;
	}

	public void Setup(CosmeticItemWebSo newCosmeticItemWebSo)
	{
		cosmeticItemWebSo = newCosmeticItemWebSo;
		if ((bool)colorSwatch)
		{
			colorSwatch.sprite = newCosmeticItemWebSo.webSo.webSprite;
		}
		button.onClick.AddListener(SelectButton);
	}

	public Button GetButton()
	{
		return button;
	}

	public void Select()
	{
		button.Select();
	}
}
