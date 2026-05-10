using UnityEngine;

public class SimpleElevator : MonoBehaviour
{
	public float minY;

	public float maxY;

	public float speed;

	public bool on;

	private float m_Direction = 1f;

	private void FixedUpdate()
	{
		if (base.transform.position.y < minY)
		{
			m_Direction = 1f;
		}
		if (base.transform.position.y > maxY)
		{
			m_Direction = -1f;
		}
		Vector3 vector = new Vector3(0f, m_Direction * speed * Time.fixedDeltaTime, 0f);
		base.transform.position += vector;
	}
}
