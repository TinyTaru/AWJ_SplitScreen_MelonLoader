using System.Collections.Generic;
using DG.Tweening;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;

namespace _Scripts.Quests.Level2;

public class CactusWateringQuest : MonoBehaviour
{
	private struct Flower
	{
		public Transform transform;

		public Vector3 targetScale;
	}

	[VariablePopup(false)]
	[SerializeField]
	private string cactiWateredVariable;

	[SerializeField]
	private Transform flowerContainer;

	[SerializeField]
	private Vector2 flowerBloomDuration;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onWatered;

	private bool isWatered;

	private List<Flower> flowers;

	private void Awake()
	{
		flowers = new List<Flower>();
		foreach (Transform item2 in flowerContainer)
		{
			Flower flower = default(Flower);
			flower.transform = item2;
			flower.targetScale = item2.localScale;
			Flower item = flower;
			flowers.Add(item);
			item2.localScale = Vector3.zero;
		}
	}

	public void WaterCactus()
	{
		if (isWatered)
		{
			return;
		}
		isWatered = true;
		foreach (Flower flower in flowers)
		{
			float duration = Random.Range(flowerBloomDuration.x, flowerBloomDuration.y);
			flower.transform.DOScale(flower.targetScale, duration).SetEase(Ease.OutCubic);
		}
		Singleton<MusicController>.Instance.PlaySound("event:/game/general/quest_progress_increment");
		onWatered.Invoke();
	}
}
