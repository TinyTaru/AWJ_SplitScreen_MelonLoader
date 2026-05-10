using UnityEngine;

public class PointAtAimTarget : MonoBehaviour
{
	[Tooltip("This object represents the aim target.  We always point toeards this")]
	public Transform AimTarget;

	private void Update()
	{
		if (!(AimTarget == null))
		{
			Vector3 forward = AimTarget.position - base.transform.position;
			if (forward.sqrMagnitude > 0.01f)
			{
				base.transform.rotation = Quaternion.LookRotation(forward);
			}
		}
	}
}
