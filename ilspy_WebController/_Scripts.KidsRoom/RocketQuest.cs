using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;
using _Scripts.Objects;

namespace _Scripts.KidsRoom;

public class RocketQuest : MonoBehaviour, IInitializable<RocketQuestSaveData>, IHasSaveData<RocketQuestSaveData>
{
	[Header("References")]
	[SerializeField]
	private Rocket rocket;

	[SerializeField]
	private MagneticLock magneticLockRocket;

	[SerializeField]
	private Transform arrowPositionMin;

	[SerializeField]
	private Transform arrowPositionMax;

	[SerializeField]
	private Transform pumpPowerArrow;

	[SerializeField]
	private Transform launchPowerArrow;

	[Header("Parameters")]
	[SerializeField]
	private float maxPower = 2000f;

	[SerializeField]
	private float launchPowerThreshold = 1000f;

	[SerializeField]
	private float minThrustDuration = 15f;

	[SerializeField]
	private float maxThrustDuration = 45f;

	[SerializeField]
	private float minPowerToTriggerSound = 100f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter pressureBuildUpSound;

	[SerializeField]
	private StudioEventEmitter pressureMaxSound;

	[SerializeField]
	private StudioEventEmitter rocketReadyLoop;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onRocketReadyToLaunchEvent;

	[SerializeField]
	private UnityEvent onRocketLaunchEvent;

	private bool rocketIsLocked;

	private bool readyForLaunch;

	private float pumpPower;

	private float launchPower;

	private void Start()
	{
		rocketIsLocked = false;
		readyForLaunch = false;
		pumpPowerArrow.localPosition = arrowPositionMin.localPosition;
	}

	private void Update()
	{
		Vector3 localPosition = launchPowerArrow.localPosition;
		localPosition.y = Mathf.Max(localPosition.y, pumpPowerArrow.localPosition.y);
		launchPowerArrow.localPosition = localPosition;
	}

	private void RocketReadyToLaunch()
	{
		onRocketReadyToLaunchEvent?.Invoke();
		rocketReadyLoop.Play();
	}

	public void Initialize(RocketQuestSaveData rocketQuestSaveData)
	{
		rocketIsLocked = rocketQuestSaveData.rocketIsLocked;
		readyForLaunch = rocketQuestSaveData.readyForLaunch;
		launchPower = rocketQuestSaveData.launchPower;
		if (rocketIsLocked && readyForLaunch)
		{
			RocketReadyToLaunch();
		}
		Vector3 localPosition = launchPowerArrow.localPosition;
		float t = Mathf.Clamp01(launchPower / maxPower);
		localPosition.y = Mathf.Max(Vector3.Lerp(arrowPositionMin.localPosition, arrowPositionMax.localPosition, t).y, pumpPowerArrow.localPosition.y);
		launchPowerArrow.localPosition = localPosition;
	}

	public RocketQuestSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Rocket Quest " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		RocketQuestSaveData result = default(RocketQuestSaveData);
		result.id = id;
		result.rocketIsLocked = rocketIsLocked;
		result.readyForLaunch = readyForLaunch;
		result.launchPower = launchPower;
		return result;
	}

	public void AddPower(float value)
	{
		pumpPower = value;
		launchPower = Mathf.Max(launchPower, pumpPower);
		float t = Mathf.Clamp01(pumpPower / maxPower);
		Vector3 endValue = Vector3.Lerp(arrowPositionMin.localPosition, arrowPositionMax.localPosition, t);
		pumpPowerArrow.DOKill();
		pumpPowerArrow.DOLocalMove(endValue, 1f).OnComplete(delegate
		{
			pumpPowerArrow.DOLocalMove(arrowPositionMin.localPosition, 1f);
			if (t >= 0.999f)
			{
				pressureMaxSound.Play();
			}
		});
		if (!readyForLaunch && launchPower > launchPowerThreshold)
		{
			readyForLaunch = true;
			if (rocketIsLocked)
			{
				RocketReadyToLaunch();
			}
		}
		if (pumpPower > minPowerToTriggerSound)
		{
			float value2 = Mathf.Clamp(10f * pumpPower / maxPower, 0f, 9f);
			pressureBuildUpSound.Play();
			pressureBuildUpSound.SetParameter("intensity", value2);
		}
	}

	private void PositionRocket()
	{
		rocket.GetComponent<Rigidbody>().position = magneticLockRocket.transform.position;
		rocket.GetComponent<Rigidbody>().rotation = Quaternion.identity;
	}

	public void LockRocket()
	{
		rocketIsLocked = true;
		if (readyForLaunch)
		{
			RocketReadyToLaunch();
		}
	}

	public void EnableMagneticLock()
	{
		magneticLockRocket.EnableMagneticLock();
	}

	public void LaunchRocket()
	{
		if (rocketIsLocked && readyForLaunch)
		{
			rocketReadyLoop.Stop();
			magneticLockRocket.DisableMagneticLock();
			float t = Mathf.InverseLerp(launchPowerThreshold, maxPower, launchPower);
			float duration = Mathf.Lerp(minThrustDuration, maxThrustDuration, t);
			rocket.LaunchRocket(duration);
			rocketIsLocked = false;
			readyForLaunch = false;
			pumpPower = 0f;
			launchPower = 0f;
			Vector3 localPosition = launchPowerArrow.localPosition;
			localPosition.y = arrowPositionMin.localPosition.y;
			launchPowerArrow.DOLocalMove(localPosition, 1f);
			onRocketLaunchEvent?.Invoke();
		}
	}
}
