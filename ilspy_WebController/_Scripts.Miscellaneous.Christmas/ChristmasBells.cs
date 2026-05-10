using System.Collections.Generic;
using UnityEngine;
using _Scripts.Singletons;

namespace _Scripts.Miscellaneous.Christmas;

public class ChristmasBells : MonoBehaviour
{
	[SerializeField]
	private int[] correctSequence;

	private Queue<int> bellQueue;

	private void Start()
	{
		bellQueue = new Queue<int>();
		for (int i = 0; i < correctSequence.Length; i++)
		{
			bellQueue.Enqueue(-1);
		}
	}

	private void CheckInput()
	{
		for (int i = 0; i < correctSequence.Length; i++)
		{
			if (correctSequence[i] != bellQueue.ToArray()[i])
			{
				return;
			}
		}
		if (Singleton<KitchenChristmasController>.Instance != null)
		{
			Singleton<KitchenChristmasController>.Instance.StartChristmas();
		}
	}

	private void PrintQueue()
	{
		string text = "";
		for (int i = 0; i < bellQueue.Count; i++)
		{
			text += $"{bellQueue.ToArray()[i]}, ";
		}
	}

	public void BellPlayed(int value)
	{
		bellQueue.Dequeue();
		bellQueue.Enqueue(value);
		PrintQueue();
		CheckInput();
	}
}
