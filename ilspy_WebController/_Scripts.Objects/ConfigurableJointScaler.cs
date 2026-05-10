using UnityEngine;

namespace _Scripts.Objects;

[RequireComponent(typeof(ConfigurableJoint))]
public class ConfigurableJointScaler : MonoBehaviour
{
	private ConfigurableJoint joint;

	private float originalLimit;

	private void Awake()
	{
		joint = GetComponent<ConfigurableJoint>();
		originalLimit = joint.linearLimit.limit;
		AdjustJointLimit();
	}

	private void AdjustJointLimit()
	{
		if (!(joint == null))
		{
			SoftJointLimit linearLimit = joint.linearLimit;
			linearLimit.limit = originalLimit * base.transform.lossyScale.x;
			joint.linearLimit = linearLimit;
		}
	}
}
