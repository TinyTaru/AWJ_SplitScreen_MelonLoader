using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Objects;
using _Scripts.Utils;

namespace _Scripts.LivingRoom;

public class AncientPotionsQuest : MonoBehaviour
{
	[Serializable]
	public class TabletShopItemSoMappingDictionary : UnitySerializedDictionary<CookingIngredientSO, TabletShopItemSo>
	{
	}

	[SerializeField]
	private CookingArea cookingArea;

	[SerializeField]
	[VariablePopup(false)]
	private string ancientPotionIngredientsVariable;

	[SerializeField]
	private TabletShopItemSoMappingDictionary tabletShopItemSoMapping;

	[SerializeField]
	private CookingRecipeSO noGravityRecipeSo;

	[SerializeField]
	private TabletShopItemSo noGravityShopItemSo;

	private Tablet tablet;

	private void Start()
	{
		cookingArea.StartCooking();
		cookingArea.OnCookingIngredientAdded += CookingArea_OnCookingIngredientAdded;
		cookingArea.OnCookingFinished += CookingAreaOnCookingFinished;
		tablet = UnityEngine.Object.FindFirstObjectByType<Tablet>();
	}

	private void OnDestroy()
	{
		cookingArea.OnCookingIngredientAdded -= CookingArea_OnCookingIngredientAdded;
	}

	private void CookingArea_OnCookingIngredientAdded(int amount)
	{
		DialogueLua.SetVariable(ancientPotionIngredientsVariable, amount);
	}

	private void CookingAreaOnCookingFinished(CookingRecipeSO recipe)
	{
		if (tablet == null)
		{
			return;
		}
		foreach (CookingIngredientSO ingredients in recipe.ingredientsList)
		{
			if (tabletShopItemSoMapping.TryGetValue(ingredients, out var value))
			{
				tablet.AddShopItem(value);
			}
		}
		if (recipe == noGravityRecipeSo)
		{
			tablet.AddShopItem(noGravityShopItemSo);
		}
	}
}
