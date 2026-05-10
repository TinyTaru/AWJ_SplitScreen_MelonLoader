using System;
using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;
using _Scripts.Objects;

namespace _Scripts.Office;

public class OfficeComputer : MonoBehaviour, IInitializable<OfficeComputerSaveData>, IHasSaveData<OfficeComputerSaveData>
{
	public enum DebuggingStep
	{
		NotStarted,
		GPU,
		CPU,
		Panel,
		RamStick1,
		RamStick2,
		Finished
	}

	[Header("Debug")]
	[SerializeField]
	private bool debug;

	[SerializeField]
	private bool debugStartQuest;

	[SerializeField]
	private bool debugRemoveScrews;

	[Header("References")]
	[SerializeField]
	private Rigidbody glass;

	[SerializeField]
	private Rigidbody[] screws;

	[SerializeField]
	private PCPart gpuPCPart;

	[SerializeField]
	private PCPart cpuPCPart;

	[SerializeField]
	private PCPart panelPCPart;

	[SerializeField]
	private PCPart ramStick1PCPart;

	[SerializeField]
	private PCPart ramStick2PCPart;

	[SerializeField]
	private Rigidbody gpuRigidbody;

	[SerializeField]
	private Rigidbody cpuRigidbody;

	[SerializeField]
	private Rigidbody panelRigidbody;

	[SerializeField]
	private Rigidbody ramStick1Rigidbody;

	[SerializeField]
	private Rigidbody ramStick2Rigidbody;

	[SerializeField]
	private GameObject shmoopGPU;

	[SerializeField]
	private GameObject shmoopCPU;

	[SerializeField]
	private GameObject shmoopPanel;

	[SerializeField]
	private GameObject shmoopRamStick1;

	[SerializeField]
	private GameObject shmoopRamStick2;

	[Header("Parameter")]
	[SerializeField]
	private int numberOfScrews;

	[SerializeField]
	private float glassPopForceMagnitude;

	[SerializeField]
	private float glassPopForceHeight;

	[SerializeField]
	private Vector3 glassPopTorque;

	[SerializeField]
	[VariablePopup(false)]
	private string shmoopEscapedCounter;

	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onReleaseGlassEvent;

	[SerializeField]
	private UnityEvent onNextDebuggingStepEvent;

	[SerializeField]
	private UnityEvent onDebuggingFinishedEvent;

	[SerializeField]
	private UnityEvent onBuildingFinishedEvent;

	private int screwCounter;

	private int shmoopCounter;

	private DebuggingStep debuggingStep;

	private int insertedParts;

	private const int totalParts = 4;

	private bool glasRemoved;

	private bool gpuRemoved;

	private bool cpuRemoved;

	private bool panelOpened;

	private bool ramStick1Removed;

	private bool ramStick2Removed;

	private void Awake()
	{
		screwCounter = numberOfScrews;
		debuggingStep = DebuggingStep.NotStarted;
		gpuRigidbody.isKinematic = true;
		cpuRigidbody.isKinematic = true;
		panelRigidbody.isKinematic = true;
		ramStick1Rigidbody.isKinematic = true;
		ramStick2Rigidbody.isKinematic = true;
		DialogueLua.SetVariable(shmoopEscapedCounter, 0);
		if (QuestLog.GetQuestState(questName) == QuestState.Active)
		{
			QuestLog.SetQuestState(questName, QuestState.Unassigned);
		}
	}

