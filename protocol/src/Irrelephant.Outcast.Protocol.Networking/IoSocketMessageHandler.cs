using System.Net.Sockets;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Protocol.Networking.EventModel;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Protocol.Networking;

internal enum HandlerReadState
{
    ReadingTlvHeader,
    ReadingTlvPayload,
}

public sealed class IoSocketMessageHandler(
    SocketAsyncEventArgs socket,
    IOptions<NetworkingOptions> options
) : IMessageHandler
{
    /// <summary>
    /// Time of last networking operation performed by this socket.
    /// </summary>
    public DateTimeOffset LastNetworkActivity { get; private set; }

    /// <summary>
    /// Buffer for TLV header reading.
    /// </summary>
    private readonly byte[] _inboundTlvHeaderBuffer = new byte[TlvHeader.Size];

    /// <summary>
    /// Offset in the _inboundTlvHeaderBuffer at which to write next chunk.
    /// </summary>
    private int _tlvHeaderWriteIndex;

    /// <summary>
    /// TLV header of the message currently being read.
    /// </summary>
    private TlvHeader? _completeTlvHeader;

    /// <summary>
    /// Resizable buffer for reading of the message payload.
    /// </summary>
    private byte[] _inboundMessageBuffer = new byte[options.Value.BufferSize];

    /// <summary>
    /// Amount of bytes read from the TLV message payload.
    /// </summary>
    private int _tlvPayloadWriteIndex;

    /// <summary>
    /// Current state of reading of the TLV message.
    /// </summary>
    private HandlerReadState _readReadState = HandlerReadState.ReadingTlvHeader;

    public event EventHandler<Message>? InboundMessageReceived;

    public event EventHandler<OutboundMessageAvailableArgs>? OutboundMessageAvailable;

    public void ProcessRead(Memory<byte> buffer, int receivedBytes, bool isAsync = true)
    {
        UpdateLastActivity(isAsync);
        int processedBytes = 0;

        do
        {
            if (_readReadState == HandlerReadState.ReadingTlvHeader)
            {
                processedBytes += ReadTlvHeader(buffer, receivedBytes, processedBytes);
            }

            if (_readReadState == HandlerReadState.ReadingTlvPayload)
            {
                processedBytes += ReadTlvPayload(buffer, receivedBytes, processedBytes);
            }

        } while (processedBytes < receivedBytes);
    }

    private int ReadTlvHeader(Memory<byte> buffer, int receivedBytes, int processedBytes)
    {
        var bytesAvailableToRead = Math.Min(
            TlvHeader.Size - _tlvHeaderWriteIndex,
            receivedBytes - processedBytes
        );
        var bufferReadRange = processedBytes..(processedBytes + bytesAvailableToRead);
        buffer[bufferReadRange].CopyTo(_inboundTlvHeaderBuffer.AsMemory(_tlvHeaderWriteIndex..));
        _tlvHeaderWriteIndex += bytesAvailableToRead;

        if (_tlvHeaderWriteIndex == TlvHeader.Size)
        {
            _completeTlvHeader = TlvHeader.Unpack(_inboundTlvHeaderBuffer);
            _tlvHeaderWriteIndex = 0;
            _readReadState =
                _completeTlvHeader.Value.MessageLength == 0
                    ? HandlerReadState.ReadingTlvHeader
                    : HandlerReadState.ReadingTlvPayload;
        }

        return bytesAvailableToRead;
    }

    private int ReadTlvPayload(Memory<byte> buffer, int receivedBytes, int processedBytes)
    {
        if (_completeTlvHeader is null)
        {
            throw new InvalidOperationException("Can't read message payload until the header was read!");
        }

        var payloadBytesAvailableToRead = Math.Min(
            _completeTlvHeader.Value.MessageLength - _tlvPayloadWriteIndex,
            receivedBytes - processedBytes
        );
        var bufferReadRange = processedBytes..(processedBytes + payloadBytesAvailableToRead);

        if (payloadBytesAvailableToRead > _inboundMessageBuffer.Length)
        {
            GrowInternalBuffer(minNewSize: payloadBytesAvailableToRead);
        }

        buffer[bufferReadRange].CopyTo(_inboundMessageBuffer.AsMemory(_tlvPayloadWriteIndex..));
        _tlvPayloadWriteIndex += payloadBytesAvailableToRead;

        if (_tlvPayloadWriteIndex == _completeTlvHeader.Value.MessageLength)
        {
            var message = new TlvMessage(
                _completeTlvHeader.Value,
                _inboundMessageBuffer.AsMemory(.._completeTlvHeader.Value.MessageLength)
            );

            var decodedMessage = options.Value.MessageCodec.Decode(message);
            OnMessageReceived(decodedMessage);
            _readReadState = HandlerReadState.ReadingTlvHeader;
            _tlvPayloadWriteIndex = 0;
        }

        return payloadBytesAvailableToRead;
    }

    private void GrowInternalBuffer(int minNewSize)
    {
        var nextPowerOfTwoSize = (int)Math.Pow(2, Math.Ceiling(Math.Log(minNewSize) / Math.Log(2)));
        if (nextPowerOfTwoSize > options.Value.MaxResizableBufferSize)
        {
            throw new InvalidOperationException(
                $"Refusing to grow internal buffer past configured max size of {options.Value.MaxResizableBufferSize}."
            );
        }
        var newBuffer = new byte[nextPowerOfTwoSize];
        _inboundMessageBuffer.CopyTo(newBuffer, 0);
        _inboundMessageBuffer = newBuffer;
    }

    private void UpdateLastActivity(bool isAsync)
    {
        // Sync operations were already pending on the socket, so no need to update activity time
        // in case of a sync operation.
        if (isAsync)
        {
            LastNetworkActivity = DateTimeOffset.UtcNow;
        }
    }

    public void EnqueueMessage(Message message)
    {
        var tlv = options.Value.MessageCodec.Encode(message);
        var packed = tlv.PackInto(_inboundMessageBuffer.AsMemory());
        OnOutboundMessageAvailable(packed);
    }

    /// <summary>
    /// Resets the state machine to be used again.
    /// </summary>
    public void Reset()
    {
        LastNetworkActivity = DateTimeOffset.MinValue;
        _completeTlvHeader = null;
        _tlvHeaderWriteIndex = 0;
        _tlvPayloadWriteIndex = 0;
        _readReadState = HandlerReadState.ReadingTlvHeader;
    }

    private void OnMessageReceived(Message message) =>
        InboundMessageReceived?.Invoke(this, message);

    private void OnOutboundMessageAvailable(Memory<byte> buffer)
    {
        OutboundMessageAvailable?.Invoke(
            this,
            new OutboundMessageAvailableArgs
            {
                MessageContents = buffer,
                Socket = socket
            }
        );
    }
}
