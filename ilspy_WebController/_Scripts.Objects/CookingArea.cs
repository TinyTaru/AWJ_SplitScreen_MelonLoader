using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.Utils;

namespace _Scripts.Objects;

public class CookingArea : MonoBehaviour
{
	[Serializable]
	public class CookingOperation
	{
		public float cookingTime;

		public CookingRecipeSO CookingRecipe;

		public List<CookingIngredient> Ingredients;

		public bool HasAllIngredients()
		{
			foreach (CookingIngredientSO ingredients in CookingRecipe.ingredientsList)
			{
				bool flag = false;
				foreach (CookingIngredient ingredient in Ingredients)
				{
					if (ingredient.IngredientSO == ingredients)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}
	}

	[SerializeField]
	private float cookingPower = 1f;

	[SerializeField]
	private bool canHavePans;

	[SerializeField]
	private bool canCookPlayer = true;

	[SerializeField]
	private CookingRecipeSO[] cookingRecipes;

	[SerializeField]
	private CookingIngredientSO specialCookingIngredientSo;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onCookingFinishedEvent;

	private bool isCooking;

	private List<BurnableObject> burnableObjects;

	private List<MeltableObject> meltableObjects;

	private List<CookingIngredient> cookingIngredients;

	private float backupCheckTimer;

	private readonly float backupCheckInterval = 1f;

	private bool performBackupCheck;

	private List<CookingOperation> cookingOperations;

	public bool IsCooking => isCooking;

	public int NumberOfSpecialIngredients => cookingIngredients.Count((CookingIngredient x) => x.IngredientSO == specialCookingIngredientSo);

	public event EventHandler OnStartCooking;

	public event EventHandler OnStopCooking;

	public event Action<int> OnCookingIngredientAdded;

	public event Action<int> OnCookingIngredientRemoved;

	public event Action<CookingRecipeSO> OnCookingFinished;

	private void Awake()
	{
		burnableObjects = new List<BurnableObject>();
		meltableObjects = new List<MeltableObject>();
		cookingIngredients = new List<CookingIngredient>();
		cookingOperations = new List<CookingOperation>();
		backupCheckTimer = backupCheckInterval;
		performBackupCheck = false;
	}

	private void Update()
	{
		if (!isCooking)
		{
			return;
		}
		for (int num = cookingOperations.Count - 1; num >= 0; num--)
		{
			CookingOperation cookingOperation = cookingOperations[num];
			if (cookingOperation.Ingredients.Any((CookingIngredient ingredient) => ingredient == null))
			{
				cookingOperations.Remove(cookingOperation);
			}
			else
			{
				cookingOperation.cookingTime += Time.deltaTime;
				CookingRecipeSO cookingRecipe = cookingOperation.CookingRecipe;
				if (cookingOperation.cookingTime >= cookingRecipe.cookingDuration)
				{
					Debug.Log("Finished cooking " + cookingRecipe.name);
					Vector3 position = Vector3.zero;
					if (cookingRecipe.spawnAmount == 0 && cookingRecipe.destroyIngredientAfterCooking)
					{
						foreach (CookingIngredient ingredient in cookingOperation.Ingredients)
						{
							position = ingredient.transform.position;
							cookingIngredients.Remove(ingredient);
							ingredient.GetMovableObject().DestroySafely();
						}
						if (!cookingRecipe.cookingFinishedSound.IsNull)
						{
							Singleton<MusicController>.Instance.PlayOneShot3D(cookingRecipe.cookingFinishedSound, position);
						}
					}
					else if (cookingRecipe.spawnAmount > 0)
					{
						foreach (CookingIngredient ingredient2 in cookingOperation.Ingredients)
						{
							position = ingredient2.transform.position;
							cookingIngredients.Remove(ingredient2);
							ingredient2.GetMovableObject().DestroySafely();
						}
						for (int i = 0; i < cookingRecipe.spawnAmount; i++)
						{
							MovableObject movableObject = UnityEngine.Object.Instantiate(cookingRecipe.cookingResult.RandomValue(), position, Quaternion.identity, null);
							SpawnableObject component = movableObject.GetComponent<SpawnableObject>();
							if (component != null)
							{
								component.Setup();
							}
							Vector3 normalized = new Vector3(UnityEngine.Random.Range(-1f, 1f), 1f, UnityEngine.Random.Range(-1f, 1f)).normalized;
							movableObject.GetRigidbody().AddForce(normalized * cookingRecipe.popForce, ForceMode.VelocityChange);
							movableObject.GetRigidbody().AddTorque(normalized * cookingRecipe.popTorque, ForceMode.VelocityChange);
						}
						if (!cookingRecipe.cookingFinishedSound.IsNull)
						{
							Singleton<MusicController>.Instance.PlayOneShot3D(cookingRecipe.cookingFinishedSound, position);
						}
					}
					if (cookingRecipe.cookingFinishedEffect != null)
					{
						UnityEngine.Object.Instantiate(cookingRecipe.cookingFinishedEffect, position, Quaternion.identity, null);
					}
					string questToFinish = cookingRecipe.questToFinish;
					if (questToFinish != string.Empty)
					{
						QuestLog.SetQuestState(questToFinish, QuestState.Success);
					}
					this.OnCookingFinished?.Invoke(cookingRecipe);
					onCookingFinishedEvent?.Invoke();
					cookingOperations.Remove(cookingOperation);
				}
			}
		}
		for (int num2 = burnableObjects.Count - 1; num2 >= 0; num2--)
		{
			BurnableObject burnableObject = burnableObjects[num2];
			if (burnableObject == null)
			{
				burnableObjects.Remove(burnableObject);
			}
			else
			{
				burnableObject.AddBurnAmount(cookingPower * Time.deltaTime);
				if (burnableObject.IsBurned)
				{
					burnableObjects.Remove(burnableObject);
					CookingIngredient component2 = burnableObject.GetComponent<CookingIngredient>();
					if (component2 != null)
					{
						RemoveCookingIngredient(component2);
					}
				}
			}
		}
		for (int num3 = meltableObjects.Count - 1; num3 >= 0; num3--)
		{
			MeltableObject meltableObject = meltableObjects[num3];
			if (meltableObject == null)
			{
				meltableObjects.Remove(meltableObject);
			}
			else
			{
				meltableObject.AddMeltAmount(cookingPower * Time.deltaTime);
				if (meltableObject.IsMelted)
				{
					meltableObjects.Remove(meltableObject);
					CookingIngredient component3 = meltableObject.GetComponent<CookingIngredient>();
					if (component3 != null)
					{
						RemoveCookingIngredient(component3);
					}
				}
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		BurnableObject componentInParent = other.GetComponentInParent<BurnableObject>();
		MeltableObject componentInParent2 = other.GetComponentInParent<MeltableObject>();
		CookingIngredient componentInParent3 = other.GetComponentInParent<CookingIngredient>();
		Pan componentInParent4 = other.GetComponentInParent<Pan>();
		if (componentInParent3 != null)
		{
			AddCookingIngredient(componentInParent3);
		}
		CookingRecipeSO[] array = cookingRecipes;
		foreach (CookingRecipeSO cookingRecipeSO in array)
		{
			CookingOperation cookingOperation = new CookingOperation
			{
				cookingTime = 0f,
				CookingRecipe = cookingRecipeSO,
				Ingredients = new List<CookingIngredient>()
			};
			foreach (CookingIngredient cookingIngredient in cookingIngredients)
			{
				if (cookingRecipeSO.ingredientsList.Contains(cookingIngredient.IngredientSO) && !cookingIngredient.IsInUse)
				{
					cookingOperation.Ingredients.Add(cookingIngredient);
				}
			}
			if (!cookingOperation.HasAllIngredients())
			{
				continue;
			}
			foreach (CookingIngredient ingredient in cookingOperation.Ingredients)
			{
				ingredient.SetIsInUse(value: true);
			}
			Debug.Log(cookingOperation.CookingRecipe.name + " is ready to be cooked!");
			cookingOperations.Add(cookingOperation);
		}
		if (componentInParent != null)
		{
			if (componentInParent.enabled && !componentInParent.IsBurned)
			{
				BodyMovement component = componentInParent.GetComponent<BodyMovement>();
				if ((!(component != null) || !component.IsPlayer || canCookPlayer) && !burnableObjects.Contains(componentInParent))
				{
					burnableObjects.Add(componentInParent);
				}
			}
		}
		else if (componentInParent2 != null)
		{
			if (componentInParent2.enabled && !componentInParent2.IsMelted && !meltableObjects.Contains(componentInParent2))
			{
				meltableObjects.Add(componentInParent2);
			}
		}
		else if (canHavePans && componentInParent4 != null)
		{
			componentInParent4.RegisterConnectedCookingArea(this);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		BurnableObject componentInParent = other.GetComponentInParent<BurnableObject>();
		MeltableObject componentInParent2 = other.GetComponentInParent<MeltableObject>();
		CookingIngredient componentInParent3 = other.GetComponentInParent<CookingIngredient>();
		Pan componentInParent4 = other.GetComponentInParent<Pan>();
		if (componentInParent3 != null)
		{
			RemoveCookingIngredient(componentInParent3);
		}
		if (componentInParent != null)
		{
			burnableObjects.Remove(componentInParent);
		}
		else if (componentInParent2 != null)
		{
			meltableObjects.Remove(componentInParent2);
		}
		else if (canHavePans && componentInParent4 != null)
		{
			componentInParent4.UnregisterConnectedCookingArea(this);
		}
	}

	private void FixedUpdate()
	{
		backupCheckTimer -= Time.fixedDeltaTime;
		if (backupCheckTimer <= 0f)
		{
			backupCheckTimer = backupCheckInterval;
			StartCoroutine(PerformPanCheckCoroutine());
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (performBackupCheck)
		{
			OnTriggerEnter(other);
		}
	}

	private IEnumerator PerformPanCheckCoroutine()
	{
		performBackupCheck = true;
		yield return new WaitForFixedUpdate();
		performBackupCheck = false;
	}

	private void AddCookingIngredient(CookingIngredient cookingIngredient)
	{
		if (cookingIngredient.IsNotBurnedOrMelted() && !cookingIngredients.Contains(cookingIngredient))
		{
			cookingIngredients.Add(cookingIngredient);
			this.OnCookingIngredientAdded?.Invoke(cookingIngredients.Count);
		}
	}

	private void RemoveCookingIngredient(CookingIngredient cookingIngredient)
	{
		cookingIngredient.SetIsInUse(value: false);
		for (int num = cookingOperations.Count - 1; num >= 0; num--)
		{
			CookingOperation cookingOperation = cookingOperations[num];
			if (cookingOperation.Ingredients.Contains(cookingIngredient))
			{
				cookingOperations.Remove(cookingOperation);
				foreach (CookingIngredient ingredient in cookingOperation.Ingredients)
				{
					ingredient.SetIsInUse(value: false);
				}
			}
		}
		cookingIngredients.Remove(cookingIngredient);
		this.OnCookingIngredientRemoved?.Invoke(cookingIngredients.Count);
	}

	public void StartCooking()
	{
		isCooking = true;
	}

	public void StopCooking()
	{
		isCooking = false;
	}
}
