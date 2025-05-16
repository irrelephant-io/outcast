using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Client.Networking.MessageProcessing;

public interface IMessageHandler;

public interface IMessageHandler<in TMessage> : IMessageHandler
    where TMessage : Message
{
    void Process(TMessage message);
}
