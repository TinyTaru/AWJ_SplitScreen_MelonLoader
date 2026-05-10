using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;

namespace SplineMesh;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class Spline : MonoBehaviour
{
	public List<SplineNode> nodes = new List<SplineNode>();

	[HideInInspector]
	public List<CubicBezierCurve> curves = new List<CubicBezierCurve>();

	public float Length;

	[SerializeField]
	private bool isLoop;

	[HideInInspector]
	public UnityEvent CurveChanged = new UnityEvent();

	private SplineNode start;

	private SplineNode end;

	public bool IsLoop
	{
		get
		{
			return isLoop;
		}
		set
		{
			isLoop = value;
			updateLoopBinding();
		}
	}

	public event ListChangeHandler<SplineNode> NodeListChanged;

	private void Reset()
	{
		nodes.Clear();
		curves.Clear();
		AddNode(new SplineNode(new Vector3(5f, 0f, 0f), new Vector3(5f, 0f, -3f)));
		AddNode(new SplineNode(new Vector3(10f, 0f, 0f), new Vector3(10f, 0f, 3f)));
		RaiseNodeListChanged(new ListChangedEventArgs<SplineNode>
		{
			type = ListChangeType.clear
		});
		UpdateAfterCurveChanged();
	}

	private void OnEnable()
	{
		RefreshCurves();
	}

	public ReadOnlyCollection<CubicBezierCurve> GetCurves()
	{
		return curves.AsReadOnly();
	}

	private void RaiseNodeListChanged(ListChangedEventArgs<SplineNode> args)
	{
		if (this.NodeListChanged != null)
		{
			this.NodeListChanged(this, args);
		}
	}

	private void UpdateAfterCurveChanged()
	{
		Length = 0f;
		foreach (CubicBezierCurve curf in curves)
		{
			Length += curf.Length;
		}
		CurveChanged.Invoke();
	}

	public CurveSample GetSample(float t)
	{
		int nodeIndexForTime = GetNodeIndexForTime(t);
		return curves[nodeIndexForTime].GetSample(t - (float)nodeIndexForTime);
	}

	public CubicBezierCurve GetCurve(float t)
	{
		return curves[GetNodeIndexForTime(t)];
	}

	private int GetNodeIndexForTime(float t)
	{
		if (t < 0f || t > (float)(nodes.Count - 1))
		{
			throw new ArgumentException($"Time must be between 0 and last node index ({nodes.Count - 1}). Given time was {t}.");
		}
		int num = Mathf.FloorToInt(t);
		if (num == nodes.Count - 1)
		{
			num--;
		}
		return num;
	}

	public void RefreshCurves()
	{
		curves.Clear();
		for (int i = 0; i < nodes.Count - 1; i++)
		{
			SplineNode n = nodes[i];
			SplineNode n2 = nodes[i + 1];
			CubicBezierCurve cubicBezierCurve = new CubicBezierCurve(n, n2);
			cubicBezierCurve.Changed.AddListener(UpdateAfterCurveChanged);
			curves.Add(cubicBezierCurve);
		}
		RaiseNodeListChanged(new ListChangedEventArgs<SplineNode>
		{
			type = ListChangeType.clear
		});
		UpdateAfterCurveChanged();
	}

	public CurveSample GetSampleAtDistance(float d)
	{
		if (d < 0f || d > Length)
		{
			throw new ArgumentException($"Distance must be between 0 and spline length ({Length}). Given distance was {d}.");
		}
		foreach (CubicBezierCurve curf in curves)
		{
			if (d > curf.Length && d < curf.Length + 0.0001f)
			{
				d = curf.Length;
			}
			if (d > curf.Length)
			{
				d -= curf.Length;
				continue;
			}
			return curf.GetSampleAtDistance(d);
		}
		throw new Exception("Something went wrong with GetSampleAtDistance.");
	}

