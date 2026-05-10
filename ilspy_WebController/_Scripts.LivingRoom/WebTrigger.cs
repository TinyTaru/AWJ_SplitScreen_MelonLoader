using UnityEngine;
using UnityEngine.Events;
using _Scripts.Web;

namespace _Scripts.LivingRoom;

public class WebTrigger : MonoBehaviour
{
	[SerializeField]
	private UnityEvent<WebThread> onWebTouchedEvent;

	private void OnTriggerEnter(Collider other)
	{
		WebThread componentInParent = other.GetComponentInParent<WebThread>();
		if (componentInParent != null)
		{
			Debug.Log("WebThread touched: " + componentInParent.name);
			onWebTouchedEvent?.Invoke(componentInParent);
		}
	}
}
