using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Objects;

public class FanControlUnit : MonoBehaviour
{
	[Header("Events")]
	[SerializeField]
	private UnityEvent onTurnedOff;

	private bool isOn = true;

	public void TurnOff()
	{
		if (isOn)
		{
			isOn = false;
			onTurnedOff.Invoke();
		}
	}
}
