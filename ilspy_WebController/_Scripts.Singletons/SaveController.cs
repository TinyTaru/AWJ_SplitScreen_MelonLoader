using System;
using System.IO;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.Wardrobe;

namespace _Scripts.Singletons;

public static class SaveController
{
	public static event EventHandler OnDialogueSystemSaveDataLoaded;

	private static ES3Settings GetES3Setting(SaveData saveData, int profile = 0)
	{
		string text = "";
		text = saveData switch
		{
			SaveData.General => "General.es3", 
			SaveData.Settings => "Settings.es3", 
			SaveData.MobileUI => "MobileUI.es3", 
			SaveData.Controls => "Controls.es3", 
			SaveData.Game => (profile != 0) ? string.Format("Profile{0}/{1}.es3", profile, "Game") : string.Format("Profile{0}/{1}.es3", Singleton<ProfileController>.Instance.CurrentProfile, "Game"), 
			SaveData.Wardrobe => (profile != 0) ? string.Format("Profile{0}/{1}.es3", profile, "Wardrobe") : string.Format("Profile{0}/{1}.es3", Singleton<ProfileController>.Instance.CurrentProfile, "Wardrobe"), 
			SaveData.Achievements => "Achievements.es3", 
			_ => throw new ArgumentOutOfRangeException("saveData", saveData, null), 
		};
		string text2 = Path.Combine(Application.persistentDataPath, text);
		if (new FileInfo(text2).Exists && string.IsNullOrWhiteSpace(File.ReadAllText(text2)))
		{
			Debug.Log("File Empty! Deleting");
			File.Delete(text2);
		}
		return new ES3Settings(text);
	}

	public static void DeleteKey(string key, SaveData saveData)
	{
		if (Exists(key, saveData))
		{
			ES3.DeleteKey(key, GetES3Setting(saveData));
		}
	}

	public static void DeleteAllFiles()
	{
		ES3.DeleteFile();
		ES3.DeleteFile(GetES3Setting(SaveData.Settings));
		ES3.DeleteFile(GetES3Setting(SaveData.MobileUI));
		ES3.DeleteFile(GetES3Setting(SaveData.Controls));
		ES3.DeleteFile(GetES3Setting(SaveData.Achievements));
		DeleteProfile(1);
		DeleteProfile(2);
		DeleteProfile(3);
	}

	public static void DeleteFile(SaveData saveData)
	{
		ES3.DeleteFile(GetES3Setting(saveData));
	}

	public static void DeleteProfile(int profile)
	{
		Debug.Log($"Deleting Profile {profile}");
		ES3.DeleteDirectory($"Profile{profile}");
		ResetDialogueSystemData();
		if (Singleton<CosmeticItemsController>.Instance != null)
		{
			Singleton<CosmeticItemsController>.Instance.ResetAllCosmeticItems(profile);
		}
	}

	public static bool Exists(string key, SaveData saveData)
	{
		return ES3.KeyExists(key, GetES3Setting(saveData));
	}

	public static void Save<T>(string key, T value, SaveData saveData)
	{
		ES3.Save(key, value, GetES3Setting(saveData));
	}

	public static void Save<T>(string key, T value, SaveData saveData, int profile)
	{
		ES3.Save(key, value, GetES3Setting(saveData, profile));
	}

	public static T Load<T>(string key, T defaultValue, SaveData saveData, int profile = 0)
	{
		try
		{
			return ES3.Load(key, defaultValue, GetES3Setting(saveData, profile));
		}
		catch (Exception)
		{
			if (Exists(key, saveData))
			{
				ES3.DeleteKey(key, GetES3Setting(saveData, profile));
			}
			ES3.Save(key, defaultValue, GetES3Setting(saveData, profile));
			return defaultValue;
		}
	}

	public static string LoadString(string key, string defaultValue, SaveData saveData, int profile = 0)
	{
		try
		{
			return ES3.LoadString(key, defaultValue, GetES3Setting(saveData, profile));
		}
		catch (Exception)
		{
			if (Exists(key, saveData))
			{
				ES3.DeleteKey(key, GetES3Setting(saveData, profile));
			}
			ES3.Save(key, defaultValue, GetES3Setting(saveData, profile));
			return defaultValue;
		}
	}

	public static void SaveDialogueSystemData()
	{
		string saveData = PersistentDataManager.GetSaveData();
		Save("DialogueSystemSaveData", saveData, SaveData.Game);
	}

	public static void LoadDialogueSystemData()
	{
		PersistentDataManager.ApplySaveData(LoadString("DialogueSystemSaveData", string.Empty, SaveData.Game));
		SaveController.OnDialogueSystemSaveDataLoaded?.Invoke(null, EventArgs.Empty);
	}

