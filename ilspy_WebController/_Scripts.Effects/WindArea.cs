using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Scripts.Effects;

public class WindArea : MonoBehaviour
{
	[SerializeField]
	private ParticleSystem windParticleSystem;

	[Header("Parameters")]
	[SerializeField]
	private bool activeAtStart = true;

	[SerializeField]
	private float forceMagnitude;

	[SerializeField]
	private float maxVelocity;

	[SerializeField]
	private float playerMultiplier;

	private List<Rigidbody> rigidbodies;

	private bool isActive;

	private void Awake()
	{
		isActive = activeAtStart;
		rigidbodies = new List<Rigidbody>();
		if (windParticleSystem != null)
		{
			windParticleSystem.transform.localPosition = Vector3.zero;
		}
		MeshRenderer component = GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	private void Start()
	{
		StartCoroutine(SetParticleSystemPositionCoroutine());
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
				float num = ((!item.CompareTag("Player")) ? forceMagnitude : (forceMagnitude * playerMultiplier));
				item.AddForce(base.transform.up * num * Time.fixedDeltaTime, ForceMode.Force);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		Rigidbody componentInParent = other.GetComponentInParent<Rigidbody>();
		if (!(componentInParent == null))
		{
			if (componentInParent.name.Contains("Battery"))
			{
				Debug.Log("Battery detected!");
			}
			else if (!rigidbodies.Contains(componentInParent))
			{
				rigidbodies.Add(componentInParent);
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

	private IEnumerator SetParticleSystemPositionCoroutine()
	{
		if (windParticleSystem != null)
		{
			yield return new WaitForSeconds(0.1f);
			windParticleSystem.transform.localPosition = Vector3.down * 0.8f;
		}
	}

	public void SetActive(bool value)
	{
		isActive = value;
		if (windParticleSystem != null)
		{
			if (value)
			{
				windParticleSystem.Play();
			}
			else
			{
				windParticleSystem.Stop();
			}
		}
	}

	public void SetForceMagnitude(float value)
	{
		forceMagnitude = value;
	}
}
