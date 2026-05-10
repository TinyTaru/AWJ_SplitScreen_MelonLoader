using MPUIKIT;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Utils;

public class ChangeTextOnSelect : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private Color textActive;

	[SerializeField]
	private Color textIdle;

	[SerializeField]
	private TextMeshProUGUI[] labels;

	[SerializeField]
	private MPImage[] images;

	private void OnEnable()
	{
		if (!(EventSystem.current.currentSelectedGameObject == base.gameObject))
		{
			for (int i = 0; i < labels.Length; i++)
			{
				labels[i].color = textIdle;
			}
			for (int j = 0; j < images.Length; j++)
			{
				images[j].color = textIdle;
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].color = textActive;
		}
		for (int j = 0; j < images.Length; j++)
		{
			images[j].color = textActive;
		}
	}

	public void ChangeSelectedItems()
	{
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].color = textActive;
		}
		for (int j = 0; j < images.Length; j++)
		{
			images[j].color = textActive;
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		for (int i = 0; i < labels.Length; i++)
		{
			labels[i].color = textIdle;
		}
		for (int j = 0; j < images.Length; j++)
		{
			images[j].color = textIdle;
		}
	}
}
