using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Scripts.General;
using _Scripts.Interactable;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.CoinDetector;

public class CoinDetector : Singleton<CoinDetector>
{
	private enum SliderPosition
	{
		Off,
		On
	}

	public enum CoinDetectorState
	{
		Disabled,
		Inactive,
		Active,
		Completed
	}

	[Header("References")]
	[SerializeField]
	private GameObject lockedPanel;

	[SerializeField]
	private GameObject inactivePanel;

	[SerializeField]
	private GameObject activePanel;

	[SerializeField]
	private GameObject completePanel;

	[SerializeField]
	private Image unlockProgressBar;

	[SerializeField]
	private GameObject collectibleStyleIcon;

	[SerializeField]
	private TextMeshProUGUI coinAmountText;

	[SerializeField]
	private MovableObject slider;

	[SerializeField]
	private ConfigurableJoint configurableJointSlider;

	[Header("Parameters")]
	[SerializeField]
	[Tooltip("Coin threshold represented as a percentage of 1.")]
	private int coinThreshold = 90;

	[SerializeField]
	private float activationDistanceThreshold = 100f;

	[SerializeField]
	private float sliderDeactivationThreshold;

	[SerializeField]
	private float sliderActivationThreshold;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter coinDetectorSound;

	[SerializeField]
	private StudioEventEmitter activateSound;

	[SerializeField]
	private StudioEventEmitter deactivateSound;

	[SerializeField]
	private StudioEventEmitter declineSound;

	private const string DISTANCE_PARAMETER = "coin_distance";

	private const string SOUND_PARAMETER = "coindetector_sound";

	private const string VOLUME_PARAMETER = "volume";

	private CoinDetectorState state;

	private int currentCoinsCollectedInLevel;

	private List<InteractableCoin> currentCoins;

	private Transform player;

	private float minDist;

	private SliderPosition sliderPosition;

	public static event Action<float> OnMinDistanceUpdated;

	public static event Action<CoinDetectorState> OnStateChanged;

	public static event Action OnCoinDetectorUnlocked;

