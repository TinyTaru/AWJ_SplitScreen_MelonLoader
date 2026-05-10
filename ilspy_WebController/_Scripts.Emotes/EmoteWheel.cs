using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.Singletons;
using _Scripts.Wardrobe;
using _Scripts.Web;

namespace _Scripts.Emotes;

public class EmoteWheel : MonoBehaviour
{
	[SerializeField]
	private Color baseColor;

	[SerializeField]
	private Color hoverColor;

	[SerializeField]
	private Color disableColor;

	[SerializeField]
	private Transform sliceContainer;

	[SerializeField]
	private GameObject mobileTouchArea;

	[SerializeField]
	private WheelSlice emoteSlicePrefab;

	[SerializeField]
	private WheelSlice webColorSlicePrefab;

	[SerializeField]
	private WheelSlice webTypeSlicePrefab;

	[FormerlySerializedAs("emotes")]
	[SerializeField]
	private WheelOptionSo[] wheelOptions;

	[SerializeField]
	private float minMouseDistanceToSelect;

	[SerializeField]
	private float maxMouseDistanceToSelect;

	[SerializeField]
	private float minStickDistanceToSelect = 0.5f;

	[SerializeField]
	private WebColorPaletteSO colorPalette;

	private bool isOpen;

	private WheelSlice[] wheelSlices;

	private Vector2 centeredMousePosition;

	private float currentAngle;

	private int selection;

	public Vector2 MousePosition { get; set; }

	public WheelOptionSo[] WheelOptions => wheelOptions;

	private void Start()
	{
		Setup();
	}

	private void Update()
	{
		if (Singleton<GameController>.Instance.InputIsKeyboardMouse)
		{
			centeredMousePosition = new Vector2(MousePosition.x - (float)Screen.width / 2f, MousePosition.y - (float)Screen.height / 2f);
			currentAngle = (Mathf.Atan2(centeredMousePosition.y, centeredMousePosition.x) * 57.29578f + 360f + 22.5f) % 360f;
			float num = centeredMousePosition.magnitude / ((float)Screen.height / 2f);
			if (num > minMouseDistanceToSelect && num < maxMouseDistanceToSelect)
			{
				selection = (int)(currentAngle / 45f);
			}
			else
			{
				selection = -1;
			}
		}
		else if (MousePosition.magnitude > minStickDistanceToSelect)
		{
			currentAngle = (Mathf.Atan2(MousePosition.y, MousePosition.x) * 57.29578f + 360f + 22.5f) % 360f;
			selection = (int)(currentAngle / 45f);
		}
		for (int i = 0; i < wheelSlices.Length; i++)
		{
			if (wheelSlices[i].Interactable)
			{
				wheelSlices[i].SetBackgroundImageColor((i == selection) ? hoverColor : baseColor);
			}
			else
			{
				wheelSlices[i].SetBackgroundImageColor(disableColor);
			}
		}
	}

	public void Setup()
	{
		foreach (Transform item in sliceContainer)
		{
			Object.Destroy(item.gameObject);
		}
		isOpen = false;
		sliceContainer.gameObject.SetActive(isOpen);
		wheelSlices = new WheelSlice[8];
		for (int i = 0; i < 8; i++)
		{
			float z = ((float)i * 45f - 22.5f) % 360f;
			WheelSlice original = null;
			WheelOptionSo wheelOptionSo = wheelOptions[i];
			if (!(wheelOptionSo is EmoteSO))
			{
				if (!(wheelOptionSo is WebWheelOptionSo))
				{
					if (wheelOptionSo is WebTypeSO)
					{
						original = webTypeSlicePrefab;
					}
				}
				else
				{
					original = webColorSlicePrefab;
				}
			}
			else
			{
				original = emoteSlicePrefab;
			}
			WheelSlice wheelSlice = Object.Instantiate(original, sliceContainer);
			wheelSlice.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
			wheelSlice.transform.rotation = Quaternion.Euler(0f, 0f, z);
			wheelSlice.SetBackgroundImageColor(baseColor);
			wheelSlice.Setup();
			if (!(wheelSlice is EmoteSlice emoteSlice))
			{
				if (!(wheelSlice is WebColorSlice webColorSlice))
				{
					if (wheelSlice is WebTypeSlice webTypeSlice)
					{
						webTypeSlice.SetDisplayText(((WebTypeSO)wheelOptions[i]).displayName);
					}
				}
				else
				{
					webColorSlice.SetWebColorImage((WebWheelOptionSo)wheelOptions[i]);
				}
			}
			else
			{
				emoteSlice.SetDisplayText((i < wheelOptions.Length) ? ((EmoteSO)wheelOptions[i]).displayName : "-");
			}
			wheelSlices[i] = wheelSlice;
		}
	}

	public void OpenEmoteWheel()
	{
		if (isOpen)
		{
			return;
		}
		for (int i = 0; i < wheelSlices.Length; i++)
		{
			WheelSlice wheelSlice = wheelSlices[i];
			if (!(wheelSlice is EmoteSlice emoteSlice))
			{
				if (!(wheelSlice is WebColorSlice webColorSlice))
				{
					if (wheelSlice is WebTypeSlice webTypeSlice)
					{
						webTypeSlice.SetDisplayText(((WebTypeSO)wheelOptions[i]).displayName);
					}
				}
				else
				{
					webColorSlice.SetWebColorImage((WebWheelOptionSo)wheelOptions[i]);
				}
			}
			else
			{
				emoteSlice.SetDisplayText((i < wheelOptions.Length) ? ((EmoteSO)wheelOptions[i]).displayName : "-");
			}
		}
		selection = -1;
		MousePosition = Vector2.zero;
		isOpen = true;
		sliceContainer.gameObject.SetActive(isOpen);
		Singleton<GameController>.Instance.State = GameController.GameState.SelectEmote;
		Cursor.visible = isOpen;
		Cursor.lockState = ((!isOpen) ? CursorLockMode.Locked : CursorLockMode.None);
	}

	public void CloseEmoteWheelAndExecuteEmote()
	{
		if (isOpen)
		{
			isOpen = false;
			sliceContainer.gameObject.SetActive(isOpen);
			Cursor.visible = isOpen;
			Cursor.lockState = ((!isOpen) ? CursorLockMode.Locked : CursorLockMode.None);
			if (selection >= 0 && selection < wheelOptions.Length && wheelSlices[selection].Interactable)
			{
				wheelOptions[selection].ExecuteSelection();
			}
			else
			{
				Singleton<GameController>.Instance.State = GameController.GameState.Running;
				Singleton<EmoteController>.Instance.CancelEmote();
			}
			selection = -1;
			MousePosition = Vector2.zero;
		}
	}

	public void CloseEmoteWheel()
	{
		if (isOpen)
		{
			isOpen = false;
			sliceContainer.gameObject.SetActive(isOpen);
		}
	}

	public void MobileExecuteEmote(Vector2 position)
	{
		centeredMousePosition = new Vector2(position.x - (float)Screen.width / 2f, position.y - (float)Screen.height / 2f);
		currentAngle = (Mathf.Atan2(centeredMousePosition.y, centeredMousePosition.x) * 57.29578f + 360f + 22.5f) % 360f;
		float num = centeredMousePosition.magnitude / ((float)Screen.height / 2f);
		if (num > minMouseDistanceToSelect && num < maxMouseDistanceToSelect)
		{
			selection = (int)(currentAngle / 45f);
		}
		else
		{
			selection = -1;
		}
		CloseEmoteWheelAndExecuteEmote();
	}
}
