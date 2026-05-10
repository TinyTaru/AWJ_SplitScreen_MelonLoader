using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.LivingRoom;

public class TabletButton : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onButtonPressedEvent;

	private Tablet tablet;

	private Collider buttonCollider;

	private void Awake()
	{
		tablet = GetComponentInParent<Tablet>();
		buttonCollider = GetComponent<Collider>();
		tablet.OnTurnOn += Tablet_OnTurnOn;
		tablet.OnTurnOff += Tablet_OnTurnOff;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.name == "WebTargetGfx")
		{
			Press();
		}
	}

	private void Press()
	{
		if (tablet.IsOn)
		{
			onButtonPressedEvent?.Invoke();
		}
	}

	private void Tablet_OnTurnOff()
	{
		if (!(buttonCollider == null))
		{
			buttonCollider.enabled = false;
		}
	}

	private void Tablet_OnTurnOn()
	{
		if (!(buttonCollider == null))
		{
			buttonCollider.enabled = true;
		}
	}
}
