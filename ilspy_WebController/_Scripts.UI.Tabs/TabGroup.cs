using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.UI.ContentFitting;
using _Scripts.UI.Selections;

namespace _Scripts.UI.Tabs;

public class TabGroup : MonoBehaviour
{
	[Header("Tab Buttons")]
	[SerializeField]
	private Button navigateLeftButton;

	[SerializeField]
	private Button navigateRightButton;

	[SerializeField]
	private bool resetTabs;

	[Header("Tab Modifiers")]
	[SerializeField]
	private int startIndex;

	[SerializeField]
	private int startLength;

	[Header("Colors")]
	[SerializeField]
	private Color tabIdle;

	[SerializeField]
	private Color tabHover;

	[SerializeField]
	private Color tabActive;

	[SerializeField]
	private Color textActive;

	[SerializeField]
	private Color textIdle;

	[SerializeField]
	private float gradientRotationSpeed = 75f;

	[Header("Animations")]
	[SerializeField]
	private float animationDuration = 0.25f;

	[SerializeField]
	private bool animateScale;

	[SerializeField]
	private bool animateFade;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference tabLeftInputAction;

	[SerializeField]
	private InputActionReference tabRightInputAction;

	[SerializeField]
	private InputActionReference tabLeftAlternativeInputAction;

	[SerializeField]
	private InputActionReference tabRightAlternativeInputAction;

	private List<TabButton> tabButtons;

	private TabButton selectedTab;

	private RectTransform inPanel;

	private RectTransform outPanel;

	private bool isAnimating;

	private bool animateOut = true;

	private int currentIndex;

	private int firstVisibleIndex;

	private int lastVisibleIndex;

	private int length;

	private Sequence panelSequence;

	private void Awake()
	{
		tabButtons = base.transform.GetComponentsInChildren<TabButton>().ToList();
		foreach (TabButton tabButton in tabButtons)
		{
			tabButton.Setup();
		}
		length = startLength;
	}

	private void OnEnable()
	{
		UpdateVisibleTabs(startIndex);
		animateOut = false;
		if (resetTabs)
		{
			ResetTabs();
		}
		SelectTabByIndex(currentIndex);
		animateOut = true;
		UpdateNavigationButtonVisuals(currentIndex);
	}

	private void OnDisable()
	{
		selectedTab = null;
		outPanel = null;
	}

	private void Update()
	{
		if (tabLeftInputAction.action.WasPerformedThisFrame() || tabLeftAlternativeInputAction.action.WasPerformedThisFrame())
		{
			TabLeft();
		}
		if (tabRightInputAction.action.WasPerformedThisFrame() || tabRightAlternativeInputAction.action.WasPerformedThisFrame())
		{
			TabRight();
		}
	}

	private void OnDestroy()
	{
		panelSequence?.Kill();
	}

	private void MoveTabs(int newIndex)
	{
		if (tabButtons.Count != 0)
		{
			if (newIndex == firstVisibleIndex)
			{
				UpdateVisibleTabs(firstVisibleIndex - 1);
			}
			if (newIndex == lastVisibleIndex)
			{
				UpdateVisibleTabs(firstVisibleIndex + 1);
			}
		}
	}

	private void SelectTabByIndex(int index)
	{
		OnTabSelected(tabButtons[index]);
	}

