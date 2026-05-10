using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.Objects;

[RequireComponent(typeof(SphereCollider))]
public class WebThreadTrigger : MonoBehaviour
{
	[Header("Parameters")]
	[SerializeField]
	private float checkPeriod = 0.5f;

	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	private int activationAmount = 1;

	[SerializeField]
	private int deactivationAmount;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onActivate;

	[SerializeField]
	private UnityEvent onDeactivate;

	private float radius;

	private bool isActive;

	private float checkTimer;

	private void Awake()
	{
		SphereCollider component = GetComponent<SphereCollider>();
		radius = base.transform.lossyScale.x * component.radius;
		checkTimer = checkPeriod;
	}

	private void FixedUpdate()
	{
		checkTimer -= Time.fixedDeltaTime;
		if (checkTimer > 0f)
		{
			return;
		}
		checkTimer = checkPeriod;
		Collider[] results = new Collider[10];
		int num = Physics.OverlapSphereNonAlloc(base.transform.position, radius, results, layerMask);
		if (num >= activationAmount)
		{
			if (!isActive)
			{
				onActivate.Invoke();
			}
			isActive = true;
		}
		else if (num <= deactivationAmount)
		{
			if (isActive)
			{
				onDeactivate.Invoke();
			}
			isActive = false;
		}
	}
}
