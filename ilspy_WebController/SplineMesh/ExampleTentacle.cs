using UnityEngine;

namespace SplineMesh;

[DisallowMultipleComponent]
public class ExampleTentacle : MonoBehaviour
{
	public float startScale = 1f;

	public float endScale = 1f;

	public float startRoll;

	public float endRoll;

	private Spline spline => GetComponent<Spline>();

	private void OnValidate()
	{
		float num = 0f;
		foreach (CubicBezierCurve curf in spline.GetCurves())
		{
			float num2 = num / spline.Length;
			num += curf.Length;
			float num3 = num / spline.Length;
			curf.n1.Scale = Vector2.one * (startScale + (endScale - startScale) * num2);
			curf.n2.Scale = Vector2.one * (startScale + (endScale - startScale) * num3);
			curf.n1.Roll = startRoll + (endRoll - startRoll) * num2;
			curf.n2.Roll = startRoll + (endRoll - startRoll) * num3;
		}
	}
}
