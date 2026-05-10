using UnityEngine;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.Miscellaneous.Christmas;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous;

public class BackgroundController : Singleton<BackgroundController>, IInitializable<BackgroundControllerSaveData>, IHasSaveData<BackgroundControllerSaveData>
{
	[Header("Default Background")]
	[SerializeField]
	private Material skyboxMaterialDefault;

	[SerializeField]
	private float fogDensityDefault;

	[SerializeField]
	private Color fogColorDefault;

	[SerializeField]
	private GameObject backgroundDefault;

	[Header("Halloween Background")]
	[SerializeField]
	private Material skyboxMaterialHalloween;

	[SerializeField]
	private float fogDensityHalloween;

	[SerializeField]
	private Color fogColorHalloween;

	[SerializeField]
	private GameObject backgroundHalloween;

	[Header("Christmas Background")]
	[SerializeField]
	private Material skyboxMaterialChristmas;

	[SerializeField]
	private float fogDensityChristmas;

	[SerializeField]
	private Color fogColorChristmas;

	[SerializeField]
	private GameObject backgroundChristmas;

	private BackgroundType backgroundType;

	public BackgroundType GetBackgroundType => backgroundType;

	private void Start()
	{
		ActivateBackgroundDefault();
	}

	private void SetBackgroundDefaultActive(bool value)
	{
		if (value)
		{
			RenderSettings.skybox = Singleton<BackgroundController>.Instance.skyboxMaterialDefault;
			RenderSettings.fogDensity = Singleton<BackgroundController>.Instance.fogDensityDefault;
			RenderSettings.fogColor = Singleton<BackgroundController>.Instance.fogColorDefault;
			if (Singleton<BackgroundController>.Instance.backgroundDefault != null)
			{
				Singleton<BackgroundController>.Instance.backgroundDefault.SetActive(value: true);
			}
		}
		else
		{
			DisableObjectSafely(Singleton<BackgroundController>.Instance.backgroundDefault);
		}
	}

	private void SetBackgroundHalloweenActive(bool value)
	{
		if (value)
		{
			RenderSettings.skybox = Singleton<BackgroundController>.Instance.skyboxMaterialHalloween;
			RenderSettings.fogDensity = Singleton<BackgroundController>.Instance.fogDensityHalloween;
			RenderSettings.fogColor = Singleton<BackgroundController>.Instance.fogColorHalloween;
			if (Singleton<BackgroundController>.Instance.backgroundHalloween != null)
			{
				Singleton<BackgroundController>.Instance.backgroundHalloween.SetActive(value: true);
			}
		}
		else
		{
			DisableObjectSafely(Singleton<BackgroundController>.Instance.backgroundHalloween);
		}
	}

	private void SetBackgroundChristmasActive(bool value)
	{
		if (value)
		{
			RenderSettings.skybox = Singleton<BackgroundController>.Instance.skyboxMaterialChristmas;
			RenderSettings.fogDensity = Singleton<BackgroundController>.Instance.fogDensityChristmas;
			RenderSettings.fogColor = Singleton<BackgroundController>.Instance.fogColorChristmas;
			if (Singleton<BackgroundController>.Instance.backgroundChristmas != null)
			{
				Singleton<BackgroundController>.Instance.backgroundChristmas.SetActive(value: true);
			}
		}
		else
		{
			DisableObjectSafely(Singleton<BackgroundController>.Instance.backgroundChristmas);
		}
	}

	public void DisableObjectSafely(GameObject gameObjectToDisable)
	{
		if (!(gameObjectToDisable == null))
		{
			DontDestroyMe[] componentsInChildren = gameObjectToDisable.GetComponentsInChildren<DontDestroyMe>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ResetParent();
			}
			gameObjectToDisable.SetActive(value: false);
		}
	}

	public void Initialize(BackgroundControllerSaveData saveData)
	{
		switch (saveData.backgroundType)
		{
		case BackgroundType.Default:
			ActivateBackgroundDefault();
			break;
		case BackgroundType.Halloween:
			ActivateBackgroundHalloween();
			break;
		case BackgroundType.Christmas:
			ActivateBackgroundChristmas();
			break;
		}
	}

	public BackgroundControllerSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Background Controller " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		BackgroundControllerSaveData result = default(BackgroundControllerSaveData);
		result.id = id;
		result.backgroundType = backgroundType;
		return result;
	}

	public void ActivateBackgroundDefault()
	{
		if (Singleton<BackgroundController>.Instance.backgroundType != 0 && !(Singleton<KitchenChristmasController>.Instance == null) && !Singleton<KitchenChristmasController>.Instance.ChristmasEventIsActive)
		{
			SetBackgroundDefaultActive(value: true);
			SetBackgroundHalloweenActive(value: false);
			SetBackgroundChristmasActive(value: false);
			Singleton<BackgroundController>.Instance.backgroundType = BackgroundType.Default;
		}
	}

	public void ActivateBackgroundHalloween()
	{
		if (Singleton<BackgroundController>.Instance.backgroundType != BackgroundType.Halloween && !(Singleton<KitchenChristmasController>.Instance == null) && !Singleton<KitchenChristmasController>.Instance.ChristmasEventIsActive)
		{
			SetBackgroundDefaultActive(value: false);
			SetBackgroundHalloweenActive(value: true);
			SetBackgroundChristmasActive(value: false);
			Singleton<BackgroundController>.Instance.backgroundType = BackgroundType.Halloween;
		}
	}

	public void ActivateBackgroundChristmas()
	{
		if (Singleton<BackgroundController>.Instance.backgroundType != BackgroundType.Christmas)
		{
			SetBackgroundDefaultActive(value: false);
			SetBackgroundHalloweenActive(value: false);
			SetBackgroundChristmasActive(value: true);
			Singleton<BackgroundController>.Instance.backgroundType = BackgroundType.Christmas;
		}
	}
}
