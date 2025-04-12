using FluentAssertions;
using Irrelephant.Outcast.Protocol.Encoding;
using Irrelephant.Outcast.Protocol.Messages;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Protocol;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Tests;

public class SocketProtocolStateTests
{
    private readonly IMessageCodec _messageCodec;
    private readonly SocketProtocolState _sut;

    public SocketProtocolStateTests()
    {
        _messageCodec = new JsonMessageCodec();
        _sut = new SocketProtocolState(
            Options.Create(
                new OutcastNetworkingOptions
                {
                    MessageCodec = _messageCodec,
                    ConnectionListenEndpoint = null!
                }
            )
        );
    }

    [Fact]
    public void ProcessRead_ShouldReadWholeMessage_WhenFullyAvailable()
    {
        var messageBuffer = new byte[1024];
        var message = new ConnectRequest("john.doe@example.com");
        var tlv = _messageCodec.Encode(message);
        var packed = tlv.PackInto(messageBuffer);

        _sut.ProcessRead(packed, packed.Length);

        _sut.ReceivedMessages.Should().ContainEquivalentOf(message);
    }

    [Fact]
    public void ProcessRead_ShouldReadMessage_DeliveredInMultipleChunks()
    {
        var messageBuffer = new byte[1024];
        var message = new ConnectRequest("john.doe@example.com");
        var tlv = _messageCodec.Encode(message);
        var packed = tlv.PackInto(messageBuffer);

        var packedChunk1 = packed[..10];
        _sut.ProcessRead(packedChunk1, packedChunk1.Length);

        var packedChunk2 = packed[10..20];
        _sut.ProcessRead(packedChunk2, packedChunk2.Length);

        var packedChunk3 = packed[20..];
        _sut.ProcessRead(packedChunk3, packedChunk3.Length);

        _sut.ReceivedMessages.Should().ContainEquivalentOf(message);
    }

    [Fact]
    public void ProcessRead_ShouldReadMessage_WhenTlvHeaderIsFragmented()
    {
        var messageBuffer = new byte[1024];
        var message = new ConnectRequest("john.doe@example.com");
        var tlv = _messageCodec.Encode(message);
        var packed = tlv.PackInto(messageBuffer);

        var packedChunk1 = packed[..5];
        _sut.ProcessRead(packedChunk1, packedChunk1.Length);

        var packedChunk2 = packed[5..];
        _sut.ProcessRead(packedChunk2, packedChunk2.Length);

        _sut.ReceivedMessages.Should().ContainEquivalentOf(message);
    }

    [Fact]
    public void ProcessRead_ShouldReadMessages_WhenMultipleAreAvailable()
    {
        var messageBuffer = new byte[1024];
        var message = new ConnectRequest("john.doe@example.com");
        var tlv = _messageCodec.Encode(message);
        var packed1 = tlv.PackInto(messageBuffer);
        var packed2 = tlv.PackInto(messageBuffer.AsMemory(packed1.Length..));

        _sut.ProcessRead(
            messageBuffer[..(packed1.Length + packed2.Length)],
            packed1.Length + packed2.Length
        );

        _sut.ReceivedMessages.Should().HaveCount(2);
        _sut.ReceivedMessages.Should().ContainEquivalentOf(message);
    }

    [Fact]
    public void ProcessRead_ShouldResizeInternalBuffer_WhenPayloadIsTooLarge()
    {
        var messageBuffer = new byte[1024];
        var message = new ConnectRequest(new string(Enumerable.Repeat('.', 512).ToArray()));
        var tlv = _messageCodec.Encode(message);
        var packed = tlv.PackInto(messageBuffer);

        _sut.ProcessRead(packed, packed.Length);

        _sut.ReceivedMessages.Should().ContainEquivalentOf(message);
    }
}
