using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.CosmeticItems;
using _Scripts.General;
using _Scripts.Utils;
using _Scripts.Wardrobe;

namespace _Scripts.Singletons;

public class CosmeticItemsController : Singleton<CosmeticItemsController>
{
	public class OnCosmeticItemUnlockedEventArgs : EventArgs
	{
		[FormerlySerializedAs("unlockableSo")]
		public CosmeticItemSo cosmeticItemSo;
	}

	[SerializeField]
	private CosmeticItemsSo cosmeticItemsSo;

	[SerializeField]
	private ColorPaletteSo colorPaletteSo;

	[Header("Default Items")]
	[SerializeField]
	private CosmeticItemEyeSo defaultEyeSo;

	[SerializeField]
	private CosmeticItemHatSo defaultHatSo;

	[SerializeField]
	private CosmeticItemAccessorySo defaultAccessorySo;

	[SerializeField]
	private CosmeticItemShoeSo defaultShoeSo;

	[SerializeField]
	private CosmeticItemWebSo defaultWebSo;

	[Header("Level Items")]
	[SerializeField]
	private CosmeticItemsListSo kitchenCosmeticItemsListSo;

	[SerializeField]
	private CosmeticItemsListSo officeCosmeticItemsListSo;

	[SerializeField]
	private CosmeticItemsListSo kidsRoomCosmeticItemsListSo;

	[SerializeField]
	private CosmeticItemsListSo livingRoomCosmeticItemsListSo;

	[Header("Exotic Items")]
	[SerializeField]
	private CosmeticItemsListSo kitchenExoticItemsListSo;

	[SerializeField]
	private CosmeticItemsListSo officeExoticItemsListSo;

	[SerializeField]
	private CosmeticItemsListSo kidsRoomExoticItemsListSo;

	[SerializeField]
	private CosmeticItemsListSo livingRoomExoticItemsListSo;

	[Header("Shop")]
	[SerializeField]
	private CosmeticItemSo[] excludedItemsFromShop;

	private Dictionary<int, bool> unlockedItems;

	private Dictionary<int, bool> visibleItems;

	private const float defaultBodyFluffiness = 0f;

	private const float defaultLegFluffiness = 0.25f;

	private const float defaultJointFluffiness = 0.5f;

	private const float defaultAbdomenFluffiness = 0f;

	private const int defaultJointColorIndex = 5;

	private const int defaultLegColorIndex = 7;

	private const int defaultBodyColorIndex = 7;

	private const int defaultAbdomenColorIndex = 7;

	private const int defaultEyeColorBaseIndex = 5;

	private const int defaultEyeColorLeftIndex = 0;

	private const int defaultEyeColorRightIndex = 0;

	private readonly bool[] defaultLegsEnabled = new bool[8] { true, true, true, true, true, true, true, true };

	private int outfitCounter;

	private Outfit defaultOutfit;

	public ColorPaletteSo ColorPalette => Singleton<CosmeticItemsController>.Instance.colorPaletteSo;

	public float DefaultBodyFluffiness => 0f;

	public float DefaultLegFluffiness => 0.25f;

	public float DefaultJointFluffiness => 0.5f;

	public float DefaultAbdomenFluffiness => 0f;

	public Color DefaultBodyColor => ColorPalette.colors[7];

	public Color DefaultAbdomenColor => ColorPalette.colors[7];

	public Color DefaultLegColor => ColorPalette.colors[7];

	public Color DefaultJointColor => ColorPalette.colors[5];

	public Color DefaultEyeColorBase => ColorPalette.colors[5];

	public Color DefaultEyeColorLeft => ColorPalette.colors[0];

	public Color DefaultEyeColorRight => ColorPalette.colors[0];

	public Outfit DefaultOutfit => defaultOutfit;

	public event EventHandler OnNewItemsAvailableInShop;

	public event EventHandler<OnCosmeticItemUnlockedEventArgs> OnCosmeticItemUnlocked;

	protected override void Awake()
	{
		base.Awake();
		LoadUnlockedItems();
		LoadVisibleItems();
		outfitCounter = SaveController.Load("OutfitCounter", 0, SaveData.Wardrobe);
		SetupDefaultOutfit();
	}

	private void OnEnable()
	{
		SceneController.OnSceneLoaded += SceneController_OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneController.OnSceneLoaded -= SceneController_OnSceneLoaded;
	}

	private static void LoadVisibleItems()
	{
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		dictionary = Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary.ToDictionary((KeyValuePair<int, CosmeticItemSo> keyValuePair) => keyValuePair.Key, (KeyValuePair<int, CosmeticItemSo> keyValuePair) => keyValuePair.Value.visibleAtStart);
		Dictionary<int, bool> dictionary2 = SaveController.Load("VisibleItems", dictionary, SaveData.Wardrobe);
		foreach (KeyValuePair<int, bool> item in dictionary2.ToList())
		{
			int key = item.Key;
			if (dictionary.ContainsKey(key) && dictionary2.ContainsKey(key))
			{
				dictionary[key] |= dictionary2[key];
			}
		}
		Singleton<CosmeticItemsController>.Instance.visibleItems = dictionary;
		SaveController.Save("VisibleItems", Singleton<CosmeticItemsController>.Instance.visibleItems, SaveData.Wardrobe);
	}

	private static void LoadUnlockedItems()
	{
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		dictionary = Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary.ToDictionary((KeyValuePair<int, CosmeticItemSo> keyValuePair) => keyValuePair.Key, (KeyValuePair<int, CosmeticItemSo> keyValuePair) => keyValuePair.Value.unlockedAtStart);
		Dictionary<int, bool> dictionary2 = SaveController.Load("UnlockedItems", dictionary, SaveData.Wardrobe);
		foreach (KeyValuePair<int, bool> item in dictionary2.ToList())
		{
			int key = item.Key;
			if (dictionary.ContainsKey(key) && dictionary2.ContainsKey(key))
			{
				dictionary[key] |= dictionary2[key];
			}
		}
		Singleton<CosmeticItemsController>.Instance.unlockedItems = dictionary;
		SaveController.Save("UnlockedItems", Singleton<CosmeticItemsController>.Instance.unlockedItems, SaveData.Wardrobe);
	}

