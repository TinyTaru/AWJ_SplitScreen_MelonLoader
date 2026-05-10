using System;

namespace PhotoMode;

[Serializable]
public class MinMax
{
	public float min;

	public float max = 1f;

	public MinMax(float minValue, float maxValue)
	{
		min = minValue;
		max = maxValue;
	}
}
