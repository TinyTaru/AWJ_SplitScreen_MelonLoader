using System;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.Singletons;
using _Scripts.Web;

namespace _Scripts.Emotes;

[Serializable]
public class WebColorSlice : WheelSlice
{
	public Image webColorImage;

	public void SetWebColorImage(WebWheelOptionSo webWheelOptionSo)
	{
		bool flag = (interactable = Singleton<CosmeticItemsController>.Instance.IsItemUnlocked(webWheelOptionSo.cosmeticItemWebSo));
		Color white = Color.white;
		if (!flag)
		{
			white.a = 0.5f;
		}
		webColorImage.color = white;
		webColorImage.sprite = webWheelOptionSo.cosmeticItemWebSo.webSo.webSprite;
		webColorImage.transform.rotation = Quaternion.identity;
	}
}
