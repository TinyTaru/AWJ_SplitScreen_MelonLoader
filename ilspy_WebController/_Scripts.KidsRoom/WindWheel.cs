using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.CosmeticItems;
using _Scripts.Singletons;

namespace _Scripts.KidsRoom;

public class WindWheel : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform wheel;

	[SerializeField]
	private CosmeticItemSo cosmeticItemSo;

	[Header("Parameters")]
	[SerializeField]
	private int rotationThreshold = 10;

	[Header("Sounds")]
	[SerializeField]
	private EventReference increaseRotationCounterSound;

	[SerializeField]
	private EventReference decreaseRotationCounterSound;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent rotationFinishedEvent;

	[SerializeField]
	private UnityEvent rotationThresholdReachedEvent;

	private bool rotationThresholdReached;

	private float accumulatedDegrees;

	private float angleOld;

	private int fullRotations;

	private int fullRotationsOld;

	private void Start()
	{
		rotationThresholdReached = Singleton<CosmeticItemsController>.Instance.IsItemUnlocked(cosmeticItemSo);
		angleOld = base.transform.localEulerAngles.z;
	}

	private void Update()
	{
		if (rotationThresholdReached)
		{
			return;
		}
		float z = wheel.localEulerAngles.z;
		float num = Mathf.DeltaAngle(angleOld, z);
		accumulatedDegrees += num;
		fullRotations = Mathf.FloorToInt(accumulatedDegrees / 360f);
		if (fullRotationsOld > 0)
		{
			if (fullRotations > fullRotationsOld)
			{
				Singleton<MusicController>.Instance.PlaySound(increaseRotationCounterSound);
				rotationFinishedEvent?.Invoke();
			}
			else if (fullRotations < fullRotationsOld)
			{
				Singleton<MusicController>.Instance.PlaySound(decreaseRotationCounterSound);
			}
		}
		else if (fullRotationsOld < 0)
		{
			if (fullRotations < fullRotationsOld)
			{
				Singleton<MusicController>.Instance.PlaySound(increaseRotationCounterSound);
				rotationFinishedEvent?.Invoke();
			}
			else if (fullRotations > fullRotationsOld)
			{
				Singleton<MusicController>.Instance.PlaySound(decreaseRotationCounterSound);
			}
		}
		else if (fullRotations != fullRotationsOld)
		{
			Singleton<MusicController>.Instance.PlaySound(increaseRotationCounterSound);
			rotationFinishedEvent?.Invoke();
		}
		fullRotationsOld = fullRotations;
		if (Mathf.Abs(fullRotations) >= rotationThreshold)
		{
			rotationThresholdReached = true;
			rotationThresholdReachedEvent?.Invoke();
		}
		angleOld = z;
	}
}
