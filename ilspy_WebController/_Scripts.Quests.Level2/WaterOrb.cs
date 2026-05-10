using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Quests.Level2;

public class WaterOrb : MonoBehaviour
{
	[QuestPopup(false)]
	[SerializeField]
	private string questName;

	[SerializeField]
	private GameObject waterSplashEffect;

	private void OnCollisionEnter(Collision collision)
	{
		if ((QuestLog.GetQuestState(questName) == QuestState.Active || QuestLog.GetQuestState(questName) == QuestState.Success) && collision.gameObject.CompareTag("Cactus_WateringQuest"))
		{
			CactusWateringQuest componentInParent = collision.gameObject.GetComponentInParent<CactusWateringQuest>();
			if (componentInParent != null)
			{
				componentInParent.WaterCactus();
			}
			if (Singleton<WebController>.Instance.WebTargetObject == base.gameObject)
			{
				Singleton<WebController>.Instance.ReleaseWeb();
			}
			Object.Instantiate(waterSplashEffect, base.transform.position, Quaternion.identity, null);
			Object.Destroy(base.gameObject);
		}
	}
}
