using UnityEngine;

namespace _Scripts.LivingRoom;

public class AirCleaner : MonoBehaviour
{
	[SerializeField]
	private GameObject windArea;

	private bool isOn;

	private void Start()
	{
		windArea.SetActive(value: false);
	}

	public void ToggleOnOff()
	{
		isOn = !isOn;
		windArea.SetActive(isOn);
	}
}