	public static void ResetDialogueSystemData()
	{
		PersistentDataManager.Reset(DatabaseResetOptions.KeepAllLoaded);
	}

	public static void SaveLevelData<T>(string key, T value, LevelSaveData levelSaveData)
	{
		ES3.Save(key, value, GetLevelDataES3Setting(levelSaveData.sceneName, levelSaveData.fileName));
	}

	public static T LoadLevelData<T>(string key, T defaultValue, LevelSaveData levelSaveData) where T : class
	{
		if (ES3.Load<object>(key, (object)defaultValue, GetLevelDataES3Setting(levelSaveData.sceneName, levelSaveData.fileName)) is T result)
		{
			return result;
		}
		return null;
	}

	private static T LoadLevelData<T>(string key, T defaultValue, string levelName, string fileName)
	{
		return ES3.Load(key, defaultValue, GetLevelDataES3Setting(levelName, fileName));
	}

	public static LevelSaveData GetLevelSaveData(string levelName, string fileName)
	{
		Debug.Log("GetLevelSaveData " + levelName);
		string path = $"Profile{Singleton<ProfileController>.Instance.CurrentProfile}/{levelName}";
		string path2 = Path.Combine(Application.persistentDataPath, path);
		if (!Directory.Exists(path2))
		{
			return null;
		}
		string[] files = Directory.GetFiles(path2, "*.es3");
		for (int i = 0; i < files.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
			if (!(fileName != fileNameWithoutExtension))
			{
				return new LevelSaveData
				{
					sceneName = levelName,
					fileName = fileName,
					date = LoadLevelData("LevelSaveData", new LevelSaveData(), levelName, fileName).date
				};
			}
		}
		return null;
	}

	public static LevelSaveData[] GetAllLevelSaveDataForLevel(string levelName)
	{
		string path = $"Profile{Singleton<ProfileController>.Instance.CurrentProfile}/{levelName}";
		string path2 = Path.Combine(Application.persistentDataPath, path);
		if (!Directory.Exists(path2))
		{
			return null;
		}
		string[] files = Directory.GetFiles(path2, "*.es3");
		LevelSaveData[] array = new LevelSaveData[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(files[i]);
			LevelSaveData levelSaveData = new LevelSaveData
			{
				sceneName = levelName,
				fileName = fileNameWithoutExtension,
				date = LoadLevelData("LevelSaveData", new LevelSaveData(), levelName, fileNameWithoutExtension).date
			};
			array[i] = levelSaveData;
		}
		return array;
	}

	private static ES3Settings GetLevelDataES3Setting(string levelName, string fileName, ES3.CompressionType compressionType = ES3.CompressionType.None)
	{
		string text = $"Profile{Singleton<ProfileController>.Instance.CurrentProfile}/{levelName}/{fileName}.es3";
		string text2 = Path.Combine(Application.persistentDataPath, text);
		if (new FileInfo(text2).Exists && string.IsNullOrWhiteSpace(File.ReadAllText(text2)))
		{
			Debug.Log("File Empty! Deleting");
			File.Delete(text2);
		}
		return new ES3Settings
		{
			path = text,
			compressionType = compressionType
		};
	}

	public static void DeleteSaveFile(string levelName, string fileName)
	{
		string text = Path.Combine(Application.persistentDataPath, $"Profile{Singleton<ProfileController>.Instance.CurrentProfile}", levelName, fileName + ".es3");
		if (new FileInfo(text).Exists)
		{
			Debug.Log("Deleted save file " + levelName + "/" + fileName);
			File.Delete(text);
		}
	}

	public static bool RenameSaveFile(string levelName, string oldFileName, string newFileName)
	{
		if (newFileName == string.Empty)
		{
			return false;
		}
		if (newFileName == oldFileName)
		{
			return true;
		}
		string path = $"Profile{Singleton<ProfileController>.Instance.CurrentProfile}";
		string path2 = Path.Combine(Application.persistentDataPath, path, levelName);
		string text = Path.Combine(path2, oldFileName + ".es3");
		string text2 = Path.Combine(path2, newFileName + ".es3");
		try
		{
			if (!File.Exists(text))
			{
				Debug.LogWarning("Rename failed: source save not found '" + levelName + "/" + oldFileName + ".es3'");
				return false;
			}
			if (File.Exists(text2))
			{
				Debug.LogWarning("Rename failed: destination already exists '" + levelName + "/" + newFileName + ".es3'");
				return false;
			}
			File.Move(text, text2);
			Debug.Log("Renamed save file " + levelName + "/" + oldFileName + " -> " + levelName + "/" + newFileName);
			LevelSaveData levelSaveData = new LevelSaveData
			{
				sceneName = levelName,
				fileName = newFileName,
				date = DateTime.Now
			};
			SaveLevelData("LevelSaveData", levelSaveData, levelSaveData);
			return true;
		}
		catch (UnauthorizedAccessException ex)
		{
			Debug.LogError("Rename failed due to access permissions: " + ex.Message);
			return false;
		}
		catch (IOException ex2)
		{
			Debug.LogError("Rename failed due to I/O error: " + ex2.Message);
			return false;
		}
		catch (Exception ex3)
		{
			Debug.LogError("Rename failed due to unexpected error: " + ex3.Message);
			return false;
		}
	}

