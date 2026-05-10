using Cinemachine;
using UnityEngine;

public class CameraMagnetTargetController : MonoBehaviour
{
	public CinemachineTargetGroup targetGroup;

	private int playerIndex;

	private CameraMagnetProperty[] cameraMagnets;

	private void Start()
	{
		cameraMagnets = GetComponentsInChildren<CameraMagnetProperty>();
		playerIndex = 0;
	}

	private void Update()
	{
		for (int i = 1; i < targetGroup.m_Targets.Length; i++)
		{
			float magnitude = (targetGroup.m_Targets[playerIndex].target.position - targetGroup.m_Targets[i].target.position).magnitude;
			if (magnitude < cameraMagnets[i - 1].Proximity)
			{
				targetGroup.m_Targets[i].weight = cameraMagnets[i - 1].MagnetStrength * (1f - magnitude / cameraMagnets[i - 1].Proximity);
			}
			else
			{
				targetGroup.m_Targets[i].weight = 0f;
			}
		}
	}
}
