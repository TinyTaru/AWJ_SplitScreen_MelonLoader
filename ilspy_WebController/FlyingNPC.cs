using UnityEngine;

public class FlyingNPC : MonoBehaviour
{
	[SerializeField]
	private FlyingNPCHolder holder;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private float moveSpeed = 10f;

	private void Awake()
	{
		holder = GetComponentInParent<FlyingNPCHolder>();
	}

	private void FixedUpdate()
	{
		if (holder.PointTransform != null)
		{
			Vector3 position = Vector3.MoveTowards(base.transform.position, holder.PointTransform.position, moveSpeed * Time.deltaTime);
			rb.MovePosition(position);
		}
		else
		{
			rb.linearVelocity = Vector3.zero;
		}
		if (holder.PointLookAtTarget != null)
		{
			Vector3 vector = holder.PointLookAtTarget.transform.position - base.transform.position;
			Quaternion rot = Quaternion.LookRotation(new Vector3(vector.x, 0f, vector.z), Vector3.up);
			rb.MoveRotation(rot);
		}
	}
}
