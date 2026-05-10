using System;
using UnityEngine;
using _Scripts.Utils;

namespace _Scripts.KidsRoom;

public class ShmoopGroundTrigger : MonoBehaviour
{
	[SerializeField]
	private LayerMask whatIsGround;

	public event EventHandler OnGroundEnter;

	public event EventHandler OnGroundExit;

	private void OnTriggerEnter(Collider other)
	{
		if (_Scripts.Utils.Utils.IsLayerInLayerMask(other.gameObject.layer, whatIsGround))
		{
			this.OnGroundEnter?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (_Scripts.Utils.Utils.IsLayerInLayerMask(other.gameObject.layer, whatIsGround))
		{
			this.OnGroundExit?.Invoke(this, EventArgs.Empty);
		}
	}
}
