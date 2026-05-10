using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.UI.Save_Slots;
using _Scripts.UI.Tabs;

namespace _Scripts.UI.Scene_Loading;

public class LevelButton : MonoBehaviour
{
	[Header("Level Data")]
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private Image levelImage;

	[SerializeField]
	private TMP_Text levelName;

	[SerializeField]
	private Button button;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private EventTrigger trigger;

	[SerializeField]
	private GameObject comingSoonText;

	private LevelCarousel levelCarousel;

	private LevelData levelData;

	private int index;

	public void Setup(LevelCarousel newLevelCarousel, LevelData newLevelData, int newIndex)
	{
		levelCarousel = newLevelCarousel;
		levelData = newLevelData;
		index = newIndex;
		levelName.text = levelData.levelName;
		levelImage.sprite = (SettingsController.ArachnophobiaMode ? levelData.levelImageArachnophobia : levelData.levelImageNormal);
		comingSoonText.SetActive(levelData.comingSoonSandbox);
		if (levelData.comingSoonSandbox)
		{
			levelImage.color = Color.gray;
		}
	}

	public void Click()
	{
		if (levelCarousel.CurrentIndex == index)
		{
			if (!levelData.comingSoonSandbox)
			{
				SaveFileCanvas.Open(levelData);
			}
		}
		else
		{
			levelCarousel.NavigateToIndex(index);
		}
	}

	public Button GetButton()
	{
		return button;
	}

	public RectTransform GetRectTransform()
	{
		return rectTransform;
	}
}
