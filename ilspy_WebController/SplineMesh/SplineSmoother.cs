using System;
using UnityEngine;

namespace SplineMesh;

[DisallowMultipleComponent]
[ExecuteInEditMode]
[RequireComponent(typeof(Spline))]
public class SplineSmoother : MonoBehaviour
{
	private Spline spline;

	[Range(0f, 1f)]
	public float curvature = 0.3f;

	private Spline Spline
	{
		get
		{
			if (spline == null)
			{
				spline = GetComponent<Spline>();
			}
			return spline;
		}
	}

	private void OnValidate()
	{
		SmoothAll();
	}

	private void OnEnable()
	{
		Spline.NodeListChanged += Spline_NodeListChanged;
		foreach (SplineNode node in Spline.nodes)
		{
			node.Changed += OnNodeChanged;
		}
		SmoothAll();
	}

	private void OnDisable()
	{
		Spline.NodeListChanged -= Spline_NodeListChanged;
		foreach (SplineNode node in Spline.nodes)
		{
			node.Changed -= OnNodeChanged;
		}
	}

	private void Spline_NodeListChanged(object sender, ListChangedEventArgs<SplineNode> args)
	{
		if (args.newItems != null)
		{
			foreach (SplineNode newItem in args.newItems)
			{
				newItem.Changed += OnNodeChanged;
			}
		}
		if (args.removedItems == null)
		{
			return;
		}
		foreach (SplineNode removedItem in args.removedItems)
		{
			removedItem.Changed -= OnNodeChanged;
		}
	}

	private void OnNodeChanged(object sender, EventArgs e)
	{
		SplineNode splineNode = (SplineNode)sender;
		SmoothNode(splineNode);
		int num = Spline.nodes.IndexOf(splineNode);
		if (num > 0)
		{
			SmoothNode(Spline.nodes[num - 1]);
		}
		if (num < Spline.nodes.Count - 1)
		{
			SmoothNode(Spline.nodes[num + 1]);
		}
	}

	private void SmoothNode(SplineNode node)
	{
		int num = Spline.nodes.IndexOf(node);
		Vector3 position = node.Position;
		Vector3 zero = Vector3.zero;
		float num2 = 0f;
		if (num != 0)
		{
			Vector3 position2 = Spline.nodes[num - 1].Position;
			Vector3 vector = position - position2;
			num2 += vector.magnitude;
			zero += vector.normalized;
		}
		if (num != Spline.nodes.Count - 1)
		{
			Vector3 position3 = Spline.nodes[num + 1].Position;
			Vector3 vector2 = position - position3;
			num2 += vector2.magnitude;
			zero -= vector2.normalized;
		}
		num2 *= 0.5f;
		zero = zero.normalized * num2 * curvature;
		Vector3 direction = zero + position;
		node.Direction = direction;
	}

	private void SmoothAll()
	{
		foreach (SplineNode node in Spline.nodes)
		{
			SmoothNode(node);
		}
	}
}
