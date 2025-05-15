using System.Numerics;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.Components.Ai;

public enum NpcBehaviorState
{
    Roaming = 0,
    Combat = 1
}

public struct Behavior()
{
    public int ThinkingCooldown;
    public int ThinkingCooldownRemaining;
    public int ThinkingCooldownVariance;
    public Vector3 AnchorPosition;
    public float RoamDistance;
    public float PursuitDistance;
    public ThreatTable ThreatTable = new();
    public StateMachine<NpcBehaviorState> State;
}

public struct PassiveBehavior;

public struct AggressiveBehavior;
