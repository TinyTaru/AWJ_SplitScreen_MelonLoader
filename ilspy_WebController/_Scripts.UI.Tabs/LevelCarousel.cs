using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UI.Tabs;

public class LevelCarousel : MonoBehaviour
{
	[Header("Level Prefab")]
	[SerializeField]
	private LevelButton levelButtonPrefab;

	[Header("Configuration")]
	[SerializeField]
	private List<LevelData> levelsData;

	[SerializeField]
	private RectTransform levelContainer;

	[SerializeField]
	private Button exitButton;

	[Header("Appearance")]
	[SerializeField]
	private float animationDuration = 0.5f;

	[SerializeField]
	private float slotWidth = 300f;

	private int currentIndex;

	private bool isAnimating;

	private bool isPopulated;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference tabLeftInputAction;

	[SerializeField]
	private InputActionReference tabRightInputAction;

	public int CurrentIndex => currentIndex;

	private void OnEnable()
	{
		currentIndex = 0;
		if (!isPopulated)
		{
			PopulateLevels();
			isPopulated = true;
		}
		UpdateLevels(instant: true);
	}

	private void Update()
	{
		if (!isAnimating)
		{
			if (tabLeftInputAction.action.WasPerformedThisFrame())
			{
				Navigate(-1);
			}
			else if (tabRightInputAction.action.WasPerformedThisFrame())
			{
				Navigate(1);
			}
		}
	}

	private void PopulateLevels()
	{
		foreach (Transform item in levelContainer)
		{
			Object.Destroy(item.gameObject);
		}
		for (int i = 0; i < levelsData.Count; i++)
		{
			LevelButton levelButton = Object.Instantiate(levelButtonPrefab, levelContainer);
			levelButton.name = levelsData[i].levelName;
			RectTransform component = levelButton.GetComponent<RectTransform>();
			if (component != null)
			{
				component.anchoredPosition = new Vector2((float)i * slotWidth, 0f);
			}
			levelButton.Setup(this, levelsData[i], i);
		}
	}

	private void UpdateLevels(bool instant = false)
	{
		float num = (float)(-currentIndex) * slotWidth;
		if (instant)
		{
			levelContainer.anchoredPosition = new Vector2(num, levelContainer.anchoredPosition.y);
		}
		else
		{
			isAnimating = true;
			levelContainer.DOAnchorPosX(num, animationDuration).SetEase(Ease.OutQuad).OnComplete(delegate
			{
				isAnimating = false;
			});
		}
		for (int i = 0; i < levelsData.Count; i++)
		{
			LevelButton component = levelContainer.GetChild(i).GetComponent<LevelButton>();
			RectTransform rectTransform = component.GetRectTransform();
			Button button = component.GetButton();
			bool flag = i == currentIndex;
			if (rectTransform != null)
			{
				Vector3 vector = (flag ? Vector3.one : (Vector3.one * 0.8f));
				if (instant)
				{
					rectTransform.localScale = vector;
				}
				else
				{
					rectTransform.DOScale(vector, animationDuration).SetEase(Ease.OutQuad);
				}
			}
			if (!(button != null))
			{
				continue;
			}
			button.interactable = true;
			if (flag)
			{
				button.Select();
				if ((bool)exitButton)
				{
					Navigation navigation = exitButton.navigation;
					navigation.selectOnDown = button;
					exitButton.navigation = navigation;
				}
			}
		}
	}

	public void Navigate(int direction)
	{
		int num = currentIndex + direction;
		if (num >= 0 && num < levelsData.Count)
		{
			currentIndex = num;
			UpdateLevels();
		}
	}

	public void NavigateToIndex(int index, bool instant = false)
	{
		if (index >= 0 && index < levelsData.Count)
		{
			currentIndex = index;
			UpdateLevels(instant);
		}
	}
}
