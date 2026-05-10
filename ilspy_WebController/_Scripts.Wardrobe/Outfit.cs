using System;
using UnityEngine;

namespace _Scripts.Wardrobe;

[Serializable]
public class Outfit : IEquatable<Outfit>
{
	public static int CurrentVersion = 2;

	public int version;

	public string name;

	public bool arachnophobiaMode;

	public bool bodyEnabled;

	public Color bodyColor;

	public float bodyFluffiness;

	public bool abdomenEnabled;

	public Color abdomenColor;

	public float abdomenFluffiness;

	public Color[] legColors;

	public float[] legSegmentFluffiness;

	public bool[] legsEnabled;

	public Color[] jointColors;

	public float[] jointSegmentFluffiness;

	public int eyeIndex;

	public Color eyeColorBase;

	public Color eyeColorLeft;

	public Color eyeColorRight;

	public float[] eyeEffects;

	public int hatIndex;

	public Color[] hatColors;

	public float[] hatEffects;

	public int accessoryIndex;

	public Color[] accessoryColors;

	public float[] accessoryEffects;

	public int shoeIndex;

	public Color[] shoeColors;

	public float[] shoeEffects;

	public bool Equals(Outfit other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		if (eyeEffects.Length != other.eyeEffects.Length || hatColors.Length != other.hatColors.Length || hatEffects.Length != other.hatEffects.Length || accessoryColors.Length != other.accessoryColors.Length || accessoryEffects.Length != other.accessoryEffects.Length || shoeColors.Length != other.shoeColors.Length || shoeEffects.Length != other.shoeEffects.Length)
		{
			return false;
		}
		if (arachnophobiaMode != other.arachnophobiaMode || bodyEnabled != other.bodyEnabled || abdomenEnabled != other.abdomenEnabled || eyeIndex != other.eyeIndex || hatIndex != other.hatIndex || accessoryIndex != other.accessoryIndex || shoeIndex != other.shoeIndex || !bodyFluffiness.Equals(other.bodyFluffiness) || !abdomenFluffiness.Equals(other.abdomenFluffiness) || !bodyColor.Equals(other.bodyColor) || !abdomenColor.Equals(other.abdomenColor) || !eyeColorBase.Equals(other.eyeColorBase) || !eyeColorLeft.Equals(other.eyeColorLeft) || !eyeColorRight.Equals(other.eyeColorRight))
		{
			return false;
		}
		for (int i = 0; i < 3; i++)
		{
			if (legColors[i] != other.legColors[i])
			{
				return false;
			}
			if (jointColors[i] != other.jointColors[i])
			{
				return false;
			}
			if (!Mathf.Approximately(legSegmentFluffiness[i], other.legSegmentFluffiness[i]))
			{
				return false;
			}
			if (!Mathf.Approximately(jointSegmentFluffiness[i], other.jointSegmentFluffiness[i]))
			{
				return false;
			}
		}
		for (int j = 0; j < legsEnabled.Length; j++)
		{
			if (legsEnabled[j] != other.legsEnabled[j])
			{
				return false;
			}
		}
		for (int k = 0; k < eyeEffects.Length; k++)
		{
			if (!Mathf.Approximately(eyeEffects[k], other.eyeEffects[k]))
			{
				return false;
			}
		}
		for (int l = 0; l < hatColors.Length; l++)
		{
			if (hatColors[l] != other.hatColors[l])
			{
				return false;
			}
		}
		for (int m = 0; m < hatEffects.Length; m++)
		{
			if (!Mathf.Approximately(hatEffects[m], other.hatEffects[m]))
			{
				return false;
			}
		}
		for (int n = 0; n < accessoryColors.Length; n++)
		{
			if (accessoryColors[n] != other.accessoryColors[n])
			{
				return false;
			}
		}
		for (int num = 0; num < accessoryEffects.Length; num++)
		{
			if (!Mathf.Approximately(accessoryEffects[num], other.accessoryEffects[num]))
			{
				return false;
			}
		}
		for (int num2 = 0; num2 < shoeColors.Length; num2++)
		{
			if (shoeColors[num2] != other.shoeColors[num2])
			{
				return false;
			}
		}
		for (int num3 = 0; num3 < shoeEffects.Length; num3++)
		{
			if (!Mathf.Approximately(shoeEffects[num3], other.shoeEffects[num3]))
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((Outfit)obj);
	}

	public static bool operator ==(Outfit a, Outfit b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(Outfit a, Outfit b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(name);
		hashCode.Add(arachnophobiaMode);
		hashCode.Add(bodyEnabled);
		hashCode.Add(bodyColor);
		hashCode.Add(bodyFluffiness);
		hashCode.Add(abdomenEnabled);
		hashCode.Add(abdomenColor);
		hashCode.Add(abdomenFluffiness);
		hashCode.Add(legColors);
		hashCode.Add(legSegmentFluffiness);
		hashCode.Add(jointColors);
		hashCode.Add(jointSegmentFluffiness);
		hashCode.Add(eyeIndex);
		hashCode.Add(eyeColorBase);
		hashCode.Add(eyeColorLeft);
		hashCode.Add(eyeColorRight);
		hashCode.Add(eyeEffects);
		hashCode.Add(hatIndex);
		hashCode.Add(hatColors);
		hashCode.Add(hatEffects);
		hashCode.Add(accessoryIndex);
		hashCode.Add(accessoryColors);
		hashCode.Add(accessoryEffects);
		hashCode.Add(shoeIndex);
		hashCode.Add(shoeColors);
		hashCode.Add(shoeEffects);
		return hashCode.ToHashCode();
	}
}
