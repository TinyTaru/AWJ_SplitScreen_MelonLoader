using System.Collections.Generic;

namespace _Scripts.LevelSaving;

public struct ChainSaveData : IHasId
{
	public string id;

	public string name;

	public List<ChainLinkSaveData> chainLinkSaveDataList;

	public string Id => id;
}
