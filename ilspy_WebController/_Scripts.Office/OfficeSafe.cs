using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;

namespace _Scripts.Office;

public class OfficeSafe : MonoBehaviour, IInitializable<OfficeSafeSaveData>, IHasSaveData<OfficeSafeSaveData>
{
	public enum SafeState
	{
		Locked,
		WrongCode,
		CorrectCode,
		Open
	}

	[Header("References")]
	[SerializeField]
	private Rigidbody door;

	[SerializeField]
	private Rigidbody handle;

	[SerializeField]
	private TextMeshPro codeInputText;

	[SerializeField]
	private MeshRenderer displayBackground;

	[SerializeField]
	private SafeHint[] safeHints;

	[Header("Parameter")]
	[SerializeField]
	private Color defaultCodeColor;

	[SerializeField]
	private Color correctCodeColor;

	[SerializeField]
	private Color wrongCodeColor;

	[SerializeField]
	private float handleOpenThreshold = 85f;

	[SerializeField]
	private float doorPopOpenTorqueImpulse = 100f;

	[SerializeField]
	private float resetCodeDelay = 2f;

	[SerializeField]
	private float niceDelay = 0.5f;

	[Header("Sounds")]
	[SerializeField]
	private StudioEventEmitter niceSound;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onWrongCodeEvent;

	[SerializeField]
	private UnityEvent onCorrectCodeEvent;

	[SerializeField]
	private UnityEvent onSafeOpenEvent;

	private string correctCode;

	private SafeState safeState;

	private string codeInput;

	private Coroutine resetCodeCor;

	private string[] hintStrings;

	private Color displayColor;

	private static MaterialPropertyBlock mpb;

	private static readonly int colorId = Shader.PropertyToID("_Color");

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		safeState = SafeState.Locked;
		door.isKinematic = true;
		handle.isKinematic = true;
		codeInput = string.Empty;
		codeInputText.text = codeInput;
		resetCodeCor = null;
		displayColor = defaultCodeColor;
		ApplyDisplayColor();
		RandomizeCodeAndHints();
	}

	private void Update()
	{
		if (safeState == SafeState.CorrectCode && handle.transform.localRotation.eulerAngles.z > handleOpenThreshold)
		{
			SafeOpened();
		}
	}

	private IEnumerator ResetCodeCoroutine()
	{
		yield return new WaitForSeconds(resetCodeDelay);
		displayColor = defaultCodeColor;
		ApplyDisplayColor();
		codeInput = string.Empty;
		codeInputText.text = codeInput;
		safeState = SafeState.Locked;
	}

	private IEnumerator PlayNiceSoundCoroutine()
	{
		yield return new WaitForSeconds(niceDelay);
		niceSound.Play();
	}

	private void RandomizeCodeAndHints()
	{
		correctCode = string.Empty;
		for (int i = 0; i < 6; i++)
		{
			correctCode += Random.Range(0, 10);
		}
		Debug.Log("Correct Code = " + correctCode);
		List<int> list = new List<int> { 0, 1, 2, 3, 4, 5 };
		hintStrings = new string[safeHints.Length];
		for (int j = 0; j < safeHints.Length; j++)
		{
			List<int> list2 = new List<int>();
			int item = list[Random.Range(0, list.Count)];
			list.Remove(item);
			list2.Add(item);
			item = list[Random.Range(0, list.Count)];
			list.Remove(item);
			list2.Add(item);
			string text = "";
			for (int k = 0; k < 6; k++)
			{
				text += (list2.Contains(k) ? ((object)correctCode[k]) : "_");
			}
			hintStrings[j] = text;
		}
		UpdateSafeHints(hintStrings);
	}

	private void UpdateSafeHints(string[] hintStrings)
	{
		for (int i = 0; i < safeHints.Length; i++)
		{
			safeHints[i].SetHint(hintStrings[i]);
		}
	}

	private void CheckCode()
	{
		if (codeInput.Length >= correctCode.Length)
		{
			if (codeInput == correctCode)
			{
				CorrectCodeInput();
			}
			else
			{
				WrongCodeInput();
			}
			ApplyDisplayColor();
		}
	}

	private void CorrectCodeInput()
	{
		displayColor = correctCodeColor;
		handle.isKinematic = false;
		safeState = SafeState.CorrectCode;
		onCorrectCodeEvent?.Invoke();
	}

	private void WrongCodeInput()
	{
		displayColor = wrongCodeColor;
		safeState = SafeState.WrongCode;
		resetCodeCor = StartCoroutine(ResetCodeCoroutine());
		onWrongCodeEvent?.Invoke();
	}

	private void SafeOpened()
	{
		door.isKinematic = false;
		door.AddTorque(-base.transform.up * doorPopOpenTorqueImpulse, ForceMode.Impulse);
		safeState = SafeState.Open;
		onSafeOpenEvent?.Invoke();
	}

	private void ApplyDisplayColor()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
		displayBackground.GetPropertyBlock(mpb);
		mpb.SetColor(colorId, displayColor);
		displayBackground.SetPropertyBlock(mpb);
	}

	public void Initialize(OfficeSafeSaveData officeSafeSaveData)
	{
		correctCode = officeSafeSaveData.correctCode;
		hintStrings = officeSafeSaveData.hintStrings;
		codeInput = officeSafeSaveData.codeInput;
		safeState = officeSafeSaveData.safeState;
		displayColor = officeSafeSaveData.displayColor;
		switch (safeState)
		{
		case SafeState.WrongCode:
			WrongCodeInput();
			break;
		case SafeState.CorrectCode:
			CorrectCodeInput();
			break;
		case SafeState.Open:
			door.isKinematic = false;
			handle.isKinematic = false;
			onCorrectCodeEvent?.Invoke();
			onSafeOpenEvent?.Invoke();
			break;
		}
		codeInputText.text = codeInput;
		ApplyDisplayColor();
		UpdateSafeHints(hintStrings);
	}

	public OfficeSafeSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Office Safe " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		OfficeSafeSaveData result = default(OfficeSafeSaveData);
		result.id = id;
		result.correctCode = correctCode;
		result.hintStrings = hintStrings;
		result.codeInput = codeInput;
		result.safeState = safeState;
		result.displayColor = displayColor;
		return result;
	}

	public void InputCode(int number)
	{
		if (safeState != SafeState.CorrectCode && safeState != SafeState.Open)
		{
			if (resetCodeCor != null)
			{
				StopCoroutine(resetCodeCor);
				resetCodeCor = null;
			}
			if (safeState == SafeState.WrongCode)
			{
				displayColor = defaultCodeColor;
				codeInput = string.Empty;
				safeState = SafeState.Locked;
				ApplyDisplayColor();
			}
			codeInput += number;
			codeInputText.text = codeInput;
			if (codeInput.Length == 2 && codeInput == "69")
			{
				StartCoroutine(PlayNiceSoundCoroutine());
			}
			CheckCode();
		}
	}
}
