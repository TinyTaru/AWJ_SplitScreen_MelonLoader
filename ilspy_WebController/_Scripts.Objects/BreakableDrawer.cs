using System.Collections;
using UnityEngine;

namespace _Scripts.Objects;

public class BreakableDrawer : MonoBehaviour
{
	[SerializeField]
	private float breakForce = 5000f;

	[SerializeField]
	private float breakTorque = float.PositiveInfinity;

	[SerializeField]
	private float delay = 5f;

	private ConfigurableJoint joint;

	private void Awake()
	{
		joint = GetComponent<ConfigurableJoint>();
	}

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(delay);
		if (!(joint == null))
		{
			joint.breakForce = breakForce;
			joint.breakTorque = breakTorque;
		}
	}
}
