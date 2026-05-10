using FMODUnity;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using _Scripts.CosmeticItems;
using _Scripts.Emotes;
using _Scripts.General;
using _Scripts.Spider;
using _Scripts.UI.MobileMonetization;
using _Scripts.Web;

namespace _Scripts.Singletons;

public class EmoteController : Singleton<EmoteController>
{
	[Header("References")]
	[SerializeField]
	private EmoteWheel emoteWheel1;

	[SerializeField]
	private EmoteWheel emoteWheel2;

	[SerializeField]
	private EmoteWheel emoteWheel3;

	[SerializeField]
	private EmoteWheel emoteWheel4;

	[SerializeField]
	private GameObject emoteCancelArea;

	[SerializeField]
	private GameObject switchEmoteWheelMobile;

	[SerializeField]
	private GameObject switchEmoteWheelMobileButton;

	[Header("Parameters")]
	[SerializeField]
	private CosmeticItemWebSo[] defaultWebColorsDemo;

	[Header("Input Actions")]
	[SerializeField]
	private InputActionReference emoteWheel1InputAction;

	[SerializeField]
	private InputActionReference emoteWheel2InputAction;

	[SerializeField]
	private InputActionReference emoteWheel3InputAction;

	[SerializeField]
	private InputActionReference emoteWheel4InputAction;

	[SerializeField]
	private InputActionReference mousePositionInputAction;

	[SerializeField]
	private InputActionReference selectEmoteInputAction;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter invalidEmoteSound;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onEmotePerformed;

	private EmoteWheel openEmoteWheel;

	private EmoteWheel webColorWheel;

	private bool emote1;

	private bool emote2;

	private bool emote3;

	private bool emote4;

	public EmoteWheel OpenEmoteWheel => openEmoteWheel;

	private void OnEnable()
	{
		emoteWheel1InputAction.action.performed += OnEmote1;
		emoteWheel2InputAction.action.performed += OnEmote2;
		emoteWheel3InputAction.action.performed += OnEmote3;
		emoteWheel4InputAction.action.performed += OnEmote4;
		mousePositionInputAction.action.performed += OnMousePosition;
		selectEmoteInputAction.action.performed += OnSelectEmote;
	}

	private void OnDisable()
	{
		emoteWheel1InputAction.action.performed -= OnEmote1;
		emoteWheel2InputAction.action.performed -= OnEmote2;
		emoteWheel3InputAction.action.performed -= OnEmote3;
		emoteWheel4InputAction.action.performed -= OnEmote4;
		mousePositionInputAction.action.performed -= OnMousePosition;
		selectEmoteInputAction.action.performed -= OnSelectEmote;
	}

	private void Start()
	{
		openEmoteWheel = null;
		ShowEmoteCancelArea(value: false);
		switchEmoteWheelMobile.SetActive(value: false);
		switchEmoteWheelMobileButton.SetActive(value: false);
		webColorWheel = emoteWheel4;
		InitializeWebColorWheel();
	}

	private void Update()
	{
		if ((Singleton<GameController>.Instance.State != 0 && Singleton<GameController>.Instance.State != GameController.GameState.SelectEmote) || Singleton<GameController>.Instance.Player.State != 0 || Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			return;
		}
		if (openEmoteWheel == null)
		{
			if (emote1)
			{
				emoteWheel1.OpenEmoteWheel();
				ShowEmoteCancelArea(value: true);
				openEmoteWheel = emoteWheel1;
				emoteWheel2.CloseEmoteWheel();
				emoteWheel3.CloseEmoteWheel();
				emoteWheel4.CloseEmoteWheel();
			}
			if (emote2)
			{
				emoteWheel2.OpenEmoteWheel();
				ShowEmoteCancelArea(value: true);
				openEmoteWheel = emoteWheel2;
				emoteWheel1.CloseEmoteWheel();
				emoteWheel3.CloseEmoteWheel();
				emoteWheel4.CloseEmoteWheel();
			}
			if (emote3)
			{
				emoteWheel3.OpenEmoteWheel();
				ShowEmoteCancelArea(value: true);
				openEmoteWheel = emoteWheel3;
				emoteWheel1.CloseEmoteWheel();
				emoteWheel2.CloseEmoteWheel();
				emoteWheel4.CloseEmoteWheel();
			}
			if (emote4)
			{
				emoteWheel4.OpenEmoteWheel();
				ShowEmoteCancelArea(value: true);
				openEmoteWheel = emoteWheel4;
				emoteWheel1.CloseEmoteWheel();
				emoteWheel2.CloseEmoteWheel();
				emoteWheel3.CloseEmoteWheel();
			}
		}
		if (!emote1 && openEmoteWheel == emoteWheel1)
		{
			emoteWheel1.CloseEmoteWheelAndExecuteEmote();
			ShowEmoteCancelArea(value: false);
			openEmoteWheel = null;
			emoteWheel2.CloseEmoteWheel();
			emoteWheel3.CloseEmoteWheel();
			emoteWheel4.CloseEmoteWheel();
		}
		if (!emote2 && openEmoteWheel == emoteWheel2)
		{
			emoteWheel2.CloseEmoteWheelAndExecuteEmote();
			ShowEmoteCancelArea(value: false);
			openEmoteWheel = null;
			emoteWheel1.CloseEmoteWheel();
			emoteWheel3.CloseEmoteWheel();
			emoteWheel4.CloseEmoteWheel();
		}
		if (!emote3 && openEmoteWheel == emoteWheel3)
		{
			emoteWheel3.CloseEmoteWheelAndExecuteEmote();
			ShowEmoteCancelArea(value: false);
			openEmoteWheel = null;
			emoteWheel1.CloseEmoteWheel();
			emoteWheel2.CloseEmoteWheel();
			emoteWheel4.CloseEmoteWheel();
		}
		if (!emote4 && openEmoteWheel == emoteWheel4)
		{
			emoteWheel4.CloseEmoteWheelAndExecuteEmote();
			ShowEmoteCancelArea(value: false);
			openEmoteWheel = null;
			emoteWheel1.CloseEmoteWheel();
			emoteWheel2.CloseEmoteWheel();
			emoteWheel3.CloseEmoteWheel();
		}
	}

