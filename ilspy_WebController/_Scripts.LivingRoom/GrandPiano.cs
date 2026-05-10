using System;
using System.Collections;
using System.Linq;
using FMOD.Studio;
using FMODUnity;
using QFSW.QC;
using UnityEngine;
using UnityEngine.Events;
using WebSocketSharp;

namespace _Scripts.LivingRoom;

public class GrandPiano : MonoBehaviour
{
	[SerializeField]
	private Transform keyContainer;

	[SerializeField]
	private StudioEventEmitter keyPressedSound;

	[SerializeField]
	private EventReference pianoKeySoundReference;

	[SerializeField]
	private GrandPianoKey[] allPianoKeys;

	[SerializeField]
	private GrandPianoKey[] whiteKeys;

	[SerializeField]
	private GrandPianoKey[] blackKeys;

	[SerializeField]
	private Material defaultWhiteKeyMaterial;

	[SerializeField]
	private Material defaultBlackKeyMaterial;

	[SerializeField]
	private Material shmoopWhiteKeyMaterial;

	[SerializeField]
	private Material shmoopBlackKeyMaterial;

	[SerializeField]
	private TextAsset beatChartCsv;

	[SerializeField]
	private Color beat4Color;

	[SerializeField]
	private Color beat2Color;

	[SerializeField]
	private Color beat1Color;

	[SerializeField]
	private Color nextKeyColor;

	[SerializeField]
	private Material tutorialMaterial;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onMiddlePedalActivatedEvent;

	private bool leftPedalActive;

	private bool middlePedalActive;

	private bool rightPedalActive;

	private EventInstance[] pianoKeySoundInstances;

	private EventInstance[] tutorialSoundInstances;

	private float currentBpm;

	private int[][] beatChartLines;

	private int currentBeat;

	private float beatDuration;

	private Color whiteKeyColor;

	private Color blackKeyColor;

	private int tutorialKeyIndexMin;

	private int tutorialKeyIndexMax;

	private bool playTutorialKeys;

	private bool overwriteBpm;

	private bool showNextTutorialKey = true;

	private void Start()
	{
		leftPedalActive = false;
		middlePedalActive = false;
		rightPedalActive = false;
		whiteKeyColor = defaultWhiteKeyMaterial.GetColor("_Color");
		blackKeyColor = defaultBlackKeyMaterial.GetColor("_Color");
		pianoKeySoundInstances = new EventInstance[keyContainer.childCount];
		tutorialSoundInstances = new EventInstance[keyContainer.childCount];
		for (int i = 0; i < keyContainer.childCount; i++)
		{
			if (!(keyContainer.GetChild(i).GetComponent<GrandPianoKey>() == null))
			{
				pianoKeySoundInstances[i] = RuntimeManager.CreateInstance(pianoKeySoundReference);
				pianoKeySoundInstances[i].setParameterByName("piano_note_index", i + 1);
				RuntimeManager.AttachInstanceToGameObject(pianoKeySoundInstances[i], keyPressedSound.gameObject);
				tutorialSoundInstances[i] = RuntimeManager.CreateInstance(pianoKeySoundReference);
				tutorialSoundInstances[i].setParameterByName("piano_note_index", i + 1);
				RuntimeManager.AttachInstanceToGameObject(tutorialSoundInstances[i], keyPressedSound.gameObject);
			}
		}
	}

	private IEnumerator TutorialModeCoroutine()
	{
		currentBeat = 0;
		for (int i = 0; i < whiteKeys.Length; i++)
		{
			whiteKeys[i].SetKeyMaterial(tutorialMaterial);
			whiteKeys[i].SetBackgroundColor(whiteKeyColor);
		}
		for (int j = 0; j < blackKeys.Length; j++)
		{
			blackKeys[j].SetKeyMaterial(tutorialMaterial);
			blackKeys[j].SetBackgroundColor(blackKeyColor);
		}
		GrandPianoKey grandPianoKey = allPianoKeys[beatChartLines[currentBeat].ToList().FindIndex((int x) => x != 0)];
		if (grandPianoKey != null)
		{
			grandPianoKey.StartTutorialProgress(4f);
			grandPianoKey.SetActiveColor(nextKeyColor);
			yield return new WaitForSeconds(4f);
		}
		while (currentBeat < beatChartLines.Length)
		{
			for (int k = tutorialKeyIndexMin; k < tutorialKeyIndexMax; k++)
			{
				int num = beatChartLines[currentBeat][k];
				if (num != 0)
				{
					if (playTutorialKeys)
					{
						PlayTutorialKey(k);
					}
					allPianoKeys[k].StartTutorialProgress(beatDuration * (float)num);
					switch (num)
					{
					case 1:
						allPianoKeys[k].SetActiveColor(beat1Color);
						break;
					case 2:
						allPianoKeys[k].SetActiveColor(beat2Color);
						break;
					case 4:
						allPianoKeys[k].SetActiveColor(beat4Color);
						break;
					}
				}
				if (showNextTutorialKey && currentBeat + 1 < beatChartLines.Length)
				{
					allPianoKeys[k].SetBackgroundColor((beatChartLines[currentBeat + 1][k] != 0) ? nextKeyColor : ((allPianoKeys[k].GetKeyColor == GrandPianoKey.KeyColor.White) ? whiteKeyColor : blackKeyColor));
				}
			}
			yield return new WaitForSeconds(beatDuration);
			currentBeat++;
		}
		GrandPianoKey[] array = allPianoKeys;
		foreach (GrandPianoKey obj in array)
		{
			obj.SetKeyMaterial((obj.GetKeyColor == GrandPianoKey.KeyColor.White) ? defaultWhiteKeyMaterial : defaultBlackKeyMaterial);
		}
	}

