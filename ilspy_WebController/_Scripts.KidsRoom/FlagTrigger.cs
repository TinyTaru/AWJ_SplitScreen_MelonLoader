using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.KidsRoom;

public class FlagTrigger : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform flag;

	[SerializeField]
	private GameObject particleEffect;

	[Header("Parameters")]
	[SerializeField]
	private float positionThreshold = 10f;

	[SerializeField]
	private float dotProductThreshold = 0.5f;

	[SerializeField]
	private float positionDuration = 2f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onFlagInPositionEvent;

	private bool flagInRange;

	private float positionTimer;

	private void Update()
	{
		if (!flagInRange)
		{
			positionTimer = positionDuration;
			return;
		}
		float num = Vector3.Distance(flag.position, base.transform.position);
		float num2 = Vector3.Dot(flag.up, base.transform.up);
		if (num < positionThreshold && num2 > dotProductThreshold)
		{
			positionTimer -= Time.deltaTime;
			if (positionTimer <= 0f)
			{
				FlagInPosition();
			}
		}
		else
		{
			positionTimer = positionDuration;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform == flag)
		{
			flagInRange = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform == flag)
		{
			flagInRange = false;
		}
	}

	private void FlagInPosition()
	{
		Object.Instantiate(particleEffect, flag.position, Quaternion.identity);
		onFlagInPositionEvent?.Invoke();
		Object.Destroy(base.gameObject);
	}
}
