using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Utils;

public class URL : MonoBehaviour
{
	[SerializeField]
	private string url;

	public void OpenURL()
	{
		if (!SettingsController.EventMode)
		{
			Application.OpenURL(url);
		}
	}
}
