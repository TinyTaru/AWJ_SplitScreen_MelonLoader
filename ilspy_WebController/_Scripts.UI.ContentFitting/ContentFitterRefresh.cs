using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ContentFitting;

public class ContentFitterRefresh : MonoBehaviour
{
	private void Awake()
	{
		RefreshContentFitters();
	}

	private void RefreshContentFitter(RectTransform transform)
	{
		if (transform == null || !transform.gameObject.activeSelf)
		{
			return;
		}
		foreach (RectTransform item in transform)
		{
			RefreshContentFitter(item);
		}
		LayoutGroup component = transform.GetComponent<LayoutGroup>();
		ContentSizeFitter component2 = transform.GetComponent<ContentSizeFitter>();
		if (component != null)
		{
			component.SetLayoutHorizontal();
			component.SetLayoutVertical();
		}
		if (component2 != null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
		}
	}

	public void RefreshContentFitters()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		RefreshContentFitter(rectTransform);
	}
}
