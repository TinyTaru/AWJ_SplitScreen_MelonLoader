using DG.Tweening;
using UnityEngine;

namespace _Scripts.Objects;

public class DuctGate : MonoBehaviour
{
	private bool isOpen;

	public void OpenGate()
	{
		if (!isOpen)
		{
			isOpen = true;
			base.transform.DOLocalMoveY(34f, 1f).SetRelative(isRelative: true).SetEase(Ease.InOutQuad);
		}
	}
}
