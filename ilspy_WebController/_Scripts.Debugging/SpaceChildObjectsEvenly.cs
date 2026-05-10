using UnityEngine;

namespace _Scripts.Debugging;

public class SpaceChildObjectsEvenly : MonoBehaviour
{
	[SerializeField]
	private Vector3 direction = Vector3.right;

	[SerializeField]
	public float spacing = 1f;

	[SerializeField]
	private bool isCentered;

	private void PositionChildObjects()
	{
		Vector3 vector = Vector3.zero;
		if (isCentered)
		{
			vector = (float)(-base.transform.childCount) / 2f * spacing * direction;
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			base.transform.GetChild(i).localPosition = (float)i * spacing * direction + vector;
		}
	}
}
