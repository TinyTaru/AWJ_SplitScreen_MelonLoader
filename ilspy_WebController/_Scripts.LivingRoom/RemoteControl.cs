using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.LivingRoom;

public class RemoteControl : MonoBehaviour
{
	[SerializeField]
	private UnityEvent onToggleOffOnButtonPressed;

	[SerializeField]
	private UnityEvent onTurnOffButtonPressed;

	[SerializeField]
	private UnityEvent onTurnOnButtonPressed;

	[SerializeField]
	private UnityEvent onPlayButtonPressed;

	[SerializeField]
	private UnityEvent onDecreaseVolumeButtonPressed;

	[SerializeField]
	private UnityEvent onIncreaseVolumeButtonPressed;

	public void ToggleOnOff()
	{
		onToggleOffOnButtonPressed?.Invoke();
	}

	public void TurnOff()
	{
		onTurnOffButtonPressed?.Invoke();
	}

	public void TurnOn()
	{
		onTurnOnButtonPressed?.Invoke();
	}

	public void Play()
	{
		onPlayButtonPressed?.Invoke();
	}

	public void DecreaseVolume()
	{
		onDecreaseVolumeButtonPressed?.Invoke();
	}

	public void IncreaseVolume()
	{
		onIncreaseVolumeButtonPressed?.Invoke();
	}
}
