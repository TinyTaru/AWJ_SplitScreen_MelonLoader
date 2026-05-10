using UnityEngine;

namespace _Scripts.Spider;

public class WaterTrigger : MonoBehaviour
{
	private BodyMovement bodyMovement;

	private void Start()
	{
		bodyMovement = GetComponentInParent<BodyMovement>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Water"))
		{
			bodyMovement.SetIsUnderwater(value: true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Water"))
		{
			bodyMovement.SetIsUnderwater(value: false);
		}
	}
}