	private void UpdateNavigationButtonVisuals(int index)
	{
		if (navigateLeftButton != null)
		{
			navigateLeftButton.gameObject.SetActive(index > 0);
			if (EventSystem.current.currentSelectedGameObject == navigateLeftButton.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		if (navigateRightButton != null)
		{
			navigateRightButton.gameObject.SetActive(index < tabButtons.Count - 1);
			if (EventSystem.current.currentSelectedGameObject == navigateRightButton.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
	}

	private void UpdateVisibleTabs(int index)
	{
		firstVisibleIndex = Mathf.Clamp(index, 0, tabButtons.Count - length);
		lastVisibleIndex = firstVisibleIndex + length - 1;
		for (int i = 0; i < tabButtons.Count; i++)
		{
			tabButtons[i].gameObject.SetActive(i >= firstVisibleIndex && i <= lastVisibleIndex);
		}
	}

	private void AnimatePanelsIn()
	{
		RectTransform panel = selectedTab.GetPanel();
		CanvasGroup component = panel.GetComponent<CanvasGroup>();
		panel.localScale = Vector3.zero;
		panel.gameObject.SetActive(value: true);
		if (animateFade && animateScale)
		{
			panelSequence.Append(panel.DOScale(Vector3.one, animationDuration).SetEase(Ease.Linear));
			panelSequence.Join(component.DOFade(1f, animationDuration).SetEase(Ease.Linear));
		}
		else if (animateScale)
		{
			component.alpha = 1f;
			panelSequence.Append(panel.DOScale(Vector3.one, animationDuration).SetEase(Ease.Linear));
		}
		else if (animateFade)
		{
			panel.localScale = Vector3.one;
			panelSequence.Append(component.DOFade(1f, animationDuration).SetEase(Ease.Linear));
		}
		else
		{
			panel.localScale = Vector3.one;
			component.alpha = 1f;
		}
		ContentFitterRefresh contentFitterRefresh = panel.GetComponent<ContentFitterRefresh>();
		if (contentFitterRefresh != null)
		{
			panelSequence.AppendCallback(delegate
			{
				contentFitterRefresh.RefreshContentFitters();
			});
		}
	}

	private void AnimatePanelsOut()
	{
		RectTransform panel = selectedTab.GetPanel();
		CanvasGroup component = panel.GetComponent<CanvasGroup>();
		if (animateFade && animateScale)
		{
			panelSequence.Append(panel.DOScale(Vector3.zero, animationDuration).SetEase(Ease.Linear));
			panelSequence.Join(component.DOFade(0f, animationDuration).SetEase(Ease.Linear));
		}
		else if (animateScale)
		{
			panelSequence.Append(panel.DOScale(Vector3.zero, animationDuration).SetEase(Ease.Linear));
		}
		else if (animateFade)
		{
			panelSequence.Append(component.DOFade(0f, animationDuration).SetEase(Ease.Linear));
		}
		else
		{
			panel.localScale = Vector3.zero;
			component.alpha = 0f;
		}
	}

	private void ResetUnselectedTabs()
	{
		foreach (TabButton tabButton in tabButtons)
		{
			if (!(selectedTab != null) || !(tabButton == selectedTab))
			{
				tabButton.SetBackground(tabIdle);
				tabButton.SetTextColor(textIdle);
				tabButton.ActivateGradientEffect(value: false);
			}
		}
	}

	public void OnTabEnter(TabButton button)
	{
		ResetUnselectedTabs();
		if (selectedTab == null || button != selectedTab)
		{
			button.SetBackground(tabHover);
		}
	}

	public void OnTabExit(TabButton button)
	{
		ResetUnselectedTabs();
	}

	public void OnTabSelected(TabButton button)
	{
		isAnimating = true;
		panelSequence?.Kill();
		panelSequence = DOTween.Sequence();
		panelSequence.SetUpdate(isIndependentUpdate: true);
		if (selectedTab != null && animateOut)
		{
			outPanel = selectedTab.GetPanel();
			SelectionManager component = outPanel.GetComponent<SelectionManager>();
			if ((bool)component)
			{
				component.Deselect();
				component.Reset();
			}
			AnimatePanelsOut();
			selectedTab.Deselect();
		}
		selectedTab = button;
		AnimatePanelsIn();
		selectedTab.Select();
		ResetUnselectedTabs();
		button.SetBackground(tabActive);
		button.SetTextColor(textActive);
		button.ActivateGradientEffect(value: true, gradientRotationSpeed);
		panelSequence.OnComplete(delegate
		{
			isAnimating = false;
			if ((bool)outPanel)
			{
				outPanel.gameObject.SetActive(value: false);
			}
			inPanel = selectedTab.GetPanel();
			SelectionManager component2 = inPanel.GetComponent<SelectionManager>();
			if ((bool)component2)
			{
				component2.UpdateSelection();
			}
		});
		panelSequence.Play();
		int siblingIndex = selectedTab.transform.GetSiblingIndex();
		MoveTabs(siblingIndex);
		currentIndex = siblingIndex;
		UpdateNavigationButtonVisuals(currentIndex);
	}

	public void ResetTabs()
	{
		if (tabButtons == null)
		{
			return;
		}
		currentIndex = 0;
		foreach (TabButton tabButton in tabButtons)
		{
			tabButton.GetPanel().gameObject.SetActive(value: false);
		}
	}

	public int GetActiveTabIndex()
	{
		return tabButtons.IndexOf(selectedTab);
	}

	public int FindTab(TabButton tab)
	{
		return tabButtons.IndexOf(tab);
	}

	public int ListLength()
	{
		return tabButtons.Count;
	}

	public void SetHome()
	{
		SelectTabByIndex(0);
	}

	public void TabLeft()
	{
		if (!isAnimating)
		{
			int index = currentIndex - 1;
			if (currentIndex > 0)
			{
				SelectTabByIndex(index);
			}
		}
	}

	public void TabRight()
	{
		if (!isAnimating)
		{
			int index = currentIndex + 1;
			if (currentIndex < tabButtons.Count - 1)
			{
				SelectTabByIndex(index);
			}
		}
	}
}
