using UnityEngine;

namespace _Scripts.Spider;

public class ThumbnailLegController : MonoBehaviour
{
	[SerializeField]
	private Transform target;

	[SerializeField]
	private Transform targetLocal;

	[SerializeField]
	private float tipHeight = 0.5f;

	[SerializeField]
	private Transform tipTarget;

	private void Update()
	{
		if (!(targetLocal == null) && !(target == null))
		{
			targetLocal.position = tipTarget.position;
			target.position = targetLocal.position + target.up * tipHeight;
			target.rotation = targetLocal.rotation;
		}
	}
}
