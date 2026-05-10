using UnityEngine;

namespace _Scripts.LivingRoom;

public class WaterLivingRoom : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform waterLower;

	[SerializeField]
	private Transform waterUpper;

	[SerializeField]
	private Transform waterStair1;

	[SerializeField]
	private Transform waterStair2;

	[SerializeField]
	private BoxCollider waterTriggerLower;

	[SerializeField]
	private BoxCollider waterTriggerUpper;

	[SerializeField]
	private GameObject waterfallStair1;

	[SerializeField]
	private GameObject waterfallStair2;

	[SerializeField]
	private GameObject waterfallUpper;

	[Header("Parameters")]
	[SerializeField]
	private float minWaterLevelLower;

	[SerializeField]
	private float minWaterLevelUpper;

	[SerializeField]
	private float maxWaterLevel;

	[SerializeField]
	private float waterLevelStair1;

	[SerializeField]
	private float waterLevelStair2;

	[SerializeField]
	private float waterFillFlow;

	[SerializeField]
	private float waterDrainFlow;

	[SerializeField]
	private float waterVisualsOffset;

	[SerializeField]
	private float aquariumShelfCoveredWaterLevel;

	[SerializeField]
	private float upperFloorCoveredWaterLevel;

	[SerializeField]
	private float lowerFloorCoveredWaterLevel;

	private bool isDrainOpenLower;

	private bool isDrainOpenUpper;

	private bool isFull;

	private bool isFullLower;

	private bool isFullUpper;

	private bool aquariumShelfCovered;

	private bool upperFloorCovered;

	private bool lowerFloorCovered;

	private bool isEmpty;

	private bool isFillingUp;

	private float waterLevel;

	private float waterLevelUpper;

	private Vector3 positionWaterLower;

	private Vector3 positionWaterUpper;

	private Vector3 waterTriggerLowerSize;

	private Vector3 waterTriggerLowerCenter;

	private Vector3 waterTriggerUpperSize;

	private Vector3 waterTriggerUpperCenter;

	private void Awake()
	{
		waterLower.gameObject.SetActive(value: false);
		waterUpper.gameObject.SetActive(value: false);
		waterStair1.gameObject.SetActive(value: false);
		waterStair2.gameObject.SetActive(value: false);
		waterfallStair1.SetActive(value: false);
		waterfallStair2.SetActive(value: false);
		waterfallUpper.SetActive(value: false);
		positionWaterLower = waterLower.position;
		positionWaterUpper = waterUpper.position;
		waterLevelStair1 = waterStair1.position.y;
		waterLevelStair2 = waterStair2.position.y;
		isEmpty = true;
		isDrainOpenLower = true;
		isDrainOpenUpper = true;
		waterLevel = minWaterLevelLower;
		waterLevelUpper = minWaterLevelUpper;
		waterTriggerLowerSize = waterTriggerLower.size;
		waterTriggerLowerCenter = waterTriggerLower.center;
		waterTriggerUpperSize = waterTriggerUpper.size;
		waterTriggerUpperCenter = waterTriggerUpper.center;
	}

	private void FixedUpdate()
	{
		float num = (isFillingUp ? waterFillFlow : (isDrainOpenUpper ? (0f - waterDrainFlow) : 0f));
		float num2 = ((!isDrainOpenUpper && isFillingUp) ? waterFillFlow : ((isDrainOpenUpper || (waterLevel < upperFloorCoveredWaterLevel && isDrainOpenLower)) ? (0f - waterDrainFlow) : 0f));
		waterLevelUpper += (aquariumShelfCovered ? num2 : num) * Time.fixedDeltaTime;
		float num3 = (isDrainOpenUpper ? aquariumShelfCoveredWaterLevel : Mathf.Max(waterLevel, upperFloorCoveredWaterLevel));
		waterLevelUpper = Mathf.Clamp(waterLevelUpper, minWaterLevelUpper, num3);
		float num4 = (upperFloorCovered ? waterFillFlow : (isDrainOpenLower ? (0f - waterDrainFlow) : 0f));
		float num5 = ((upperFloorCovered && isFillingUp) ? waterFillFlow : 0f) - ((isDrainOpenLower || (waterLevel >= upperFloorCoveredWaterLevel && isDrainOpenUpper)) ? waterDrainFlow : 0f);
		waterLevel += (lowerFloorCovered ? num5 : num4) * Time.fixedDeltaTime;
		float max = (isDrainOpenLower ? Mathf.Max(waterLevel, lowerFloorCoveredWaterLevel) : (isDrainOpenUpper ? Mathf.Max(waterLevel, num3) : maxWaterLevel));
		waterLevel = Mathf.Clamp(waterLevel, minWaterLevelLower, max);
		aquariumShelfCovered = waterLevelUpper >= aquariumShelfCoveredWaterLevel;
		upperFloorCovered = waterLevelUpper >= upperFloorCoveredWaterLevel;
		lowerFloorCovered = waterLevel >= lowerFloorCoveredWaterLevel;
		UpdateWaterVisualsAndTriggerAreas();
	}

	public void StartFillingUp()
	{
		isFillingUp = true;
	}

	public void StopFillingUp()
	{
		isFillingUp = false;
	}

	public void CloseDrainLower()
	{
		isDrainOpenLower = false;
	}

	public void OpenDrainLower()
	{
		isDrainOpenLower = true;
	}

	public void CloseDrainUpper()
	{
		isDrainOpenUpper = false;
	}

	public void OpenDrainUpper()
	{
		isDrainOpenUpper = true;
	}

	private void UpdateWaterVisualsAndTriggerAreas()
	{
		if (waterLevel >= maxWaterLevel)
		{
			isFull = true;
		}
		else if (waterLevel <= minWaterLevelLower)
		{
			isEmpty = true;
		}
		else if (waterLevel > minWaterLevelLower)
		{
			isEmpty = false;
		}
		positionWaterLower.y = waterLevel + waterVisualsOffset;
		waterLower.position = positionWaterLower;
		positionWaterUpper.y = Mathf.Max(waterLevelUpper, waterLevel) + waterVisualsOffset;
		waterUpper.position = positionWaterUpper;
		waterLower.gameObject.SetActive(upperFloorCovered || waterLevel > minWaterLevelLower);
		waterUpper.gameObject.SetActive(waterLevelUpper > minWaterLevelUpper);
		waterfallUpper.SetActive(upperFloorCovered && waterLevel < upperFloorCoveredWaterLevel);
		waterStair1.gameObject.SetActive(upperFloorCovered && waterLevel <= waterLevelStair1);
		waterfallStair1.SetActive(waterStair1.gameObject.activeSelf);
		waterStair2.gameObject.SetActive(upperFloorCovered && waterLevel <= waterLevelStair2);
		waterfallStair2.gameObject.SetActive(waterStair2.gameObject.activeSelf);
		bool active = lowerFloorCovered;
		bool active2 = upperFloorCovered;
		waterTriggerLower.gameObject.SetActive(active);
		waterTriggerUpper.gameObject.SetActive(active2);
		waterTriggerLowerSize.y = waterLevel + waterVisualsOffset;
		waterTriggerLower.size = waterTriggerLowerSize;
		waterTriggerLowerCenter.y = (waterLevel + waterVisualsOffset) / 2f;
		waterTriggerLower.center = waterTriggerLowerCenter;
		waterTriggerUpperSize.y = waterLevelUpper - upperFloorCoveredWaterLevel + waterVisualsOffset;
		waterTriggerUpper.size = waterTriggerUpperSize;
		waterTriggerUpperCenter.y = (waterLevelUpper - upperFloorCoveredWaterLevel + waterVisualsOffset) / 2f;
		waterTriggerUpper.center = waterTriggerUpperCenter;
	}
}
