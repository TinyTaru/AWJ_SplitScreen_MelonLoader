using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using _Scripts.General;
using _Scripts.LevelSaving;
using _Scripts.Objects;

namespace _Scripts.Web;

public class WebJoint : MonoBehaviour
{
	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private UniqueID uniqueID;

	[SerializeField]
	private UnityEvent onAllWebsRemovedEvent;

	public List<WebJoint> connectedWebJoints;

	public List<WebThread> attachedWebThreads;

	public List<SpringJoint> springJoints;

	private FixedJoint fixedJoint;

	private bool hasFixedJoint;

	private MovableObject attachedObject;

	private Transform anchor;

	public Rigidbody Rb => rb;

	public bool HasFixedJoint => hasFixedJoint;

	public Transform Anchor => anchor;

	public bool IsKinematic => rb.isKinematic;

	public FixedJoint GetFixedJoint()
	{
		return fixedJoint;
	}

	public void SetupWebJoint(GameObject object1 = null, Vector3 position = default(Vector3))
	{
		if (object1 != null && position != default(Vector3))
		{
			attachedObject = object1.GetComponent<MovableObject>();
			if (attachedObject != null)
			{
				base.transform.position = position;
				rb.linearDamping = 0f;
				Rb.isKinematic = false;
				fixedJoint = rb.gameObject.AddComponent<FixedJoint>();
				fixedJoint.connectedBody = object1.GetComponent<Rigidbody>();
				attachedObject.AddWebJoint(this);
				hasFixedJoint = true;
			}
			else
			{
				base.transform.position = position;
				Rb.isKinematic = true;
			}
		}
		else
		{
			base.transform.position = position;
			Rb.isKinematic = false;
		}
		anchor = base.transform;
		springJoints = new List<SpringJoint>();
		uniqueID.GenerateNewID();
	}

	public void AddConnectedWebJoint(WebJoint webJoint)
	{
		if (connectedWebJoints == null)
		{
			connectedWebJoints = new List<WebJoint>();
		}
		if (!connectedWebJoints.Contains(webJoint))
		{
			connectedWebJoints.Add(webJoint);
		}
	}

	public void RemoveConnectedWebJoint(WebJoint webJoint)
	{
		connectedWebJoints.Remove(webJoint);
	}

	public void RemoveMeFromConnectedWebJoints()
	{
		foreach (WebJoint connectedWebJoint in connectedWebJoints)
		{
			connectedWebJoint.RemoveConnectedWebJoint(this);
		}
	}

	public void AddAttachedWeb(WebThread webThread)
	{
		if (attachedWebThreads == null)
		{
			attachedWebThreads = new List<WebThread>();
		}
		if (!attachedWebThreads.Contains(webThread))
		{
			attachedWebThreads.Add(webThread);
		}
	}

	public void RemoveAttachedWeb(WebThread webThread)
	{
		attachedWebThreads.Remove(webThread);
		if (attachedWebThreads.Count == 0)
		{
			onAllWebsRemovedEvent?.Invoke();
		}
	}

	public void RemoveSpringJoint(WebJoint connectedWebJoint)
	{
		if (rb == null)
		{
			return;
		}
		foreach (SpringJoint item in springJoints.ToList())
		{
			if (item.connectedBody == connectedWebJoint.Rb)
			{
				DontDestroyMe[] componentsInChildren = item.GetComponentsInChildren<DontDestroyMe>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].ResetParent();
				}
				springJoints.Remove(item);
				Object.Destroy(item);
			}
		}
	}

	public void DestroySafely()
	{
		DontDestroyMe[] componentsInChildren = GetComponentsInChildren<DontDestroyMe>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].ResetParent();
		}
		if (attachedObject != null)
		{
			attachedObject.RemoveWebJoint(this);
		}
		Object.Destroy(base.gameObject);
	}

	public void SetupPlayerWebJoint()
	{
		rb = GetComponent<Rigidbody>();
		springJoints = new List<SpringJoint>();
	}

	public void SetAnchor(Transform newAnchor)
	{
		anchor = newAnchor;
	}
}
