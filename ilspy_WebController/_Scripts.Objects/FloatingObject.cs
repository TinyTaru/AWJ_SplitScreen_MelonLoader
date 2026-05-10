using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
[DisallowMultipleComponent]
public class FloatingObject : MonoBehaviour
{
	[SerializeField]
	private float underWaterDrag = 1f;

	[SerializeField]
	private float underWaterAngularDrag = 1f;

	[SerializeField]
	private float floatingPower = 3f;

	[SerializeField]
	private Transform floaterContainer;

	[SerializeField]
	private float disableDistance = 100f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter waterSplashSound;

	private MovableObject movableObject;

	private Rigidbody rb;

	private List<Floater> floaters;

	private float airDrag;

	private float airAngularDrag;

	private bool isUnderWater;

	private Transform player;

	private bool canSleep;

	public StudioEventEmitter WaterSplashSound
	{
		get
		{
			return waterSplashSound;
		}
		set
		{
			waterSplashSound = value;
		}
	}

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
		floaters = new List<Floater>();
		foreach (Transform item in floaterContainer)
		{
			Floater component = item.GetComponent<Floater>();
			if (component != null)
			{
				floaters.Add(component);
			}
		}
	}

	private void Start()
	{
		rb = movableObject.GetRigidbody();
		airDrag = rb.linearDamping;
		airAngularDrag = rb.angularDamping;
		if (Singleton<GameController>.Instance == null)
		{
			player = null;
		}
		else
		{
			player = Singleton<GameController>.Instance.Player.Root;
		}
		StartCoroutine(EnableSleeping());
	}

	private void FixedUpdate()
	{
		if (rb.IsSleeping())
		{
			return;
		}
		bool flag = false;
		foreach (Floater floater in floaters)
		{
			if (floater.IsUnderWater)
			{
				Vector3 force = Vector3.up * floatingPower * floater.GetDifference();
				rb.AddForceAtPosition(force, floater.transform.position, ForceMode.Force);
				flag = true;
			}
		}
		if (!isUnderWater && flag)
		{
			SwitchState(value: true);
		}
		else if (isUnderWater && !flag)
		{
			SwitchState(value: false);
		}
	}

	private IEnumerator EnableSleeping()
	{
		yield return new WaitForSeconds(1f);
		canSleep = true;
	}

	private void SwitchState(bool value)
	{
		isUnderWater = value;
		if (isUnderWater)
		{
			rb.linearDamping = underWaterDrag;
			rb.angularDamping = underWaterAngularDrag;
			if (waterSplashSound != null)
			{
				waterSplashSound.Play();
			}
		}
		else
		{
			rb.linearDamping = airDrag;
			rb.angularDamping = airAngularDrag;
		}
	}

	public void AddFloater(Floater newFloater)
	{
		if (floaters == null)
		{
			floaters = new List<Floater> { newFloater };
		}
		else
		{
			floaters.Add(newFloater);
		}
	}
}
