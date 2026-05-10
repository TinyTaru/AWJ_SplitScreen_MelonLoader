using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;

namespace _Scripts.KidsRoom;

public class CarTrackBooster : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private bool activeAtStart = true;

	[SerializeField]
	private float maxVelocity;

	[Header("Sound Effects")]
	[SerializeField]
	private StudioEventEmitter boostActivateSound;

	private List<Rigidbody> rigidbodies;

	private bool isActive;

	private float forceMagnitude;

	private int boostLevel;

	private void Awake()
	{
		isActive = activeAtStart;
		rigidbodies = new List<Rigidbody>();
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	private void FixedUpdate()
	{
		if (!isActive)
		{
			return;
		}
		foreach (Rigidbody item in rigidbodies.ToList())
		{
			if (item == null)
			{
				rigidbodies.Remove(item);
			}
			else if (!item.isKinematic && base.transform.InverseTransformVector(item.linearVelocity).y * base.transform.localScale.y < maxVelocity)
			{
				float num = forceMagnitude;
				item.AddForce(base.transform.up * num * Time.fixedDeltaTime, ForceMode.Force);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody componentInParent = other.GetComponentInParent<Rigidbody>();
		if (!(componentInParent == null) && !componentInParent.name.Contains("Battery") && !componentInParent.CompareTag("Player") && !rigidbodies.Contains(componentInParent))
		{
			rigidbodies.Add(componentInParent);
			switch (boostLevel)
			{
			case 1:
				boostActivateSound.Play();
				boostActivateSound.SetParameter("strength", 0f);
				break;
			case 2:
				boostActivateSound.Play();
				boostActivateSound.SetParameter("strength", 1f);
				break;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Rigidbody componentInParent = other.GetComponentInParent<Rigidbody>();
		if (!(componentInParent == null) && rigidbodies.Contains(componentInParent))
		{
			rigidbodies.Remove(componentInParent);
		}
	}

	public void SetActive(bool value)
	{
		isActive = value;
	}

	public void SetBoostLevel(int level, float force)
	{
		boostLevel = level;
		forceMagnitude = force;
	}
}
