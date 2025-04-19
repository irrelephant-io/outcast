using System.Collections.Concurrent;
using System.Net.Sockets;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer;

namespace Irrelephant.Outcast.Networking.Transport;

/// <summary>
/// Indicates the state of the currently ongoing read operation.
/// </summary>
internal enum HandlerReadState
{
    AwaitingHeader,
    ReadingHeader,
    AwaitingPayload,
    ReadingPayload,
}

/// <inheritdoc/>
public class TcpTransportHandler : ITransportHandler
{
    /// <inheritdoc/>
    public event EventHandler<TlvMessage>? InboundMessage;

    /// <inheritdoc/>
    public event EventHandler? Closed;

    /// <summary>
    /// Socket args for the current IO socket
    /// </summary>
    private SocketAsyncEventArgs _socketAsyncEventArgs = null!;

    /// <summary>
    /// Internal buffer used for IO operations performed by the socket.
    /// </summary>
    internal readonly byte[] SocketBuffer = new byte[1024];

    /// <summary>
    /// Region of the <see cref="SocketBuffer"/> for which the read operations are yet to be processed.
    /// </summary>
    internal Memory<byte> UnprocessedReadBuffer = Memory<byte>.Empty;

    /// <summary>
    /// State of the reading process for the next message.
    /// </summary>
    internal HandlerReadState ReadState = HandlerReadState.AwaitingHeader;

    /// <summary>
    /// Internal buffer used to build up partially delivered TLV headers.
    /// </summary>
    private readonly byte[] _tlvHeaderReadBuffer = new byte[TlvHeader.Size];

    /// <summary>
    /// Completely read TLV header of the message currently being read.
    /// </summary>
    internal TlvHeader? CompleteTlvHeader;

    /// <summary>
    /// Region of the <see cref="_tlvHeaderReadBuffer"/> that is yet to be filled by read information.
    /// </summary>
    private Memory<byte> _tlvHeaderBuilderMemory = Memory<byte>.Empty;

    /// <summary>
    /// Internal buffer used to build up partially delivered TLV payloads.
    /// </summary>
    private readonly byte[] _tlvPayloadReadBuffer = new byte[1024];

    /// <summary>
    /// Region of the <see cref="_tlvPayloadReadBuffer"/> that is yet to be filled by read information.
    /// </summary>
    private Memory<byte> _tlvPayloadBuilderMemory = Memory<byte>.Empty;

    /// <summary>
    /// Queue of inbound messages that need to be processed.
    /// </summary>
    internal readonly ConcurrentQueue<TlvMessage> InboundMessages = new();

    internal TcpTransportHandler() { }

    static TcpTransportHandler FromSocketAsyncEventArgs(SocketAsyncEventArgs socketArgs)
    {
        var handler = new TcpTransportHandler
        {
            _socketAsyncEventArgs = socketArgs
        };

        handler._socketAsyncEventArgs.SetBuffer(handler.SocketBuffer, 0, handler.SocketBuffer.Length);
        handler._socketAsyncEventArgs.Completed += handler.OnSocketOperationCompleted;

        handler.StartRead();

        return handler;
    }

    private void OnSocketOperationCompleted(object? sender, SocketAsyncEventArgs socketArgs)
    {
        if (socketArgs.LastOperation == SocketAsyncOperation.Receive)
        {
            if (socketArgs.SocketError != SocketError.Success)
            {
                ProcessDisconnect();
            }

            ProcessReceive(socketArgs);
        }
    }

    private void ProcessReceive(SocketAsyncEventArgs socketArgs)
    {
        UnprocessedReadBuffer = new Memory<byte>(SocketBuffer, 0, socketArgs.BytesTransferred);
        ProcessRead();
        StartRead();
    }

    private void StartRead()
    {
        var isAsync = _socketAsyncEventArgs.ConnectSocket!.ReceiveAsync(_socketAsyncEventArgs);
        if (!isAsync)
        {
            UnprocessedReadBuffer = new Memory<byte>(SocketBuffer, 0, _socketAsyncEventArgs.BytesTransferred);
            ProcessRead();
        }
    }

    private void ProcessDisconnect()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    internal void ProcessRead()
    {
        do
        {
            switch (ReadState)
            {
                case HandlerReadState.AwaitingHeader:
                    StartReadingTlvHeader();
                    break;
                case HandlerReadState.ReadingHeader:
                    ContinueReadingTlvHeader();
                    break;
                case HandlerReadState.AwaitingPayload:
                    StartReadingTlvPayload();
                    break;
                case HandlerReadState.ReadingPayload:
                    ContinueReadingTlvPayload();
                    break;
            }
        } while (UnprocessedReadBuffer.Length > 0);
    }

