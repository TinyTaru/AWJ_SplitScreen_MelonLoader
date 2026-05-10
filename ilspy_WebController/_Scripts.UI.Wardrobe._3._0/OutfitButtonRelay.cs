using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Wardrobe._3._0;

public class OutfitButtonRelay : MonoBehaviour, ISelectHandler, IEventSystemHandler
{
	[SerializeField]
	private OutfitButton parent;

	public void OnSelect(BaseEventData eventData)
	{
		if (parent == null)
		{
			parent = GetComponentInParent<OutfitButton>();
		}
		if (parent != null)
		{
			parent.OnOutfitButtonSelected();
		}
	}
}
