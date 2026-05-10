using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.UI.Selections;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe._3._0;

public class OutfitButton : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Button outfitButton;

	[SerializeField]
	private Button deleteOutfitButton;

	[SerializeField]
	private TextMeshProUGUI outfitLabel;

	[SerializeField]
	private SelectOnEnable selectOnEnable;

	private Outfit outfit;

	private OutfitController outfitController;

	public static event Action<OutfitButton> OnOutfitSelected;

	public void Setup(OutfitController outfitController, Outfit outfit)
	{
		this.outfitController = outfitController;
		this.outfit = outfit;
		outfitLabel.text = outfit.name;
	}

	public Outfit GetOutfit()
	{
		return outfit;
	}

	public Button GetOutfitButton()
	{
		return outfitButton;
	}

	public void ToggleOnEnable()
	{
		selectOnEnable.enabled = !selectOnEnable.enabled;
	}

	public void ApplyOutfit()
	{
		outfitController.ApplyOutfit(outfit);
	}

	public void Edit()
	{
		outfitController.ShowSaveSlotRenamingPanel(outfit);
	}

	public void Delete()
	{
		outfitController.ShowSaveSlotDeletionPanel(outfit);
	}

	public void Select()
	{
		outfitButton.Select();
	}

	public void OnOutfitButtonSelected()
	{
		OutfitButton.OnOutfitSelected?.Invoke(this);
	}
}
