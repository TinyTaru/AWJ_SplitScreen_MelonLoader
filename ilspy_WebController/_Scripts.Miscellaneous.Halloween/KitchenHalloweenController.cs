using System;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.LevelSaving;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous.Halloween;

public class KitchenHalloweenController : Singleton<KitchenHalloweenController>, IInitializable<KitchenHalloweenControllerSaveData>, IHasSaveData<KitchenHalloweenControllerSaveData>
{
	[Header("References")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private GameObject halloweenObjects;

	private bool halloweenEventIsActive;

	private bool halloweenObjectsAreActive;

	public bool HalloweenEventIsActive => halloweenEventIsActive;

	public bool HalloweenObjectsAreActive => halloweenObjectsAreActive;

	public event EventHandler OnActivateHalloweenEffects;

	private void ActivateHalloweenObjects()
	{
		halloweenObjects.SetActive(value: true);
		Singleton<MusicController>.Instance.SetHalloweenParameter(1f);
		DialogueLua.SetVariable("Kitchen.HalloweenEventActive", true);
		this.OnActivateHalloweenEffects?.Invoke(this, EventArgs.Empty);
	}

	public void Initialize(KitchenHalloweenControllerSaveData saveData)
	{
		halloweenEventIsActive = saveData.halloweenEventIsActive;
		halloweenObjectsAreActive = saveData.halloweenObjectsAreActive;
		if (halloweenObjectsAreActive)
		{
			ActivateHalloweenObjects();
		}
	}

	public KitchenHalloweenControllerSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Kitchen Halloween Controller " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		KitchenHalloweenControllerSaveData result = default(KitchenHalloweenControllerSaveData);
		result.id = id;
		result.halloweenEventIsActive = halloweenEventIsActive;
		result.halloweenObjectsAreActive = halloweenObjectsAreActive;
		return result;
	}

	public void StartHalloween()
	{
		if (!halloweenEventIsActive)
		{
			halloweenEventIsActive = true;
			animator.SetTrigger("StartHalloweenEvent");
		}
	}

	public void StopHalloween()
	{
		if (halloweenEventIsActive)
		{
			halloweenEventIsActive = false;
			Singleton<BackgroundController>.Instance.ActivateBackgroundDefault();
		}
	}

	public void ActivateHalloweenEffects()
	{
		halloweenObjectsAreActive = true;
		ActivateHalloweenObjects();
		Singleton<BackgroundController>.Instance.ActivateBackgroundHalloween();
	}
}
