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
public class TcpTransportHandlerReceive : ITransportHandler
{
    /// <inheritdoc/>
    public event EventHandler? Closed;

    /// <summary>
    /// Socket args for the current IO socket
    /// </summary>
    private SocketAsyncEventArgs _socketAsyncEventArgs = null!;

    /// <summary>
    /// Internal buffer used for read operations performed by the socket.
    /// </summary>
    internal readonly byte[] SocketIoBuffer = new byte[1024];

    /// <summary>
    /// Region of the <see cref="SocketIoBuffer"/> for which the read operations are yet to be processed.
    /// </summary>
    internal Memory<byte> UnprocessedReadBuffer = Memory<byte>.Empty;

    /// <summary>
    /// Region of the <see cref="SocketIoBuffer"/> for which the write operations are yet to be processed.
    /// </summary>
    internal Memory<byte> UnprocessedWriteBuffer = Memory<byte>.Empty;

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
    public ConcurrentQueue<TlvMessage> InboundMessages { get; } = new();

    /// <summary>
    /// Queue of outbound messages that need to be sent.
    /// </summary>
    internal readonly ConcurrentQueue<TlvMessage> OutboundMessages = new();

    internal TcpTransportHandlerReceive() { }

    /// <summary>
    /// Synchronizes access to underlying socket, allowing only one async operation at a time.
    /// </summary>
    private SemaphoreSlim _asyncOperationSemaphore = new(1, 1);

    /// <summary>
    /// Creates a transport handler out of a connected or accepted socket.
    /// </summary>
    /// <param name="socketArgs">
    /// SocketAsyncEventArgs produced as a result of the socket connect/accept operation.
    /// </param>
    public static TcpTransportHandlerReceive FromSocketAsyncEventArgs(SocketAsyncEventArgs socketArgs)
    {
        var handler = new TcpTransportHandlerReceive
        {
            _socketAsyncEventArgs = socketArgs
        };

        handler._socketAsyncEventArgs.Completed += handler.OnSocketOperationCompleted;

        return handler;
    }

    public void Receive()
    {
        var socket = _socketAsyncEventArgs.ConnectSocket ?? _socketAsyncEventArgs.AcceptSocket;
        if (socket is null)
        {
            throw new InvalidOperationException("Neither connect nor accept socket were available for receiving.");
        }

        _asyncOperationSemaphore.Wait();
        if (socket.Available == 0)
        {
            _asyncOperationSemaphore.Release();
            return;
        }

        _socketAsyncEventArgs.SetBuffer(SocketIoBuffer, 0, SocketIoBuffer.Length);
        var isAsync = socket.ReceiveAsync(_socketAsyncEventArgs);
        if (!isAsync)
        {
            ProcessReceive(_socketAsyncEventArgs);
        }
    }

    public void EnqueueOutboundMessage(TlvMessage tlvMessage) =>
        OutboundMessages.Enqueue(tlvMessage);

    public void Transmit()
    {
        _asyncOperationSemaphore.Wait();
        ProcessTransmit(_socketAsyncEventArgs);
    }

    private void ProcessTransmit(SocketAsyncEventArgs socketArgs)
    {
        var socket = socketArgs.ConnectSocket ?? socketArgs.AcceptSocket;
        if (socket is null)
        {
            _asyncOperationSemaphore.Release();
            throw new InvalidOperationException("Neither connect nor accept socket were available for sending.");
        }

        var writeBuffer = PrepareOutboundBuffer();
        if (!writeBuffer.IsEmpty)
        {
            _socketAsyncEventArgs.SetBuffer(writeBuffer);
            var isAsync = socket.SendAsync(_socketAsyncEventArgs);
            if (!isAsync)
            {
                ProcessTransmit(socketArgs);
            }
        }
        else
        {
            _asyncOperationSemaphore.Release();
        }
    }

    private Memory<byte> PrepareOutboundBuffer()
    {
        int bytesToWrite = 0;

        UnprocessedWriteBuffer = new Memory<byte>(SocketIoBuffer, 0, SocketIoBuffer.Length);
        while (OutboundMessages.TryPeek(out var peekedMessage))
        {
            if (UnprocessedWriteBuffer.Length > peekedMessage.Size && OutboundMessages.TryDequeue(out var message))
            {
                message.PackInto(UnprocessedWriteBuffer);
                UnprocessedWriteBuffer = UnprocessedWriteBuffer.Slice(message.Size);
                bytesToWrite += message.Size;
            }
            else
            {
                break;
            }
        }

        return new Memory<byte>(SocketIoBuffer, 0, bytesToWrite);
    }

    private void OnSocketOperationCompleted(object? sender, SocketAsyncEventArgs socketArgs)
    {
        if (socketArgs.BytesTransferred == 0 || socketArgs.SocketError != SocketError.Success)
        {
            ProcessDisconnect();
        }

        if (socketArgs.LastOperation == SocketAsyncOperation.Receive)
        {
            ProcessReceive(socketArgs);
        }

        if (socketArgs.LastOperation == SocketAsyncOperation.Send)
        {
            ProcessTransmit(socketArgs);
        }
    }

    private void ProcessReceive(SocketAsyncEventArgs socketArgs)
    {
        UnprocessedReadBuffer = new Memory<byte>(SocketIoBuffer, 0, socketArgs.BytesTransferred);
        ProcessRead();
        _asyncOperationSemaphore.Release();
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
            UnprocessedReadBuffer.Slice(0, _tlvHeaderBuilderMemory.Length).CopyTo(_tlvPayloadBuilderMemory);
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

    public void Dispose()
    {
        _socketAsyncEventArgs.Dispose();
    }
}
