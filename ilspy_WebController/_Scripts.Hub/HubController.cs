using System;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.Hub;

public class HubController : MonoBehaviour
{
	[Header("Kitchen")]
	[SerializeField]
	private GameObject kitchenGate;

	[SerializeField]
	private GameObject kitchenLoadingZone;

	[SerializeField]
	private LevelData kitchenLevelData;

	[Header("Office")]
	[SerializeField]
	private GameObject officeGate;

	[SerializeField]
	private GameObject officeLoadingZone;

	[SerializeField]
	private LevelData officeLevelData;

	[Header("Kids Room")]
	[SerializeField]
	private GameObject kidsRoomGate;

	[SerializeField]
	private GameObject kidsRoomLoadingZone;

	[SerializeField]
	private LevelData kidsRoomLevelData;

	[Header("Living Room")]
	[SerializeField]
	private GameObject livingRoomGate;

	[SerializeField]
	private GameObject livingRoomLoadingZone;

	[SerializeField]
	private LevelData livingRoomLevelData;

	[Header("Living Room")]
	[SerializeField]
	private GameObject hobbyRoomGate;

	[SerializeField]
	private GameObject hobbyRoomLoadingZone;

	[SerializeField]
	private GameObject endOfStoryLevelGameObject;

	private Collider kitchenLoadingZoneTrigger;

	private Collider officeLoadingZoneTrigger;

	private Collider kidsRoomLoadingZoneTrigger;

	private Collider livingRoomLoadingZoneTrigger;

	private void Start()
	{
		Singleton<ProfileController>.Instance.UnlockKitchen();
		kitchenLoadingZoneTrigger = kitchenLoadingZone.GetComponent<Collider>();
		officeLoadingZoneTrigger = officeLoadingZone.GetComponent<Collider>();
		kidsRoomLoadingZoneTrigger = kidsRoomLoadingZone.GetComponent<Collider>();
		livingRoomLoadingZoneTrigger = livingRoomLoadingZone.GetComponent<Collider>();
		UnlockKitchenLevel();
		UnlockOfficeLevel();
		UnlockKidsRoomLevel();
		UnlockLivingRoomLevel();
		UnlockHobbyRoomLevel();
		Singleton<GameController>.Instance.OnResetPlayer += GameController_OnResetPlayer;
	}

	private void UnlockKitchenLevel()
	{
		bool kitchenUnlocked = Singleton<ProfileController>.Instance.KitchenUnlocked;
		if (kitchenGate != null)
		{
			kitchenGate.SetActive(!kitchenUnlocked);
		}
		if (kitchenLoadingZone != null)
		{
			kitchenLoadingZone.SetActive(kitchenUnlocked);
		}
	}

	private void UnlockOfficeLevel()
	{
		bool officeUnlocked = Singleton<ProfileController>.Instance.OfficeUnlocked;
		if (officeGate != null)
		{
			officeGate.SetActive(!officeUnlocked);
		}
		if (officeLoadingZone != null)
		{
			officeLoadingZone.SetActive(officeUnlocked);
		}
	}

	private void UnlockKidsRoomLevel()
	{
		bool kidsRoomUnlocked = Singleton<ProfileController>.Instance.KidsRoomUnlocked;
		if (kidsRoomGate != null)
		{
			kidsRoomGate.SetActive(!kidsRoomUnlocked);
		}
		if (kidsRoomLoadingZone != null)
		{
			kidsRoomLoadingZone.SetActive(kidsRoomUnlocked);
		}
	}

	private void UnlockLivingRoomLevel()
	{
		bool livingRoomUnlocked = Singleton<ProfileController>.Instance.LivingRoomUnlocked;
		if (livingRoomGate != null)
		{
			livingRoomGate.SetActive(!livingRoomUnlocked);
		}
		if (livingRoomLoadingZone != null)
		{
			livingRoomLoadingZone.SetActive(livingRoomUnlocked);
		}
	}

	private void UnlockHobbyRoomLevel()
	{
		bool hobbyRoomUnlocked = Singleton<ProfileController>.Instance.HobbyRoomUnlocked;
		if (hobbyRoomGate != null)
		{
			hobbyRoomGate.SetActive(!hobbyRoomUnlocked);
		}
		if (hobbyRoomLoadingZone != null)
		{
			hobbyRoomLoadingZone.SetActive(hobbyRoomUnlocked);
		}
		endOfStoryLevelGameObject.SetActive(hobbyRoomUnlocked);
	}

	private void TryLoadLevel(LevelData levelData)
	{
		Singleton<SceneController>.Instance.LoadSpecificStoryLevel(levelData.sceneName);
	}

	public void LoadKitchenLevel()
	{
		TryLoadLevel(kitchenLevelData);
	}

	public void LoadOfficeLevel()
	{
		TryLoadLevel(officeLevelData);
	}

	public void LoadKidsRoomLevel()
	{
		TryLoadLevel(kidsRoomLevelData);
	}

	public void LoadLivingRoomLevel()
	{
		TryLoadLevel(livingRoomLevelData);
	}

	private void GameController_OnResetPlayer(object sender, EventArgs e)
	{
		kitchenLoadingZoneTrigger.enabled = true;
		officeLoadingZoneTrigger.enabled = true;
		kidsRoomLoadingZoneTrigger.enabled = true;
		livingRoomLoadingZoneTrigger.enabled = true;
	}
}
