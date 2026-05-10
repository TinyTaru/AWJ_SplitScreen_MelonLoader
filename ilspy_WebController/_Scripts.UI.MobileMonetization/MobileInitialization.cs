using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UnityConsent;
using _Scripts.General;
using _Scripts.Singletons;
using _Scripts.UnityGamingServices;

namespace _Scripts.UI.MobileMonetization;

public class MobileInitialization : MonoBehaviour
{
	[SerializeField]
	private OnlineBootstrap onlineBootstrap;

	[Header("Age Gating")]
	[SerializeField]
	private GameObject ageGatingPanel;

	[SerializeField]
	private Slider ageGatingSlider;

	[SerializeField]
	private TextMeshProUGUI ageGatingAgeText;

	[SerializeField]
	private int defaultAge = 12;

	[SerializeField]
	private Button decreaseAgeButton;

	[SerializeField]
	private Button increaseAgeButton;

	[SerializeField]
	private TextMeshProUGUI ageGatingAgeTextButtons;

	[SerializeField]
	private Button ageGatingContinueButton;

	[Header("Privacy Policy and Terms")]
	[SerializeField]
	private GameObject privacyPolicyPanel;

	[SerializeField]
	private Toggle allowAnalyticsToggle;

	[Header("Loading Main Menu")]
	[SerializeField]
	private GameObject loadingText;

	private int selectedAge;

	private void Start()
	{
		selectedAge = defaultAge;
		ageGatingPanel.SetActive(value: false);
		privacyPolicyPanel.SetActive(value: false);
		loadingText.SetActive(value: false);
		ConsentManager.SetIsChild(SaveController.Load("IsChild", defaultValue: true, SaveData.General));
		ConsentManager.SetCanGiveConsent(SaveController.Load("CanGiveConsent", defaultValue: false, SaveData.General));
		bool num = SaveController.Load("AgeGatingFinished", defaultValue: false, SaveData.General);
		bool flag = SaveController.Load("PrivacyPolicyAndTermsAccepted", defaultValue: false, SaveData.General);
		if (!num)
		{
			UpdateAgeText();
			ageGatingPanel.SetActive(value: true);
			ageGatingPanel.transform.DOScale(1f, 0.2f).From(0f);
		}
		else if (!flag)
		{
			privacyPolicyPanel.SetActive(value: true);
			privacyPolicyPanel.transform.DOScale(1f, 0.2f).From(0f);
		}
		else
		{
			ConsentManager.LevelPlayConfigured = (Action<bool>)Delegate.Combine(ConsentManager.LevelPlayConfigured, new Action<bool>(LevelPlayConfigured));
			ConsentManager.Setup();
		}
	}

	private void UpdateAgeText()
	{
		int num = (int)ageGatingSlider.value;
		ageGatingAgeText.text = ((num == 18) ? "18+" : num.ToString());
		ageGatingAgeTextButtons.text = ((selectedAge == 18) ? "18+" : selectedAge.ToString());
	}

	public void InputAge()
	{
		UpdateAgeText();
		ageGatingContinueButton.interactable = true;
	}

	public void DecreaseAge()
	{
		selectedAge--;
		selectedAge = Mathf.Max(selectedAge, 3);
		UpdateAgeText();
	}

	public void IncreaseAge()
	{
		selectedAge++;
		selectedAge = Mathf.Min(selectedAge, 18);
		UpdateAgeText();
	}

	public void ConfirmAge()
	{
		int num = selectedAge;
		bool flag;
		bool flag2;
		if (num < 13)
		{
			flag = true;
			flag2 = false;
		}
		else if (num < 16)
		{
			flag = false;
			flag2 = false;
		}
		else
		{
			flag = false;
			flag2 = true;
		}
		ConsentManager.SetIsChild(flag);
		ConsentManager.SetCanGiveConsent(flag2);
		SaveController.Save("IsChild", flag, SaveData.General);
		SaveController.Save("CanGiveConsent", flag2, SaveData.General);
		SaveController.Save("AgeGatingFinished", value: true, SaveData.General);
		ageGatingPanel.SetActive(value: false);
		privacyPolicyPanel.SetActive(value: true);
		privacyPolicyPanel.transform.DOScale(1f, 0.2f).From(0f);
	}

	public void AcceptPrivacyPolicyAndTerms()
	{
		privacyPolicyPanel.SetActive(value: false);
		SaveController.Save("PrivacyPolicyAndTermsAccepted", value: true, SaveData.General);
		bool isOn = allowAnalyticsToggle.isOn;
		ConsentState consentState = EndUserConsent.GetConsentState();
		consentState.AnalyticsIntent = (isOn ? ConsentStatus.Granted : ConsentStatus.Denied);
		EndUserConsent.SetConsentState(consentState);
		SaveController.Save("AllowAnalytics", isOn, SaveData.Settings);
		ConsentManager.LevelPlayConfigured = (Action<bool>)Delegate.Combine(ConsentManager.LevelPlayConfigured, new Action<bool>(LevelPlayConfigured));
		ConsentManager.Setup();
	}

	private void LevelPlayConfigured(bool configurationSuccessful)
	{
		ConsentManager.LevelPlayConfigured = (Action<bool>)Delegate.Remove(ConsentManager.LevelPlayConfigured, new Action<bool>(LevelPlayConfigured));
		loadingText.SetActive(value: true);
		onlineBootstrap.SetPrivacyPolicyAndTermsAccepted(value: true);
	}
}
