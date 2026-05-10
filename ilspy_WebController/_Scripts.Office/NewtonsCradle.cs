using UnityEngine;

namespace _Scripts.Office;

public class NewtonsCradle : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Transform firstBall;

	[SerializeField]
	private Transform[] otherBalls;

	[SerializeField]
	private Transform[] otherRings;

	[Header("Parameters")]
	[SerializeField]
	private float distanceBetweenBalls;

	private void UpdateBallPositions()
	{
		for (int i = 0; i < otherBalls.Length; i++)
		{
			otherBalls[i].position = firstBall.position + Vector3.left * (1 + i) * distanceBetweenBalls;
			otherRings[i].position = firstBall.position + Vector3.left * (1 + i) * distanceBetweenBalls;
		}
	}
}
