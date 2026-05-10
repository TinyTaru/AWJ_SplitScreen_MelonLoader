using UnityEngine;

namespace _Scripts.KidsRoom;

public class PlushieHead : MonoBehaviour
{
	[SerializeField]
	private int earAmount;

	private PlushieBody attachedBody;

	private int attachedEars;

	public bool IsComplete => attachedEars == earAmount;

	private void Start()
	{
		attachedEars = 0;
	}

	public void AttachBody(PlushieBody body)
	{
		attachedBody = body;
		attachedBody.CheckCompletion();
	}

	public void AttachEar()
	{
		attachedEars++;
		if (attachedBody != null)
		{
			attachedBody.CheckCompletion();
		}
	}
}
