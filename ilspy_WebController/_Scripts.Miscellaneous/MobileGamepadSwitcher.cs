using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _Scripts.Miscellaneous;

public class MobileGamepadSwitcher : MonoBehaviour
{
	[FormerlySerializedAs("gamepadControls")]
	[SerializeField]
	private GameObject[] gamepadGameObjects;

	[SerializeField]
	private GameObject[] touchGameObjects;

	private PlayerInput playerInput;

	private void Awake()
	{
		GameObject[] array = gamepadGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		array = touchGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}
}
