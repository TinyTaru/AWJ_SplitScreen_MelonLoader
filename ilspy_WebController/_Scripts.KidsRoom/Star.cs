using UnityEngine;

namespace _Scripts.KidsRoom;

public class Star : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private LayerMask whatIsAffectedByExplosion;

	[SerializeField]
	private float explosionRadius = 10f;

	[SerializeField]
	private float explosionForce = 100f;

	private bool canExplode;

	private void OnCollisionEnter(Collision other)
	{
		if (!canExplode)
		{
			return;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, explosionRadius, whatIsAffectedByExplosion);
		for (int i = 0; i < array.Length; i++)
		{
			Rigidbody component = array[i].GetComponent<Rigidbody>();
			if (!(component == null))
			{
				component.AddExplosionForce(explosionForce, base.transform.position, explosionRadius, 0f, ForceMode.Impulse);
			}
		}
		canExplode = false;
	}

	public void SetCanExplode(bool value)
	{
		canExplode = value;
	}
}
