using System;
using UnityEngine;

namespace Cinemachine.Examples;

[AddComponentMenu("")]
[ExecuteAlways]
public class CinemachineFadeOutNearbyObjects : CinemachineExtension
{
	[Tooltip("Radius of the look at target.")]
	public float m_LookAtTargetRadius = 1f;

	[Tooltip("Minimum distance to have fading out effect in front of the camera.")]
	public float m_MinDistance;

	[Tooltip("Maximum distance to have fading out effect in front of the camera.")]
	public float m_MaxDistance = 8f;

	[Tooltip("If true, MaxDistance will be set to distance between this virtual camera and LookAt target minus LookAtTargetRadius.")]
	public bool m_SetToCameraToLookAtDistance;

	[Tooltip("Material using the FadeOut shader.")]
	public Material m_FadeOutMaterial;

	private static readonly int k_MaxDistanceID = Shader.PropertyToID("_MaxDistance");

	private static readonly int k_MinDistanceID = Shader.PropertyToID("_MinDistance");

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage == CinemachineCore.Stage.Finalize && !(m_FadeOutMaterial == null) && m_FadeOutMaterial.HasProperty(k_MaxDistanceID) && m_FadeOutMaterial.HasProperty(k_MinDistanceID))
		{
			if (m_SetToCameraToLookAtDistance && vcam.LookAt != null)
			{
				m_MaxDistance = Vector3.Distance(vcam.transform.position, vcam.LookAt.position) - m_LookAtTargetRadius;
			}
			m_FadeOutMaterial.SetFloat(k_MaxDistanceID, m_MaxDistance);
			m_FadeOutMaterial.SetFloat(k_MinDistanceID, m_MinDistance);
		}
	}

	private void OnValidate()
	{
		m_LookAtTargetRadius = Math.Max(0f, m_LookAtTargetRadius);
		m_MinDistance = Math.Max(0f, m_MinDistance);
		m_MaxDistance = Math.Max(0f, m_MaxDistance);
	}
}
