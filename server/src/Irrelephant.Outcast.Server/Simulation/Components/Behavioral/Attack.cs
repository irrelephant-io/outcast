using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

public enum AttackState
{
    Ready = 0,
    Windup = 1,
    Damage = 2,
    Recovery = 3
}

public struct Attack
{
    public StateMachine<AttackState> State;

    public int Damage;
    public int Cooldown;

    public int AttackCooldown;
    public int AttackCooldownRemaining;

    public Arch.Core.Entity? AttackTarget;

    public float Range;
}

public struct DespawnMarker : IComponent;
