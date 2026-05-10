using UnityEngine;

namespace Cinemachine.Examples;

[SaveDuringPlay]
public class ThirdPersonFollowDistanceModifier : MonoBehaviour
{
	[Tooltip("Camera angle that corresponds to the start of the distance graph")]
	public float MinAngle;

	[Tooltip("Camera angle that corresponds to the end of the distance graph")]
	public float MaxAngle;

	[Tooltip("Defines how the camera distance scales as a function of vertical camera angle.  X axis of graph go from 0 to 1, Y axis is the multiplier that will be applied to the base distance.")]
	public AnimationCurve DistanceScale;

	private Cinemachine3rdPersonFollow TpsFollow;

	private Transform FollowTarget;

	private float BaseDistance;

	private void Reset()
	{
		MinAngle = -90f;
		MaxAngle = 90f;
		DistanceScale = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 2f);
	}

	private void OnEnable()
	{
		CinemachineVirtualCamera componentInChildren = GetComponentInChildren<CinemachineVirtualCamera>();
		if (componentInChildren != null)
		{
			TpsFollow = componentInChildren.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
			FollowTarget = componentInChildren.Follow;
		}
		if (TpsFollow != null)
		{
			BaseDistance = TpsFollow.CameraDistance;
		}
	}

	private void OnDisable()
	{
		if (TpsFollow != null)
		{
			TpsFollow.CameraDistance = BaseDistance;
		}
	}

	private void Update()
	{
		if (TpsFollow != null && FollowTarget != null)
		{
			float num = FollowTarget.rotation.eulerAngles.x;
			if (num > 180f)
			{
				num -= 360f;
			}
			float time = (num - MinAngle) / (MaxAngle - MinAngle);
			TpsFollow.CameraDistance = BaseDistance * DistanceScale.Evaluate(time);
		}
	}
}
