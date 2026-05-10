using MPUIKIT;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using _Scripts.Singletons;

namespace _Scripts.UI.Tabs;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerClickHandler, IPointerExitHandler
{
	[SerializeField]
	private RectTransform panel;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onTabSelected;

	[SerializeField]
	private UnityEvent onTabDeselected;

	private TabGroup tabGroup;

	private TextMeshProUGUI text;

	private MPImage background;

	private float gradientRotationSpeed = 75f;

	private bool gradientEffectIsActive;

	private GradientEffect gradientEffect;

	private void Update()
	{
		AnimateGradientEffect();
	}

	private void AnimateGradientEffect()
	{
		if (gradientEffectIsActive)
		{
			gradientEffect.Rotation -= gradientRotationSpeed * Time.unscaledDeltaTime;
			background.GradientEffect = gradientEffect;
		}
	}

	public void Setup()
	{
		tabGroup = GetComponentInParent<TabGroup>(includeInactive: true);
		text = GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		background = GetComponent<MPImage>();
		gradientEffect = background.GradientEffect;
		gradientEffectIsActive = false;
	}

	public void ActivateGradientEffect(bool value, float newGradientRotationSpeed = 0f)
	{
		gradientRotationSpeed = newGradientRotationSpeed;
		gradientEffectIsActive = value;
		gradientEffect.Enabled = gradientEffectIsActive;
		background.GradientEffect = gradientEffect;
	}

	public void Select()
	{
		onTabSelected?.Invoke();
	}

	public void Deselect()
	{
		onTabDeselected?.Invoke();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Singleton<MusicController>.Instance.SelectSound();
		tabGroup.OnTabEnter(this);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int activeTabIndex = tabGroup.GetActiveTabIndex();
		if (tabGroup.FindTab(this) != activeTabIndex)
		{
			Singleton<MusicController>.Instance.ClickSound();
			tabGroup.OnTabSelected(this);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		tabGroup.OnTabExit(this);
	}

	public void SetBackground(Color color)
	{
		background.color = color;
	}

	public void SetTextColor(Color color)
	{
		text.color = color;
	}

	public RectTransform GetPanel()
	{
		return panel;
	}
}
