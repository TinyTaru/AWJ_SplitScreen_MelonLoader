using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.Puzzles;
using _Scripts.Singletons;

namespace _Scripts.Objects;

[DisallowMultipleComponent]
public class MagneticLock : MonoBehaviour, IInitializable<MagneticLockSaveData>, IHasSaveData<MagneticLockSaveData>
{
	public enum MagneticLockState
	{
		Disabled,
		Free,
		Locking,
		Locked,
		Cooldown
	}

	[Header("References")]
	[SerializeField]
	private Rigidbody rb;

	[FormerlySerializedAs("objectsToAutomaticallyAssign")]
	[SerializeField]
	private MagneticObjectType objectType;

	[SerializeField]
	private List<MagneticObject> lockableObjects;

	[Header("Parameters")]
	[FormerlySerializedAs("initialLockState")]
	[SerializeField]
	private MagneticLockState initialMagneticLockState = MagneticLockState.Free;

	[SerializeField]
	private float distanceThreshold;

	[SerializeField]
	private float angleThreshold;

	[SerializeField]
	private float breakForce = float.PositiveInfinity;

	[SerializeField]
	private float breakTorque = float.PositiveInfinity;

	[SerializeField]
	private float cooldownDuration = 1f;

	[SerializeField]
	private float lockDuration = 0.2f;

	[SerializeField]
	private bool enableCollision;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onMagneticLockStartLocking;

	[SerializeField]
	private UnityEvent onMagneticLockActivated;

	[SerializeField]
	private UnityEvent onMagneticLockDeactivated;

	private float cooldownTimer;

	private float lockTimer;

	private MagneticObject connectedObject;

	private Rigidbody connectedRigidBody;

	private List<MagneticObject> objectsInRange;

	private MagneticLockState magneticLockState;

	private bool magneticLockEnabled;

	private bool isKinematic;

	private Vector3 lockedPosition;

	private Quaternion lockedRotation;

	private FixedJoint fixedJoint;

	private int magneticAnchorIndex;

	public MagneticObjectType ObjectType => objectType;

	public MagneticObject ConnectedObject => connectedObject;

	public float BreakForce => breakForce;

	public float BreakTorque => breakTorque;

	private void Awake()
	{
		objectsInRange = new List<MagneticObject>();
		isKinematic = rb.isKinematic;
		if (objectType != 0)
		{
			lockableObjects = new List<MagneticObject>();
		}
		magneticLockState = initialMagneticLockState;
		magneticLockEnabled = magneticLockState != MagneticLockState.Disabled;
	}

	private void Start()
	{
		if (objectType != 0)
		{
			MagneticObject[] array = UnityEngine.Object.FindObjectsByType<MagneticObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			foreach (MagneticObject magneticObject in array)
			{
				if (magneticObject.ObjectType == objectType)
				{
					AutoAssignMagneticObject(magneticObject);
				}
			}
		}
		if (magneticLockState == MagneticLockState.Locked && lockableObjects.Count == 1)
		{
			ManualLock(lockableObjects[0]);
		}
	}

