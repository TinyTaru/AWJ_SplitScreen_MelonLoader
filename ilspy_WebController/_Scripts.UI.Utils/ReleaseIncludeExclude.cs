using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.UI.Utils;

[DisallowMultipleComponent]
public class ReleaseIncludeExclude : MonoBehaviour
{
	[Header("Includes")]
	[SerializeField]
	private List<GameObject> includeInSteamRelease;

	[SerializeField]
	private List<GameObject> includeInSteamDemo;

	[FormerlySerializedAs("includeInSteamSupporter")]
	[SerializeField]
	private List<GameObject> includeInSteamPlaytest;

	[SerializeField]
	private List<GameObject> includeInEvent;

	[SerializeField]
	private List<GameObject> includeInMobile;

	[Header("Excludes")]
	[FormerlySerializedAs("excludeInMobile")]
	[FormerlySerializedAs("excludeOnMobile")]
	[SerializeField]
	private List<GameObject> excludeFromMobile;

	private void Start()
	{
		DisableAllOptions();
		foreach (GameObject item in includeInSteamRelease)
		{
			if (!(item == null))
			{
				item.SetActive(value: true);
			}
		}
	}

	private void DisableAllOptions()
	{
		foreach (GameObject item in includeInSteamRelease.Union(includeInSteamDemo).ToList().Union(includeInSteamPlaytest)
			.ToList()
			.Union(includeInMobile)
			.ToList()
			.Union(includeInEvent)
			.ToList())
		{
			if (!(item == null))
			{
				item.SetActive(value: false);
			}
		}
	}
}
