using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Animations;

public static class UIAnimation
{
	private static float defaultDuration = 0.5f;

	private static Ease defaultEase = Ease.OutQuint;

	public static Sequence CreateSequence()
	{
		return DOTween.Sequence();
	}

	public static Tweener AnimatePosition(Transform target, Vector3 endValue, float duration = -1f, Ease? ease = null)
	{
		if (duration < 0f)
		{
			duration = defaultDuration;
		}
		if (!ease.HasValue)
		{
			ease = defaultEase;
		}
		return target.DOMove(endValue, duration).SetEase(ease.Value);
	}

	public static Tweener AnimateLocalPosition(Transform target, Vector3 endValue, float duration = -1f, Ease? ease = null)
	{
		if (duration < 0f)
		{
			duration = defaultDuration;
		}
		if (!ease.HasValue)
		{
			ease = defaultEase;
		}
		return target.DOLocalMove(endValue, duration).SetEase(ease.Value);
	}

	public static Tweener AnimateAnchorPosition(RectTransform target, Vector2 endValue, float duration = -1f, Ease? ease = null)
	{
		if (duration < 0f)
		{
			duration = defaultDuration;
		}
		if (!ease.HasValue)
		{
			ease = defaultEase;
		}
		return DOTween.To(() => target.anchoredPosition, delegate(Vector2 x)
		{
			target.anchoredPosition = x;
		}, endValue, duration).SetEase(ease.Value);
	}

	public static Tweener AnimateScale(Transform target, Vector3 endValue, float duration = -1f, Ease? ease = null)
	{
		if (duration < 0f)
		{
			duration = defaultDuration;
		}
		if (!ease.HasValue)
		{
			ease = defaultEase;
		}
		return target.DOScale(endValue, duration).SetEase(ease.Value);
	}

	public static Tweener AnimateFade(CanvasGroup target, float endValue, float duration = -1f, Ease? ease = null)
	{
		if (duration < 0f)
		{
			duration = defaultDuration;
		}
		if (!ease.HasValue)
		{
			ease = defaultEase;
		}
		return target.DOFade(endValue, duration).SetEase(ease.Value);
	}

	public static Tweener AnimateRotation(Transform target, Vector3 endValue, float duration = -1f, Ease? ease = null)
	{
		if (duration < 0f)
		{
			duration = defaultDuration;
		}
		if (!ease.HasValue)
		{
			ease = defaultEase;
		}
		return target.DORotate(endValue, duration).SetEase(ease.Value);
	}

	public static Tweener AnimateFillAmount(Image target, float endValue, float duration = -1f, Ease? ease = null)
	{
		if (duration < 0f)
		{
			duration = defaultDuration;
		}
		if (!ease.HasValue)
		{
			ease = defaultEase;
		}
		return target.DOFillAmount(endValue, duration).SetEase(ease.Value);
	}

	public static Tweener AnimatePunchScale(Transform target, Vector3 punch, float duration = -1f, int vibrato = 10, float elasticity = 1f)
	{
		if (duration < 0f)
		{
			duration = defaultDuration;
		}
		return target.DOPunchScale(punch, duration, vibrato, elasticity).SetEase(defaultEase);
	}

	public static Tweener AnimateQuickScaleIn(Transform target, float duration = 0.2f, Ease? ease = null)
	{
		if (!ease.HasValue)
		{
			ease = Ease.OutQuint;
		}
		return target.DOScale(Vector3.one, duration).SetEase(ease.Value);
	}

	public static Tweener AnimateQuickScaleOut(Transform target, float duration = 0.2f, Ease? ease = null)
	{
		if (!ease.HasValue)
		{
			ease = Ease.OutBack;
		}
		return target.DOScale(Vector3.zero, duration).SetEase(ease.Value);
	}

	public static Tweener AnimateMove(RectTransform target, Vector3 endValue, float duration, Ease ease)
	{
		return target.DOAnchorPos(endValue, duration).SetEase(ease);
	}
}
