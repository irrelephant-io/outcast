using System.Numerics;

namespace Irrelephant.Outcast.Server.Simulation.Components.Ai;

public struct Behavior
{
    public int ThinkingCooldown;
    public int ThinkingCooldownRemaining;
    public int ThinkingCooldownVariance;
    public Vector3 AnchorPosition;
    public float RoamDistance;
}

public struct PassiveBehavior;

public struct AggressiveBehavior;
