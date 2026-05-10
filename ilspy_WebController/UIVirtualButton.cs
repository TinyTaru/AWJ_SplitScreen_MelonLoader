using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIVirtualButton : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler
{
	[Header("Output")]
	public UnityEvent<bool> buttonStateOutputEvent;

	public UnityEvent buttonClickOutputEvent;

	public void OnPointerDown(PointerEventData eventData)
	{
		OutputButtonStateValue(buttonState: true);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		OutputButtonStateValue(buttonState: false);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OutputButtonClickEvent();
	}

	private void OutputButtonStateValue(bool buttonState)
	{
		buttonStateOutputEvent.Invoke(buttonState);
	}

	private void OutputButtonClickEvent()
	{
		buttonClickOutputEvent.Invoke();
	}
}
