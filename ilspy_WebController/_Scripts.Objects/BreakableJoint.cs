using UnityEngine;
using UnityEngine.Events;
using _Scripts.LevelSaving;

namespace _Scripts.Objects;

public class BreakableJoint : MonoBehaviour, IInitializable<BreakableJointSaveData>, IHasSaveData<BreakableJointSaveData>
{
	[SerializeField]
	private UnityEvent onJointDestroyed;

	public bool JointIsBroken => base.gameObject.GetComponents<Joint>().Length == 0;

	public void Initialize(BreakableJointSaveData saveData)
	{
		if (saveData.jointIsBroken)
		{
			Joint[] components = GetComponents<Joint>();
			foreach (Joint joint in components)
			{
				Debug.Log($"Destroying joint {joint} on {base.name}");
				Object.Destroy(joint);
				onJointDestroyed?.Invoke();
			}
		}
	}

	public BreakableJointSaveData GetSaveData()
	{
		UniqueID component = GetComponent<UniqueID>();
		string id = string.Empty;
		if (component == null)
		{
			Debug.LogError("Breakable Joint " + base.gameObject.name + " has no uniqueID");
		}
		else
		{
			id = component.ID;
		}
		BreakableJointSaveData result = default(BreakableJointSaveData);
		result.id = id;
		result.name = base.gameObject.name;
		result.jointIsBroken = JointIsBroken;
		return result;
	}
}
