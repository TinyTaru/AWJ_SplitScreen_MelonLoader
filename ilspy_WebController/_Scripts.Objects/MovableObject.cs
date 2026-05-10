using System;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.Web;

namespace _Scripts.Objects;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class MovableObject : MonoBehaviour, IInitializable<MovableObjectSaveData>, IHasSaveData<MovableObjectSaveData>
{
	public class OnForwardCollisionCheckEventArgs : EventArgs
	{
		public Collision other;
	}

	public class OnDestroyEventArgs : EventArgs
	{
		public string id;

		public string name;
	}

	[SerializeField]
	private float mass = 1f;

	[SerializeField]
	private bool canBePushed;

	[SerializeField]
	private bool collideWithCamera;

	[SerializeField]
	private float gravityMultiplier;

	[SerializeField]
	private List<Collider> colliders;

	[Header("Parameters")]
	[SerializeField]
	private float minSpeedForCollisionSound = 10f;

	[SerializeField]
	private float maxSpeedForCollisionSound = 30f;

	[SerializeField]
	private float collisionCooldownDuration = 0.5f;

	[SerializeField]
	private float jointBreakPopForce = 1f;

	[SerializeField]
	private Vector3 relativePopForceDirection = new Vector3(0f, 1f, 0f);

	[SerializeField]
	private bool useCenterOfMassAsWebAnchor;

	[SerializeField]
	private bool doesntNeedCollisionSound;

	[Header("Effects")]
	[SerializeField]
	private ParticleSystem collisionEffect;

	[SerializeField]
	private StudioEventEmitter collisionSound;

	[SerializeField]
	private StudioEventEmitter jointBreakSound;

	[SerializeField]
	private UnityEvent OnJointBreakEvent;

	[SerializeField]
	private UnityEvent onCollisionEvent;

	[SerializeField]
	private UnityEvent onMainWebAttached;

	[SerializeField]
	private UnityEvent onMainWebReleased;

	[SerializeField]
	private UnityEvent onWebJointAdded;

	[SerializeField]
	private UnityEvent onWebJointRemoved;

	[SerializeField]
	private UnityEvent onPlayerSpiderAdded;

	[SerializeField]
	private UnityEvent onPlayerSpiderRemoved;

	[SerializeField]
	private UnityEvent onCurrentWebTargetSet;

	[SerializeField]
	private UnityEvent onCurrentWebTargetReset;

	private List<WebJoint> connectedWebJoints;

	private Rigidbody rb;

	private float collisionCooldownTimer;

	private bool mainWebIsAttached;

	private bool hasPlayerSpider;

	private bool isCurrentWebTarget;

	public StudioEventEmitter CollisionSound
	{
		get
		{
			return collisionSound;
		}
		set
		{
			collisionSound = value;
		}
	}

	public float GravityMultiplier => gravityMultiplier;

	public bool UseCenterOfMassAsWebAnchor => useCenterOfMassAsWebAnchor;

	public bool HasCollisionSound
	{
		get
		{
			if (!(collisionSound != null))
			{
				return doesntNeedCollisionSound;
			}
			return true;
		}
	}

	public bool HasConnectedWebJoint => connectedWebJoints.Count > 0;

	public bool HasPlayerSpider => hasPlayerSpider;

	public bool IsCurrentWebTarget => isCurrentWebTarget;

	public event EventHandler<OnForwardCollisionCheckEventArgs> OnForwardCollisionCheck;

	public event EventHandler<OnDestroyEventArgs> OnDestroy;

