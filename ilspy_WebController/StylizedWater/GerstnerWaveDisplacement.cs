using System;
using UnityEngine;

namespace StylizedWater;

public static class GerstnerWaveDisplacement
{
	private static Vector3 GerstnerWave(Vector3 position, float steepness, float wavelength, float speed, float direction)
	{
		direction = direction * 2f - 1f;
		Vector2 normalized = new Vector2(Mathf.Cos(MathF.PI * direction), Mathf.Sin(MathF.PI * direction)).normalized;
		float num = MathF.PI * 2f / wavelength;
		float num2 = steepness / num;
		float f = num * (Vector2.Dot(normalized, new Vector2(position.x, position.z)) - speed * Time.time);
		return new Vector3(normalized.x * num2 * Mathf.Cos(f), num2 * Mathf.Sin(f), normalized.y * num2 * Mathf.Cos(f));
	}

	public static Vector3 GetWaveDisplacement(Vector3 position, float steepness, float wavelength, float speed, float[] directions)
	{
		return Vector3.zero + GerstnerWave(position, steepness, wavelength, speed, directions[0]) + GerstnerWave(position, steepness, wavelength, speed, directions[1]) + GerstnerWave(position, steepness, wavelength, speed, directions[2]) + GerstnerWave(position, steepness, wavelength, speed, directions[3]);
	}
}
