using UnityEngine;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.UI.Mobile_UI;

public class MobileCustomizationController : Singleton<MobileCustomizationController>
{
	protected override void Awake()
	{
		base.Awake();
	}

	public void SaveButtonPosition(string buttonName, Vector2 position)
	{
		SaveController.Save(buttonName + "_Position", position, SaveData.MobileUI);
	}

	public void SaveButtonSize(string buttonName, float size)
	{
		SaveController.Save(buttonName + "_Size", size, SaveData.MobileUI);
	}

	public Vector2 GetButtonPosition(string buttonName)
	{
		return SaveController.Load(buttonName + "_Position", Vector2.zero, SaveData.MobileUI);
	}

	public float GetButtonSize(string buttonName)
	{
		if (SaveController.Exists(buttonName + "Scale_Size", SaveData.MobileUI))
		{
			float num = SaveController.Load(buttonName + "Scale_Size", 1f, SaveData.MobileUI);
			SaveController.Save(buttonName + "_Size", num, SaveData.MobileUI);
			SaveController.DeleteKey(buttonName + "Scale_Size", SaveData.MobileUI);
			return num;
		}
		return SaveController.Load(buttonName + "_Size", 1f, SaveData.MobileUI);
	}

	public bool ButtonExists(string buttonName)
	{
		return SaveController.Exists(buttonName + "_Position", SaveData.MobileUI);
	}
}
