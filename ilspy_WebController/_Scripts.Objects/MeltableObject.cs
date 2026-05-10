using System;
using UnityEngine;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

[RequireComponent(typeof(MovableObject))]
[DisallowMultipleComponent]
public class MeltableObject : MonoBehaviour, IInitializable<MeltableObjectSaveData>, IHasSaveData<MeltableObjectSaveData>
{
	public class OnSizeChangedEventArgs : EventArgs
	{
		public float Size;
	}

	public EventHandler<OnSizeChangedEventArgs> OnSizeChangedEvent;

	[Range(0f, 1f)]
	[SerializeField]
	private float initialMeltAmount;

	[SerializeField]
	private float meltDuration = 10f;

	private MovableObject movableObject;

	private float meltAmount;

	private Vector3 initialScale;

	private bool isMelted;

	public bool IsMelted => isMelted;

	public float MeltAmount => meltAmount;

	private void Awake()
	{
		movableObject = GetComponent<MovableObject>();
	}

	private void Start()
	{
		meltAmount = initialMeltAmount;
		initialScale = base.transform.localScale;
		if (meltAmount >= 1f)
		{
			isMelted = true;
			return;
		}
		isMelted = false;
		UpdateSize();
	}

	private void LateUpdate()
	{
		if (IsMelted)
		{
			movableObject.DestroySafely();
		}
	}

	private void UpdateSize()
	{
		Vector3 localScale = initialScale * (1f - meltAmount);
		base.transform.localScale = localScale;
		OnSizeChangedEvent?.Invoke(this, new OnSizeChangedEventArgs
		{
			Size = localScale.x
		});
	}

	public void Initialize(MeltableObjectSaveData saveData)
	{
		initialMeltAmount = saveData.meltAmount;
		Start();
	}

	public MeltableObjectSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Meltable Object " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		MeltableObjectSaveData result = default(MeltableObjectSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.meltAmount = meltAmount;
		return result;
	}

	public void SetInitialScale(Vector3 newInitializeScale)
	{
		initialScale = newInitializeScale;
	}

	public void AddMeltAmount(float value)
	{
		if (!isMelted)
		{
			meltAmount = Mathf.Clamp01(meltAmount + value / meltDuration);
			if (meltAmount >= 1f)
			{
				isMelted = true;
			}
			else
			{
				UpdateSize();
			}
		}
	}
}
