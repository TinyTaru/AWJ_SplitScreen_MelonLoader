using System;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

[DisallowMultipleComponent]
public class BurnableObject : MonoBehaviour, IInitializable<BurnableObjectSaveData>, IHasSaveData<BurnableObjectSaveData>
{
	[SerializeField]
	private bool canBurn = true;

	[Range(0f, 1f)]
	[SerializeField]
	private float initialBurnAmount;

	[SerializeField]
	private float burnDuration = 20f;

	[SerializeField]
	private ParticleSystem[] smokeParticles;

	[SerializeField]
	private ParticleSystem[] cookingParticles;

	[SerializeField]
	private string burnAmountProperty = "_BurnAmount";

	private List<Material> materials;

	private float burnAmount;

	private bool isBurned;

	private bool isCooking;

	private float oldBurnAmount;

	private Renderer[] renderers;

	private static MaterialPropertyBlock mpb;

	private int burnId;

	public float BurnAmount => burnAmount;

	public bool IsBurned => isBurned;

	public event Action<float> BurnAmountChanged;

	private void Reset()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		List<ParticleSystem> list = new List<ParticleSystem>();
		List<ParticleSystem> list2 = new List<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			if (particleSystem.name.ToUpper().Contains("SMOKE"))
			{
				list.Add(particleSystem);
			}
			if (particleSystem.name.ToUpper().Contains("COOKING"))
			{
				list2.Add(particleSystem);
			}
		}
		if (list.Count > 0)
		{
			smokeParticles = list.ToArray();
		}
		if (list2.Count > 0)
		{
			cookingParticles = list2.ToArray();
		}
	}

	private void Awake()
	{
		burnId = Shader.PropertyToID(burnAmountProperty);
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		burnAmount = initialBurnAmount;
		oldBurnAmount = burnAmount;
		isCooking = false;
		RebuildRendererCache();
		if (burnAmount >= 1f)
		{
			isBurned = true;
			SetSmokeParticlesActive(value: true);
		}
		ApplyBurnToAll();
	}

	private void Update()
	{
		if (burnAmount > oldBurnAmount)
		{
			if (!isCooking)
			{
				SetCookingParticlesActive(value: true);
				isCooking = true;
			}
		}
		else if (isCooking)
		{
			SetCookingParticlesActive(value: false);
			isCooking = false;
		}
		oldBurnAmount = burnAmount;
	}

	private void RebuildRendererCache()
	{
		renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
	}

	private void ApplyBurnToAll()
	{
		Renderer[] array = renderers;
		foreach (Renderer renderer in array)
		{
			if (!(renderer == null))
			{
				renderer.GetPropertyBlock(mpb);
				mpb.SetFloat(burnId, burnAmount);
				renderer.SetPropertyBlock(mpb);
			}
		}
		if (!Mathf.Approximately(burnAmount, oldBurnAmount))
		{
			this.BurnAmountChanged?.Invoke(burnAmount);
		}
	}

	private void SetSmokeParticlesActive(bool value)
	{
		ParticleSystem[] array = smokeParticles;
		foreach (ParticleSystem particleSystem in array)
		{
			if (!(particleSystem == null))
			{
				if (value)
				{
					particleSystem.Play();
				}
				else
				{
					particleSystem.Stop();
				}
			}
		}
	}

	private void SetCookingParticlesActive(bool value)
	{
		ParticleSystem[] array = cookingParticles;
		foreach (ParticleSystem particleSystem in array)
		{
			if (value)
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
			}
		}
	}

	public void Initialize(BurnableObjectSaveData saveData)
	{
		initialBurnAmount = saveData.burnAmount;
		Start();
	}

	public BurnableObjectSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Burnable Object " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		BurnableObjectSaveData result = default(BurnableObjectSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.burnAmount = burnAmount;
		return result;
	}

	public void AddBurnAmount(float value)
	{
		if (canBurn)
		{
			burnAmount = Mathf.Clamp01(burnAmount + value / burnDuration);
			if (burnAmount >= 1f)
			{
				isBurned = true;
				SetSmokeParticlesActive(value: true);
			}
			ApplyBurnToAll();
		}
	}

	public void RemoveBurnAmount(float value)
	{
		if (canBurn)
		{
			burnAmount = Mathf.Clamp01(burnAmount - value / burnDuration);
			if (burnAmount < 1f)
			{
				isBurned = false;
				SetSmokeParticlesActive(value: false);
			}
			ApplyBurnToAll();
		}
	}

	public void SetBurnAmount(float value)
	{
		burnAmount = Mathf.Clamp01(value);
		if (burnAmount >= 1f)
		{
			isBurned = true;
			SetSmokeParticlesActive(value: true);
		}
		else if (burnAmount == 0f)
		{
			isBurned = false;
			SetSmokeParticlesActive(value: false);
		}
		ApplyBurnToAll();
	}
}
