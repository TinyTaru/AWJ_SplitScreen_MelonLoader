using System;
using System.Collections.Generic;
using MPUIKIT;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.UI.Wardrobe;

public class ColorSelector : MonoBehaviour
{
	private enum Colorable
	{
		Body,
		Abdomen,
		Leg,
		Joint,
		Hat1,
		Hat2,
		Accessory1,
		Accessory2,
		Shoe1,
		Shoe2,
		EyeBase,
		EyeLeft,
		EyeRight
	}

	[Header("Body Part Setup")]
	[SerializeField]
	private Colorable partToColor;

	[SerializeField]
	private Transform colorButtonContainer;

	[SerializeField]
	private Button colorButtonPrefab;

	private WardrobeController wardrobeController;

	private List<Color> colors;

	private List<Button> buttons = new List<Button>();

	private int currentColorIndex;

	private int currentHatIndex;

	private Button lastActive;

	private void OnEnable()
	{
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
		int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
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

	private Color LoadColor()
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
		case Colorable.Leg:
			result = Singleton<CosmeticItemsController>.Instance.LoadLegColor(0);
			break;
		case Colorable.Joint:
			result = Singleton<CosmeticItemsController>.Instance.LoadJointColor(0);
			break;
		case Colorable.Hat1:
			result = Singleton<CosmeticItemsController>.Instance.LoadHatColor(0);
			break;
		case Colorable.Hat2:
			result = Singleton<CosmeticItemsController>.Instance.LoadHatColor(1);
			break;
		case Colorable.Accessory1:
			result = Singleton<CosmeticItemsController>.Instance.LoadAccessoryColor(0);
			break;
		case Colorable.Accessory2:
			result = Singleton<CosmeticItemsController>.Instance.LoadAccessoryColor(1);
			break;
		case Colorable.Shoe1:
			result = Singleton<CosmeticItemsController>.Instance.LoadShoeColor(0);
			break;
		case Colorable.Shoe2:
			result = Singleton<CosmeticItemsController>.Instance.LoadShoeColor(1);
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
		}
		return result;
	}

	private void ChangeBodyColor(Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetBodyColor(color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveBodyColor(color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			if ((bool)lastActive)
			{
				lastActive.GetComponent<Animator>().SetBool("Loaded", value: false);
				lastActive.GetComponent<Animator>().SetTrigger("Normal");
				lastActive = buttons[colorIndex];
			}
			if (buttons.Count > 0)
			{
				buttons[colorIndex].GetComponent<Animator>().SetBool("Loaded", value: true);
			}
		}
	}

	private void ChangeAbdomenColor(Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetAbdomenColor(color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveAbdomenColor(color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void ChangeLegColor(Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetLegColor(0, color);
		});
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetLegColor(1, color);
		});
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetLegColor(2, color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveLegColor(0, color);
			Singleton<CosmeticItemsController>.Instance.SaveLegColor(1, color);
			Singleton<CosmeticItemsController>.Instance.SaveLegColor(2, color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void ChangeJointColor(Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetJointColor(0, color);
		});
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetJointColor(1, color);
		});
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetJointColor(2, color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveJointColor(0, color);
			Singleton<CosmeticItemsController>.Instance.SaveJointColor(1, color);
			Singleton<CosmeticItemsController>.Instance.SaveJointColor(2, color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void ChangeEyeColorBase(Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetEyeColorBase(color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveEyeColorBase(color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void ChangeEyeColorLeft(Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetEyeColorLeft(color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveEyeColorLeft(color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void ChangeEyeColorRight(Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetEyeColorRight(color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveEyeColorRight(color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void ChangeHatColor(int index, Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetHatColor(index, color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveHatColor(index, color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void ChangeAccessoryColor(int index, Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetAccessoryColor(index, color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveAccessoryColor(index, color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void ChangeShoeColor(int index, Color color, bool saveSetting = false)
	{
		wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
		{
			x.SetShoeColor(index, color);
		});
		if (saveSetting)
		{
			Singleton<CosmeticItemsController>.Instance.SaveShoeColor(index, color);
			int colorIndex = Singleton<CosmeticItemsController>.Instance.GetColorIndex(color);
			OverWriteOldColor(colorIndex);
		}
	}

	private void OverWriteOldColor(int colorIndex)
	{
		if ((bool)lastActive)
		{
			lastActive.GetComponent<Animator>().SetBool("Loaded", value: false);
			lastActive.GetComponent<Animator>().SetTrigger("Normal");
			lastActive = buttons[colorIndex];
		}
		if (buttons.Count > 0)
		{
			buttons[colorIndex].GetComponent<Animator>().SetBool("Loaded", value: true);
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

	public void Setup(WardrobeController newWardrobeController)
	{
		wardrobeController = newWardrobeController;
	}

	public void InitializeColorButtons()
	{
		ClearButtons();
		colors = Singleton<CosmeticItemsController>.Instance.ColorPalette.colors;
		for (int i = 0; i < colors.Count; i++)
		{
			int colorIndex = i;
			Color color = colors[i];
			Button button = UnityEngine.Object.Instantiate(colorButtonPrefab, colorButtonContainer);
			button.transform.GetChild(0).GetChild(0).GetComponentInChildren<MPImage>()
				.color = color;
			EventTrigger component = button.GetComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.Select;
			entry.callback.AddListener(delegate
			{
				SetColor(colorIndex);
			});
			component.triggers.Add(entry);
			button.onClick.AddListener(delegate
			{
				SetColor(colorIndex, saveSetting: true);
			});
			buttons.Add(button);
		}
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
		case Colorable.Leg:
			ChangeLegColor(color, saveSetting);
			break;
		case Colorable.Joint:
			ChangeJointColor(color, saveSetting);
			break;
		case Colorable.Hat1:
			ChangeHatColor(0, color, saveSetting);
			break;
		case Colorable.Hat2:
			ChangeHatColor(1, color, saveSetting);
			break;
		case Colorable.Accessory1:
			ChangeAccessoryColor(0, color, saveSetting);
			break;
		case Colorable.Accessory2:
			ChangeAccessoryColor(1, color, saveSetting);
			break;
		case Colorable.Shoe1:
			ChangeShoeColor(0, color, saveSetting);
			break;
		case Colorable.Shoe2:
			ChangeShoeColor(1, color, saveSetting);
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

	private void SceneExit()
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
		case Colorable.Leg:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetLegColor(0, activeColor);
			});
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetLegColor(1, activeColor);
			});
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetLegColor(2, activeColor);
			});
			break;
		case Colorable.Joint:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetJointColor(0, activeColor);
			});
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetJointColor(1, activeColor);
			});
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetJointColor(2, activeColor);
			});
			break;
		case Colorable.Hat1:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetHatColor(0, activeColor);
			});
			break;
		case Colorable.Hat2:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetHatColor(1, activeColor);
			});
			break;
		case Colorable.Accessory1:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetAccessoryColor(0, activeColor);
			});
			break;
		case Colorable.Accessory2:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetAccessoryColor(1, activeColor);
			});
			break;
		case Colorable.Shoe1:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetShoeColor(0, activeColor);
			});
			break;
		case Colorable.Shoe2:
			wardrobeController.SpiderCustomizations.ForEach(delegate(SpiderCustomization x)
			{
				x.SetShoeColor(1, activeColor);
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

	public void RandomizeAllColor()
	{
		Color randomColor = Singleton<CosmeticItemsController>.Instance.GetRandomColor();
		switch (partToColor)
		{
		case Colorable.Body:
			ChangeBodyColor(randomColor, saveSetting: true);
			break;
		case Colorable.Abdomen:
			ChangeAbdomenColor(randomColor, saveSetting: true);
			break;
		case Colorable.Leg:
			ChangeLegColor(randomColor, saveSetting: true);
			break;
		case Colorable.Joint:
			ChangeJointColor(randomColor, saveSetting: true);
			break;
		case Colorable.Hat1:
			ChangeHatColor(0, randomColor, saveSetting: true);
			break;
		case Colorable.Hat2:
			ChangeHatColor(1, randomColor, saveSetting: true);
			break;
		case Colorable.Accessory1:
			ChangeAccessoryColor(0, randomColor, saveSetting: true);
			break;
		case Colorable.Accessory2:
			ChangeAccessoryColor(1, randomColor, saveSetting: true);
			break;
		case Colorable.Shoe1:
			ChangeShoeColor(0, randomColor, saveSetting: true);
			break;
		case Colorable.Shoe2:
			ChangeShoeColor(1, randomColor, saveSetting: true);
			break;
		case Colorable.EyeBase:
			ChangeEyeColorBase(Singleton<CosmeticItemsController>.Instance.DefaultEyeColorBase, saveSetting: true);
			break;
		case Colorable.EyeLeft:
			ChangeEyeColorLeft(Singleton<CosmeticItemsController>.Instance.DefaultEyeColorLeft, saveSetting: true);
			break;
		case Colorable.EyeRight:
			ChangeEyeColorRight(Singleton<CosmeticItemsController>.Instance.DefaultEyeColorRight, saveSetting: true);
			break;
		default:
			Debug.Log("not finished");
			break;
		}
		SetCorrectColor();
	}

	public void DefaultColor()
	{
		switch (partToColor)
		{
		case Colorable.Body:
			ChangeBodyColor(Singleton<CosmeticItemsController>.Instance.DefaultBodyColor, saveSetting: true);
			break;
		case Colorable.Abdomen:
			ChangeAbdomenColor(Singleton<CosmeticItemsController>.Instance.DefaultAbdomenColor, saveSetting: true);
			break;
		case Colorable.Leg:
			ChangeLegColor(Singleton<CosmeticItemsController>.Instance.DefaultLegColor, saveSetting: true);
			break;
		case Colorable.Joint:
			ChangeJointColor(Singleton<CosmeticItemsController>.Instance.DefaultJointColor, saveSetting: true);
			break;
		case Colorable.EyeBase:
			ChangeEyeColorBase(Singleton<CosmeticItemsController>.Instance.DefaultEyeColorBase, saveSetting: true);
			break;
		case Colorable.EyeLeft:
			ChangeEyeColorLeft(Singleton<CosmeticItemsController>.Instance.DefaultEyeColorLeft, saveSetting: true);
			break;
		case Colorable.EyeRight:
			ChangeEyeColorRight(Singleton<CosmeticItemsController>.Instance.DefaultEyeColorRight, saveSetting: true);
			break;
		case Colorable.Hat1:
			ChangeHatColor(0, Color.black, saveSetting: true);
			break;
		case Colorable.Hat2:
			ChangeHatColor(1, Color.black, saveSetting: true);
			break;
		case Colorable.Accessory1:
			ChangeAccessoryColor(0, Color.black, saveSetting: true);
			break;
		case Colorable.Accessory2:
			ChangeAccessoryColor(1, Color.black, saveSetting: true);
			break;
		case Colorable.Shoe1:
			ChangeShoeColor(0, Color.black, saveSetting: true);
			break;
		case Colorable.Shoe2:
			ChangeShoeColor(1, Color.black, saveSetting: true);
			break;
		default:
			Debug.Log("not finished");
			break;
		}
		SetCorrectColor();
	}
}
