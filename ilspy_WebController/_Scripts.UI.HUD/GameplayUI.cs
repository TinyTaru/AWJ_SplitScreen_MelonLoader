using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.LivingRoom;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.UI.HUD;

public class GameplayUI : MonoBehaviour
{
	[SerializeField]
	private GameObject crossHairs;

	[SerializeField]
	private Image crossHairsImage;

	[SerializeField]
	protected Color webTargetActiveColor;

	[SerializeField]
	protected Color noWebTargetActiveColor;

	[Header("Ancient Potion Effects")]
	[SerializeField]
	private Transform ancientPotionEffectContainer;

	[SerializeField]
	private AncientPotionEffectUI ancientPotionEffectUIPrefab;

	private Dictionary<AncientPotionEffectSo, AncientPotionEffectUI> potionEffectUIDict;

	private void Start()
	{
		if (Singleton<GameController>.Instance != null)
		{
			Singleton<GameController>.Instance.OnGameStateChanged += GameControllerOnGameStateChanged;
		}
		if (Singleton<AncientPotionController>.Instance != null)
		{
			Singleton<AncientPotionController>.Instance.OnEffectStarted += PotionEffect_OnEffectStarted;
			Singleton<AncientPotionController>.Instance.OnEffectsUpdated += PotionController_OnEffectsUpdated;
			Singleton<AncientPotionController>.Instance.OnEffectEnded += PotionEffect_OnEffectEnded;
		}
		if (Singleton<TaskListUI>.Instance != null)
		{
			Singleton<TaskListUI>.Instance.OnShowTodoList += TaskListUI_OnShowTodoList;
			Singleton<TaskListUI>.Instance.OnHideTodoList += TaskListUI_OnHideTodoList;
		}
		potionEffectUIDict = new Dictionary<AncientPotionEffectSo, AncientPotionEffectUI>();
	}

	private void Update()
	{
		crossHairsImage.enabled = Singleton<WebController>.Instance.CanShootWebs;
		crossHairsImage.color = (Singleton<WebController>.Instance.WebTargetActive ? webTargetActiveColor : noWebTargetActiveColor);
	}

	private void GameControllerOnGameStateChanged(object sender, GameController.OnGameStateChangedEventArgs e)
	{
		if (e.State == GameController.GameState.Running)
		{
			crossHairs.SetActive(value: true);
			ancientPotionEffectContainer.gameObject.SetActive(value: true);
		}
		else
		{
			crossHairs.SetActive(value: false);
			ancientPotionEffectContainer.gameObject.SetActive(value: false);
		}
	}

	private void TaskListUI_OnShowTodoList(object sender, EventArgs e)
	{
		crossHairs.SetActive(value: false);
	}

	private void TaskListUI_OnHideTodoList(object sender, EventArgs e)
	{
		crossHairs.SetActive(value: true);
	}

	private void PotionEffect_OnEffectStarted(AncientPotionEffectSo ancientPotionEffectSo)
	{
		AncientPotionEffectUI ancientPotionEffectUI = UnityEngine.Object.Instantiate(ancientPotionEffectUIPrefab, ancientPotionEffectContainer);
		ancientPotionEffectUI.Setup(ancientPotionEffectSo);
		potionEffectUIDict.TryAdd(ancientPotionEffectSo, ancientPotionEffectUI);
	}

	private void PotionController_OnEffectsUpdated(List<AncientPotionEffect> ancientPotionEffects)
	{
		foreach (AncientPotionEffect ancientPotionEffect in ancientPotionEffects)
		{
			potionEffectUIDict.TryGetValue(ancientPotionEffect.GetAncientPotionEffectSo(), out var value);
			if (value != null)
			{
				value.UpdateRemainingTime(ancientPotionEffect.GetRemainingDuration());
			}
		}
	}

	private void PotionEffect_OnEffectEnded(AncientPotionEffectSo ancientPotionEffectSo)
	{
		potionEffectUIDict.TryGetValue(ancientPotionEffectSo, out var value);
		if (value != null)
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
		potionEffectUIDict.Remove(ancientPotionEffectSo);
	}
}
