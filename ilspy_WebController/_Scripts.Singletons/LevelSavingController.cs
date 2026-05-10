using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.General;
using _Scripts.KidsRoom;
using _Scripts.LevelSaving;
using _Scripts.Miscellaneous;
using _Scripts.Miscellaneous.Christmas;
using _Scripts.Miscellaneous.Halloween;
using _Scripts.Objects;
using _Scripts.Office;
using _Scripts.Puzzles;
using _Scripts.UI.Scene_Loading;
using _Scripts.UnityGamingServices;
using _Scripts.Web;

namespace _Scripts.Singletons;

public class LevelSavingController : Singleton<LevelSavingController>
{
	[SerializeField]
	private SpawnableObjectDictionarySO spawnableObjectDictionary;

	private static int index;

	private static LevelSaveData levelSaveData;

	private static Dictionary<string, GameObject> defaultGameObjectDict = new Dictionary<string, GameObject>();

	private static Dictionary<string, GameObject> uniqueGameObjectDict = new Dictionary<string, GameObject>();

	private static Dictionary<string, string> brokenObjectDict = new Dictionary<string, string>();

	private static Dictionary<string, string> destroyedObjectDict = new Dictionary<string, string>();

	private static Dictionary<string, string> spawnedObjectDict = new Dictionary<string, string>();

	private const string LEVEL_SAVE_DATA = "LevelSaveData";

	private const string INDEX = "LevelSavingIndex";

	private const string DESTROYED_OBJECT_SAVE_DATA = "DestroyedObjectSaveData";

	private const string SPAWNED_OBJECT_SAVE_DATA = "SpawnedObjectSaveData";

	private const string MOVABLE_OBJECT_SAVE_DATA = "MovableObjectSaveData";

	private const string WEB_JOINT_SAVE_DATA = "WebJointSaveData";

	private const string WEB_THREAD_SAVE_DATA = "WebThreadSaveData";

	private const string BURNABLE_OBJECT_SAVE_DATA = "BurnableObjectSaveData";

	private const string MELTABLE_OBJECT_SAVE_DATA = "MeltableObjectSaveData";

	private const string CLEANABLE_OBJECT_SAVE_DATA = "CleanableObjectSaveData";

	private const string BROKEN_OBJECT_SAVE_DATA = "BrokenObjectSaveData";

	private const string CHAIN_SAVE_DATA = "ChainSaveData";

	private const string MAGNETIC_LOCK_SAVE_DATA = "MagneticLockSaveData";

	private const string MAGNETIC_OBJECT_SAVE_DATA = "MagneticObjectSaveData";

	private const string BREAKABLE_JOINT_SAVE_DATA = "BreakableJointSaveData";

	private const string KITCHEN_SINK_SAVE_DATA = "KitchenSinkSaveData";

	private const string KITCHEN_WATER_SAVE_DATA = "KitchenWaterSaveData";

	private const string SPLAT_SAVE_DATA = "SplatSaveData";

	private const string DEFAULT_WEB_SAVE_DATA = "DefaultWebSaveData";

	private const string KITCHEN_PLANT_SAVE_DATA = "KitchenPlantSaveData";

	private const string BACKGROUND_CONTROLLER_SAVE_DATA = "BackgroundControllerSaveData";

	private const string KITCHEN_HALLOWEEN_CONTROLLER_SAVE_DATA = "KitchenHalloweenControllerSaveData";

	private const string KITCHEN_HALLOWEEN_BAT_SAVE_DATA = "KitchenHalloweenBatSaveData";

	private const string KITCHEN_HALLOWEEN_GHOST_SAVE_DATA = "KitchenHalloweenGhostSaveData";

	private const string KITCHEN_CHRISTMAS_CONTROLLER_SAVE_DATA = "KitchenChritmasControllerSaveData";

	private const string HOLOGRAM_SAVE_DATA = "HologramSaveData";

	private const string SNOWBALL_SAVE_DATA = "SnowballSaveData";

