using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;

namespace _Scripts.Office;

public class QuestRemoveSpiderWebs : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private int amountOfSpiderWebsToRemove = 1;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onWebRemovedEvent;

	[SerializeField]
	private UnityEvent onAllSpiderWebsRemovedEvent;

	private int activeSpiderWebs;

	private bool allWebsRemoved;

	private void Start()
	{
		activeSpiderWebs = amountOfSpiderWebsToRemove;
		allWebsRemoved = false;
	}

	public void SpiderWebRemoved()
	{
		if (!allWebsRemoved && Singleton<SceneController>.Instance.IsStoryLevel)
		{
			activeSpiderWebs--;
			onWebRemovedEvent?.Invoke();
			if (activeSpiderWebs <= 0)
			{
				allWebsRemoved = true;
				onAllSpiderWebsRemovedEvent?.Invoke();
			}
		}
	}
}