	private void OnEnable()
	{
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		SettingsController.OnSettingsUpdated += SettingsController_OnSettingsUpdated;
		if (Singleton<CoinController>.Instance != null)
		{
			Singleton<CoinController>.Instance.OnAllCoinsCollectedInCurrentLevel += CoinController_OnAllCoinsCollectedInCurrentLevel;
		}
		InteractableCoin[] array = UnityEngine.Object.FindObjectsByType<InteractableCoin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnCoinCollected += Coin_OnCoinCollected;
		}
	}

	private void Start()
	{
		if (Singleton<GameController>.Instance != null && Singleton<GameController>.Instance.Player != null)
		{
			player = Singleton<GameController>.Instance.Player.transform;
		}
		if (!(Singleton<CoinController>.Instance == null))
		{
			currentCoinsCollectedInLevel = Singleton<CoinController>.Instance.GetCollectedCoinAmountInCurrentLevel();
			UpdateProgressUI();
			if (currentCoinsCollectedInLevel >= Singleton<CoinController>.Instance.TotalCoinsPerLevel)
			{
				CompleteCoinDetector();
			}
			else if (currentCoinsCollectedInLevel >= coinThreshold)
			{
				DeactivateCoinDetector();
			}
			else
			{
				DisableCoinDetector();
			}
		}
	}

	private void Update()
	{
		float x = slider.transform.localPosition.x;
		switch (state)
		{
		case CoinDetectorState.Disabled:
			if (sliderPosition == SliderPosition.Off && x > sliderActivationThreshold)
			{
				sliderPosition = SliderPosition.On;
				StartCoroutine(DeclineCoroutine());
			}
			if (sliderPosition == SliderPosition.On && x < sliderDeactivationThreshold)
			{
				sliderPosition = SliderPosition.Off;
			}
			break;
		case CoinDetectorState.Inactive:
			if (sliderPosition == SliderPosition.Off && x > sliderActivationThreshold)
			{
				activateSound.Play();
				sliderPosition = SliderPosition.On;
				ActivateCoinDetector();
			}
			break;
		case CoinDetectorState.Active:
		{
			if (sliderPosition == SliderPosition.On && x < sliderDeactivationThreshold)
			{
				deactivateSound.Play();
				sliderPosition = SliderPosition.Off;
				DeactivateCoinDetector();
				break;
			}
			CalculateDistanceFromNearestCoin();
			float num = Mathf.Clamp01(minDist / activationDistanceThreshold);
			CoinDetector.OnMinDistanceUpdated?.Invoke(num);
			SetCoinDetectorSoundDistanceParameter(num);
			if (minDist <= activationDistanceThreshold)
			{
				UnmuteCoinDetectorSound();
			}
			else
			{
				MuteCoinDetectorSound();
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case CoinDetectorState.Completed:
			break;
		}
	}

	private void OnDisable()
	{
		SettingsController.OnSettingsUpdated -= SettingsController_OnSettingsUpdated;
		if (Singleton<CoinController>.Instance != null)
		{
			Singleton<CoinController>.Instance.OnAllCoinsCollectedInCurrentLevel -= CoinController_OnAllCoinsCollectedInCurrentLevel;
		}
		InteractableCoin[] array = UnityEngine.Object.FindObjectsByType<InteractableCoin>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnCoinCollected -= Coin_OnCoinCollected;
		}
	}

	private IEnumerator DeclineCoroutine()
	{
		slider.RemoveAllWebJoints();
		slider.ReleaseWeb();
		declineSound.Play();
		coinAmountText.color = Color.red;
		yield return new WaitForSeconds(0.5f);
		coinAmountText.color = Color.white;
	}

	private void UpdateProgressUI()
	{
		float fillAmount = Mathf.Clamp01((float)currentCoinsCollectedInLevel / (float)coinThreshold);
		unlockProgressBar.fillAmount = fillAmount;
		int num = ((state == CoinDetectorState.Disabled) ? coinThreshold : Singleton<CoinController>.Instance.TotalCoinsPerLevel);
		coinAmountText.text = $"{currentCoinsCollectedInLevel} / {num}";
	}

	private void DisableCoinDetector()
	{
		lockedPanel.SetActive(value: true);
		inactivePanel.SetActive(value: false);
		activePanel.SetActive(value: false);
		completePanel.SetActive(value: false);
		state = CoinDetectorState.Disabled;
		UpdateProgressUI();
	}

	private void UnlockCoinDetector()
	{
		DeactivateCoinDetector();
		CoinDetector.OnCoinDetectorUnlocked?.Invoke();
	}

	private void DeactivateCoinDetector()
	{
		lockedPanel.SetActive(value: false);
		inactivePanel.SetActive(value: true);
		activePanel.SetActive(value: false);
		completePanel.SetActive(value: false);
		JointDrive xDrive = configurableJointSlider.xDrive;
		xDrive.positionSpring = 0f;
		configurableJointSlider.xDrive = xDrive;
		coinDetectorSound.Stop();
		state = CoinDetectorState.Inactive;
		UpdateProgressUI();
		CoinDetector.OnStateChanged?.Invoke(state);
	}

	private void ActivateCoinDetector()
	{
		lockedPanel.SetActive(value: false);
		inactivePanel.SetActive(value: false);
		activePanel.SetActive(value: true);
		completePanel.SetActive(value: false);
		coinDetectorSound.Play();
		coinDetectorSound.SetParameter("volume", 0f);
		UpdateCoinDetectorSound();
		currentCoins = UnityEngine.Object.FindObjectsByType<InteractableCoin>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
		state = CoinDetectorState.Active;
		UpdateProgressUI();
		CoinDetector.OnStateChanged?.Invoke(state);
	}

	private void CompleteCoinDetector()
	{
		lockedPanel.SetActive(value: false);
		inactivePanel.SetActive(value: false);
		activePanel.SetActive(value: false);
		completePanel.SetActive(value: true);
		JointDrive xDrive = configurableJointSlider.xDrive;
		xDrive.positionSpring = 0f;
		configurableJointSlider.xDrive = xDrive;
		coinDetectorSound.Stop();
		collectibleStyleIcon.SetActive(value: false);
		state = CoinDetectorState.Completed;
		UpdateProgressUI();
		CoinDetector.OnStateChanged?.Invoke(state);
	}

	private void CheckCoinThreshold()
	{
		if (currentCoinsCollectedInLevel >= coinThreshold)
		{
			UnlockCoinDetector();
		}
	}

	private void RemoveCoinFromCurrentCoinsList(int coinId)
	{
		if (currentCoins == null)
		{
			return;
		}
		foreach (InteractableCoin currentCoin in currentCoins)
		{
			if (currentCoin.CoinId == coinId)
			{
				currentCoins.Remove(currentCoin);
				break;
			}
		}
	}

	private void CalculateDistanceFromNearestCoin()
	{
		minDist = float.MaxValue;
		if (currentCoins == null || player == null)
		{
			return;
		}
		foreach (InteractableCoin currentCoin in currentCoins)
		{
			float num = Vector3.Distance(player.position, currentCoin.transform.position);
			if (num < minDist)
			{
				minDist = num;
			}
		}
	}

	private void UpdateCoinDetectorMode()
	{
		switch (SettingsController.CoinDetectorMode)
		{
		case CoinDetectorModeEnum.Sound:
			coinDetectorSound.Play();
			coinDetectorSound.SetParameter("volume", 0f);
			break;
		default:
			coinDetectorSound.Stop();
			break;
		case CoinDetectorModeEnum.Both:
			coinDetectorSound.Play();
			coinDetectorSound.SetParameter("volume", 0f);
			break;
		}
		CoinDetector.OnStateChanged?.Invoke(state);
	}

	private void MuteCoinDetectorSound()
	{
		coinDetectorSound.SetParameter("volume", 0f);
	}

	private void UnmuteCoinDetectorSound()
	{
		coinDetectorSound.SetParameter("volume", 1f);
	}

	private void SetCoinDetectorSoundDistanceParameter(float value)
	{
		coinDetectorSound.SetParameter("coin_distance", value);
	}

	private void UpdateCoinDetectorSound()
	{
		coinDetectorSound.SetParameter("coindetector_sound", (float)SettingsController.CoinDetectorSound);
	}

	private void CoinCollectedByPlayer(int coinId)
	{
		currentCoinsCollectedInLevel++;
		RemoveCoinFromCurrentCoinsList(coinId);
		UpdateProgressUI();
		if (state == CoinDetectorState.Disabled)
		{
			CheckCoinThreshold();
		}
	}

	private void Coin_OnCoinCollected(object sender, InteractableCoin.OnCoinCollectedEventArgs e)
	{
		CoinCollectedByPlayer(e.coindId);
	}

	private void SettingsController_OnSettingsUpdated(object sender, EventArgs e)
	{
		UpdateCoinDetectorMode();
		UpdateCoinDetectorSound();
	}

	private void CoinController_OnAllCoinsCollectedInCurrentLevel()
	{
		CompleteCoinDetector();
	}
}
