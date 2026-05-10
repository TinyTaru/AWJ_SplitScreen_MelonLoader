using FMOD.Studio;
using UnityEngine;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.Effects;

public class WindParticleSystem : MonoBehaviour
{
	[SerializeField]
	private float windDeactivateThreshold;

	[SerializeField]
	private float windActivateThreshold;

	[SerializeField]
	private float maxWindVelocity;

	[SerializeField]
	private float windSoundVolume;

	private Rigidbody playerRigidBody;

	private ParticleSystem system;

	private ParticleSystem.EmissionModule systemEmission;

	private EventInstance windSound;

	private bool particlesActive;

	private bool isUnderwater;

	private void Start()
	{
		Singleton<MusicController>.Instance.SetWindLoopVolume(windSoundVolume);
		playerRigidBody = Singleton<GameController>.Instance.Player.Rb;
		Singleton<GameController>.Instance.Player.OnUnderwaterChanged += Player_OnUnderwaterChanged;
		system = GetComponent<ParticleSystem>();
		systemEmission = system.emission;
		particlesActive = false;
		systemEmission.enabled = false;
	}

	private void Update()
	{
		if (!isUnderwater)
		{
			float magnitude = playerRigidBody.linearVelocity.magnitude;
			if (magnitude > windActivateThreshold && !particlesActive)
			{
				particlesActive = true;
				Singleton<MusicController>.Instance.StartWindLoop();
				systemEmission.enabled = true;
			}
			else if (magnitude < windDeactivateThreshold && particlesActive)
			{
				particlesActive = false;
				Singleton<MusicController>.Instance.StopWindLoop();
				systemEmission.enabled = false;
			}
			if (particlesActive)
			{
				float windLoopSpeed = Mathf.InverseLerp(windDeactivateThreshold, maxWindVelocity, magnitude);
				Singleton<MusicController>.Instance.SetWindLoopSpeed(windLoopSpeed);
			}
		}
	}

	private void OnDisable()
	{
		Singleton<MusicController>.Instance.StopWindLoop();
	}

	private void Player_OnUnderwaterChanged(object sender, BodyMovement.OnUnderwaterChangedEventArgs e)
	{
		isUnderwater = e.isUnderwater;
		if (isUnderwater)
		{
			particlesActive = false;
			Singleton<MusicController>.Instance.StopWindLoop();
			systemEmission.enabled = false;
		}
	}
}
