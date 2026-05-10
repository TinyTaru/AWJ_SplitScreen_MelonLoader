using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Debugging;

public class InputDeviceTest : MonoBehaviour
{
	private void Start()
	{
		foreach (InputDevice device in InputSystem.devices)
		{
			Debug.Log("Device: " + device.displayName + ", interface: " + device.description.interfaceName + ", product: " + device.description.product);
		}
	}
}