	private const string PRINTABLE_OBJECT_SAVE_DATA = "PrintableObjectSaveData";

	private const string OFFICE_SAFE_SAVE_DATA = "OfficeSafeSaveData";

	private const string OFFICE_COMPUTER_SAVE_DATA = "OfficeComputerSaveData";

	private const string OFFICE_PRINTER_SAVE_DATA = "OfficePrinterSaveData";

	private const string DESK_FAN_SAVE_DATA = "DeskFanSaveData";

	private const string SLIDING_PUZZLE_SAVE_DATA = "SlidingPuzzleSaveData";

	private const string STAR_SHOOTER_SAVE_DATA = "StarShooterSaveData";

	private const string INVESTIGATION_KIDSROOM_SAVE_DATA = "InvestigationKidsRoomSaveData";

	private const string ROCKET_SAVE_DATA = "RocketSaveData";

	private const string ROCKET_QUEST_SAVE_DATA = "RocketQuestSaveData";

	public Dictionary<string, GameObject> UniqueGameObjectDict => uniqueGameObjectDict;

	protected override void Awake()
	{
		base.Awake();
		index = SaveController.Load("LevelSavingIndex", 0, SaveData.General);
	}

	private void Start()
	{
		SceneController.OnSceneLoadingStarted -= SceneController_OnSceneLoadingStarted;
		SceneController.OnSceneLoaded -= SceneController_OnSceneLoaded;
		SceneController.OnSceneLoadingStarted += SceneController_OnSceneLoadingStarted;
		SceneController.OnSceneLoaded += SceneController_OnSceneLoaded;
	}

	private void OnEnable()
	{
		if (Singleton<RewardedAdsManager>.Instance != null)
		{
			Singleton<RewardedAdsManager>.Instance.OnAdShown += RewardedAdsManager_OnAdShown;
		}
	}

