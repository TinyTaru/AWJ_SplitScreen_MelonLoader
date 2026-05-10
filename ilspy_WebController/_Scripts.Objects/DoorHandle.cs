using UnityEngine;
using UnityEngine.Events;
using _Scripts.Achievements;

namespace _Scripts.Objects;

public class DoorHandle : MonoBehaviour
{
	[SerializeField]
	private float downThreshold = 15f;

	[SerializeField]
	private float upThreshold = 5f;

	[SerializeField]
	private UnityEvent onDownEvent;

	[SerializeField]
	private UnityEvent onUpEvent;

	private bool IsDown;

	private void Awake()
	{
		IsDown = false;
	}

	private void Update()
	{
		float num = base.transform.localRotation.eulerAngles.z;
		if (num > 180f)
		{
			num -= 360f;
		}
		num = Mathf.Abs(num);
		if (!IsDown && num >= downThreshold)
		{
			IsDown = true;
			onDownEvent?.Invoke();
			AchievementEvents.TryToOpenDoor();
		}
		else if (IsDown && num <= upThreshold)
		{
			IsDown = false;
			onUpEvent?.Invoke();
		}
	}
}
