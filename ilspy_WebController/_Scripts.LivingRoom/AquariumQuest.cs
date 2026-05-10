using PixelCrushers.DialogueSystem;
using Sirenix.Utilities;
using UnityEngine;

namespace _Scripts.LivingRoom;

public class AquariumQuest : MonoBehaviour
{
	[SerializeField]
	private GameObject waterfall;

	[SerializeField]
	private Transform waterfallAngleInside;

	[SerializeField]
	private Transform waterfallAngleOutside;

	[SerializeField]
	private Transform faucetCap;

	[SerializeField]
	private Transform faucetCapClosePosition;

	[SerializeField]
	private Transform faucetCapOpenPosition;

	[SerializeField]
	private int websToRemove = 3;

	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[SerializeField]
	[VariablePopup(false)]
	private string teddyBearRemovedVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string allFiltersInstalledVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string allWebsRemovedVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string faucetIsOnVariable;

	[SerializeField]
	[VariablePopup(false)]
	private string waterfallIsInsideVariable;

	[SerializeField]
	private GameObject windAreas;

	[SerializeField]
	private GameObject[] reservoirArmWaterTriggers;

	[SerializeField]
	private GameObject output1WaterTriggers;

	[SerializeField]
	private GameObject output2WaterTriggers;

	[SerializeField]
	private Transform waterVisuals;

	[SerializeField]
	private float maxWaterLevel;

	[SerializeField]
	private float waterFillSpeed = 20f;

	private int websRemoved;

	private bool allWebsRemoved;

	private bool allFiltersInstalled;

	private bool teddyBearRemoved;

	private bool faucetIsOn;

	private bool waterfallIsInside;

	private float waterFallAngleOutsideDegree;

	private float waterFallAngleInsideDegree;

	private WaterLivingRoom waterLivingRoom;

	private float waterLevel;

	private void Start()
	{
		waterfall.SetActive(value: false);
		reservoirArmWaterTriggers.ForEach(delegate(GameObject x)
		{
			x.SetActive(value: false);
		});
		output1WaterTriggers.SetActive(value: false);
		output2WaterTriggers.SetActive(value: false);
		waterLevel = 0f;
		waterVisuals.localPosition = new Vector3(0f, waterLevel, 0f);
		allWebsRemoved = false;
		allFiltersInstalled = false;
		teddyBearRemoved = false;
		faucetIsOn = false;
		waterfallIsInside = true;
		UpdateDialogueSystemVariables();
		waterFallAngleOutsideDegree = waterfallAngleOutside.transform.eulerAngles.y;
		if (waterFallAngleOutsideDegree > 180f)
		{
			waterFallAngleOutsideDegree -= 360f;
		}
		Debug.Log($"waterfall angle outside: {waterFallAngleOutsideDegree}");
		waterFallAngleInsideDegree = waterfallAngleInside.transform.eulerAngles.y;
		if (waterFallAngleInsideDegree > 180f)
		{
			waterFallAngleInsideDegree -= 360f;
		}
		Debug.Log($"waterfall angle inside: {waterFallAngleInsideDegree}");
		waterLivingRoom = Object.FindFirstObjectByType<WaterLivingRoom>();
		Invoke("StartQuest", 1f);
	}

	private void Update()
	{
		waterLevel += ((faucetIsOn && waterfallIsInside) ? waterFillSpeed : 0f) * Time.deltaTime;
		waterLevel = Mathf.Clamp(waterLevel, 0f, maxWaterLevel);
		waterVisuals.localPosition = new Vector3(0f, waterLevel, 0f);
		float y = faucetCap.position.y;
		if (faucetIsOn)
		{
			if (waterfallIsInside)
			{
				output1WaterTriggers.SetActive(value: true);
				if (teddyBearRemoved)
				{
					output2WaterTriggers.SetActive(value: true);
				}
			}
			if (y < faucetCapClosePosition.position.y)
			{
				faucetIsOn = false;
				reservoirArmWaterTriggers.ForEach(delegate(GameObject x)
				{
					x.SetActive(value: false);
				});
				waterfall.SetActive(value: false);
				UpdateDialogueSystemVariables();
				if (waterLivingRoom != null)
				{
					waterLivingRoom.StopFillingUp();
				}
			}
		}
		else if (y > faucetCapClosePosition.position.y)
		{
			faucetIsOn = true;
			reservoirArmWaterTriggers.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: true);
			});
			waterfall.SetActive(value: true);
			UpdateDialogueSystemVariables();
			CheckIfQuestIsDone();
			if (!waterfallIsInside && waterLivingRoom != null)
			{
				waterLivingRoom.StartFillingUp();
			}
		}
		float num = waterfall.transform.eulerAngles.y;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (waterfallIsInside)
		{
			if (num < waterFallAngleOutsideDegree)
			{
				Debug.Log("Waterfall is outside");
				waterfallIsInside = false;
				UpdateDialogueSystemVariables();
				if (faucetIsOn && waterLivingRoom != null)
				{
					waterLivingRoom.StartFillingUp();
				}
			}
		}
		else if (num > waterFallAngleInsideDegree)
		{
			Debug.Log("Waterfall is inside");
			waterfallIsInside = true;
			UpdateDialogueSystemVariables();
			CheckIfQuestIsDone();
			if (waterLivingRoom != null)
			{
				waterLivingRoom.StopFillingUp();
			}
		}
	}

	private void UpdateDialogueSystemVariables()
	{
		DialogueLua.SetVariable(faucetIsOnVariable, faucetIsOn);
		DialogueLua.SetVariable(waterfallIsInsideVariable, waterfallIsInside);
		DialogueLua.SetVariable(allFiltersInstalledVariable, allFiltersInstalled);
		DialogueLua.SetVariable(allWebsRemovedVariable, allWebsRemoved);
		DialogueLua.SetVariable(teddyBearRemovedVariable, teddyBearRemoved);
	}

	private void CheckIfQuestIsDone()
	{
		if (faucetIsOn && waterfallIsInside && allFiltersInstalled && allWebsRemoved && teddyBearRemoved)
		{
			windAreas.SetActive(value: true);
			QuestLog.SetQuestState(questName, QuestState.Success);
		}
	}

	public void StartQuest()
	{
	}

	public void OnAllFiltersInstalled()
	{
		allFiltersInstalled = true;
		UpdateDialogueSystemVariables();
		CheckIfQuestIsDone();
	}

	public void OnWebRemoved()
	{
		websRemoved++;
		if (websRemoved >= websToRemove)
		{
			allWebsRemoved = true;
			UpdateDialogueSystemVariables();
			CheckIfQuestIsDone();
		}
	}

	public void OnTeddyBearRemoved()
	{
		teddyBearRemoved = true;
		UpdateDialogueSystemVariables();
		CheckIfQuestIsDone();
		if (output1WaterTriggers.activeSelf)
		{
			output2WaterTriggers.SetActive(value: true);
		}
	}
}
