using System.Numerics;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

public enum MoveState
{
    Idle = 0,
    Moving = 1,
    Locked = 2,
}

public struct Movement : IComponent
{
    public StateMachine<MoveState> State;
    public Vector3? TargetPosition;
    public Arch.Core.Entity? FollowEntity;
    public float FollowDistance;
    public float MoveSpeed;

    public void LockMovement()
    {
        State.GoToState(MoveState.Locked);
    }

    public void UnlockMovement()
    {
        State.GoToState(State.Previous);
    }
}
