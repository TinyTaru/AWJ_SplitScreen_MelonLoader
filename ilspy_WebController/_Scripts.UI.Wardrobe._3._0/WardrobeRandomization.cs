using System;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.CosmeticItems;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.Wardrobe;

namespace _Scripts.UI.Wardrobe._3._0;

public class WardrobeRandomization : MonoBehaviour
{
	[SerializeField]
	private GameObject spiderToggleContainer;

	[SerializeField]
	private GameObject ballToggleContainer;

	[SerializeField]
	private Selectable exitButton;

	[SerializeField]
	private Selectable bodyButton;

	[SerializeField]
	private Selectable ballButton;

	private WardrobeControllerV3 wardrobeController;

	private bool randomizeBody = true;

	private bool randomizeLegs = true;

	private bool randomizeJoints = true;

	private bool randomizeEye = true;

	private bool randomizeHat = true;

	private bool randomizeAccessory = true;

	private bool randomizeShoe = true;

	private void Awake()
	{
		wardrobeController = GetComponentInParent<WardrobeControllerV3>();
	}

	private void OnEnable()
	{
		bool arachnophobiaMode = SettingsController.ArachnophobiaMode;
		spiderToggleContainer.SetActive(!arachnophobiaMode);
		ballToggleContainer.SetActive(arachnophobiaMode);
		Navigation navigation = default(Navigation);
		navigation.mode = Navigation.Mode.Explicit;
		navigation.selectOnDown = (arachnophobiaMode ? ballButton : bodyButton);
		Navigation navigation2 = navigation;
		exitButton.navigation = navigation2;
	}

	private void ApplyToAllCustomizations(Action<SpiderCustomization> action)
	{
		foreach (SpiderCustomization spiderCustomization in wardrobeController.SpiderCustomizations)
		{
			action(spiderCustomization);
		}
	}

