using System;
using System.Collections;
using Dreamteck.Splines;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;
using _Scripts.Singletons;
using _Scripts.Web;

namespace _Scripts.LivingRoom;

public class Fly : MonoBehaviour
{
	[SerializeField]
	private WebJoint webJoint;

	[SerializeField]
	private WebTrigger webTrigger;

	[SerializeField]
	private SplineFollower followTarget;

	[SerializeField]
	private float splineFollowerSpeed = 60f;

	[SerializeField]
	private float flyFollowSpeed = 40f;

	[SerializeField]
	private Animator animator;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onFlyCaught;

	[SerializeField]
	private UnityEvent onFlyReleased;

	private MovableObject movableObject;

	private Rigidbody rb;

	private bool isCaught;

	public event Action OnFlyCaught;

	public event Action OnFlyReleased;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
		rb = movableObject.GetRigidbody();
	}

	private void Start()
	{
		isCaught = false;
		webJoint.SetAnchor(base.transform);
		movableObject.SetLayerToDefault();
		followTarget.followSpeed = splineFollowerSpeed;
	}

	private void FixedUpdate()
	{
		if (!isCaught && !(followTarget == null))
		{
			Vector3 normalized = (followTarget.transform.position - base.transform.position).normalized;
			rb.linearVelocity = normalized * flyFollowSpeed;
			rb.rotation = Quaternion.LookRotation(normalized, Vector3.up);
		}
	}

	private IEnumerator ActivateWebTriggerDelays()
	{
		yield return new WaitForSeconds(0.5f);
		webTrigger.gameObject.SetActive(value: true);
	}

	public void WebThreadTouched(WebThread webThread)
	{
		if (!isCaught && !movableObject.HasPlayerSpider)
		{
			Debug.Log("Fly successfully caught!");
			isCaught = true;
			animator.speed = 2f;
			Singleton<WebController>.Instance.CatchFly(webThread, webJoint);
			followTarget.followSpeed = 0f;
			movableObject.SetLayerToWebbedObject();
			onFlyCaught?.Invoke();
			this.OnFlyCaught?.Invoke();
		}
	}

	public void ReleaseFly()
	{
		Debug.Log("Fly released!");
		isCaught = false;
		animator.speed = 1f;
		followTarget.followSpeed = splineFollowerSpeed;
		if (!movableObject.HasPlayerSpider)
		{
			StartCoroutine(ActivateWebTriggerDelays());
			movableObject.SetLayerToDefault();
		}
		onFlyReleased?.Invoke();
		this.OnFlyReleased?.Invoke();
	}

	public void PlayerSpiderRemoved()
	{
		if (!isCaught)
		{
			StartCoroutine(ActivateWebTriggerDelays());
			movableObject.SetLayerToDefault();
		}
	}
}
