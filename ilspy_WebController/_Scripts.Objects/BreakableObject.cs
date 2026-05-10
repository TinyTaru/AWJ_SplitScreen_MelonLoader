using System;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Achievements;
using _Scripts.Interactable;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
[DisallowMultipleComponent]
public class BreakableObject : MonoBehaviour
{
	public class OnBreakEventArgs : EventArgs
	{
		public string id;

		public string name;
	}

	[SerializeField]
	private bool canBreak = true;

	[SerializeField]
	private float breakThreshold = 300f;

	[SerializeField]
	private float breakPopForce = 1f;

	[SerializeField]
	private List<GameObject> brokenPieces;

	[SerializeField]
	private GameObject objectInside;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter breakSound;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onBreakEvent;

	private MovableObject movableObject;

	private Rigidbody rb;

	private bool isBroken;

	public event EventHandler<OnBreakEventArgs> OnBreak;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
		foreach (GameObject brokenPiece in brokenPieces)
		{
			brokenPiece.SetActive(value: false);
		}
		isBroken = false;
	}

	private void Start()
	{
		rb = movableObject.GetRigidbody();
	}

	private void OnCollisionEnter(Collision other)
	{
		float magnitude = other.relativeVelocity.magnitude;
		float magnitude2 = other.impulse.magnitude;
		if (canBreak && magnitude2 * magnitude > breakThreshold)
		{
			Break();
		}
	}

	private void Break()
	{
		if (isBroken)
		{
			return;
		}
		Vector3 linearVelocity = rb.linearVelocity;
		Vector3 angularVelocity = rb.angularVelocity;
		CleanableObject component = GetComponent<CleanableObject>();
		foreach (GameObject brokenPiece in brokenPieces)
		{
			brokenPiece.SetActive(value: true);
			brokenPiece.transform.parent = null;
			Rigidbody component2 = brokenPiece.GetComponent<Rigidbody>();
			if (component2 != null)
			{
				component2.linearVelocity = linearVelocity;
				component2.angularVelocity = angularVelocity;
				component2.AddExplosionForce(breakPopForce, rb.position, 10f);
			}
			if (component != null)
			{
				CleanableObject component3 = brokenPiece.GetComponent<CleanableObject>();
				if (component3 != null)
				{
					component3.SetInitialDirtAmount(component.DirtAmount);
				}
			}
		}
		if (objectInside != null)
		{
			Transform transform = UnityEngine.Object.Instantiate(objectInside, base.transform.position, base.transform.rotation, null).transform;
			SpawnableObject component4 = transform.GetComponent<SpawnableObject>();
			if (component4 != null)
			{
				component4.Setup();
			}
			else if (transform.GetComponent<InteractableUnlockable>() == null)
			{
				Debug.LogError(transform.name + " was spawned by breaking " + base.name + " but it is missing the SpawnableObject component! Please add it to the prefab of " + transform.name + "!");
			}
			float x = transform.localScale.x;
			transform.localScale = 0.9f * x * Vector3.one;
			transform.DOScale(x, 2f);
			Rigidbody component5 = transform.GetComponent<Rigidbody>();
			if (component5 != null)
			{
				component5.linearVelocity = linearVelocity / 2f;
				component5.angularVelocity = angularVelocity / 2f;
			}
		}
		if (breakSound != null)
		{
			breakSound.Play();
		}
		onBreakEvent?.Invoke();
		AchievementEvents.ItemBroken();
		UniqueID component6 = GetComponent<UniqueID>();
		if (component6 != null)
		{
			this.OnBreak?.Invoke(this, new OnBreakEventArgs
			{
				id = component6.ID,
				name = base.gameObject.name
			});
		}
		movableObject.DestroySafely();
		isBroken = true;
	}

	public void FreeBrokenPieces()
	{
		foreach (GameObject brokenPiece in brokenPieces)
		{
			brokenPiece.SetActive(value: true);
			brokenPiece.transform.parent = null;
		}
	}

	public void SetBreakable(bool value)
	{
		canBreak = value;
	}
}
