using DG.Tweening;
using Dreamteck.Splines;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using _Scripts.LevelSaving;
using _Scripts.Objects;
using _Scripts.Singletons;
using _Scripts.Utils;

namespace _Scripts.Office;

public class OfficePrinter : MonoBehaviour, IInitializable<OfficePrinterSaveData>, IHasSaveData<OfficePrinterSaveData>
{
	[Header("Debug")]
	[SerializeField]
	private bool debug;

	[SerializeField]
	private bool debugGearboxRepaired;

	[SerializeField]
	private bool debugPaperFilledUp;

	[Header("References")]
	[SerializeField]
	private ConstantRotation[] gears;

	[SerializeField]
	private Image gearboxStatusImage;

	[SerializeField]
	private Image paperStatusImage;

	[SerializeField]
	private Sprite crossSprite;

	[SerializeField]
	private Sprite checkmarkSprite;

	[SerializeField]
	private GameObject statusInformationPanel;

	[SerializeField]
	private GameObject printingPanel;

	[SerializeField]
	private TextMeshProUGUI printingText;

	[SerializeField]
	private PrintInstruction[] printInstructions;

	[SerializeField]
	private SplineComputer printSpline;

	[Header("Parameters")]
	[SerializeField]
	private int numberOfGears;

	[SerializeField]
	private Color crossColor;

	[SerializeField]
	private Color checkmarkColor;

	[SerializeField]
	private float printDurationPerPage;

	[SerializeField]
	private float printingTextUpdateInterval;

	[SerializeField]
	private int amountOfPagesToPrint = 1;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onActivateGearsEvent;

	[SerializeField]
	private UnityEvent onPaperTrayFilledEvent;

	[SerializeField]
	private UnityEvent onPrintingStartedEvent;

	[SerializeField]
	private UnityEvent onSpawnPageEvent;

	[SerializeField]
	private UnityEvent onPageFinishedPrintingEvent;

	[SerializeField]
	private UnityEvent onPrintingFinishedEvent;

	private bool gearboxRepaired;

	private bool paperFilledIn;

	private bool isPrinting;

	private int missingGears;

	private float printTimer;

	private float printingTextUpdateTimer;

	private int printingTextIndex;

	private int pagesRemaining;

	private float printProgress;

	private Paper currentPage;

	private int pageIndex;

	private int printInstructionIndex;

	private bool printButtonIsPressed;

	private readonly string[] printingTexts = new string[7] { "Printing\n ", "Printing\n.", "Printing\n..", "Printing\n...", "Printing\n....", "Printing\n.....", "Printing\n......" };

