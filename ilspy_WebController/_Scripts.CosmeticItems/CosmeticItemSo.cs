using System;
using UnityEngine;
using UnityEngine.Serialization;
using _Scripts.General;

namespace _Scripts.CosmeticItems;

[Serializable]
public class CosmeticItemSo : ScriptableObject
{
	public string displayName;

	public int positionInWardrobe;

	public int positionInDemo;

	[Header("Demo")]
	public bool visibleInDemo;

	[FormerlySerializedAs("unlockedByDefault")]
	public bool unlockedInDemo;

	[Header("Event")]
	public bool unlockedInEvent;

	[Header("Game")]
	public bool hideInGame;

	public bool visibleAtStart;

	public bool unlockedAtStart;

	public CosmeticItemRarity rarity;

	public int price;
}
