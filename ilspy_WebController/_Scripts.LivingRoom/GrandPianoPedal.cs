using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.LivingRoom;

public class GrandPianoPedal : MonoBehaviour
{
	[SerializeField]
	private float activationThreshold = -5f;

	[SerializeField]
	private float deactivationThreshold;

	[SerializeField]
	private UnityEvent onActivationEvent;

	[SerializeField]
	private UnityEvent onDeactivationEvent;

	private bool isActive;

	private void Update()
	{
		float num = base.transform.localEulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (!isActive)
		{
			if (num < activationThreshold)
			{
				isActive = true;
				onActivationEvent?.Invoke();
			}
		}
		else if (num > deactivationThreshold)
		{
			isActive = false;
			onDeactivationEvent?.Invoke();
		}
	}
}
