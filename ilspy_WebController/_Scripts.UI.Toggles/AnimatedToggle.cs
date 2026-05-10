using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.UI.Toggles;

public class AnimatedToggle : MonoBehaviour
{
	[Header("Animation")]
	[SerializeField]
	private Animator animator;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onEvent;

	[SerializeField]
	private UnityEvent offEvent;

	private bool isOn = true;

	private void OnEnable()
	{
		animator.speed = 1000f;
		animator.SetBool("isOn", isOn);
	}

	private void OnToggleChanged()
	{
		animator.speed = 1f;
		animator.SetBool("isOn", isOn);
		if (isOn)
		{
			onEvent.Invoke();
		}
		else
		{
			offEvent.Invoke();
		}
	}

	public void ToggleValue()
	{
		isOn = !isOn;
		OnToggleChanged();
	}

	public void SetInitialState(bool value)
	{
		isOn = value;
		animator.speed = 1000f;
		animator.SetBool("isOn", isOn);
	}

	public bool GetState()
	{
		return isOn;
	}
}