	public void AddNode(SplineNode node)
	{
		nodes.Add(node);
		if (nodes.Count != 1)
		{
			CubicBezierCurve cubicBezierCurve = new CubicBezierCurve(nodes[nodes.IndexOf(node) - 1], node);
			cubicBezierCurve.Changed.AddListener(UpdateAfterCurveChanged);
			curves.Add(cubicBezierCurve);
		}
		RaiseNodeListChanged(new ListChangedEventArgs<SplineNode>
		{
			type = ListChangeType.Add,
			newItems = new List<SplineNode> { node }
		});
		UpdateAfterCurveChanged();
		updateLoopBinding();
	}

	public void InsertNode(int index, SplineNode node)
	{
		if (index == 0)
		{
			throw new Exception("Can't insert a node at index 0");
		}
		_ = nodes[index - 1];
		SplineNode n = nodes[index];
		nodes.Insert(index, node);
		curves[index - 1].ConnectEnd(node);
		CubicBezierCurve cubicBezierCurve = new CubicBezierCurve(node, n);
		cubicBezierCurve.Changed.AddListener(UpdateAfterCurveChanged);
		curves.Insert(index, cubicBezierCurve);
		RaiseNodeListChanged(new ListChangedEventArgs<SplineNode>
		{
			type = ListChangeType.Insert,
			newItems = new List<SplineNode> { node },
			insertIndex = index
		});
		UpdateAfterCurveChanged();
		updateLoopBinding();
	}

	public void RemoveNode(SplineNode node)
	{
		int num = nodes.IndexOf(node);
		if (nodes.Count <= 2)
		{
			throw new Exception("Can't remove the node because a spline needs at least 2 nodes.");
		}
		CubicBezierCurve cubicBezierCurve = ((num == nodes.Count - 1) ? curves[num - 1] : curves[num]);
		if (num != 0 && num != nodes.Count - 1)
		{
			SplineNode n = nodes[num + 1];
			curves[num - 1].ConnectEnd(n);
		}
		nodes.RemoveAt(num);
		cubicBezierCurve.Changed.RemoveListener(UpdateAfterCurveChanged);
		curves.Remove(cubicBezierCurve);
		RaiseNodeListChanged(new ListChangedEventArgs<SplineNode>
		{
			type = ListChangeType.Remove,
			removedItems = new List<SplineNode> { node },
			removeIndex = num
		});
		UpdateAfterCurveChanged();
		updateLoopBinding();
	}

	private void updateLoopBinding()
	{
		if (start != null)
		{
			start.Changed -= StartNodeChanged;
		}
		if (end != null)
		{
			end.Changed -= EndNodeChanged;
		}
		if (isLoop)
		{
			start = nodes[0];
			end = nodes[nodes.Count - 1];
			start.Changed += StartNodeChanged;
			end.Changed += EndNodeChanged;
			StartNodeChanged(null, null);
		}
		else
		{
			start = null;
			end = null;
		}
	}

	private void StartNodeChanged(object sender, EventArgs e)
	{
		end.Changed -= EndNodeChanged;
		end.Position = start.Position;
		end.Direction = start.Direction;
		end.Roll = start.Roll;
		end.Scale = start.Scale;
		end.Up = start.Up;
		end.Changed += EndNodeChanged;
	}

	private void EndNodeChanged(object sender, EventArgs e)
	{
		start.Changed -= StartNodeChanged;
		start.Position = end.Position;
		start.Direction = end.Direction;
		start.Roll = end.Roll;
		start.Scale = end.Scale;
		start.Up = end.Up;
		start.Changed += StartNodeChanged;
	}

	public CurveSample GetProjectionSample(Vector3 pointToProject)
	{
		CurveSample result = default(CurveSample);
		float num = float.MaxValue;
		foreach (CubicBezierCurve curf in curves)
		{
			CurveSample projectionSample = curf.GetProjectionSample(pointToProject);
			if (curf == curves[0])
			{
				result = projectionSample;
				num = (projectionSample.location - pointToProject).sqrMagnitude;
				continue;
			}
			float sqrMagnitude = (projectionSample.location - pointToProject).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = projectionSample;
			}
		}
		return result;
	}
}
