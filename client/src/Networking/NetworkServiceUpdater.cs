using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using Irrelephant.Outcast.Client.Networking.MessageProcessing;
using Irrelephant.Outcast.Client.Ui.Control;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking;

public partial class NetworkServiceUpdater : Node
{
    private NetworkService _networkService = null!;

    [Export] public PlayerController PlayerController { get; set; } = null!;

    private readonly Dictionary<Type, Action<Message>> _messageProcessors = new();

    bool MatchingInterfaceCriterion(Type interfaceType, object? criterionMessageType) =>
        interfaceType == typeof(IMessageHandler<>).MakeGenericType((Type)criterionMessageType!);

    public override void _Ready()
    {
        var messageTypes = typeof(Message).Assembly
            .GetTypes()
            .Where(it => it.IsAssignableTo(typeof(Message)))
            .ToHashSet();

        var allMessageProcessors = typeof(NetworkServiceUpdater).Assembly
            .GetTypes()
            .Where(it => it.IsAssignableTo(typeof(IMessageHandler)))
            .ToHashSet();

        var typeFilter = new TypeFilter(MatchingInterfaceCriterion);
        foreach (var messageType in messageTypes)
        {
            var processorType = allMessageProcessors.FirstOrDefault(
                it => it.FindInterfaces(typeFilter, messageType).Length > 0
            );
            if (processorType is null)
            {
                continue;
            }
            var processorInstance = Activator.CreateInstance(processorType);
            var processMethod = processorType.GetMethod(
                nameof(IMessageHandler<Message>.Process),
                BindingFlags.Instance | BindingFlags.Public,
                types: [messageType]
            );
            _messageProcessors.Add(
                messageType,
                msg => processMethod!.Invoke(processorInstance, [msg])
            );
        }

        _networkService = GetNode<NetworkService>("..");
    }

    public override void _Process(double delta)
    {
        if (_networkService.Client is null)
        {
            return;
        }

        while (_networkService.Client.TryDequeueInboundMessage(out var message))
        {
            if (_messageProcessors.TryGetValue(message.GetType(), out var processor))
            {
                processor.Invoke(message);
            }
            else
            {
                GD.PrintErr(
                    $"Unable to process incoming network message of type `{message.GetType().FullName}`. No processor found!"
                );
            }
        }

        base._Process(delta);
    }
}
