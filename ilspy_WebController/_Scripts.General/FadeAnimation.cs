using UnityEngine;
using UnityEngine.Events;

namespace _Scripts.General;

public class FadeAnimation : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private UnityEvent onFadeOutFinished;

	[SerializeField]
	private UnityEvent onFadeInFinished;

	public void StartFadeOutAnimation()
	{
		animator.SetTrigger("FadeOut");
	}

	public void FadeOutFinished()
	{
		onFadeOutFinished.Invoke();
	}

	public void StartFadeInAnimation()
	{
		animator.SetTrigger("FadeIn");
	}

	public void FadeInFinished()
	{
		onFadeInFinished.Invoke();
	}
}
