using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Spider;
using _Scripts.Utils;

namespace _Scripts.Objects;

public class Blender : MonoBehaviour, IAffectedByWater
{
	private enum BlenderState
	{
		Unplugged,
		Off,
		On,
		Disabled
	}

	private enum BlenderMode
	{
		Spin,
		Yeet
	}

	[SerializeField]
	private BlenderState initialBlenderState;

	[SerializeField]
	private float offThreshold;

	[SerializeField]
	private float spinThreshold = 0.5f;

	[SerializeField]
	private float yeetThreshold = 1f;

	[SerializeField]
	private float tolerance = 0.1f;

	[SerializeField]
	private float maxBladeRotationSpeed = 360f;

	[SerializeField]
	private float bladeAcceleration = 1f;

	[SerializeField]
	private float ejectForce = 1000f;

	[SerializeField]
	private float yeetForceMagnitude = 2000f;

	[SerializeField]
	private float disableDuration = 10f;

	[SerializeField]
	private float ejectCooldown = 1f;

	[SerializeField]
	private float maxKnobAngle = 45f;

	[Header("References")]
	[SerializeField]
	private Rigidbody rigidbodySolidPart;

	[SerializeField]
	private Rigidbody knob;

	[SerializeField]
	private Transform blade;

	[SerializeField]
	private Collider killCollider;

	[SerializeField]
	private ParticleSystem killParticlePrefab;

	[SerializeField]
	private Collider bladeCollider;

	[SerializeField]
	private ParticleSystem disabledParticles;

	[SerializeField]
	private GameObject[] ignoredObjects;

	[Header("Events")]
	[SerializeField]
	private UnityEvent onBlenderOffEvent;

	[SerializeField]
	private UnityEvent onBlenderOnEvent;

	[SerializeField]
	private UnityEvent onBlenderSpinActivatedEvent;

	[SerializeField]
	private UnityEvent onBlenderYeetActivatedEvent;

	[SerializeField]
	private UnityEvent onBlenderUnplugEvent;

	[SerializeField]
	private UnityEvent onObjectDestroyedEvent;

	[SerializeField]
	private UnityEvent onObjectEjectedEvent;

	[SerializeField]
	private UnityEvent onPlayerYeetedEvent;

	[SerializeField]
	private UnityEvent onDisabledEvent;

	private bool isPluggedIn;

	private BlenderState blenderState;

	private BlenderMode blenderMode;

	private float bladeSpeed;

	private float disableTimer;

	private float ejectCooldownTimer;

	private void Start()
	{
		blenderState = initialBlenderState;
		blenderMode = BlenderMode.Spin;
		isPluggedIn = blenderState != BlenderState.Unplugged;
		killCollider.enabled = false;
		bladeCollider.enabled = true;
		disabledParticles.Stop();
	}