	private void FixedUpdate()
	{
		switch (magneticLockState)
		{
		case MagneticLockState.Free:
		{
			foreach (MagneticObject item in objectsInRange)
			{
				if (item.GetMagneticObjectState != MagneticObject.MagneticObjectState.Free)
				{
					continue;
				}
				if (item == null)
				{
					objectsInRange.Remove(item);
					continue;
				}
				for (int i = 0; i < item.MagneticAnchors.Count; i++)
				{
					Transform transform = item.MagneticAnchors[i];
					if (transform == null)
					{
						objectsInRange.Remove(item);
						continue;
					}
					float num = Vector3.Distance(base.transform.position, transform.transform.position);
					float num2 = Quaternion.Angle(base.transform.rotation, transform.rotation);
					if (num < distanceThreshold && num2 < angleThreshold)
					{
						connectedObject = item;
						connectedObject.StartLocking();
						connectedRigidBody = connectedObject.GetRigidbody();
						rb.isKinematic = true;
						connectedRigidBody.isKinematic = true;
						magneticAnchorIndex = i;
						CalculateLockedPosition(item, transform);
						connectedRigidBody.DOMove(lockedPosition, lockDuration);
						CalculateLockedRotation(transform);
						connectedRigidBody.DORotate(lockedRotation.eulerAngles, lockDuration);
						lockTimer = lockDuration;
						magneticLockState = MagneticLockState.Locking;
						onMagneticLockStartLocking?.Invoke();
						break;
					}
				}
			}
			break;
		}
		case MagneticLockState.Locking:
			lockTimer -= Time.fixedDeltaTime;
			if (lockTimer <= 0f)
			{
				ActivateMagneticLock();
			}
			break;
		case MagneticLockState.Cooldown:
			cooldownTimer -= Time.fixedDeltaTime;
			if (cooldownTimer <= 0f)
			{
				magneticLockState = MagneticLockState.Free;
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MagneticLockState.Disabled:
		case MagneticLockState.Locked:
			break;
		}
	}

	public void OnJointBreak(float breakForce)
	{
		DeactivateMagneticLock();
	}

	private void OnTriggerEnter(Collider other)
	{
		MagneticObject componentInParent = other.GetComponentInParent<MagneticObject>();
		if (!(componentInParent == null) && lockableObjects.Contains(componentInParent))
		{
			objectsInRange.Add(componentInParent);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		MagneticObject componentInParent = other.GetComponentInParent<MagneticObject>();
		if (!(componentInParent == null) && objectsInRange.Contains(componentInParent))
		{
			objectsInRange.Remove(componentInParent);
		}
	}

	private IEnumerator ActivateMagneticLockCoroutine()
	{
		connectedRigidBody.position = lockedPosition;
		connectedRigidBody.rotation = lockedRotation;
		connectedObject.ActivateMagneticLock(this);
		if (magneticLockEnabled)
		{
			magneticLockState = MagneticLockState.Locked;
			onMagneticLockActivated?.Invoke();
		}
		else
		{
			DeactivateMagneticLock();
		}
		yield return null;
		fixedJoint = rb.gameObject.AddComponent<FixedJoint>();
		fixedJoint.connectedBody = connectedRigidBody;
		fixedJoint.breakForce = breakForce;
		fixedJoint.breakTorque = breakTorque;
		fixedJoint.enableCollision = enableCollision;
		connectedRigidBody.angularVelocity = Vector3.zero;
		connectedRigidBody.linearVelocity = Vector3.zero;
		connectedRigidBody.isKinematic = false;
		rb.isKinematic = isKinematic;
	}

	private void ActivateMagneticLockInstantly()
	{
		connectedRigidBody.position = lockedPosition;
		connectedRigidBody.rotation = lockedRotation;
		connectedObject.ActivateMagneticLock(this);
		if (magneticLockEnabled)
		{
			magneticLockState = MagneticLockState.Locked;
			onMagneticLockActivated?.Invoke();
		}
		else
		{
			DeactivateMagneticLock();
		}
		fixedJoint = rb.gameObject.AddComponent<FixedJoint>();
		fixedJoint.connectedBody = connectedRigidBody;
		fixedJoint.breakForce = breakForce;
		fixedJoint.breakTorque = breakTorque;
		fixedJoint.enableCollision = enableCollision;
		connectedRigidBody.angularVelocity = Vector3.zero;
		connectedRigidBody.linearVelocity = Vector3.zero;
		connectedRigidBody.isKinematic = false;
		rb.isKinematic = isKinematic;
	}

	private void CalculateLockedPosition(MagneticObject magneticObject, Transform magneticAnchor)
	{
		lockedPosition = base.transform.position - base.transform.rotation * Quaternion.Inverse(magneticAnchor.localRotation) * magneticAnchor.localPosition * magneticObject.transform.lossyScale.x;
	}

	private void CalculateLockedRotation(Transform magneticAnchor)
	{
		lockedRotation = base.transform.rotation * Quaternion.Inverse(magneticAnchor.localRotation.normalized);
	}

	private void ActivateMagneticLock()
	{
		ActivateMagneticLockInstantly();
	}

	private void DeactivateMagneticLock()
	{
		if (fixedJoint != null)
		{
			UnityEngine.Object.Destroy(fixedJoint);
		}
		connectedObject.DeactivateMagneticLock();
		magneticLockState = (magneticLockEnabled ? MagneticLockState.Cooldown : MagneticLockState.Disabled);
		cooldownTimer = cooldownDuration;
		onMagneticLockDeactivated?.Invoke();
	}

	public void Initialize(MagneticLockSaveData saveData)
	{
		magneticLockEnabled = saveData.magneticLockEnabled;
		magneticLockState = saveData.lockState;
		switch (magneticLockState)
		{
		case MagneticLockState.Disabled:
		case MagneticLockState.Free:
		case MagneticLockState.Cooldown:
			ManualDeactivate();
			break;
		case MagneticLockState.Locking:
		case MagneticLockState.Locked:
		{
			if (saveData.connectedObjectId == null)
			{
				break;
			}
			LevelSavingController.TryGetUniqueGameObjectById(saveData.connectedObjectId, out var uniqueGameObject);
			if (uniqueGameObject != null)
			{
				MagneticObject component = uniqueGameObject.GetComponent<MagneticObject>();
				if (component != null)
				{
					LockInstantly(component, saveData.magneticAnchorIndex);
				}
			}
			break;
		}
		}
	}

	public MagneticLockSaveData GetSaveData()
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
		string connectedObjectId = string.Empty;
		if (connectedObject != null)
		{
			UniqueID component2 = connectedObject.GetComponent<UniqueID>();
			if (component2 == null)
			{
				Debug.LogError("Connect Object " + connectedObject.name + " of Magnetic Lock " + base.gameObject.name + " has no uniqueID");
			}
			else
			{
				connectedObjectId = component2.ID;
			}
		}
		MagneticLockSaveData result = default(MagneticLockSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.lockState = magneticLockState;
		result.magneticLockEnabled = magneticLockEnabled;
		result.magneticAnchorIndex = magneticAnchorIndex;
		result.connectedObjectId = connectedObjectId;
		return result;
	}

	public void EnableMagneticLock()
	{
		if (magneticLockState != MagneticLockState.Locking && magneticLockState != MagneticLockState.Locked)
		{
			magneticLockState = MagneticLockState.Free;
			magneticLockEnabled = true;
		}
	}

	public void DisableMagneticLock()
	{
		magneticLockEnabled = false;
		switch (magneticLockState)
		{
		case MagneticLockState.Free:
			magneticLockState = MagneticLockState.Disabled;
			break;
		case MagneticLockState.Locked:
			DeactivateMagneticLock();
			break;
		case MagneticLockState.Cooldown:
			magneticLockState = MagneticLockState.Disabled;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MagneticLockState.Disabled:
		case MagneticLockState.Locking:
			break;
		}
	}

	public void AutoAssignMagneticObject(MagneticObject magneticObject)
	{
		if (!lockableObjects.Contains(magneticObject))
		{
			lockableObjects.Add(magneticObject);
		}
	}

	public void LockInstantly(MagneticObject magneticObject, int mewMagneticAnchorIndex = 0)
	{
		magneticAnchorIndex = mewMagneticAnchorIndex;
		connectedObject = magneticObject;
		connectedObject.LockImmediately();
		connectedRigidBody = connectedObject.GetRigidbody();
		lockTimer = lockDuration;
		connectedRigidBody.angularVelocity = Vector3.zero;
		connectedRigidBody.linearVelocity = Vector3.zero;
		rb.isKinematic = true;
		connectedRigidBody.isKinematic = true;
		Transform magneticAnchor = connectedObject.MagneticAnchors[magneticAnchorIndex];
		CalculateLockedPosition(magneticObject, magneticAnchor);
		CalculateLockedRotation(magneticAnchor);
		ActivateMagneticLockInstantly();
	}

	private void ManualLock(MagneticObject magneticObject, int mewMagneticAnchorIndex = 0)
	{
		magneticAnchorIndex = mewMagneticAnchorIndex;
		connectedObject = magneticObject;
		connectedObject.LockImmediately();
		connectedRigidBody = connectedObject.GetRigidbody();
		lockTimer = lockDuration;
		connectedRigidBody.angularVelocity = Vector3.zero;
		connectedRigidBody.linearVelocity = Vector3.zero;
		rb.isKinematic = true;
		connectedRigidBody.isKinematic = true;
		Transform magneticAnchor = connectedObject.MagneticAnchors[magneticAnchorIndex];
		CalculateLockedPosition(magneticObject, magneticAnchor);
		CalculateLockedRotation(magneticAnchor);
		StartCoroutine(ActivateMagneticLockCoroutine());
	}

	public void ManualDeactivate()
	{
		if (fixedJoint != null)
		{
			UnityEngine.Object.Destroy(fixedJoint);
		}
		cooldownTimer = cooldownDuration;
	}

	public void SetLockableObjects(List<MagneticObject> magneticObjects)
	{
		lockableObjects = magneticObjects;
	}

	public void SetObjectType(MagneticObjectType newObjectType)
	{
		objectType = newObjectType;
	}

	public void ConfigureMagneticLock(float newDistanceThreshold, float newAngleThreshold, float newBreakForce, float newBreakTorque)
	{
		distanceThreshold = newDistanceThreshold;
		angleThreshold = newAngleThreshold;
		breakForce = newBreakForce;
		breakTorque = newBreakTorque;
	}

	public float GetFixedJointBreakForce()
	{
		if (fixedJoint == null)
		{
			return float.NegativeInfinity;
		}
		return fixedJoint.breakForce;
	}

	public void SetFixedJointBreakForce(float force)
	{
		if (!(fixedJoint == null))
		{
			fixedJoint.breakForce = force;
		}
	}
}
