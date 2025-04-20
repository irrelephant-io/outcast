using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Networking.Transport.Abstractions;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Encoding;

/// <summary>
/// Transforms transport-level messages to protocol-level messages.
/// </summary>
public interface IMessageCodec
{
    /// <summary>
    /// Decodes a transport level message into a protocol level message.
    /// </summary>
    /// <param name="tlvMessage">Message to decode.</param>
    /// <returns>Decoded protocol-level message.</returns>
    public Message Decode(TlvMessage tlvMessage);

    /// <summary>
    /// Encodes a protocol level message into a transport level message.
    /// </summary>
    /// <param name="message">Protocol level message.</param>
    /// <returns>Transport level message.</returns>
    public TlvMessage Encode(Message message);
}
