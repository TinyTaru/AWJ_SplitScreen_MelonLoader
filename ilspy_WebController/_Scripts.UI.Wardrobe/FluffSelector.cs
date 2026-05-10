using UnityEngine;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.Settings;

namespace _Scripts.UI.Wardrobe;

public class FluffSelector : MonoBehaviour
{
	[Header("Body Part Setup")]
	[SerializeField]
	private Slider bodyFluffinessSlider;

	[SerializeField]
	private Slider legFluffinessSlider;

	[SerializeField]
	private Slider jointFluffinessSlider;

	[SerializeField]
	private Slider abdomenFluffinessSlider;

	[SerializeField]
	private SettingsSelection abdomenEnabledSelection;

	private WardrobeController wardrobeController;

	private void OnEnable()
	{
		bodyFluffinessSlider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadBodyFluffiness());
		abdomenFluffinessSlider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadAbdomenFluffiness());
		legFluffinessSlider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadLegFluffiness(0));
		jointFluffinessSlider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadJointFluffiness(0));
	}

	public void Setup(WardrobeController newWardrobeController)
	{
		wardrobeController = newWardrobeController;
	}

	public void ResetFluffinessAndAbdomen()
	{
		ChangeBodyFluff(Singleton<CosmeticItemsController>.Instance.DefaultBodyFluffiness);
		ChangeAbdomenFluff(Singleton<CosmeticItemsController>.Instance.DefaultAbdomenFluffiness);
		ChangeLegFluff(Singleton<CosmeticItemsController>.Instance.DefaultLegFluffiness);
		ChangeJointFluff(Singleton<CosmeticItemsController>.Instance.DefaultJointFluffiness);
		EnableAbdomen(0);
		bodyFluffinessSlider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.DefaultBodyFluffiness);
		abdomenFluffinessSlider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.DefaultAbdomenFluffiness);
		legFluffinessSlider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.DefaultLegFluffiness);
		jointFluffinessSlider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.DefaultJointFluffiness);
		abdomenEnabledSelection.SetValueWithoutNotify(0);
	}

	public void ChangeBodyFluff(float fluffAmount)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetBodyFluffiness(fluffAmount);
		});
		Singleton<CosmeticItemsController>.Instance.SaveBodyFluffiness(fluffAmount);
	}

	public void ChangeAbdomenFluff(float fluffAmount)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetAbdomenFluffiness(fluffAmount);
		});
		Singleton<CosmeticItemsController>.Instance.SaveAbdomenFluffiness(fluffAmount);
	}

	public void ChangeLegFluff(float fluffAmount)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetLegFluffiness(0, fluffAmount);
		});
		Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(0, fluffAmount);
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetLegFluffiness(1, fluffAmount);
		});
		Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(1, fluffAmount);
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetLegFluffiness(2, fluffAmount);
		});
		Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(2, fluffAmount);
	}

	public void ChangeJointFluff(float fluffAmount)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetJointFluffiness(0, fluffAmount);
		});
		Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(0, fluffAmount);
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetJointFluffiness(1, fluffAmount);
		});
		Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(1, fluffAmount);
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetJointFluffiness(2, fluffAmount);
		});
		Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(2, fluffAmount);
	}

	public void EnableAbdomen(int value)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetAbdomenEnabled(value == 1);
		});
		Singleton<CosmeticItemsController>.Instance.SaveAbdomenEnabled(value == 1);
	}
}
