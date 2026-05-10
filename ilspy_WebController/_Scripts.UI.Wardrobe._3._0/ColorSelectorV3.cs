using System;
using System.Collections.Generic;
using MPUIKIT;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.Spider;
using _Scripts.UI.Utils;

namespace _Scripts.UI.Wardrobe._3._0;

public class ColorSelectorV3 : MonoBehaviour
{
	public enum Colorable
	{
		Body,
		Abdomen,
		LegInner,
		LegMiddle,
		LegTip,
		JointInner,
		JointMiddle,
		JointTip,
		EyeBase,
		EyeLeft,
		EyeRight,
		Hat,
		Accessory,
		Shoe
	}

	[Header("Body Part Setup")]
	[SerializeField]
	private Colorable partToColor;

	[SerializeField]
	private Transform colorButtonContainer;

	[SerializeField]
	private WardrobeColorButton colorButtonPrefab;

	[SerializeField]
	private Button customColorButton;

	[SerializeField]
	private MPImage customColorButtonSwatch;

	[SerializeField]
	private TextMeshProUGUI headerLabel;

	private WardrobeControllerV3 wardrobeController;

	private List<Button> buttons = new List<Button>();

	private int index;

	private Button lastActive;

	private void OnEnable()
	{
		wardrobeController = GetComponentInParent<WardrobeControllerV3>();
		InitializeColorButtons();
		SetCorrectColor();
	}

	private void OnDisable()
	{
		foreach (Button button in buttons)
		{
			button.OnDeselect(null);
		}
		SceneExit();
	}

	public void SetCorrectColor()
	{
		Color color = LoadColor();
		customColorButtonSwatch.color = color;
		int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
		if (colorIndex == -1)
		{
			customColorButton.Select();
			customColorButton.GetComponent<ChangeTextOnSelect>().OnSelect(null);
			return;
		}
		if ((bool)lastActive)
		{
			lastActive.GetComponent<Animator>().SetBool("Loaded", value: false);
			lastActive = buttons[colorIndex];
		}
		if (base.gameObject.activeInHierarchy && buttons.Count > 0)
		{
			buttons[colorIndex].Select();
		}
		if (buttons.Count > 0)
		{
			buttons[colorIndex].GetComponent<Animator>().SetBool("Loaded", value: true);
			lastActive = buttons[colorIndex];
		}
	}

	public Color LoadColor()
	{
		Color result = Color.black;
		switch (partToColor)
		{
		case Colorable.Body:
			result = Singleton<CosmeticItemsController>.Instance.LoadBodyColor();
			break;
		case Colorable.Abdomen:
			result = Singleton<CosmeticItemsController>.Instance.LoadAbdomenColor();
			break;
		case Colorable.LegInner:
			result = Singleton<CosmeticItemsController>.Instance.LoadLegColor(0);
			break;
		case Colorable.LegMiddle:
			result = Singleton<CosmeticItemsController>.Instance.LoadLegColor(1);
			break;
		case Colorable.LegTip:
			result = Singleton<CosmeticItemsController>.Instance.LoadLegColor(2);
			break;
		case Colorable.JointInner:
			result = Singleton<CosmeticItemsController>.Instance.LoadJointColor(0);
			break;
		case Colorable.JointMiddle:
			result = Singleton<CosmeticItemsController>.Instance.LoadJointColor(1);
			break;
		case Colorable.JointTip:
			result = Singleton<CosmeticItemsController>.Instance.LoadJointColor(2);
			break;
		case Colorable.Hat:
			result = Singleton<CosmeticItemsController>.Instance.LoadHatColor(index);
			break;
		case Colorable.Accessory:
			result = Singleton<CosmeticItemsController>.Instance.LoadAccessoryColor(index);
			break;
		case Colorable.Shoe:
			result = Singleton<CosmeticItemsController>.Instance.LoadShoeColor(index);
			break;
		case Colorable.EyeBase:
			result = Singleton<CosmeticItemsController>.Instance.LoadEyeColorBase();
			break;
		case Colorable.EyeLeft:
			result = Singleton<CosmeticItemsController>.Instance.LoadEyeColorLeft();
			break;
		case Colorable.EyeRight:
			result = Singleton<CosmeticItemsController>.Instance.LoadEyeColorRight();
			break;
		default:
			Debug.Log("not finished");
			break;
		}
		return result;
	}

	private void ChangeColor(Color color, Action<SpiderCustomization> setColor, Action<Color> saveColor, bool saveSetting = false, bool customColor = false)
	{
		foreach (SpiderCustomization spiderCustomization in wardrobeController.SpiderCustomizations)
		{
			setColor(spiderCustomization);
		}
		if (saveSetting)
		{
			saveColor(color);
			if (!Singleton<GameController>.Instance.InputIsTouch && !customColor)
			{
				wardrobeController.GoBack();
				return;
			}
			OverWriteOldColor(color);
			customColorButtonSwatch.color = color;
		}
	}

