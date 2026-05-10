using System;
using UnityEngine;

namespace SplineMesh;

[Serializable]
public class SplineNode
{
	[SerializeField]
	private Vector3 position;

	[SerializeField]
	private Vector3 direction;

	[SerializeField]
	private Vector3 up = Vector3.up;

	[SerializeField]
	private Vector2 scale = Vector2.one;

	[SerializeField]
	private float roll;

	public Vector3 Position
	{
		get
		{
			return position;
		}
		set
		{
			if (!position.Equals(value))
			{
				position.x = value.x;
				position.y = value.y;
				position.z = value.z;
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	public Vector3 Direction
	{
		get
		{
			return direction;
		}
		set
		{
			if (!direction.Equals(value))
			{
				direction.x = value.x;
				direction.y = value.y;
				direction.z = value.z;
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	public Vector3 Up
	{
		get
		{
			return up;
		}
		set
		{
			if (!up.Equals(value))
			{
				up.x = value.x;
				up.y = value.y;
				up.z = value.z;
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	public Vector2 Scale
	{
		get
		{
			return scale;
		}
		set
		{
			if (!scale.Equals(value))
			{
				scale.x = value.x;
				scale.y = value.y;
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	public float Roll
	{
		get
		{
			return roll;
		}
		set
		{
			if (roll != value)
			{
				roll = value;
				if (this.Changed != null)
				{
					this.Changed(this, EventArgs.Empty);
				}
			}
		}
	}

	[HideInInspector]
	public event EventHandler Changed;

	public SplineNode(Vector3 position, Vector3 direction)
	{
		Position = position;
		Direction = direction;
	}
}
