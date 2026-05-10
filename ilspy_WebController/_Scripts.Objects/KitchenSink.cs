using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;
using _Scripts.Singletons;

namespace _Scripts.Objects;

public class KitchenSink : MonoBehaviour, IInitializable<KitchenSinkSaveData>, IHasSaveData<KitchenSinkSaveData>
{
	[SerializeField]
	private float minWaterLevel;

	[SerializeField]
	private float maxWaterLevel;

	[SerializeField]
	private float maxWaterFlow;

	[SerializeField]
	private float waterDrainAmount;

	[SerializeField]
	private float waterFallMaxHeight;

	[Header("References")]
	[SerializeField]
	private Rigidbody knob;

	[SerializeField]
	private Transform waterVisuals;

	[SerializeField]
	private BoxCollider waterTrigger;

	[SerializeField]
	private Transform waterFall;

	[SerializeField]
	private ParticleSystem waterfallParticles;

	[SerializeField]
	private WaterOrbSpawner waterOrbSpawnerKitchen;

	[SerializeField]
	private WaterKitchen waterKitchen;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onStartEvent;

	[SerializeField]
	private UnityEvent onDrainPlugEvent;

	[SerializeField]
	private UnityEvent onDrainUnplugEvent;

	[SerializeField]
	private UnityEvent onWaterFallStartEvent;

	[SerializeField]
	private UnityEvent onWaterFallStopEvent;

	[SerializeField]
	private UnityEvent onOverflowHoleCloseEvent;

	[SerializeField]
	private UnityEvent onOverflowHoleOpenEvent;

	private float knobMaxAngle;

	private float waterLevel;

	private bool drainIsOpen;

	private bool overflowHoleIsOpen;

	private Vector3 waterTriggerSize;

	private Vector3 waterTriggerCenter;

	private Vector3 waterFallScale;

	private Vector3 waterFallParticlesPosition;

	public float WaterLevel => waterLevel;

	private void Awake()
	{
		waterLevel = 0f;
		HingeJoint component = knob.GetComponent<HingeJoint>();
		knobMaxAngle = component.limits.max;
		knob.transform.localRotation = Quaternion.Euler(0f, 0f - knobMaxAngle, 0f);
		waterTriggerSize = waterTrigger.size;
		waterTriggerCenter = waterTrigger.center;
		waterFallScale = waterFall.localScale;
		waterFallParticlesPosition = waterfallParticles.transform.localPosition;
		waterFall.gameObject.SetActive(value: false);
		waterfallParticles.Stop();
		drainIsOpen = true;
		overflowHoleIsOpen = true;
	}

	private void Start()
	{
		onStartEvent?.Invoke();
	}

	private void Update()
	{
		float knobValue = GetKnobValue(knob, knobMaxAngle);
		float num = maxWaterFlow * knobValue - (drainIsOpen ? waterDrainAmount : 0f);
		waterLevel += num * Time.deltaTime;
		if (waterKitchen != null)
		{
			if (waterLevel >= maxWaterLevel && !waterKitchen.IsFillingUp && !overflowHoleIsOpen && num > 0f)
			{
				waterKitchen.StartFillingUp();
			}
			else if (waterLevel <= maxWaterLevel && waterKitchen.IsFillingUp)
			{
				waterKitchen.StopFillingUp();
			}
			if (overflowHoleIsOpen)
			{
				waterKitchen.StopFillingUp();
			}
		}
		waterLevel = Mathf.Clamp(waterLevel, minWaterLevel, maxWaterLevel);
		waterVisuals.localPosition = Vector3.up * waterLevel;
		waterTriggerSize.y = waterLevel;
		waterTrigger.size = waterTriggerSize;
		waterTriggerCenter.y = waterLevel / 2f;
		waterTrigger.center = waterTriggerCenter;
		bool active = waterLevel > 0.1f;
		waterVisuals.gameObject.SetActive(active);
		waterTrigger.gameObject.SetActive(active);
		bool flag = knobValue > 0.01f;
		waterFall.gameObject.SetActive(flag);
		if (waterfallParticles.isEmitting && !flag)
		{
			waterfallParticles.Stop();
			onWaterFallStopEvent?.Invoke();
		}
		else if (!waterfallParticles.isEmitting && flag)
		{
			waterfallParticles.Play();
			onWaterFallStartEvent?.Invoke();
		}
		waterFallScale.y = (waterFallMaxHeight - waterLevel) / waterFallMaxHeight;
		waterFall.localScale = waterFallScale;
		waterFallParticlesPosition.y = waterLevel - waterFallMaxHeight;
		waterfallParticles.transform.localPosition = waterFallParticlesPosition;
		if (flag)
		{
			waterOrbSpawnerKitchen.TrySpawnWaterOrb();
		}
	}

	private float GetKnobValue(Rigidbody knob, float maxAngle)
	{
		float num = knob.transform.localRotation.eulerAngles.y;
		if (num > 180f)
		{
			num -= 360f;
		}
		return (num / maxAngle + 1f) / 2f;
	}

	private string GetCurrentWaterOrbId()
	{
		WaterOrb currentWaterOrb = waterOrbSpawnerKitchen.CurrentWaterOrb;
		if (currentWaterOrb == null)
		{
			return "";
		}
		UniqueID component = currentWaterOrb.GetComponent<UniqueID>();
		if (component == null)
		{
			return "";
		}
		return component.ID;
	}

	public void Initialize(KitchenSinkSaveData saveData)
	{
		LevelSavingController.TryGetUniqueGameObjectById(saveData.currentWaterOrbId, out var uniqueGameObject);
		WaterOrb currentWaterOrb = ((uniqueGameObject == null) ? null : uniqueGameObject.GetComponent<WaterOrb>());
		waterLevel = saveData.waterLevel;
		waterOrbSpawnerKitchen.SetCurrentWaterOrb(currentWaterOrb);
	}

	public KitchenSinkSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Kitchen Sink " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		KitchenSinkSaveData result = default(KitchenSinkSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.currentWaterOrbId = GetCurrentWaterOrbId();
		result.waterLevel = waterLevel;
		return result;
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

	public void CloseOverflowHole()
	{
		overflowHoleIsOpen = false;
		onOverflowHoleCloseEvent?.Invoke();
	}

	public void OpenOverflowHole()
	{
		overflowHoleIsOpen = true;
		onOverflowHoleOpenEvent?.Invoke();
	}
}
