using FluentAssertions;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Protocol.Networking;
using Microsoft.Extensions.Options;

namespace Irrelephant.Outcast.Server.Tests;

public class MessageHandlerTests
{
    private readonly IMessageCodec _messageCodec;
    private readonly IoSocketMessageHandler _sut;

    public MessageHandlerTests()
    {
        _messageCodec = new JsonMessageCodec();
        _sut = new IoSocketMessageHandler(
            new(),
            Options.Create(
                new NetworkingOptions
                {
                    MessageCodec = _messageCodec
                }
            )
        );
    }

    [Fact]
    public void ProcessRead_ShouldReadWholeMessage_WhenFullyAvailable()
    {
        var messageBuffer = new byte[128];
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
        var messageBuffer = new byte[128];
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
        var messageBuffer = new byte[128];
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
        var messageBuffer = new byte[128];
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

    [Fact]
    public void ProcessRead_OnlyUpdatesNetworkTime_WhenPayloadIsHeartbeat()
    {
        var messageBuffer = new byte[64];
        var heartbeat = new Heartbeat();
        var tlv = _messageCodec.Encode(heartbeat);
        var packed = tlv.PackInto(messageBuffer);
        var preInvokeNetworkTime = _sut.LastNetworkActivity;
        using var eventMonitor = _sut.Monitor();

        _sut.ProcessRead(packed, packed.Length);

        _sut.LastNetworkActivity
            .Should()
            .BeAfter(
                preInvokeNetworkTime,
                because: "heartbeat updates last network time for network management."
            );

        eventMonitor
            .Should()
            .NotRaise(
                nameof(_sut.InboundMessageReceived),
                because: "heartbeat is not used for protocol communication."
            );
    }

    [Fact]
    public void EnqueueMessage_EmitsEvent_WhenMessageIsReadyToBeSent()
    {
        using var eventMonitor = _sut.Monitor();
        _sut.EnqueueMessage(new Heartbeat());

        eventMonitor
            .Should()
            .Raise(nameof(_sut.OutboundMessageAvailable));
    }

    [Fact]
    public void Reset_GetsHandlerReadyForReuse_WhenMessageIsPartiallyRead()
    {
        var messageBuffer = new byte[64];
        var connectRequest = new ConnectRequest("john.doe@example.com");
        var tlv = _messageCodec.Encode(connectRequest);
        var packed = tlv.PackInto(messageBuffer);
        using var eventMonitor = _sut.Monitor();

        // TLV message was chunked, handler is waiting for the remained of the message
        _sut.ProcessRead(packed, packed.Length / 2);
        eventMonitor
            .Should()
            .NotRaise(
                nameof(_sut.InboundMessageReceived),
                because: "message was not delivered in its full."
            );

        _sut.Reset();
        _sut.ProcessRead(packed, packed.Length);

        eventMonitor
            .Should()
            .Raise(
                nameof(_sut.InboundMessageReceived),
                because: "message is delivered after reset, which instructs the state machine to read from start."
            );
    }
}
