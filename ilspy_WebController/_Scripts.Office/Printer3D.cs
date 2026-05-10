using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using _Scripts.Objects;

namespace _Scripts.Office;

public class Printer3D : MonoBehaviour
{
	private enum PrinterState
	{
		Unplugged,
		Idle,
		Printing
	}

	[Header("References")]
	[SerializeField]
	private List<Blueprint3DModelSo> defaultModelList;

	[SerializeField]
	private Transform objectPrintTransform;

	[SerializeField]
	private MagneticLock magneticLockFilament;

	[Space(10f)]
	[SerializeField]
	private GameObject statusInformationPanel;

	[SerializeField]
	private Image powerStatusImage;

	[SerializeField]
	private Image filamentStatusImage;

	[SerializeField]
	private Sprite crossSprite;

	[SerializeField]
	private Sprite checkmarkSprite;

	[Space(10f)]
	[SerializeField]
	private GameObject idlePanel;

	[SerializeField]
	private Image modelImage;

	[SerializeField]
	private TextMeshProUGUI modelNameText;

	[Space(10f)]
	[SerializeField]
	private GameObject printingPanel;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onPlugInEvent;

	[SerializeField]
	private UnityEvent onUnplugEvent;

	[SerializeField]
	private UnityEvent onStartPrintingEvent;

	[SerializeField]
	private UnityEvent onFinishPrintingEvent;

	private PrinterState state;

	private List<Blueprint3DModelSo> modelList;

	private Blueprint3DModelSo selectedModel;

	private int selectedModelIndex;

	private FilamentSpool loadedFilamentSpool;

	private float printTimer;

	private bool isPluggedIn;

	private GameObject hologram;

	private float fixedJointBreakForce;

	private Color filamentColor;

	private void Awake()
	{
		modelList = new List<Blueprint3DModelSo>(defaultModelList);
		state = PrinterState.Idle;
		loadedFilamentSpool = null;
		statusInformationPanel.SetActive(value: true);
		idlePanel.SetActive(value: false);
		printingPanel.SetActive(value: false);
		selectedModelIndex = 0;
		selectedModel = modelList[selectedModelIndex];
		DisplayCurrentModel();
		filamentStatusImage.DOFade(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
	}

	private void Start()
	{
	}

	private void Update()
	{
		switch (state)
		{
		case PrinterState.Printing:
			printTimer -= Time.deltaTime;
			if (printTimer <= 0f)
			{
				FinishPrinting();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case PrinterState.Unplugged:
		case PrinterState.Idle:
			break;
		}
	}

	private void LockFilament()
	{
		magneticLockFilament.SetFixedJointBreakForce(float.PositiveInfinity);
	}

	private void UnlockFilament()
	{
		magneticLockFilament.SetFixedJointBreakForce(fixedJointBreakForce);
	}

	private void FinishPrinting()
	{
		if (hologram != null)
		{
			UnityEngine.Object.Destroy(hologram.gameObject);
			hologram = null;
		}
		PrintableObject printableObject = UnityEngine.Object.Instantiate(selectedModel.model, objectPrintTransform.position, objectPrintTransform.rotation, null);
		printableObject.SetColor(loadedFilamentSpool.FilamentColor);
		printableObject.GetComponent<SpawnableObject>().Setup();
		UnlockFilament();
		state = PrinterState.Idle;
		idlePanel.SetActive(value: true);
		printingPanel.SetActive(value: false);
		DisplayCurrentModel();
		onFinishPrintingEvent?.Invoke();
	}

	private void DisplayCurrentModel()
	{
		if (!(loadedFilamentSpool == null))
		{
			modelImage.sprite = selectedModel.modelSprite;
			modelImage.color = filamentColor;
			modelNameText.text = selectedModel.modelName;
			if (hologram != null)
			{
				UnityEngine.Object.Destroy(hologram);
			}
			hologram = UnityEngine.Object.Instantiate(selectedModel.hologram, objectPrintTransform.position, objectPrintTransform.rotation, null);
			hologram.transform.localScale = 0.98f * Vector3.one;
		}
	}

	public void LoadFilament()
	{
		loadedFilamentSpool = magneticLockFilament.ConnectedObject.GetComponent<FilamentSpool>();
		filamentColor = loadedFilamentSpool.FilamentColor;
		Debug.Log("Loaded Filament Spool " + loadedFilamentSpool.name);
		statusInformationPanel.SetActive(value: false);
		idlePanel.SetActive(value: true);
		fixedJointBreakForce = magneticLockFilament.GetFixedJointBreakForce();
		DisplayCurrentModel();
	}

	public void UnloadFilament()
	{
		loadedFilamentSpool = null;
		Debug.Log("Unloaded Filament Spool");
		statusInformationPanel.SetActive(value: true);
		idlePanel.SetActive(value: false);
		if (hologram != null)
		{
			UnityEngine.Object.Destroy(hologram);
			hologram = null;
		}
	}

	public void StartPrinting()
	{
		if (state == PrinterState.Idle)
		{
			if (!loadedFilamentSpool)
			{
				Debug.Log("No Filament loaded! Can't start the 3D print!");
				return;
			}
			LockFilament();
			printTimer = selectedModel.printTime;
			state = PrinterState.Printing;
			idlePanel.SetActive(value: false);
			printingPanel.SetActive(value: true);
			onStartPrintingEvent?.Invoke();
		}
	}

	public void SelectPreviousModel()
	{
		if (state == PrinterState.Idle)
		{
			int count = modelList.Count;
			selectedModelIndex = (selectedModelIndex + count - 1) % count;
			selectedModel = modelList[selectedModelIndex];
			DisplayCurrentModel();
		}
	}

	public void SelectNextModel()
	{
		if (state == PrinterState.Idle)
		{
			int count = modelList.Count;
			selectedModelIndex = (selectedModelIndex + 1) % count;
			selectedModel = modelList[selectedModelIndex];
			DisplayCurrentModel();
		}
	}

	public void InsertUsbStick()
	{
	}

	public void RemoveUsbStick()
	{
		selectedModelIndex = 0;
		selectedModel = modelList[selectedModelIndex];
	}

	public void Unplug()
	{
		onUnplugEvent?.Invoke();
	}

	public void Plugin()
	{
		onPlugInEvent?.Invoke();
	}
}
