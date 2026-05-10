using UnityEngine;

namespace _Scripts.Objects;

public class MagneticJointHandler : MonoBehaviour
{
	[SerializeField]
	private MagneticLock magneticLock;

	private void OnJointBreak(float breakForce)
	{
		if (!(magneticLock == null))
		{
			magneticLock.OnJointBreak(breakForce);
		}
	}
}
