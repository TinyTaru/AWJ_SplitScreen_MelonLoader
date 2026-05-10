using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.UI.ScrollView;

public class ScrollRectPosition : MonoBehaviour
{
	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference mouseScrollInputAction;

	[Header("Scrolling Settings")]
	[SerializeField]
	private float scrollStep = 108f;

	[SerializeField]
	private float controllerScrollThreshold = 0.5f;

	private RectTransform scrollRectTransform;

	private RectTransform contentPanel;

	private RectTransform selectedRectTransform;

	private GameObject lastSelected;

	private ScrollRect scrollRect;

	private bool controllerStickEngaged;

	private bool jumpToSelectedNextFrame;

	private void Start()
	{
		scrollRect = GetComponent<ScrollRect>();
		scrollRectTransform = GetComponent<RectTransform>();
		contentPanel = scrollRect.content;
	}

	private void OnEnable()
	{
		mouseScrollInputAction.action.performed += OnMouseScroll;
		lastSelected = null;
		jumpToSelectedNextFrame = true;
	}

	private void OnDisable()
	{
		mouseScrollInputAction.action.performed -= OnMouseScroll;
	}

	private void Update()
	{
		if (jumpToSelectedNextFrame)
		{
			jumpToSelectedNextFrame = false;
			ForceJumpToCurrentlySelected();
		}
		else
		{
			HandleScrollToSelection();
		}
	}

	private void HandleScrollToSelection()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (!(currentSelectedGameObject == null) && !(currentSelectedGameObject.transform.parent != contentPanel.transform) && !(currentSelectedGameObject == lastSelected))
		{
			selectedRectTransform = currentSelectedGameObject.GetComponent<RectTransform>();
			float num = Mathf.Abs(selectedRectTransform.anchoredPosition.y);
			float num2 = num + selectedRectTransform.rect.height;
			float y = contentPanel.anchoredPosition.y;
			float num3 = contentPanel.anchoredPosition.y + scrollRectTransform.rect.height;
			if (num2 > num3)
			{
				contentPanel.anchoredPosition += new Vector2(0f, scrollStep);
			}
			else if (num < y)
			{
				contentPanel.anchoredPosition -= new Vector2(0f, scrollStep);
			}
			lastSelected = currentSelectedGameObject;
		}
	}

	private void ForceJumpToCurrentlySelected()
	{
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (!(currentSelectedGameObject == null) && !(currentSelectedGameObject.transform.parent != contentPanel.transform))
		{
			RectTransform component = currentSelectedGameObject.GetComponent<RectTransform>();
			float selectedY = Mathf.Abs(component.anchoredPosition.y);
			float height = component.rect.height;
			JumpToSelected(selectedY, height);
			lastSelected = currentSelectedGameObject;
		}
	}

	private void JumpToSelected(float selectedY, float selectedHeight)
	{
		float height = scrollRectTransform.rect.height;
		float height2 = contentPanel.rect.height;
		float value = Mathf.Round((selectedY - height / 2f + selectedHeight / 2f) / scrollStep) * scrollStep;
		float max = height2 - height;
		float y = Mathf.Clamp(value, 0f, max);
		contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, y);
	}

	private void OnMouseScroll(InputAction.CallbackContext ctx)
	{
		float y = ctx.ReadValue<Vector2>().y;
		if (Mathf.Abs(y) > 0.1f)
		{
			float direction = Mathf.Sign(y);
			ApplyScroll(direction);
		}
	}

	private bool CanScroll()
	{
		return contentPanel.rect.height > scrollRectTransform.rect.height + 0.1f;
	}

	private void ApplyScroll(float direction)
	{
		if (CanScroll())
		{
			float max = contentPanel.rect.height - scrollRectTransform.rect.height;
			float y = contentPanel.anchoredPosition.y;
			float num = Mathf.Clamp(y + (0f - direction) * scrollStep, 0f, max);
			if (!Mathf.Approximately(num, y))
			{
				contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, num);
			}
		}
	}
}
