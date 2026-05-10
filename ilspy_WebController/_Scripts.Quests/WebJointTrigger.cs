using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Quests;

public class WebJointTrigger : MonoBehaviour
{
	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	private int webJointsNeeded;

	[SerializeField]
	private UnityEvent onIncrement = new UnityEvent();

	[SerializeField]
	private UnityEvent onDecrement = new UnityEvent();

	private bool isCovered;

	private void Update()
	{
		if ((QuestLog.GetQuestState("Demo0.3/Level2/ProtectGems") & QuestState.Active) == 0)
		{
			return;
		}
		Collider[] results = new Collider[10];
		if (Physics.OverlapSphereNonAlloc(base.transform.position, base.transform.localScale.x / 2f, results, layerMask) >= webJointsNeeded)
		{
			if (!isCovered)
			{
				onIncrement.Invoke();
			}
			isCovered = true;
		}
		else
		{
			if (isCovered)
			{
				onDecrement.Invoke();
			}
			isCovered = false;
		}
	}
}
