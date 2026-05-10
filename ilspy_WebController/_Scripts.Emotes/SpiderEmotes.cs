using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Singletons;
using _Scripts.Spider;

namespace _Scripts.Emotes;

public class SpiderEmotes : MonoBehaviour
{
	public enum SpiderAnimationType
	{
		Idle,
		Emotes
	}

	[SerializeField]
	private Animator emoteAnimator;

	[SerializeField]
	private EmoteSO[] idleEmotes;

	[SerializeField]
	private EmoteSoundRefs emoteSoundRefs;

	[Header("Particle Systems")]
	[SerializeField]
	private ParticleSystem heartParticleSystem;

	[SerializeField]
	private ParticleSystem cookieParticleSystem;

	[SerializeField]
	private ParticleSystem confettiParticleSystem;

	[SerializeField]
	private ParticleSystem sleepParticleSystem;

	[SerializeField]
	private ParticleSystem coffeeParticleSystem;

	[SerializeField]
	private UnityEvent OnEmoteFinished;

	private BodyMovement bodyMovement;

	private Coroutine disableAnimatorCoroutine;

	private bool playSound;

	private EventInstance sleepZzzEventInstance;

	private EventInstance coffeeDrinkEventInstance;

	private void Start()
	{
		bodyMovement = GetComponent<BodyMovement>();
		SetIdleAnimatorController(SpiderAnimationType.Idle);
	}

	private IEnumerator PerformEmoteCor(string emote, bool stopCurrentEmote = false)
	{
		if (disableAnimatorCoroutine != null)
		{
			StopCoroutine(disableAnimatorCoroutine);
		}
		SetIdleAnimatorController(SpiderAnimationType.Emotes);
		emoteAnimator.enabled = true;
		yield return StartCoroutine(bodyMovement.ResetLegs(BodyMovement.MovementState.Emote));
		if (stopCurrentEmote)
		{
			emoteAnimator.Play("Idle");
		}
		emoteAnimator.SetTrigger(emote);
	}

	private IEnumerator DisableAnimator()
	{
		yield return new WaitForSeconds(0.1f);
		emoteAnimator.enabled = false;
	}

	private EventInstance PlaySound(EventReference eventRef)
	{
		if (!playSound)
		{
			return default(EventInstance);
		}
		return Singleton<MusicController>.Instance.PlaySound(eventRef);
	}

	public void StopEmote()
	{
		StopAllCoroutines();
		emoteAnimator.Play("Idle");
		SetWalkingState();
	}

	public void EmoteFinished()
	{
		Singleton<WebController>.Instance.ShowWebTarget(value: true);
		OnEmoteFinished.Invoke();
	}

	public void SetIdleAnimatorController(SpiderAnimationType type)
	{
	}

	public void PerformEmote(string emote)
	{
		PerformAdditionalActionIfNeeded();
		PerformEmote(emote, playSound: true, stopCurrentEmote: false);
	}

	private void PerformAdditionalActionIfNeeded()
	{
		WalkingNPC component = GetComponent<WalkingNPC>();
		if (!(component == null))
		{
			component.StopSplineFollower();
		}
	}

	public void PerformEmote(string emote, bool playSound, bool stopCurrentEmote)
	{
		if (!(emote == ""))
		{
			if (Singleton<GameController>.Instance.State != GameController.GameState.Dialogue && bodyMovement.IsPlayer)
			{
				Singleton<WebController>.Instance.ShowWebTarget(value: false);
				Singleton<GameController>.Instance.State = GameController.GameState.PerformEmote;
			}
			this.playSound = playSound;
			StartCoroutine(PerformEmoteCor(emote, stopCurrentEmote));
		}
	}

	public void PerformRandomIdleEmote()
	{
		playSound = false;
		string emoteName = idleEmotes[Random.Range(0, idleEmotes.Length)].emoteName;
		StartCoroutine(PerformEmoteCor(emoteName));
	}

	public void SetWalkingState()
	{
		SetIdleAnimatorController(SpiderAnimationType.Idle);
		bodyMovement.State = BodyMovement.MovementState.Walking;
		playSound = true;
		if (Singleton<GameController>.Instance.State != GameController.GameState.Respawn && Singleton<GameController>.Instance.State != GameController.GameState.Dialogue && Singleton<GameController>.Instance.State != GameController.GameState.Debugging && bodyMovement.IsPlayer)
		{
			if (Singleton<GameController>.Instance.LastState == GameController.GameState.SelectEmote)
			{
				Singleton<GameController>.Instance.State = GameController.GameState.Running;
			}
			else
			{
				Singleton<GameController>.Instance.State = Singleton<GameController>.Instance.LastState;
			}
		}
	}

