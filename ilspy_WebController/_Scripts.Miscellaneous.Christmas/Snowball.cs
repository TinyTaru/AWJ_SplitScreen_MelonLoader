using System;
using UnityEngine;
using _Scripts.LevelSaving;
using _Scripts.Objects;

namespace _Scripts.Miscellaneous.Christmas;

[RequireComponent(typeof(MovableObject))]
public class Snowball : MonoBehaviour, IInitializable<SnowballSaveData>, IHasSaveData<SnowballSaveData>
{
	[Header("References")]
	[SerializeField]
	private Transform sphere;

	[SerializeField]
	private MovableObject movableObject;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private MeltableObject meltableObject;

	[Header("Parameters")]
	[SerializeField]
	private float defaultRadius = 5f;

	[SerializeField]
	private float growRate = 0.1f;

	[SerializeField]
	private float minVelocityToGrow = 1f;

	[SerializeField]
	private float canPushThreshold = 2f;

	private float defaultMass;

	private float size;

	private bool canBePushed;

	public float Size => size;

	public float DefaultMass => defaultMass;

	private void Awake()
	{
		sphere.localScale = Vector3.one * defaultRadius;
		defaultMass = rb.mass;
		size = base.transform.localScale.x;
	}

	private void Start()
	{
		if (size >= canPushThreshold)
		{
			canBePushed = true;
			movableObject.SetCanBePushed(value: false);
			movableObject.SetCollideWithCamera(value: true);
			movableObject.ResetLayer();
		}
		if (meltableObject != null)
		{
			MeltableObject obj = meltableObject;
			obj.OnSizeChangedEvent = (EventHandler<MeltableObject.OnSizeChangedEventArgs>)Delegate.Combine(obj.OnSizeChangedEvent, new EventHandler<MeltableObject.OnSizeChangedEventArgs>(MeltableObject_OnSizeChangedEvent));
		}
	}

	private void FixedUpdate()
	{
		float magnitude = rb.linearVelocity.magnitude;
		if (base.transform.position.y < base.transform.localScale.x * defaultRadius * 0.52f && magnitude > minVelocityToGrow)
		{
			size += growRate * Time.fixedDeltaTime * magnitude;
			base.transform.localScale = Vector3.one * size;
			if (meltableObject != null)
			{
				meltableObject.SetInitialScale(Vector3.one * size);
			}
			UpdateMass();
		}
	}

	private void UpdateMass()
	{
		rb.mass = size * defaultMass;
		if (!canBePushed)
		{
			if (size >= canPushThreshold)
			{
				canBePushed = true;
				movableObject.SetCanBePushed(value: false);
				movableObject.SetCollideWithCamera(value: true);
				movableObject.ResetLayer();
			}
		}
		else if (size < canPushThreshold)
		{
			canBePushed = false;
			movableObject.SetCanBePushed(value: true);
			movableObject.SetCollideWithCamera(value: false);
			movableObject.ResetLayer();
		}
	}

	public void Initialize(SnowballSaveData saveData)
	{
		size = saveData.size;
		defaultMass = saveData.defaultMass;
		base.transform.localScale = Vector3.one * size;
		if (meltableObject != null)
		{
			meltableObject.SetInitialScale(Vector3.one * size);
		}
		UpdateMass();
	}

	public SnowballSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Snowball " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		SnowballSaveData result = default(SnowballSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.size = size;
		result.defaultMass = defaultMass;
		return result;
	}

	public void Initialize(float newSize, float newDefaultMass)
	{
		size = newSize;
		defaultMass = newDefaultMass;
		base.transform.localScale = Vector3.one * size;
		if (meltableObject != null)
		{
			meltableObject.SetInitialScale(Vector3.one * size);
		}
		UpdateMass();
	}

	private void MeltableObject_OnSizeChangedEvent(object sender, MeltableObject.OnSizeChangedEventArgs e)
	{
		size = e.Size;
		UpdateMass();
	}
}
