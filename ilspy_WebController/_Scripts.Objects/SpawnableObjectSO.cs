using UnityEngine;

namespace _Scripts.Objects;

[CreateAssetMenu(fileName = "New Spawnable Object", menuName = "FTG/New Spawnable Object")]
public class SpawnableObjectSO : ScriptableObject
{
	public SpawnableObject spawnableObject;
}
