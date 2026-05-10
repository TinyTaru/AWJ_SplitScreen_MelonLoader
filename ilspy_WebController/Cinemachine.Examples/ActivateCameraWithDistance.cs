using UnityEngine;

namespace Cinemachine.Examples;

[AddComponentMenu("")]
public class ActivateCameraWithDistance : MonoBehaviour
{
	public GameObject objectToCheck;

	public float distanceToObject = 15f;

	public CinemachineVirtualCameraBase initialActiveCam;

	public CinemachineVirtualCameraBase switchCameraTo;

	private CinemachineBrain brain;

	private void Start()
	{
		brain = Camera.main.GetComponent<CinemachineBrain>();
		SwitchCam(initialActiveCam);
	}

	private void Update()
	{
		if ((bool)objectToCheck && (bool)switchCameraTo)
		{
			if (Vector3.Distance(base.transform.position, objectToCheck.transform.position) < distanceToObject)
			{
				SwitchCam(switchCameraTo);
			}
			else
			{
				SwitchCam(initialActiveCam);
			}
		}
	}

	public void SwitchCam(CinemachineVirtualCameraBase vcam)
	{
		if (!(brain == null) && !(vcam == null) && brain.ActiveVirtualCamera != vcam)
		{
			vcam.MoveToTopOfPrioritySubqueue();
		}
	}
}
