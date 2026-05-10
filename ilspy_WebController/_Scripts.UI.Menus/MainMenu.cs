using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.UI.Panels;
using _Scripts.UI.Scene_Loading;

namespace _Scripts.UI.Menus;

[RequireComponent(typeof(PanelManager))]
public class MainMenu : MonoBehaviour
{
	[Header("Panels")]
	[SerializeField]
	private RectTransform splashPanel;

	[SerializeField]
	private RectTransform phobiaPanel;

	[SerializeField]
	private RectTransform mainMenuPanel;

	[SerializeField]
	private RectTransform trailerPanel;

	[Header("Trailer")]
	[SerializeField]
	private VideoPlayer trailerVideoPlayer;

	[Header("Scenes")]
	[SerializeField]
	private LevelData initializeUnityServicesLevelData;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference continueAction;

	[SerializeField]
	private InputActionReference continueMobileAction;

	[SerializeField]
	private InputActionReference anyKeyInputAction;

	[SerializeField]
	private InputActionReference goBackInput;

	private PanelManager panelManager;

	private void Awake()
	{
		panelManager = GetComponent<PanelManager>();
		Physics.gravity = SettingsController.DefaultGravity;
	}

	private void Start()
	{
		Singleton<MusicController>.Instance.MuteSoundBus(mute: true);
		Singleton<MusicController>.Instance.StartMusic(LevelMusic.Menu);
		if (Singleton<SceneController>.Instance.LastScene == "" || Singleton<SceneController>.Instance.LastScene == initializeUnityServicesLevelData.sceneName)
		{
			panelManager.OpenPanel(splashPanel);
		}
		else
		{
			panelManager.OpenPanel(mainMenuPanel);
		}
		SettingsController.SetEventMode(0);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void Update()
	{
		if (panelManager.IsAnimating)
		{
			return;
		}
		if (splashPanel.gameObject.activeSelf)
		{
			if (continueAction.action.WasPerformedThisFrame())
			{
				bool flag = SaveController.Load("ShowArachnophobiaPanel", defaultValue: true, SaveData.General);
				panelManager.OpenPanel(flag ? phobiaPanel : mainMenuPanel);
			}
		}
		else if (trailerPanel != null && trailerPanel.gameObject.activeSelf)
		{
			if (anyKeyInputAction.action.WasPerformedThisFrame())
			{
				StopTrailer();
			}
		}
		else if (!mainMenuPanel.gameObject.activeSelf && goBackInput.action.WasPerformedThisFrame())
		{
			panelManager.GoBack();
		}
	}

	private void StopTrailer()
	{
		if (!(trailerVideoPlayer == null))
		{
			panelManager.GoBack();
		}
	}

	public void PlayTrailer()
	{
		if (!(trailerPanel == null) && !(trailerVideoPlayer == null))
		{
			panelManager.OpenPanel(trailerPanel);
		}
	}

	public void ContinueExternal()
	{
		if (splashPanel.gameObject.activeSelf)
		{
			bool flag = SaveController.Load("ShowArachnophobiaPanel", defaultValue: true, SaveData.General);
			panelManager.OpenPanel(flag ? phobiaPanel : mainMenuPanel);
		}
	}
}
