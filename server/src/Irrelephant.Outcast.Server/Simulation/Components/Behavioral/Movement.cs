using System.Numerics;
using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components.Data;

namespace Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

public enum MoveState
{
    Idle = 0,
    Moving = 1,
    Stopped = 2,
    Locked = 3
}

public struct Movement : IComponent
{
    public StateMachine<MoveState> State;
    public Vector3? TargetPosition;
    public Entity? FollowEntity;
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

    public void SetMoveToPosition(Vector3 position)
    {
        FollowEntity = null;
        TargetPosition = position;
    }

    public void SetFollowEntity(Entity entity, float followDistance)
    {
        TargetPosition = null;
        FollowEntity = entity;
        FollowDistance = followDistance;
    }
}