	private void Update()
	{
		float knobValue = GetKnobValue();
		blade.Rotate(Vector3.up, bladeSpeed * Time.deltaTime);
		ejectCooldownTimer -= Time.deltaTime;
		switch (blenderState)
		{
		case BlenderState.Unplugged:
			bladeSpeed -= bladeAcceleration * Time.deltaTime;
			bladeSpeed = Mathf.Max(bladeSpeed, 0f);
			break;
		case BlenderState.Off:
			bladeSpeed -= bladeAcceleration * Time.deltaTime;
			bladeSpeed = Mathf.Max(bladeSpeed, 0f);
			if (knobValue > spinThreshold - tolerance)
			{
				ActivateSpinMode();
				TurnBlenderOn();
			}
			break;
		case BlenderState.On:
		{
			float num = ((blenderMode == BlenderMode.Yeet) ? maxBladeRotationSpeed : (maxBladeRotationSpeed * 0.5f));
			bladeSpeed = _Scripts.Utils.Utils.ExponentialDecay(bladeSpeed, num, 10f, Time.deltaTime);
			if (bladeSpeed >= num * 0.95f)
			{
				killCollider.enabled = true;
			}
			switch (blenderMode)
			{
			case BlenderMode.Spin:
				if (knobValue < offThreshold + tolerance)
				{
					TurnBlenderOff();
				}
				else if (knobValue > yeetThreshold - tolerance)
				{
					ActivateYeetMode();
				}
				break;
			case BlenderMode.Yeet:
				if (knobValue < spinThreshold + tolerance)
				{
					ActivateSpinMode();
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		}
		case BlenderState.Disabled:
			bladeSpeed -= bladeAcceleration * Time.deltaTime;
			bladeSpeed = Mathf.Max(bladeSpeed, 0f);
			disableTimer -= Time.deltaTime;
			if (disableTimer <= 0f)
			{
				blenderState = (isPluggedIn ? BlenderState.Off : BlenderState.Unplugged);
				knob.isKinematic = false;
				disabledParticles.Stop();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private IEnumerator EjectObjectCoroutine(MovableObject movableObject)
	{
		rigidbodySolidPart.isKinematic = true;
		Debug.Log(movableObject.name);
		movableObject.GetRigidbody().AddForce(rigidbodySolidPart.transform.up * ejectForce, ForceMode.VelocityChange);
		onObjectEjectedEvent?.Invoke();
		yield return new WaitForSeconds(0.1f);
		rigidbodySolidPart.isKinematic = false;
	}

	private IEnumerator YeetPlayerCoroutine(BodyMovement player)
	{
		rigidbodySolidPart.isKinematic = true;
		player.YeetPlayer(yeetForceMagnitude, rigidbodySolidPart.transform.up);
		onPlayerYeetedEvent?.Invoke();
		yield return new WaitForSeconds(0.1f);
		rigidbodySolidPart.isKinematic = false;
	}

	private void AnimateKnob(float value)
	{
	}

	private void TurnBlenderOn()
	{
		blenderState = BlenderState.On;
		onBlenderOnEvent?.Invoke();
	}

	private void TurnBlenderOff()
	{
		AnimateKnob(offThreshold);
		killCollider.enabled = false;
		blenderState = BlenderState.Off;
		bladeCollider.enabled = true;
		onBlenderOffEvent?.Invoke();
	}

	private void ActivateSpinMode()
	{
		AnimateKnob(spinThreshold);
		blenderMode = BlenderMode.Spin;
		bladeCollider.enabled = true;
		onBlenderSpinActivatedEvent?.Invoke();
	}

	private void ActivateYeetMode()
	{
		AnimateKnob(yeetThreshold);
		blenderMode = BlenderMode.Yeet;
		bladeCollider.enabled = false;
		onBlenderYeetActivatedEvent?.Invoke();
	}

	private float GetKnobValue()
	{
		float num = Vector3.SignedAngle(rigidbodySolidPart.transform.up, -knob.transform.right, knob.transform.up);
		if (num > 180f)
		{
			num -= 360f;
		}
		return (num / maxKnobAngle + 1f) / 2f;
	}

	public void TouchedByWater()
	{
		if (isPluggedIn)
		{
			knob.transform.DOLocalRotate(new Vector3(0f, 0f - maxKnobAngle, 0f), 0.2f);
			killCollider.enabled = false;
			bladeCollider.enabled = true;
			blenderState = BlenderState.Disabled;
			disableTimer = disableDuration;
			onDisabledEvent?.Invoke();
			disabledParticles.Play();
		}
	}

	public void PlugIn()
	{
		isPluggedIn = true;
		blenderState = BlenderState.Off;
	}

	public void Unplug()
	{
		isPluggedIn = false;
		switch (blenderState)
		{
		case BlenderState.Off:
			blenderState = BlenderState.Unplugged;
			break;
		case BlenderState.On:
			killCollider.enabled = false;
			blenderState = BlenderState.Unplugged;
			break;
		case BlenderState.Disabled:
			knob.isKinematic = false;
			disabledParticles.Stop();
			blenderState = BlenderState.Unplugged;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case BlenderState.Unplugged:
			break;
		}
		onBlenderUnplugEvent?.Invoke();
	}

	public void EjectObject(MovableObject movableObject)
	{
		if (!ignoredObjects.Contains(movableObject.gameObject) && !(ejectCooldownTimer > 0f))
		{
			ejectCooldownTimer = ejectCooldown;
			StartCoroutine(EjectObjectCoroutine(movableObject));
		}
	}

	public void KillObject(BlendableObject blendableObject)
	{
		PlayKillParticles(blendableObject.ParticleColor, blendableObject.transform.position);
		blendableObject.GetMovableObject().DestroySafely();
		onObjectDestroyedEvent?.Invoke();
	}

	public void YeetPlayer(BodyMovement player)
	{
		if (blenderMode == BlenderMode.Yeet)
		{
			StartCoroutine(YeetPlayerCoroutine(player));
		}
	}

	private void PlayKillParticles(Color color, Vector3 position)
	{
		ParticleSystem particleSystem = UnityEngine.Object.Instantiate(killParticlePrefab, position, Quaternion.identity, base.transform);
		ParticleSystem.MainModule main = particleSystem.main;
		main.startColor = new ParticleSystem.MinMaxGradient(color, color * 0.6f);
		particleSystem.Play();
	}
}
