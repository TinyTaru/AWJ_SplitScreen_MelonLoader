using System;
using MPUIKIT;
using UnityEngine;
using UnityEngine.InputSystem;
using _Scripts.Singletons;
using _Scripts.UI.Panels;
using _Scripts.UI.Shop;
using _Scripts.UI.Tabs;
using _Scripts.UI.Wardrobe._3._0;

namespace _Scripts.UI.Menus;

[RequireComponent(typeof(PanelManager))]
public class PauseMenu : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private RectTransform pauseCanvas;

	[SerializeField]
	private RectTransform wardrobeCanvas;

	[SerializeField]
	private WardrobeControllerV3 wardrobeControllerV3;

	[SerializeField]
	private TabGroup pauseCanvasTabGroup;

	[SerializeField]
	private MPImage menuBackground;

	[SerializeField]
	private CoinUI coinUI;

	[SerializeField]
	private GameObject pauseMenuStoryLevel;

	[SerializeField]
	private GameObject pauseMenuCreativeLevel;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference goBackInput;

	private PanelManager panelManager;

	private bool isEditingEmoteWheel;

	private bool colorPickerIsActive;

	private void Awake()
	{
		panelManager = GetComponent<PanelManager>();
	}

	private void Start()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnPauseGame += GameController_OnPauseGame;
			Singleton<GameController>.Instance.OnOpenWardrobeDirectly += GameController_OnOpenWardrobeDirectly;
		}
		wardrobeControllerV3.OnWardrobeClosed += WardrobeControllerV3_OnWardrobeClosed;
		ColorPicker[] componentsInChildren = base.transform.GetComponentsInChildren<ColorPicker>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetActive += ColorPicker_OnSetActive;
		}
	}

	private void Update()
	{
		if (!panelManager.IsAnimating && goBackInput.action.WasPerformedThisFrame() && !isEditingEmoteWheel && !colorPickerIsActive)
		{
			if (pauseCanvas.gameObject.activeSelf)
			{
				ContinueGame();
			}
			else if (panelManager.PeekStack() == wardrobeCanvas && panelManager.GetStackSize() == 1)
			{
				wardrobeControllerV3.CloseWardrobe();
				ContinueGame();
			}
			else if (panelManager.PeekStack() == wardrobeCanvas)
			{
				wardrobeControllerV3.GoBack();
			}
			else
			{
				panelManager.GoBack();
			}
		}
	}

	private void PauseGame()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel)
		{
			coinUI.Show();
			pauseMenuStoryLevel.SetActive(value: true);
			pauseMenuCreativeLevel.SetActive(value: false);
		}
		else
		{
			coinUI.Hide(hideImmediate: true);
			pauseMenuStoryLevel.SetActive(value: false);
			pauseMenuCreativeLevel.SetActive(value: true);
		}
		panelManager.ClearStack();
		menuBackground.enabled = true;
		panelManager.OpenPanel(pauseCanvas);
	}

	private void OpenWardrobeDirectly()
	{
		panelManager.ClearStack();
		wardrobeControllerV3.OpenWardrobe();
		panelManager.SetAnimationDuration(0f);
	}

	private void SetColorEditMode(bool val)
	{
		colorPickerIsActive = val;
		if (goBackInput != null && goBackInput.action != null)
		{
			if (val)
			{
				goBackInput.action.Disable();
			}
			else
			{
				goBackInput.action.Enable();
			}
		}
	}

	public void SetEmoteEditMode(bool val)
	{
		isEditingEmoteWheel = val;
	}

	public void ContinueGame()
	{
		if (!(Singleton<GameController>.Instance == null))
		{
			if (Singleton<SceneController>.Instance.IsStoryLevel)
			{
				coinUI.Hide();
			}
			panelManager.CloseCurrentPanel(delegate
			{
				Singleton<GameController>.Instance.ContinueGame();
				menuBackground.enabled = false;
			});
			pauseCanvasTabGroup.ResetTabs();
			menuBackground.enabled = false;
		}
	}

	public void ResetPlayer()
	{
		if (!(Singleton<GameController>.Instance == null))
		{
			menuBackground.enabled = false;
			if (Singleton<SceneController>.Instance.IsStoryLevel)
			{
				coinUI.Hide();
			}
			panelManager.CloseCurrentPanel(delegate
			{
				Singleton<GameController>.Instance.ContinueGame();
				Singleton<GameController>.Instance.ResetPlayer();
			});
		}
	}

	public void OpenWardrobe()
	{
		menuBackground.enabled = false;
		wardrobeControllerV3.OpenWardrobe();
		panelManager.SetAnimationDuration(0f);
	}

	private void GameController_OnPauseGame(object sender, EventArgs e)
	{
		PauseGame();
	}

	private void GameController_OnOpenWardrobeDirectly(object sender, EventArgs e)
	{
		OpenWardrobeDirectly();
	}

	private void WardrobeControllerV3_OnWardrobeClosed(object sender, EventArgs e)
	{
		panelManager.ResetAnimationDuration();
		if (panelManager.PeekStack() == wardrobeCanvas && panelManager.GetStackSize() == 1)
		{
			ContinueGame();
		}
		else
		{
			menuBackground.enabled = true;
		}
	}

	private void ColorPicker_OnSetActive(bool value)
	{
		SetColorEditMode(value);
	}
}
