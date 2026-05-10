using DG.Tweening;
using UnityEngine;

namespace _Scripts.Spider;

public class LegTargetJump : MonoBehaviour
{
	[SerializeField]
	private float radius = 0.2f;

	[SerializeField]
	private float minTime = 0.5f;

	[SerializeField]
	private float maxTime = 1f;

	private Vector3 initialPosition;

	private void Start()
	{
		initialPosition = base.transform.localPosition;
		MoveToRandomPosition();
	}

	private void MoveToRandomPosition()
	{
		Vector3 endValue = initialPosition + new Vector3(Random.Range(0f - radius, radius), Random.Range(0f - radius, radius), Random.Range(0f - radius, radius));
		float duration = Random.Range(minTime, maxTime);
		base.transform.DOLocalMove(endValue, duration).OnComplete(MoveToRandomPosition);
	}
}
