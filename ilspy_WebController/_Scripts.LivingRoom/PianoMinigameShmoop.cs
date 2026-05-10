using DG.Tweening;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.LivingRoom;

public class PianoMinigameShmoop : MonoBehaviour
{
	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private float jumpForce;

	[SerializeField]
	private float jumpTorque;

	[SerializeField]
	private float maxJumpAngle;

	[SerializeField]
	private float fallForce = 5f;

	[SerializeField]
	private float shrinkDuration = 2f;

	private Transform pianoStringEndpoint;

	private Transform judgmentLineTransform;

	private float shmoopSpeed;

	private int shmoopStringId;

	private bool isCounted;

	private void Update()
	{
		base.transform.position += base.transform.forward * (shmoopSpeed * base.transform.lossyScale.x * Time.deltaTime);
		if (!isCounted && Vector3.Dot(base.transform.position - pianoStringEndpoint.position, pianoStringEndpoint.forward) < 0f)
		{
			isCounted = true;
			Singleton<PianoMinigame>.Instance.OnShmoopEndReached(this, shmoopStringId);
		}
	}

	public void SetPianoStringEndpoint(Transform endpoint)
	{
		pianoStringEndpoint = endpoint;
	}

	public void SetShmoopSpeed(float shmoopSpeed)
	{
		this.shmoopSpeed = shmoopSpeed;
	}

	public void SetShmoopStringId(int pianoStringId)
	{
		shmoopStringId = pianoStringId;
	}

	public void Jump()
	{
		isCounted = true;
		rb.isKinematic = false;
		rb.useGravity = true;
		Vector3 vector = Quaternion.Euler(Random.Range(0.1f, maxJumpAngle), Random.Range(0f, 360f), 0f) * Vector3.up;
		Vector3 force = vector * jumpForce;
		rb.AddForce(force, ForceMode.Impulse);
		Vector3 torque = Vector3.Cross(Vector3.up, vector) * jumpTorque;
		rb.AddTorque(torque, ForceMode.Impulse);
		base.transform.DOScale(0f, shrinkDuration).OnComplete(delegate
		{
			Object.Destroy(base.gameObject);
		});
	}

	public void FallDown()
	{
		rb.isKinematic = false;
		rb.useGravity = true;
		Vector3 force = Vector3.down * fallForce;
		rb.AddForce(force, ForceMode.Impulse);
		Vector3 rhs = Quaternion.Euler(Random.Range(0.1f, maxJumpAngle), Random.Range(0f, 360f), 0f) * Vector3.up;
		Vector3 torque = Vector3.Cross(Vector3.up, rhs) * jumpTorque;
		rb.AddTorque(torque, ForceMode.Impulse);
		base.transform.DOScale(0f, shrinkDuration).OnComplete(delegate
		{
			Object.Destroy(base.gameObject);
		});
	}
}
