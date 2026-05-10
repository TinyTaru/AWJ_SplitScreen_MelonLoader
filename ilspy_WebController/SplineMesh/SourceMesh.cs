using System;
using System.Collections.Generic;
using UnityEngine;

namespace SplineMesh;

public struct SourceMesh
{
	private Vector3 translation;

	private Quaternion rotation;

	private Vector3 scale;

	private List<MeshVertex> vertices;

	private int[] triangles;

	private float minX;

	private float length;

	internal Mesh Mesh { get; }

	internal List<MeshVertex> Vertices
	{
		get
		{
			if (vertices == null)
			{
				BuildData();
			}
			return vertices;
		}
	}

	internal int[] Triangles
	{
		get
		{
			if (vertices == null)
			{
				BuildData();
			}
			return triangles;
		}
	}

	internal float MinX
	{
		get
		{
			if (vertices == null)
			{
				BuildData();
			}
			return minX;
		}
	}

	internal float Length
	{
		get
		{
			if (vertices == null)
			{
				BuildData();
			}
			return length;
		}
	}

	private SourceMesh(Mesh mesh)
	{
		Mesh = mesh;
		translation = default(Vector3);
		rotation = default(Quaternion);
		scale = default(Vector3);
		vertices = null;
		triangles = null;
		minX = 0f;
		length = 0f;
	}

	private SourceMesh(SourceMesh other)
	{
		Mesh = other.Mesh;
		translation = other.translation;
		rotation = other.rotation;
		scale = other.scale;
		vertices = null;
		triangles = null;
		minX = 0f;
		length = 0f;
	}

	public static SourceMesh Build(Mesh mesh)
	{
		return new SourceMesh(mesh);
	}

	public SourceMesh Translate(Vector3 translation)
	{
		SourceMesh result = new SourceMesh(this);
		result.translation = translation;
		return result;
	}

	public SourceMesh Translate(float x, float y, float z)
	{
		return Translate(new Vector3(x, y, z));
	}

	public SourceMesh Rotate(Quaternion rotation)
	{
		SourceMesh result = new SourceMesh(this);
		result.rotation = rotation;
		return result;
	}

	public SourceMesh Scale(Vector3 scale)
	{
		SourceMesh result = new SourceMesh(this);
		result.scale = scale;
		return result;
	}

	public SourceMesh Scale(float x, float y, float z)
	{
		return Scale(new Vector3(x, y, z));
	}

	private void BuildData()
	{
		bool flag = scale.x < 0f;
		if (scale.y < 0f)
		{
			flag = !flag;
		}
		if (scale.z < 0f)
		{
			flag = !flag;
		}
		triangles = (flag ? MeshUtility.GetReversedTriangles(Mesh) : Mesh.triangles);
		int num = 0;
		vertices = new List<MeshVertex>(Mesh.vertexCount);
		Vector3[] array = Mesh.vertices;
		for (int i = 0; i < array.Length; i++)
		{
			MeshVertex meshVertex = new MeshVertex(array[i], Mesh.normals[num++]);
			if (rotation != Quaternion.identity)
			{
				meshVertex.position = rotation * meshVertex.position;
				meshVertex.normal = rotation * meshVertex.normal;
			}
			if (scale != Vector3.one)
			{
				meshVertex.position = Vector3.Scale(meshVertex.position, scale);
				meshVertex.normal = Vector3.Scale(meshVertex.normal, scale);
			}
			if (translation != Vector3.zero)
			{
				meshVertex.position += translation;
			}
			vertices.Add(meshVertex);
		}
		minX = float.MaxValue;
		float num2 = float.MinValue;
		foreach (MeshVertex vertex in vertices)
		{
			Vector3 position = vertex.position;
			num2 = Math.Max(num2, position.x);
			minX = Math.Min(minX, position.x);
		}
		length = Math.Abs(num2 - minX);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		SourceMesh sourceMesh = (SourceMesh)obj;
		if (Mesh == sourceMesh.Mesh && translation == sourceMesh.translation && rotation == sourceMesh.rotation)
		{
			return scale == sourceMesh.scale;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(SourceMesh sm1, SourceMesh sm2)
	{
		return sm1.Equals(sm2);
	}

	public static bool operator !=(SourceMesh sm1, SourceMesh sm2)
	{
		return sm1.Equals(sm2);
	}
}