	private void Reset()
	{
		rb = GetComponent<Rigidbody>();
		rb.linearDamping = 0.2f;
		rb.angularDamping = 0.2f;
	}

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		connectedWebJoints = new List<WebJoint>();
		collisionCooldownTimer = collisionCooldownDuration;
		if (!collideWithCamera)
		{
			base.transform.tag = "WebbedObject";
		}
		SetDefaultLayer();
	}

	private void OnCollisionEnter(Collision other)
	{
		float magnitude = other.relativeVelocity.magnitude;
		if (magnitude > minSpeedForCollisionSound && collisionCooldownTimer <= 0f)
		{
			collisionCooldownTimer = collisionCooldownDuration;
			if (collisionSound != null)
			{
				float value = Mathf.Clamp01((magnitude - minSpeedForCollisionSound) / (maxSpeedForCollisionSound - minSpeedForCollisionSound));
				collisionSound.Play();
				collisionSound.SetParameter("volume", value);
			}
			if (collisionEffect != null)
			{
				collisionEffect.transform.position = other.contacts[0].point;
				collisionEffect.Play();
			}
			onCollisionEvent?.Invoke();
		}
		this.OnForwardCollisionCheck?.Invoke(this, new OnForwardCollisionCheckEventArgs
		{
			other = other
		});
	}

	private void OnJointBreak(float breakForce)
	{
		if (jointBreakSound != null)
		{
			jointBreakSound.Play();
		}
		Vector3 normalized = (base.transform.right * relativePopForceDirection.x + base.transform.up * relativePopForceDirection.y + base.transform.forward * relativePopForceDirection.z).normalized;
		rb.AddForce(normalized * jointBreakPopForce, ForceMode.Impulse);
		OnJointBreakEvent?.Invoke();
	}

	private void Update()
	{
		collisionCooldownTimer -= Time.deltaTime;
	}

	public void SetLayerToWebbedObject()
	{
		string layer = (collideWithCamera ? "WebbedObjectWithCameraCollision" : "WebbedObject");
		SetLayer(layer);
	}

	public void SetDefaultLayer()
	{
		string layer = (canBePushed ? "Movable" : (collideWithCamera ? "WebbedObjectWithCameraCollision" : "WebbedObject"));
		SetLayer(layer);
	}

	public void SetLayerToAttachedToPlayer()
	{
		SetLayer("AttachedToPlayer");
	}

	public void SetLayerToDefault()
	{
		SetLayer("Default");
	}

	private void AutoAssignColliders()
	{
		colliders.Clear();
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.gameObject.activeSelf && !collider.isTrigger && !collider.GetComponent<IgnoreLayerCheck>())
			{
				collider.gameObject.layer = LayerMask.NameToLayer("Movable");
				colliders.Add(collider);
			}
		}
	}

	private void QuickSetup()
	{
		Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>();
		bool flag = false;
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (!transform.name.ToUpper().Contains("_COL"))
			{
				continue;
			}
			flag = true;
			MeshFilter component = transform.GetComponent<MeshFilter>();
			MeshRenderer component2 = transform.GetComponent<MeshRenderer>();
			if (!(component == null) && !(component2 == null))
			{
				if (!transform.gameObject.GetComponent<Collider>())
				{
					MeshCollider meshCollider = transform.gameObject.AddComponent<MeshCollider>();
					meshCollider.sharedMesh = component.sharedMesh;
					meshCollider.convex = true;
				}
				UnityEngine.Object.DestroyImmediate(component);
				UnityEngine.Object.DestroyImmediate(component2);
			}
		}
		if (!flag && GetComponent<Collider>() == null)
		{
			MeshFilter component3 = GetComponent<MeshFilter>();
			MeshRenderer component4 = GetComponent<MeshRenderer>();
			if (component3 != null && component4 != null)
			{
				MeshCollider meshCollider2 = base.transform.gameObject.AddComponent<MeshCollider>();
				meshCollider2.sharedMesh = component3.sharedMesh;
				meshCollider2.convex = true;
			}
		}
		AutoAssignColliders();
	}

	private void AutoAssignSounds()
	{
		StudioEventEmitter[] componentsInChildren = base.transform.GetComponentsInChildren<StudioEventEmitter>();
		foreach (StudioEventEmitter studioEventEmitter in componentsInChildren)
		{
			if (studioEventEmitter.name == "CollisionSound")
			{
				collisionSound = studioEventEmitter;
				FixCollisionsSoundParameters();
			}
			if (studioEventEmitter.name == "JointBreakSound")
			{
				jointBreakSound = studioEventEmitter;
			}
		}
	}

	private void FixCollisionsSoundParameters()
	{
		if (!(collisionSound == null) && collisionSound.Params[0].Name == "collision_material")
		{
			float value = collisionSound.Params[0].Value;
			collisionSound.Params = new ParamRef[2]
			{
				new ParamRef(),
				new ParamRef()
			};
			collisionSound.Params[0].Name = "volume";
			collisionSound.Params[0].Value = 1f;
			collisionSound.Params[1].Name = "collision_material";
			collisionSound.Params[1].Value = value;
		}
	}

	private void UpdateRigidbodyMass()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if (!(component == null))
		{
			component.mass = mass;
		}
	}

	public void Initialize(MovableObjectSaveData saveData)
	{
		base.transform.position = saveData.position;
		base.transform.rotation = saveData.rotation;
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}
		if (!rb.isKinematic)
		{
			rb.linearVelocity = saveData.linearVelocity;
			rb.angularVelocity = saveData.angularVelocity;
		}
	}

	public MovableObjectSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Movable Object " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		MovableObjectSaveData result = default(MovableObjectSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.position = base.transform.position;
		result.rotation = base.transform.rotation;
		result.linearVelocity = rb.linearVelocity;
		result.angularVelocity = rb.angularVelocity;
		return result;
	}

	public void SetColliderActive(bool value)
	{
		foreach (Collider collider in colliders)
		{
			if (collider == null)
			{
				Debug.LogWarning("Collider is null. Please check the Movable Object " + base.name);
			}
			else
			{
				collider.enabled = value;
			}
		}
	}

	public void SetCollideWithCamera(bool value)
	{
		collideWithCamera = value;
	}

	public void SetCanBePushed(bool value)
	{
		canBePushed = value;
	}

	private void SetLayer(string layer)
	{
		if (base.gameObject != null)
		{
			base.gameObject.layer = LayerMask.NameToLayer(layer);
		}
		foreach (Collider collider in colliders)
		{
			if (collider == null)
			{
				Debug.LogWarning("Collider is null. Please check the Movable Object " + base.name);
			}
			else
			{
				collider.gameObject.layer = LayerMask.NameToLayer(layer);
			}
		}
	}

	public void DestroySafely()
	{
		DontDestroyMe[] componentsInChildren = GetComponentsInChildren<DontDestroyMe>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetParent();
		}
		RemoveAllWebJoints();
		ReleaseWeb();
		UnityEngine.Object.Destroy(base.gameObject);
		string id = GetComponent<UniqueID>()?.ID;
		this.OnDestroy?.Invoke(this, new OnDestroyEventArgs
		{
			id = id,
			name = base.name
		});
	}

	public void DisableSafely()
	{
		DontDestroyMe[] componentsInChildren = GetComponentsInChildren<DontDestroyMe>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetParent();
		}
		RemoveAllWebJoints();
		ReleaseWeb();
		base.gameObject.SetActive(value: false);
	}

	public void AddWebJoint(WebJoint webJoint)
	{
		if (!connectedWebJoints.Contains(webJoint))
		{
			connectedWebJoints.Add(webJoint);
			SetLayerToWebbedObject();
			onWebJointAdded?.Invoke();
		}
	}

	public void RemoveWebJoint(WebJoint webJoint)
	{
		connectedWebJoints.Remove(webJoint);
		ResetLayer();
		onWebJointRemoved?.Invoke();
	}

	public void ResetLayer()
	{
		if (!base.enabled)
		{
			return;
		}
		bool flag = false;
		foreach (WebJoint connectedWebJoint in connectedWebJoints)
		{
			if ((bool)connectedWebJoint.GetComponent<BodyMovement>())
			{
				flag = true;
				break;
			}
		}
		if (connectedWebJoints.Count > 0 && !flag)
		{
			SetLayerToWebbedObject();
		}
		if (connectedWebJoints.Count == 0 && Singleton<GameController>.Instance.Player.TargetMovableObject != this)
		{
			SetDefaultLayer();
		}
	}

	public void ReleaseWeb()
	{
		if (!(Singleton<WebController>.Instance == null))
		{
			if (Singleton<WebController>.Instance.WebTargetObject == base.gameObject)
			{
				Singleton<WebController>.Instance.ReleaseWeb();
			}
			if (Singleton<WebController>.Instance.WebAnchorObject == base.gameObject)
			{
				Singleton<WebController>.Instance.ActivateDefaultMode();
			}
		}
	}

	public void RemoveAllWebJoints(bool playAnimation = false)
	{
		foreach (WebJoint item in connectedWebJoints.ToList())
		{
			if (!(item == null))
			{
				Singleton<WebController>.Instance.DestroyWebJoint(item, playAnimation);
				RemoveWebJoint(item);
			}
		}
	}

	public void BreakAllJoints()
	{
		Joint[] components = GetComponents<Joint>();
		for (int i = 0; i < components.Length; i++)
		{
			UnityEngine.Object.Destroy(components[i]);
		}
		Vector3 normalized = (base.transform.right * relativePopForceDirection.x + base.transform.up * relativePopForceDirection.y + base.transform.forward * relativePopForceDirection.z).normalized;
		rb.AddForce(normalized * jointBreakPopForce, ForceMode.Impulse);
	}

	public Rigidbody GetRigidbody()
	{
		if (rb == null)
		{
			rb = GetComponent<Rigidbody>();
		}
		return rb;
	}

	public int ConnectedWebJointCount()
	{
		return connectedWebJoints.Count;
	}

	public void MainWebAttached()
	{
		if (!mainWebIsAttached)
		{
			onMainWebAttached?.Invoke();
		}
		mainWebIsAttached = true;
	}

	public void MainWebReleased()
	{
		if (mainWebIsAttached)
		{
			onMainWebReleased?.Invoke();
		}
		mainWebIsAttached = false;
	}

	public void ExcludePlayerCollision()
	{
		hasPlayerSpider = true;
		LayerMask excludeLayers = LayerMask.GetMask("Spider");
		foreach (Collider collider in colliders)
		{
			if (collider == null)
			{
				Debug.LogWarning("Collider is null. Please check the Movable Object " + base.name);
			}
			else
			{
				collider.excludeLayers = excludeLayers;
			}
		}
		onPlayerSpiderAdded?.Invoke();
	}

	public void ResetCollisionExclusions()
	{
		hasPlayerSpider = false;
		foreach (Collider collider in colliders)
		{
			if (collider == null)
			{
				Debug.LogWarning("Collider is null. Please check the Movable Object " + base.name);
			}
			else
			{
				collider.excludeLayers = default(LayerMask);
			}
		}
		onPlayerSpiderRemoved?.Invoke();
	}

	public void SetAsCurrentWebTarget(bool value)
	{
		isCurrentWebTarget = value;
		if (isCurrentWebTarget)
		{
			onCurrentWebTargetSet?.Invoke();
		}
		else
		{
			onCurrentWebTargetReset?.Invoke();
		}
	}
}
