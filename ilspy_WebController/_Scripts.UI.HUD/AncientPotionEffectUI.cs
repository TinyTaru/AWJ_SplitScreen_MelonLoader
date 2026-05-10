using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.LivingRoom;
using _Scripts.Utils;

namespace _Scripts.UI.HUD;

public class AncientPotionEffectUI : MonoBehaviour
{
	[SerializeField]
	private Image effectImage;

	[SerializeField]
	private TextMeshProUGUI remainingDurationText;

	public void Setup(AncientPotionEffectSo ancientPotionEffectSo)
	{
		effectImage.sprite = ancientPotionEffectSo.effectSprite;
		remainingDurationText.text = _Scripts.Utils.Utils.FormatTime(ancientPotionEffectSo.effectDuration, 0);
	}

	public void UpdateRemainingTime(float newDuration)
	{
		remainingDurationText.text = _Scripts.Utils.Utils.FormatTime(newDuration, 0);
	}
}
