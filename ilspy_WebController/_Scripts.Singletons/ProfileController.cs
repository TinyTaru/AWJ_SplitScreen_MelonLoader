using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Scripts.General;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.Singletons;

public class ProfileController : Singleton<ProfileController>
{
	public class OnLevelUnlockedEventArgs : EventArgs
	{
		public LevelData levelData;
	}

	[Header("Level Data")]
	[SerializeField]
	private LevelData tutorialLevelData;

	[SerializeField]
	private LevelData kitchenLevelData;

	[SerializeField]
	private LevelData officeLevelData;

	[SerializeField]
	private LevelData kidsRoomLevelData;

	[SerializeField]
	private LevelData livingRoomLevelData;

	[SerializeField]
	private LevelData hobbyRoomLevelData;

	[Header("Profile Sprites")]
	[SerializeField]
	private Sprite profile1Sprite;

	[SerializeField]
	private Sprite profile1SpriteArachnophobia;

	[SerializeField]
	private Sprite profile2Sprite;

	[SerializeField]
	private Sprite profile2SpriteArachnophobia;

	[SerializeField]
	private Sprite profile3Sprite;

	[SerializeField]
	private Sprite profile3SpriteArachnophobia;

	private int currentProfile = 1;

	private bool kitchenUnlocked;

	private bool officeUnlocked;

	private bool kidsRoomUnlocked;

	private bool livingRoomUnlocked;

	private bool hobbyRoomUnlocked;

	public int CurrentProfile => Singleton<ProfileController>.Instance.currentProfile;

	public bool KitchenUnlocked => Singleton<ProfileController>.Instance.kitchenUnlocked;

	public bool OfficeUnlocked => Singleton<ProfileController>.Instance.officeUnlocked;

	public bool KidsRoomUnlocked => Singleton<ProfileController>.Instance.kidsRoomUnlocked;

	public bool LivingRoomUnlocked => Singleton<ProfileController>.Instance.livingRoomUnlocked;

	public bool HobbyRoomUnlocked => Singleton<ProfileController>.Instance.hobbyRoomUnlocked;

	public event EventHandler<OnLevelUnlockedEventArgs> OnLevelUnlocked;

	protected override void Awake()
	{
		base.Awake();
		Singleton<ProfileController>.Instance.currentProfile = SaveController.Load("currentProfile", 1, SaveData.General);
		LoadUnlockedLevels();
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += SceneManager_OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= SceneManager_OnSceneLoaded;
	}

	private static void LoadUnlockedLevels()
	{
		Singleton<ProfileController>.Instance.kitchenUnlocked = SaveController.Load("kitchenUnlocked", defaultValue: false, SaveData.Game);
		Singleton<ProfileController>.Instance.officeUnlocked = SaveController.Load("officeUnlocked", defaultValue: false, SaveData.Game);
		Singleton<ProfileController>.Instance.kidsRoomUnlocked = SaveController.Load("kidsRoomUnlocked", defaultValue: false, SaveData.Game);
		Singleton<ProfileController>.Instance.livingRoomUnlocked = SaveController.Load("livingRoomUnlocked", defaultValue: false, SaveData.Game);
		Singleton<ProfileController>.Instance.hobbyRoomUnlocked = SaveController.Load("hobbyRoomUnlocked", defaultValue: false, SaveData.Game);
	}

	public Sprite GetProfileSprite(int profile)
	{
		return profile switch
		{
			1 => SettingsController.ArachnophobiaMode ? profile1SpriteArachnophobia : profile1Sprite, 
			2 => SettingsController.ArachnophobiaMode ? profile2SpriteArachnophobia : profile2Sprite, 
			3 => SettingsController.ArachnophobiaMode ? profile3SpriteArachnophobia : profile3Sprite, 
			_ => null, 
		};
	}

	public void LoadProfile(int profile)
	{
		Debug.Log($"Select Profile {profile}");
		Singleton<ProfileController>.Instance.currentProfile = profile;
		SaveController.Save("currentProfile", Singleton<ProfileController>.Instance.currentProfile, SaveData.General);
	}

	public void DeleteProfile(int profile)
	{
		SaveController.DeleteProfile(profile);
	}

	public void UnlockKitchen()
	{
		if (!Singleton<ProfileController>.Instance.kitchenUnlocked)
		{
			Singleton<ProfileController>.Instance.kitchenUnlocked = true;
			SaveController.Save("kitchenUnlocked", Singleton<ProfileController>.Instance.kitchenUnlocked, SaveData.Game);
		}
	}

	public void UnlockOffice()
	{
		if (!Singleton<ProfileController>.Instance.officeUnlocked)
		{
			Singleton<ProfileController>.Instance.officeUnlocked = true;
			SaveController.Save("officeUnlocked", Singleton<ProfileController>.Instance.officeUnlocked, SaveData.Game);
			Singleton<ProfileController>.Instance.OnLevelUnlocked?.Invoke(this, new OnLevelUnlockedEventArgs
			{
				levelData = officeLevelData
			});
		}
	}

	public void UnlockKidsRoom()
	{
		if (!Singleton<ProfileController>.Instance.kidsRoomUnlocked)
		{
			Singleton<ProfileController>.Instance.kidsRoomUnlocked = true;
			SaveController.Save("kidsRoomUnlocked", Singleton<ProfileController>.Instance.kidsRoomUnlocked, SaveData.Game);
			Singleton<ProfileController>.Instance.OnLevelUnlocked?.Invoke(this, new OnLevelUnlockedEventArgs
			{
				levelData = kidsRoomLevelData
			});
		}
	}

	public void UnlockLivingRoom()
	{
		if (!Singleton<ProfileController>.Instance.livingRoomUnlocked)
		{
			Singleton<ProfileController>.Instance.livingRoomUnlocked = true;
			SaveController.Save("livingRoomUnlocked", Singleton<ProfileController>.Instance.livingRoomUnlocked, SaveData.Game);
			Singleton<ProfileController>.Instance.OnLevelUnlocked?.Invoke(this, new OnLevelUnlockedEventArgs
			{
				levelData = livingRoomLevelData
			});
		}
	}

	public void UnlockHobbyRoom()
	{
		if (!Singleton<ProfileController>.Instance.hobbyRoomUnlocked)
		{
			Singleton<ProfileController>.Instance.hobbyRoomUnlocked = true;
			SaveController.Save("hobbyRoomUnlocked", Singleton<ProfileController>.Instance.hobbyRoomUnlocked, SaveData.Game);
			Singleton<ProfileController>.Instance.OnLevelUnlocked?.Invoke(this, new OnLevelUnlockedEventArgs
			{
				levelData = hobbyRoomLevelData
			});
		}
	}

	public LevelData GetLevelData(StoryLevel level)
	{
		return level switch
		{
			StoryLevel.Tutorial => tutorialLevelData, 
			StoryLevel.Kitchen => kitchenLevelData, 
			StoryLevel.Office => officeLevelData, 
			StoryLevel.KidsRoom => kidsRoomLevelData, 
			StoryLevel.HobbyRoom => hobbyRoomLevelData, 
			_ => throw new ArgumentOutOfRangeException("level", level, null), 
		};
	}

	private void SceneManager_OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		LoadUnlockedLevels();
	}
}
