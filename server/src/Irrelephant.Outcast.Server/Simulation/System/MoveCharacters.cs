using System.Numerics;
using Arch.Core;
using Irrelephant.Outcast.Server.Simulation.Components;

namespace Irrelephant.Outcast.Server.Simulation.System;

public static class MoveCharacters
{
    public static void Run(World world, float deltaTime)
    {
        var allMovableEntitiesQuery = new QueryDescription().WithAll<Transform, Movable>();
        world.Query(
            in allMovableEntitiesQuery,
            (ref Transform transform, ref Movable movable) =>
            {
                if (movable.Position == transform.Position)
                {
                    movable.IsMoved = false;
                    return;
                }

                if (Vector3.Distance(movable.Position, transform.Position) < movable.MoveSpeed * deltaTime)
                {
                    transform.Position = movable.Position;
                }
                else
                {
                    var movement = Vector3.Normalize(movable.Position - transform.Position)
                                   * movable.MoveSpeed
                                   * deltaTime;

                    transform.Position += movement;
                }

                movable.IsMoved = true;
            }
        );
    }
}
