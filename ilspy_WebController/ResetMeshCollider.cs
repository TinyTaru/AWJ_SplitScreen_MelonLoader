using UnityEngine;

public class ResetMeshCollider : MonoBehaviour
{
	private void Start()
	{
		Object.Destroy(GetComponent<MeshCollider>());
		base.gameObject.AddComponent<MeshCollider>();
	}
}
