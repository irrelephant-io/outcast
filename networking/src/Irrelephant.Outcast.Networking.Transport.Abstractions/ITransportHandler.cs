using System.Collections.Concurrent;

namespace Irrelephant.Outcast.Networking.Transport.Abstractions;

public interface ITransportHandler : IDisposable
{
    /// Is emitted when the underlying transport connection is closed due to disconnection or error.
    public event EventHandler? Closed;

    /// <summary>
    /// All messages received by the transport handler and are awaiting to be processed
    /// </summary>
    ConcurrentQueue<TlvMessage> InboundMessages { get; }

    /// <summary>
    /// Enqueues a message into the send queue to be sent over the transport when the queue is flushed.
    /// </summary>
    /// <remarks>
    /// Will not send messages over the socket until <see cref="Transmit"/> is called.
    /// </remarks>
    /// <param name="tlvMessage">Message to enqueue.</param>
    void EnqueueOutboundMessage(TlvMessage tlvMessage);

    /// <summary>
    /// Performs receive operations on the underlying transport, populating <see cref="InboundMessages"/> queue
    /// </summary>
    void Receive();

    /// <summary>
    /// Flushes the currently enqueued outbound traffic into the transport.
    /// </summary>
    void Transmit();

    /// <summary>
    /// Timestamp of the last recorded network activity on this handler.
    /// </summary>
    DateTime LastNetworkActivity { get; }
}
