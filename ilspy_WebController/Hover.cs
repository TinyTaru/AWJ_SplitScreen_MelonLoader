using DG.Tweening;
using UnityEngine;

public class Hover : MonoBehaviour
{
	[Tooltip("Sets the Amplitude how extreme the hovering should be.")]
	[SerializeField]
	[Range(0f, 10f)]
	private float hoveringAmplitude = 1f;

	[Tooltip("Sets the Duration of one hovering direction")]
	[SerializeField]
	[Range(0f, 10f)]
	private float hoverDuration = 0.5f;

	private float initialYPosition;

	private bool isMovingUp;

	private float hoverMaxPosition => initialYPosition + hoveringAmplitude / 2f;

	private float hoverMinPosition => initialYPosition - hoveringAmplitude / 2f;

	private void Start()
	{
		initialYPosition = base.transform.position.y;
	}

	private void Update()
	{
		FlyOnPoint();
	}

	private void FlyOnPoint()
	{
		if (hoverMaxPosition - base.transform.position.y < 0.1f && isMovingUp)
		{
			isMovingUp = false;
		}
		else if (base.transform.position.y - hoverMinPosition < 0.1f && !isMovingUp)
		{
			isMovingUp = true;
		}
		base.transform.DOMoveY(isMovingUp ? hoverMaxPosition : hoverMinPosition, hoverDuration);
	}
}
