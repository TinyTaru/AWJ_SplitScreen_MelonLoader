using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageRaycast : MonoBehaviour
{
	private void Start()
	{
		Image component = GetComponent<Image>();
		if (component != null)
		{
			component.alphaHitTestMinimumThreshold = 0.1f;
		}
	}
}
