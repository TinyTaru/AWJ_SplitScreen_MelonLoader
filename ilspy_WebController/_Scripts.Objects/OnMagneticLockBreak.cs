using UnityEngine;

namespace _Scripts.Objects;

public class OnMagneticLockBreak : MonoBehaviour
{
	[SerializeField]
	private MagneticLock magneticLock;

	private void OnJointBreak(float breakForce)
	{
		magneticLock.OnJointBreak(breakForce);
	}
}
