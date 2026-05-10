using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.KidsRoom;

[DisallowMultipleComponent]
public class PianoKey : MonoBehaviour
{
	private enum KeyPosition
	{
		Middle,
		Down,
		Up,
		MainWeb
	}

	[Header("Parameters")]
	[SerializeField]
	private int keyIndex;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent<int> onKeyPressed;

	[SerializeField]
	private UnityEvent<int> onKeyReleased;

	private KeyPosition keyPosition;

	private bool isMainWebAttached;

	private const float positiveActivationThreshold = 3f;

	private const float poistiveDeactivationThreshold = 1f;

	private const float negativeActivationThreshold = -2f;

	private const float negativeDeactivationThreshold = -1f;

	private void Start()
	{
		keyPosition = KeyPosition.Middle;
	}

	private void Update()
	{
		float x = base.transform.localRotation.eulerAngles.x;
		x = ((x > 180f) ? (x - 360f) : x);
		switch (keyPosition)
		{
		case KeyPosition.Middle:
			if (x > 3f)
			{
				keyPosition = KeyPosition.Up;
				PressKey();
			}
			else if (x < -2f)
			{
				keyPosition = KeyPosition.Down;
				PressKey();
			}
			break;
		case KeyPosition.Down:
			if (x > -1f)
			{
				keyPosition = KeyPosition.Middle;
				ReleaseKey();
			}
			break;
		case KeyPosition.Up:
			if (x < 1f)
			{
				keyPosition = KeyPosition.Middle;
				ReleaseKey();
			}
			break;
		case KeyPosition.MainWeb:
			if (!isMainWebAttached && x > -1f && x < 1f)
			{
				keyPosition = KeyPosition.Middle;
				ReleaseKey();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetKeyIndex()
	{
		keyIndex = base.transform.GetSiblingIndex() + 1;
	}

	private void PressKey()
	{
		onKeyPressed?.Invoke(keyIndex);
	}

	private void ReleaseKey()
	{
		onKeyReleased?.Invoke(keyIndex);
	}

	public void MainWebAttached()
	{
		keyPosition = KeyPosition.MainWeb;
		isMainWebAttached = true;
		PressKey();
	}

	public void MainWebReleased()
	{
		isMainWebAttached = false;
		ReleaseKey();
	}
}
