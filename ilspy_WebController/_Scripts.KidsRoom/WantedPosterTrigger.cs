using System;
using UnityEngine;
using _Scripts.General;

namespace _Scripts.KidsRoom;

[RequireComponent(typeof(BoxCollider))]
public class WantedPosterTrigger : MonoBehaviour
{
	public class OnRewardConditionMetEventArgs : EventArgs
	{
		public GameObject rewardActor;

		public int rewardQuantity;

		public Vector3 rewardSpawnPosition;
	}

	[SerializeField]
	[Tooltip("The CharacterSO corresponding to the character in this wanted poster")]
	private CharacterSO wantedCharacter;

	[SerializeField]
	[Tooltip("The WantedRewardSO specifying the reward details")]
	private WantedRewardSO wantedRewardSO;

	private bool rewardGiven;

	public event EventHandler<OnRewardConditionMetEventArgs> OnRewardConditionMet;

	private void Reset()
	{
		if (GetComponent<BoxCollider>() == null)
		{
			base.gameObject.AddComponent<BoxCollider>();
		}
		base.gameObject.GetComponent<BoxCollider>().isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!rewardGiven)
		{
			WantedObject componentInParent = other.gameObject.GetComponentInParent<WantedObject>();
			if ((bool)componentInParent && componentInParent.GetCharacterSo() == wantedCharacter)
			{
				this.OnRewardConditionMet?.Invoke(this, new OnRewardConditionMetEventArgs
				{
					rewardActor = wantedRewardSO.rewardActor,
					rewardQuantity = wantedRewardSO.rewardQuantity,
					rewardSpawnPosition = base.gameObject.transform.position
				});
				rewardGiven = true;
			}
		}
	}
}
