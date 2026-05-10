using UnityEngine;

namespace _Scripts.Objects;

public class WaterOrbSpawnerTutorial : WaterOrbSpawner
{
	private bool questActive;

	protected new void Update()
	{
		base.Update();
		TrySpawnWaterOrb();
	}

	protected new void OnTriggerExit(Collider other)
	{
		if (other.GetComponentInParent<WaterOrb>() == base.CurrentWaterOrb)
		{
			EnableButtonPromptForCurrentWaterOrb(value: false);
		}
		base.OnTriggerExit(other);
	}

	private void EnableButtonPromptForCurrentWaterOrb(bool value)
	{
		if (base.CurrentWaterOrb is WaterOrbTutorial waterOrbTutorial)
		{
			waterOrbTutorial.EnableButtonPrompt(value);
		}
	}

	protected override void SpawnWaterOrb()
	{
		base.SpawnWaterOrb();
		EnableButtonPromptForCurrentWaterOrb(questActive);
	}

	public void SetQuestActive(bool value)
	{
		questActive = value;
		EnableButtonPromptForCurrentWaterOrb(value);
	}
}
