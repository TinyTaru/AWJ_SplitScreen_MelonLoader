using System;
using UnityEngine;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

public class KitchenPlant : MonoBehaviour, IInitializable<KitchenPlantSaveData>, IHasSaveData<KitchenPlantSaveData>
{
	public class OnPlantWateredEventEventArgs : EventArgs
	{
		public bool wateredByKitchenWater;
	}

	[Header("References")]
	[SerializeField]
	private ParticleSystem plantWateredParticleSystem;

	[SerializeField]
	private ParticleSystem wateredContinuousParticleSystem;

	[Header("Parameters")]
	[SerializeField]
	private bool standingOnFloor;

	private bool isWatered;

	public bool StandingOnFloor => standingOnFloor;

	public bool IsWatered => isWatered;

	public event EventHandler<OnPlantWateredEventEventArgs> OnPlantWateredEvent;

	public void Initialize(KitchenPlantSaveData saveData)
	{
		isWatered = saveData.isWatered;
		if (isWatered)
		{
			wateredContinuousParticleSystem.Play();
		}
	}

	public KitchenPlantSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Kitchen Plant " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		KitchenPlantSaveData result = default(KitchenPlantSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.isWatered = isWatered;
		return result;
	}

	public bool WaterPlant(bool wateredByKitchenWater = false)
	{
		if (isWatered)
		{
			return false;
		}
		isWatered = true;
		plantWateredParticleSystem.Play();
		wateredContinuousParticleSystem.Play();
		this.OnPlantWateredEvent?.Invoke(this, new OnPlantWateredEventEventArgs
		{
			wateredByKitchenWater = wateredByKitchenWater
		});
		return true;
	}
}
