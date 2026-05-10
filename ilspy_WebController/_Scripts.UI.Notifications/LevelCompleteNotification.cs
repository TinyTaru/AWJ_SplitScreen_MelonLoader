using System;
using FMOD.Studio;
using FMODUnity;
using MPUIKIT;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UI.Notifications;

public class LevelCompleteNotification : MonoBehaviour, IQueueableNotification
{
	[Header("References")]
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private MPImage levelSprite;

	[SerializeField]
	private TextMeshProUGUI messageText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[Header("Level Data")]
	[SerializeField]
	private LevelData tutorialLevelData;

	[SerializeField]
	private LevelData kitchenLevelData;

	[SerializeField]
	private LevelData officeLevelData;

	[FormerlySerializedAs("kidsroomLevelData")]
	[SerializeField]
	private LevelData kidsRoomLevelData;

	[SerializeField]
	private LevelData livingRoomLevelData;

	[Header("Sounds")]
	[SerializeField]
	private EventReference itemUnlockedSoundRef;

	public event System.EventHandler OnPopUpCompleted;

	private void SetSprite(Sprite sprite)
	{
		Color white = Color.white;
		white.a = 1f;
		levelSprite.color = white;
		levelSprite.sprite = sprite;
	}

	public void ShowMessage()
	{
		animator.SetTrigger("Play");
	}

	public void PlaySound()
	{
		EventInstance eventInstance = RuntimeManager.CreateInstance(itemUnlockedSoundRef);
		eventInstance.start();
		eventInstance.release();
	}

	public void OnAnimationFinished()
	{
		this.OnPopUpCompleted?.Invoke(this, EventArgs.Empty);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void SetLevel(LevelMusic level)
	{
		LevelData levelData = tutorialLevelData;
		switch (level)
		{
		case LevelMusic.Kitchen:
			levelData = kitchenLevelData;
			break;
		case LevelMusic.Office:
			levelData = officeLevelData;
			break;
		case LevelMusic.KidsRoom:
			levelData = kidsRoomLevelData;
			break;
		case LevelMusic.LivingRoom:
			levelData = livingRoomLevelData;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case LevelMusic.Menu:
		case LevelMusic.Hub:
		case LevelMusic.Tutorial:
		case LevelMusic.IslandForest:
		case LevelMusic.IslandDesert:
			break;
		}
		titleText.text = DialogueManager.GetLocalizedText(levelData.levelName);
		if (SettingsController.ArachnophobiaMode)
		{
			SetSprite(levelData.levelImageArachnophobia);
		}
		else
		{
			SetSprite(levelData.levelImageNormal);
		}
	}
}