	private void OnDisable()
	{
		if (Singleton<RewardedAdsManager>.Instance != null)
		{
			Singleton<RewardedAdsManager>.Instance.OnAdShown -= RewardedAdsManager_OnAdShown;
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (levelSaveData != null && !Singleton<SceneController>.Instance.IsStoryLevel && !Singleton<SceneController>.Instance.CurrentScene.ToUpper().Contains("MAINMENU") && !hasFocus)
		{
			SaveLevelData();
		}
	}

	private static void LoadLevelData()
	{
		Debug.Log("LevelSavingController LoadLevelData");
		SetupDictionaries();
		InitializeKitchenHalloweenController();
		InitializeKitchenHalloweenBats();
		InitializeKitchenChristmasController();
		InitializeSlidingPuzzles();
		InitializeHolograms();
		InitializeBrokenObjects();
		InitializeDestroyedObjects();
		InitializeSpawnedObjects();
		InitializeChains();
		InitializeMovableObjects();
		InitializeBurnableObjects();
		InitializeMeltableObjects();
		InitializeCleanableObjects();
		InitializeMagneticObjects();
		InitializeMagneticLocks();
		InitializeBreakableJoints();
		InitializeSplats();
		InitializeKitchenPlants();
		InitializeKitchenSink();
		InitializeWaterKitchen();
		InitializeKitchenHalloweenGhosts();
		InitializeSnowballs();
		InitializePrintableObjects();
		InitializeOfficeSafe();
		InitializeOfficeComputer();
		InitializeOfficePrinter();
		InitializeDeskFan();
		InitializeStarShooters();
		InitializeInvestigationKidsRooms();
		InitializeRockets();
		InitializeRocketQuests();
		InitializeSavedWebs();
		InitializeDefaultWebs();
		InitializeBackgroundController();
	}

	private static void SaveLevelData()
	{
		Debug.Log("LevelSavingController SaveLevelData");
		SaveLevelSaveData();
		SaveSpawnedObjectSaveData();
		SaveMovableObjectSaveData();
		SaveDestroyedObjectSaveData();
		SaveBrokenObjectSaveData();
		SaveBurnableObjectSaveData();
		SaveMeltableObjectSaveData();
		SaveCleanableObjectSaveData();
		SaveChainSaveData();
		SaveMagneticObjectsSaveData();
		SaveMagneticLockSaveData();
		SaveBreakableJointSaveData();
		SaveHologramSaveData();
		SaveSplatSaveData();
		SaveKitchenPlantSaveData();
		SaveKitchenSinkSaveData();
		SaveWaterKitchenSaveData();
		SaveKitchenHalloweenControllerSaveData();
		SaveKitchenHalloweenBatSaveData();
		SaveKitchenHalloweenGhostSaveData();
		SaveKitchenChristmasControllerSaveData();
		SaveSnowballSaveData();
		SavePrintableObjectSaveData();
		SaveOfficeSafeSaveData();
		SaveOfficeComputerSaveData();
		SaveSlidingPuzzleSaveData();
		SaveOfficePrinterSaveData();
		SaveDeskFanSaveData();
		SaveStarShooterSaveData();
		SaveInvestigationKidsRoomSaveData();
		SaveRocketSaveData();
		SaveRocketQuestSaveData();
		SaveWebJointSaveData();
		SaveWebThreadSaveData();
		SaveDefaultWebSaveData();
		SaveBackgroundControllerSaveData();
	}

	private static void SetupDictionaries()
	{
		defaultGameObjectDict = new Dictionary<string, GameObject>();
		UniqueID[] array = UnityEngine.Object.FindObjectsByType<UniqueID>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (UniqueID uniqueID in array)
		{
			defaultGameObjectDict.TryAdd(uniqueID.ID, uniqueID.gameObject);
			SetupEventListeners(uniqueID);
		}
		uniqueGameObjectDict = new Dictionary<string, GameObject>(defaultGameObjectDict);
		brokenObjectDict = new Dictionary<string, string>();
		destroyedObjectDict = new Dictionary<string, string>();
		spawnedObjectDict = new Dictionary<string, string>();
	}

	private static void SetupEventListeners(UniqueID uniqueID)
	{
		MovableObject component = uniqueID.gameObject.GetComponent<MovableObject>();
		if (component != null)
		{
			component.OnDestroy += MovableObject_OnDestroy;
		}
		BreakableObject component2 = uniqueID.gameObject.GetComponent<BreakableObject>();
		if (component2 != null)
		{
			component2.OnBreak += BreakableObject_OnBreak;
		}
	}

	private static void InitializeAll<TComponent, TData>(string dataKey) where TComponent : Component, IInitializable<TData> where TData : struct, IHasId
	{
		Debug.Log("InitializeAll<" + typeof(TComponent).Name + ", " + typeof(TData).Name + ">");
		List<TData> list = SaveController.LoadLevelData(dataKey, new List<TData>(), levelSaveData);
		if (list == null)
		{
			Debug.LogWarning("Wrong save data type for " + dataKey);
			return;
		}
		foreach (TData item in list)
		{
			string id = item.Id;
			if (string.IsNullOrEmpty(id))
			{
				continue;
			}
			uniqueGameObjectDict.TryGetValue(id, out var value);
			if (!(value == null))
			{
				TComponent component = value.GetComponent<TComponent>();
				if (!(component == null))
				{
					component.Initialize(item);
				}
			}
		}
	}

	private static void SaveAll<TComponent, TData>(string dataKey, FindObjectsInactive includeInactive) where TComponent : Component, IHasSaveData<TData> where TData : struct
	{
		Debug.Log("SaveAll<" + typeof(TComponent).Name + ", " + typeof(TData).Name + ">");
		List<TData> list = new List<TData>();
		TComponent[] array = UnityEngine.Object.FindObjectsByType<TComponent>(includeInactive, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			TData saveData = array[i].GetSaveData();
			list.Add(saveData);
		}
		SaveController.SaveLevelData(dataKey, list, levelSaveData);
	}

	public static void GenerateNewLevelSaveData(LevelData levelData)
	{
		Debug.Log("GenerateNewLevelSaveData");
		int num = 1;
		string text = $"{DialogueManager.GetLocalizedText(levelData.levelName)}_{index}";
		LevelSaveData levelSaveData = SaveController.GetLevelSaveData(levelData.sceneName, text);
		if (levelSaveData != null)
		{
			while (text == levelSaveData.fileName && num < 100)
			{
				text = $"{DialogueManager.GetLocalizedText(levelData.levelName)}_{index} ({num})";
				levelSaveData = SaveController.GetLevelSaveData(levelData.sceneName, text);
				if (levelSaveData == null)
				{
					break;
				}
				num++;
			}
		}
		LevelSavingController.levelSaveData = new LevelSaveData
		{
			sceneName = levelData.sceneName,
			fileName = text,
			date = DateTime.Now
		};
		index++;
		SaveController.Save("LevelSavingIndex", index, SaveData.General);
	}

	public static void LoadLevelSaveData(LevelSaveData newLevelSaveData)
	{
		Debug.Log("LoadLevelSaveData");
		levelSaveData = newLevelSaveData;
	}

	private static void SaveLevelSaveData()
	{
		if (levelSaveData != null)
		{
			Debug.Log("SaveLevelSaveData");
			levelSaveData.date = DateTime.Now;
			SaveController.SaveLevelData("LevelSaveData", levelSaveData, levelSaveData);
		}
	}

	private static void InitializeSavedWebs()
	{
		Debug.Log("InitializeSavedWebs");
		List<WebJointSaveData> webJointSaveDataList = SaveController.LoadLevelData("WebJointSaveData", new List<WebJointSaveData>(), levelSaveData);
		List<WebThreadSaveData> webThreadSaveDataList = SaveController.LoadLevelData("WebThreadSaveData", new List<WebThreadSaveData>(), levelSaveData);
		Singleton<WebController>.Instance.InitializeSavedWebs(webJointSaveDataList, webThreadSaveDataList);
	}

	private static void SaveWebJointSaveData()
	{
		Debug.Log("SaveWebJointSaveData");
		List<WebJointSaveData> webJointSaveDataList = Singleton<WebController>.Instance.GetWebJointSaveDataList();
		SaveController.SaveLevelData("WebJointSaveData", webJointSaveDataList, levelSaveData);
	}

	private static void SaveWebThreadSaveData()
	{
		Debug.Log("SaveWebThreadSaveData");
		List<WebThreadSaveData> webThreadSaveDataList = Singleton<WebController>.Instance.GetWebThreadSaveDataList();
		SaveController.SaveLevelData("WebThreadSaveData", webThreadSaveDataList, levelSaveData);
	}

	private static void InitializeDestroyedObjects()
	{
		Debug.Log("InitializeDestroyedObjects");
		foreach (DestroyedObjectSaveData item in SaveController.LoadLevelData("DestroyedObjectSaveData", new List<DestroyedObjectSaveData>(), levelSaveData))
		{
			destroyedObjectDict.TryAdd(item.id, item.name);
			Debug.Log("destroyedObjectDict.TryAdd(" + item.id + ", " + item.name + ")");
			uniqueGameObjectDict.TryGetValue(item.id, out var value);
			if (value != null)
			{
				UnityEngine.Object.Destroy(value);
				Debug.Log("Destroy " + item.id + ", " + item.name);
			}
		}
	}

	private static void SaveDestroyedObjectSaveData()
	{
		Debug.Log("SaveDestroyedObjectSaveData");
		List<DestroyedObjectSaveData> destroyedGameObjectSaveDataList = GetDestroyedGameObjectSaveDataList();
		SaveController.SaveLevelData("DestroyedObjectSaveData", destroyedGameObjectSaveDataList, levelSaveData);
	}

	private static List<DestroyedObjectSaveData> GetDestroyedGameObjectSaveDataList()
	{
		List<DestroyedObjectSaveData> list = new List<DestroyedObjectSaveData>();
		foreach (KeyValuePair<string, string> item2 in destroyedObjectDict)
		{
			if (defaultGameObjectDict.ContainsKey(item2.Key) && !spawnedObjectDict.ContainsKey(item2.Key) && !brokenObjectDict.ContainsKey(item2.Key))
			{
				DestroyedObjectSaveData destroyedObjectSaveData = default(DestroyedObjectSaveData);
				destroyedObjectSaveData.id = item2.Key;
				destroyedObjectSaveData.name = item2.Value;
				DestroyedObjectSaveData item = destroyedObjectSaveData;
				list.Add(item);
			}
		}
		return list;
	}

	private static void InitializeSpawnedObjects()
	{
		Debug.Log("InitializeSpawnedObjects");
		foreach (SpawnedObjectSaveData item in SaveController.LoadLevelData("SpawnedObjectSaveData", new List<SpawnedObjectSaveData>(), levelSaveData))
		{
			if (!destroyedObjectDict.ContainsKey(item.id) && !brokenObjectDict.ContainsKey(item.id) && !defaultGameObjectDict.ContainsKey(item.id))
			{
				SpawnableObject spawnableObject = UnityEngine.Object.Instantiate(Singleton<LevelSavingController>.Instance.spawnableObjectDictionary.dictionary[item.spawnableObjectType].spawnableObject);
				spawnableObject.gameObject.name = item.name;
				UniqueID component = spawnableObject.GetComponent<UniqueID>();
				component.ForceID(item.id);
				uniqueGameObjectDict.TryAdd(component.ID, spawnableObject.gameObject);
				spawnedObjectDict.TryAdd(component.ID, spawnableObject.gameObject.name);
				SetupEventListeners(component);
			}
		}
	}

	private static void SaveSpawnedObjectSaveData()
	{
		Debug.Log("SaveSpawnedObjectSaveData");
		List<SpawnedObjectSaveData> spawnedObjectSaveDataList = GetSpawnedObjectSaveDataList();
		SaveController.SaveLevelData("SpawnedObjectSaveData", spawnedObjectSaveDataList, levelSaveData);
	}

	private static List<SpawnedObjectSaveData> GetSpawnedObjectSaveDataList()
	{
		List<SpawnedObjectSaveData> list = new List<SpawnedObjectSaveData>();
		SpawnableObject[] array = UnityEngine.Object.FindObjectsByType<SpawnableObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (SpawnableObject spawnableObject in array)
		{
			SpawnedObjectSaveData item = default(SpawnedObjectSaveData);
			UniqueID component = spawnableObject.GetComponent<UniqueID>();
			if (component != null)
			{
				item.id = component.ID;
			}
			if (!defaultGameObjectDict.ContainsKey(item.id))
			{
				item.name = spawnableObject.gameObject.name;
				item.spawnableObjectType = spawnableObject.GetSpawnableObjectType();
				list.Add(item);
			}
		}
		return list;
	}

	private static void InitializeBrokenObjects()
	{
		Debug.Log("InitializeBrokenObjects");
		foreach (BrokenObjectSaveData item in SaveController.LoadLevelData("BrokenObjectSaveData", new List<BrokenObjectSaveData>(), levelSaveData))
		{
			string id = item.id;
			if (id == null)
			{
				continue;
			}
			uniqueGameObjectDict.TryGetValue(id, out var value);
			if (value == null)
			{
				continue;
			}
			BreakableObject component = value.GetComponent<BreakableObject>();
			if (!(component == null))
			{
				component.FreeBrokenPieces();
				brokenObjectDict.TryAdd(item.id, item.name);
				Debug.Log("brokenObjectDict.TryAdd(" + item.id + " " + item.name + ")");
				uniqueGameObjectDict.TryGetValue(item.id, out var value2);
				if (!(value2 == null))
				{
					UnityEngine.Object.Destroy(value2);
					Debug.Log("Destroy " + item.id + ", " + item.name);
				}
			}
		}
	}

	private static void SaveBrokenObjectSaveData()
	{
		Debug.Log("SaveBrokenObjectSaveData");
		List<BrokenObjectSaveData> brokenObjectSaveDataList = GetBrokenObjectSaveDataList();
		SaveController.SaveLevelData("BrokenObjectSaveData", brokenObjectSaveDataList, levelSaveData);
	}

	private static List<BrokenObjectSaveData> GetBrokenObjectSaveDataList()
	{
		List<BrokenObjectSaveData> list = new List<BrokenObjectSaveData>();
		foreach (KeyValuePair<string, string> item2 in brokenObjectDict)
		{
			if (!spawnedObjectDict.ContainsKey(item2.Key) && uniqueGameObjectDict.ContainsKey(item2.Key))
			{
				BrokenObjectSaveData brokenObjectSaveData = default(BrokenObjectSaveData);
				brokenObjectSaveData.id = item2.Key;
				brokenObjectSaveData.name = item2.Value;
				BrokenObjectSaveData item = brokenObjectSaveData;
				list.Add(item);
			}
		}
		return list;
	}

	private static void InitializeMovableObjects()
	{
		InitializeAll<MovableObject, MovableObjectSaveData>("MovableObjectSaveData");
	}

	private static void SaveMovableObjectSaveData()
	{
		SaveAll<MovableObject, MovableObjectSaveData>("MovableObjectSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeBurnableObjects()
	{
		InitializeAll<BurnableObject, BurnableObjectSaveData>("BurnableObjectSaveData");
	}

	private static void SaveBurnableObjectSaveData()
	{
		SaveAll<BurnableObject, BurnableObjectSaveData>("BurnableObjectSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeMeltableObjects()
	{
		InitializeAll<MeltableObject, MeltableObjectSaveData>("MeltableObjectSaveData");
	}

	private static void SaveMeltableObjectSaveData()
	{
		SaveAll<MeltableObject, MeltableObjectSaveData>("MeltableObjectSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeCleanableObjects()
	{
		InitializeAll<CleanableObject, CleanableObjectSaveData>("CleanableObjectSaveData");
	}

	private static void SaveCleanableObjectSaveData()
	{
		SaveAll<CleanableObject, CleanableObjectSaveData>("CleanableObjectSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeChains()
	{
		InitializeAll<Chain, ChainSaveData>("ChainSaveData");
	}

	private static void SaveChainSaveData()
	{
		SaveAll<Chain, ChainSaveData>("ChainSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeMagneticLocks()
	{
		InitializeAll<MagneticLock, MagneticLockSaveData>("MagneticLockSaveData");
	}

	private static void SaveMagneticLockSaveData()
	{
		SaveAll<MagneticLock, MagneticLockSaveData>("MagneticLockSaveData", FindObjectsInactive.Include);
	}

	private static void InitializeMagneticObjects()
	{
		InitializeAll<MagneticObject, MagneticObjectSaveData>("MagneticObjectSaveData");
	}

	private static void SaveMagneticObjectsSaveData()
	{
		SaveAll<MagneticObject, MagneticObjectSaveData>("MagneticObjectSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeBreakableJoints()
	{
		InitializeAll<BreakableJoint, BreakableJointSaveData>("BreakableJointSaveData");
	}

	private static void SaveBreakableJointSaveData()
	{
		SaveAll<BreakableJoint, BreakableJointSaveData>("BreakableJointSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeKitchenSink()
	{
		InitializeAll<KitchenSink, KitchenSinkSaveData>("KitchenSinkSaveData");
	}

	private static void SaveKitchenSinkSaveData()
	{
		SaveAll<KitchenSink, KitchenSinkSaveData>("KitchenSinkSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeWaterKitchen()
	{
		InitializeAll<WaterKitchen, WaterKitchenSaveData>("KitchenWaterSaveData");
	}

	private static void SaveWaterKitchenSaveData()
	{
		SaveAll<WaterKitchen, WaterKitchenSaveData>("KitchenWaterSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeSplats()
	{
		InitializeAll<Splat, SplatSaveData>("SplatSaveData");
	}

	private static void SaveSplatSaveData()
	{
		SaveAll<Splat, SplatSaveData>("SplatSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeKitchenPlants()
	{
		InitializeAll<KitchenPlant, KitchenPlantSaveData>("KitchenPlantSaveData");
	}

	private static void SaveKitchenPlantSaveData()
	{
		SaveAll<KitchenPlant, KitchenPlantSaveData>("KitchenPlantSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeDefaultWebs()
	{
		InitializeAll<DefaultWeb, DefaultWebSaveData>("DefaultWebSaveData");
	}

	private static void SaveDefaultWebSaveData()
	{
		SaveAll<DefaultWeb, DefaultWebSaveData>("DefaultWebSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeBackgroundController()
	{
		InitializeAll<BackgroundController, BackgroundControllerSaveData>("BackgroundControllerSaveData");
	}

	private static void SaveBackgroundControllerSaveData()
	{
		SaveAll<BackgroundController, BackgroundControllerSaveData>("BackgroundControllerSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeKitchenHalloweenController()
	{
		InitializeAll<KitchenHalloweenController, KitchenHalloweenControllerSaveData>("KitchenHalloweenControllerSaveData");
	}

	private static void SaveKitchenHalloweenControllerSaveData()
	{
		SaveAll<KitchenHalloweenController, KitchenHalloweenControllerSaveData>("KitchenHalloweenControllerSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeKitchenHalloweenBats()
	{
		InitializeAll<KitchenHalloweenBat, KitchenHalloweenBatSaveData>("KitchenHalloweenBatSaveData");
	}

	private static void SaveKitchenHalloweenBatSaveData()
	{
		SaveAll<KitchenHalloweenBat, KitchenHalloweenBatSaveData>("KitchenHalloweenBatSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeKitchenHalloweenGhosts()
	{
		InitializeAll<Ghost, KitchenHalloweenGhostSaveData>("KitchenHalloweenGhostSaveData");
	}

	private static void SaveKitchenHalloweenGhostSaveData()
	{
		SaveAll<Ghost, KitchenHalloweenGhostSaveData>("KitchenHalloweenGhostSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeKitchenChristmasController()
	{
		InitializeAll<KitchenChristmasController, KitchenChristmasControllerSaveData>("KitchenChritmasControllerSaveData");
	}

	private static void SaveKitchenChristmasControllerSaveData()
	{
		SaveAll<KitchenChristmasController, KitchenChristmasControllerSaveData>("KitchenChritmasControllerSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeHolograms()
	{
		InitializeAll<Hologram, HologramSaveData>("HologramSaveData");
	}

	private static void SaveHologramSaveData()
	{
		SaveAll<Hologram, HologramSaveData>("HologramSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeSnowballs()
	{
		InitializeAll<Snowball, SnowballSaveData>("SnowballSaveData");
	}

	private static void SaveSnowballSaveData()
	{
		SaveAll<Snowball, SnowballSaveData>("SnowballSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializePrintableObjects()
	{
		InitializeAll<PrintableObject, PrintableObjectSaveData>("PrintableObjectSaveData");
	}

	private static void SavePrintableObjectSaveData()
	{
		SaveAll<PrintableObject, PrintableObjectSaveData>("PrintableObjectSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeOfficeSafe()
	{
		InitializeAll<OfficeSafe, OfficeSafeSaveData>("OfficeSafeSaveData");
	}

	private static void SaveOfficeSafeSaveData()
	{
		SaveAll<OfficeSafe, OfficeSafeSaveData>("OfficeSafeSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeOfficeComputer()
	{
		InitializeAll<OfficeComputer, OfficeComputerSaveData>("OfficeComputerSaveData");
	}

	private static void SaveOfficeComputerSaveData()
	{
		SaveAll<OfficeComputer, OfficeComputerSaveData>("OfficeComputerSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeOfficePrinter()
	{
		InitializeAll<OfficePrinter, OfficePrinterSaveData>("OfficePrinterSaveData");
	}

	private static void SaveOfficePrinterSaveData()
	{
		SaveAll<OfficePrinter, OfficePrinterSaveData>("OfficePrinterSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeDeskFan()
	{
		InitializeAll<DeskFan, DeskFanSaveData>("DeskFanSaveData");
	}

	private static void SaveDeskFanSaveData()
	{
		SaveAll<DeskFan, DeskFanSaveData>("DeskFanSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeSlidingPuzzles()
	{
		InitializeAll<SlidingPuzzle, SlidingPuzzleSaveData>("SlidingPuzzleSaveData");
	}

	private static void SaveSlidingPuzzleSaveData()
	{
		SaveAll<SlidingPuzzle, SlidingPuzzleSaveData>("SlidingPuzzleSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeStarShooters()
	{
		InitializeAll<StarShooter, StarShooterSaveData>("StarShooterSaveData");
	}

	private static void SaveStarShooterSaveData()
	{
		SaveAll<StarShooter, StarShooterSaveData>("StarShooterSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeInvestigationKidsRooms()
	{
		InitializeAll<InvestigationKidsRoom, InvestigationKidsRoomSaveData>("InvestigationKidsRoomSaveData");
	}

	private static void SaveInvestigationKidsRoomSaveData()
	{
		SaveAll<InvestigationKidsRoom, InvestigationKidsRoomSaveData>("InvestigationKidsRoomSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeRockets()
	{
		InitializeAll<_Scripts.KidsRoom.Rocket, RocketSaveData>("RocketSaveData");
	}

	private static void SaveRocketSaveData()
	{
		SaveAll<_Scripts.KidsRoom.Rocket, RocketSaveData>("RocketSaveData", FindObjectsInactive.Exclude);
	}

	private static void InitializeRocketQuests()
	{
		InitializeAll<RocketQuest, RocketQuestSaveData>("RocketQuestSaveData");
	}

	private static void SaveRocketQuestSaveData()
	{
		SaveAll<RocketQuest, RocketQuestSaveData>("RocketQuestSaveData", FindObjectsInactive.Exclude);
	}

	public static void TryAddUniqueGameObjectById(string id, GameObject uniqueGameObject)
	{
		uniqueGameObjectDict.TryAdd(id, uniqueGameObject);
	}

	public static bool TryGetUniqueGameObjectById(string id, out GameObject uniqueGameObject)
	{
		GameObject value;
		bool result = uniqueGameObjectDict.TryGetValue(id, out value);
		uniqueGameObject = value;
		return result;
	}

	private static void RewardedAdsManager_OnAdShown()
	{
		if (levelSaveData != null && !Singleton<SceneController>.Instance.IsStoryLevel && Singleton<SceneController>.Instance.LastScene.ToUpper().Contains("LEVEL"))
		{
			SaveLevelData();
		}
	}

	private static void SceneController_OnSceneLoadingStarted(object sender, EventArgs e)
	{
		if (levelSaveData != null && !SceneController.LastLevelWasStoryLevel && Singleton<SceneController>.Instance.LastScene.ToUpper().Contains("LEVEL"))
		{
			SaveLevelData();
		}
	}

	private static void SceneController_OnSceneLoaded(object sender, EventArgs e)
	{
		if (levelSaveData != null && !Singleton<SceneController>.Instance.IsStoryLevel && Singleton<SceneController>.Instance.CurrentScene.ToUpper().Contains("LEVEL"))
		{
			LoadLevelData();
		}
	}

	private static void MovableObject_OnDestroy(object sender, MovableObject.OnDestroyEventArgs e)
	{
		if (!destroyedObjectDict.ContainsKey(e.id))
		{
			destroyedObjectDict.TryAdd(e.id, e.name);
		}
	}

	private static void BreakableObject_OnBreak(object sender, BreakableObject.OnBreakEventArgs e)
	{
		if (!brokenObjectDict.ContainsKey(e.id))
		{
			brokenObjectDict.TryAdd(e.id, e.name);
		}
	}
}
