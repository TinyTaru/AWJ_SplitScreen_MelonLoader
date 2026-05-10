using DG.Tweening;
using PixelCrushers.DialogueSystem;
using UnityEngine;

namespace _Scripts.Quests;

[AddComponentMenu("Pixel Crushers/Dialogue System/UI/Standard UI/Selection/Standard UI Quest Tracker")]
public class QuestTracker : StandardUIQuestTracker
{
	[Header("Punch Scale")]
	[SerializeField]
	private Transform punchScaleTransform;

	[SerializeField]
	private float extraScale = 1f;

	[SerializeField]
	private float duration = 1f;

	[SerializeField]
	private int vibrato = 10;

	[SerializeField]
	private float elasticity = 1f;

	private Sequence sequence;

	private void Awake()
	{
		punchScaleTransform.localScale = Vector3.one;
	}

	public override void UpdateTracker()
	{
		if (refreshCoroutine == null && !DOTween.IsTweening(punchScaleTransform, alsoCheckIfIsPlaying: true))
		{
			punchScaleTransform.localScale = Vector3.one;
			punchScaleTransform.DOPunchScale(Vector3.one * extraScale, duration, vibrato, elasticity);
		}
		base.UpdateTracker();
	}
}
