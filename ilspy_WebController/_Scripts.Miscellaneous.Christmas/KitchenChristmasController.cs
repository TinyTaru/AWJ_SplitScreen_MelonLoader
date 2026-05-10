using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous.Christmas;

public class KitchenChristmasController : Singleton<KitchenChristmasController>, IInitializable<KitchenChristmasControllerSaveData>, IHasSaveData<KitchenChristmasControllerSaveData>
{
	[Header("Debug")]
	[SerializeField]
	private bool startEventImmediate;

	[Header("References")]
	[SerializeField]
	private Animator animator;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onStartChristmasEvent;

	[SerializeField]
	private UnityEvent onActivateChristmasEffectsEvent;

	private bool christmasEventIsActive;

	public bool ChristmasEventIsActive => christmasEventIsActive;

	public event EventHandler OnActivateChristmasEffects;

	private void Start()
	{
		if (startEventImmediate)
		{
			ActivateChristmasEffects();
		}
	}

	public void Initialize(KitchenChristmasControllerSaveData saveData)
	{
		christmasEventIsActive = saveData.christmasEventIsActive;
		if (christmasEventIsActive)
		{
			ActivateChristmasEffects();
		}
	}

	public KitchenChristmasControllerSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Kitchen Christmas Controller " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		KitchenChristmasControllerSaveData result = default(KitchenChristmasControllerSaveData);
		result.id = id;
		result.christmasEventIsActive = christmasEventIsActive;
		return result;
	}

	public void StartChristmas()
	{
		if (!christmasEventIsActive)
		{
			Singleton<MusicController>.Instance.SetChristmasParameter(1f);
			christmasEventIsActive = true;
			onStartChristmasEvent?.Invoke();
			animator.SetTrigger("StartChristmasEvent");
		}
	}

	public void ActivateChristmasEffects()
	{
		DialogueLua.SetVariable("Kitchen.ChristmasEventActive", true);
		Singleton<MusicController>.Instance.SetChristmasParameter(1f);
		Singleton<BackgroundController>.Instance.ActivateBackgroundChristmas();
		onActivateChristmasEffectsEvent?.Invoke();
		this.OnActivateChristmasEffects?.Invoke(this, EventArgs.Empty);
	}

	public void DisableObjectSafely(GameObject gameObjectToDisable)
	{
		DontDestroyMe[] componentsInChildren = gameObjectToDisable.GetComponentsInChildren<DontDestroyMe>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetParent();
		}
		gameObjectToDisable.SetActive(value: false);
	}
}
