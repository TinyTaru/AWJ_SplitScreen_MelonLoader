using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Objects;

public class EggTimer : MonoBehaviour
{
	[SerializeField]
	private float speedThreshold = -4f;

	[SerializeField]
	private float angleInterval = 0.5f;

	[Header("References")]
	[SerializeField]
	private Transform bottom;

	[SerializeField]
	private Transform top;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onStartMovingEvent;

	[SerializeField]
	private UnityEvent onStopMovingEvent;

	[SerializeField]
	private UnityEvent onFinishedEvent;

	private bool isFinished;

	private bool isMoving;

	private float oldAngle;

	private float angleTimer;

	private void Awake()
	{
		isFinished = true;
		isMoving = false;
		angleTimer = angleInterval;
	}

	private void Update()
	{
		angleTimer -= Time.deltaTime;
		if (!(angleTimer > 0f))
		{
			angleTimer = angleInterval;
			float num = Quaternion.Angle(bottom.rotation, top.rotation);
			if (isFinished && num > 5f)
			{
				isFinished = false;
			}
			else if (!isFinished && num < 3f)
			{
				isFinished = true;
				onFinishedEvent?.Invoke();
			}
			float num2 = (num - oldAngle) / angleInterval;
			if (!isMoving && num2 < speedThreshold)
			{
				isMoving = true;
				onStartMovingEvent?.Invoke();
			}
			else if (isMoving && num2 > speedThreshold + 1f)
			{
				isMoving = false;
				onStopMovingEvent?.Invoke();
			}
			oldAngle = num;
		}
	}
}
