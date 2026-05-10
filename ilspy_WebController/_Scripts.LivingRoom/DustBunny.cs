using System;
using DG.Tweening;
using UnityEngine;
using _Scripts.Objects;

namespace _Scripts.LivingRoom;

[RequireComponent(typeof(MovableObject))]
public class DustBunny : MonoBehaviour
{
	private MovableObject movableObject;

	private bool isCleaned;

	public bool IsCleaned => isCleaned;

	public event Action OnCleanedEvent;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
	}

	public void Clean(Transform target)
	{
		if (!isCleaned)
		{
			isCleaned = true;
			movableObject.SetColliderActive(value: false);
			movableObject.GetRigidbody().isKinematic = true;
			DOTween.Sequence().Join(base.transform.DOScale(0f, 0.5f)).Join(base.transform.DOMove(target.position, 0.5f))
				.OnComplete(delegate
				{
					movableObject.DestroySafely();
					this.OnCleanedEvent?.Invoke();
				});
		}
	}
}
