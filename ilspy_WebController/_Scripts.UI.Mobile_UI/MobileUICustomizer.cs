using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.UI.Panels;

namespace _Scripts.UI.Mobile_UI;

public class MobileUICustomizer : MonoBehaviour
{
	[Header("Buttons")]
	[SerializeField]
	private RectTransform[] buttons;

	[SerializeField]
	private RectTransform[] defaults;

	[Header("Slider Elements")]
	[SerializeField]
	private Slider sizeSlider;

	[SerializeField]
	private CanvasGroup sliderContainer;

	[Header("Size Limits")]
	[SerializeField]
	private float minButtonSize = 0.5f;

	[SerializeField]
	private float maxButtonSize = 2f;

	[Header("Thresholds")]
	[SerializeField]
	private float dragThreshold = 10f;

	[SerializeField]
	private RectTransform safeArea;

	[Header("Colors")]
	[SerializeField]
	private Color activeColor;

	[SerializeField]
	private Color passiveColor;

	[Header("InputActions")]
	[SerializeField]
	private InputActionReference pointerPosition;

	[SerializeField]
	private InputActionReference pointerClick;

	private Dictionary<string, Vector2> defaultPositions = new Dictionary<string, Vector2>();

	private Dictionary<string, float> buttonSizes = new Dictionary<string, float>();

	private bool isDragging;

	private RectTransform draggedButton;

	private RectTransform previouslySelectedButton;

	private Vector2 dragOffset;

	private PanelManager panelManager;

	private Vector2 initialMousePosition;

	public static event Action OnButtonCustomizationSaved;

	private void Awake()
	{
		panelManager = GetComponentInParent<PanelManager>();
		RectTransform[] array = defaults;
		foreach (RectTransform rectTransform in array)
		{
			defaultPositions[rectTransform.name] = rectTransform.anchoredPosition;
		}
	}

	private void OnEnable()
	{
		RectTransform[] array = buttons;
		foreach (RectTransform rectTransform in array)
		{
			Vector2 buttonPosition = Singleton<MobileCustomizationController>.Instance.GetButtonPosition(rectTransform.name);
			if (buttonPosition != Vector2.zero)
			{
				rectTransform.anchoredPosition = buttonPosition;
			}
			float buttonSize = Singleton<MobileCustomizationController>.Instance.GetButtonSize(rectTransform.name);
			if (buttonSize > 1f || buttonSize < 1f)
			{
				UpdateButtonSize(buttonSize, rectTransform);
			}
		}
		sliderContainer.alpha = 0f;
	}

	private void Update()
	{
		Vector2 activePos = pointerPosition.action.ReadValue<Vector2>();
		if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
		{
			activePos = Touchscreen.current.primaryTouch.position.ReadValue();
			if (!isDragging)
			{
				StartDrag(activePos);
			}
		}
		else if (pointerClick.action.WasPressedThisFrame() && !isDragging)
		{
			StartDrag(activePos);
		}
		if (Touchscreen.current != null && !Touchscreen.current.primaryTouch.press.isPressed)
		{
			if (isDragging)
			{
				EndDrag();
			}
		}
		else if (pointerClick.action.WasReleasedThisFrame() && isDragging)
		{
			EndDrag();
		}
		if (isDragging)
		{
			DragButton(activePos);
		}
	}

