namespace Irrelephant.Outcast.Server.Simulation.Components;

public enum AttackState
{

}

public struct Attack
{
    public int Damage;
    public int Cooldown;

    public int AttackCooldown;
    public int AttackCooldownRemaining;

    public Arch.Core.Entity? AttackTarget;

    public float Range;
}

public struct DespawnMarker : IComponent;
