using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe;

public class WardrobeController : MonoBehaviour
{
	[Header("Spider Rotation")]
	[SerializeField]
	private float mouseRotateFactor = 40f;

	[SerializeField]
	private float controllerRotateFactor = 25f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onEnableEvent;

	[SerializeField]
	private UnityEvent onDisableEvent;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference controllerRotateInputAction;

	[SerializeField]
	private InputActionReference defaultSpiderInputAction;

	[SerializeField]
	private InputActionReference randomSpiderInputAction;

	[SerializeField]
	private InputActionReference mouseClickInputAction;

	[SerializeField]
	private InputActionReference mouseDragInputAction;

	private ColorSelector[] colorSelectors;

	private WardrobeSelector[] wardrobeSelectors;

	private FluffSelector[] fluffSelectors;

	private bool isDragging;

	private float controllerRotate;

	private Vector2 mouseDrag;

	private List<SpiderCustomization> spiderCustomizations;

	public List<SpiderCustomization> SpiderCustomizations => spiderCustomizations;

	public event EventHandler OnWardrobeClosed;

	private void Awake()
	{
		colorSelectors = GetComponentsInChildren<ColorSelector>(includeInactive: true);
		wardrobeSelectors = GetComponentsInChildren<WardrobeSelector>(includeInactive: true);
		fluffSelectors = GetComponentsInChildren<FluffSelector>(includeInactive: true);
		LoadSpiderCustomizations();
		SetupSelectors();
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
		if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
		{
			Vector2 vector = Touchscreen.current.primaryTouch.delta.ReadValue();
			if (!IsPointerOverUIOrSelected())
			{
				Singleton<WardrobeIsland>.Instance.Rotate((0f - vector.x) * mouseRotateFactor);
			}
		}
		if ((bool)randomSpiderInputAction && randomSpiderInputAction.action.WasPerformedThisFrame())
		{
			RandomOutfit();
		}
		if ((bool)defaultSpiderInputAction && defaultSpiderInputAction.action.WasPerformedThisFrame())
		{
			DefaultOutfit();
		}
	}

	private void OnEnable()
	{
		InitializeUI();
		Singleton<WardrobeIsland>.Instance.ShowWardrobeIsland();
		Singleton<CameraController>.Instance.EnableWardrobeCamera();
		Singleton<WebController>.Instance.SetWardrobeWebActive(value: true);
		controllerRotateInputAction.action.performed += OnControllerRotate;
		mouseClickInputAction.action.performed += OnMouseClickPerformed;
		mouseClickInputAction.action.canceled += OnMouseClickCanceled;
		mouseDragInputAction.action.performed += OnMouseDrag;
		if (QuestLog.GetQuestState("Demo0.3/Tutorial/Customization") == QuestState.Active)
		{
			QuestLog.SetQuestEntryState("Demo0.3/Tutorial/Customization", 1, QuestState.Success);
			QuestLog.SetQuestEntryState("Demo0.3/Tutorial/Customization", 2, QuestState.Active);
			DialogueManager.SendUpdateTracker();
		}
		onEnableEvent?.Invoke();
	}

	private void OnDisable()
	{
		controllerRotateInputAction.action.performed -= OnControllerRotate;
		mouseClickInputAction.action.performed -= OnMouseClickPerformed;
		mouseClickInputAction.action.canceled -= OnMouseClickCanceled;
		mouseDragInputAction.action.performed -= OnMouseDrag;
		Singleton<WardrobeIsland>.Instance.HideWardrobeIsland();
		Singleton<CameraController>.Instance.DisableWardrobeCamera();
		Singleton<WebController>.Instance.SetWardrobeWebActive(value: false);
		this.OnWardrobeClosed?.Invoke(this, EventArgs.Empty);
		onDisableEvent?.Invoke();
	}

	private void SetupSelectors()
	{
		if (colorSelectors.Length != 0)
		{
			ColorSelector[] array = colorSelectors;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Setup(this);
			}
		}
		if (wardrobeSelectors.Length != 0)
		{
			WardrobeSelector[] array2 = wardrobeSelectors;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Setup(this);
			}
		}
		FluffSelector[] array3 = fluffSelectors;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].Setup(this);
		}
	}

	private void InitializeUI()
	{
		if (colorSelectors.Length != 0)
		{
			ColorSelector[] array = colorSelectors;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].InitializeColorButtons();
			}
		}
		if (wardrobeSelectors.Length != 0)
		{
			WardrobeSelector[] array2 = wardrobeSelectors;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].InitializeAccessoryButton();
			}
		}
	}

	private void LoadSpiderCustomizations()
	{
		spiderCustomizations = new List<SpiderCustomization>();
		spiderCustomizations.Add(Singleton<GameController>.Instance.Player.Customization);
		spiderCustomizations.Add(Singleton<WardrobeIsland>.Instance.Customization);
	}

	private bool IsPointerOverUIOrSelected()
	{
		bool num = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
		bool flag = EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null;
		return num || flag;
	}

	public void DefaultOutfit()
	{
		WardrobeSelector[] array = wardrobeSelectors;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetDefaultItem();
		}
		ColorSelector[] array2 = colorSelectors;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].DefaultColor();
		}
		FluffSelector[] array3 = fluffSelectors;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].ResetFluffinessAndAbdomen();
		}
	}

	public void RandomOutfit()
	{
		WardrobeSelector[] array = wardrobeSelectors;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetRandomItem();
		}
		ColorSelector[] array2 = colorSelectors;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].RandomizeAllColor();
		}
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
}
