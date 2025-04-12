using FluentAssertions;
using Irrelephant.Outcast.Protocol.DataTransfer.Encoding;
using Irrelephant.Outcast.Protocol.DataTransfer.Messages;
using Irrelephant.Outcast.Server.Configuration;
using Irrelephant.Outcast.Server.Networking;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Tests;

public class ClientMessageQueueTests
{
    private readonly IMessageCodec _messageCodec;
    private readonly IoSocketMessageHandler _sut;

    public ClientMessageQueueTests()
    {
        _messageCodec = new JsonMessageCodec();
        _sut = new IoSocketMessageHandler(
            new(),
            Options.Create(
                new NetworkingOptions
                {
                    MessageCodec = _messageCodec,
                    ConnectionListenEndpoint = null!,
                    Logger = null!
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
        _sut.InboundMessageReceived += (_, received) =>
        {
            received.Should().BeEquivalentTo(message);
        };
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

        _sut.InboundMessageReceived += (_, received) =>
        {
            received.Should().BeEquivalentTo(message);
        };
    }

    [Fact]
    public void ProcessRead_ShouldReadMessage_WhenTlvHeaderIsFragmented()
    {
        var messageBuffer = new byte[1024];
        var message = new ConnectRequest("john.doe@example.com");
        var tlv = _messageCodec.Encode(message);
        var packed = tlv.PackInto(messageBuffer);
        using var eventMonitor = _sut.Monitor();

        var packedChunk1 = packed[..5];
        _sut.ProcessRead(packedChunk1, packedChunk1.Length);

        var packedChunk2 = packed[5..];
        _sut.ProcessRead(packedChunk2, packedChunk2.Length);

        eventMonitor
            .Should().Raise(nameof(_sut.InboundMessageReceived))
            .WithArgs<ConnectRequest>(it => it.Name == message.Name);
    }

    [Fact]
    public void ProcessRead_ShouldReadMessages_WhenMultipleAreAvailable()
    {
        var messageBuffer = new byte[1024];
        var message = new ConnectRequest("john.doe@example.com");
        var tlv = _messageCodec.Encode(message);
        using var eventMonitor = _sut.Monitor();

        var packed1 = tlv.PackInto(messageBuffer);
        var packed2 = tlv.PackInto(messageBuffer.AsMemory(packed1.Length..));

        _sut.ProcessRead(
            messageBuffer[..(packed1.Length + packed2.Length)],
            packed1.Length + packed2.Length
        );

        eventMonitor
            .Should().Raise(nameof(_sut.InboundMessageReceived))
            .WithArgs<ConnectRequest>(it => it.Name == message.Name);

        eventMonitor.OccurredEvents.Should().HaveCount(2);
    }

    [Fact]
    public void ProcessRead_ShouldResizeInternalBuffer_WhenPayloadIsTooLarge()
    {
        var messageBuffer = new byte[1024];
        var message = new ConnectRequest(new string(Enumerable.Repeat('.', 512).ToArray()));
        var tlv = _messageCodec.Encode(message);
        var packed = tlv.PackInto(messageBuffer);
        using var eventMonitor = _sut.Monitor();

        _sut.ProcessRead(packed, packed.Length);

        eventMonitor
            .Should().Raise(nameof(_sut.InboundMessageReceived))
            .WithArgs<ConnectRequest>(it => it.Name == message.Name);
    }
}
