using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;

namespace _Scripts.LivingRoom;

[RequireComponent(typeof(MovableObject))]
public class ButtonRobotVacuumCleaner : MonoBehaviour
{
	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onButtonPressedEvent;

	[SerializeField]
	private UnityEvent onButtonReleasedEvent;

	private MovableObject movableObject;

	private Vector3 startPosition;

	private bool isPressed;

	private bool isMainWebAttached;

	private bool isWebJointAttached;

	private bool isPlayerSpiderAttached;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
	}

	private void PressButton()
	{
		if (!isPressed)
		{
			isPressed = true;
			onButtonPressedEvent?.Invoke();
		}
	}

	private void ReleaseButton()
	{
		if (isPressed && !isWebJointAttached && !isMainWebAttached && !isPlayerSpiderAttached)
		{
			isPressed = false;
			onButtonReleasedEvent?.Invoke();
		}
	}

	public void MainWebAttached()
	{
		isMainWebAttached = true;
		PressButton();
	}

	public void MainWebReleased()
	{
		isMainWebAttached = false;
		ReleaseButton();
	}

	public void WebJointAdded()
	{
		isWebJointAttached = true;
		PressButton();
	}

	public void WebJointRemoved()
	{
		isWebJointAttached = movableObject.HasConnectedWebJoint;
		ReleaseButton();
	}

	public void PlayerSpiderAdded()
	{
		isPlayerSpiderAttached = true;
		PressButton();
	}

	public void PlayerSpiderRemoved()
	{
		isPlayerSpiderAttached = false;
		ReleaseButton();
	}
}
