using System;
using UnityEngine;

public class BillboardLineRendererCircle : MonoBehaviour
{
	public Color color = Color.black;

	public float width = 1f;

	public int numSegments = 50;

	public float radius = 0.5f;

	private LineRenderer _lineRenderer;

	private void Start()
	{
		_lineRenderer = base.gameObject.GetComponent<LineRenderer>();
		if (!(_lineRenderer != null))
		{
			_lineRenderer = base.gameObject.AddComponent<LineRenderer>();
			_lineRenderer.materials = new Material[1]
			{
				new Material(Shader.Find("Universal Render Pipeline/Unlit"))
				{
					color = color
				}
			};
			_lineRenderer.startWidth = width * 0.01f;
			_lineRenderer.endWidth = width * 0.01f;
			_lineRenderer.positionCount = numSegments + 1;
			_lineRenderer.useWorldSpace = false;
			float num = MathF.PI * 2f / (float)numSegments;
			float num2 = 0f;
			for (int i = 0; i < numSegments + 1; i++)
			{
				float x = Mathf.Cos(num2);
				float y = Mathf.Sin(num2);
				Vector3 vector = new Vector3(x, y, 0f);
				_lineRenderer.SetPosition(i, vector * radius);
				num2 += num;
			}
		}
	}

	[ContextMenu("Reinitialize")]
	private void Reinitialize()
	{
		if (_lineRenderer != null)
		{
			UnityEngine.Object.DestroyImmediate(_lineRenderer);
		}
		Start();
		Update();
	}

	private void Update()
	{
		base.transform.LookAt(Camera.main.transform);
		base.transform.Rotate(0f, 180f, 0f);
	}
}
