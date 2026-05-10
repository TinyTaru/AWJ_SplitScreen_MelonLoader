using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.UI.Wardrobe._3._0;

namespace _Scripts.UI.ScrollView;

public class IndexedVerticalScrollController : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private Button exitButton;

	[Header("Scroll View")]
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransform viewport;

	[SerializeField]
	[Min(0f)]
	private float stepPixels;

	[SerializeField]
	private bool invertWheel;

	[SerializeField]
	[Min(0f)]
	private float heldScrollMinInterval = 0.08f;

	[Header("Viewport Logic")]
	[SerializeField]
	[Min(1f)]
	private int visibleSlots = 4;

	[SerializeField]
	[Min(0f)]
	private int bottomBuffer;

	[SerializeField]
	[Min(0f)]
	private int topBuffer = 1;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference navigateAction;

	[SerializeField]
	private InputActionReference scrollWheelAction;

	private readonly List<OutfitButton> outfitButtons = new List<OutfitButton>();

	private float lastHeldScrollAt = -999f;

	private bool navigateHeld;

	private float maxScrollable;

	private float holdRepeatDelay = 0.35f;

	private float holdRepeatRate = 0.08f;

	private float nextHoldTime;

	private int heldDirection;

	private void OnEnable()
	{
		OutfitButton.OnOutfitSelected += OutfitButton_OnOutfitSelected;
		ScrollToTop();
		navigateAction.action.performed += OnNavigatePerformed;
		navigateAction.action.canceled += OnNavigateCanceled;
		scrollWheelAction.action.performed += OnScrollWheel;
		StartCoroutine(CoNextFrameRecalc());
	}

	private void OnDisable()
	{
		OutfitButton.OnOutfitSelected -= OutfitButton_OnOutfitSelected;
		navigateAction.action.started -= OnNavigatePerformed;
		navigateAction.action.canceled -= OnNavigateCanceled;
		scrollWheelAction.action.performed -= OnScrollWheel;
		navigateHeld = false;
	}

	private void Update()
	{
		if (navigateHeld && !(Time.unscaledTime < nextHoldTime))
		{
			nextHoldTime = Time.unscaledTime + holdRepeatRate;
			MoveOneStepIfThreshold(heldDirection);
		}
	}

	private IEnumerator CoNextFrameRecalc()
	{
		yield return null;
		RecalculateScrollable();
		AutoInitStepIfNeeded();
		if (outfitButtons.Count > 0)
		{
			outfitButtons[0].Select();
		}
		else
		{
			exitButton.Select();
		}
	}

	private IEnumerator CoNextFrameAlignTop()
	{
		if (base.isActiveAndEnabled)
		{
			yield return null;
			if (content != null)
			{
				content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
			}
		}
	}

	private void MoveSteps(int direction, int steps)
	{
		if (steps > 0)
		{
			RecalculateScrollable();
			float num = Mathf.Max(1f, stepPixels);
			float y = Mathf.Clamp(content.anchoredPosition.y + (float)direction * num * (float)steps, 0f, maxScrollable);
			content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
		}
	}

	private void EnsureIndexVisible(int index)
	{
		if (outfitButtons.Count == 0)
		{
			return;
		}
		int topIndex = GetTopIndex();
		int num = topIndex + visibleSlots - 1;
		int num2 = topIndex + topBuffer;
		int num3 = num - bottomBuffer;
		if (index < num2 || index > num3)
		{
			if (index < num2)
			{
				int num4 = Mathf.Clamp(index - topBuffer, 0, Mathf.Max(0, outfitButtons.Count - visibleSlots));
				int steps = Mathf.Max(0, topIndex - num4);
				MoveSteps(-1, steps);
			}
			else if (index > num3)
			{
				int num5 = Mathf.Clamp(index - (visibleSlots - bottomBuffer - 1), 0, Mathf.Max(0, outfitButtons.Count - visibleSlots));
				int steps2 = Mathf.Max(0, num5 - topIndex);
				MoveSteps(1, steps2);
			}
		}
	}

	private bool TryGetSelectedOutfitIndex(out int selectedIndex)
	{
		selectedIndex = -1;
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		if (currentSelectedGameObject == null)
		{
			return false;
		}
		OutfitButton componentInParent = currentSelectedGameObject.GetComponentInParent<OutfitButton>();
		if (componentInParent == null)
		{
			return false;
		}
		int num = outfitButtons.IndexOf(componentInParent);
		if (num < 0)
		{
			return false;
		}
		selectedIndex = num;
		return true;
	}

	private int GetTopIndex()
	{
		float num = Mathf.Max(1f, stepPixels);
		int value = Mathf.FloorToInt(content.anchoredPosition.y / num);
		int max = Mathf.Max(0, outfitButtons.Count - visibleSlots);
		return Mathf.Clamp(value, 0, max);
	}

	private void MoveOneStepIfThreshold(int direction)
	{
		if (outfitButtons.Count <= visibleSlots || !TryGetSelectedOutfitIndex(out var selectedIndex))
		{
			return;
		}
		int topIndex = GetTopIndex();
		if (direction > 0)
		{
			int num = topIndex + (visibleSlots - bottomBuffer - 1);
			if (selectedIndex >= num)
			{
				MoveOneStep(1);
			}
		}
		else if (direction < 0)
		{
			int num2 = topIndex + topBuffer;
			if (selectedIndex <= num2)
			{
				MoveOneStep(-1);
			}
		}
	}

	private void MoveOneStep(int direction)
	{
		RecalculateScrollable();
		float num = Mathf.Max(1f, stepPixels);
		float y = Mathf.Clamp(content.anchoredPosition.y + (float)direction * num, 0f, maxScrollable);
		content.anchoredPosition = new Vector2(content.anchoredPosition.x, y);
	}

	private void RecalculateScrollable()
	{
		float num = Mathf.Max(0f, content.rect.height);
		float num2 = Mathf.Max(0f, viewport.rect.height);
		maxScrollable = Mathf.Max(0f, num - num2);
	}

	private void AutoInitStepIfNeeded()
	{
		if (stepPixels > 0f)
		{
			return;
		}
		float num = 0f;
		if (content != null && content.childCount > 0)
		{
			RectTransform rectTransform = content.GetChild(0) as RectTransform;
			if (rectTransform != null)
			{
				num = Mathf.Abs(rectTransform.rect.height);
			}
		}
		stepPixels = ((num > 0f) ? num : 100f);
	}

	public void RegisterOutfitButtons(List<OutfitButton> buttons)
	{
		outfitButtons.Clear();
		outfitButtons.AddRange(buttons);
	}

	public void ScrollToTop()
	{
		if (scrollRect != null)
		{
			scrollRect.verticalNormalizedPosition = 1f;
		}
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(CoNextFrameAlignTop());
		}
	}

	private void OutfitButton_OnOutfitSelected(OutfitButton btn)
	{
		int num = outfitButtons.IndexOf(btn);
		if (num >= 0 && navigateHeld)
		{
			float unscaledTime = Time.unscaledTime;
			if (!(unscaledTime - lastHeldScrollAt < heldScrollMinInterval))
			{
				lastHeldScrollAt = unscaledTime;
				EnsureIndexVisible(num);
			}
		}
	}

	private void OnNavigatePerformed(InputAction.CallbackContext ctx)
	{
		Vector2 vector = ctx.ReadValue<Vector2>();
		if (!(Mathf.Abs(vector.y) < 0.5f))
		{
			int direction = ((!(vector.y > 0f)) ? 1 : (-1));
			MoveOneStepIfThreshold(direction);
			navigateHeld = true;
		}
	}

	private void OnNavigateStarted(InputAction.CallbackContext ctx)
	{
		Vector2 vector = ctx.ReadValue<Vector2>();
		if (!(Mathf.Abs(vector.y) < 0.5f))
		{
			int direction = ((!(vector.y > 0f)) ? 1 : (-1));
			MoveOneStepIfThreshold(direction);
			navigateHeld = true;
			heldDirection = direction;
			nextHoldTime = Time.unscaledTime + holdRepeatDelay;
		}
	}

	private void OnNavigateCanceled(InputAction.CallbackContext ctx)
	{
		navigateHeld = false;
		heldDirection = 0;
	}

	private void OnScrollWheel(InputAction.CallbackContext ctx)
	{
		Vector2 vector = ctx.ReadValue<Vector2>();
		if (!(Mathf.Abs(vector.y) < 0.01f))
		{
			int direction = ((!(vector.y > 0f)) ? ((!invertWheel) ? 1 : (-1)) : (invertWheel ? 1 : (-1)));
			MoveOneStep(direction);
		}
	}
}
