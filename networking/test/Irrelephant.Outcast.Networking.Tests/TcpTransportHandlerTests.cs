using System.Net.Sockets;
using FluentAssertions;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer;
using Irrelephant.Outcast.Networking.Transport;

namespace Irrelephant.Outcast.Networking.Tests;

public class TcpTransportHandlerTests
{
    private readonly TcpTransportHandler _sut = new();

    [Fact]
    void ProcessRead_CanReadWholeTlvHeader_WhenAvailable()
    {
        var header = new TlvHeader(MessageType: 13, MessageLength: 0);
        header.PackInto(_sut.SocketBuffer);
        _sut.UnprocessedReadBuffer = new Memory<byte>(_sut.SocketBuffer, 0, TlvHeader.Size);

        _sut.ProcessRead();

        // Since we read the whole TLV and the payload is 0 bytes long, we should end up Awaiting state
        _sut.ReadState.Should().Be(HandlerReadState.AwaitingHeader);
        _sut.CompleteTlvHeader.Should().Be(header);
    }

    [Fact]
    void ProcessRead_CanReadChunkedTlvHeader_WhenItWasPartitionedOnReceive()
    {
        var header = new TlvHeader(MessageType: 13, MessageLength: 0);
        header.PackInto(_sut.SocketBuffer);
        _sut.UnprocessedReadBuffer = new Memory<byte>(_sut.SocketBuffer, 0, 5);

        _sut.ProcessRead();

        // Since we read a part of the TLV and the payload is 0 bytes long, we should end up in ReadingHeader state
        _sut.ReadState.Should().Be(HandlerReadState.ReadingHeader);
        _sut.CompleteTlvHeader.Should().BeNull();

        // Pretend like next chunk of the TLV came in the next transmission unit
        _sut.UnprocessedReadBuffer = new Memory<byte>(_sut.SocketBuffer, 5, 2);
        _sut.ProcessRead();

        // Still not done reading... will continue on the next run
        _sut.ReadState.Should().Be(HandlerReadState.ReadingHeader);
        _sut.CompleteTlvHeader.Should().BeNull();

        _sut.UnprocessedReadBuffer = new Memory<byte>(_sut.SocketBuffer, 7, 1);
        _sut.ProcessRead();

        // Not we are done reading
        _sut.ReadState.Should().Be(HandlerReadState.AwaitingHeader);
        _sut.CompleteTlvHeader.Should().Be(header);
    }

    [Fact]
    public void ProcessRead_CanReadWholeMessage_WhenAvailable()
    {
        const int messageSize = 10;
        var tlvMessage = new TlvMessage
        {
            Header = new TlvHeader(MessageType: 13, messageSize),
            MessageValue = Enumerable.Range(0, messageSize).Select(i => (byte)i).ToArray()
        };

        tlvMessage.PackInto(_sut.SocketBuffer);
        _sut.UnprocessedReadBuffer = new Memory<byte>(_sut.SocketBuffer, 0, tlvMessage.Size);

        _sut.ProcessRead();

        var receivedMessage = _sut.InboundMessages.Single();
        receivedMessage.Header.Should().BeEquivalentTo(tlvMessage.Header);
        receivedMessage.MessageValue.Length.Should().Be(tlvMessage.Header.MessageLength);
        _sut.UnprocessedReadBuffer.Length.Should().Be(0);
    }

    [Fact]
    public void ProcessRead_CanReadMessage_WhenBothHeaderAndBodyArePartitionedOnReceive()
    {
        const int messageSize = 30;
        var tlvMessage = new TlvMessage
        {
            Header = new TlvHeader(MessageType: 13, messageSize),
            MessageValue = Enumerable.Range(0, messageSize).Select(i => (byte)i).ToArray()
        };

        tlvMessage.PackInto(_sut.SocketBuffer);
        _sut.UnprocessedReadBuffer = new Memory<byte>(_sut.SocketBuffer, 0, TlvHeader.Size - 3);

        _sut.ProcessRead();

        _sut.UnprocessedReadBuffer = new Memory<byte>(_sut.SocketBuffer, TlvHeader.Size - 3, 10);
        _sut.ProcessRead();

        _sut.UnprocessedReadBuffer = new Memory<byte>(
            _sut.SocketBuffer,
            TlvHeader.Size + 7,
            3
        );
        _sut.ProcessRead();

        _sut.UnprocessedReadBuffer = new Memory<byte>(
            _sut.SocketBuffer,
            TlvHeader.Size + 10,
            tlvMessage.Size - TlvHeader.Size - 10
        );
        _sut.ProcessRead();

        var receivedMessage = _sut.InboundMessages.Single();
        receivedMessage.Header.Should().BeEquivalentTo(tlvMessage.Header);
        receivedMessage.MessageValue.Length.Should().Be(tlvMessage.Header.MessageLength);
        _sut.UnprocessedReadBuffer.Length.Should().Be(0);
    }
}
