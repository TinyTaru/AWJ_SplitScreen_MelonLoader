using UnityEngine;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
[DisallowMultipleComponent]
public class CookingIngredient : MonoBehaviour
{
	[SerializeField]
	private CookingIngredientSO ingredientSO;

	private MovableObject movableObject;

	private BurnableObject burnableObject;

	private MeltableObject meltableObject;

	private bool isInUse;

	public bool IsInUse => isInUse;

	public CookingIngredientSO IngredientSO => ingredientSO;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
		burnableObject = GetComponent<BurnableObject>();
		meltableObject = GetComponent<MeltableObject>();
	}

	public bool IsNotBurnedOrMelted()
	{
		if (burnableObject != null)
		{
			return !burnableObject.IsBurned;
		}
		if (meltableObject != null)
		{
			return !meltableObject.IsMelted;
		}
		return false;
	}

	public void SetIsInUse(bool value)
	{
		isInUse = value;
	}

	public MovableObject GetMovableObject()
	{
		return movableObject;
	}
}