	private void RandomizeBall()
	{
		if (randomizeBody)
		{
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetBodyEnabled(value: true);
			});
			Singleton<CosmeticItemsController>.Instance.SaveBodyEnabled(value: true);
			Color colorBody = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetBodyColor(colorBody);
			});
			Singleton<CosmeticItemsController>.Instance.SaveBodyColor(colorBody);
			Color colorAbdomen = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetAbdomenColor(colorAbdomen);
			});
			Singleton<CosmeticItemsController>.Instance.SaveAbdomenColor(colorAbdomen);
			Color colorLegs = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetLegColor(0, colorLegs);
			});
			Singleton<CosmeticItemsController>.Instance.SaveLegColor(0, colorLegs);
			Color colorJoints = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetJointColor(0, colorJoints);
			});
			Singleton<CosmeticItemsController>.Instance.SaveJointColor(0, colorJoints);
			float bodyFluffiness = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetBodyFluffiness(bodyFluffiness);
			});
			Singleton<CosmeticItemsController>.Instance.SaveBodyFluffiness(bodyFluffiness);
		}
	}

	private void RandomizeBodyAndAbdomen()
	{
		if (randomizeBody)
		{
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetBodyEnabled(value: true);
			});
			Singleton<CosmeticItemsController>.Instance.SaveBodyEnabled(value: true);
			Color colorBody = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetBodyColor(colorBody);
			});
			Singleton<CosmeticItemsController>.Instance.SaveBodyColor(colorBody);
			float bodyFluffiness = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetBodyFluffiness(bodyFluffiness);
			});
			Singleton<CosmeticItemsController>.Instance.SaveBodyFluffiness(bodyFluffiness);
			bool abdomenEnabled = UnityEngine.Random.Range(0, 2) == 1;
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetAbdomenEnabled(abdomenEnabled);
			});
			Singleton<CosmeticItemsController>.Instance.SaveAbdomenEnabled(abdomenEnabled);
			Color colorAbdomen = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetAbdomenColor(colorAbdomen);
			});
			Singleton<CosmeticItemsController>.Instance.SaveAbdomenColor(colorAbdomen);
			float abdomenFluffiness = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetAbdomenFluffiness(abdomenFluffiness);
			});
			Singleton<CosmeticItemsController>.Instance.SaveAbdomenFluffiness(abdomenFluffiness);
		}
	}

	private void RandomizeLegs()
	{
		if (randomizeLegs)
		{
			Color colorLegsInner = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetLegColor(0, colorLegsInner);
			});
			Singleton<CosmeticItemsController>.Instance.SaveLegColor(0, colorLegsInner);
			Color colorLegsMiddle = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetLegColor(1, colorLegsMiddle);
			});
			Singleton<CosmeticItemsController>.Instance.SaveLegColor(1, colorLegsMiddle);
			Color colorLegsTip = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetLegColor(2, colorLegsTip);
			});
			Singleton<CosmeticItemsController>.Instance.SaveLegColor(2, colorLegsTip);
			float fluffinessLegInner = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetLegFluffiness(0, fluffinessLegInner);
			});
			Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(0, fluffinessLegInner);
			float fluffinessLegMiddle = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetLegFluffiness(1, fluffinessLegMiddle);
			});
			Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(1, fluffinessLegMiddle);
			float fluffinessLegTip = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetLegFluffiness(2, fluffinessLegTip);
			});
			Singleton<CosmeticItemsController>.Instance.SaveLegFluffiness(2, fluffinessLegTip);
		}
	}

	private void RandomizeJoints()
	{
		if (randomizeJoints)
		{
			Color colorJointsInner = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetJointColor(0, colorJointsInner);
			});
			Singleton<CosmeticItemsController>.Instance.SaveJointColor(0, colorJointsInner);
			Color colorJointsMiddle = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetJointColor(1, colorJointsMiddle);
			});
			Singleton<CosmeticItemsController>.Instance.SaveJointColor(1, colorJointsMiddle);
			Color colorJointsTip = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetJointColor(2, colorJointsTip);
			});
			Singleton<CosmeticItemsController>.Instance.SaveJointColor(2, colorJointsTip);
			float fluffinessJointInner = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetJointFluffiness(0, fluffinessJointInner);
			});
			Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(0, fluffinessJointInner);
			float fluffinessJointMiddle = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetJointFluffiness(1, fluffinessJointMiddle);
			});
			Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(1, fluffinessJointMiddle);
			float fluffinessJointTip = UnityEngine.Random.Range(0f, 1f);
			ApplyToAllCustomizations(delegate(SpiderCustomization s)
			{
				s.SetJointFluffiness(2, fluffinessJointTip);
			});
			Singleton<CosmeticItemsController>.Instance.SaveJointFluffiness(2, fluffinessJointTip);
		}
	}

	private void RandomizeHat()
	{
		if (!randomizeHat)
		{
			return;
		}
		CosmeticItemHatSo randomUnlockedHat = Singleton<CosmeticItemsController>.Instance.GetRandomUnlockedHat();
		int hatIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(randomUnlockedHat);
		ApplyToAllCustomizations(delegate(SpiderCustomization s)
		{
			s.ChangeHat(hatIndex);
		});
		Singleton<CosmeticItemsController>.Instance.SaveHat(hatIndex);
		Hat hat = randomUnlockedHat.hatSo.hat;
		for (int i = 0; i < 6; i++)
		{
			int index = i;
			if (index < hat.NumberOfColors)
			{
				Color color = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
				ApplyToAllCustomizations(delegate(SpiderCustomization s)
				{
					s.SetHatColor(index, color);
				});
				Singleton<CosmeticItemsController>.Instance.SaveHatColor(index, color);
			}
			if (index < hat.NumberOfEffects)
			{
				float value = hat.GetDefaultEffect(index);
				ApplyToAllCustomizations(delegate(SpiderCustomization s)
				{
					s.SetHatEffect(index, value);
				});
				Singleton<CosmeticItemsController>.Instance.SaveHatEffect(index, value);
			}
		}
	}

	private void RandomizeAccessory()
	{
		if (!randomizeAccessory)
		{
			return;
		}
		CosmeticItemAccessorySo randomUnlockedAccessory = Singleton<CosmeticItemsController>.Instance.GetRandomUnlockedAccessory();
		int accessoryIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(randomUnlockedAccessory);
		ApplyToAllCustomizations(delegate(SpiderCustomization s)
		{
			s.ChangeAccessory(accessoryIndex);
		});
		Singleton<CosmeticItemsController>.Instance.SaveAccessory(accessoryIndex);
		Accessory accessory = randomUnlockedAccessory.accessorySo.accessory;
		for (int i = 0; i < 6; i++)
		{
			int index = i;
			if (index < accessory.NumberOfColors)
			{
				Color color = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
				ApplyToAllCustomizations(delegate(SpiderCustomization s)
				{
					s.SetAccessoryColor(index, color);
				});
				Singleton<CosmeticItemsController>.Instance.SaveAccessoryColor(index, color);
			}
			if (index < accessory.NumberOfEffects)
			{
				float value = accessory.GetDefaultEffect(index);
				ApplyToAllCustomizations(delegate(SpiderCustomization s)
				{
					s.SetAccessoryEffect(index, value);
				});
				Singleton<CosmeticItemsController>.Instance.SaveAccessoryEffect(index, value);
			}
		}
	}

	private void RandomizeShoes()
	{
		if (!randomizeShoe)
		{
			return;
		}
		CosmeticItemShoeSo randomUnlockedShoe = Singleton<CosmeticItemsController>.Instance.GetRandomUnlockedShoe();
		int shoeIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(randomUnlockedShoe);
		ApplyToAllCustomizations(delegate(SpiderCustomization s)
		{
			s.ChangeShoe(shoeIndex);
		});
		Singleton<CosmeticItemsController>.Instance.SaveShoe(shoeIndex);
		Shoe shoe = randomUnlockedShoe.shoeSo.shoe;
		for (int i = 0; i < 6; i++)
		{
			int index = i;
			if (index < shoe.NumberOfColors)
			{
				Color color = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
				ApplyToAllCustomizations(delegate(SpiderCustomization s)
				{
					s.SetShoeColor(index, color);
				});
				Singleton<CosmeticItemsController>.Instance.SaveShoeColor(index, color);
			}
			if (index < shoe.NumberOfEffects)
			{
				float value = shoe.GetDefaultEffect(index);
				ApplyToAllCustomizations(delegate(SpiderCustomization s)
				{
					s.SetShoeEffect(index, value);
				});
				Singleton<CosmeticItemsController>.Instance.SaveShoeEffect(index, value);
			}
		}
	}

	private void RandomizeEyes()
	{
		if (!randomizeEye)
		{
			return;
		}
		CosmeticItemEyeSo randomUnlockedEye = Singleton<CosmeticItemsController>.Instance.GetRandomUnlockedEye();
		int eyeIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(randomUnlockedEye);
		ApplyToAllCustomizations(delegate(SpiderCustomization s)
		{
			s.ChangeEye(eyeIndex);
		});
		Singleton<CosmeticItemsController>.Instance.SaveEye(eyeIndex);
		Eye eye = randomUnlockedEye.eyeSo.eye;
		Color colorBase = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
		ApplyToAllCustomizations(delegate(SpiderCustomization s)
		{
			s.SetEyeColorBase(colorBase);
		});
		Singleton<CosmeticItemsController>.Instance.SaveEyeColorBase(colorBase);
		Color colorLeft = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
		ApplyToAllCustomizations(delegate(SpiderCustomization s)
		{
			s.SetEyeColorLeft(colorLeft);
		});
		Singleton<CosmeticItemsController>.Instance.SaveEyeColorLeft(colorLeft);
		Color colorRight = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
		ApplyToAllCustomizations(delegate(SpiderCustomization s)
		{
			s.SetEyeColorRight(colorRight);
		});
		Singleton<CosmeticItemsController>.Instance.SaveEyeColorRight(colorRight);
		for (int i = 0; i < 6; i++)
		{
			int index = i;
			if (index < eye.NumberOfEffects)
			{
				float value = eye.GetDefaultEffect(index);
				if (eye.GetEffect(index).Name == "Size")
				{
					value = UnityEngine.Random.Range(0.75f, 1.25f);
				}
				else if (eye.GetEffect(index).Name == "Distance")
				{
					value = UnityEngine.Random.Range(0.75f, 1.25f);
				}
				ApplyToAllCustomizations(delegate(SpiderCustomization s)
				{
					s.SetEyeEffect(index, value);
				});
				Singleton<CosmeticItemsController>.Instance.SaveEyeEffect(index, value);
			}
		}
	}

	public void Randomize()
	{
		if (SettingsController.ArachnophobiaMode)
		{
			RandomizeBall();
		}
		else
		{
			RandomizeBodyAndAbdomen();
			RandomizeLegs();
			RandomizeJoints();
		}
		RandomizeEyes();
		RandomizeHat();
		RandomizeAccessory();
		RandomizeShoes();
	}

	public void SetBodyRandomization(bool value)
	{
		randomizeBody = value;
	}

	public void SetLegRandomization(bool value)
	{
		randomizeLegs = value;
	}

	public void SetJointRandomization(bool value)
	{
		randomizeJoints = value;
	}

	public void SetHatRandomization(bool value)
	{
		randomizeHat = value;
	}

	public void SetAccessoryRandomization(bool value)
	{
		randomizeAccessory = value;
	}

	public void SetShoeRandomization(bool value)
	{
		randomizeShoe = value;
	}

	public void SetEyesRandomization(bool value)
	{
		randomizeEye = value;
	}
}
