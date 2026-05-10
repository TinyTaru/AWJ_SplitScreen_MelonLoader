using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.Toggles;

namespace _Scripts.UI.Wardrobe._3._0;

public class LegEnableToggle : MonoBehaviour
{
	[SerializeField]
	private int legIndex;

	[SerializeField]
	private AnimatedToggle animatedToggle;

	private WardrobeControllerV3 wardrobeController;

	private void Awake()
	{
		wardrobeController = GetComponentInParent<WardrobeControllerV3>();
	}

	private void OnEnable()
	{
		animatedToggle.SetInitialState(Singleton<CosmeticItemsController>.Instance.LoadLegsEnabled()[legIndex]);
	}

	public void EnableLeg(bool value)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization s)
		{
			s.EnableLeg(legIndex, value);
		});
		bool[] array = Singleton<CosmeticItemsController>.Instance.LoadLegsEnabled();
		array[legIndex] = value;
		Singleton<CosmeticItemsController>.Instance.SaveLegsEnabled(array);
	}
}
