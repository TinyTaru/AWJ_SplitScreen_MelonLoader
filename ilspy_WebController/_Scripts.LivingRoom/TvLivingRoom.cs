using System;
using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine;
using _Scripts.General;
using _Scripts.Singletons;

namespace _Scripts.LivingRoom;

public class TvLivingRoom : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private int screenMaterialIndex = 1;

	[SerializeField]
	private float startingSoundVolume = 0.5f;

	[SerializeField]
	private float volumeIncrements = 0.1f;

	[Header("Static Screens")]
	[SerializeField]
	private Texture2D blackScreenTexture;

	[Header("Screen Saver")]
	[SerializeField]
	private Texture2D screenSaverTexture;

	[SerializeField]
	private Rigidbody screenSaverLogo;

	[SerializeField]
	private Vector2 screenSaverStartingPositionBounds;

	[SerializeField]
	private float screenSaverAngle;

	[SerializeField]
	private float screenSaverSpeed = 10f;

	[Header("Movies")]
	[SerializeField]
	private Movie spookyMovie;

	[SerializeField]
	private Movie romanticMovie;

	[SerializeField]
	private Movie actionMovie;

	[Header("UI")]
	[SerializeField]
	private CanvasGroup volumeHud;

	[SerializeField]
	private RectTransform volumeBar;

	[SerializeField]
	private TextMeshProUGUI volumeValue;

	[SerializeField]
	private float maxVolumeBarHeight = 20f;

	[SerializeField]
	private float maxVolumeValue = 20f;

	[SerializeField]
	private float volumeHudFadeDuration = 0.5f;

	[SerializeField]
	private float volumeHudDisplayDuration = 2f;

	[SerializeField]
	private GameObject volumeOnImage;

	[SerializeField]
	private GameObject volumeOffImage;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter turnOnSound;

	[SerializeField]
	private StudioEventEmitter turnOffSound;

	private bool isOn;

	private bool screenSaverIsActive;

	private bool movieIsPlaying;

	private MovieType movieType;

	private Movie currentMovie;

	private EventInstance startScreenMusic;

	private EventInstance movieMusic;

	private float soundVolume;

	private Coroutine movieCoroutine;

	private Coroutine volumeHudCoroutine;

	private Vector3 screenSaverMoveDirection;

	private float movieSpeed = 1f;

	private static MaterialPropertyBlock mpb;

	private static readonly int textureId = Shader.PropertyToID("_Texture");

	public bool ScreenSaverIsActive => screenSaverIsActive;

	public event Action OnMovieStarted;

	public event Action OnMovieFinished;

	public event Action OnMovieStopped;

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		isOn = false;
		volumeHud.alpha = 0f;
		screenSaverIsActive = false;
		screenSaverLogo.gameObject.SetActive(value: false);
		movieSpeed = 1f;
		soundVolume = startingSoundVolume;
		ApplyVolume();
	}

	private void FixedUpdate()
	{
		if (screenSaverIsActive && screenSaverLogo.linearVelocity.sqrMagnitude < screenSaverSpeed * screenSaverSpeed)
		{
			screenSaverLogo.linearVelocity = screenSaverLogo.linearVelocity.normalized * screenSaverSpeed;
		}
	}

	private void OnDestroy()
	{
		StopMovieMusic();
		StopStartScreenMusic();
		Singleton<MusicController>.Instance.SetLivingRoomTvOnState(isOn: false);
	}

	private IEnumerator MovieCoroutine()
	{
		StopStartScreenMusic();
		PlayMovieMusic();
		switch (movieType)
		{
		case MovieType.Spooky:
			DialogueLua.SetVariable("LivingRoom.Movie", "Spooky");
			break;
		case MovieType.Romantic:
			DialogueLua.SetVariable("LivingRoom.Movie", "Romantic");
			break;
		case MovieType.Action:
			DialogueLua.SetVariable("LivingRoom.Movie", "Action");
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MovieType.None:
			break;
		}
		this.OnMovieStarted?.Invoke();
		Frame[] frames = currentMovie.frames;
		foreach (Frame frame in frames)
		{
			SetTexture(frame.texture);
			yield return new WaitForSeconds(frame.duration);
		}
		SetTexture(currentMovie.startScreenTexture);
		movieIsPlaying = false;
		this.OnMovieFinished?.Invoke();
		StopMovieMusic();
		PlayStartScreenMusic();
	}

	private IEnumerator VolumeHudCoroutine()
	{
		volumeBar.sizeDelta = new Vector2(volumeBar.sizeDelta.x, soundVolume * maxVolumeBarHeight);
		int num = Mathf.RoundToInt(soundVolume * maxVolumeValue);
		volumeValue.text = num.ToString();
		volumeOnImage.SetActive(num > 0);
		volumeOffImage.SetActive(num == 0);
		yield return volumeHud.DOFade(1f, volumeHudFadeDuration).WaitForCompletion();
		yield return new WaitForSeconds(volumeHudDisplayDuration);
		yield return volumeHud.DOFade(0f, volumeHudFadeDuration).WaitForCompletion();
	}

	private void SetTexture(Texture2D texture)
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		meshRenderer.GetPropertyBlock(mpb, screenMaterialIndex);
		mpb.SetTexture(textureId, texture);
		meshRenderer.SetPropertyBlock(mpb, screenMaterialIndex);
	}

	private void StartScreenSaver()
	{
		if (!screenSaverIsActive)
		{
			screenSaverIsActive = true;
			SetTexture(screenSaverTexture);
			screenSaverLogo.gameObject.SetActive(value: true);
			Vector2 vector = new Vector2(UnityEngine.Random.Range(0f - screenSaverStartingPositionBounds.x, screenSaverStartingPositionBounds.x), UnityEngine.Random.Range(0f - screenSaverStartingPositionBounds.y, screenSaverStartingPositionBounds.y));
			screenSaverLogo.transform.localPosition = vector;
			int num = UnityEngine.Random.Range(0, 4);
			float f = (screenSaverAngle + 90f * (float)num) * (MathF.PI / 180f);
			Vector3 force = (screenSaverLogo.transform.right * Mathf.Cos(f) + screenSaverLogo.transform.up * Mathf.Sin(f)) * 15f;
			screenSaverLogo.AddForce(force, ForceMode.Impulse);
		}
	}

	private void StopScreenSaver()
	{
		screenSaverIsActive = false;
		screenSaverLogo.gameObject.SetActive(value: false);
		screenSaverLogo.linearVelocity = Vector3.zero;
	}

	private void PlayStartScreenMusic()
	{
		Singleton<MusicController>.Instance.SetLivingRoomTvOnState(isOn: true);
		startScreenMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		startScreenMusic = RuntimeManager.CreateInstance(currentMovie.startScreenSoundEvent);
		startScreenMusic.start();
		startScreenMusic.setParameterByName("volume", soundVolume);
	}

	private void StopStartScreenMusic()
	{
		startScreenMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	private void PlayMovieMusic()
	{
		movieMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		movieMusic = RuntimeManager.CreateInstance(currentMovie.scoreSoundEvent);
		movieMusic.start();
		movieMusic.setParameterByName("volume", soundVolume);
	}

	private void StopMovieMusic()
	{
		movieMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
	}

	private void ApplyVolume()
	{
		startScreenMusic.setParameterByName("volume", soundVolume);
		movieMusic.setParameterByName("volume", soundVolume);
	}

	public void ToggleOnOff()
	{
		if (isOn)
		{
			TurnOff();
		}
		else
		{
			TurnOn();
		}
	}

	public void TurnOff()
	{
		if (isOn)
		{
			if (movieIsPlaying)
			{
				this.OnMovieStopped?.Invoke();
			}
			isOn = false;
			movieIsPlaying = false;
			turnOffSound.Play();
			StopScreenSaver();
			SetTexture(blackScreenTexture);
			StopMovieMusic();
			StopStartScreenMusic();
			Singleton<MusicController>.Instance.SetLivingRoomTvOnState(isOn: false);
			if (movieCoroutine != null)
			{
				StopCoroutine(movieCoroutine);
			}
		}
	}

	public void TurnOn()
	{
		if (!isOn)
		{
			isOn = true;
			turnOnSound.Play();
			if (currentMovie == null)
			{
				StartScreenSaver();
			}
			else
			{
				StopScreenSaver();
				SetTexture(currentMovie.startScreenTexture);
				PlayStartScreenMusic();
			}
			if (movieCoroutine != null)
			{
				StopCoroutine(movieCoroutine);
			}
		}
	}

	public void PlayMovie()
	{
		if (!isOn || currentMovie == null)
		{
			return;
		}
		if (movieIsPlaying)
		{
			movieIsPlaying = false;
			if (movieCoroutine != null)
			{
				StopCoroutine(movieCoroutine);
			}
			this.OnMovieStopped?.Invoke();
			SetTexture(currentMovie.startScreenTexture);
			StopMovieMusic();
			PlayStartScreenMusic();
		}
		else
		{
			movieIsPlaying = true;
			movieCoroutine = StartCoroutine(MovieCoroutine());
		}
	}

	public void DecreaseVolume()
	{
		if (isOn)
		{
			soundVolume = Mathf.Clamp01(soundVolume - volumeIncrements);
			ApplyVolume();
			if (volumeHudCoroutine != null)
			{
				StopCoroutine(volumeHudCoroutine);
			}
			volumeHudCoroutine = StartCoroutine(VolumeHudCoroutine());
		}
	}

	public void IncreaseVolume()
	{
		if (isOn)
		{
			soundVolume = Mathf.Clamp01(soundVolume + volumeIncrements);
			ApplyVolume();
			if (volumeHudCoroutine != null)
			{
				StopCoroutine(volumeHudCoroutine);
			}
			volumeHudCoroutine = StartCoroutine(VolumeHudCoroutine());
		}
	}

	public void SetMovieType(MovieType newMovieType)
	{
		if (movieType == newMovieType)
		{
			return;
		}
		movieIsPlaying = false;
		movieType = newMovieType;
		currentMovie = movieType switch
		{
			MovieType.None => null, 
			MovieType.Spooky => spookyMovie, 
			MovieType.Romantic => romanticMovie, 
			MovieType.Action => actionMovie, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (isOn)
		{
			if (movieCoroutine != null)
			{
				StopCoroutine(movieCoroutine);
				StopMovieMusic();
			}
			if (currentMovie == null)
			{
				Singleton<MusicController>.Instance.SetLivingRoomTvOnState(isOn: false);
				StopStartScreenMusic();
				StopMovieMusic();
				StartScreenSaver();
			}
			else
			{
				StopScreenSaver();
				SetTexture(currentMovie.startScreenTexture);
				PlayStartScreenMusic();
			}
		}
	}
}
