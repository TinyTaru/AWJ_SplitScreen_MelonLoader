using Cinemachine;
using UnityEngine;

public class MoveAimTarget : MonoBehaviour
{
	public CinemachineBrain Brain;

	public RectTransform ReticleImage;

	[Tooltip("How far to raycast to place the aim target")]
	public float AimDistance;

	[Tooltip("Objects on these layers will be detected")]
	public LayerMask CollideAgainst;

	[TagField]
	[Tooltip("Obstacles with this tag will be ignored.  It's a good idea to set this field to the player's tag")]
	public string IgnoreTag = string.Empty;

	[Header("Axis Control")]
	[Tooltip("The Vertical axis.  Value is -90..90. Controls the vertical orientation")]
	[AxisStateProperty]
	public AxisState VerticalAxis;

	[Tooltip("The Horizontal axis.  Value is -180..180.  Controls the horizontal orientation")]
	[AxisStateProperty]
	public AxisState HorizontalAxis;

	private void OnValidate()
	{
		VerticalAxis.Validate();
		HorizontalAxis.Validate();
		AimDistance = Mathf.Max(1f, AimDistance);
	}

	private void Reset()
	{
		AimDistance = 200f;
		ReticleImage = null;
		CollideAgainst = 1;
		IgnoreTag = string.Empty;
		VerticalAxis = new AxisState(-70f, 70f, wrap: false, rangeLocked: false, 10f, 0.1f, 0.1f, "Mouse Y", invert: true);
		VerticalAxis.m_SpeedMode = AxisState.SpeedMode.InputValueGain;
		HorizontalAxis = new AxisState(-180f, 180f, wrap: true, rangeLocked: false, 10f, 0.1f, 0.1f, "Mouse X", invert: false);
		HorizontalAxis.m_SpeedMode = AxisState.SpeedMode.InputValueGain;
	}

	private void OnEnable()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(PlaceReticle);
		CinemachineCore.CameraUpdatedEvent.AddListener(PlaceReticle);
	}

	private void OnDisable()
	{
		CinemachineCore.CameraUpdatedEvent.RemoveListener(PlaceReticle);
	}

	private void Update()
	{
		if (!(Brain == null))
		{
			HorizontalAxis.Update(Time.deltaTime);
			VerticalAxis.Update(Time.deltaTime);
			PlaceTarget();
		}
	}

	private void PlaceTarget()
	{
		Quaternion quaternion = Quaternion.Euler(VerticalAxis.Value, HorizontalAxis.Value, 0f);
		Vector3 rawPosition = Brain.CurrentCameraState.RawPosition;
		base.transform.position = GetProjectedAimTarget(rawPosition + quaternion * Vector3.forward, rawPosition);
	}

	private Vector3 GetProjectedAimTarget(Vector3 pos, Vector3 camPos)
	{
		Vector3 origin = pos;
		Vector3 normalized = (pos - camPos).normalized;
		pos += AimDistance * normalized;
		if ((int)CollideAgainst != 0 && RaycastIgnoreTag(new Ray(origin, normalized), out var hitInfo, AimDistance, CollideAgainst))
		{
			pos = hitInfo.point;
		}
		return pos;
	}

	private bool RaycastIgnoreTag(Ray ray, out RaycastHit hitInfo, float rayLength, int layerMask)
	{
		float num = 0f;
		while (Physics.Raycast(ray, out hitInfo, rayLength, layerMask, QueryTriggerInteraction.Ignore))
		{
			if (IgnoreTag.Length == 0 || !hitInfo.collider.CompareTag(IgnoreTag))
			{
				hitInfo.distance += num;
				return true;
			}
			Ray ray2 = new Ray(ray.GetPoint(rayLength), -ray.direction);
			if (!hitInfo.collider.Raycast(ray2, out hitInfo, rayLength))
			{
				break;
			}
			float num2 = rayLength - (hitInfo.distance - 0.001f);
			if (num2 < 0.001f)
			{
				break;
			}
			num += num2;
			rayLength = hitInfo.distance - 0.001f;
			if (rayLength < 0.001f)
			{
				break;
			}
			ray.origin = ray2.GetPoint(rayLength);
		}
		return false;
	}

	private void PlaceReticle(CinemachineBrain brain)
	{
		if (!(brain == null) && !(brain != Brain) && !(ReticleImage == null) && !(brain.OutputCamera == null))
		{
			PlaceTarget();
			_ = brain.CurrentCameraState;
			Camera outputCamera = brain.OutputCamera;
			Vector3 vector = outputCamera.WorldToScreenPoint(base.transform.position);
			Vector2 anchoredPosition = new Vector2(vector.x - (float)outputCamera.pixelWidth * 0.5f, vector.y - (float)outputCamera.pixelHeight * 0.5f);
			ReticleImage.anchoredPosition = anchoredPosition;
		}
	}
}
