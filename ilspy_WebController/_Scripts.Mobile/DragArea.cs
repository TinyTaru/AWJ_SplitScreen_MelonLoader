using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace _Scripts.Mobile;

public class DragArea : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerMoveHandler
{
	[SerializeField]
	private UnityEngine.Camera cam;

	[SerializeField]
	private UnityEvent<Vector2> onMove;

	[SerializeField]
	private UnityEvent onPointerDown;

	[SerializeField]
	private UnityEvent onPointerUp;

	[SerializeField]
	private float sensitivity;

	private bool pointerDown;

	private Vector2 currentPosition;

	private Vector2 oldPosition;

	private int pointerId;

	private void Start()
	{
		pointerDown = false;
	}

	private void Update()
	{
		if (pointerDown)
		{
			Vector2 vector = (currentPosition - oldPosition) / Screen.width;
			oldPosition = currentPosition;
			onMove.Invoke(vector * sensitivity);
		}
	}

	private void PrintRectTransform()
	{
		Debug.Log($"anchoredPosition {GetComponent<RectTransform>().anchoredPosition}");
		Debug.Log($"sizeDelta {GetComponent<RectTransform>().sizeDelta}");
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!pointerDown)
		{
			pointerId = eventData.pointerId;
			oldPosition = eventData.position;
			currentPosition = oldPosition;
			pointerDown = true;
			onPointerDown.Invoke();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.pointerId == pointerId)
		{
			onMove.Invoke(Vector2.zero);
			pointerDown = false;
			onPointerUp.Invoke();
		}
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		if (eventData.pointerId == pointerId)
		{
			currentPosition = eventData.position;
		}
	}

	public void Reset()
	{
		onMove.Invoke(Vector2.zero);
		pointerDown = false;
	}
}
