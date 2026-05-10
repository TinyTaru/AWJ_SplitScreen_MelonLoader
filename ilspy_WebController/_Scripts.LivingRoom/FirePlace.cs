using System;
using System.Collections;
using FMODUnity;
using UnityEngine;
using _Scripts.Objects;

namespace _Scripts.LivingRoom;

public class FirePlace : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private CookingArea cookingArea;

	[SerializeField]
	private MeshRenderer scavengerHuntHintMeshRenderer;

	[Header("Particles")]
	[SerializeField]
	private ParticleSystem smallFireParticles;

	[SerializeField]
	private ParticleSystem mediumFireParticles;

	[SerializeField]
	private ParticleSystem bigFireParticles;

	[SerializeField]
	private ParticleSystem smokeParticles;

	[Header("Parameters")]
	[SerializeField]
	private float smallCookingAreaRadius = 10f;

	[SerializeField]
	private float mediumCookingAreaRadius = 15f;

	[SerializeField]
	private float bigCookingAreaRadius = 20f;

	[SerializeField]
	private float smallFireSoundIntensity = 0.2f;

	[SerializeField]
	private float mediumFireSoundIntensity = 0.5f;

	[SerializeField]
	private float bigFireSoundIntensity = 0.8f;

	[SerializeField]
	private float smokeParticlesDuration = 15f;

	[SerializeField]
	private float hintRevealDuration = 3f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter fireSound;

	[SerializeField]
	private StudioEventEmitter douseFlamesSound;

	[SerializeField]
	private StudioEventEmitter hintVisibleSound;

	private SphereCollider cookingAreaCollider;

	private int numberOfWoodenLogs;

	private Coroutine smokeParticlesCoroutine;

	private bool hintIsVisible;

	private static MaterialPropertyBlock mpb;

	private static readonly int hintVisibilityId = Shader.PropertyToID("_HintVisibility");

	public event Action OnScavengerHuntHintVisible;

	private void Awake()
	{
		cookingAreaCollider = cookingArea.GetComponent<SphereCollider>();
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		fireSound.Play();
		cookingArea.StartCooking();
		hintIsVisible = false;
		SetHintVisibility(0f);
		numberOfWoodenLogs = 0;
		UpdateFire();
		if (cookingArea != null)
		{
			cookingArea.OnCookingIngredientAdded += CookingAreaOnOnCookingIngredientAdded;
			cookingArea.OnCookingIngredientRemoved += CookingAreaOnOnCookingIngredientRemoved;
		}
	}

	private void OnDestroy()
	{
		if (cookingArea != null)
		{
			cookingArea.OnCookingIngredientAdded -= CookingAreaOnOnCookingIngredientAdded;
			cookingArea.OnCookingIngredientRemoved -= CookingAreaOnOnCookingIngredientRemoved;
		}
	}

	private IEnumerator SmokeParticlesCoroutine()
	{
		smokeParticles.Play();
		if (!hintIsVisible)
		{
			douseFlamesSound.Play();
			float hintVisibility = 0f;
			while (hintVisibility < 1f)
			{
				hintVisibility += Time.deltaTime / hintRevealDuration;
				SetHintVisibility(hintVisibility);
				yield return null;
			}
			this.OnScavengerHuntHintVisible?.Invoke();
			hintVisibleSound.Play();
		}
		yield return new WaitForSeconds(hintIsVisible ? smokeParticlesDuration : (smokeParticlesDuration - hintRevealDuration));
		smokeParticles.Stop();
		hintIsVisible = true;
	}

	private void SetHintVisibility(float value)
	{
		scavengerHuntHintMeshRenderer.GetPropertyBlock(mpb, 1);
		mpb.SetFloat(hintVisibilityId, value);
		scavengerHuntHintMeshRenderer.SetPropertyBlock(mpb, 1);
	}

	private void UpdateFire()
	{
		Debug.Log($"Updating fire particles for {numberOfWoodenLogs}");
		switch (numberOfWoodenLogs)
		{
		case 0:
			fireSound.SetParameter("fireplace_intensity", smallFireSoundIntensity);
			cookingAreaCollider.radius = smallCookingAreaRadius;
			smallFireParticles.Play();
			mediumFireParticles.Stop();
			bigFireParticles.Stop();
			break;
		case 1:
			fireSound.SetParameter("fireplace_intensity", mediumFireSoundIntensity);
			cookingAreaCollider.radius = mediumCookingAreaRadius;
			smallFireParticles.Stop();
			mediumFireParticles.Play();
			bigFireParticles.Stop();
			break;
		default:
			fireSound.SetParameter("fireplace_intensity", bigFireSoundIntensity);
			cookingAreaCollider.radius = bigCookingAreaRadius;
			smallFireParticles.Stop();
			mediumFireParticles.Stop();
			bigFireParticles.Play();
			break;
		}
	}

	public void DouseFlames()
	{
		if (numberOfWoodenLogs >= 2)
		{
			if (smokeParticlesCoroutine != null)
			{
				StopCoroutine(smokeParticlesCoroutine);
			}
			smokeParticlesCoroutine = StartCoroutine(SmokeParticlesCoroutine());
		}
	}

	private void CookingAreaOnOnCookingIngredientAdded(int _)
	{
		numberOfWoodenLogs = cookingArea.NumberOfSpecialIngredients;
		UpdateFire();
	}

	private void CookingAreaOnOnCookingIngredientRemoved(int _)
	{
		numberOfWoodenLogs = cookingArea.NumberOfSpecialIngredients;
		UpdateFire();
	}
}
