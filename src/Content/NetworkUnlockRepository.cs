using System.Collections.Generic;
using System.Linq;

public class NetworkUnlockRepository
{
    private readonly GameWorld _gameWorld;

    public NetworkUnlockRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    public List<NetworkUnlock> GetAllNetworkUnlocks()
    {
        return _gameWorld.WorldState.NetworkUnlocks;
    }

    public NetworkUnlock GetNetworkUnlockById(string id)
    {
        return _gameWorld.WorldState.NetworkUnlocks
            .FirstOrDefault(n => n.Id == id);
    }

    public List<NetworkUnlock> GetNetworkUnlocksForNpc(string npcId)
    {
        return _gameWorld.WorldState.NetworkUnlocks
            .Where(n => n.UnlockerNpcId == npcId)
            .ToList();
    }

    public NetworkUnlockTarget GetUnlockTarget(string unlockerNpcId, string targetNpcId)
    {
        List<NetworkUnlock> unlocks = GetNetworkUnlocksForNpc(unlockerNpcId);
        foreach (NetworkUnlock unlock in unlocks)
        {
            NetworkUnlockTarget? target = unlock.Unlocks.FirstOrDefault(u => u.NpcId == targetNpcId);
            if (target != null) return target;
        }
        return null;
    }
}
