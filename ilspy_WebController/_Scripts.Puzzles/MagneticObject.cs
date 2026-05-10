using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.Puzzles;

[RequireComponent(typeof(MovableObject))]
[DisallowMultipleComponent]
public class MagneticObject : MonoBehaviour, IInitializable<MagneticObjectSaveData>, IHasSaveData<MagneticObjectSaveData>
{
	public enum MagneticObjectState
	{
		Disabled,
		Free,
		Locking,
		Locked,
		Cooldown
	}

	[Header("References")]
	[SerializeField]
	private List<Transform> magneticAnchors;

	[Header("Parameters")]
	[SerializeField]
	private MagneticObjectType objectType;

	[SerializeField]
	private MagneticObjectState initialMagneticObjectState = MagneticObjectState.Free;

	[SerializeField]
	private float cooldownDuration = 1f;

	[SerializeField]
	private float lockingMaxDuration = 2f;

	[SerializeField]
	private bool releaseMainWebOnLocking;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onLockingProcessStartedEvent;

	[SerializeField]
	private UnityEvent OnMagenticLockActivated;

	[SerializeField]
	private UnityEvent OnMagenticLockDeactivated;

	private Rigidbody rb;

	private MagneticLock connectedMagneticLock;

	private float cooldownTimer;

	private float lockingMaxTimer;

	private MagneticObjectState magneticObjectState;

	private new bool enabled;

	public List<Transform> MagneticAnchors => magneticAnchors;

	public bool IsLocked => connectedMagneticLock != null;

	public MagneticObjectState GetMagneticObjectState => magneticObjectState;

	public MagneticObjectType ObjectType => objectType;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		magneticObjectState = initialMagneticObjectState;
	}

	private void Start()
	{
		enabled = true;
		if (objectType == MagneticObjectType.None)
		{
			return;
		}
		MagneticLock[] array = UnityEngine.Object.FindObjectsByType<MagneticLock>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		foreach (MagneticLock magneticLock in array)
		{
			if (magneticLock.ObjectType == objectType)
			{
				magneticLock.AutoAssignMagneticObject(this);
			}
		}
	}

	private void Update()
	{
		switch (magneticObjectState)
		{
		case MagneticObjectState.Locking:
			lockingMaxTimer -= Time.deltaTime;
			if (lockingMaxTimer <= 0f)
			{
				rb.isKinematic = false;
				magneticObjectState = (enabled ? MagneticObjectState.Free : MagneticObjectState.Disabled);
			}
			break;
		case MagneticObjectState.Cooldown:
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer <= 0f)
			{
				magneticObjectState = MagneticObjectState.Free;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MagneticObjectState.Disabled:
		case MagneticObjectState.Free:
		case MagneticObjectState.Locked:
			break;
		}
	}

	public void Initialize(MagneticObjectSaveData saveData)
	{
		magneticObjectState = saveData.magneticObjectState;
	}

	public MagneticObjectSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Magnetic Object " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		MagneticObjectSaveData result = default(MagneticObjectSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.magneticObjectState = magneticObjectState;
		return result;
	}

	public Rigidbody GetRigidbody()
	{
		return rb;
	}

	public void EnableMagneticObject()
	{
		enabled = true;
		magneticObjectState = MagneticObjectState.Free;
	}

	public void DisableMagneticObject()
	{
		enabled = false;
		switch (magneticObjectState)
		{
		case MagneticObjectState.Free:
			magneticObjectState = MagneticObjectState.Disabled;
			break;
		case MagneticObjectState.Locked:
			DeactivateMagneticLock();
			break;
		case MagneticObjectState.Cooldown:
			magneticObjectState = MagneticObjectState.Disabled;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MagneticObjectState.Disabled:
		case MagneticObjectState.Locking:
			break;
		}
	}

	public void StartLocking()
	{
		if (magneticObjectState != MagneticObjectState.Locking)
		{
			if (releaseMainWebOnLocking)
			{
				Singleton<WebController>.Instance.ReleaseWeb(playAnimation: false);
			}
			lockingMaxTimer = lockingMaxDuration;
			magneticObjectState = MagneticObjectState.Locking;
			onLockingProcessStartedEvent?.Invoke();
		}
	}

	public void LockImmediately()
	{
		magneticObjectState = MagneticObjectState.Locked;
	}

	public void ActivateMagneticLock(MagneticLock magneticLock)
	{
		magneticObjectState = (enabled ? MagneticObjectState.Locked : MagneticObjectState.Disabled);
		connectedMagneticLock = magneticLock;
		OnMagenticLockActivated?.Invoke();
	}

	public void DeactivateMagneticLock()
	{
		magneticObjectState = (enabled ? MagneticObjectState.Cooldown : MagneticObjectState.Disabled);
		cooldownTimer = cooldownDuration;
		connectedMagneticLock = null;
		OnMagenticLockDeactivated?.Invoke();
	}
}