	public void PlayHeartParticleSystem()
	{
		heartParticleSystem.Play();
	}

	public void PlayCookieParticleSystem()
	{
		cookieParticleSystem.Play();
	}

	public void PlayConfettiParticleSystem()
	{
		confettiParticleSystem.Play();
	}

	public void PlaySleepParticleSystem()
	{
		sleepParticleSystem.Play();
	}

	public void PlayCoffeeParticleSystem()
	{
		coffeeParticleSystem.Play();
	}

	public void StopCoffeeParticleSystem()
	{
		coffeeParticleSystem.Stop();
	}

	public void PlayApplauseCheerSound()
	{
		PlaySound(emoteSoundRefs.applauseCheerSoundRef);
	}

	public void PlayApplauseClapSound()
	{
		PlaySound(emoteSoundRefs.applauseClapSoundRef);
	}

	public void PlayCelebrateRevealSound()
	{
		PlaySound(emoteSoundRefs.celebrateRevealSoundRef);
	}

	public void PlayCelebrateThrowSound()
	{
		PlaySound(emoteSoundRefs.celebrateThrowSoundRef);
	}

	public void PlayCelebratePartySound()
	{
		PlaySound(emoteSoundRefs.celebratePartySoundRef);
	}

	public void PlayCircleSound()
	{
		PlaySound(emoteSoundRefs.circleSoundRef);
	}

	public void PlayDabSound()
	{
		PlaySound(emoteSoundRefs.dabSoundRef);
	}

	public void PlayHeartSound()
	{
		PlaySound(emoteSoundRefs.heartSoundRef);
	}

	public void PlayHiSound()
	{
		PlaySound(emoteSoundRefs.hiSoundRef);
	}

	public void PlayKissSound()
	{
		PlaySound(emoteSoundRefs.kissSoundRef);
	}

	public void PlayNoSound()
	{
		PlaySound(emoteSoundRefs.noSoundRef);
	}

	public void PlayOmNomNomRevealSound()
	{
		PlaySound(emoteSoundRefs.omNomNomRevealSoundRef);
	}

	public void PlayOmNomNomEatSound()
	{
		PlaySound(emoteSoundRefs.omNomNomEatSoundRef);
	}

	public void PlayOmNomNomWipeSound()
	{
		PlaySound(emoteSoundRefs.omNomNomWipeSoundRef);
	}

	public void PlayPointUpSound()
	{
		PlaySound(emoteSoundRefs.pointUpSoundRef);
	}

	public void PlaySleepStretchSound()
	{
		PlaySound(emoteSoundRefs.sleepStretchSoundRef);
	}

	public void PlaySleepYawnSound()
	{
		PlaySound(emoteSoundRefs.sleepYawnSoundRef);
	}

	public void PlaySleepZzzSound()
	{
		sleepZzzEventInstance = PlaySound(emoteSoundRefs.sleepZzzSoundRef);
	}

	public void StopSleepZzzSound()
	{
		sleepZzzEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	public void PlayCoffeeRevealSound()
	{
		PlaySound(emoteSoundRefs.sleepCoffeeRevealSoundRef);
	}

	public void PlayCoffeeBlowSound()
	{
		PlaySound(emoteSoundRefs.sleepCoffeeBlowSoundRef);
	}

	public void PlayCoffeeDrinkSound()
	{
		coffeeDrinkEventInstance = PlaySound(emoteSoundRefs.sleepCoffeeDrinkSoundRef);
	}

	public void StopCoffeeDrinkSound()
	{
		coffeeDrinkEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	public void PlayCoffeeFinishSound()
	{
		PlaySound(emoteSoundRefs.sleepCoffeeFinishSoundRef);
	}

	public void PlayUpDownSound()
	{
		PlaySound(emoteSoundRefs.upDownSoundRef);
	}

	public void PlayVSound()
	{
		PlaySound(emoteSoundRefs.vSoundRef);
	}

	public void PlayWeWillRockYouStompSound()
	{
		PlaySound(emoteSoundRefs.weWillRockYouStompSoundRef);
	}

	public void PlayWeWillRockYouClapSound()
	{
		PlaySound(emoteSoundRefs.weWillRockYouClapSoundRef);
	}

	public void PlayYesSound()
	{
		PlaySound(emoteSoundRefs.yesSoundRef);
	}
}
