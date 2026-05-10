using System;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.UI.Wardrobe._3._0;

public class FluffinessSlider : MonoBehaviour
{
	[SerializeField]
	private Fluffiness bodyPart;

	private WardrobeControllerV3 wardrobeController;

	private Slider slider;

	private void Awake()
	{
		wardrobeController = base.gameObject.GetComponentInParent<WardrobeControllerV3>();
		slider = GetComponentInChildren<Slider>();
		slider.onValueChanged.AddListener(SetValue);
	}

	private void OnEnable()
	{
		switch (bodyPart)
		{
		case Fluffiness.Abdomen:
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadAbdomenFluffiness());
			break;
		case Fluffiness.Body:
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadBodyFluffiness());
			break;
		case Fluffiness.LegInner:
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadLegFluffiness(0));
			break;
		case Fluffiness.LegMiddle:
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadLegFluffiness(1));
			break;
		case Fluffiness.LegTip:
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadLegFluffiness(2));
			break;
		case Fluffiness.JointInner:
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadJointFluffiness(0));
			break;
		case Fluffiness.JointMiddle:
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadJointFluffiness(1));
			break;
		case Fluffiness.JointTip:
			slider.SetValueWithoutNotify(Singleton<CosmeticItemsController>.Instance.LoadJointFluffiness(2));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetValue(float value)
	{
		switch (bodyPart)
		{
		case Fluffiness.Abdomen:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetAbdomenFluffiness(value);
			});
			slider.SetValueWithoutNotify(value);
			Singleton<CosmeticItemsController>.Instance.SaveAbdomenFluffiness(value);
			break;
		case Fluffiness.Body:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetBodyFluffiness(value);
			});
			slider.SetValueWithoutNotify(value);
			Singleton<CosmeticItemsController>.Instance.SaveBodyFluffiness(value);
			break;
		case Fluffiness.LegInner:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetLegFluffiness(0, value);
			});
			slider.SetValueWithoutNotify(value);
			Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(0, value);
			break;
		case Fluffiness.LegMiddle:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetLegFluffiness(1, value);
			});
			slider.SetValueWithoutNotify(value);
			Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(1, value);
			break;
		case Fluffiness.LegTip:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetLegFluffiness(2, value);
			});
			slider.SetValueWithoutNotify(value);
			Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(2, value);
			break;
		case Fluffiness.JointInner:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetJointFluffiness(0, value);
			});
			slider.SetValueWithoutNotify(value);
			Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(0, value);
			break;
		case Fluffiness.JointMiddle:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetJointFluffiness(1, value);
			});
			slider.SetValueWithoutNotify(value);
			Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(1, value);
			break;
		case Fluffiness.JointTip:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetJointFluffiness(2, value);
			});
			slider.SetValueWithoutNotify(value);
			Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(2, value);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
