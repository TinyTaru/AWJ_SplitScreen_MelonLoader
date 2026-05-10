using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.UI.Selections;

namespace _Scripts.UI.Panels;

public class PanelManager : MonoBehaviour
{
	[Header("Animations")]
	[SerializeField]
	private float animationDuration = 0.15f;

	[SerializeField]
	private Ease easingType = Ease.OutQuint;

	[Header("Input Map")]
	[SerializeField]
	private InputActionAsset[] inputActionAssets;

	private Stack<RectTransform> panelStack = new Stack<RectTransform>();

	private bool isAnimating;

	private List<InputActionMap> actionMaps;

	private Sequence showSequence;

	private Sequence hideSequence;

	private float defaultAnimationDuration;

	private float lastAnimationStartedAt;

	private const float checkActionMapPeriod = 1f;

	public bool IsAnimating => isAnimating;

	private void Awake()
	{
		defaultAnimationDuration = animationDuration;
		actionMaps = new List<InputActionMap>();
		InputActionAsset[] array = inputActionAssets;
		foreach (InputActionAsset inputActionAsset in array)
		{
			actionMaps.Add(inputActionAsset.FindActionMap("UI"));
		}
	}

	private void OnDestroy()
	{
		showSequence?.Kill();
		hideSequence?.Kill();
	}

	private void Update()
	{
		if (panelStack.Count != 0 && isAnimating && Time.realtimeSinceStartup > lastAnimationStartedAt + 1f)
		{
			SetIsAnimating(value: false);
		}
	}

	private void CreateShowSequence(RectTransform panel)
	{
		SelectionManager selectionManager = panel.GetComponentInChildren<SelectionManager>();
		CanvasGroup component = panel.GetComponent<CanvasGroup>();
		if (component == null)
		{
			Debug.LogError(panel.name + " needs a CanvasGroup component to be animated!", panel);
			return;
		}
		panel.localScale = Vector3.zero;
		component.alpha = 0f;
		panel.gameObject.SetActive(value: true);
		showSequence?.Kill();
		showSequence = DOTween.Sequence();
		showSequence.SetUpdate(isIndependentUpdate: true);
		showSequence.SetAutoKill(autoKillOnCompletion: false);
		showSequence.Append(component.DOFade(1f, animationDuration).SetEase(easingType));
		showSequence.Join(panel.DOScale(Vector3.one, animationDuration).SetEase(easingType));
		showSequence.AppendCallback(delegate
		{
			if (selectionManager != null)
			{
				selectionManager.UpdateSelection();
			}
			SetIsAnimating(value: false);
		});
	}

	private void CreateHideSequence(RectTransform panel)
	{
		SelectionManager selectionManager = panel.GetComponentInChildren<SelectionManager>();
		CanvasGroup component = panel.GetComponent<CanvasGroup>();
		if (component == null)
		{
			Debug.LogError(panel.name + " needs a CanvasGroup component to be animated!", panel);
			return;
		}
		hideSequence?.Kill();
		hideSequence = DOTween.Sequence();
		hideSequence.SetUpdate(isIndependentUpdate: true);
		hideSequence.SetAutoKill(autoKillOnCompletion: false);
		hideSequence.Append(component.DOFade(0f, animationDuration).SetEase(easingType));
		hideSequence.Join(panel.DOScale(0f, animationDuration).SetEase(easingType));
		hideSequence.AppendCallback(delegate
		{
			panel.gameObject.SetActive(value: false);
			if (selectionManager != null)
			{
				selectionManager.Deselect();
			}
		});
	}

	private void EnableActionMaps()
	{
		foreach (InputActionMap actionMap in actionMaps)
		{
			actionMap.Enable();
		}
	}

	private void DisableActionMaps()
	{
		foreach (InputActionMap actionMap in actionMaps)
		{
			actionMap.Disable();
		}
	}

	private void SetIsAnimating(bool value)
	{
		isAnimating = value;
		if (isAnimating)
		{
			lastAnimationStartedAt = Time.realtimeSinceStartup;
			DisableActionMaps();
		}
		else
		{
			EnableActionMaps();
		}
	}

