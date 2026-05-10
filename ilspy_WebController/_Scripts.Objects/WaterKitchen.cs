using System.Collections.Generic;
using StylizedWater;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using _Scripts.Achievements;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

public class WaterKitchen : MonoBehaviour, IInitializable<WaterKitchenSaveData>, IHasSaveData<WaterKitchenSaveData>
{
	[Range(0f, 1f)]
	[SerializeField]
	private float startingWaterLevel;

	[SerializeField]
	private float minWaterLevel;

	[SerializeField]
	private float maxWaterLevel;

	[SerializeField]
	private float waterVisualsOffset;

	[SerializeField]
	private float maxWaterFlow;

	[SerializeField]
	private float waterDrainAmount;

	[SerializeField]
	private float waterFallMaxHeight;

	[Header("Quests")]
	[SerializeField]
	private float waterLevelCleanFloor = 1f;

	[SerializeField]
	private float waterLevelWaterPlants = 2f;

	[Header("References")]
	[SerializeField]
	private Transform waterVisuals;

	[SerializeField]
	private BoxCollider waterTrigger;

	[SerializeField]
	private Transform waterFall;

	[SerializeField]
	private ParticleSystem waterfallParticles;

	public UniversalRendererData rendererData;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onStartEvent;

	[SerializeField]
	private UnityEvent onDrainPlugEvent;

	[SerializeField]
	private UnityEvent onDrainUnplugEvent;

	private bool drainIsOpen;

	private bool isFillingUp;

	private bool isFull;

	private bool isEmpty = true;

	private float waterLevel;

	private Vector3 waterFallDefaultScale;

	private Vector3 waterTriggerSize;

	private Vector3 waterTriggerCenter;

	private Vector3 waterFallScale;

	private Vector3 waterFallParticlesPosition;

	private CausticsFeature causticsFeature;

	private bool floorIsCleaned;

	private bool plantsAreWatered;

	private Splat[] splats;

	private List<KitchenPlant> kitchenPlants;

	private bool ignoreMaxWaterLevel;

	public bool IsFillingUp => isFillingUp;

	public float WaterLevel => waterLevel;

	private void Awake()
	{
		waterLevel = Mathf.Lerp(minWaterLevel, maxWaterLevel, startingWaterLevel);
		waterTriggerSize = waterTrigger.size;
		waterTriggerCenter = waterTrigger.center;
		waterVisuals.gameObject.SetActive(value: false);
		waterTrigger.gameObject.SetActive(value: false);
		waterFallScale = waterFall.localScale;
		waterFallDefaultScale = waterFallScale;
		waterFallParticlesPosition = waterfallParticles.transform.localPosition;
		waterFall.gameObject.SetActive(value: false);
		waterfallParticles.Stop();
		foreach (ScriptableRendererFeature rendererFeature in rendererData.rendererFeatures)
		{
			if (rendererFeature is CausticsFeature causticsFeature)
			{
				this.causticsFeature = causticsFeature;
				this.causticsFeature.SetActive(active: false);
			}
		}
	}

	private void Start()
	{
		splats = Object.FindObjectsByType<Splat>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		kitchenPlants = new List<KitchenPlant>();
		KitchenPlant[] array = Object.FindObjectsByType<KitchenPlant>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (KitchenPlant kitchenPlant in array)
		{
			if (kitchenPlant.StandingOnFloor)
			{
				kitchenPlants.Add(kitchenPlant);
			}
		}
		onStartEvent?.Invoke();
	}

