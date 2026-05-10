using System.Collections.Generic;
using FMODUnity;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.CosmeticItems;

namespace _Scripts.Objects;

[CreateAssetMenu(fileName = "New Cooking Recipe", menuName = "FTG/New Cooking Recipe")]
public class CookingRecipeSO : ScriptableObject
{
	public List<CookingIngredientSO> ingredientsList;

	public float cookingDuration = 1f;

	public int spawnAmount = 1;

	public bool destroyIngredientAfterCooking;

	public MovableObject[] cookingResult;

	public float popForce;

	public float popTorque;

	public GameObject cookingFinishedEffect;

	[QuestPopup(false)]
	public string questToFinish;

	public EventReference cookingFinishedSound;

	[FormerlySerializedAs("unlockableSO")]
	public CosmeticItemSo cosmeticItemSo;
}
