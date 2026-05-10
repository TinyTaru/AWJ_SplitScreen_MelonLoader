using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.LivingRoom;

[DisallowMultipleComponent]
public class GrandPianoKey : MonoBehaviour
{
	private enum KeyPosition
	{
		Middle,
		Down,
		Up,
		MainWeb
	}

	public enum KeyColor
	{
		White,
		Black
	}

	[Header("References")]
	[SerializeField]
	private GrandPiano grandPiano;

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private KeyColor keyColor;

	[Header("Parameters")]
	[SerializeField]
	private int keyIndex;

	[Space(10f)]
	[SerializeField]
	private float positiveActivationThreshold = 2f;

	[SerializeField]
	private float positiveDeactivationThreshold = 1f;

	[SerializeField]
	private float negativeActivationThreshold = -3f;

	[SerializeField]
	private float negativeDeactivationThreshold = -1f;

	private KeyPosition keyPosition;

	private bool isMainWebAttached;

	private string keyName;

	private static MaterialPropertyBlock mpb;

	private static readonly int activeColorId = Shader.PropertyToID("_ActiveColor");

	private static readonly int backgroundColorId = Shader.PropertyToID("_BackgroundColor");

	private static readonly int progressId = Shader.PropertyToID("_Progress");

	public KeyColor GetKeyColor => keyColor;

	public string KeyName => keyName;

	private void Awake()
	{
		if (mpb == null)
		{
			mpb = new MaterialPropertyBlock();
		}
	}

	private void Start()
	{
		keyPosition = KeyPosition.Middle;
		keyName = base.name.Split('_').Last();
	}

	private void Update()
	{
		float x = base.transform.localRotation.eulerAngles.x;
		x = ((x > 180f) ? (x - 360f) : x);
		switch (keyPosition)
		{
		case KeyPosition.Middle:
			if (x > positiveActivationThreshold)
			{
				keyPosition = KeyPosition.Up;
				PressKey();
			}
			else if (x < negativeActivationThreshold)
			{
				keyPosition = KeyPosition.Down;
				PressKey();
			}
			break;
		case KeyPosition.Down:
			if (x > negativeDeactivationThreshold)
			{
				keyPosition = KeyPosition.Middle;
				ReleaseKey();
			}
			break;
		case KeyPosition.Up:
			if (x < positiveDeactivationThreshold)
			{
				keyPosition = KeyPosition.Middle;
				ReleaseKey();
			}
			break;
		case KeyPosition.MainWeb:
			if (!isMainWebAttached && x > negativeDeactivationThreshold && x < positiveDeactivationThreshold)
			{
				keyPosition = KeyPosition.Middle;
				ReleaseKey();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private IEnumerator TutorialProgressCoroutine(float duration)
	{
		float progress = 1f;
		meshRenderer.GetPropertyBlock(mpb);
		mpb.SetFloat(progressId, progress);
		meshRenderer.SetPropertyBlock(mpb);
		do
		{
			progress -= Time.deltaTime / duration;
			yield return null;
			meshRenderer.GetPropertyBlock(mpb);
			mpb.SetFloat(progressId, progress);
			meshRenderer.SetPropertyBlock(mpb);
		}
		while (progress > 0f);
	}

	private void PressKey()
	{
		grandPiano.PressKey(keyIndex);
	}

	private void ReleaseKey()
	{
		grandPiano.ReleaseKey(keyIndex);
	}

	private bool HasPersistent(UnityEvent unityEvent, UnityEngine.Object target, string methodName)
	{
		for (int i = 0; i < unityEvent.GetPersistentEventCount(); i++)
		{
			if (unityEvent.GetPersistentTarget(i) == target && unityEvent.GetPersistentMethodName(i) == methodName)
			{
				return true;
			}
		}
		return false;
	}

	public void SetKeyMaterial(Material material)
	{
		meshRenderer.sharedMaterial = material;
	}

	public void MainWebAttached()
	{
		keyPosition = KeyPosition.MainWeb;
		isMainWebAttached = true;
		PressKey();
	}

	public void StartTutorialProgress(float duration)
	{
		StartCoroutine(TutorialProgressCoroutine(duration));
	}

	public void SetActiveColor(Color color)
	{
		meshRenderer.GetPropertyBlock(mpb);
		mpb.SetColor(activeColorId, color);
		meshRenderer.SetPropertyBlock(mpb);
	}

	public void SetBackgroundColor(Color color)
	{
		meshRenderer.GetPropertyBlock(mpb);
		mpb.SetColor(backgroundColorId, color);
		meshRenderer.SetPropertyBlock(mpb);
	}

	public void MainWebReleased()
	{
		isMainWebAttached = false;
		ReleaseKey();
	}
}