	private void Update()
	{
		if (isEmpty && drainIsOpen)
		{
			return;
		}
		float num = ((!isFull && isFillingUp) ? maxWaterFlow : 0f) - (drainIsOpen ? waterDrainAmount : 0f);
		waterLevel += num * Time.deltaTime;
		if (!ignoreMaxWaterLevel)
		{
			waterLevel = Mathf.Clamp(waterLevel, minWaterLevel, maxWaterLevel);
		}
		else
		{
			waterVisuals.localPosition = Vector3.up * (waterLevel + waterVisualsOffset);
			waterTriggerSize.y = waterLevel;
			waterTrigger.size = waterTriggerSize;
			waterTriggerCenter.y = waterLevel / 2f;
			waterTrigger.center = waterTriggerCenter;
		}
		if (isFull)
		{
			if (waterLevel < maxWaterLevel)
			{
				isFull = false;
			}
			return;
		}
		UpdateWaterVisuals();
		if (!floorIsCleaned && waterLevel > waterLevelCleanFloor)
		{
			floorIsCleaned = true;
			Splat[] array = splats;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].CleanByWaterKitchen();
			}
		}
		if (plantsAreWatered || !(waterLevel > waterLevelWaterPlants))
		{
			return;
		}
		plantsAreWatered = true;
		foreach (KitchenPlant kitchenPlant in kitchenPlants)
		{
			kitchenPlant.WaterPlant(wateredByKitchenWater: true);
		}
	}

	private void UpdateWaterVisuals()
	{
		if (waterLevel >= maxWaterLevel)
		{
			isFull = true;
			waterFall.gameObject.SetActive(value: false);
			waterfallParticles.Stop();
			AchievementEvents.KitchenFullyFlooded();
		}
		else if (waterLevel <= minWaterLevel)
		{
			isEmpty = true;
		}
		else if (waterLevel > minWaterLevel)
		{
			isEmpty = false;
		}
		waterVisuals.localPosition = Vector3.up * (waterLevel + waterVisualsOffset);
		waterTriggerSize.y = waterLevel;
		waterTrigger.size = waterTriggerSize;
		waterTriggerCenter.y = waterLevel / 2f;
		waterTrigger.center = waterTriggerCenter;
		bool active = waterLevel > 0.1f;
		waterVisuals.gameObject.SetActive(active);
		waterTrigger.gameObject.SetActive(active);
		waterFallScale.y = (waterFallMaxHeight - waterLevel) / waterFallMaxHeight * waterFallDefaultScale.y;
		waterFall.localScale = waterFallScale;
		waterFallParticlesPosition.y = waterLevel;
		waterfallParticles.transform.localPosition = waterFallParticlesPosition;
	}

	public void Initialize(WaterKitchenSaveData waterKitchenSaveData)
	{
		waterLevel = waterKitchenSaveData.waterLevel;
		drainIsOpen = waterKitchenSaveData.drainIsOpen;
		isFillingUp = waterKitchenSaveData.isFillingUp;
		isFull = waterKitchenSaveData.isFull;
		isEmpty = waterKitchenSaveData.isEmpty;
		UpdateWaterVisuals();
	}

	public WaterKitchenSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Water Kitchen " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		WaterKitchenSaveData result = default(WaterKitchenSaveData);
		result.id = id;
		result.waterLevel = waterLevel;
		result.drainIsOpen = drainIsOpen;
		result.isFillingUp = isFillingUp;
		result.isFull = isFull;
		result.isEmpty = isEmpty;
		return result;
	}

	public void SetWaterLevel(float value)
	{
		waterLevel = value;
		ignoreMaxWaterLevel = true;
	}

	public void StartFillingUp()
	{
		if (!isFull)
		{
			isFillingUp = true;
			waterFall.gameObject.SetActive(isFillingUp);
			waterfallParticles.Play();
		}
	}

	public void StopFillingUp()
	{
		isFillingUp = false;
		waterFall.gameObject.SetActive(isFillingUp);
		waterfallParticles.Stop();
	}

	public void CloseDrain()
	{
		drainIsOpen = false;
		onDrainPlugEvent?.Invoke();
	}

	public void OpenDrain()
	{
		drainIsOpen = true;
		onDrainUnplugEvent?.Invoke();
	}
}