	private void UnlockAllEyes()
	{
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			if (item.Value is CosmeticItemEyeSo)
			{
				UnlockItem(item.Value);
			}
		}
	}

	private void UnlockAllHats()
	{
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			if (item.Value is CosmeticItemHatSo)
			{
				UnlockItem(item.Value);
			}
		}
	}

	private void UnlockAllHatsInDemo()
	{
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			if (item.Value is CosmeticItemHatSo && item.Value.visibleInDemo)
			{
				UnlockItem(item.Value);
			}
		}
	}

	private void UnlockAllWebs()
	{
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			if (item.Value is CosmeticItemWebSo)
			{
				Debug.Log($"Unlock {item.Value}");
				UnlockItem(item.Value);
			}
		}
	}

	private void UnlockAllAccessories()
	{
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			if (item.Value is CosmeticItemAccessorySo)
			{
				Debug.Log($"Unlock {item.Value}");
				UnlockItem(item.Value);
			}
		}
	}

	private void UnlockAllShoes()
	{
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			if (item.Value is CosmeticItemShoeSo)
			{
				Debug.Log($"Unlock {item.Value}");
				UnlockItem(item.Value);
			}
		}
	}

	private void UnlockAllCosmeticItems()
	{
		UnlockAllEyes();
		UnlockAllHats();
		UnlockAllAccessories();
		UnlockAllShoes();
		UnlockAllWebs();
	}

	private void UnlockAllAvailableCosmeticItems()
	{
		UnlockCosmeticItems(kitchenCosmeticItemsListSo.list);
		UnlockCosmeticItems(kitchenExoticItemsListSo.list);
		UnlockCosmeticItems(officeCosmeticItemsListSo.list);
		UnlockCosmeticItems(officeExoticItemsListSo.list);
		UnlockCosmeticItems(kidsRoomCosmeticItemsListSo.list);
		UnlockCosmeticItems(kidsRoomExoticItemsListSo.list);
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			if (item.Value.rarity == CosmeticItemRarity.Event)
			{
				UnlockItem(item.Value);
			}
		}
	}

	private void UnlockCosmeticItems(List<CosmeticItemSo> cosmeticItemsList)
	{
		foreach (CosmeticItemSo cosmeticItems in cosmeticItemsList)
		{
			UnlockItem(cosmeticItems);
		}
	}

	private void ShowAllCosmeticItems()
	{
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			MakeItemVisibleInShop(item.Value);
		}
	}

	private void ShowAllUnlockableCosmeticItems()
	{
		MakeCosmeticItemsVisibleForKitchen();
		MakeExoticItemsVisibleForKitchen();
		MakeCosmeticItemsVisibleForOffice();
		MakeExoticItemsVisibleForOffice();
		MakeCosmeticItemsVisibleForKidsRoom();
		MakeExoticItemsVisibleForKidsRoom();
		foreach (KeyValuePair<int, CosmeticItemSo> item in cosmeticItemsSo.dictionary)
		{
			if (item.Value.rarity == CosmeticItemRarity.Event)
			{
				MakeItemVisibleInShop(item.Value);
			}
		}
	}

	private bool MakeItemVisibleInShop(CosmeticItemSo cosmeticItemSo)
	{
		if (IsItemVisible(cosmeticItemSo))
		{
			return false;
		}
		int itemIndex = GetItemIndex(cosmeticItemSo);
		Singleton<CosmeticItemsController>.Instance.visibleItems[itemIndex] = true;
		SaveController.Save("VisibleItems", Singleton<CosmeticItemsController>.Instance.visibleItems, SaveData.Wardrobe);
		return true;
	}

	private void MakeItemsVisibleInShop(CosmeticItemsListSo cosmeticItemsListSo)
	{
		if (cosmeticItemsListSo == null || cosmeticItemsListSo.list.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (CosmeticItemSo item in cosmeticItemsListSo.list)
		{
			if (MakeItemVisibleInShop(item))
			{
				num++;
			}
		}
		if (num > 0)
		{
			Singleton<CosmeticItemsController>.Instance.OnNewItemsAvailableInShop?.Invoke(this, EventArgs.Empty);
		}
	}

	private void MakeCosmeticItemsAvailableInShop(StoryLevel level)
	{
		switch (level)
		{
		case StoryLevel.Kitchen:
			MakeItemsVisibleInShop(kitchenCosmeticItemsListSo);
			break;
		case StoryLevel.Office:
			MakeItemsVisibleInShop(officeCosmeticItemsListSo);
			break;
		case StoryLevel.KidsRoom:
			MakeItemsVisibleInShop(kidsRoomCosmeticItemsListSo);
			break;
		case StoryLevel.LivingRoom:
			MakeItemsVisibleInShop(livingRoomCosmeticItemsListSo);
			break;
		default:
			throw new ArgumentOutOfRangeException("level", level, null);
		case StoryLevel.Tutorial:
			break;
		}
	}

	private void MakeExoticItemsAvailableInShop(StoryLevel level)
	{
		switch (level)
		{
		case StoryLevel.Kitchen:
			MakeItemsVisibleInShop(kitchenExoticItemsListSo);
			break;
		case StoryLevel.Office:
			MakeItemsVisibleInShop(officeExoticItemsListSo);
			break;
		case StoryLevel.KidsRoom:
			MakeItemsVisibleInShop(kidsRoomExoticItemsListSo);
			break;
		case StoryLevel.LivingRoom:
			MakeItemsVisibleInShop(livingRoomExoticItemsListSo);
			break;
		default:
			throw new ArgumentOutOfRangeException("level", level, null);
		case StoryLevel.Tutorial:
			break;
		}
	}

	private void CheckUnlockedItems()
	{
		int completedQuestsKitchen = Singleton<QuestController>.Instance.GetCompletedQuestsKitchen();
		Debug.Log($"completedQuestsKitchen: {completedQuestsKitchen}");
		if (completedQuestsKitchen >= 3)
		{
			MakeCosmeticItemsVisibleForKitchen();
		}
		if (completedQuestsKitchen >= 5)
		{
			MakeExoticItemsVisibleForKitchen();
		}
		int completedQuestsOffice = Singleton<QuestController>.Instance.GetCompletedQuestsOffice();
		if (completedQuestsOffice >= 3)
		{
			MakeCosmeticItemsVisibleForOffice();
		}
		if (completedQuestsOffice >= 5)
		{
			MakeExoticItemsVisibleForOffice();
		}
		int completedQuestsKidsRoom = Singleton<QuestController>.Instance.GetCompletedQuestsKidsRoom();
		if (completedQuestsKidsRoom >= 3)
		{
			MakeCosmeticItemsVisibleForKidsRoom();
		}
		if (completedQuestsKidsRoom >= 5)
		{
			MakeExoticItemsVisibleForKidsRoom();
		}
		Singleton<QuestController>.Instance.GetCompletedQuestsLivingRoom();
		if (completedQuestsKidsRoom >= 3)
		{
			MakeCosmeticItemsVisibleForLivingRoom();
		}
		if (completedQuestsKidsRoom >= 5)
		{
			MakeExoticItemsVisibleForLivingRoom();
		}
	}

	private void SetupDefaultOutfit()
	{
		defaultOutfit = new Outfit
		{
			name = "Default",
			arachnophobiaMode = false,
			bodyEnabled = true,
			bodyColor = DefaultBodyColor,
			bodyFluffiness = 0f,
			abdomenEnabled = false,
			abdomenColor = DefaultAbdomenColor,
			abdomenFluffiness = 0f,
			legsEnabled = new bool[8] { true, true, true, true, true, true, true, true },
			eyeIndex = GetDefaultEyeIndex(),
			eyeColorBase = DefaultEyeColorBase,
			eyeColorLeft = DefaultEyeColorLeft,
			eyeColorRight = DefaultEyeColorRight,
			hatIndex = GetDefaultHatIndex(),
			hatColors = new Color[0],
			hatEffects = new float[0],
			accessoryIndex = GetDefaultAccessoryIndex(),
			accessoryColors = new Color[0],
			accessoryEffects = new float[0],
			shoeIndex = GetDefaultShoeIndex(),
			shoeColors = new Color[0],
			shoeEffects = new float[0]
		};
		defaultOutfit.legColors = new Color[3];
		defaultOutfit.jointColors = new Color[3];
		defaultOutfit.legSegmentFluffiness = new float[3];
		defaultOutfit.jointSegmentFluffiness = new float[3];
		for (int i = 0; i < 3; i++)
		{
			defaultOutfit.legColors[i] = DefaultLegColor;
			defaultOutfit.jointColors[i] = DefaultJointColor;
			defaultOutfit.legSegmentFluffiness[i] = DefaultLegFluffiness;
			defaultOutfit.jointSegmentFluffiness[i] = DefaultJointFluffiness;
		}
		Eye eye = GetDefaultEye().eyeSo.eye;
		defaultOutfit.eyeEffects = new float[eye.NumberOfEffects];
		for (int j = 0; j < eye.NumberOfEffects; j++)
		{
			defaultOutfit.eyeEffects[j] = eye.GetDefaultEffect(j);
		}
	}

	public void UnlockAllDemoItems()
	{
		UnlockAllHatsInDemo();
		UnlockAllWebs();
	}

	public void ResetAllCosmeticItems(int profile = 0)
	{
		Debug.Log("Reset All Cosmetic Items");
		new Dictionary<int, bool>();
		foreach (KeyValuePair<int, bool> item in Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary.ToDictionary((KeyValuePair<int, CosmeticItemSo> keyValuePair) => keyValuePair.Key, (KeyValuePair<int, CosmeticItemSo> keyValuePair) => keyValuePair.Value.unlockedAtStart))
		{
			Singleton<CosmeticItemsController>.Instance.unlockedItems[item.Key] = item.Value;
		}
		SaveController.Save("UnlockedItems", Singleton<CosmeticItemsController>.Instance.unlockedItems, SaveData.Wardrobe, profile);
		new Dictionary<int, bool>();
		foreach (KeyValuePair<int, bool> item2 in Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary.ToDictionary((KeyValuePair<int, CosmeticItemSo> keyValuePair) => keyValuePair.Key, (KeyValuePair<int, CosmeticItemSo> keyValuePair) => keyValuePair.Value.visibleAtStart))
		{
			Singleton<CosmeticItemsController>.Instance.visibleItems[item2.Key] = item2.Value;
		}
		SaveController.Save("VisibleItems", Singleton<CosmeticItemsController>.Instance.visibleItems, SaveData.Wardrobe, profile);
	}

	public int GetItemIndex(CosmeticItemSo cosmeticItemSo)
	{
		return _Scripts.Utils.Utils.GetKeyFromValue(Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary, cosmeticItemSo);
	}

	public void Unlock(CosmeticItemSo cosmeticItemSo)
	{
		if (!IsItemUnlocked(cosmeticItemSo))
		{
			Singleton<CosmeticItemsController>.Instance.OnCosmeticItemUnlocked?.Invoke(this, new OnCosmeticItemUnlockedEventArgs
			{
				cosmeticItemSo = cosmeticItemSo
			});
		}
		UnlockItem(cosmeticItemSo);
	}

	public bool DoesItemExists(CosmeticItemSo cosmeticItemSo)
	{
		return Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary.ContainsValue(cosmeticItemSo);
	}

	public bool IsItemVisible(CosmeticItemSo cosmeticItemSo)
	{
		int itemIndex = GetItemIndex(cosmeticItemSo);
		return Singleton<CosmeticItemsController>.Instance.visibleItems[itemIndex];
	}

	public bool IsItemUnlocked(CosmeticItemSo cosmeticItemSo)
	{
		int itemIndex = GetItemIndex(cosmeticItemSo);
		return Singleton<CosmeticItemsController>.Instance.unlockedItems[itemIndex];
	}

	public bool CanBuyItem(CosmeticItemSo cosmeticItemSo)
	{
		return Singleton<CoinController>.Instance.CheckIfEnoughCoins(cosmeticItemSo.price);
	}

	public void BuyItem(CosmeticItemSo cosmeticItemSo)
	{
		Singleton<CoinController>.Instance.UseCoins(cosmeticItemSo.price);
	}

	public void UnlockItem(CosmeticItemSo cosmeticItemSo)
	{
		int itemIndex = GetItemIndex(cosmeticItemSo);
		Singleton<CosmeticItemsController>.Instance.unlockedItems[itemIndex] = true;
		SaveController.Save("UnlockedItems", Singleton<CosmeticItemsController>.Instance.unlockedItems, SaveData.Wardrobe);
	}

	public bool IsWebItem(int index)
	{
		return cosmeticItemsSo.dictionary[index] is CosmeticItemWebSo;
	}

	public CosmeticItemSo GetCosmeticItem(int index)
	{
		return cosmeticItemsSo.dictionary[index];
	}

	public List<T> GetAllItemsOfType<T>()
	{
		List<T> list = new List<T>();
		foreach (KeyValuePair<int, CosmeticItemSo> item2 in Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary)
		{
			if (item2.Value is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<T> GetUnlockedItems<T>() where T : CosmeticItemSo
	{
		List<T> list = new List<T>();
		foreach (KeyValuePair<int, CosmeticItemSo> item2 in Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary)
		{
			if (item2.Value is T item && Singleton<CosmeticItemsController>.Instance.unlockedItems[item2.Key] && !item2.Value.hideInGame)
			{
				list.Add(item);
			}
		}
		return list.OrderBy((T x) => x.positionInWardrobe).ToList();
	}

	public List<CosmeticItemHatSo> GetVisibleHatItemsInDemo()
	{
		List<CosmeticItemHatSo> list = new List<CosmeticItemHatSo>();
		foreach (KeyValuePair<int, CosmeticItemSo> item in Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary)
		{
			if (item.Value is CosmeticItemHatSo { visibleInDemo: not false } cosmeticItemHatSo)
			{
				list.Add(cosmeticItemHatSo);
			}
		}
		return list.OrderBy((CosmeticItemHatSo x) => x.positionInDemo).ToList();
	}

	public List<T> GetVisibleShopItems<T>() where T : CosmeticItemSo
	{
		List<T> list = new List<T>();
		foreach (KeyValuePair<int, CosmeticItemSo> item2 in Singleton<CosmeticItemsController>.Instance.cosmeticItemsSo.dictionary)
		{
			if (item2.Value is T item && Singleton<CosmeticItemsController>.Instance.visibleItems[item2.Key] && !Singleton<CosmeticItemsController>.Instance.unlockedItems[item2.Key] && !item2.Value.hideInGame)
			{
				list.Add(item);
			}
		}
		return list.OrderBy((T x) => x.positionInWardrobe).ToList();
	}

	public int GetValidEyeIndex(int index)
	{
		if (index == 0 || index >= cosmeticItemsSo.dictionary.Count || !cosmeticItemsSo.dictionary.ContainsKey(index) || !(GetCosmeticItem(index) is CosmeticItemEyeSo))
		{
			return GetDefaultEyeIndex();
		}
		return index;
	}

	public int GetValidHatIndex(int index)
	{
		if (index == 0 || index >= cosmeticItemsSo.dictionary.Count || !cosmeticItemsSo.dictionary.ContainsKey(index) || !(GetCosmeticItem(index) is CosmeticItemHatSo))
		{
			return GetDefaultHatIndex();
		}
		return index;
	}

	public int GetValidAccessoryIndex(int index)
	{
		if (index == 0 || index >= cosmeticItemsSo.dictionary.Count || !cosmeticItemsSo.dictionary.ContainsKey(index) || !(GetCosmeticItem(index) is CosmeticItemAccessorySo))
		{
			return GetDefaultAccessoryIndex();
		}
		return index;
	}

	public int GetValidShoeIndex(int index)
	{
		if (index == 0 || index >= cosmeticItemsSo.dictionary.Count || !cosmeticItemsSo.dictionary.ContainsKey(index) || !(GetCosmeticItem(index) is CosmeticItemShoeSo))
		{
			return GetDefaultShoeIndex();
		}
		return index;
	}

	public int GetValidWebIndex(int index)
	{
		if (index == 0 || index >= cosmeticItemsSo.dictionary.Count || !cosmeticItemsSo.dictionary.ContainsKey(index) || !(GetCosmeticItem(index) is CosmeticItemWebSo))
		{
			return GetDefaultWebIndex();
		}
		return index;
	}

	public CosmeticItemEyeSo GetDefaultEye()
	{
		return defaultEyeSo;
	}

	public CosmeticItemHatSo GetDefaultHat()
	{
		return defaultHatSo;
	}

	public CosmeticItemAccessorySo GetDefaultAccessory()
	{
		return defaultAccessorySo;
	}

	public CosmeticItemShoeSo GetDefaultShoe()
	{
		return defaultShoeSo;
	}

	public int GetDefaultEyeIndex()
	{
		return GetItemIndex(defaultEyeSo);
	}

	public int GetDefaultHatIndex()
	{
		return GetItemIndex(defaultHatSo);
	}

	public int GetDefaultAccessoryIndex()
	{
		return GetItemIndex(defaultAccessorySo);
	}

	public int GetDefaultShoeIndex()
	{
		return GetItemIndex(defaultShoeSo);
	}

	public int GetDefaultWebIndex()
	{
		return GetItemIndex(defaultWebSo);
	}

	public void MakeCosmeticItemsVisibleForKitchen()
	{
		MakeCosmeticItemsAvailableInShop(StoryLevel.Kitchen);
	}

	public void MakeCosmeticItemsVisibleForOffice()
	{
		MakeCosmeticItemsAvailableInShop(StoryLevel.Office);
	}

	public void MakeCosmeticItemsVisibleForKidsRoom()
	{
		MakeCosmeticItemsAvailableInShop(StoryLevel.KidsRoom);
	}

	public void MakeCosmeticItemsVisibleForLivingRoom()
	{
		MakeCosmeticItemsAvailableInShop(StoryLevel.LivingRoom);
	}

	public void MakeExoticItemsVisibleForKitchen()
	{
		MakeExoticItemsAvailableInShop(StoryLevel.Kitchen);
	}

	public void MakeExoticItemsVisibleForOffice()
	{
		MakeExoticItemsAvailableInShop(StoryLevel.Office);
	}

	public void MakeExoticItemsVisibleForKidsRoom()
	{
		MakeExoticItemsAvailableInShop(StoryLevel.KidsRoom);
	}

	public void MakeExoticItemsVisibleForLivingRoom()
	{
		MakeExoticItemsAvailableInShop(StoryLevel.LivingRoom);
	}

	public void ResetSpiderCustomization()
	{
		SaveBodyEnabled(value: true);
		SaveBodyColor(DefaultBodyColor);
		SaveBodyFluffiness(0f);
		SaveAbdomenEnabled(value: false);
		SaveAbdomenColor(DefaultAbdomenColor);
		SaveAbdomenFluffiness(0f);
		SaveLegsEnabled(defaultLegsEnabled);
		for (int i = 0; i < 3; i++)
		{
			SaveLegColor(i, DefaultLegColor);
			SaveJointColor(i, DefaultJointColor);
			SaveLegFluffiness(i, 0.25f);
			SaveJointFluffiness(i, 0.5f);
		}
		SaveEye(GetDefaultHatIndex());
		SaveEyeColorBase(DefaultEyeColorBase);
		SaveEyeColorLeft(DefaultEyeColorLeft);
		SaveEyeColorRight(DefaultEyeColorRight);
		SaveHat(GetDefaultHatIndex());
		SaveAccessory(GetDefaultAccessoryIndex());
		SaveShoe(GetDefaultShoeIndex());
		for (int j = 0; j < 6; j++)
		{
			DeleteEyeEffect(j);
			DeleteHatColor(j);
			DeleteHatEffect(j);
			DeleteAccessoryColor(j);
			DeleteAccessoryEffect(j);
			DeleteShoeColor(j);
			DeleteShoeEffect(j);
		}
	}

	public int GetColorIndex(Color color)
	{
		for (int i = 0; i < ColorPalette.colors.Count; i++)
		{
			Color color2 = ColorPalette.colors[i];
			if (Mathf.Abs(color.r - color2.r) <= 0.01f && Mathf.Abs(color.g - color2.g) <= 0.01f && Mathf.Abs(color.b - color2.b) <= 0.01f)
			{
				return i;
			}
		}
		return -1;
	}

	public Color GetRandomColor()
	{
		return ColorPalette.colors.RandomValue();
	}

	public CosmeticItemHatSo GetRandomUnlockedHat()
	{
		List<CosmeticItemHatSo> list = GetUnlockedItems<CosmeticItemHatSo>();
		if (list.Count <= 0)
		{
			return GetDefaultHat();
		}
		return list.RandomValue();
	}

	public CosmeticItemAccessorySo GetRandomUnlockedAccessory()
	{
		List<CosmeticItemAccessorySo> list = GetUnlockedItems<CosmeticItemAccessorySo>();
		if (list.Count <= 0)
		{
			return GetDefaultAccessory();
		}
		return list.RandomValue();
	}

	public CosmeticItemShoeSo GetRandomUnlockedShoe()
	{
		List<CosmeticItemShoeSo> list = GetUnlockedItems<CosmeticItemShoeSo>();
		if (list.Count <= 0)
		{
			return GetDefaultShoe();
		}
		return list.RandomValue();
	}

	public CosmeticItemEyeSo GetRandomUnlockedEye()
	{
		List<CosmeticItemEyeSo> list = GetUnlockedItems<CosmeticItemEyeSo>();
		if (list.Count <= 0)
		{
			return GetDefaultEye();
		}
		return list.RandomValue();
	}

	public void SaveBodyEnabled(bool value)
	{
		SaveController.Save("BodyEnabled", value, SaveData.Wardrobe);
	}

	public bool LoadBodyEnabled()
	{
		return SaveController.Load("BodyEnabled", defaultValue: true, SaveData.Wardrobe);
	}

	public void SaveBodyColor(Color color)
	{
		color.a = 1f;
		SaveController.Save("BodyColor", color, SaveData.Wardrobe);
	}

	public Color LoadBodyColor()
	{
		return SaveController.Load("BodyColor", DefaultBodyColor, SaveData.Wardrobe);
	}

	public void SaveBodyFluffiness(float value)
	{
		SaveController.Save("BodyFluffiness", value, SaveData.Wardrobe);
	}

	public float LoadBodyFluffiness()
	{
		return SaveController.Load("BodyFluffiness", 0f, SaveData.Wardrobe);
	}

	public void SaveLegColor(int index, Color color)
	{
		color.a = 1f;
		SaveController.Save($"LegColor{index}", color, SaveData.Wardrobe);
	}

	public Color LoadLegColor(int index)
	{
		return SaveController.Load($"LegColor{index}", DefaultLegColor, SaveData.Wardrobe);
	}

	public void SaveLegFluffiness(int index, float value)
	{
		SaveController.Save($"LegFluffiness{index}", value, SaveData.Wardrobe);
	}

	public float LoadLegFluffiness(int index)
	{
		return SaveController.Load($"LegFluffiness{index}", 0.25f, SaveData.Wardrobe);
	}

	public void SaveLegsEnabled(bool[] values)
	{
		SaveController.Save("LegsEnabled", values, SaveData.Wardrobe);
	}

	public bool[] LoadLegsEnabled()
	{
		bool[] array = defaultLegsEnabled.ToArray();
		bool[] array2 = SaveController.Load("LegsEnabled", defaultLegsEnabled, SaveData.Wardrobe);
		if (array2 == null)
		{
			return array;
		}
		for (int i = 0; i < array2.Length; i++)
		{
			array[i] = array2[i];
		}
		return array2;
	}

	public void SaveJointColor(int index, Color color)
	{
		color.a = 1f;
		SaveController.Save($"JointColor{index}", color, SaveData.Wardrobe);
	}

	public Color LoadJointColor(int index)
	{
		return SaveController.Load($"JointColor{index}", DefaultJointColor, SaveData.Wardrobe);
	}

	public void SaveJointFluffiness(int index, float value)
	{
		SaveController.Save($"JointFluffiness{index}", value, SaveData.Wardrobe);
	}

	public float LoadJointFluffiness(int index)
	{
		return SaveController.Load($"JointFluffiness{index}", 0.5f, SaveData.Wardrobe);
	}

	public void SaveAbdomenEnabled(bool value)
	{
		SaveController.Save("AbdomenEnabled", value, SaveData.Wardrobe);
	}

	public bool LoadAbdomenEnabled()
	{
		return SaveController.Load("AbdomenEnabled", defaultValue: false, SaveData.Wardrobe);
	}

	public void SaveAbdomenColor(Color color)
	{
		color.a = 1f;
		SaveController.Save("AbdomenColor", color, SaveData.Wardrobe);
	}

	public Color LoadAbdomenColor()
	{
		return SaveController.Load("AbdomenColor", DefaultAbdomenColor, SaveData.Wardrobe);
	}

	public void SaveAbdomenFluffiness(float value)
	{
		SaveController.Save("AbdomenFluffiness", value, SaveData.Wardrobe);
	}

	public float LoadAbdomenFluffiness()
	{
		return SaveController.Load("AbdomenFluffiness", 0f, SaveData.Wardrobe);
	}

	public void SaveEye(int index)
	{
		SaveController.Save("Eye", index, SaveData.Wardrobe);
	}

	public int LoadEye()
	{
		return GetValidEyeIndex(SaveController.Load("Eye", GetDefaultEyeIndex(), SaveData.Wardrobe));
	}

	public Color LoadEyeColor(int index)
	{
		return index switch
		{
			1 => LoadEyeColorLeft(), 
			2 => LoadEyeColorRight(), 
			_ => LoadEyeColorBase(), 
		};
	}

	public void SaveEyeColorBase(Color color)
	{
		color.a = 1f;
		SaveController.Save("EyeColorBase", color, SaveData.Wardrobe);
	}

	public Color LoadEyeColorBase()
	{
		return SaveController.Load("EyeColorBase", DefaultEyeColorBase, SaveData.Wardrobe);
	}

	public void SaveEyeColorLeft(Color color)
	{
		color.a = 1f;
		SaveController.Save("EyeColorLeft", color, SaveData.Wardrobe);
	}

	public Color LoadEyeColorLeft()
	{
		return SaveController.Load("EyeColorLeft", DefaultEyeColorLeft, SaveData.Wardrobe);
	}

	public void SaveEyeColorRight(Color color)
	{
		color.a = 1f;
		SaveController.Save("EyeColorRight", color, SaveData.Wardrobe);
	}

	public Color LoadEyeColorRight()
	{
		return SaveController.Load("EyeColorRight", DefaultEyeColorRight, SaveData.Wardrobe);
	}

	public bool EyeEffectExists(int index)
	{
		return SaveController.Exists($"EyeEffect{index}", SaveData.Wardrobe);
	}

	public void DeleteEyeEffect(int index)
	{
		SaveController.DeleteKey($"EyeEffect{index}", SaveData.Wardrobe);
	}

	public void SaveEyeEffect(int index, float effect)
	{
		SaveController.Save($"EyeEffect{index}", effect, SaveData.Wardrobe);
	}

	public float LoadEyeEffect(int index)
	{
		return SaveController.Load($"EyeEffect{index}", GetDefaultEye().eyeSo.eye.GetDefaultEffect(index), SaveData.Wardrobe);
	}

	public void SaveHat(int index)
	{
		SaveController.Save("Hat", index, SaveData.Wardrobe);
	}

	public int LoadHat()
	{
		return GetValidHatIndex(SaveController.Load("Hat", GetDefaultHatIndex(), SaveData.Wardrobe));
	}

	public bool HatColorExists(int index)
	{
		return SaveController.Exists($"HatColor{index}", SaveData.Wardrobe);
	}

	public void DeleteHatColor(int index)
	{
		SaveController.DeleteKey($"HatColor{index}", SaveData.Wardrobe);
	}

	public void SaveHatColor(int index, Color color)
	{
		color.a = 1f;
		SaveController.Save($"HatColor{index}", color, SaveData.Wardrobe);
	}

	public Color LoadHatColor(int index)
	{
		return SaveController.Load($"HatColor{index}", Color.black, SaveData.Wardrobe);
	}

	public bool HatEffectExists(int index)
	{
		return SaveController.Exists($"HatEffect{index}", SaveData.Wardrobe);
	}

	public void DeleteHatEffect(int index)
	{
		SaveController.DeleteKey($"HatEffect{index}", SaveData.Wardrobe);
	}

	public void SaveHatEffect(int index, float effect)
	{
		SaveController.Save($"HatEffect{index}", effect, SaveData.Wardrobe);
	}

	public float LoadHatEffect(int index)
	{
		return SaveController.Load($"HatEffect{index}", 0f, SaveData.Wardrobe);
	}

	public void SaveAccessory(int index)
	{
		SaveController.Save("Accessory", index, SaveData.Wardrobe);
	}

	public int LoadAccessory()
	{
		return GetValidAccessoryIndex(SaveController.Load("Accessory", GetDefaultAccessoryIndex(), SaveData.Wardrobe));
	}

	public bool AccessoryColorExists(int index)
	{
		return SaveController.Exists($"AccessoryColor{index}", SaveData.Wardrobe);
	}

	public void DeleteAccessoryColor(int index)
	{
		SaveController.DeleteKey($"AccessoryColor{index}", SaveData.Wardrobe);
	}

	public void SaveAccessoryColor(int index, Color color)
	{
		color.a = 1f;
		SaveController.Save($"AccessoryColor{index}", color, SaveData.Wardrobe);
	}

	public Color LoadAccessoryColor(int index)
	{
		return SaveController.Load($"AccessoryColor{index}", Color.black, SaveData.Wardrobe);
	}

	public bool AccessoryEffectExists(int index)
	{
		return SaveController.Exists($"AccessoryEffect{index}", SaveData.Wardrobe);
	}

	public void DeleteAccessoryEffect(int index)
	{
		SaveController.DeleteKey($"AccessoryEffect{index}", SaveData.Wardrobe);
	}

	public void SaveAccessoryEffect(int index, float value)
	{
		SaveController.Save($"AccessoryEffect{index}", value, SaveData.Wardrobe);
	}

	public float LoadAccessoryEffect(int index)
	{
		return SaveController.Load($"AccessoryEffect{index}", 0f, SaveData.Wardrobe);
	}

	public void SaveShoe(int index)
	{
		SaveController.Save("Shoe", index, SaveData.Wardrobe);
	}

	public int LoadShoe()
	{
		return GetValidShoeIndex(SaveController.Load("Shoe", GetDefaultShoeIndex(), SaveData.Wardrobe));
	}

	public bool ShoeColorExists(int index)
	{
		return SaveController.Exists($"ShoeColor{index}", SaveData.Wardrobe);
	}

	public void DeleteShoeColor(int index)
	{
		SaveController.DeleteKey($"ShoeColor{index}", SaveData.Wardrobe);
	}

	public void SaveShoeColor(int index, Color color)
	{
		color.a = 1f;
		SaveController.Save($"ShoeColor{index}", color, SaveData.Wardrobe);
	}

	public Color LoadShoeColor(int index)
	{
		return SaveController.Load($"ShoeColor{index}", Color.black, SaveData.Wardrobe);
	}

	public bool ShoeEffectExists(int index)
	{
		return SaveController.Exists($"ShoeEffect{index}", SaveData.Wardrobe);
	}

	public void DeleteShoeEffect(int index)
	{
		SaveController.DeleteKey($"ShoeEffect{index}", SaveData.Wardrobe);
	}

	public void SaveShoeEffect(int index, float value)
	{
		SaveController.Save($"ShoeEffect{index}", value, SaveData.Wardrobe);
	}

	public float LoadShoeEffect(int index)
	{
		return SaveController.Load($"ShoeEffect{index}", 0f, SaveData.Wardrobe);
	}

	public void SaveCurrentOutfit()
	{
		Outfit outfit = GenerateCurrentOutfit();
		outfitCounter++;
		SaveController.Save("OutfitCounter", outfitCounter, SaveData.Wardrobe);
		SaveController.SaveOutfit(outfit);
	}

	public void ApplyOutfit(Outfit outfit)
	{
		SettingsController.SetArachnophobiaMode(outfit.arachnophobiaMode);
		SaveBodyEnabled(outfit.bodyEnabled);
		SaveBodyColor(outfit.bodyColor);
		SaveBodyFluffiness(outfit.bodyFluffiness);
		SaveAbdomenEnabled(outfit.abdomenEnabled);
		SaveAbdomenColor(outfit.abdomenColor);
		SaveAbdomenFluffiness(outfit.abdomenFluffiness);
		SaveLegsEnabled(outfit.legsEnabled);
		for (int i = 0; i < 3; i++)
		{
			SaveLegColor(i, (outfit.legColors == null) ? DefaultLegColor : outfit.legColors[i]);
			SaveJointColor(i, (outfit.jointColors == null) ? DefaultJointColor : outfit.jointColors[i]);
			SaveLegFluffiness(i, (outfit.legSegmentFluffiness == null) ? 0.25f : outfit.legSegmentFluffiness[i]);
			SaveJointFluffiness(i, (outfit.jointSegmentFluffiness == null) ? 0.5f : outfit.jointSegmentFluffiness[i]);
		}
		SaveEye(outfit.eyeIndex);
		SaveEyeColorBase(outfit.eyeColorBase);
		SaveEyeColorLeft(outfit.eyeColorLeft);
		SaveEyeColorRight(outfit.eyeColorRight);
		SaveHat(outfit.hatIndex);
		SaveAccessory(outfit.accessoryIndex);
		SaveShoe(outfit.shoeIndex);
		Eye eye = ((CosmeticItemEyeSo)GetCosmeticItem(outfit.eyeIndex)).eyeSo.eye;
		for (int j = 0; j < eye.NumberOfEffects; j++)
		{
			SaveEyeEffect(j, (j < outfit.eyeEffects.Length) ? outfit.eyeEffects[j] : eye.GetDefaultEffect(j));
		}
		Hat hat = ((CosmeticItemHatSo)GetCosmeticItem(outfit.hatIndex)).hatSo.hat;
		for (int k = 0; k < hat.NumberOfColors; k++)
		{
			SaveHatColor(k, (k < outfit.hatColors.Length) ? outfit.hatColors[k] : hat.GetDefaultColor(k));
		}
		for (int l = 0; l < hat.NumberOfEffects; l++)
		{
			SaveHatEffect(l, (l < outfit.hatEffects.Length) ? outfit.hatEffects[l] : hat.GetDefaultEffect(l));
		}
		Accessory accessory = ((CosmeticItemAccessorySo)GetCosmeticItem(outfit.accessoryIndex)).accessorySo.accessory;
		for (int m = 0; m < accessory.NumberOfColors; m++)
		{
			SaveAccessoryColor(m, (m < outfit.accessoryColors.Length) ? outfit.accessoryColors[m] : accessory.GetDefaultColor(m));
		}
		for (int n = 0; n < accessory.NumberOfEffects; n++)
		{
			SaveAccessoryEffect(n, (n < outfit.accessoryEffects.Length) ? outfit.accessoryEffects[n] : accessory.GetDefaultEffect(n));
		}
		Shoe shoe = ((CosmeticItemShoeSo)GetCosmeticItem(outfit.shoeIndex)).shoeSo.shoe;
		for (int num = 0; num < shoe.NumberOfColors; num++)
		{
			SaveShoeColor(num, (num < outfit.shoeColors.Length) ? outfit.shoeColors[num] : shoe.GetDefaultColor(num));
		}
		for (int num2 = 0; num2 < shoe.NumberOfEffects; num2++)
		{
			SaveShoeEffect(num2, (num2 < outfit.shoeEffects.Length) ? outfit.shoeEffects[num2] : shoe.GetDefaultEffect(num2));
		}
	}

	private bool CheckIfOutfitsAreEqual(string outfitName)
	{
		Outfit outfit = SaveController.LoadOutfit(outfitName);
		return CheckIfOutfitsAreEqual(outfit);
	}

	public bool CheckIfOutfitsAreEqual(Outfit outfit)
	{
		bool flag = GenerateCurrentOutfit() == outfit;
		Debug.Log($"Outfits are equal: {flag}");
		return flag;
	}

	private void ApplyOutfit(string outfitName)
	{
		Outfit[] array = SaveController.LoadAllOutfits();
		foreach (Outfit outfit in array)
		{
			if (outfit.name == outfitName)
			{
				ApplyOutfit(outfit);
				Singleton<GameController>.Instance.Player.Customization.LoadDefaultIndices();
				Singleton<GameController>.Instance.Player.Customization.Refresh();
				break;
			}
		}
	}

	private Outfit GenerateCurrentOutfit()
	{
		Outfit outfit = new Outfit
		{
			version = Outfit.CurrentVersion,
			name = $"Outfit_{outfitCounter}",
			arachnophobiaMode = SettingsController.ArachnophobiaMode,
			bodyEnabled = LoadBodyEnabled(),
			bodyColor = LoadBodyColor(),
			bodyFluffiness = LoadBodyFluffiness(),
			abdomenEnabled = LoadAbdomenEnabled(),
			abdomenColor = LoadAbdomenColor(),
			abdomenFluffiness = LoadAbdomenFluffiness(),
			legsEnabled = LoadLegsEnabled(),
			eyeIndex = LoadEye(),
			eyeColorBase = LoadEyeColorBase(),
			eyeColorLeft = LoadEyeColorLeft(),
			eyeColorRight = LoadEyeColorRight(),
			hatIndex = LoadHat(),
			accessoryIndex = LoadAccessory(),
			shoeIndex = LoadShoe()
		};
		outfit.legColors = new Color[3];
		outfit.jointColors = new Color[3];
		outfit.legSegmentFluffiness = new float[3];
		outfit.jointSegmentFluffiness = new float[3];
		for (int i = 0; i < 3; i++)
		{
			outfit.legColors[i] = LoadLegColor(i);
			outfit.jointColors[i] = LoadJointColor(i);
			outfit.legSegmentFluffiness[i] = LoadLegFluffiness(i);
			outfit.jointSegmentFluffiness[i] = LoadJointFluffiness(i);
		}
		Eye eye = ((CosmeticItemEyeSo)GetCosmeticItem(outfit.eyeIndex)).eyeSo.eye;
		outfit.eyeEffects = new float[eye.NumberOfEffects];
		for (int j = 0; j < eye.NumberOfEffects; j++)
		{
			outfit.eyeEffects[j] = LoadEyeEffect(j);
		}
		Hat hat = ((CosmeticItemHatSo)GetCosmeticItem(outfit.hatIndex)).hatSo.hat;
		outfit.hatColors = new Color[hat.NumberOfColors];
		for (int k = 0; k < hat.NumberOfColors; k++)
		{
			outfit.hatColors[k] = LoadHatColor(k);
		}
		outfit.hatEffects = new float[hat.NumberOfEffects];
		for (int l = 0; l < hat.NumberOfEffects; l++)
		{
			outfit.hatEffects[l] = LoadHatEffect(l);
		}
		Accessory accessory = ((CosmeticItemAccessorySo)GetCosmeticItem(outfit.accessoryIndex)).accessorySo.accessory;
		outfit.accessoryColors = new Color[accessory.NumberOfColors];
		for (int m = 0; m < accessory.NumberOfColors; m++)
		{
			outfit.accessoryColors[m] = LoadAccessoryColor(m);
		}
		outfit.accessoryEffects = new float[accessory.NumberOfEffects];
		for (int n = 0; n < accessory.NumberOfEffects; n++)
		{
			outfit.accessoryEffects[n] = LoadAccessoryEffect(n);
		}
		Shoe shoe = ((CosmeticItemShoeSo)GetCosmeticItem(outfit.shoeIndex)).shoeSo.shoe;
		outfit.shoeColors = new Color[shoe.NumberOfColors];
		for (int num = 0; num < shoe.NumberOfColors; num++)
		{
			outfit.shoeColors[num] = LoadShoeColor(num);
		}
		outfit.shoeEffects = new float[shoe.NumberOfEffects];
		for (int num2 = 0; num2 < shoe.NumberOfEffects; num2++)
		{
			outfit.shoeEffects[num2] = LoadShoeEffect(num2);
		}
		return outfit;
	}

	public int GetOutfitCounter()
	{
		return outfitCounter;
	}

	private void SceneController_OnSceneLoaded(object sender, EventArgs eventArgs)
	{
		CheckUnlockedItems();
	}
}
