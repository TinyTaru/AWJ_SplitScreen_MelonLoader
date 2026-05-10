using UnityEngine;

public class FlyingFollowTrigger : MonoBehaviour
{
	[SerializeField]
	private bool notifiesParent = true;

	private GameObject parent;

	private void Start()
	{
		parent = base.transform.parent.gameObject;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (notifiesParent)
		{
			parent.SendMessage("OnTriggerEnter", other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (notifiesParent)
		{
			parent.SendMessage("OnTriggerExit", other);
		}
	}
}
