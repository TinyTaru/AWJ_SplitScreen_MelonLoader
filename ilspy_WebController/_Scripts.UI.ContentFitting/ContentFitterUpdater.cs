using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ContentFitting;

public class ContentFitterUpdater : MonoBehaviour
{
	[SerializeField]
	private RectTransform contenfitter;

	private void OnRectTransformDimensionsChange()
	{
		StartCoroutine(MyCor());
	}

	private IEnumerator MyCor()
	{
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(contenfitter);
	}
}
