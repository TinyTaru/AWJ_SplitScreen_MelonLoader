using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SplineMesh;

[Serializable]
public class CubicBezierCurve
{
	private const int STEP_COUNT = 30;

	private const float T_STEP = 1f / 30f;

	private readonly List<CurveSample> samples = new List<CurveSample>(30);

	public SplineNode n1;

	public SplineNode n2;

	public UnityEvent Changed = new UnityEvent();

	public float Length { get; private set; }

	public CubicBezierCurve(SplineNode n1, SplineNode n2)
	{
		this.n1 = n1;
		this.n2 = n2;
		n1.Changed += ComputeSamples;
		n2.Changed += ComputeSamples;
		ComputeSamples(null, null);
	}

	public void ConnectStart(SplineNode n1)
	{
		this.n1.Changed -= ComputeSamples;
		this.n1 = n1;
		n1.Changed += ComputeSamples;
		ComputeSamples(null, null);
	}

	public void ConnectEnd(SplineNode n2)
	{
		this.n2.Changed -= ComputeSamples;
		this.n2 = n2;
		n2.Changed += ComputeSamples;
		ComputeSamples(null, null);
	}

	public Vector3 GetInverseDirection()
	{
		return 2f * n2.Position - n2.Direction;
	}

	private Vector3 GetLocation(float t)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = t * t;
		return n1.Position * (num2 * num) + n1.Direction * (3f * num2 * t) + GetInverseDirection() * (3f * num * num3) + n2.Position * (num3 * t);
	}

	private Vector3 GetTangent(float t)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = t * t;
		return (n1.Position * (0f - num2) + n1.Direction * (3f * num2 - 2f * num) + GetInverseDirection() * (-3f * num3 + 2f * t) + n2.Position * num3).normalized;
	}

	private Vector3 GetUp(float t)
	{
		return Vector3.Lerp(n1.Up, n2.Up, t);
	}

	private Vector2 GetScale(float t)
	{
		return Vector2.Lerp(n1.Scale, n2.Scale, t);
	}

	private float GetRoll(float t)
	{
		return Mathf.Lerp(n1.Roll, n2.Roll, t);
	}

	private void ComputeSamples(object sender, EventArgs e)
	{
		samples.Clear();
		Length = 0f;
		Vector3 a = GetLocation(0f);
		for (float num = 0f; num < 1f; num += 1f / 30f)
		{
			Vector3 location = GetLocation(num);
			Length += Vector3.Distance(a, location);
			a = location;
			samples.Add(CreateSample(Length, num));
		}
		Length += Vector3.Distance(a, GetLocation(1f));
		samples.Add(CreateSample(Length, 1f));
		if (Changed != null)
		{
			Changed.Invoke();
		}
	}

	private CurveSample CreateSample(float distance, float time)
	{
		return new CurveSample(GetLocation(time), GetTangent(time), GetUp(time), GetScale(time), GetRoll(time), distance, time, this);
	}

	public CurveSample GetSample(float time)
	{
		AssertTimeInBounds(time);
		CurveSample curveSample = samples[0];
		CurveSample curveSample2 = default(CurveSample);
		bool flag = false;
		foreach (CurveSample sample in samples)
		{
			if (sample.timeInCurve >= time)
			{
				curveSample2 = sample;
				flag = true;
				break;
			}
			curveSample = sample;
		}
		if (!flag)
		{
			throw new Exception("Can't find curve samples.");
		}
		float t = ((curveSample2 == curveSample) ? 0f : ((time - curveSample.timeInCurve) / (curveSample2.timeInCurve - curveSample.timeInCurve)));
		return CurveSample.Lerp(curveSample, curveSample2, t);
	}

	public CurveSample GetSampleAtDistance(float d)
	{
		if (d < 0f || d > Length)
		{
			throw new ArgumentException("Distance must be positive and less than curve length. Length = " + Length + ", given distance was " + d);
		}
		CurveSample curveSample = samples[0];
		CurveSample curveSample2 = default(CurveSample);
		bool flag = false;
		foreach (CurveSample sample in samples)
		{
			if (sample.distanceInCurve >= d)
			{
				curveSample2 = sample;
				flag = true;
				break;
			}
			curveSample = sample;
		}
		if (!flag)
		{
			throw new Exception("Can't find curve samples.");
		}
		float t = ((curveSample2 == curveSample) ? 0f : ((d - curveSample.distanceInCurve) / (curveSample2.distanceInCurve - curveSample.distanceInCurve)));
		return CurveSample.Lerp(curveSample, curveSample2, t);
	}

	private static void AssertTimeInBounds(float time)
	{
		if (time < 0f || time > 1f)
		{
			throw new ArgumentException("Time must be between 0 and 1 (was " + time + ").");
		}
	}

	public CurveSample GetProjectionSample(Vector3 pointToProject)
	{
		float num = float.PositiveInfinity;
		int num2 = -1;
		int num3 = 0;
		foreach (CurveSample sample in samples)
		{
			float sqrMagnitude = (sample.location - pointToProject).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				num2 = num3;
			}
			num3++;
		}
		CurveSample a;
		CurveSample b;
		if (num2 == 0)
		{
			a = samples[num2];
			b = samples[num2 + 1];
		}
		else if (num2 == samples.Count - 1)
		{
			a = samples[num2 - 1];
			b = samples[num2];
		}
		else
		{
			float sqrMagnitude2 = (pointToProject - samples[num2 - 1].location).sqrMagnitude;
			float sqrMagnitude3 = (pointToProject - samples[num2 + 1].location).sqrMagnitude;
			if (sqrMagnitude2 < sqrMagnitude3)
			{
				a = samples[num2 - 1];
				b = samples[num2];
			}
			else
			{
				a = samples[num2];
				b = samples[num2 + 1];
			}
		}
		float value = (Vector3.Project(pointToProject - a.location, b.location - a.location) + a.location - a.location).sqrMagnitude / (b.location - a.location).sqrMagnitude;
		value = Mathf.Clamp(value, 0f, 1f);
		return CurveSample.Lerp(a, b, value);
	}
}
