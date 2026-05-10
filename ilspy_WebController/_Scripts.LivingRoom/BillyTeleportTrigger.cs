using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.LivingRoom;

public class BillyTeleportTrigger : MonoBehaviour
{
	[SerializeField]
	private BodyMovement billy;

	[SerializeField]
	private Transform billyTeleportTransform;

	[SerializeField]
	[VariablePopup(false)]
	private string talkedToBillyVariableName;

	[SerializeField]
	[VariablePopup(false)]
	private string billyNextToVeraVariableName;

	private bool billyTeleported;

	private void Start()
	{
		StartCoroutine(TeleportBillyAtStartCoroutine());
	}

	private void OnTriggerEnter(Collider other)
	{
		if (Singleton<SceneController>.Instance.IsStoryLevel && !billyTeleported)
		{
			BodyMovement componentInParent = other.GetComponentInParent<BodyMovement>();
			if (componentInParent != null && componentInParent.IsPlayer && DialogueLua.GetVariable(talkedToBillyVariableName).asBool)
			{
				billyTeleported = true;
				billy.Respawn(billyTeleportTransform);
				DialogueLua.SetVariable(billyNextToVeraVariableName, true);
				base.gameObject.SetActive(value: false);
			}
		}
	}

	private IEnumerator TeleportBillyAtStartCoroutine()
	{
		yield return new WaitForSeconds(1f);
		if (DialogueLua.GetVariable(billyNextToVeraVariableName).asBool)
		{
			billyTeleported = true;
			billy.Respawn(billyTeleportTransform);
			base.gameObject.SetActive(value: false);
		}
	}
}
