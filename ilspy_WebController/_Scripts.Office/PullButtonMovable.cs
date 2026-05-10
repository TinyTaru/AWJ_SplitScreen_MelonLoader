using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Office;

[RequireComponent(typeof(ConfigurableJoint))]
public class PullButtonMovable : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private float pressDistance = 0.09f;

	[SerializeField]
	private float releaseDistance = 0.05f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onButtonPressedEvent;

	[SerializeField]
	private UnityEvent onButtonReleasedEvent;

	private ConfigurableJoint configurableJoint;

	private bool isPressed;

	private bool isMainWebAttached;

	private void Awake()
	{
		configurableJoint = GetComponent<ConfigurableJoint>();
	}

	private void FixedUpdate()
	{
		if (!isMainWebAttached)
		{
			Vector3 b = configurableJoint.connectedBody.transform.TransformPoint(configurableJoint.connectedAnchor);
			float num = Vector3.Distance(base.transform.position, b);
			if (!isPressed && num > pressDistance)
			{
				isPressed = true;
				onButtonPressedEvent?.Invoke();
			}
			else if (isPressed && num < releaseDistance)
			{
				isPressed = false;
				onButtonReleasedEvent?.Invoke();
			}
		}
	}

	public void MainWebAttached()
	{
		isPressed = true;
		isMainWebAttached = true;
		onButtonPressedEvent?.Invoke();
	}

	public void MainWebReleased()
	{
		isMainWebAttached = false;
		onButtonReleasedEvent?.Invoke();
	}
}
