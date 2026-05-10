using UnityEngine;

namespace SplineMesh;

public struct CurveSample
{
	public readonly Vector3 location;

	public readonly Vector3 tangent;

	public readonly Vector3 up;

	public readonly Vector2 scale;

	public readonly float roll;

	public readonly float distanceInCurve;

	public readonly float timeInCurve;

	public readonly CubicBezierCurve curve;

	private Quaternion rotation;

	public Quaternion Rotation
	{
		get
		{
			if (rotation == Quaternion.identity)
			{
				Vector3 upwards = Vector3.Cross(tangent, Vector3.Cross(Quaternion.AngleAxis(roll, Vector3.forward) * up, tangent).normalized);
				rotation = Quaternion.LookRotation(tangent, upwards);
			}
			return rotation;
		}
	}

	public CurveSample(Vector3 location, Vector3 tangent, Vector3 up, Vector2 scale, float roll, float distanceInCurve, float timeInCurve, CubicBezierCurve curve)
	{
		this.location = location;
		this.tangent = tangent;
		this.up = up;
		this.roll = roll;
		this.scale = scale;
		this.distanceInCurve = distanceInCurve;
		this.timeInCurve = timeInCurve;
		this.curve = curve;
		rotation = Quaternion.identity;
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		CurveSample curveSample = (CurveSample)obj;
		if (location == curveSample.location && tangent == curveSample.tangent && up == curveSample.up && scale == curveSample.scale && roll == curveSample.roll && distanceInCurve == curveSample.distanceInCurve)
		{
			return timeInCurve == curveSample.timeInCurve;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(CurveSample cs1, CurveSample cs2)
	{
		return cs1.Equals(cs2);
	}

	public static bool operator !=(CurveSample cs1, CurveSample cs2)
	{
		return !cs1.Equals(cs2);
	}

	public static CurveSample Lerp(CurveSample a, CurveSample b, float t)
	{
		return new CurveSample(Vector3.Lerp(a.location, b.location, t), Vector3.Lerp(a.tangent, b.tangent, t).normalized, Vector3.Lerp(a.up, b.up, t), Vector2.Lerp(a.scale, b.scale, t), Mathf.Lerp(a.roll, b.roll, t), Mathf.Lerp(a.distanceInCurve, b.distanceInCurve, t), Mathf.Lerp(a.timeInCurve, b.timeInCurve, t), a.curve);
	}

	public MeshVertex GetBent(MeshVertex vert)
	{
		MeshVertex meshVertex = new MeshVertex(vert.position, vert.normal, vert.uv);
		meshVertex.position = Vector3.Scale(meshVertex.position, new Vector3(0f, scale.y, scale.x));
		meshVertex.position = Quaternion.AngleAxis(roll, Vector3.right) * meshVertex.position;
		meshVertex.normal = Quaternion.AngleAxis(roll, Vector3.right) * meshVertex.normal;
		meshVertex.position.x = 0f;
		Quaternion quaternion = Rotation * Quaternion.Euler(0f, -90f, 0f);
		meshVertex.position = quaternion * meshVertex.position + location;
		meshVertex.normal = quaternion * meshVertex.normal;
		return meshVertex;
	}
}
