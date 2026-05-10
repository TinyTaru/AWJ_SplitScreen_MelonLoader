using UnityEngine;

namespace _Scripts.Puzzles;

public class WoodenGate : MonoBehaviour
{
	[SerializeField]
	private Transform gate;

	[SerializeField]
	private Rope rope;

	[SerializeField]
	private float offset;

	[SerializeField]
	private float minHeight;

	[SerializeField]
	private float maxHeight;

	private void FixedUpdate()
	{
		float num = Mathf.Clamp(rope.GetLength() + offset, minHeight, maxHeight);
		Vector3 localPosition = Vector3.up * num;
		gate.localPosition = localPosition;
	}
}
