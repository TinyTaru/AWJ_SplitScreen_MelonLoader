using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;

namespace _Scripts.KidsRoom;

public class InvestigationKidsRoom : MonoBehaviour, IInitializable<InvestigationKidsRoomSaveData>, IHasSaveData<InvestigationKidsRoomSaveData>
{
	private enum ButtonColor
	{
		None,
		Red,
		Green,
		Blue
	}

	[Header("References")]
	[SerializeField]
	private GameObject secretDoor;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onCorrectCodeEnteredEvent;

	private ButtonColor[] correctCode = new ButtonColor[5]
	{
		ButtonColor.Red,
		ButtonColor.Red,
		ButtonColor.Green,
		ButtonColor.Blue,
		ButtonColor.Green
	};

	private ButtonColor[] codeInput;

	private bool secretDoorActive = true;

	private void Start()
	{
		codeInput = new ButtonColor[5];
	}

	private void UpdateCodeInput(ButtonColor newButtonColor)
	{
		for (int i = 0; i <= 3; i++)
		{
			codeInput[i] = codeInput[i + 1];
		}
		codeInput[4] = newButtonColor;
		Debug.Log("Current code: " + string.Join(", ", codeInput));
		CheckCode();
	}

	private void CheckCode()
	{
		for (int i = 0; i < codeInput.Length; i++)
		{
			if (codeInput[i] != correctCode[i])
			{
				return;
			}
		}
		Debug.Log("Correct Code!");
		secretDoorActive = false;
		secretDoor.SetActive(secretDoorActive);
		onCorrectCodeEnteredEvent?.Invoke();
	}

	public void Initialize(InvestigationKidsRoomSaveData investigationKidsRoomSaveData)
	{
		secretDoorActive = investigationKidsRoomSaveData.secretDoorActive;
		secretDoor.SetActive(secretDoorActive);
	}

	public InvestigationKidsRoomSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Investigation Kids Room " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		InvestigationKidsRoomSaveData result = default(InvestigationKidsRoomSaveData);
		result.id = id;
		result.secretDoorActive = secretDoorActive;
		return result;
	}

	public void RedButtonPressed()
	{
		UpdateCodeInput(ButtonColor.Red);
	}

	public void GreenButtonPressed()
	{
		UpdateCodeInput(ButtonColor.Green);
	}

	public void BlueButtonPressed()
	{
		UpdateCodeInput(ButtonColor.Blue);
	}
}
