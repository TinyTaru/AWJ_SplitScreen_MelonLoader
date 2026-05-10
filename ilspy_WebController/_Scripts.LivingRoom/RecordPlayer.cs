using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using _Scripts.General;
using _Scripts.Objects;
using _Scripts.Singletons;

namespace _Scripts.LivingRoom;

public class RecordPlayer : MonoBehaviour
{
	private enum RecordPlayerState
	{
		Idle,
		VinylInserted,
		MoveArmToStartPosition,
		Playing
	}

	[SerializeField]
	private EventReference recordPlayerReference;

	[SerializeField]
	private float defaultVolume = 1f;

	[SerializeField]
	private float defaultSpeed;

	[SerializeField]
	private Transform speedSlider;

	[SerializeField]
	private ConfigurableJoint speedSliderJoint;

	[SerializeField]
	private float speedParameterUpdateThreshold = 0.05f;

	[SerializeField]
	private MagneticLock magneticLockPlatter;

	[SerializeField]
	private Rigidbody rigidbodyPlatter;

	[SerializeField]
	private float defaultPlatterSpeed = 180f;

	[SerializeField]
	private float songLengthJazz = 100f;

	[SerializeField]
	private float songLengthFunk = 100f;

	[SerializeField]
	private float songLengthPiano = 100f;

	[SerializeField]
	private Transform volumeKnob;

	[SerializeField]
	private float volumeParameterUpdateThreshold = 0.05f;

	[SerializeField]
	private float volumeKnobZeroRotation = -135f;

	[SerializeField]
	private float volumeKnobRange = 270f;

	[SerializeField]
	private bool autoRestartSong = true;

	[SerializeField]
	private float armStartAngle = 15f;

	[SerializeField]
	private float armEndAngle = 36f;

	[SerializeField]
	private Rigidbody rigidbodyArm;

	[SerializeField]
	[QuestPopup(false)]
	private string questName;

	[SerializeField]
	private StudioEventEmitter armMoveSound;

	[SerializeField]
	private float armMoveSpeed = 20f;

	private Dictionary<RecordType, float> songLengthDictionary;

	private RecordType currentRecordType;

	private int recordNumber;

	private EventInstance recordPlayerSound;

	private float volume;

	private float speed;

	private float speedSliderNeutralZPosition;

	private float speedSliderMaxZOffset;

	private float oldSpeedSliderValue;

	private VinylRecord currentVinylRecord;

	private float currentSongLength;

	private float songProgress;

	private float platterSpeed;

	private Rigidbody rigidbodyVinylRecord;

	private bool playButtonPressed;

	private float oldVolumeKnobValue;

	private float armAngleRange;

	private RecordPlayerState state;

	private FixedJoint fixedJointPlatter;

	private float fixedJointPlatterBreakForce;

	private float fixedJointPlatterBreakTorque;

	private bool isPluggedIn;

	private Coroutine moveArmCoroutine;

	private void Start()
	{
		isPluggedIn = false;
		state = RecordPlayerState.Idle;
		currentRecordType = RecordType.None;
		recordPlayerSound = RuntimeManager.CreateInstance(recordPlayerReference);
		volume = defaultVolume;
		speed = defaultSpeed;
		speedSliderNeutralZPosition = speedSlider.localPosition.z;
		speedSliderMaxZOffset = speedSliderJoint.linearLimit.limit;
		currentSongLength = 0f;
		platterSpeed = defaultPlatterSpeed;
		songLengthDictionary = new Dictionary<RecordType, float>
		{
			{
				RecordType.None,
				0f
			},
			{
				RecordType.Funk,
				songLengthFunk
			},
			{
				RecordType.Jazz,
				songLengthJazz
			},
			{
				RecordType.Piano,
				songLengthPiano
			}
		};
		volumeKnob.localRotation = Quaternion.Euler(0f, 135f, 0f);
		armAngleRange = armEndAngle - armStartAngle;
		fixedJointPlatterBreakForce = magneticLockPlatter.BreakForce;
		fixedJointPlatterBreakTorque = magneticLockPlatter.BreakTorque;
	}

