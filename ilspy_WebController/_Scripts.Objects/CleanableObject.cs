using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

[DisallowMultipleComponent]
public class CleanableObject : MonoBehaviour, IInitializable<CleanableObjectSaveData>, IHasSaveData<CleanableObjectSaveData>
{
	[Header("Parameters")]
	[Range(0f, 1f)]
	[SerializeField]
	private float initialDirtAmount = 1f;

	[SerializeField]
	private float cleanDuration = 9f;

	[Header("References")]
	[SerializeField]
	private ParticleSystem[] cleaningParticles;

	[SerializeField]
	private ParticleSystem[] cleaningFinishedParticles;

	[Header("Shader")]
	[Tooltip("Float property on your material used to drive the cleaning effect.")]
	[SerializeField]
	private string dirtAmountProperty = "_DirtAmount";

	[Header("Events")]
	[SerializeField]
	private UnityEvent onCleaningFinishedEvent;

	private float dirtAmount;

	private bool isCleaned;

	private bool isCleaning;

	private float oldDirtAmount;

	private readonly List<Renderer> renderers = new List<Renderer>();

	private static MaterialPropertyBlock mpb;

	private static int dirtId;

	public bool IsCleaned => isCleaned;

	public float DirtAmount => dirtAmount;

	private void Reset()
	{
		ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		List<ParticleSystem> list = new List<ParticleSystem>();
		List<ParticleSystem> list2 = new List<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			string text = particleSystem.name.ToUpper();
			if (text.Contains("CLEANING"))
			{
				list.Add(particleSystem);
			}
			if (text.Contains("FINISH"))
			{
				list2.Add(particleSystem);
			}
		}
		if (list.Count > 0)
		{
			cleaningParticles = list.ToArray();
		}
		if (list2.Count > 0)
		{
			cleaningFinishedParticles = list2.ToArray();
		}
	}

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		if (string.IsNullOrEmpty(dirtAmountProperty))
		{
			dirtAmountProperty = "_DirtAmount";
		}
	}

	private void Start()
	{
		dirtId = Shader.PropertyToID(dirtAmountProperty);
		dirtAmount = Mathf.Clamp01(initialDirtAmount);
		oldDirtAmount = dirtAmount;
		isCleaned = dirtAmount <= 0f;
		RebuildRendererCache();
		ApplyToAllRenderers();
		SetCleaningParticlesActive(value: false);
	}

	private void Update()
	{
		if (dirtAmount < oldDirtAmount)
		{
			if (!isCleaning)
			{
				isCleaning = true;
				SetCleaningParticlesActive(value: true);
			}
		}
		else if (isCleaning)
		{
			isCleaning = false;
			SetCleaningParticlesActive(value: false);
		}
		oldDirtAmount = dirtAmount;
	}

	private void RebuildRendererCache()
	{
		renderers.Clear();
		GetComponentsInChildren(includeInactive: true, renderers);
	}

	private void ApplyToAllRenderers()
	{
		foreach (Renderer renderer in renderers)
		{
			if (!(renderer == null))
			{
				renderer.GetPropertyBlock(mpb);
				mpb.SetFloat(dirtId, dirtAmount);
				renderer.SetPropertyBlock(mpb);
			}
		}
	}

	private void SetCleaningParticlesActive(bool value)
	{
		if (cleaningParticles == null)
		{
			return;
		}
		ParticleSystem[] array = cleaningParticles;
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

	private void MarkCleaned()
	{
		if (isCleaned || dirtAmount > 0f)
		{
			return;
		}
		isCleaned = true;
		SetCleaningParticlesActive(value: false);
		ApplyToAllRenderers();
		onCleaningFinishedEvent?.Invoke();
		if (cleaningFinishedParticles == null)
		{
			return;
		}
		for (int i = 0; i < cleaningFinishedParticles.Length; i++)
		{
			ParticleSystem particleSystem = cleaningFinishedParticles[i];
			if (particleSystem != null)
			{
				particleSystem.Play();
			}
		}
	}

	public void Initialize(CleanableObjectSaveData saveData)
	{
		SetInitialDirtAmount(saveData.dirtAmount);
	}

	public CleanableObjectSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Cleanable Object " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		CleanableObjectSaveData result = default(CleanableObjectSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.dirtAmount = dirtAmount;
		return result;
	}

	public void SetInitialDirtAmount(float value)
	{
		initialDirtAmount = value;
		Start();
	}

	public void RemoveDirtAmountPeriodically(float value)
	{
		if (!isCleaned)
		{
			dirtAmount = Mathf.Clamp01(dirtAmount - value / cleanDuration);
			ApplyToAllRenderers();
			MarkCleaned();
		}
	}

	public bool RemoveDirtAmountAbsolute(float value)
	{
		if (isCleaned)
		{
			return false;
		}
		dirtAmount = Mathf.Clamp01(dirtAmount - value);
		ApplyToAllRenderers();
		MarkCleaned();
		return true;
	}

	public void SetDirtAmount(float value)
	{
		dirtAmount = Mathf.Clamp01(value);
		isCleaned = dirtAmount <= 0f;
		ApplyToAllRenderers();
		if (isCleaned)
		{
			MarkCleaned();
		}
	}

	public void PlayCleaningFinishedEffect()
	{
		SetCleaningParticlesActive(value: false);
		ParticleSystem[] array = cleaningFinishedParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}
}
