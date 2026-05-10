using System;
using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.LivingRoom;

public class ScavengerHuntQuestFinishedTrigger : MonoBehaviour
{
	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	private bool isQuestFinished;

	public event Action OnQuestFinished;

	private void Start()
	{
		StartCoroutine(CheckQuestStateCoroutine());
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel && !isQuestFinished)
		{
			BodyMovement componentInParent = other.GetComponentInParent<BodyMovement>();
			if (componentInParent != null && componentInParent.IsPlayer)
			{
				isQuestFinished = true;
				this.OnQuestFinished?.Invoke();
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private IEnumerator CheckQuestStateCoroutine()
	{
		yield return new WaitForSeconds(2f);
		if (QuestLog.GetQuestState(questName) == QuestState.Success)
		{
			isQuestFinished = true;
		}
	}
}