    private void StartReadingTlvHeader()
    {
        if (UnprocessedReadBuffer.Length >= TlvHeader.Size)
        {
            CompleteTlvHeader = TlvHeader.Unpack(UnprocessedReadBuffer);
            UnprocessedReadBuffer = UnprocessedReadBuffer.Slice(TlvHeader.Size);
            ReadState = CompleteTlvHeader.Value.MessageLength > 0
                ? HandlerReadState.AwaitingPayload
                : HandlerReadState.AwaitingHeader;
        }
        else
        {
            _tlvHeaderBuilderMemory = new Memory<byte>(_tlvHeaderReadBuffer);
            UnprocessedReadBuffer.CopyTo(_tlvHeaderBuilderMemory);
            _tlvHeaderBuilderMemory = _tlvHeaderBuilderMemory.Slice(UnprocessedReadBuffer.Length);
            UnprocessedReadBuffer = Memory<byte>.Empty;
            ReadState = HandlerReadState.ReadingHeader;
        }
    }

    private void ContinueReadingTlvHeader()
    {
        if (UnprocessedReadBuffer.Length >= _tlvHeaderBuilderMemory.Length)
        {
            UnprocessedReadBuffer.Slice(0, _tlvHeaderBuilderMemory.Length).CopyTo(_tlvHeaderBuilderMemory);
            UnprocessedReadBuffer = UnprocessedReadBuffer.Slice(_tlvHeaderBuilderMemory.Length);
            CompleteTlvHeader = TlvHeader.Unpack(_tlvHeaderReadBuffer);
            _tlvHeaderBuilderMemory = Memory<byte>.Empty;
            ReadState = CompleteTlvHeader.Value.MessageLength > 0
                ? HandlerReadState.AwaitingPayload
                : HandlerReadState.AwaitingHeader;
        }
        else
        {
            UnprocessedReadBuffer.CopyTo(_tlvHeaderBuilderMemory);
            _tlvHeaderBuilderMemory = _tlvHeaderBuilderMemory.Slice(UnprocessedReadBuffer.Length);
            UnprocessedReadBuffer = Memory<byte>.Empty;
        }
    }

    private void StartReadingTlvPayload()
    {
        if (UnprocessedReadBuffer.Length >= CompleteTlvHeader!.Value.MessageLength)
        {
            var message = new TlvMessage(
                CompleteTlvHeader!.Value,
                UnprocessedReadBuffer.Slice(0, CompleteTlvHeader.Value.MessageLength)
            );

            InboundMessages.Enqueue(message);
            UnprocessedReadBuffer = UnprocessedReadBuffer.Slice(message.Header.MessageLength);
            ReadState = HandlerReadState.AwaitingHeader;
        }
        else
        {
            _tlvPayloadBuilderMemory = new Memory<byte>(_tlvPayloadReadBuffer, 0, CompleteTlvHeader.Value.MessageLength);
            UnprocessedReadBuffer.CopyTo(_tlvPayloadBuilderMemory);
            _tlvPayloadBuilderMemory = _tlvPayloadBuilderMemory.Slice(UnprocessedReadBuffer.Length);
            UnprocessedReadBuffer = Memory<byte>.Empty;
            ReadState = HandlerReadState.ReadingPayload;
        }
    }

    private void ContinueReadingTlvPayload()
    {
        if (UnprocessedReadBuffer.Length >= _tlvPayloadBuilderMemory.Length)
        {
            UnprocessedReadBuffer.CopyTo(_tlvPayloadBuilderMemory);
            UnprocessedReadBuffer = UnprocessedReadBuffer.Slice(_tlvPayloadBuilderMemory.Length);
            var message = new TlvMessage(
                CompleteTlvHeader!.Value,
                new Memory<byte>(_tlvPayloadReadBuffer, 0, CompleteTlvHeader!.Value.MessageLength)
            );
            InboundMessages.Enqueue(message);
            _tlvPayloadBuilderMemory = Memory<byte>.Empty;

            ReadState = HandlerReadState.AwaitingHeader;
        }
        else
        {
            UnprocessedReadBuffer.CopyTo(_tlvPayloadBuilderMemory);
            _tlvPayloadBuilderMemory = _tlvPayloadBuilderMemory.Slice(UnprocessedReadBuffer.Length);
            UnprocessedReadBuffer = Memory<byte>.Empty;
        }
    }
}
