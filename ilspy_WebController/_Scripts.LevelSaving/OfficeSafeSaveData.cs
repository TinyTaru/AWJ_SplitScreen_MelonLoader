using UnityEngine;
using _Scripts.Office;

namespace _Scripts.LevelSaving;

public struct OfficeSafeSaveData : IHasId
{
	public string id;

	public string correctCode;

	public string[] hintStrings;

	public OfficeSafe.SafeState safeState;

	public string codeInput;

	public Color displayColor;

	public string Id => id;
}