	private void Update()
	{
		float num = volumeKnob.transform.localRotation.eulerAngles.y;
		if (num > 180f)
		{
			num -= 360f;
		}
		float num2 = (num - volumeKnobZeroRotation) / volumeKnobRange;
		if (Mathf.Abs(num2 - oldVolumeKnobValue) > volumeParameterUpdateThreshold)
		{
			SetVolume(num2);
			oldVolumeKnobValue = num2;
		}
		float num3 = (speedSlider.localPosition.z - speedSliderNeutralZPosition) / speedSliderMaxZOffset;
		if (Mathf.Abs(num3 - oldSpeedSliderValue) > speedParameterUpdateThreshold)
		{
			SetSpeed(num3);
			oldSpeedSliderValue = num3;
		}
		switch (state)
		{
		case RecordPlayerState.Playing:
		{
			recordPlayerSound.getTimelinePosition(out var position);
			songProgress = (float)position * 0.001f / currentSongLength;
			platterSpeed = defaultPlatterSpeed * (1f + num3 * 0.5f);
			if (songProgress >= 1f)
			{
				if (autoRestartSong)
				{
					recordPlayerSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
					PlaySong();
				}
				else
				{
					StopSong();
				}
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case RecordPlayerState.Idle:
		case RecordPlayerState.VinylInserted:
		case RecordPlayerState.MoveArmToStartPosition:
			break;
		}
	}

	private void FixedUpdate()
	{
		switch (state)
		{
		case RecordPlayerState.MoveArmToStartPosition:
			RotatePlatterAndVinylRecord();
			break;
		case RecordPlayerState.Playing:
		{
			RotatePlatterAndVinylRecord();
			float y = armStartAngle + armAngleRange * songProgress;
			rigidbodyArm.transform.localRotation = Quaternion.Euler(0f, y, 0f);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case RecordPlayerState.Idle:
		case RecordPlayerState.VinylInserted:
			break;
		}
	}

	private void OnDestroy()
	{
		recordPlayerSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.SetLivingRoomRecordPlayerOnState(isOn: false);
	}

	private IEnumerator MoveArmToStartPositionCoroutine()
	{
		state = RecordPlayerState.MoveArmToStartPosition;
		armMoveSound.Play();
		yield return rigidbodyArm.transform.DOLocalRotate(new Vector3(0f, armStartAngle, 0f), armMoveSpeed).SetSpeedBased().WaitForCompletion();
		armMoveSound.Stop();
		StartSong();
	}

	private void SetVinylRecordType(RecordType newRecordType)
	{
		currentRecordType = newRecordType;
		recordPlayerSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		switch (currentRecordType)
		{
		case RecordType.Funk:
			recordNumber = 1;
			break;
		case RecordType.Jazz:
			recordNumber = 0;
			break;
		case RecordType.Piano:
			recordNumber = 2;
			break;
		default:
			throw new ArgumentOutOfRangeException("currentRecordType", currentRecordType, null);
		case RecordType.None:
			break;
		}
		state = ((currentRecordType != 0) ? RecordPlayerState.VinylInserted : RecordPlayerState.Idle);
	}

	private void RotatePlatterAndVinylRecord()
	{
		rigidbodyPlatter.angularVelocity = new Vector3(0f, platterSpeed * (MathF.PI / 180f), 0f);
		rigidbodyVinylRecord.angularVelocity = new Vector3(0f, platterSpeed * (MathF.PI / 180f), 0f);
	}

	private void PlaySong()
	{
		if (isPluggedIn)
		{
			Debug.Log($"PlaySong. currentRecordType {currentRecordType}");
			if (currentRecordType != 0)
			{
				songProgress = 0f;
				rigidbodyVinylRecord = currentVinylRecord.GetComponent<Rigidbody>();
				rigidbodyArm.isKinematic = true;
				fixedJointPlatter = rigidbodyPlatter.GetComponent<FixedJoint>();
				fixedJointPlatter.breakForce = float.PositiveInfinity;
				fixedJointPlatter.breakTorque = float.PositiveInfinity;
				moveArmCoroutine = StartCoroutine(MoveArmToStartPositionCoroutine());
			}
		}
	}

	private void StartSong()
	{
		state = RecordPlayerState.Playing;
		currentSongLength = songLengthDictionary[currentRecordType];
		Singleton<MusicController>.Instance.SetLivingRoomRecordPlayerOnState(isOn: true);
		recordPlayerSound.start();
		recordPlayerSound.setParameterByName("volume", volume);
		recordPlayerSound.setParameterByName("record_speed", speed);
		recordPlayerSound.setParameterByName("record_number", recordNumber);
		QuestLog.SetQuestState(questName, QuestState.Success);
	}

	private void StopSong()
	{
		state = RecordPlayerState.VinylInserted;
		recordPlayerSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		Singleton<MusicController>.Instance.SetLivingRoomRecordPlayerOnState(isOn: false);
		rigidbodyArm.isKinematic = false;
		fixedJointPlatter.breakForce = fixedJointPlatterBreakForce;
		fixedJointPlatter.breakTorque = fixedJointPlatterBreakTorque;
	}

	private void SetVolume(float newVolume)
	{
		volume = newVolume;
		recordPlayerSound.setParameterByName("volume", volume);
	}

	private void SetSpeed(float newSpeed)
	{
		speed = newSpeed;
		recordPlayerSound.setParameterByName("record_speed", speed);
	}

	public void PlugIn()
	{
		isPluggedIn = true;
	}

	public void UnPlug()
	{
		isPluggedIn = false;
		switch (state)
		{
		case RecordPlayerState.MoveArmToStartPosition:
			StopCoroutine(moveArmCoroutine);
			rigidbodyArm.transform.DOKill();
			armMoveSound.Stop();
			StopSong();
			break;
		case RecordPlayerState.Playing:
			StopSong();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case RecordPlayerState.Idle:
		case RecordPlayerState.VinylInserted:
			break;
		}
	}

	public void InsertVinylRecord()
	{
		currentVinylRecord = magneticLockPlatter.ConnectedObject.GetComponent<VinylRecord>();
		SetVinylRecordType((!(currentVinylRecord == null)) ? currentVinylRecord.GetRecordType() : RecordType.None);
	}

	public void RemoveVinylRecord()
	{
		currentVinylRecord = null;
		SetVinylRecordType(RecordType.None);
	}

	public void PressPlayButton()
	{
		playButtonPressed = true;
		switch (state)
		{
		case RecordPlayerState.VinylInserted:
			PlaySong();
			break;
		case RecordPlayerState.Playing:
			StopSong();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case RecordPlayerState.Idle:
		case RecordPlayerState.MoveArmToStartPosition:
			break;
		}
	}

	public void ReleasePlayButton()
	{
		playButtonPressed = false;
	}
}
