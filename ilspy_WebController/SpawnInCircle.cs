using UnityEngine;

public class SpawnInCircle : MonoBehaviour
{
	public GameObject Prefab;

	public float Radius = 1000f;

	public float Amount = 10000f;

	public bool DoIt;

	private void Update()
	{
		if (DoIt && Prefab != null)
		{
			Vector3 position = base.transform.position;
			Quaternion rotation = base.transform.rotation;
			for (int i = 0; (float)i < Amount; i++)
			{
				int num = Random.Range(0, 360);
				Vector3 vector = new Vector3(Mathf.Cos(num), 0f, Mathf.Sin(num));
				vector = position + vector * Mathf.Sqrt(Random.Range(0f, 1f)) * Radius;
				Object.Instantiate(Prefab, vector, rotation, base.transform.parent);
			}
		}
		DoIt = false;
	}
}
