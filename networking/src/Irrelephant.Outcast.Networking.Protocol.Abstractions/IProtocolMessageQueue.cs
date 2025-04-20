using System.Diagnostics.CodeAnalysis;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions;

/// <summary>
/// Protocol message queue implementing protocol-level communications on top of a specified transport.
/// </summary>
public interface IProtocolMessageQueue
{
    /// <summary>
    /// Tries to dequeue a protocol message.
    /// </summary>
    /// <param name="inboundMessage">Dequeued message.</param>
    /// <returns>True if a message was successfully dequeued. False otherwise.</returns>
    bool TryDequeueInboundMessage(
        [NotNullWhen(true)]out Message? inboundMessage
    );

    /// <summary>
    /// Enqueues a protocol message to be sent over the transport.
    /// </summary>
    /// <param name="message"></param>
    void EnqueueOutboundMessage(Message message);

    /// <summary>
    /// Initiates `Receive` operations on the underlying transport, populating the inbound message queue.
    /// </summary>
    public void Receive();

    /// <summary>
    /// Initiates `Transmit` operations on the underlying transport, flushing the outbound message queue.
    /// </summary>
    public void Transmit();
}
