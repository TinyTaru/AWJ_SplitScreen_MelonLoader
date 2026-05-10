using FMOD.Studio;
using FMODUnity;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Profiles;

public class ProfileButton : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private int profile;

	[Header("References")]
	[SerializeField]
	private Image profileImage;

	[SerializeField]
	private TextMeshProUGUI profileText;

	[SerializeField]
	private TextMeshProUGUI storyLevelText;

	[SerializeField]
	private TextMeshProUGUI completedQuestsText;

	[FormerlySerializedAs("totalCoinsText")]
	[SerializeField]
	private TextMeshProUGUI currentCoinsText;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference clearProfile;

	[Header("Hold Time")]
	[SerializeField]
	private float requiredHoldTime = 5f;

	[Header("Images")]
	[SerializeField]
	private Image progressBar;

	[Header("Sounds")]
	[SerializeField]
	private EventReference clearProfileSoundRef;

	private float holdTime;

	private bool isSelected;

	private bool isClearing;

	private ProfileSelectionCanvas profileSelectionCanvas;

	private EventInstance clearProfileSound;

	private void Awake()
	{
		profileSelectionCanvas = GetComponentInParent<ProfileSelectionCanvas>();
		clearProfileSound = RuntimeManager.CreateInstance(clearProfileSoundRef);
	}

	private void Update()
	{
		if (isSelected)
		{
			if (clearProfile.action.WasPressedThisFrame())
			{
				StartClearing();
			}
			if (clearProfile.action.WasReleasedThisFrame())
			{
				StopClearing();
			}
		}
		if (isClearing)
		{
			holdTime += Time.deltaTime;
			float fillAmount = Mathf.Clamp01(holdTime / requiredHoldTime);
			progressBar.fillAmount = fillAmount;
			if (holdTime >= requiredHoldTime)
			{
				DeleteProfile(profile);
				ResetFill();
				profileSelectionCanvas.ShowDeletionMessage(profile);
			}
		}
	}

	private void OnEnable()
	{
		ResetFill();
		UpdateProfileInformation();
	}

	private void OnDisable()
	{
		SetSelected(value: false);
	}

	private void StartClearing()
	{
		isClearing = true;
		clearProfileSound.start();
	}

	private void StopClearing()
	{
		isClearing = false;
		clearProfileSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		ResetFill();
	}

	private void ResetFill()
	{
		progressBar.fillAmount = 0f;
		holdTime = 0f;
	}

	public void UpdateProfileInformation()
	{
		profileImage.sprite = Singleton<ProfileController>.Instance.GetProfileSprite(profile);
		profileText.text = string.Format("{0} {1}", DialogueManager.GetLocalizedText("Menu_Profile"), profile);
		string text = SaveController.LoadString("StoryLevel", "", SaveData.Game, profile);
		if (text == "")
		{
			storyLevelText.text = DialogueManager.GetLocalizedText("Menu_New Game");
		}
		else
		{
			string levelName = Singleton<SceneController>.Instance.GetLevelName(text);
			storyLevelText.text = DialogueManager.GetLocalizedText(levelName);
		}
		int num = SaveController.Load("CompletedQuests", 0, SaveData.Game, profile);
		int totalQuests = Singleton<QuestController>.Instance.TotalQuests;
		completedQuestsText.text = $"{num}/{totalQuests}";
		int num2 = SaveController.Load("CurrentCoinAmount", 0, SaveData.Game, profile);
		currentCoinsText.text = $"x{num2}";
	}

	public void SetSelected(bool value)
	{
		isSelected = value;
		if (!isSelected)
		{
			StopClearing();
		}
	}

	public void LoadProfile()
	{
		Singleton<ProfileController>.Instance.LoadProfile(profile);
	}

	public void DeleteProfile(int profile)
	{
		Singleton<ProfileController>.Instance.DeleteProfile(profile);
	}
}
