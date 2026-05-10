using UnityEngine;

namespace Cinemachine.Examples;

public class ScriptingExample : MonoBehaviour
{
	private CinemachineVirtualCamera vcam;

	private CinemachineFreeLook freelook;

	private float lastSwapTime;

	private void Start()
	{
		CinemachineBrain cinemachineBrain = GameObject.Find("Main Camera").AddComponent<CinemachineBrain>();
		cinemachineBrain.m_ShowDebugText = true;
		cinemachineBrain.m_DefaultBlend.m_Time = 1f;
		vcam = new GameObject("VirtualCamera").AddComponent<CinemachineVirtualCamera>();
		vcam.m_LookAt = GameObject.Find("Cube").transform;
		vcam.m_Priority = 10;
		vcam.gameObject.transform.position = new Vector3(0f, 1f, 0f);
		CinemachineComposer cinemachineComposer = vcam.AddCinemachineComponent<CinemachineComposer>();
		cinemachineComposer.m_ScreenX = 0.3f;
		cinemachineComposer.m_ScreenY = 0.35f;
		freelook = new GameObject("Follow").AddComponent<CinemachineFreeLook>();
		freelook.m_LookAt = GameObject.Find("Cylinder/Sphere").transform;
		freelook.m_Follow = GameObject.Find("Cylinder").transform;
		freelook.m_Priority = 11;
		CinemachineVirtualCamera rig = freelook.GetRig(0);
		CinemachineVirtualCamera rig2 = freelook.GetRig(1);
		CinemachineVirtualCamera rig3 = freelook.GetRig(2);
		rig.GetCinemachineComponent<CinemachineComposer>().m_ScreenY = 0.35f;
		rig2.GetCinemachineComponent<CinemachineComposer>().m_ScreenY = 0.25f;
		rig3.GetCinemachineComponent<CinemachineComposer>().m_ScreenY = 0.15f;
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup - lastSwapTime > 5f)
		{
			freelook.enabled = !freelook.enabled;
			lastSwapTime = Time.realtimeSinceStartup;
		}
	}
}
