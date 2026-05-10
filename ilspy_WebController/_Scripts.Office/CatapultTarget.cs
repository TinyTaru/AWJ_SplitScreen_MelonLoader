using System;
using UnityEngine;
using _Scripts.Objects;

namespace _Scripts.Office;

public class CatapultTarget : MonoBehaviour
{
	[SerializeField]
	private float startSize = 2f;

	[SerializeField]
	private float endSize = 0.5f;

	[SerializeField]
	private float minShrinkDuration = 10f;

	[SerializeField]
	private float maxShrinkDuration = 15f;

	private float shrinkRate;

	private float size;

	private MovableObject movableObject;

	private bool isShrinking;

	public event EventHandler OnTargetHit;

	public event EventHandler OnDeactivate;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
	}

	private void Start()
	{
		movableObject.OnForwardCollisionCheck += MovableObject_OnForwardCollisionCheck;
	}

	private void OnEnable()
	{
		isShrinking = false;
		base.transform.localScale = Vector3.one * startSize;
		float num = UnityEngine.Random.Range(minShrinkDuration, maxShrinkDuration);
		shrinkRate = (endSize - startSize) / num;
		size = startSize;
	}

	private void Update()
	{
		if (isShrinking)
		{
			size += shrinkRate * Time.deltaTime;
			base.transform.localScale = Vector3.one * size;
			if (size < endSize)
			{
				Deactivate();
			}
		}
	}

	public void Activate()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Deactivate()
	{
		if (movableObject == null)
		{
			movableObject = GetComponent<MovableObject>();
		}
		movableObject.DisableSafely();
		this.OnDeactivate?.Invoke(this, EventArgs.Empty);
	}

	private void MovableObject_OnForwardCollisionCheck(object sender, MovableObject.OnForwardCollisionCheckEventArgs e)
	{
		NerfGunBullet componentInParent = e.other.gameObject.GetComponentInParent<NerfGunBullet>();
		if (componentInParent != null && !componentInParent.TargetHit)
		{
			componentInParent.hitTarget();
			isShrinking = true;
			this.OnTargetHit?.Invoke(this, EventArgs.Empty);
		}
	}
}