	private void Start()
	{
		if (!debug)
		{
			return;
		}
		if (debugStartQuest)
		{
			StartQuest();
		}
		if (debugRemoveScrews)
		{
			Rigidbody[] array = screws;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(value: false);
				RemoveScrew();
			}
		}
	}

	private IEnumerator SetGlassBreakableCoroutine()
	{
		yield return new WaitForSeconds(0.3f);
		if (glass != null)
		{
			glass.GetComponent<BreakableObject>().SetBreakable(value: true);
		}
	}

	private void ReleaseGlass()
	{
		glass.isKinematic = false;
		Vector3 force = base.transform.right * glassPopForceMagnitude;
		Vector3 position = glass.transform.position;
		position.y = glassPopForceHeight;
		glass.AddForceAtPosition(force, position, ForceMode.Impulse);
		glass.AddTorque(glassPopTorque, ForceMode.Impulse);
		StartCoroutine(SetGlassBreakableCoroutine());
		glasRemoved = true;
		onReleaseGlassEvent?.Invoke();
	}

	private void EnableDebuggingComponent()
	{
		Debug.Log($"EnableDebuggingComponent {debuggingStep}");
		switch (debuggingStep)
		{
		case DebuggingStep.GPU:
			gpuPCPart.StartWiggle();
			gpuRigidbody.isKinematic = false;
			break;
		case DebuggingStep.CPU:
			cpuPCPart.StartWiggle();
			cpuRigidbody.isKinematic = false;
			break;
		case DebuggingStep.Panel:
			panelPCPart.StartWiggle();
			panelRigidbody.isKinematic = false;
			break;
		case DebuggingStep.RamStick1:
			ramStick1PCPart.StartWiggle();
			ramStick1Rigidbody.isKinematic = false;
			break;
		case DebuggingStep.RamStick2:
			ramStick2PCPart.StartWiggle();
			ramStick2Rigidbody.isKinematic = false;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case DebuggingStep.NotStarted:
		case DebuggingStep.Finished:
			break;
		}
	}

	public void Initialize(OfficeComputerSaveData officeComputerSaveData)
	{
		screwCounter = officeComputerSaveData.screwCounter;
		shmoopCounter = officeComputerSaveData.shmoopCounter;
		debuggingStep = officeComputerSaveData.debuggingStep;
		insertedParts = officeComputerSaveData.insertedParts;
		glasRemoved = officeComputerSaveData.glassRemoved;
		gpuRemoved = officeComputerSaveData.gpuRemoved;
		cpuRemoved = officeComputerSaveData.cpuRemoved;
		panelOpened = officeComputerSaveData.panelOpened;
		ramStick1Removed = officeComputerSaveData.ramStick1Removed;
		ramStick2Removed = officeComputerSaveData.ramStick2Removed;
		if (glasRemoved)
		{
			glass.isKinematic = false;
		}
		if (gpuRemoved)
		{
			UnityEngine.Object.Destroy(shmoopGPU);
			gpuRigidbody.isKinematic = false;
			gpuPCPart.StopWiggle();
			cpuPCPart.StartWiggle();
			debuggingStep = DebuggingStep.CPU;
		}
		if (cpuRemoved)
		{
			UnityEngine.Object.Destroy(shmoopCPU);
			cpuRigidbody.isKinematic = false;
			cpuPCPart.StopWiggle();
			panelPCPart.StartWiggle();
			debuggingStep = DebuggingStep.Panel;
		}
		if (panelOpened)
		{
			UnityEngine.Object.Destroy(shmoopPanel);
			panelRigidbody.isKinematic = false;
			panelPCPart.StopWiggle();
			ramStick1PCPart.StartWiggle();
			debuggingStep = DebuggingStep.RamStick1;
		}
		if (ramStick1Removed)
		{
			UnityEngine.Object.Destroy(shmoopRamStick1);
			ramStick1Rigidbody.isKinematic = false;
			ramStick1PCPart.StopWiggle();
			ramStick2PCPart.StartWiggle();
			debuggingStep = DebuggingStep.RamStick2;
		}
		if (ramStick2Removed)
		{
			UnityEngine.Object.Destroy(shmoopRamStick2);
			ramStick2Rigidbody.isKinematic = false;
			ramStick2PCPart.StopWiggle();
			debuggingStep = DebuggingStep.Finished;
		}
		EnableDebuggingComponent();
		if (debuggingStep == DebuggingStep.Finished)
		{
			Debug.Log("onDebuggingFinishedEvent");
			onDebuggingFinishedEvent?.Invoke();
		}
	}

	public OfficeComputerSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Office Computer " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		OfficeComputerSaveData result = default(OfficeComputerSaveData);
		result.id = id;
		result.screwCounter = screwCounter;
		result.shmoopCounter = shmoopCounter;
		result.debuggingStep = debuggingStep;
		result.insertedParts = insertedParts;
		result.glassRemoved = glasRemoved;
		result.gpuRemoved = gpuRemoved;
		result.cpuRemoved = cpuRemoved;
		result.panelOpened = panelOpened;
		result.ramStick1Removed = ramStick1Removed;
		result.ramStick2Removed = ramStick2Removed;
		return result;
	}

	public void StartQuest()
	{
		debuggingStep = DebuggingStep.GPU;
		EnableDebuggingComponent();
	}

	public void RemoveScrew()
	{
		screwCounter--;
		if (screwCounter == 0)
		{
			ReleaseGlass();
		}
	}

	public void RemoveGPU()
	{
		gpuRemoved = true;
	}

	public void RemoveCPU()
	{
		cpuRemoved = true;
	}

	public void OpenPanel()
	{
		panelOpened = true;
	}

	public void RemoveRamStick1()
	{
		ramStick1Removed = true;
	}

	public void RemoveRamStick2()
	{
		ramStick2Removed = true;
	}

	public void StartNextDebuggingStep()
	{
		Debug.Log($"StartNextDebuggingStep {debuggingStep}");
		gpuPCPart.StopWiggle();
		cpuPCPart.StopWiggle();
		panelPCPart.StopWiggle();
		ramStick1PCPart.StopWiggle();
		ramStick2PCPart.StopWiggle();
		debuggingStep++;
		if (debuggingStep == DebuggingStep.Finished)
		{
			Debug.Log("onDebuggingFinishedEvent");
			onDebuggingFinishedEvent?.Invoke();
		}
		else
		{
			EnableDebuggingComponent();
			onNextDebuggingStepEvent?.Invoke();
		}
	}

	public void InsertPart()
	{
		insertedParts++;
		if (insertedParts == 4)
		{
			onBuildingFinishedEvent?.Invoke();
		}
	}

	public void PressPowerButton()
	{
		Debug.Log("Power button pressed!");
	}
}
