using UnityEngine;

namespace _Scripts.UI.ContentFitting;

public class CanvasSettings : MonoBehaviour
{
	[SerializeField]
	private string sortingLayerName;

	[SerializeField]
	private int orderInLayer;

	private void Start()
	{
		Canvas component = GetComponent<Canvas>();
		if (component != null)
		{
			component.sortingLayerName = sortingLayerName;
			component.sortingOrder = orderInLayer;
		}
	}
}