	private void Awake()
	{
		missingGears = numberOfGears;
		gearboxRepaired = false;
		paperFilledIn = false;
		isPrinting = false;
		gearboxStatusImage.sprite = crossSprite;
		gearboxStatusImage.color = crossColor;
		gearboxStatusImage.DOFade(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
		paperStatusImage.sprite = crossSprite;
		paperStatusImage.color = crossColor;
		statusInformationPanel.SetActive(value: true);
		printingPanel.SetActive(value: false);
		printInstructionIndex = 0;
		printButtonIsPressed = false;
	}

	private void Start()
	{
		if (debug)
		{
			if (debugGearboxRepaired)
			{
				ActivateGears();
			}
			if (debugPaperFilledUp)
			{
				FillInPaper();
			}
		}
		FillInPaper();
	}

	private void Update()
	{
		if (isPrinting)
		{
			UpdatePrintingText();
			UpdatePrintTimer();
			UpdateCurrentPage();
		}
	}

	private void UpdateCurrentPage()
	{
		if (currentPage != null)
		{
			printProgress = 1f - printTimer / printDurationPerPage;
			SplineSample splineSample = printSpline.Evaluate(printProgress);
			currentPage.transform.position = splineSample.position;
			currentPage.transform.rotation = splineSample.rotation;
		}
	}

	private void UpdatePrintTimer()
	{
		printTimer -= Time.deltaTime;
		if (printTimer <= 0f)
		{
			pagesRemaining--;
			if (pagesRemaining == 0)
			{
				currentPage.SetKinematic(value: false);
				FinishPrinting();
				return;
			}
			currentPage.SetKinematic(value: false);
			onPageFinishedPrintingEvent?.Invoke();
			SpawnPage();
			printTimer = printDurationPerPage;
		}
	}

	private void UpdatePrintingText()
	{
		printingTextUpdateTimer -= Time.deltaTime;
		if (printingTextUpdateTimer <= 0f)
		{
			printingTextUpdateTimer = printingTextUpdateInterval;
			printingTextIndex = (printingTextIndex + 1) % printingTexts.Length;
			printingText.text = printingTexts[printingTextIndex];
		}
	}

	private void FillInPaper()
	{
		paperStatusImage.sprite = checkmarkSprite;
		paperStatusImage.color = checkmarkColor;
		paperFilledIn = true;
		onPaperTrayFilledEvent?.Invoke();
	}

	private void ActivateGears()
	{
		ConstantRotation[] array = gears;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		gearboxStatusImage.DOKill();
		gearboxStatusImage.sprite = checkmarkSprite;
		gearboxStatusImage.color = checkmarkColor;
		gearboxRepaired = true;
		onActivateGearsEvent?.Invoke();
	}

	private void SpawnPage()
	{
		SplineSample splineSample = printSpline.Evaluate(0.0);
		Paper original = printInstructions[printInstructionIndex].pages[pageIndex];
		currentPage = Object.Instantiate(original, splineSample.position, splineSample.rotation, null);
		currentPage.SetKinematic(value: true);
		currentPage.GetComponent<SpawnableObject>().Setup();
		pageIndex++;
		onSpawnPageEvent?.Invoke();
	}

	private void StartPrinting()
	{
		pageIndex = 0;
		printingTextIndex = 0;
		printingText.text = printingTexts[printingTextIndex];
		printingTextUpdateTimer = printingTextUpdateInterval;
		printTimer = printDurationPerPage;
		statusInformationPanel.SetActive(value: false);
		printingPanel.SetActive(value: true);
		pagesRemaining = amountOfPagesToPrint;
		SpawnPage();
		isPrinting = true;
		onPrintingStartedEvent?.Invoke();
	}

	private void FinishPrinting()
	{
		currentPage = null;
		statusInformationPanel.SetActive(value: true);
		printingPanel.SetActive(value: false);
		isPrinting = false;
		printInstructionIndex = (printInstructionIndex + 1) % printInstructions.Length;
		onPrintingFinishedEvent?.Invoke();
		if (printButtonIsPressed)
		{
			StartPrinting();
		}
	}

	public void Initialize(OfficePrinterSaveData officePrinterSaveData)
	{
		isPrinting = officePrinterSaveData.isPrinting;
		printTimer = officePrinterSaveData.printTimer;
		pagesRemaining = officePrinterSaveData.pagesRemaining;
		printProgress = officePrinterSaveData.printProgress;
		pageIndex = officePrinterSaveData.pageIndex;
		printInstructionIndex = officePrinterSaveData.printInstructionIndex;
		Singleton<LevelSavingController>.Instance.UniqueGameObjectDict.TryGetValue(officePrinterSaveData.currentPageId, out var value);
		if (!(value != null))
		{
			return;
		}
		Paper component = value.GetComponent<Paper>();
		if (component != null)
		{
			currentPage = component;
			if (isPrinting)
			{
				currentPage.SetKinematic(value: true);
			}
		}
	}

	public OfficePrinterSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Office Printer " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		string currentPageId = string.Empty;
		if (currentPage != null)
		{
			UniqueID component2 = currentPage.GetComponent<UniqueID>();
			if (component2 == null)
			{
				Debug.LogError("Current page inside the printer with name " + currentPage.name + " has no uniqueID");
			}
			else
			{
				currentPageId = component2.ID;
			}
		}
		OfficePrinterSaveData result = default(OfficePrinterSaveData);
		result.id = id;
		result.isPrinting = isPrinting;
		result.printTimer = printTimer;
		result.pagesRemaining = pagesRemaining;
		result.printProgress = printProgress;
		result.currentPageId = currentPageId;
		result.pageIndex = pageIndex;
		result.printInstructionIndex = printInstructionIndex;
		return result;
	}

	public void InsertGear()
	{
		missingGears--;
		if (missingGears == 0)
		{
			ActivateGears();
		}
	}

	public void PressPrintButton()
	{
		printButtonIsPressed = true;
		if (gearboxRepaired && paperFilledIn && !isPrinting)
		{
			StartPrinting();
		}
	}

	public void ReleasePrintButton()
	{
		printButtonIsPressed = false;
	}
}
