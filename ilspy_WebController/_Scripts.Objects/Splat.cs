using System;
using UnityEngine;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

public class Splat : MonoBehaviour, IInitializable<SplatSaveData>, IHasSaveData<SplatSaveData>
{
	public class OnSplatCleanedEventEventArgs : EventArgs
	{
		public bool cleanedByKitchenWater;
	}

	[Header("References")]
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private ParticleSystem cleaningFinishedParticles;

	[Header("Parameters")]
	[Range(0f, 1f)]
	[SerializeField]
	private float initialDirtAmount = 1f;

	private float dirtAmount;

	private Soap soapInRange;

	private bool isCleaned;

	private static MaterialPropertyBlock mpb;

	private static readonly int DirtId = Shader.PropertyToID("_DirtAmount");

	public float DirtAmount => dirtAmount;

	public event EventHandler<OnSplatCleanedEventEventArgs> OnSplatCleanedEvent;

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		dirtAmount = initialDirtAmount;
	}

	private void Update()
	{
		if (!isCleaned && soapInRange != null)
		{
			dirtAmount -= soapInRange.CleaningAmount * Time.deltaTime;
			ApplyMpb();
			if (dirtAmount <= 0f)
			{
				MarkClean();
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!isCleaned)
		{
			Soap componentInParent = other.GetComponentInParent<Soap>();
			if (componentInParent != null)
			{
				soapInRange = componentInParent;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInParent<Soap>() != null)
		{
			soapInRange = null;
		}
	}

	private void ApplyMpb()
	{
		if ((bool)meshRenderer)
		{
			meshRenderer.GetPropertyBlock(mpb);
			mpb.SetFloat(DirtId, dirtAmount);
			meshRenderer.SetPropertyBlock(mpb);
		}
	}

	private void MarkClean(bool cleanedByKitchenWater = false)
	{
		if (!isCleaned)
		{
			isCleaned = true;
			dirtAmount = 0f;
			meshRenderer.enabled = false;
			ApplyMpb();
			cleaningFinishedParticles.Play();
			this.OnSplatCleanedEvent?.Invoke(this, new OnSplatCleanedEventEventArgs
			{
				cleanedByKitchenWater = cleanedByKitchenWater
			});
		}
	}

	public void Initialize(SplatSaveData saveData)
	{
		dirtAmount = saveData.dirtAmount;
		ApplyMpb();
		if (dirtAmount <= 0f)
		{
			MarkClean();
		}
	}

	public SplatSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Splat " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		SplatSaveData result = default(SplatSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.dirtAmount = dirtAmount;
		return result;
	}

	public bool RemoveDirtAmountAbsolute(float value)
	{
		if (isCleaned)
		{
			return false;
		}
		dirtAmount = Mathf.Clamp01(dirtAmount - value);
		ApplyMpb();
		if (dirtAmount <= 0f)
		{
			MarkClean();
		}
		return true;
	}

	public void CleanByWaterKitchen()
	{
		MarkClean(cleanedByKitchenWater: true);
	}
}
