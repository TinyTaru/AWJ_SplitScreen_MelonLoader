using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;
using _Scripts.Objects;

namespace _Scripts.KidsRoom;

[RequireComponent(typeof(MovableObject))]
public class Rocket : MonoBehaviour, IInitializable<RocketSaveData>, IHasSaveData<RocketSaveData>
{
	[Header("Parameters")]
	[SerializeField]
	private float maxThrust;

	[SerializeField]
	private float maxSidewaysThrust = 100f;

	[SerializeField]
	private AnimationCurve thrustCurve;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onThrusterStartedEvent;

	[SerializeField]
	private UnityEvent onThrusterFinishedEvent;

	private bool isFlying;

	private float thrustDuration;

	private float thrustTimer;

	private Rigidbody rb;

	private void Start()
	{
		isFlying = false;
		rb = GetComponent<MovableObject>().GetRigidbody();
	}

	private void FixedUpdate()
	{
		if (isFlying)
		{
			float time = thrustTimer / thrustDuration;
			Vector3 vector = base.transform.up * maxThrust * thrustCurve.Evaluate(time);
			rb.AddForce(vector * Time.fixedDeltaTime, ForceMode.Force);
			Vector3 force = base.transform.right * maxSidewaysThrust * thrustCurve.Evaluate(time);
			Vector3 position = base.transform.position + base.transform.up * 2f + base.transform.forward * 1f;
			rb.AddForceAtPosition(force, position, ForceMode.Force);
			thrustTimer += Time.fixedDeltaTime;
			if (thrustTimer >= thrustDuration)
			{
				isFlying = false;
				onThrusterFinishedEvent?.Invoke();
			}
		}
	}

	public void Initialize(RocketSaveData rocketSaveData)
	{
		isFlying = rocketSaveData.isFlying;
		thrustDuration = rocketSaveData.thrustDuration;
		thrustTimer = rocketSaveData.thrustTimer;
		if (isFlying)
		{
			onThrusterStartedEvent?.Invoke();
		}
	}

	public RocketSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Office Computer " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		RocketSaveData result = default(RocketSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.isFlying = isFlying;
		result.thrustDuration = thrustDuration;
		result.thrustTimer = thrustTimer;
		return result;
	}

	public void LaunchRocket(float duration)
	{
		isFlying = true;
		thrustTimer = 0f;
		thrustDuration = duration;
		onThrusterStartedEvent?.Invoke();
	}
}
