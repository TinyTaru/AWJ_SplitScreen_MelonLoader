using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SplineMesh;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ExtrusionSegment : MonoBehaviour
{
	[Serializable]
	public class Vertex
	{
		public Vector2 point;

		public Vector2 normal;

		public float uCoord;

		public Vertex(Vector2 point, Vector2 normal, float uCoord)
		{
			this.point = point;
			this.normal = normal;
			this.uCoord = uCoord;
		}

		public Vertex(Vertex other)
		{
			point = other.point;
			normal = other.normal;
			uCoord = other.uCoord;
		}
	}

	private bool isDirty;

	private MeshFilter mf;

	private Mesh result;

	private bool useSpline;

	private CubicBezierCurve curve;

	private Spline spline;

	private float intervalStart;

	private float intervalEnd;

	private List<Vertex> shapeVertices = new List<Vertex>();

	private float textureScale = 1f;

	private float textureOffset;

	private float sampleSpacing = 0.1f;

	public List<Vertex> ShapeVertices
	{
		get
		{
			return shapeVertices;
		}
		set
		{
			if (value != shapeVertices)
			{
				SetDirty();
				shapeVertices = value;
			}
		}
	}

	public float TextureScale
	{
		get
		{
			return textureScale;
		}
		set
		{
			if (value != textureScale)
			{
				SetDirty();
				textureScale = value;
			}
		}
	}

	public float TextureOffset
	{
		get
		{
			return textureOffset;
		}
		set
		{
			if (value != textureOffset)
			{
				SetDirty();
				textureOffset = value;
			}
		}
	}

	public float SampleSpacing
	{
		get
		{
			return sampleSpacing;
		}
		set
		{
			if (value != sampleSpacing)
			{
				if (value <= 0f)
				{
					throw new ArgumentOutOfRangeException("SampleSpacing", "Must be greater than 0");
				}
				SetDirty();
				sampleSpacing = value;
			}
		}
	}

	private void OnEnable()
	{
		mf = GetComponent<MeshFilter>();
		if (mf.sharedMesh == null)
		{
			mf.sharedMesh = new Mesh();
		}
	}

	public void SetInterval(CubicBezierCurve curve)
	{
		if (this.curve != curve)
		{
			if (curve == null)
			{
				throw new ArgumentNullException("curve");
			}
			if (this.curve != null)
			{
				this.curve.Changed.RemoveListener(SetDirty);
			}
			this.curve = curve;
			spline = null;
			curve.Changed.AddListener(SetDirty);
			useSpline = false;
			SetDirty();
		}
	}

	public void SetInterval(Spline spline, float intervalStart, float intervalEnd = 0f)
	{
		if (!(this.spline == spline) || this.intervalStart != intervalStart || this.intervalEnd != intervalEnd)
		{
			if (spline == null)
			{
				throw new ArgumentNullException("spline");
			}
			if (intervalStart < 0f || intervalStart >= spline.Length)
			{
				throw new ArgumentOutOfRangeException("interval start must be 0 or greater and lesser than spline length (was " + intervalStart + ")");
			}
			if ((intervalEnd != 0f && intervalEnd <= intervalStart) || intervalEnd > spline.Length)
			{
				throw new ArgumentOutOfRangeException("interval end must be 0 or greater than interval start, and lesser than spline length (was " + intervalEnd + ")");
			}
			if (this.spline != null)
			{
				this.spline.CurveChanged.RemoveListener(SetDirty);
			}
			this.spline = spline;
			spline.CurveChanged.AddListener(SetDirty);
			curve = null;
			this.intervalStart = intervalStart;
			this.intervalEnd = intervalEnd;
			useSpline = true;
			SetDirty();
		}
	}

	private void SetDirty()
	{
		isDirty = true;
	}

	private void LateUpdate()
	{
		ComputeIfNeeded();
	}

	public void ComputeIfNeeded()
	{
		if (isDirty)
		{
			Compute();
			isDirty = false;
		}
	}

	private List<CurveSample> GetPath()
	{
		List<CurveSample> list = new List<CurveSample>();
		if (useSpline)
		{
			for (float num = intervalStart; num < intervalEnd; num += sampleSpacing)
			{
				list.Add(spline.GetSampleAtDistance(num));
			}
			list.Add(spline.GetSampleAtDistance(intervalEnd));
		}
		else
		{
			for (float num2 = 0f; num2 < curve.Length; num2 += sampleSpacing)
			{
				list.Add(curve.GetSampleAtDistance(num2));
			}
			list.Add(curve.GetSampleAtDistance(curve.Length));
		}
		return list;
	}

	public void Compute()
	{
		List<CurveSample> path = GetPath();
		int count = shapeVertices.Count;
		int num = path.Count - 1;
		List<int> list = new List<int>(count * 2 * num * 3);
		List<MeshVertex> list2 = new List<MeshVertex>(count * 2 * num * 3);
		foreach (CurveSample item5 in path)
		{
			foreach (Vertex shapeVertex in shapeVertices)
			{
				list2.Add(item5.GetBent(new MeshVertex(new Vector3(0f, shapeVertex.point.y, 0f - shapeVertex.point.x), new Vector3(0f, shapeVertex.normal.y, 0f - shapeVertex.normal.x), new Vector2(shapeVertex.uCoord, textureScale * (item5.distanceInCurve + textureOffset)))));
			}
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < shapeVertices.Count; j++)
			{
				int num3 = ((j != shapeVertices.Count - 1) ? 1 : (-(shapeVertices.Count - 1)));
				int item = num2 + shapeVertices.Count;
				int item2 = num2;
				int item3 = num2 + num3;
				int item4 = num2 + num3 + shapeVertices.Count;
				list.Add(item3);
				list.Add(item2);
				list.Add(item);
				list.Add(item);
				list.Add(item4);
				list.Add(item3);
				num2++;
			}
		}
		MeshUtility.Update(mf.sharedMesh, mf.sharedMesh, list, list2.Select((MeshVertex b) => b.position), list2.Select((MeshVertex b) => b.normal), list2.Select((MeshVertex b) => b.uv));
		MeshCollider component = GetComponent<MeshCollider>();
		if (component != null)
		{
			component.sharedMesh = mf.sharedMesh;
		}
	}
}
