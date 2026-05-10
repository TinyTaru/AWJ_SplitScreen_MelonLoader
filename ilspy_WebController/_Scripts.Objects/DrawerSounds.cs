using FMODUnity;
using UnityEngine;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
public class DrawerSounds : MonoBehaviour
{
	[SerializeField]
	private float maxMoveVelocity = 1f;

	[SerializeField]
	private float maxShutVelocity = 1f;

	[SerializeField]
	private float maxOpenVelocity = 1f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter drawerMoveSound;

	[SerializeField]
	private StudioEventEmitter drawerShutSound;

	[SerializeField]
	private StudioEventEmitter drawerOpenSound;

	private Rigidbody rb;

	private Joint joint;

	private bool isMoving;

	private Vector3 shutPosition;

	private Vector3 openPosition;

	private bool isShut;

	private bool isOpen;

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		joint = GetComponent<Joint>();
		shutPosition = rb.position;
		openPosition = shutPosition + base.transform.forward * 2f * rb.GetComponent<ConfigurableJoint>().linearLimit.limit;
	}

	private void OnDisable()
	{
		if (drawerMoveSound != null)
		{
			drawerMoveSound.Stop();
		}
		if (drawerShutSound != null)
		{
			drawerShutSound.Stop();
		}
		if (drawerOpenSound != null)
		{
			drawerOpenSound.Stop();
		}
	}

	private void Start()
	{
		isMoving = false;
		isShut = true;
		isOpen = false;
	}

	private void Update()
	{
		if (!(joint == null))
		{
			float sqrMagnitude = rb.linearVelocity.sqrMagnitude;
			HandleMoveSound(sqrMagnitude);
			HandleShutSound(sqrMagnitude);
			HandleOpenSound(sqrMagnitude);
		}
	}

	private void HandleOpenSound(float velocity)
	{
		if (!(drawerOpenSound == null))
		{
			float sqrMagnitude = (rb.position - openPosition).sqrMagnitude;
			if (isOpen && sqrMagnitude > 2f)
			{
				isOpen = false;
			}
			else if (!isOpen && sqrMagnitude < 0.5f)
			{
				isOpen = true;
				drawerOpenSound.Play();
				drawerOpenSound.SetParameter("volume", velocity / maxOpenVelocity);
			}
		}
	}

	private void HandleShutSound(float velocity)
	{
		if (!(drawerShutSound == null))
		{
			float sqrMagnitude = (rb.position - shutPosition).sqrMagnitude;
			if (isShut && sqrMagnitude > 2f)
			{
				isShut = false;
			}
			else if (!isShut && sqrMagnitude < 0.5f)
			{
				isShut = true;
				drawerShutSound.Play();
				drawerShutSound.SetParameter("volume", velocity / maxShutVelocity);
			}
		}
	}

	private void HandleMoveSound(float velocity)
	{
		if (!(drawerMoveSound == null))
		{
			drawerMoveSound.SetParameter("velocity", velocity / maxMoveVelocity);
			if (!isMoving && velocity > 0.2f)
			{
				isMoving = true;
				drawerMoveSound.Play();
			}
			else if (isMoving && velocity < 0.1f)
			{
				isMoving = false;
				drawerMoveSound.Stop();
			}
		}
	}
}