	private void StartDrag(Vector2 activePos)
	{
		RectTransform[] array = buttons;
		foreach (RectTransform rectTransform in array)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, activePos, null))
			{
				SelectButton(rectTransform);
				sliderContainer.alpha = 0f;
				sizeSlider.interactable = false;
				isDragging = true;
				draggedButton = rectTransform;
				initialMousePosition = activePos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(draggedButton.parent as RectTransform, activePos, null, out var localPoint);
				dragOffset = draggedButton.anchoredPosition - localPoint;
				break;
			}
		}
	}

	private void DragButton(Vector2 activePos)
	{
		if (Vector2.Distance(initialMousePosition, activePos) > dragThreshold)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(draggedButton.parent as RectTransform, activePos, null, out var localPoint);
			Vector2 targetPosition = localPoint + dragOffset;
			targetPosition = ClampToSafeArea(targetPosition, draggedButton);
			draggedButton.anchoredPosition = targetPosition;
		}
	}

	private Vector2 ClampToSafeArea(Vector2 targetPosition, RectTransform button)
	{
		Vector2 vector = new Vector2(button.rect.width * button.localScale.x, button.rect.height * button.localScale.y);
		Vector2 min = safeArea.rect.min;
		Vector2 max = safeArea.rect.max;
		float num = 120f;
		float num2 = 120f;
		float num3 = Mathf.Clamp(targetPosition.x, min.x + vector.x / 2f, max.x - vector.x / 2f);
		float num4 = Mathf.Clamp(targetPosition.y, min.y + vector.y / 2f, max.y - vector.y / 2f);
		if (num3 - vector.x / 2f < min.x + num && num4 + vector.y / 2f > max.y - num2)
		{
			if (Mathf.Abs(num3 - (min.x + num)) > Mathf.Abs(num4 - (max.y - num2)))
			{
				num4 = max.y - num2 - vector.y / 2f;
			}
			else
			{
				num3 = min.x + num + vector.x / 2f;
			}
		}
		return new Vector2(num3, num4);
	}

	private void EndDrag()
	{
		isDragging = false;
		if (draggedButton.name != "JoystickMoveArea")
		{
			AssignSizeSlider(draggedButton);
			sliderContainer.alpha = 1f;
			sizeSlider.interactable = true;
		}
		draggedButton = null;
	}

	private void AssignSizeSlider(RectTransform button)
	{
		sizeSlider.onValueChanged.RemoveAllListeners();
		float x = button.localScale.x;
		float maxValue = Mathf.Min(maxButtonSize, GetMaxAllowedScale(button));
		sizeSlider.minValue = minButtonSize;
		sizeSlider.maxValue = maxValue;
		sizeSlider.value = x;
		sizeSlider.onValueChanged.AddListener(delegate(float newSize)
		{
			UpdateButtonSize(newSize, button);
		});
	}

	private float GetMaxAllowedScale(RectTransform button)
	{
		Vector2 vector = new Vector2(button.rect.width, button.rect.height);
		Vector2 anchoredPosition = button.anchoredPosition;
		Vector2 min = safeArea.rect.min;
		Vector2 max = safeArea.rect.max;
		float a = Mathf.Abs(anchoredPosition.x - min.x);
		float b = Mathf.Abs(max.x - anchoredPosition.x);
		float b2 = Mathf.Abs(anchoredPosition.y - min.y);
		float a2 = Mathf.Abs(max.y - anchoredPosition.y);
		float a3 = Mathf.Min(a, b) / (vector.x / 2f);
		float b3 = Mathf.Min(a2, b2) / (vector.y / 2f);
		return Mathf.Min(a3, b3);
	}

	private void UpdateButtonSize(float newSize, RectTransform button)
	{
		float maxAllowedScale = GetMaxAllowedScale(button);
		newSize = Mathf.Min(newSize, maxAllowedScale);
		button.localScale = new Vector3(newSize, newSize, 1f);
		buttonSizes[button.name] = newSize;
		Vector2 anchoredPosition = ClampToSafeArea(button.anchoredPosition, button);
		button.anchoredPosition = anchoredPosition;
	}

	private void SelectButton(RectTransform button)
	{
		if (previouslySelectedButton != null && previouslySelectedButton != button)
		{
			previouslySelectedButton.GetComponentInParent<Image>().color = passiveColor;
		}
		previouslySelectedButton = button;
		button.GetComponentInParent<Image>().color = activeColor;
	}

	public void GoBack()
	{
		panelManager.GoBack();
	}

	public void SaveButtonProperties()
	{
		RectTransform[] array = buttons;
		foreach (RectTransform rectTransform in array)
		{
			Singleton<MobileCustomizationController>.Instance.SaveButtonPosition(rectTransform.name, rectTransform.anchoredPosition);
			Singleton<MobileCustomizationController>.Instance.SaveButtonSize(rectTransform.name, rectTransform.localScale.x);
		}
		DeselectAllButtons();
		sliderContainer.alpha = 0f;
		MobileUICustomizer.OnButtonCustomizationSaved?.Invoke();
	}

	public void ResetToDefault()
	{
		RectTransform[] array = buttons;
		foreach (RectTransform rectTransform in array)
		{
			RectTransform[] array2 = defaults;
			foreach (RectTransform rectTransform2 in array2)
			{
				if (rectTransform.name == rectTransform2.name)
				{
					rectTransform.anchoredPosition = defaultPositions[rectTransform.name];
					float x = rectTransform2.localScale.x;
					UpdateButtonSize(x, rectTransform);
				}
			}
		}
		DeselectAllButtons();
		sliderContainer.alpha = 0f;
	}

	public void DeselectAllButtons()
	{
		RectTransform[] array = buttons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponentInParent<Image>().color = passiveColor;
		}
		sliderContainer.alpha = 0f;
		previouslySelectedButton = null;
	}

	public void PrintPositions()
	{
		RectTransform[] array = buttons;
		foreach (RectTransform rectTransform in array)
		{
			Debug.Log("Button name: " + rectTransform.name + ", Position: " + Singleton<MobileCustomizationController>.Instance.GetButtonPosition(rectTransform.name).ToString());
		}
	}
}
