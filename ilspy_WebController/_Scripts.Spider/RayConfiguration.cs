using System;
using UnityEngine;

namespace _Scripts.Spider;

[Serializable]
public class RayConfiguration
{
	public int numberOfRays = 1;

	public float radius;

	public float verticalOffset;

	public float angle;

	public float rayLength;

	public float sphereRadius;

	public Color color;

	[HideInInspector]
	public Ray[] rays;
}
