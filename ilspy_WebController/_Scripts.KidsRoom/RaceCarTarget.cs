using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.KidsRoom;

public class RaceCarTarget : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onToyCarHit;

	private bool gotHitByToyCar;

	private void Start()
	{
		gotHitByToyCar = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!gotHitByToyCar)
		{
			ToyCar componentInParent = other.gameObject.GetComponentInParent<ToyCar>();
			if (!(componentInParent == null) && componentInParent.HasPlayerSpider())
			{
				gotHitByToyCar = true;
				onToyCarHit?.Invoke();
			}
		}
	}
}
