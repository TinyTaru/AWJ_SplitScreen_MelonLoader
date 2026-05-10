using MPUIKIT;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using _Scripts.Singletons;

namespace _Scripts.UI.Wardrobe._3._0;

public class WardrobeColorButton : MonoBehaviour
{
	[Header("Button Components")]
	[SerializeField]
	private MPImage swatch;

	[SerializeField]
	private EventTrigger trigger;

	[SerializeField]
	private Button button;

	public void Setup(int colorIndex, UnityAction<int, bool> onColorSelected)
	{
		swatch.color = Singleton<CosmeticItemsController>.Instance.ColorPalette.colors[colorIndex];
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.Select;
		entry.callback.AddListener(delegate
		{
			onColorSelected(colorIndex, arg1: false);
		});
		trigger.triggers.Add(entry);
		button.onClick.AddListener(delegate
		{
			onColorSelected(colorIndex, arg1: true);
		});
	}

	public Button GetButton()
	{
		return button;
	}
}