	private void ChangeCosmeticItemColor(Color color, Action<SpiderCustomization> setColor, Action<int, Color> saveColor, bool saveSetting = false, bool customColor = false)
	{
		foreach (SpiderCustomization spiderCustomization in wardrobeController.SpiderCustomizations)
		{
			setColor(spiderCustomization);
		}
		if (saveSetting)
		{
			saveColor(index, color);
			if (!Singleton<GameController>.Instance.InputIsTouch && !customColor)
			{
				wardrobeController.GoBack();
				return;
			}
			OverWriteOldColor(color);
			customColorButtonSwatch.color = color;
		}
	}

	private void ChangeBodyColor(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeColor(color, delegate(SpiderCustomization s)
		{
			s.SetBodyColor(color);
		}, Singleton<CosmeticItemsController>.Instance.SaveBodyColor, saveSetting, customColor);
	}

	private void ChangeAbdomenColor(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeColor(color, delegate(SpiderCustomization s)
		{
			s.SetAbdomenColor(color);
		}, Singleton<CosmeticItemsController>.Instance.SaveAbdomenColor, saveSetting, customColor);
	}

	private void ChangeLegColor(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeCosmeticItemColor(color, delegate(SpiderCustomization s)
		{
			s.SetLegColor(index, color);
		}, Singleton<CosmeticItemsController>.Instance.SaveLegColor, saveSetting, customColor);
	}

	private void ChangeJointColor(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeCosmeticItemColor(color, delegate(SpiderCustomization s)
		{
			s.SetJointColor(index, color);
		}, Singleton<CosmeticItemsController>.Instance.SaveJointColor, saveSetting, customColor);
	}

	private void ChangeEyeColorBase(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeColor(color, delegate(SpiderCustomization s)
		{
			s.SetEyeColorBase(color);
		}, Singleton<CosmeticItemsController>.Instance.SaveEyeColorBase, saveSetting, customColor);
	}

	private void ChangeEyeColorLeft(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeColor(color, delegate(SpiderCustomization s)
		{
			s.SetEyeColorLeft(color);
		}, Singleton<CosmeticItemsController>.Instance.SaveEyeColorLeft, saveSetting, customColor);
	}

	private void ChangeEyeColorRight(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeColor(color, delegate(SpiderCustomization s)
		{
			s.SetEyeColorRight(color);
		}, Singleton<CosmeticItemsController>.Instance.SaveEyeColorRight, saveSetting, customColor);
	}

	private void ChangeHatColor(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeCosmeticItemColor(color, delegate(SpiderCustomization s)
		{
			s.SetHatColor(index, color);
		}, Singleton<CosmeticItemsController>.Instance.SaveHatColor, saveSetting, customColor);
	}

	private void ChangeAccessoryColor(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeCosmeticItemColor(color, delegate(SpiderCustomization s)
		{
			s.SetAccessoryColor(index, color);
		}, Singleton<CosmeticItemsController>.Instance.SaveAccessoryColor, saveSetting, customColor);
	}

	private void ChangeShoeColor(Color color, bool saveSetting = false, bool customColor = false)
	{
		ChangeCosmeticItemColor(color, delegate(SpiderCustomization s)
		{
			s.SetShoeColor(index, color);
		}, Singleton<CosmeticItemsController>.Instance.SaveShoeColor, saveSetting, customColor);
	}

	private void OverWriteOldColor(Color color)
	{
		int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
		if (colorIndex != -1)
		{
			if ((bool)lastActive)
			{
				lastActive.GetComponent<Animator>().SetBool("Loaded", value: false);
				lastActive.GetComponent<Animator>().SetTrigger("Normal");
			}
			if (buttons.Count > 0)
			{
				buttons[colorIndex].GetComponent<Animator>().SetBool("Loaded", value: true);
				lastActive = buttons[colorIndex];
			}
			customColorButtonSwatch.color = color;
		}
	}

	private void ClearButtons()
	{
		for (int num = buttons.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(buttons[num].gameObject);
		}
		buttons = new List<Button>();
	}

