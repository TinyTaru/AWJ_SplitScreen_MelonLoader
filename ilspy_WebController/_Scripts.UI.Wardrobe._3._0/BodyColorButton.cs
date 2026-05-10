using System;
using MPUIKIT;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Singletons;

namespace _Scripts.UI.Wardrobe._3._0;

public class BodyColorButton : MonoBehaviour
{
	private enum BodyPart
	{
		Body,
		LegInner,
		LegMiddle,
		LegTip,
		JointInner,
		JointMiddle,
		JointTip,
		Abdomen,
		EyeBase,
		EyeLeft,
		EyeRight
	}

	[Header("References")]
	[SerializeField]
	private Button button;

	[SerializeField]
	private MPImage image;

	[Header("Parameters")]
	[SerializeField]
	private BodyPart bodyPart;

	private void Awake()
	{
		int index = 0;
		ColorSelectorV3.Colorable partToColor;
		switch (bodyPart)
		{
		case BodyPart.Body:
			partToColor = ColorSelectorV3.Colorable.Body;
			break;
		case BodyPart.LegInner:
			partToColor = ColorSelectorV3.Colorable.LegInner;
			break;
		case BodyPart.LegMiddle:
			partToColor = ColorSelectorV3.Colorable.LegMiddle;
			index = 1;
			break;
		case BodyPart.LegTip:
			partToColor = ColorSelectorV3.Colorable.LegTip;
			index = 2;
			break;
		case BodyPart.JointInner:
			partToColor = ColorSelectorV3.Colorable.JointInner;
			break;
		case BodyPart.JointMiddle:
			partToColor = ColorSelectorV3.Colorable.JointMiddle;
			index = 1;
			break;
		case BodyPart.JointTip:
			partToColor = ColorSelectorV3.Colorable.JointTip;
			index = 2;
			break;
		case BodyPart.Abdomen:
			partToColor = ColorSelectorV3.Colorable.Abdomen;
			break;
		case BodyPart.EyeBase:
			partToColor = ColorSelectorV3.Colorable.EyeBase;
			break;
		case BodyPart.EyeLeft:
			partToColor = ColorSelectorV3.Colorable.EyeLeft;
			break;
		case BodyPart.EyeRight:
			partToColor = ColorSelectorV3.Colorable.EyeRight;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		WardrobeControllerV3 wardrobeControllerV3 = GetComponentInParent<WardrobeControllerV3>();
		button.onClick.AddListener(delegate
		{
			wardrobeControllerV3.OpenColorSelectorPanel(partToColor, index);
		});
	}

	private void OnEnable()
	{
		ApplyColor();
	}

	private void ApplyColor()
	{
		switch (bodyPart)
		{
		case BodyPart.Body:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadBodyColor();
			break;
		case BodyPart.LegInner:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadLegColor(0);
			break;
		case BodyPart.LegMiddle:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadLegColor(1);
			break;
		case BodyPart.LegTip:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadLegColor(2);
			break;
		case BodyPart.JointInner:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadJointColor(0);
			break;
		case BodyPart.JointMiddle:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadJointColor(1);
			break;
		case BodyPart.JointTip:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadJointColor(2);
			break;
		case BodyPart.Abdomen:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadAbdomenColor();
			break;
		case BodyPart.EyeBase:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadEyeColorBase();
			break;
		case BodyPart.EyeLeft:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadEyeColorLeft();
			break;
		case BodyPart.EyeRight:
			image.color = Singleton<CosmeticItemsController>.Instance.LoadEyeColorRight();
			break;
		default:
			Debug.LogWarning("Invalid color part: " + bodyPart);
			break;
		}
	}
}
