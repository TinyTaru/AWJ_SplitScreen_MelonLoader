using UnityEngine;

namespace _Scripts.Puzzles;

public class Rope : MonoBehaviour
{
	[SerializeField]
	private LineRenderer lineRenderer;

	[SerializeField]
	private Transform ropeStart;

	[SerializeField]
	private Transform ropeEnd;

	[SerializeField]
	private CapsuleCollider ropeCollider;

	private Vector3[] positions = new Vector3[2];

	private void FixedUpdate()
	{
		positions[0] = ropeStart.localPosition;
		positions[1] = ropeEnd.localPosition;
		lineRenderer.SetPositions(positions);
		Vector3 localPosition = (positions[0] + positions[1]) / 2f;
		ropeCollider.transform.localPosition = localPosition;
		Vector3 forward = ropeStart.position - ropeEnd.position;
		ropeCollider.transform.rotation = Quaternion.LookRotation(forward);
		ropeCollider.height = forward.magnitude;
	}

	public float GetLength()
	{
		return Vector3.Distance(positions[0], positions[1]);
	}
}
