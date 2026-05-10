using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.UI.Toggles;

namespace _Scripts.UI.Wardrobe._3._0;

public class ArachnophobiaModeToggle : MonoBehaviour
{
	[SerializeField]
	private AnimatedToggle animatedToggle;

	[SerializeField]
	private UnityEvent<bool> onEnableEvent;

	private void OnEnable()
	{
		bool arachnophobiaMode = SettingsController.ArachnophobiaMode;
		animatedToggle.SetInitialState(arachnophobiaMode);
		onEnableEvent?.Invoke(arachnophobiaMode);
	}

	public void SetArachnophobiaMode(bool value)
	{
		SettingsController.SetArachnophobiaMode(value);
	}
}
