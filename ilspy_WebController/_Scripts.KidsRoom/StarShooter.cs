using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using _Scripts.LevelSaving;
using _Scripts.Objects;

namespace _Scripts.KidsRoom;

public class StarShooter : MonoBehaviour, IInitializable<StarShooterSaveData>, IHasSaveData<StarShooterSaveData>
{
	[Header("References")]
	[SerializeField]
	private Transform cannon;

	[SerializeField]
	private Transform starSpawnTransform;

	[SerializeField]
	private MovableObject starPrefab;

	[SerializeField]
	private MovableObject shmoopPrefab;

	[SerializeField]
	private MagneticLock reloadMagneticLockStar;

	[SerializeField]
	private MagneticLock reloadMagneticLockShmoop;

	[SerializeField]
	private Rigidbody rigidbodyRotator;

	[SerializeField]
	private float rotationSpeedThresholdRotator = 1f;

	[Header("Parameters")]
	[SerializeField]
	private float aimSpeed = 10f;

	[SerializeField]
	private float shootForce = 1000f;

	[SerializeField]
	private float miniumAngle = -75f;

	[SerializeField]
	private float maximumAngle = 75f;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onStarLoadedEvent;

	[SerializeField]
	private UnityEvent onShmoopLoadedEvent;

	[FormerlySerializedAs("onShootEvent")]
	[SerializeField]
	private UnityEvent onShootButtonPressedEvent;

	[SerializeField]
	private UnityEvent onStarShotEvent;

	[SerializeField]
	private UnityEvent onShmoopShotEvent;

	[SerializeField]
	private UnityEvent onStartAimUpOrDownEvent;

	[SerializeField]
	private UnityEvent onStopAimEvent;

	private float angle;

	private bool isAimingUp;

	private bool isAimingDown;

	private bool starLoaded;

	private bool shmoopLoaded;

	private bool shootButtonPressed;

	private bool isRotating;

	public float ShootForce => shootForce;

	private void Start()
	{
		angle = ((cannon.localEulerAngles.x > 180f) ? (cannon.localEulerAngles.x - 360f) : cannon.localEulerAngles.x);
		starLoaded = false;
		shmoopLoaded = false;
	}

	private void FixedUpdate()
	{
		float num = Time.fixedDeltaTime * aimSpeed;
		if (isAimingUp)
		{
			angle -= num;
		}
		else if (isAimingDown)
		{
			angle += num;
		}
		angle = Mathf.Clamp(angle, miniumAngle, maximumAngle);
		cannon.localRotation = Quaternion.Euler(angle, 0f, 0f);
		if (shootButtonPressed)
		{
			Shoot();
		}
	}

	private IEnumerator LoadStarCoroutine()
	{
		yield return new WaitForSeconds(0.1f);
		MovableObject component = reloadMagneticLockStar.ConnectedObject.GetComponent<MovableObject>();
		reloadMagneticLockStar.DisableMagneticLock();
		reloadMagneticLockShmoop.DisableMagneticLock();
		component.DestroySafely();
		starLoaded = true;
		onStarLoadedEvent?.Invoke();
	}

	private IEnumerator LoadShmoopCoroutine()
	{
		yield return new WaitForSeconds(0.1f);
		MovableObject component = reloadMagneticLockShmoop.ConnectedObject.GetComponent<MovableObject>();
		reloadMagneticLockStar.DisableMagneticLock();
		reloadMagneticLockShmoop.DisableMagneticLock();
		component.DestroySafely();
		shmoopLoaded = true;
		onShmoopLoadedEvent?.Invoke();
	}

	private void Shoot()
	{
		onShootButtonPressedEvent?.Invoke();
		if (!starLoaded && !shmoopLoaded)
		{
			return;
		}
		if (starLoaded)
		{
			starLoaded = false;
			MovableObject movableObject = Object.Instantiate(starPrefab, starSpawnTransform.position, starSpawnTransform.rotation);
			movableObject.GetRigidbody().AddForce(starSpawnTransform.forward * shootForce, ForceMode.Impulse);
			Star component = movableObject.GetComponent<Star>();
			if (component != null)
			{
				component.SetCanExplode(value: true);
			}
			component.GetComponent<SpawnableObject>().Setup();
			onStarShotEvent?.Invoke();
		}
		else if (shmoopLoaded)
		{
			shmoopLoaded = false;
			MovableObject movableObject2 = Object.Instantiate(shmoopPrefab, starSpawnTransform.position, starSpawnTransform.rotation);
			movableObject2.GetRigidbody().AddForce(starSpawnTransform.forward * shootForce, ForceMode.Impulse);
			movableObject2.GetComponent<SpawnableObject>().Setup();
			onShmoopShotEvent?.Invoke();
		}
		reloadMagneticLockStar.EnableMagneticLock();
		reloadMagneticLockShmoop.EnableMagneticLock();
	}

	public void Initialize(StarShooterSaveData starShooterSaveData)
	{
		Debug.Log("Initializing " + starShooterSaveData.name);
		angle = starShooterSaveData.angle;
		starLoaded = starShooterSaveData.starLoaded;
		shmoopLoaded = starShooterSaveData.shmoopLoaded;
		if (shmoopLoaded)
		{
			onShmoopLoadedEvent?.Invoke();
		}
		if (starLoaded)
		{
			onStarLoadedEvent?.Invoke();
		}
	}

	public StarShooterSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Star Shooter " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		StarShooterSaveData result = default(StarShooterSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.angle = angle;
		result.starLoaded = starLoaded;
		result.shmoopLoaded = shmoopLoaded;
		return result;
	}

	public void AimUp()
	{
		isAimingUp = true;
		onStartAimUpOrDownEvent?.Invoke();
	}

	public void AimDown()
	{
		isAimingDown = true;
		onStartAimUpOrDownEvent?.Invoke();
	}

	public void StopAiming()
	{
		isAimingUp = false;
		isAimingDown = false;
		onStopAimEvent?.Invoke();
	}

	public void PressShootButton()
	{
		shootButtonPressed = true;
	}

	public void ReleaseShootButton()
	{
		shootButtonPressed = false;
	}

	public void LoadStar()
	{
		if (!starLoaded && !shmoopLoaded)
		{
			StartCoroutine(LoadStarCoroutine());
		}
	}

	public void LoadShmoop()
	{
		if (!starLoaded && !shmoopLoaded)
		{
			StartCoroutine(LoadShmoopCoroutine());
		}
	}
}
