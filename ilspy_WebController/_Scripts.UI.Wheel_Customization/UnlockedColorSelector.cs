using System.Collections.Generic;
using UnityEngine;
using _Scripts.CosmeticItems;

namespace _Scripts.UI.Wheel_Customization;

public class UnlockedColorSelector : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private Transform container;

	[SerializeField]
	private UnlockedColorButton colorButtonPrefab;

	private List<UnlockedColorButton> buttons = new List<UnlockedColorButton>();

	private void ClearButtons()
	{
		for (int num = buttons.Count - 1; num >= 0; num--)
		{
			Object.Destroy(buttons[num].gameObject);
		}
		buttons = new List<UnlockedColorButton>();
	}

	public List<UnlockedColorButton> GetAllButtons()
	{
		return buttons;
	}

	public void InitializeUnlockedColors(List<CosmeticItemWebSo> unlockedCosmeticWebItems)
	{
		ClearButtons();
		foreach (CosmeticItemWebSo unlockedCosmeticWebItem in unlockedCosmeticWebItems)
		{
			UnlockedColorButton unlockedColorButton = Object.Instantiate(colorButtonPrefab, container);
			unlockedColorButton.Setup(unlockedCosmeticWebItem);
			buttons.Add(unlockedColorButton);
		}
		if (buttons.Count > 0)
		{
			buttons[0].GetButton().Select();
		}
	}
}
