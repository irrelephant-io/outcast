﻿using System.Diagnostics;
using System.Numerics;
using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;

namespace Irrelephant.Outcast.Server.Simulation.System.Simulation;

public static class MoveCharactersSystem
{
    public static void Run(World world, float deltaTime)
    {
        var allMovableEntitiesQuery = new QueryDescription().WithAll<Transform, Movement>();
        world.Query(
            in allMovableEntitiesQuery,
            (ref Transform transform, ref Movement movement) =>
            {
                movement.State.ClearStateChange();
                while (movement.State.EnterTick())
                {
                    switch (movement.State.Current)
                    {
                        case MoveState.Idle:
                            RunIdleState(ref transform, ref movement);
                            break;
                        case MoveState.Moving:
                            RunMovingState(ref transform, ref movement, deltaTime);
                            break;
                        case MoveState.Stopped:
                            RunStoppedState(ref transform, ref movement);
                            break;
                        case MoveState.Locked:
                            RunLockedState(ref transform, ref movement);
                            break;
                        default:
                            throw new UnreachableException();
                    }
                }
            }
        );
    }

    private static void RunLockedState(ref Transform transform, ref Movement movement)
    {
        var target = GetTargetPosition(ref movement);
        if (!target.HasValue || !movement.FollowEntity.HasValue)
        {
            return;
        }

        // Only rotate towards target entity when movement is locked.
        var deltaToTarget = target.Value - transform.Position;
        transform.Rotation = Vector3.UnitY * (float)Math.Atan2(-deltaToTarget.Z, deltaToTarget.X);
    }

    private static void RunStoppedState(ref Transform transform, ref Movement movement)
    {
        var target = GetTargetPosition(ref movement);
        if (!target.HasValue)
        {
            movement.State.GoToState(MoveState.Idle);
            return;
        }

        if (!IsInPosition(ref transform, ref movement))
        {
            movement.State.GoToState(MoveState.Moving);
        }

        // Look towards target position in stopped state.
        var deltaToTarget = target.Value - transform.Position;
        transform.Rotation = Vector3.UnitY * (float)Math.Atan2(-deltaToTarget.Z, deltaToTarget.X);
    }

    private static void RunIdleState(ref Transform transform, ref Movement movement)
    {
        if (movement.FollowEntity.HasValue && !movement.FollowEntity.Value.IsAlive())
        {
            movement.FollowEntity = null;
        }

        if (!IsInPosition(ref transform, ref movement))
        {
            movement.State.GoToState(MoveState.Moving);
        }
    }

    private static void RunMovingState(ref Transform transform, ref Movement movement, float deltaTime)
    {
        if (IsInPosition(ref transform, ref movement))
        {
            // Keep following the entity with minimal movement or go to Stopped state if in position next to target.
            if (movement.FollowEntity.HasValue)
            {
                movement.State.GoToState(MoveState.Stopped);
            }
            else
            {
                // Destination reached.
                movement.TargetPosition = null;
                movement.State.GoToState(MoveState.Idle);
            }

            return;
        }

        var target = GetTargetPosition(ref movement);
        if (!target.HasValue)
        {
            movement.State.GoToState(MoveState.Idle);
            return;
        }

        var speedFactor = movement.MoveSpeed * deltaTime;
        var deltaToTarget = target.Value - transform.Position;
        var deltaMovement = deltaToTarget.LengthSquared() > speedFactor * speedFactor
            ? Vector3.Normalize(deltaToTarget) * speedFactor
            : deltaToTarget;

        transform.Position += deltaMovement;
        transform.Rotation = Vector3.UnitY * (float)Math.Atan2(-deltaMovement.Z, deltaMovement.X);
    }

    private static Vector3? GetTargetPosition(ref Movement movement)
    {
        if (movement.FollowEntity.HasValue && movement.FollowEntity.Value.IsAlive())
        {
            ref var targetTransform = ref movement.FollowEntity.Value.TryGetRef<Transform>(out var exists);
            if (exists)
            {
                return targetTransform.Position;
            }
        }

        if (movement.TargetPosition.HasValue)
        {
            return movement.TargetPosition.Value;
        }

        return null;
    }

    private static bool IsInPosition(ref Transform transform, ref Movement movement)
    {
        if (movement.TargetPosition.HasValue)
        {
            return (movement.TargetPosition.Value - transform.Position).LengthSquared() < 0.001f;
        }
        if (movement.FollowEntity.HasValue && movement.FollowEntity.Value.IsAlive())
        {
            var targetTransform = movement.FollowEntity.Value.TryGetRef<Transform>(out var targetHasTransform);
            if (targetHasTransform)
            {
                return (targetTransform.Position - transform.Position).LengthSquared()
                    < movement.FollowDistance * movement.FollowDistance;
            }
        }

        return true;
    }
}
