using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Scripts.General;
using _Scripts.LivingRoom;
using _Scripts.Singletons;

namespace _Scripts.Spider;

public class AncientPotionController : Singleton<AncientPotionController>
{
	[SerializeField]
	private float effectUpdateInterval = 1f;

	[SerializeField]
	private AncientPotionEffectSo[] potionEffectSos;

	private List<AncientPotionEffect> activeEffects;

	private Coroutine updateEffectsCoroutine;

	public event Action<AncientPotionEffectSo> OnEffectStarted;

	public event Action<List<AncientPotionEffect>> OnEffectsUpdated;

	public event Action<AncientPotionEffectSo> OnEffectEnded;

	private void Start()
	{
		activeEffects = new List<AncientPotionEffect>();
	}

	private IEnumerator UpdateEffectsCoroutine()
	{
		while (true)
		{
			yield return new WaitForSeconds(effectUpdateInterval);
			if (activeEffects.Count == 0)
			{
				break;
			}
			UpdateEffects();
		}
	}

	private void UpdateEffects()
	{
		foreach (AncientPotionEffect item in activeEffects.ToList())
		{
			item.ReduceRemainingDuration(effectUpdateInterval);
		}
		this.OnEffectsUpdated?.Invoke(activeEffects);
	}

	private void ExtendExistingEffect(AncientPotionEffectSo ancientPotionEffectSo, AncientPotionEffect ancientPotionEffect)
	{
		ancientPotionEffect.IncreaseRemainingDuration(ancientPotionEffectSo.effectDuration);
	}

	private void StartNewEffect(AncientPotionEffectSo ancientPotionEffectSo)
	{
		AncientPotionEffect ancientPotionEffect = new AncientPotionEffect(ancientPotionEffectSo);
		ancientPotionEffect.OnEffectRanOut += NewAncientPotionEffect_OnEffectRanOut;
		activeEffects.Add(ancientPotionEffect);
		if (updateEffectsCoroutine == null)
		{
			updateEffectsCoroutine = StartCoroutine(UpdateEffectsCoroutine());
		}
		if (ancientPotionEffectSo.effectType == AncientPotionEffectType.ColorFilter)
		{
			SettingsController.InvertColors(value: true);
		}
		Debug.Log($"Effect {ancientPotionEffectSo.effectType} started!");
		this.OnEffectStarted?.Invoke(ancientPotionEffectSo);
	}

	public void StartOrExtendEffect(AncientPotionEffectSo ancientPotionEffectSo)
	{
		AncientPotionEffect ancientPotionEffect = activeEffects.FirstOrDefault((AncientPotionEffect x) => x.GetEffectType() == ancientPotionEffectSo.effectType);
		if (ancientPotionEffect != null)
		{
			ExtendExistingEffect(ancientPotionEffectSo, ancientPotionEffect);
		}
		else
		{
			StartNewEffect(ancientPotionEffectSo);
		}
	}

	private void NewAncientPotionEffect_OnEffectRanOut(AncientPotionEffectSo ancientPotionEffectSo)
	{
		Debug.Log($"Effect {ancientPotionEffectSo.effectType} ended!");
		this.OnEffectEnded?.Invoke(ancientPotionEffectSo);
		AncientPotionEffect ancientPotionEffect = activeEffects.FirstOrDefault((AncientPotionEffect x) => x.GetEffectType() == ancientPotionEffectSo.effectType);
		if (ancientPotionEffect != null)
		{
			activeEffects.Remove(ancientPotionEffect);
		}
		if (ancientPotionEffectSo.effectType == AncientPotionEffectType.ColorFilter)
		{
			SettingsController.InvertColors(value: false);
		}
		if (activeEffects.Count == 0 && updateEffectsCoroutine != null)
		{
			StopCoroutine(updateEffectsCoroutine);
			updateEffectsCoroutine = null;
		}
	}
}
