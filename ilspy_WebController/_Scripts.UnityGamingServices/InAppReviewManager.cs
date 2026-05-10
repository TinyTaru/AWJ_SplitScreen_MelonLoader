using System.Collections;
using _Scripts.Singletons;

namespace _Scripts.UnityGamingServices;

public class InAppReviewManager : Singleton<InAppReviewManager>
{
	private IEnumerator RequestReviewCoroutine()
	{
		yield break;
	}

	public void RequestReview()
	{
	}
}
