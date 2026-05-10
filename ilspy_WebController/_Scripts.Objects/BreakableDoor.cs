using System.Collections;
using UnityEngine;

namespace _Scripts.Objects;

public class BreakableDoor : MonoBehaviour
{
	[SerializeField]
	private float breakForce = 7000f;

	[SerializeField]
	private float breakTorque = float.PositiveInfinity;

	[SerializeField]
	private float delay = 5f;

	private HingeJoint joint;

	private void Awake()
	{
		joint = GetComponent<HingeJoint>();
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
