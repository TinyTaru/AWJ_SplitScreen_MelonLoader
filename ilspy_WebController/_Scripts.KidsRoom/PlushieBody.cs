using UnityEngine;
using UnityEngine.Events;
using _Scripts.Objects;

namespace _Scripts.KidsRoom;

public class PlushieBody : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private MagneticLock magneticLockHead;

	[Header("Parameters")]
	[SerializeField]
	private int legAmount;

	[SerializeField]
	private bool hasTail;

	[Header("Unity Events")]
	[SerializeField]
	private UnityEvent onPlushieCompleted;

	private PlushieHead attachedHead;

	private int attachedLegs;

	private bool tailAttached;

	private bool isAssembled;

	private void Start()
	{
		attachedLegs = 0;
		tailAttached = false;
	}

	public void AttachHead()
	{
		attachedHead = magneticLockHead.ConnectedObject.GetComponent<PlushieHead>();
		attachedHead.AttachBody(this);
		CheckCompletion();
	}

	public void AttachLeg()
	{
		attachedLegs++;
		CheckCompletion();
	}

	public void AttachTail()
	{
		tailAttached = true;
		CheckCompletion();
	}

	public void CheckCompletion()
	{
		if (!(attachedHead == null) && attachedLegs == legAmount && tailAttached == hasTail && attachedHead.IsComplete && !isAssembled)
		{
			isAssembled = true;
			onPlushieCompleted?.Invoke();
			Debug.Log("Plushie is fully assembled!");
		}
	}
}
