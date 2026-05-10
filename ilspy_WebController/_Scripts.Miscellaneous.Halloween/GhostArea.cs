using UnityEngine;

namespace _Scripts.Miscellaneous.Halloween;

public class GhostArea : MonoBehaviour
{
	private void OnTriggerExit(Collider other)
	{
		Ghost componentInParent = other.GetComponentInParent<Ghost>();
		if (componentInParent != null)
		{
			componentInParent.DestroySafely();
		}
	}
}
