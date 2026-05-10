using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;
using _Scripts.Singletons;

namespace _Scripts.Office;

public class Hologram : MonoBehaviour, IInitializable<HologramSaveData>, IHasSaveData<HologramSaveData>
{
	[Header("Debug")]
	[SerializeField]
	private bool debug;

	[SerializeField]
	private bool debugStartBuildingOnStart;

	[Header("References")]
	[SerializeField]
	private GameObject[] hologramLayers;

	[SerializeField]
	private UnityEvent onBuildingFinishedEvent;

	private bool buildingInProgress;

	private int currentLayer;

	private int[] tilesNeededPerLayer;

	private int[] amountOfTilesPerLayer;

	public bool BuildingInProgress => buildingInProgress;

	public int CurrentLayer => currentLayer;

	public int[] AmountOfTilesPerLayer => amountOfTilesPerLayer;

	private void Awake()
	{
		buildingInProgress = false;
		tilesNeededPerLayer = new int[hologramLayers.Length];
		amountOfTilesPerLayer = new int[hologramLayers.Length];
		for (int i = 0; i < hologramLayers.Length; i++)
		{
			tilesNeededPerLayer[i] = hologramLayers[i].transform.childCount;
			amountOfTilesPerLayer[i] = 0;
			DisableLayer(i);
		}
	}

	private void Start()
	{
		if (debug && debugStartBuildingOnStart)
		{
			StartBuilding();
		}
	}

	private IEnumerator InitializedCoroutine(bool newBuildInProgress, int newCurrentLayer, int[] newAmountOfTilesPerLayer)
	{
		buildingInProgress = newBuildInProgress;
		if (buildingInProgress)
		{
			for (int i = 0; i < hologramLayers.Length; i++)
			{
				EnableLayer(i);
			}
			yield return new WaitForSeconds(0.1f);
			currentLayer = newCurrentLayer;
			amountOfTilesPerLayer = newAmountOfTilesPerLayer;
			for (int j = currentLayer + 1; j < hologramLayers.Length; j++)
			{
				DisableLayer(j);
			}
			EnableLayer(currentLayer);
		}
	}

	private void EnableLayer(int layerIndex)
	{
		if (layerIndex >= 0 && layerIndex < hologramLayers.Length)
		{
			hologramLayers[layerIndex].SetActive(value: true);
		}
	}

	private void DisableLayer(int layerIndex)
	{
		if (layerIndex >= 0 && layerIndex < hologramLayers.Length)
		{
			hologramLayers[layerIndex].SetActive(value: false);
		}
	}

	public void Initialize(HologramSaveData saveData)
	{
		StartCoroutine(InitializedCoroutine(saveData.buildInProgress, saveData.currentLayer, saveData.amountOfTilesPerLayer));
	}

	public HologramSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Hologram " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		HologramSaveData result = default(HologramSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.amountOfTilesPerLayer = amountOfTilesPerLayer;
		result.buildInProgress = buildingInProgress;
		result.currentLayer = currentLayer;
		return result;
	}

	public void StartBuilding()
	{
		if (!buildingInProgress)
		{
			buildingInProgress = true;
			currentLayer = 0;
			EnableLayer(currentLayer);
		}
	}

	public void PlaceTile()
	{
		if (currentLayer >= amountOfTilesPerLayer.Length)
		{
			return;
		}
		amountOfTilesPerLayer[currentLayer]++;
		if (amountOfTilesPerLayer[currentLayer] == tilesNeededPerLayer[currentLayer])
		{
			currentLayer++;
			if (currentLayer < hologramLayers.Length)
			{
				EnableLayer(currentLayer);
			}
			else if (Singleton<SceneController>.Instance.IsStoryLevel)
			{
				onBuildingFinishedEvent?.Invoke();
			}
		}
	}
}
