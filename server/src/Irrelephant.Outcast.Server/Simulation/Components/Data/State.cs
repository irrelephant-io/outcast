namespace Irrelephant.Outcast.Server.Simulation.Components.Data;

public enum EntityState
{
    Normal = 0,
    Combat = 1,
    Dead = 2,
}


public struct DespawnMarker : IComponent;

public struct Corpse : IComponent
{
    public int DespawnTimer;
    public bool AutoDespawn;
}

public struct State : IComponent
{
    const int CombatCooldown = 50;

    public StateMachine<EntityState> EntityState;
    public int CombatCooldownRemaining;

    public void EnterCombat()
    {
        CombatCooldownRemaining = CombatCooldown;
        EntityState.GoToState(Data.EntityState.Combat);
    }
}
