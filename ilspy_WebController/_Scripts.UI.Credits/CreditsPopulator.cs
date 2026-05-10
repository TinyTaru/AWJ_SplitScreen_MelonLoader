using TMPro;
using UnityEngine;

namespace _Scripts.UI.Credits;

public class CreditsPopulator : MonoBehaviour
{
	private enum SupporterCategories
	{
		CobwebConnoisseurs,
		CozyCrawlers,
		ItsyBitsySupporters,
		CaffeinatedCrawlers
	}

	[SerializeField]
	private GameObject supporterTextBoxPrefab;

	[SerializeField]
	private Transform layoutGroupLeftColumn;

	[SerializeField]
	private Transform layoutGroupRightColumn;

	[SerializeField]
	private SupporterSO supporterSo;

	[SerializeField]
	[Tooltip("Used to select which array to use from the supporterScriptableObject")]
	private SupporterCategories supporterCategory;

	private void Start()
	{
		PopulateSupporterNames();
	}

	private void PopulateSupporterNames()
	{
		string[] supporterNamesFromSO = GetSupporterNamesFromSO();
		Debug.Log($"Populating {base.gameObject.name} names with {supporterNamesFromSO.Length} names found from SO array");
		if (supporterNamesFromSO == null)
		{
			Debug.LogError($"Supporter names not found for {base.gameObject.name} with supporterCategory set to {supporterCategory}! Check if the enum is correct!");
			return;
		}
		foreach (Transform item in layoutGroupLeftColumn)
		{
			Object.Destroy(item.gameObject);
		}
		foreach (Transform item2 in layoutGroupRightColumn)
		{
			Object.Destroy(item2.gameObject);
		}
		for (int i = 0; i < supporterNamesFromSO.Length; i++)
		{
			GameObject gameObject = ((i >= Mathf.CeilToInt((float)supporterNamesFromSO.Length / 2f)) ? Object.Instantiate(supporterTextBoxPrefab, layoutGroupRightColumn) : Object.Instantiate(supporterTextBoxPrefab, layoutGroupLeftColumn));
			gameObject.GetComponent<TextMeshProUGUI>().text = supporterNamesFromSO[i];
		}
	}

	private string[] GetSupporterNamesFromSO()
	{
		return supporterCategory switch
		{
			SupporterCategories.CaffeinatedCrawlers => supporterSo.caffeinatedCrawlers, 
			SupporterCategories.CozyCrawlers => supporterSo.cozyCrawlers, 
			SupporterCategories.ItsyBitsySupporters => supporterSo.itsyBitsySupporters, 
			SupporterCategories.CobwebConnoisseurs => supporterSo.cobwebConnoisseurs, 
			_ => null, 
		};
	}
}
