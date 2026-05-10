using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.Panels;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe._3._0;

public class WardrobeControllerV3 : MonoBehaviour
{
	[Header("UI Elements")]
	[SerializeField]
	private RectTransform wardrobeMenu;

	[SerializeField]
	private RectTransform colorSelectorPanel;

	[SerializeField]
	private ColorSelectorV3 colorSelector;

	[SerializeField]
	private RectTransform itemSelectorPanel;

	[SerializeField]
	private ItemSelectorV3 itemSelector;

	[Header("Spider Rotation")]
	[SerializeField]
	private float mobileRotateFactor = 20f;

	[SerializeField]
	private float mouseRotateFactor = 40f;

	[SerializeField]
	private float controllerRotateFactor = 25f;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference controllerRotateInputAction;

	[SerializeField]
	private InputActionReference mouseClickInputAction;

	[SerializeField]
	private InputActionReference mouseDragInputAction;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onEnableEvent;

	[SerializeField]
	private UnityEvent onDisableEvent;

	private PanelManager panelManager;

	private List<SpiderCustomization> spiderCustomizations;

	private bool isDragging;

	private float controllerRotate;

	private Vector2 mouseDrag;

	public List<SpiderCustomization> SpiderCustomizations => spiderCustomizations;

	public event EventHandler OnWardrobeClosed;

	private void Awake()
	{
		panelManager = GetComponentInParent<PanelManager>();
	}

	private void Start()
	{
		LoadSpiderCustomizations();
	}

	private void OnEnable()
	{
		controllerRotateInputAction.action.performed += OnControllerRotate;
		mouseClickInputAction.action.performed += OnMouseClickPerformed;
		mouseClickInputAction.action.canceled += OnMouseClickCanceled;
		mouseDragInputAction.action.performed += OnMouseDrag;
	}

	private void OnDisable()
	{
		controllerRotateInputAction.action.performed -= OnControllerRotate;
		mouseClickInputAction.action.performed -= OnMouseClickPerformed;
		mouseClickInputAction.action.canceled -= OnMouseClickCanceled;
		mouseDragInputAction.action.performed -= OnMouseDrag;
	}

	private void Update()
	{
		if (controllerRotate > 0f || controllerRotate < 0f)
		{
			Singleton<WardrobeIsland>.Instance.Rotate((0f - controllerRotate) * controllerRotateFactor);
		}
		if (isDragging && !IsPointerOverUIOrSelected())
		{
			Singleton<WardrobeIsland>.Instance.Rotate((0f - mouseDrag.x) * mouseRotateFactor);
		}
	}

	private void LoadSpiderCustomizations()
	{
		spiderCustomizations = new List<SpiderCustomization>
		{
			Singleton<GameController>.Instance.Player.Customization,
			Singleton<WardrobeIsland>.Instance.Customization
		};
	}

	private void OnMouseClickPerformed(InputAction.CallbackContext ctx)
	{
		if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
		{
			isDragging = true;
		}
		else
		{
			isDragging = true;
		}
	}

	private void OnMouseClickCanceled(InputAction.CallbackContext ctx)
	{
		if (Touchscreen.current != null && !Touchscreen.current.primaryTouch.press.isPressed)
		{
			isDragging = false;
		}
		else
		{
			isDragging = false;
		}
	}

	private void OnMouseDrag(InputAction.CallbackContext ctx)
	{
		mouseDrag = ctx.ReadValue<Vector2>();
	}

	private void OnControllerRotate(InputAction.CallbackContext ctx)
	{
		controllerRotate = ctx.ReadValue<float>();
	}

	private bool IsPointerOverUIOrSelected()
	{
		bool num = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		bool flag = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null;
		return num || flag;
	}

	public void OpenWardrobe()
	{
		panelManager.OpenPanel(wardrobeMenu);
		Singleton<WardrobeIsland>.Instance.ShowWardrobeIsland();
		Singleton<CameraController>.Instance.EnableWardrobeCamera();
		Singleton<WebController>.Instance.SetWardrobeWebActive(value: true);
		if (QuestLog.GetQuestState("Demo0.3/Tutorial/Customization") == QuestState.Active)
		{
			QuestLog.SetQuestEntryState("Demo0.3/Tutorial/Customization", 1, QuestState.Success);
			QuestLog.SetQuestEntryState("Demo0.3/Tutorial/Customization", 2, QuestState.Active);
			DialogueManager.SendUpdateTracker();
		}
		onEnableEvent?.Invoke();
	}

	public void CloseWardrobe()
	{
		Singleton<WardrobeIsland>.Instance.HideWardrobeIsland();
		Singleton<CameraController>.Instance.DisableWardrobeCamera();
		Singleton<WebController>.Instance.SetWardrobeWebActive(value: false);
		this.OnWardrobeClosed?.Invoke(this, EventArgs.Empty);
		onDisableEvent?.Invoke();
	}

	public void OpenPanel(RectTransform panel)
	{
		panelManager.OpenPanel(panel);
	}

	public void OpenColorSelectorPanel(ColorSelectorV3.Colorable partToColor, int colorIndex = 0)
	{
		colorSelector.SetPartToColor(partToColor, colorIndex);
		panelManager.OpenPanel(colorSelectorPanel);
	}

	public void OpenItemSelectorPanel(CosmeticItemType cosmeticItem)
	{
		itemSelector.SetCosmeticItem(cosmeticItem);
		panelManager.OpenPanel(itemSelectorPanel);
	}

	public void GoBack()
	{
		if (panelManager.PeekStack() == wardrobeMenu)
		{
			CloseWardrobe();
		}
		panelManager.GoBack();
	}
}
