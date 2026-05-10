using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIVirtualTouchZone : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IDragHandler, IPointerUpHandler
{
	[Header("Rect References")]
	public RectTransform containerRect;

	public RectTransform handleRect;

	[Header("Settings")]
	public bool clampToMagnitude;

	public float magnitudeMultiplier = 1f;

	public bool invertXOutputValue;

	public bool invertYOutputValue;

	private Vector2 pointerDownPosition;

	private Vector2 currentPointerPosition;

	[Header("Output")]
	public UnityEvent<Vector2> touchZoneOutputEvent;

	private void Start()
	{
		SetupHandle();
	}

	private void SetupHandle()
	{
		if ((bool)handleRect)
		{
			SetObjectActiveState(handleRect.gameObject, newState: false);
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out pointerDownPosition);
		if ((bool)handleRect)
		{
			SetObjectActiveState(handleRect.gameObject, newState: true);
			UpdateHandleRectPosition(pointerDownPosition);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out currentPointerPosition);
		Vector2 deltaBetweenPositions = GetDeltaBetweenPositions(pointerDownPosition, currentPointerPosition);
		Vector2 position = ClampValuesToMagnitude(deltaBetweenPositions);
		Vector2 vector = ApplyInversionFilter(position);
		OutputPointerEventValue(vector * magnitudeMultiplier);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		pointerDownPosition = Vector2.zero;
		currentPointerPosition = Vector2.zero;
		OutputPointerEventValue(Vector2.zero);
		if ((bool)handleRect)
		{
			SetObjectActiveState(handleRect.gameObject, newState: false);
			UpdateHandleRectPosition(Vector2.zero);
		}
	}

	private void OutputPointerEventValue(Vector2 pointerPosition)
	{
		touchZoneOutputEvent.Invoke(pointerPosition);
	}

	private void UpdateHandleRectPosition(Vector2 newPosition)
	{
		handleRect.anchoredPosition = newPosition;
	}

	private void SetObjectActiveState(GameObject targetObject, bool newState)
	{
		targetObject.SetActive(newState);
	}

	private Vector2 GetDeltaBetweenPositions(Vector2 firstPosition, Vector2 secondPosition)
	{
		return secondPosition - firstPosition;
	}

	private Vector2 ClampValuesToMagnitude(Vector2 position)
	{
		return Vector2.ClampMagnitude(position, 1f);
	}

	private Vector2 ApplyInversionFilter(Vector2 position)
	{
		if (invertXOutputValue)
		{
			position.x = InvertValue(position.x);
		}
		if (invertYOutputValue)
		{
			position.y = InvertValue(position.y);
		}
		return position;
	}

	private float InvertValue(float value)
	{
		return 0f - value;
	}
}
