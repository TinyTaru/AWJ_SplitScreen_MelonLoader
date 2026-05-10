using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.UI.Utils;

public class TMPLinkHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public void OnPointerClick(PointerEventData eventData)
	{
		TextMeshProUGUI component = GetComponent<TextMeshProUGUI>();
		int num = TMP_TextUtilities.FindIntersectingLink(component, eventData.position, eventData.pressEventCamera);
		if (num == -1)
		{
			return;
		}
		TMP_LinkInfo tMP_LinkInfo = component.textInfo.linkInfo[num];
		string linkID = tMP_LinkInfo.GetLinkID();
		if (!(linkID == "terms-and-conditions"))
		{
			if (linkID == "privacy-policy")
			{
				Application.OpenURL("https://firetotemgames.notion.site/privacy-policy");
			}
		}
		else
		{
			Application.OpenURL("https://firetotemgames.notion.site/terms-and-conditions");
		}
	}
}
