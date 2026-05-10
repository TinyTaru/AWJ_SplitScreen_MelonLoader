using System;
using _Scripts.General;

namespace _Scripts.LivingRoom;

[Serializable]
public class AncientPotionEffect
{
	private AncientPotionEffectSo ancientPotionEffectSo;

	private float remainingDuration;

	private const float maxDuration = 3600f;

	public event Action<AncientPotionEffectSo> OnEffectRanOut;

	public AncientPotionEffect(AncientPotionEffectSo newEffectSo)
	{
		ancientPotionEffectSo = newEffectSo;
		remainingDuration = ancientPotionEffectSo.effectDuration;
	}

	public AncientPotionEffectSo GetAncientPotionEffectSo()
	{
		return ancientPotionEffectSo;
	}

	public AncientPotionEffectType GetEffectType()
	{
		return ancientPotionEffectSo.effectType;
	}

	public void IncreaseRemainingDuration(float value)
	{
		remainingDuration += value;
		remainingDuration = Math.Min(remainingDuration, 3600f);
	}

	public void ReduceRemainingDuration(float value)
	{
		remainingDuration -= value;
		if (remainingDuration < 0f)
		{
			this.OnEffectRanOut?.Invoke(ancientPotionEffectSo);
		}
	}

	public float GetRemainingDuration()
	{
		return remainingDuration;
	}
}
