using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.Web;

namespace _Scripts.Office;

public class WebParticles : MonoBehaviour
{
	[SerializeField]
	private WebThread webThread;

	[FormerlySerializedAs("dustParticleSystemTransform")]
	[SerializeField]
	private Transform particleTransform;

	private void Awake()
	{
		if (webThread != null)
		{
			webThread.OnWebUpdated += WebThread_OnWebUpdated;
		}
	}

	private void OnDestroy()
	{
		if (webThread != null)
		{
			webThread.OnWebUpdated -= WebThread_OnWebUpdated;
		}
	}

	private void WebThread_OnWebUpdated(object sender, WebThread.OnWebUpdatedEventArgs e)
	{
		particleTransform.localScale = new Vector3(1f, 1f, e.length);
	}
}