	private void ShowNextTutorialKey(bool value)
	{
		showNextTutorialKey = value;
	}

	private void PlayTutorialKeys(bool value)
	{
		playTutorialKeys = value;
	}

	private void SetTutorialBpm(float value)
	{
		overwriteBpm = true;
		currentBpm = value;
	}

	private void ResetTutorialBpm()
	{
		overwriteBpm = false;
	}

	private void SetTutorialVolume(float value)
	{
		value = Mathf.Clamp01(value);
		EventInstance[] array = tutorialSoundInstances;
		foreach (EventInstance eventInstance in array)
		{
			eventInstance.setParameterByName("volume", value);
		}
	}

	private void PlayTutorialKey(int keyIndex)
	{
		tutorialSoundInstances[keyIndex - 1].setParameterByName("piano_left_pedal", leftPedalActive ? 1f : 0f);
		tutorialSoundInstances[keyIndex - 1].setParameterByName("piano_middle_pedal", middlePedalActive ? 1f : 0f);
		tutorialSoundInstances[keyIndex - 1].setParameterByName("piano_right_pedal", rightPedalActive ? 1f : 0f);
		tutorialSoundInstances[keyIndex - 1].start();
	}

	private void SetWhiteKeyMaterial(Material material)
	{
		GrandPianoKey[] array = whiteKeys;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetKeyMaterial(material);
		}
	}

	private void SetBlackKeyMaterial(Material material)
	{
		GrandPianoKey[] array = blackKeys;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetKeyMaterial(material);
		}
	}

	private void ReadCsvFile()
	{
		if (beatChartCsv == null)
		{
			return;
		}
		string[] array = beatChartCsv.text.Split("\n");
		string[] array2 = array[0].Split(",");
		if (!overwriteBpm)
		{
			currentBpm = float.Parse(array2[0]);
		}
		beatDuration = 60f / currentBpm;
		Debug.Log($"Read BPM as {currentBpm}");
		tutorialKeyIndexMin = 0;
		tutorialKeyIndexMax = allPianoKeys.Length - 1;
		beatChartLines = new int[array.Length - 2][];
		for (int i = 1; i < array.Length - 1; i++)
		{
			beatChartLines[i - 1] = new int[88];
			if (array.All((string x) => x.IsNullOrEmpty()))
			{
				continue;
			}
			string[] array3 = array[i].Split(",");
			for (int j = 1; j < array2.Length; j++)
			{
				int.TryParse(array3[j], out var result);
				if (result == 0)
				{
					continue;
				}
				string text = array2[j];
				for (int k = 0; k < allPianoKeys.Length; k++)
				{
					if (string.Equals(allPianoKeys[k].KeyName.Trim(), text.Trim(), StringComparison.CurrentCultureIgnoreCase))
					{
						beatChartLines[i - 1][k] = result;
						if (k < tutorialKeyIndexMin)
						{
							tutorialKeyIndexMin = k;
						}
						if (k > tutorialKeyIndexMax)
						{
							tutorialKeyIndexMax = k;
						}
					}
				}
			}
		}
		string text2 = "";
		for (int l = 0; l < beatChartLines.Length; l++)
		{
			string text3 = "";
			int[] array4 = beatChartLines[l];
			foreach (int num in array4)
			{
				text3 += $"{num} ";
			}
			text2 += $"Beat {l}: {text3}\n";
		}
		Debug.Log(text2);
	}

	public void PressKey(int keyIndex)
	{
		pianoKeySoundInstances[keyIndex - 1].setParameterByName("piano_left_pedal", leftPedalActive ? 1f : 0f);
		pianoKeySoundInstances[keyIndex - 1].setParameterByName("piano_middle_pedal", middlePedalActive ? 1f : 0f);
		pianoKeySoundInstances[keyIndex - 1].setParameterByName("piano_right_pedal", rightPedalActive ? 1f : 0f);
		pianoKeySoundInstances[keyIndex - 1].start();
	}

	public void ReleaseKey(int keyIndex)
	{
		if (!rightPedalActive)
		{
			pianoKeySoundInstances[keyIndex - 1].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
	}

	public void SetLeftPedalActive(bool value)
	{
		leftPedalActive = value;
	}

	public void SetMiddlePedalActive(bool value)
	{
		middlePedalActive = value;
		if (middlePedalActive)
		{
			SetWhiteKeyMaterial(shmoopWhiteKeyMaterial);
			SetBlackKeyMaterial(shmoopBlackKeyMaterial);
			onMiddlePedalActivatedEvent?.Invoke();
		}
		else
		{
			SetWhiteKeyMaterial(defaultWhiteKeyMaterial);
			SetBlackKeyMaterial(defaultBlackKeyMaterial);
		}
	}

	public void SetRightPedalActive(bool value)
	{
		rightPedalActive = value;
		if (!rightPedalActive)
		{
			EventInstance[] array = pianoKeySoundInstances;
			foreach (EventInstance eventInstance in array)
			{
				eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			}
		}
	}

	[Command("StartTutorialMode", QFSW.QC.Platform.AllPlatforms, MonoTargetType.Single)]
	public void StartTutorialMode()
	{
		ReadCsvFile();
		StartCoroutine(TutorialModeCoroutine());
	}
}
