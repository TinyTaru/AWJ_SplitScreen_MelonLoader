using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.Effects;
using _Scripts.LevelSaving;
using _Scripts.Utils;

namespace _Scripts.Office;

public class DeskFan : MonoBehaviour, IInitializable<DeskFanSaveData>, IHasSaveData<DeskFanSaveData>
{
	[Header("References")]
	[SerializeField]
	private Rigidbody cage;

	[SerializeField]
	private ConstantRotation blade;

	[SerializeField]
	private WindArea windArea;

	[Header("Parameters")]
	[SerializeField]
	private bool activeAtStart;

	[SerializeField]
	private Vector3 cageStartRotation;

	[SerializeField]
	private float bladeRotationSpeed;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onTurnedOnEvent;

	[SerializeField]
	private UnityEvent onTurnedOffEvent;

	private bool isActive;

	private void Start()
	{
		cage.rotation = Quaternion.Euler(cageStartRotation);
		if (activeAtStart)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	private IEnumerator InitializeCoroutine(DeskFanSaveData deskFanSaveData)
	{
		yield return null;
		isActive = deskFanSaveData.isActive;
		if (isActive)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	private void TurnOn()
	{
		isActive = true;
		blade.SetSpeed(bladeRotationSpeed);
		windArea.SetActive(value: true);
		onTurnedOnEvent?.Invoke();
	}

	private void TurnOff()
	{
		isActive = false;
		blade.SetSpeed(0f);
		windArea.SetActive(value: false);
		onTurnedOffEvent?.Invoke();
	}

	public void Initialize(DeskFanSaveData deskFanSaveData)
	{
		StartCoroutine(InitializeCoroutine(deskFanSaveData));
	}

	public DeskFanSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Desk Fan " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		DeskFanSaveData result = default(DeskFanSaveData);
		result.id = id;
		result.isActive = isActive;
		return result;
	}

	public void ToggleOnOff()
	{
		if (isActive)
		{
			TurnOff();
		}
		else
		{
			TurnOn();
		}
	}
}