	public void OpenPanel(RectTransform panel, bool removeCurrentPanelFromStack)
	{
		if (panel == null)
		{
			return;
		}
		if (panelStack.Count == 0)
		{
			CreateShowSequence(panel);
			showSequence.Restart();
		}
		else
		{
			RectTransform rectTransform = panelStack.Peek();
			if (removeCurrentPanelFromStack)
			{
				panelStack.Pop();
			}
			if (Mathf.Approximately(animationDuration, 0f))
			{
				rectTransform.gameObject.SetActive(value: false);
				panel.gameObject.SetActive(value: true);
				SetIsAnimating(value: false);
			}
			else
			{
				SetIsAnimating(value: true);
				CreateHideSequence(rectTransform);
				hideSequence.OnComplete(delegate
				{
					CreateShowSequence(panel);
					showSequence.Restart();
				});
				hideSequence.Restart();
			}
		}
		panelStack.Push(panel);
	}

	public void OpenPanel(RectTransform panel)
	{
		if (panel == null)
		{
			return;
		}
		if (panelStack.Count == 0)
		{
			CreateShowSequence(panel);
			showSequence.Restart();
		}
		else
		{
			RectTransform rectTransform = panelStack.Peek();
			if (Mathf.Approximately(animationDuration, 0f))
			{
				rectTransform.gameObject.SetActive(value: false);
				panel.gameObject.SetActive(value: true);
				SetIsAnimating(value: false);
			}
			else
			{
				SetIsAnimating(value: true);
				CreateHideSequence(rectTransform);
				hideSequence.OnComplete(delegate
				{
					CreateShowSequence(panel);
					showSequence.Restart();
				});
				hideSequence.Restart();
			}
		}
		panelStack.Push(panel);
	}

	public void CloseCurrentPanel(Action onCompleteAction)
	{
		if (panelStack.Count == 0)
		{
			return;
		}
		RectTransform rectTransform = panelStack.Pop();
		if (Mathf.Approximately(animationDuration, 0f))
		{
			rectTransform.gameObject.SetActive(value: false);
			onCompleteAction?.Invoke();
			SetIsAnimating(value: false);
			return;
		}
		SetIsAnimating(value: true);
		CreateHideSequence(rectTransform);
		hideSequence.OnComplete(delegate
		{
			SetIsAnimating(value: false);
			onCompleteAction?.Invoke();
		});
		hideSequence.Restart();
	}

	public void GoBack()
	{
		GoBack(1);
	}

	public void GoBack(int amount = 1)
	{
		if (panelStack.Count <= amount)
		{
			return;
		}
		RectTransform rectTransform = panelStack.Pop();
		for (int i = 0; i < amount - 1; i++)
		{
			panelStack.Pop();
		}
		RectTransform previousPanel = panelStack.Peek();
		if (Mathf.Approximately(animationDuration, 0f))
		{
			rectTransform.gameObject.SetActive(value: false);
			previousPanel.gameObject.SetActive(value: true);
			SetIsAnimating(value: false);
			return;
		}
		SetIsAnimating(value: true);
		CreateHideSequence(rectTransform);
		hideSequence.OnComplete(delegate
		{
			CreateShowSequence(previousPanel);
			showSequence.Restart();
		});
		hideSequence.Restart();
	}

	public RectTransform PeekStack()
	{
		if (panelStack.Count != 0)
		{
			return panelStack.Peek();
		}
		return null;
	}

	public RectTransform PopStack()
	{
		if (panelStack.Count != 0)
		{
			return panelStack.Pop();
		}
		return null;
	}

	public void ClearStack()
	{
		panelStack.Clear();
	}

	public int GetStackSize()
	{
		return panelStack.Count;
	}

	public void SetAnimationDuration(float value)
	{
		animationDuration = value;
	}

	public void ResetAnimationDuration()
	{
		animationDuration = defaultAnimationDuration;
	}
}
