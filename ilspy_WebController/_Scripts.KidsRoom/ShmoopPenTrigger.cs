using System;
using UnityEngine;

namespace _Scripts.KidsRoom;

public class ShmoopPenTrigger : MonoBehaviour
{
	public event EventHandler OnPenEnter;

	public event EventHandler OnPenExit;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Shmoop"))
		{
			this.OnPenEnter?.Invoke(this, EventArgs.Empty);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Shmoop"))
		{
			this.OnPenExit?.Invoke(this, EventArgs.Empty);
		}
	}
}
