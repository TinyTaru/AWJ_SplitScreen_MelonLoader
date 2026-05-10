using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.LivingRoom;

public class FishWaterTrigger : MonoBehaviour
{
	[SerializeField]
	private LayerMask whatIsWater;

	private List<GameObject> waterList;

	[SerializeField]
	private UnityEvent onWaterEntered;

	[SerializeField]
	private UnityEvent onWaterExited;

	private void Start()
	{
		waterList = new List<GameObject>();
		Collider[] array = Physics.OverlapSphere(base.transform.position, 2f, whatIsWater);
		foreach (Collider collider in array)
		{
			waterList.Add(collider.gameObject);
		}
		if (waterList.Count > 0)
		{
			onWaterEntered?.Invoke();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Water") && !waterList.Contains(other.gameObject))
		{
			waterList.Add(other.gameObject);
			onWaterEntered?.Invoke();
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
			onWaterExited?.Invoke();
		}
	}
}
