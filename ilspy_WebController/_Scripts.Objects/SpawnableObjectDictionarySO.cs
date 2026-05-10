using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using _Scripts.General;

namespace _Scripts.Objects;

[CreateAssetMenu(fileName = "New Spawnable Object Dictionary", menuName = "FTG/New Spawnable Object Dictionary")]
public class SpawnableObjectDictionarySO : SerializedScriptableObject
{
	public Dictionary<SpawnableObjectType, SpawnableObjectSO> dictionary;
}
