using Sirenix.Utilities;
using UnityEngine;

namespace _Scripts.UI.Credits;

[CreateAssetMenu(fileName = "New SupporterSO", menuName = "ScriptableObjects/SupporterSO")]
public class SupporterSO : ScriptableObject
{
	public string[] cobwebConnoisseurs;

	public string[] cozyCrawlers;

	public string[] itsyBitsySupporters;

	public string[] caffeinatedCrawlers;

	private void SortAlphabetically()
	{
		cobwebConnoisseurs.Sort();
		cozyCrawlers.Sort();
		itsyBitsySupporters.Sort();
		caffeinatedCrawlers.Sort();
	}
}
