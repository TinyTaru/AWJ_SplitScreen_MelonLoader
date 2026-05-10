using System;
using MPUIKIT;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.UI.Wardrobe._3._0;

public class ColorPicker : Selectable, IPointerDownHandler, IEventSystemHandler, IDragHandler, IPointerUpHandler
{
	[Header("References")]
	[SerializeField]
	private ColorSelectorV3 colorSelector;

	[SerializeField]
	private RectTransform wheelRect;

	[SerializeField]
	private RectTransform selectorRect;

	[SerializeField]
	private float radius = 172.5f;

	[SerializeField]
	private MPImage colorTest;

	[SerializeField]
	private MPImage border;

	[SerializeField]
	private Color normalBorderColor;

	[SerializeField]
	private Color selectedBorderColor;

	[SerializeField]
	private Color editBorderColor;

	[SerializeField]
	private MPImage pasteColorSwatch;

	[SerializeField]
	private Slider saturationSlider;

	[SerializeField]
	private Slider valueSlider;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference mouseNavigation;

	[SerializeField]
	private InputActionReference controllerNavigation;

	[SerializeField]
	private InputActionReference submitAction;

	[SerializeField]
	private InputActionReference cancelAction;

	private bool isEditing;

	private bool isMouseDragging;

	private float currentHue;

	private float saturation = 1f;

	private float value = 1f;

	private Navigation cachedNavigation;

	private float cachedHue;

	private float cachedSaturation;

	private float cachedValue;

	private static float copiedHue;

	private static float copiedSaturation;

	private static float copiedValue;

	public event Action<bool> SetActive;

	protected override void Awake()
	{
		base.Awake();
		copiedHue = 0f;
		copiedSaturation = 0f;
		copiedValue = 1f;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		pasteColorSwatch.color = HueToColor(copiedHue, copiedSaturation, copiedValue);
		LoadHue();
		SetHueFromLoad(currentHue);
		submitAction.action.performed += OnSubmitPerformed;
		cancelAction.action.performed += OnCancelPerformed;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		submitAction.action.performed -= OnSubmitPerformed;
		cancelAction.action.performed -= OnCancelPerformed;
	}

	private void Update()
	{
		if (isEditing)
		{
			HandleControllerRotation();
			colorTest.color = HueToColor(currentHue, saturation, value);
		}
	}

	private void LoadHue()
	{
		Color color = colorSelector.LoadColor();
		ColoToHSV(color);
	}

	private void ColoToHSV(Color color)
	{
		Color.RGBToHSV(color, out var H, out saturation, out value);
		currentHue = H * 360f;
	}

	private void SetHueFromLoad(float hue)
	{
		currentHue = hue;
		UpdateSelectorPosition();
		EmitColor(currentHue);
	}

	private Color HueToColor(float hue, float s, float v)
	{
		return Color.HSVToRGB(hue / 360f, s, v);
	}

	private void EmitColor(float finalHue)
	{
		Color color = HueToColor(finalHue, saturation, value);
		colorTest.color = color;
		colorSelector.ApplyCustomColor(color);
		saturationSlider.SetValueWithoutNotify(saturation);
		valueSlider.SetValueWithoutNotify(value);
	}

	private void OnSubmitPerformed(InputAction.CallbackContext ctx)
	{
		if (!(EventSystem.current.currentSelectedGameObject != base.gameObject) && !isMouseDragging)
		{
			if (!isEditing)
			{
				EnterEditMode();
			}
			else
			{
				CommitEdit();
			}
		}
	}

	private void HandleControllerRotation()
	{
		Vector2 vector = controllerNavigation.action.ReadValue<Vector2>();
		if (!(vector.sqrMagnitude < 0.1f))
		{
			float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			if (num < 0f)
			{
				num += 360f;
			}
			SetHue(num);
		}
	}

	private void UpdateHueFromPointer(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRect, eventData.position, eventData.pressEventCamera, out var localPoint);
		float num = Mathf.Atan2(localPoint.y, localPoint.x) * 57.29578f;
		if (num < 0f)
		{
			num += 360f;
		}
		SetHue(num);
	}

	private void SetHue(float rawAngle)
	{
		currentHue = rawAngle;
		UpdateSelectorPosition();
		EmitColor(currentHue);
	}

	private void UpdateSelectorPosition()
	{
		float f = currentHue * (MathF.PI / 180f);
		selectorRect.localPosition = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * radius;
	}

	private void EnterEditMode()
	{
		if (!isEditing)
		{
			cachedHue = currentHue;
			cachedSaturation = saturation;
			cachedValue = value;
			isEditing = true;
			cachedNavigation = base.navigation;
			Navigation navigation = base.navigation;
			navigation.mode = Navigation.Mode.None;
			base.navigation = navigation;
			border.color = editBorderColor;
			this.SetActive?.Invoke(obj: true);
		}
	}

	private void CommitEdit()
	{
		ExitEditMode();
	}

	private void RevertEdit()
	{
		currentHue = cachedHue;
		saturation = cachedSaturation;
		value = cachedValue;
		UpdateSelectorPosition();
		EmitColor(currentHue);
		ExitEditMode();
	}

	private void ExitEditMode()
	{
		if (isEditing)
		{
			isEditing = false;
			base.navigation = cachedNavigation;
			border.color = selectedBorderColor;
			this.SetActive?.Invoke(obj: false);
		}
	}

	private void OnCancelPerformed(InputAction.CallbackContext ctx)
	{
		if (isEditing && !isMouseDragging)
		{
			RevertEdit();
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		EnterEditMode();
	}

	public new void OnPointerDown(PointerEventData eventData)
	{
		isMouseDragging = true;
		cachedHue = currentHue;
		cachedSaturation = saturation;
		cachedValue = value;
		isEditing = true;
		border.color = editBorderColor;
		this.SetActive?.Invoke(obj: true);
		UpdateHueFromPointer(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (isEditing)
		{
			UpdateHueFromPointer(eventData);
		}
	}

	public new void OnPointerUp(PointerEventData eventData)
	{
		if (isMouseDragging)
		{
			isMouseDragging = false;
			ExitEditMode();
		}
	}

	public void SetSaturation(float s)
	{
		saturation = s;
		EmitColor(currentHue);
	}

	public void SetValue(float v)
	{
		value = v;
		EmitColor(currentHue);
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		ExitEditMode();
	}

	public void SetSelectedOutlineColor()
	{
		border.color = selectedBorderColor;
	}

	public void SetDeselectedOutlineColor()
	{
		border.color = normalBorderColor;
	}

	public void CopyCurrentColor()
	{
		copiedHue = currentHue;
		copiedSaturation = saturation;
		copiedValue = value;
		pasteColorSwatch.color = HueToColor(copiedHue, copiedSaturation, copiedValue);
		Debug.Log($"Copied Color → H:{copiedHue} S:{copiedSaturation} V:{copiedValue}");
	}

	public void PasteCopiedColor()
	{
		currentHue = copiedHue;
		saturation = copiedSaturation;
		value = copiedValue;
		UpdateSelectorPosition();
		EmitColor(currentHue);
		Debug.Log($"Pasted Color → H:{currentHue} S:{saturation} V:{value}");
	}
}
