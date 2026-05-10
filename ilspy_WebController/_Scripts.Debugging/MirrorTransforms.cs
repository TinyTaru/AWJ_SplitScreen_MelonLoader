using UnityEngine;

namespace _Scripts.Debugging;

public class MirrorTransforms : MonoBehaviour
{
	[SerializeField]
	private Transform original;

	[SerializeField]
	private Transform mirrored;

	private void MirrorEar()
	{
		Vector3 localPosition = original.localPosition;
		localPosition.x = 0f - localPosition.x;
		mirrored.localPosition = localPosition;
		Vector3 vector = new Vector3(0f - original.up.x, original.up.y, original.up.z);
		Vector3 vector2 = Vector3.Cross(new Vector3(0f - original.right.x, original.right.y, original.right.z), vector);
		mirrored.LookAt(mirrored.position + vector2, vector);
	}

	private void MirrorLeg()
	{
		Vector3 localPosition = original.localPosition;
		localPosition.x = 0f - localPosition.x;
		mirrored.localPosition = localPosition;
		Vector3 vector = new Vector3(0f - original.forward.x, original.forward.y, original.forward.z);
		Vector3 worldUp = new Vector3(0f - original.up.x, original.up.y, original.up.z);
		mirrored.LookAt(mirrored.position + vector, worldUp);
	}
}
