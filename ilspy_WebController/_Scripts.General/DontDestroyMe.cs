using UnityEngine;

namespace _Scripts.General;

public class DontDestroyMe : MonoBehaviour
{
	private Transform initialParent;

	private void Awake()
	{
		initialParent = base.transform.parent;
	}

	public void ResetParent()
	{
		base.transform.parent = initialParent;
	}
}
