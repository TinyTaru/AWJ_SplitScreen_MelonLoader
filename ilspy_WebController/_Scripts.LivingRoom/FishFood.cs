using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using _Scripts.General;
using _Scripts.Objects;

namespace _Scripts.LivingRoom;

public class FishFood : MonoBehaviour
{
	[SerializeField]
	private float underWaterDrag = 1f;

	[SerializeField]
	private float underWaterAngularDrag = 1f;

	[SerializeField]
	private ParticleSystem eatingParticles;

	[SerializeField]
	private FishFoodType foodType;

	[SerializeField]
	private StudioEventEmitter foodAppliedSound;

	[SerializeField]
	private LayerMask whatIsWater;

	private MovableObject movableObject;

	private bool isBeingEaten;

	private bool isInWater;

	private Rigidbody rb;

	private float airDrag;

	private float airAngularDrag;

	private List<GameObject> waterList;

	public bool IsBeingEaten => isBeingEaten;

	public FishFoodType FoodType => foodType;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
		rb = movableObject.GetRigidbody();
		isInWater = Physics.CheckSphere(rb.position, 2f, whatIsWater);
		rb.useGravity = true;
		airDrag = rb.linearDamping;
		airAngularDrag = rb.angularDamping;
		if (isInWater)
		{
			rb.useGravity = false;
			rb.linearDamping = underWaterDrag;
			rb.angularDamping = underWaterAngularDrag;
		}
		waterList = new List<GameObject>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Water") && !waterList.Contains(other.gameObject))
		{
			waterList.Add(other.gameObject);
			EnterWater();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Water"))
		{
			if (waterList.Contains(other.gameObject))
			{
				waterList.Remove(other.gameObject);
			}
			if (waterList.Count == 0)
			{
				ExitWater();
			}
		}
	}

	private void EnterWater()
	{
		isInWater = true;
		rb.useGravity = false;
		rb.linearDamping = underWaterDrag;
		rb.angularDamping = underWaterAngularDrag;
	}

	private void ExitWater()
	{
		isInWater = false;
		rb.useGravity = true;
		rb.linearDamping = airDrag;
		rb.angularDamping = airAngularDrag;
	}

	public void StartEating()
	{
		if (!isBeingEaten)
		{
			isBeingEaten = true;
			movableObject.SetColliderActive(value: false);
			movableObject.GetRigidbody().isKinematic = true;
			eatingParticles.Play();
		}
	}

	public void Destroy()
	{
		foodAppliedSound.Play();
		foodAppliedSound.transform.parent = null;
		Object.Destroy(foodAppliedSound.gameObject, 2f);
		eatingParticles.transform.parent = null;
		movableObject.DestroySafely();
	}
}
