using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UI.Menus;

public class HomePage : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private TextMeshProUGUI profileButtonText;

	[SerializeField]
	private Image profileButtonImage;

	[SerializeField]
	private TextMeshProUGUI storyModeButtonText;

	[SerializeField]
	private Image storyModeButtonImageNormal;

	[SerializeField]
	private Image storyModeButtonImageArachnophobia;

	[Header("Level Data")]
	[SerializeField]
	private LevelData officeLevelData;

	[SerializeField]
	private LevelData kidsRoomLevelData;

	private string storyLevel;

	private void OnEnable()
	{
		SaveController.Save("ShowArachnophobiaPanel", value: false, SaveData.General);
		int currentProfile = Singleton<ProfileController>.Instance.CurrentProfile;
		profileButtonText.text = string.Format("{0} {1}", DialogueManager.GetLocalizedText("Menu_Profile"), currentProfile);
		profileButtonImage.sprite = Singleton<ProfileController>.Instance.GetProfileSprite(currentProfile);
		storyLevel = SaveController.LoadString("StoryLevel", string.Empty, SaveData.Game);
		if (storyLevel == string.Empty)
		{
			storyModeButtonText.text = DialogueManager.GetLocalizedText("Menu_New Game");
			LevelData levelData = Singleton<SceneController>.Instance.GetLevelData("EA_Level_Tutorial");
			storyModeButtonImageNormal.sprite = levelData.levelImageNormal;
			storyModeButtonImageArachnophobia.sprite = levelData.levelImageArachnophobia;
		}
		else
		{
			storyModeButtonText.text = DialogueManager.GetLocalizedText("Menu_Continue");
			LevelData levelData2 = Singleton<SceneController>.Instance.GetLevelData(storyLevel);
			storyModeButtonImageNormal.sprite = levelData2.levelImageNormal;
			storyModeButtonImageArachnophobia.sprite = levelData2.levelImageArachnophobia;
		}
	}

	public void TryLoadStoryLevel()
	{
		Singleton<SceneController>.Instance.LoadStoryLevel();
	}
}
