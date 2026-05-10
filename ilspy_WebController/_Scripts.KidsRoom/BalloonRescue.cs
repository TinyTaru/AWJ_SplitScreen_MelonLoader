using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;

namespace _Scripts.KidsRoom;

public class BalloonRescue : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private int balloonsToCollect = 5;

	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onBallonAddedEvent;

	[SerializeField]
	private UnityEvent onBallonRemovedEvent;

	[SerializeField]
	private UnityEvent onBalloonsCollectedEvent;

	private int balloonsCollected;

	private void Start()
	{
		balloonsCollected = 0;
	}

	private void CheckBallonsCollected()
	{
		if (balloonsCollected >= balloonsToCollect)
		{
			onBalloonsCollectedEvent?.Invoke();
		}
	}

	public void AddBallon()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel && QuestLog.GetQuestState(questName) != QuestState.Success)
		{
			balloonsCollected++;
			balloonsCollected = Mathf.Min(balloonsToCollect, balloonsCollected);
			CheckBallonsCollected();
			onBallonAddedEvent?.Invoke();
		}
	}

	public void RemoveBallon()
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel && QuestLog.GetQuestState(questName) != QuestState.Success)
		{
			balloonsCollected--;
			balloonsCollected = Mathf.Max(0, balloonsCollected);
			onBallonRemovedEvent?.Invoke();
		}
	}
}
