using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace _Scripts.Mobile;

public class EmoteCancelArea : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	[SerializeField]
	private UnityEvent onPointerDown;

	public void OnPointerDown(PointerEventData eventData)
	{
	}
}
