using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Office;

public class AnimatedGate : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform leftGate;

	[SerializeField]
	private Transform rightGate;

	[Header("Parameters")]
	[SerializeField]
	private float leftGateCloseAngle;

	[SerializeField]
	private float leftGateOpenAngle;

	[SerializeField]
	private float rightGateCloseAngle;

	[SerializeField]
	private float rightGateOpenAngle;

	[SerializeField]
	private float animationTime = 1f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onStartEvent;

	private void Start()
	{
		onStartEvent?.Invoke();
	}

	public void OpenGate()
	{
		leftGate.localEulerAngles = new Vector3(0f, leftGateOpenAngle, 0f);
		rightGate.localEulerAngles = new Vector3(0f, rightGateOpenAngle, 0f);
	}

	public void CloseGate()
	{
		leftGate.localEulerAngles = new Vector3(0f, leftGateCloseAngle, 0f);
		rightGate.localEulerAngles = new Vector3(0f, rightGateCloseAngle, 0f);
	}

	public void PlayCloseGateAnimation()
	{
		leftGate.DOLocalRotate(new Vector3(0f, leftGateCloseAngle, 0f), animationTime);
		rightGate.DOLocalRotate(new Vector3(0f, rightGateCloseAngle, 0f), animationTime);
	}

	public void PlayOpenGateAnimation()
	{
		leftGate.DOLocalRotate(new Vector3(0f, leftGateOpenAngle, 0f), animationTime);
		rightGate.DOLocalRotate(new Vector3(0f, rightGateOpenAngle, 0f), animationTime);
	}
}
