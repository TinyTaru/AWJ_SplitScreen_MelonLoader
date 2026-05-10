using UnityEngine;

namespace _Scripts.Wardrobe.Eyes;

public class FlameEyes : MonoBehaviour
{
	[Header("Left")]
	[SerializeField]
	private SpriteRenderer flameLeft;

	[Header("Right")]
	[SerializeField]
	private SpriteRenderer flameRight;

	public void SetFlameColorLeft(Color color)
	{
		flameLeft.material.color = color;
	}

	public void SetFlameColorRight(Color color)
	{
		flameRight.material.color = color;
	}
}
