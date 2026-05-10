using UnityEngine;

namespace SplineMesh;

public static class CameraUtility
{
	public static bool IsOnScreen(Vector3 position)
	{
		Vector3 vector = Camera.current.WorldToViewportPoint(position);
		if (vector.z > 0f && vector.x > 0f && vector.y > 0f && vector.x < 1f)
		{
			return vector.y < 1f;
		}
		return false;
	}
}
