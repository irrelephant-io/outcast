using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

public enum AttackState
{
    Ready = 0,
    Windup = 1,
    Damage = 2,
    Recovery = 3
}

public struct AttackDamageDealt
{
    public Entity Entity;
    public int Damage;
}

public struct Attack()
{
    public StateMachine<AttackState> State;

    public int Damage;

    public int AttackCooldown;
    public int AttackCooldownRemaining;

    public Arch.Core.Entity? AttackTarget;

    public Arch.Core.Entity? LockedInTarget;

    public float Range;

    public List<AttackDamageDealt> CompletedAttacks = new();

    public void ClearAttackCommand()
    {
        AttackTarget = null;
    }

    public void ClearCompletedAttacks()
    {
        CompletedAttacks.Clear();
    }
}

public struct DespawnMarker : IComponent;
