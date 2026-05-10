using UnityEngine;

namespace Cinemachine.Examples;

[AddComponentMenu("")]
public class ActivateCamOnPlay : MonoBehaviour
{
	public CinemachineVirtualCameraBase vcam;

	private void Start()
	{
		if ((bool)vcam)
		{
			vcam.MoveToTopOfPrioritySubqueue();
		}
	}
}
