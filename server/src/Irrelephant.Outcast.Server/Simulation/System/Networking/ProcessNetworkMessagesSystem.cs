using Arch.Core;
using Arch.Core.Extensions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Simulation.Components;
using Irrelephant.Outcast.Server.Simulation.Components.Behavioral;
using Irrelephant.Outcast.Server.Simulation.Components.Communication;
using Irrelephant.Outcast.Server.Simulation.Components.Data;
using Irrelephant.Outcast.Server.Simulation.System.EntityLifecycle;

namespace Irrelephant.Outcast.Server.Simulation.System.Networking;

public class ProcessNetworkMessagesSystem(
    EntitySpawner entitySpawner
)
{
    public void Run(World world)
    {
        var allClients = new QueryDescription().WithAll<ProtocolClient>();
        world.Query(
            in allClients,
            (Entity entity, ref ProtocolClient protocolClient) =>
            {
                while (protocolClient.Network.TryDequeueInboundMessage(out var message))
                {
                    if (message is MoveCommand move)
                    {
                        ProcessMoveRequest(world, ref entity, move);
                    }
                    else if (message is ConnectRequest request)
                    {
                        ProcessConnectRequest(ref protocolClient, ref entity, request);
                    }
                    else if (message is AttackCommand attack)
                    {
                        ProcessAttackRequest(world, entity, attack);
                    }
                }
            }
        );
    }

    private static void ProcessMoveRequest(
        World world,
        ref Entity entity,
        MoveCommand move
    )
    {
        if (world.Has<Movement, GlobalId>(entity))
        {
            ref var movement = ref entity.TryGetRef<Movement>(out _);
            ref var attack = ref entity.TryGetRef<Attack>(out var hasAttack);

            if (hasAttack)
            {
                attack.ClearAttackCommand();
            }

            movement.SetMoveToPosition(move.MovePosition);
        }
    }

    private void ProcessAttackRequest(
        World world,
        Entity entity,
        AttackCommand attack
    )
    {
        if (world.Has<Attack, GlobalId>(entity))
        {
            var query = new QueryDescription().WithAll<GlobalId>().WithNone<Corpse>();
            world.Query(
                in query,
                (Entity targetEntity, ref GlobalId gid, ref State state) =>
                {
                    if (gid.Id == attack.EntityId)
                    {
                        ref var attacker = ref entity.TryGetRef<Attack>(out _);
                        attacker.AttackTarget = targetEntity;
                    }
                }
            );
        }
    }

    private void ProcessConnectRequest(
        ref ProtocolClient protocolClient,
        ref Entity entity,
        ConnectRequest request
    )
    {
        entitySpawner.SpawnConnectingPlayer(ref entity, request.Name);
        var gid = entity.Get<GlobalId>();
        var transform = entity.Get<Transform>();
        protocolClient.Network.EnqueueOutboundMessage(
            new ConnectResponse
            (
                protocolClient.Network.SessionId,
                gid.Id,
                SpawnPosition: transform.Position,
                YAxisRotation: transform.Rotation.Y,
                gid.ArchetypeId
            )
        );

        var movement = entity.Get<Movement>();
        protocolClient.Network.EnqueueOutboundMessage(
            new MovementSpeedNotification(gid.Id, movement.MoveSpeed)
        );

        var health = entity.Get<Health>();
        protocolClient.Network.EnqueueOutboundMessage(
            new HealthNotification(gid.Id, health.CurrentHealth)
        );
        protocolClient.Network.EnqueueOutboundMessage(
            new MaxHealthNotification(gid.Id, health.MaxHealth)
        );

        protocolClient.Network.EnqueueOutboundMessage(
            new ConnectTransferComplete(protocolClient.Network.SessionId)
        );
    }
}
