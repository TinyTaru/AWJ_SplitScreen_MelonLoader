using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.Toggles;

namespace _Scripts.UI.Wardrobe._3._0;

public class EnableAbdomenToggle : MonoBehaviour
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
		bool flag = Singleton<CosmeticItemsController>.Instance.LoadAbdomenEnabled();
		animatedToggle.SetInitialState(flag);
		onEnableEvent?.Invoke(flag);
	}

	public void SetAbdomenEnabled(bool value)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetAbdomenEnabled(value);
		});
		Singleton<CosmeticItemsController>.Instance.SaveAbdomenEnabled(value);
	}
}