	private void SelectEmote()
	{
		if (openEmoteWheel != null)
		{
			openEmoteWheel.CloseEmoteWheelAndExecuteEmote();
			openEmoteWheel = null;
			emote1 = false;
			emote2 = false;
			emote3 = false;
			emote4 = false;
		}
	}

	private void ShowEmoteCancelArea(bool value)
	{
		if (!value)
		{
			emoteCancelArea.SetActive(value: false);
		}
	}

	private void InitializeWebColorWheel()
	{
		for (int i = 0; i < webColorWheel.WheelOptions.Length; i++)
		{
			int index = SaveController.Load($"WebWheelSlice_{i}", Singleton<CosmeticItemsController>.Instance.GetDefaultWebIndex(), SaveData.Wardrobe);
			index = Singleton<CosmeticItemsController>.Instance.GetValidWebIndex(index);
			CosmeticItemWebSo cosmeticItemWebSo = (CosmeticItemWebSo)Singleton<CosmeticItemsController>.Instance.GetCosmeticItem(index);
			SetWebColorToSlot(cosmeticItemWebSo, i);
		}
	}

	public void PerformEmoteFromWheel(string emoteName, bool playSound = true)
	{
		Singleton<CameraController>.Instance.PreventCameraMovement(0.5f);
		Singleton<GameController>.Instance.Player.Emotes.PerformEmote(emoteName, playSound, stopCurrentEmote: false);
		onEmotePerformed.Invoke();
		if (QuestLog.GetQuestState("Demo0.3/Tutorial/Emote") == QuestState.Active)
		{
			QuestLog.SetQuestEntryState("Demo0.3/Tutorial/Emote", 1, QuestState.Success);
			QuestLog.SetQuestEntryState("Demo0.3/Tutorial/Emote", 2, QuestState.Active);
			DialogueManager.SendUpdateTracker();
		}
	}

	public void PerformEmoteAll(string emoteName, bool playSound = true, bool stopCurrentEmote = false)
	{
		foreach (BodyMovement spider in Singleton<GameController>.Instance.Spiders)
		{
			spider.Emotes.PerformEmote(emoteName, playSound, stopCurrentEmote);
		}
	}

	public void InvalidEmote()
	{
		invalidEmoteSound.Play();
	}

	public void MobileEmote(bool value)
	{
	}

	public void MobileWebColor(bool value)
	{
	}

	public void MobileSwitchEmoteWheel(bool value)
	{
	}

	public void CancelEmote()
	{
	}

	public void CloseAllWheels()
	{
		openEmoteWheel = null;
		emote1 = false;
		emote2 = false;
		emote3 = false;
		emote4 = false;
		emoteWheel1.CloseEmoteWheel();
		emoteWheel2.CloseEmoteWheel();
		emoteWheel3.CloseEmoteWheel();
		emoteWheel4.CloseEmoteWheel();
		switchEmoteWheelMobileButton.SetActive(value: false);
		ShowEmoteCancelArea(value: false);
	}

	public CosmeticItemWebSo[] GetCurrentWebColors()
	{
		CosmeticItemWebSo[] array = new CosmeticItemWebSo[8];
		for (int i = 0; i < webColorWheel.WheelOptions.Length; i++)
		{
			if (webColorWheel.WheelOptions[i] is WebWheelOptionSo webWheelOptionSo)
			{
				array[i] = webWheelOptionSo.cosmeticItemWebSo;
			}
		}
		return array;
	}

	public void SetWebColorToSlot(CosmeticItemWebSo cosmeticItemWebSo, int slot)
	{
		if (webColorWheel.WheelOptions[slot] is WebWheelOptionSo webWheelOptionSo)
		{
			webWheelOptionSo.SetCosmeticItemWebSo(cosmeticItemWebSo);
		}
		webColorWheel.Setup();
		int itemIndex = Singleton<CosmeticItemsController>.Instance.GetItemIndex(cosmeticItemWebSo);
		SaveController.Save($"WebWheelSlice_{slot}", itemIndex, SaveData.Wardrobe);
	}

	private void OnEmote1(InputAction.CallbackContext ctx)
	{
		if (!Singleton<WebController>.Instance.WebActive && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			emote1 = ctx.ReadValueAsButton();
		}
	}

	private void OnEmote2(InputAction.CallbackContext ctx)
	{
		if (!Singleton<WebController>.Instance.WebActive && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			emote2 = ctx.ReadValueAsButton();
		}
	}

	private void OnEmote3(InputAction.CallbackContext ctx)
	{
		if (!Singleton<WebController>.Instance.WebActive && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			emote3 = ctx.ReadValueAsButton();
		}
	}

	private void OnEmote4(InputAction.CallbackContext ctx)
	{
		if (!Singleton<WebController>.Instance.WebActive && !Singleton<PlayTimeCanvas>.Instance.IsOpen)
		{
			Singleton<MobileControls>.Instance.ShowButtons(value: false);
			emote4 = ctx.ReadValueAsButton();
		}
	}

	private void OnMousePosition(InputAction.CallbackContext ctx)
	{
		if (!(openEmoteWheel == null))
		{
			openEmoteWheel.MousePosition = ctx.ReadValue<Vector2>();
		}
	}

	private void OnSelectEmote(InputAction.CallbackContext ctx)
	{
		SelectEmote();
	}
}
