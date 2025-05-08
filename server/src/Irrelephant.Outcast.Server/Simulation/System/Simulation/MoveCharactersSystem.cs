using System.Numerics;
using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components;

namespace Irrelephant.Outcast.Server.Simulation.System.Simulation;

public static class MoveCharacters
{
    public static void Run(World world, float deltaTime)
    {
        var allMovableEntitiesQuery = new QueryDescription().WithAll<Transform, Movable>();
        world.Query(
            in allMovableEntitiesQuery,
            (ref Transform transform, ref Movable movable) =>
            {
                if (movable.TargetPosition == transform.Position)
                {
                    if (movable.State == MoveState.Moving)
                    {
                        movable.State = MoveState.StoppedMoving;
                    }
                    else if (movable.State == MoveState.StoppedMoving)
                    {
                        movable.State = MoveState.Idle;
                    }

                    return;
                }

                if (movable.State == MoveState.Idle)
                {
                    movable.State = MoveState.StartedMoving;
                }
                else if (movable.State == MoveState.StartedMoving)
                {
                    movable.State = MoveState.Moving;
                }

                if (Vector3.Distance(movable.TargetPosition, transform.Position) < movable.MoveSpeed * deltaTime)
                {
                    transform.Position = movable.TargetPosition;
                }
                else
                {
                    var movement = Vector3.Normalize(movable.TargetPosition - transform.Position)
                                   * movable.MoveSpeed
                                   * deltaTime;

                    transform.Position += movement;
                    transform.Rotation = Vector3.UnitY * (float)Math.Atan2(-movement.Z, movement.X);
                }
            }
        );
    }
}
