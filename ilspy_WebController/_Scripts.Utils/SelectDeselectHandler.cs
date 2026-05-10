using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace _Scripts.Utils;

[DisallowMultipleComponent]
public class SelectDeselectHandler : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private UnityEvent onSelect;

	[SerializeField]
	private UnityEvent onDeselect;

	public void OnSelect(BaseEventData eventData)
	{
		onSelect?.Invoke();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		onDeselect?.Invoke();
	}
}
