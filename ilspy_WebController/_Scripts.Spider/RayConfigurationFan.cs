using System;
using UnityEngine;

namespace _Scripts.Spider;

[Serializable]
public class RayConfigurationFan
{
	public bool followDirection;

	public int numberOfRays;

	public float radius;

	public float verticalOffset;

	public float directionAngle;

	public float fanAngle;

	public float verticalAngle;

	public float rayLength;

	public float sphereRadius;

	public Color color;

	[HideInInspector]
	public Ray[] rays;
}
