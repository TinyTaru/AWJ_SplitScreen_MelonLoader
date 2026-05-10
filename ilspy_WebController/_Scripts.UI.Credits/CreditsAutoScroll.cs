using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.UI.Credits;

public class CreditsAutoScroll : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransform contentWindow;

	[Header("Parameters")]
	[SerializeField]
	private float autoScrollSpeed = 0.1f;

	[SerializeField]
	private float gamepadScrollSpeed = 0.1f;

	[SerializeField]
	private float pauseDuration = 2.5f;

	[SerializeField]
	private float autoScrollDelay = 1f;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference mouseScrollInputAction;

	[SerializeField]
	private InputActionReference gamepadScrollInputAction;

	[SerializeField]
	private InputActionReference mouseClickInputAction;

	private float userScrollCooldown;

	private bool userScrolling;

	private Vector2 gamepadScrollInput;

	private float scrollRectVerticalPosition;

	private void OnEnable()
	{
		userScrolling = true;
		userScrollCooldown = autoScrollDelay;
		StartCoroutine(ResetScrollRectCoroutine());
		gamepadScrollInputAction.action.performed += OnGamepadScrollInputActionPerformed;
	}

	private void Update()
	{
		GetUserScrollInput();
		if (userScrolling)
		{
			HandleUserScroll();
		}
		else
		{
			HandleAutoScroll();
		}
	}

	private void OnDisable()
	{
		gamepadScrollInputAction.action.performed -= OnGamepadScrollInputActionPerformed;
	}

	private IEnumerator ResetScrollRectCoroutine()
	{
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
		scrollRectVerticalPosition = 1f;
		scrollRect.verticalNormalizedPosition = scrollRectVerticalPosition;
	}

	private void GetUserScrollInput()
	{
		if (mouseScrollInputAction.action.WasPerformedThisFrame() || Mathf.Abs(gamepadScrollInput.y) > 0.1f || mouseClickInputAction.action.IsPressed())
		{
			userScrolling = true;
			userScrollCooldown = pauseDuration;
		}
	}

	private void HandleUserScroll()
	{
		if (Mathf.Abs(gamepadScrollInput.y) > 0.1f)
		{
			scrollRectVerticalPosition += gamepadScrollInput.y * gamepadScrollSpeed * Time.deltaTime;
			scrollRectVerticalPosition = Mathf.Clamp01(scrollRectVerticalPosition);
			scrollRect.verticalNormalizedPosition = scrollRectVerticalPosition;
		}
		userScrollCooldown -= Time.unscaledDeltaTime;
		if (userScrollCooldown <= 0f)
		{
			scrollRectVerticalPosition = scrollRect.verticalNormalizedPosition;
			userScrolling = false;
		}
	}

	private void HandleAutoScroll()
	{
		if (!(scrollRectVerticalPosition <= 0f))
		{
			scrollRectVerticalPosition -= autoScrollSpeed * Time.unscaledDeltaTime;
			scrollRectVerticalPosition = Mathf.Clamp01(scrollRectVerticalPosition);
			scrollRect.verticalNormalizedPosition = scrollRectVerticalPosition;
		}
	}

	private void OnGamepadScrollInputActionPerformed(InputAction.CallbackContext obj)
	{
		gamepadScrollInput = obj.action.ReadValue<Vector2>();
	}
}