	private void SetColor(int colorIndex, bool saveSetting = false)
	{
		Color color = Singleton<CosmeticItemsController>.Instance.ColorPalette.colors[colorIndex];
		switch (partToColor)
		{
		case Colorable.Body:
			ChangeBodyColor(color, saveSetting);
			break;
		case Colorable.Abdomen:
			ChangeAbdomenColor(color, saveSetting);
			break;
		case Colorable.LegInner:
		case Colorable.LegMiddle:
		case Colorable.LegTip:
			ChangeLegColor(color, saveSetting);
			break;
		case Colorable.JointInner:
		case Colorable.JointMiddle:
		case Colorable.JointTip:
			ChangeJointColor(color, saveSetting);
			break;
		case Colorable.Hat:
			ChangeHatColor(color, saveSetting);
			break;
		case Colorable.Accessory:
			ChangeAccessoryColor(color, saveSetting);
			break;
		case Colorable.Shoe:
			ChangeShoeColor(color, saveSetting);
			break;
		case Colorable.EyeBase:
			ChangeEyeColorBase(color, saveSetting);
			break;
		case Colorable.EyeLeft:
			ChangeEyeColorLeft(color, saveSetting);
			break;
		case Colorable.EyeRight:
			ChangeEyeColorRight(color, saveSetting);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void InitializeColorButtons()
	{
		ClearButtons();
		int count = Singleton<CosmeticItemsController>.Instance.ColorPalette.colors.Count;
		for (int i = 0; i < count; i++)
		{
			WardrobeColorButton wardrobeColorButton = UnityEngine.Object.Instantiate(colorButtonPrefab, colorButtonContainer);
			wardrobeColorButton.Setup(i, SetColor);
			buttons.Add(wardrobeColorButton.GetButton());
		}
	}

	public void SetPartToColor(Colorable newPartToColor, int newIndex = 0)
	{
		partToColor = newPartToColor;
		index = newIndex;
		bool arachnophobiaMode = SettingsController.ArachnophobiaMode;
		string text;
		switch (partToColor)
		{
		case Colorable.Body:
			text = (arachnophobiaMode ? "Wardrobe_Generic_Color_1" : "Wardrobe_Body Color");
			break;
		case Colorable.LegInner:
			text = (arachnophobiaMode ? "Wardrobe_Generic_Color_2" : $"Wardrobe_Generic_Color_{index + 1}");
			break;
		case Colorable.JointInner:
			text = (arachnophobiaMode ? "Wardrobe_Generic_Color_3" : $"Wardrobe_Generic_Color_{index + 1}");
			break;
		case Colorable.Abdomen:
			text = "Wardrobe_Abdomen Color";
			break;
		case Colorable.EyeBase:
			text = "Wardrobe_Eye Color Base";
			break;
		case Colorable.EyeLeft:
			text = "Wardrobe_Eye Color Left";
			break;
		case Colorable.EyeRight:
			text = "Wardrobe_Eye Color Right";
			break;
		case Colorable.LegMiddle:
		case Colorable.LegTip:
		case Colorable.JointMiddle:
		case Colorable.JointTip:
		case Colorable.Hat:
		case Colorable.Accessory:
		case Colorable.Shoe:
			text = $"Wardrobe_Generic_Color_{index + 1}";
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		string s = text;
		headerLabel.text = DialogueManager.GetLocalizedText(s);
	}

	public void ApplyCustomColor(Color color)
	{
		switch (partToColor)
		{
		case Colorable.Body:
			ChangeBodyColor(color, saveSetting: true, customColor: true);
			break;
		case Colorable.Abdomen:
			ChangeAbdomenColor(color, saveSetting: true, customColor: true);
			break;
		case Colorable.LegInner:
		case Colorable.LegMiddle:
		case Colorable.LegTip:
			ChangeLegColor(color, saveSetting: true, customColor: true);
			break;
		case Colorable.JointInner:
		case Colorable.JointMiddle:
		case Colorable.JointTip:
			ChangeJointColor(color, saveSetting: true, customColor: true);
			break;
		case Colorable.EyeBase:
			ChangeEyeColorBase(color, saveSetting: true, customColor: true);
			break;
		case Colorable.EyeLeft:
			ChangeEyeColorLeft(color, saveSetting: true, customColor: true);
			break;
		case Colorable.EyeRight:
			ChangeEyeColorRight(color, saveSetting: true, customColor: true);
			break;
		case Colorable.Hat:
			ChangeHatColor(color, saveSetting: true, customColor: true);
			break;
		case Colorable.Accessory:
			ChangeAccessoryColor(color, saveSetting: true, customColor: true);
			break;
		case Colorable.Shoe:
			ChangeShoeColor(color, saveSetting: true, customColor: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SceneExit()
	{
		Color activeColor = LoadColor();
		switch (partToColor)
		{
		case Colorable.Body:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetBodyColor(activeColor);
			});
			break;
		case Colorable.Abdomen:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetAbdomenColor(activeColor);
			});
			break;
		case Colorable.LegInner:
		case Colorable.LegMiddle:
		case Colorable.LegTip:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetLegColor(index, activeColor);
			});
			break;
		case Colorable.JointInner:
		case Colorable.JointMiddle:
		case Colorable.JointTip:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetJointColor(index, activeColor);
			});
			break;
		case Colorable.Hat:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetHatColor(index, activeColor);
			});
			break;
		case Colorable.Accessory:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetAccessoryColor(index, activeColor);
			});
			break;
		case Colorable.Shoe:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetShoeColor(index, activeColor);
			});
			break;
		case Colorable.EyeBase:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetEyeColorBase(activeColor);
			});
			break;
		case Colorable.EyeLeft:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetEyeColorLeft(activeColor);
			});
			break;
		case Colorable.EyeRight:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetEyeColorRight(activeColor);
			});
			break;
		default:
			Debug.Log("not finished");
			break;
		}
	}
}