	public static void SaveOutfit(Outfit outfit)
	{
		ES3.Save("Outfit", outfit, GetOutfitES3Setting(outfit.name));
	}

	public static Outfit LoadOutfit(string outfitName)
	{
		return ES3.Load<Outfit>("Outfit", GetOutfitES3Setting(outfitName));
	}

	public static Outfit[] LoadAllOutfits()
	{
		string path = $"Profile{Singleton<ProfileController>.Instance.CurrentProfile}/Outfits";
		string path2 = Path.Combine(Application.persistentDataPath, path);
		if (!Directory.Exists(path2))
		{
			return null;
		}
		string[] files = Directory.GetFiles(path2, "*.es3");
		Outfit[] array = new Outfit[files.Length];
		for (int i = 0; i < files.Length; i++)
		{
			Outfit outfit = LoadOutfit(Path.GetFileNameWithoutExtension(files[i]));
			MigrateOutfitIfNeeded(ref outfit);
			array[i] = outfit;
		}
		return array;
	}

	private static void MigrateOutfitIfNeeded(ref Outfit outfit)
	{
		bool flag = false;
		if (outfit.version < 2)
		{
			outfit.bodyEnabled = true;
			outfit.version = 2;
			flag = true;
		}
		if (flag)
		{
			SaveOutfit(outfit);
		}
	}

	public static void DeleteOutfit(string outfitName)
	{
		string text = Path.Combine(Application.persistentDataPath, $"Profile{Singleton<ProfileController>.Instance.CurrentProfile}", "Outfits", outfitName + ".es3");
		if (new FileInfo(text).Exists)
		{
			File.Delete(text);
			Debug.Log("Deleted outfit '" + outfitName + "'!");
		}
	}

	public static bool RenameOutfit(string oldOutfitName, string newOutfitName)
	{
		if (newOutfitName == string.Empty)
		{
			return false;
		}
		if (newOutfitName == oldOutfitName)
		{
			return true;
		}
		string path = $"Profile{Singleton<ProfileController>.Instance.CurrentProfile}";
		string path2 = Path.Combine(Application.persistentDataPath, path, "Outfits");
		string text = Path.Combine(path2, oldOutfitName + ".es3");
		string text2 = Path.Combine(path2, newOutfitName + ".es3");
		try
		{
			if (!File.Exists(text))
			{
				Debug.LogWarning("Renaming outfit failed: source not found '" + oldOutfitName + ".es3'");
				return false;
			}
			if (File.Exists(text2))
			{
				Debug.LogWarning("Renaming outfit failed: destination already exists '" + newOutfitName + ".es3'");
				return false;
			}
			File.Move(text, text2);
			Debug.Log("Renamed outfit " + oldOutfitName + " -> " + newOutfitName);
			Outfit outfit = LoadOutfit(newOutfitName);
			outfit.name = newOutfitName;
			SaveOutfit(outfit);
			return true;
		}
		catch (UnauthorizedAccessException ex)
		{
			Debug.LogError("Rename failed due to access permissions: " + ex.Message);
			return false;
		}
		catch (IOException ex2)
		{
			Debug.LogError("Rename failed due to I/O error: " + ex2.Message);
			return false;
		}
		catch (Exception ex3)
		{
			Debug.LogError("Rename failed due to unexpected error: " + ex3.Message);
			return false;
		}
	}

	private static void ListAllOutfits()
	{
		Outfit[] array = LoadAllOutfits();
		for (int i = 0; i < array.Length; i++)
		{
			Debug.Log(array[i].name);
		}
	}

	private static ES3Settings GetOutfitES3Setting(string outfitName, ES3.CompressionType compressionType = ES3.CompressionType.None)
	{
		string text = Path.Combine($"Profile{Singleton<ProfileController>.Instance.CurrentProfile}", "Outfits");
		if (!ES3.DirectoryExists(text))
		{
			Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, text));
		}
		string text2 = Path.Combine(text, outfitName + ".es3");
		string text3 = Path.Combine(Application.persistentDataPath, text2);
		if (new FileInfo(text3).Exists && string.IsNullOrWhiteSpace(File.ReadAllText(text3)))
		{
			Debug.Log("File Empty! Deleting");
			File.Delete(text3);
		}
		return new ES3Settings
		{
			path = text2,
			compressionType = compressionType
		};
	}
}
