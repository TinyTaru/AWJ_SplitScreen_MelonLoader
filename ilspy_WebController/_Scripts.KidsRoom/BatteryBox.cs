using FMODUnity;
using TMPro;
using UnityEngine;

namespace _Scripts.KidsRoom;

public class BatteryBox : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private TextMeshPro arrows;

	[SerializeField]
	private CarTrackBooster carTrackBooster;

	[Header("Parameters")]
	[SerializeField]
	private int initialNumberOfBatteries;

	[SerializeField]
	private Color colorBattery0;

	[SerializeField]
	private Color colorBattery1;

	[SerializeField]
	private Color colorBattery2;

	[SerializeField]
	private float forceMagnitudeBattery0;

	[SerializeField]
	private float forceMagnitudeBattery1;

	[SerializeField]
	private float forceMagnitudeBattery2;

	[Header("Sound Effects")]
	[SerializeField]
	private StudioEventEmitter batteryInsertSound;

	private int numberOfBatteries;

	private void Awake()
	{
		numberOfBatteries = initialNumberOfBatteries;
		ApplyBatterySettings();
	}

	private void ApplyBatterySettings()
	{
		if (!(arrows == null) && !(carTrackBooster == null))
		{
			switch (numberOfBatteries)
			{
			case 0:
				arrows.color = colorBattery0;
				carTrackBooster.SetBoostLevel(0, forceMagnitudeBattery0);
				break;
			case 1:
				arrows.color = colorBattery1;
				carTrackBooster.SetBoostLevel(1, forceMagnitudeBattery1);
				break;
			case 2:
				arrows.color = colorBattery2;
				carTrackBooster.SetBoostLevel(2, forceMagnitudeBattery2);
				break;
			}
		}
	}

	public void AddBattery()
	{
		numberOfBatteries++;
		numberOfBatteries = Mathf.Min(numberOfBatteries, 2);
		ApplyBatterySettings();
		batteryInsertSound.Play();
	}
}
