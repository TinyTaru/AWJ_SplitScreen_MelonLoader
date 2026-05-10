using UnityEngine;

namespace _Scripts.UI.Toggles;

public class ButtonGroupToggler : MonoBehaviour
{
	[SerializeField]
	private GameObject onState;

	[SerializeField]
	private GameObject offState;

	public void SetState(bool isOn)
	{
		if (onState != null)
		{
			onState.SetActive(isOn);
		}
		if (offState != null)
		{
			offState.SetActive(!isOn);
		}
	}
}
