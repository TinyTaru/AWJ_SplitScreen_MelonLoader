using System.Collections;
using FMODUnity;
using UnityEngine;
using _Scripts.Objects;
using _Scripts.Utils;

namespace _Scripts.Office;

public class Catapult : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private MovableObject catapultMovableObject;

	[SerializeField]
	private MovableObject armMovableObject;

	[SerializeField]
	private HingeJoint hingeJointArm;

	[SerializeField]
	private GameObject paperBallDummyPrefab;

	[SerializeField]
	private MovableObject paperBallPrefab;

	[SerializeField]
	private Transform paperBallSpawnPosition;

	[SerializeField]
	private Transform paperBallShootDirection;

	[Header("Parameters")]
	[SerializeField]
	private float springDefaultArm = 500f;

	[SerializeField]
	private float springShootArm = 10000f;

	[SerializeField]
	private float armReleaseAngle = 59f;

	[SerializeField]
	private float armArmingAngle = 40f;

	[SerializeField]
	private float paperBallCollisionDelay = 0.5f;

	[SerializeField]
	private float paperBallRespawnDelay = 1f;

	[SerializeField]
	private float minTensionSoundVelocity = 0.1f;

	[SerializeField]
	private float maxTensionSoundVelocity = 1f;

	[SerializeField]
	private float soundDecay = 1f;

	[SerializeField]
	private float releaseVelocityFactor = 10f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter catapultTensionSound;

	private float armStartingAngle;

	private bool isArmed;

	private MovableObject paperBall;

	private GameObject paperBallDummy;

	private float armAngularVelocity;

	private float armAngle;

	private float armAngleOld;

	private float velocityParameter;

	private float releaseAngle;

	private Quaternion catapultStartRotation;

	private Quaternion armStartRotation;

	private Vector3 armStartPosition;

	private void Start()
	{
		isArmed = false;
		catapultStartRotation = catapultMovableObject.transform.rotation;
		armStartRotation = armMovableObject.transform.rotation;
		armStartPosition = armMovableObject.transform.position;
		armStartingAngle = armMovableObject.transform.localRotation.eulerAngles.z;
		SpawnPaperBallDummy();
		catapultTensionSound.Play();
	}

	private void Update()
	{
		armAngle = armMovableObject.transform.localRotation.eulerAngles.z;
		if (armAngle > armStartingAngle + 5f)
		{
			return;
		}
		if (isArmed)
		{
			if (armAngle > armReleaseAngle)
			{
				Shoot();
			}
		}
		else if (armAngle < armArmingAngle)
		{
			isArmed = true;
		}
		armAngularVelocity = (armAngle - armAngleOld) / Time.deltaTime;
		float b = Mathf.InverseLerp(minTensionSoundVelocity, maxTensionSoundVelocity, Mathf.Abs(armAngularVelocity));
		velocityParameter = _Scripts.Utils.Utils.ExponentialDecay(velocityParameter, b, soundDecay, Time.deltaTime);
		catapultTensionSound.SetParameter("velocity", velocityParameter);
		armAngleOld = armAngle;
	}

	private void OnDestroy()
	{
		catapultTensionSound.Stop();
	}

	private IEnumerator ShootPaperBallCoroutine()
	{
		paperBallDummy.SetActive(value: false);
		paperBall = Object.Instantiate(paperBallPrefab, paperBallDummy.transform.position, paperBallDummy.transform.rotation);
		paperBall.GetComponent<SpawnableObject>().Setup();
		Rigidbody paperBallRigidbody = paperBall.GetRigidbody();
		paperBallRigidbody.detectCollisions = false;
		paperBallRigidbody.linearVelocity = paperBallShootDirection.forward * Mathf.Abs((armReleaseAngle - releaseAngle) * releaseVelocityFactor);
		yield return new WaitForSeconds(paperBallCollisionDelay);
		paperBallRigidbody.detectCollisions = true;
		yield return new WaitForSeconds(paperBallRespawnDelay);
		paperBallDummy.SetActive(value: true);
	}

	private IEnumerator ResetCoroutine()
	{
		Rigidbody catapultRigidbody = catapultMovableObject.GetRigidbody();
		Rigidbody armRigidbody = armMovableObject.GetRigidbody();
		catapultMovableObject.RemoveAllWebJoints();
		armMovableObject.RemoveAllWebJoints();
		yield return new WaitForFixedUpdate();
		catapultRigidbody.isKinematic = true;
		armRigidbody.isKinematic = true;
		catapultRigidbody.rotation = catapultStartRotation;
		catapultRigidbody.angularVelocity = Vector3.zero;
		catapultRigidbody.linearVelocity = Vector3.zero;
		armRigidbody.rotation = armStartRotation;
		armRigidbody.position = armStartPosition;
		armRigidbody.angularVelocity = Vector3.zero;
		armRigidbody.linearVelocity = Vector3.zero;
		catapultRigidbody.isKinematic = false;
		armRigidbody.isKinematic = false;
		isArmed = false;
	}

	private void SpawnPaperBallDummy()
	{
		paperBallDummy = Object.Instantiate(paperBallDummyPrefab, paperBallSpawnPosition.position, paperBallSpawnPosition.rotation, armMovableObject.transform);
	}

	private void Shoot()
	{
		StartCoroutine(ShootPaperBallCoroutine());
		isArmed = false;
	}

	public void Reset()
	{
		StartCoroutine(ResetCoroutine());
	}

	public void SetDefaultSpringForce()
	{
		JointSpring spring = hingeJointArm.spring;
		spring.spring = springDefaultArm;
		hingeJointArm.spring = spring;
	}

	public void SetShootSpringForce()
	{
		releaseAngle = armAngle;
		JointSpring spring = hingeJointArm.spring;
		spring.spring = springShootArm;
		hingeJointArm.spring = spring;
	}
}
