using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Selections;

public class SelectOnEnable : MonoBehaviour
{
	private Selectable selectable;

	private void Awake()
	{
		selectable = GetComponentInChildren<Selectable>();
	}

	private void OnEnable()
	{
		StartCoroutine(OnEnableCoroutine());
	}

	private void Update()
	{
	}

	private IEnumerator OnEnableCoroutine()
	{
		yield return null;
		selectable.Select();
		selectable.OnSelect(null);
	}
}
