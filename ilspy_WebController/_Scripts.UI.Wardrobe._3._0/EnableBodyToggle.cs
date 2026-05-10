using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.Toggles;

namespace _Scripts.UI.Wardrobe._3._0;

public class EnableBodyToggle : MonoBehaviour
{
	[SerializeField]
	private AnimatedToggle animatedToggle;

	[SerializeField]
	private UnityEvent<bool> onEnableEvent;

	private WardrobeControllerV3 wardrobeController;

	private void Awake()
	{
		wardrobeController = GetComponentInParent<WardrobeControllerV3>();
	}

	private void OnEnable()
	{
		bool flag = Singleton<CosmeticItemsController>.Instance.LoadBodyEnabled();
		animatedToggle.SetInitialState(flag);
		onEnableEvent?.Invoke(flag);
	}

	public void SetBodyEnabled(bool value)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetBodyEnabled(value);
		});
		Singleton<CosmeticItemsController>.Instance.SaveBodyEnabled(value);
	}
}
