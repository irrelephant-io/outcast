using FluentAssertions;
using Irrelephant.Outcast.Networking.Protocol;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Encoding;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages;
using Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages.Primitives;

namespace Irrelephant.Outcast.Networking.Tests;

public class JsonMessageCodecTests
{
    private readonly IMessageCodec _sut = new JsonMessageCodec();

    [Fact]
    public void EncodingMessageWithPrimitives_ShouldYieldSameMessage_AsWhenEncoded()
    {
        var message = new ConnectResponse(
            "Some name",
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Vector3Primitive(100, 200, 300),
            69f
        );

        var tlv = _sut.Encode(message);
        var decoded = _sut.Decode(tlv);

        decoded.Should().BeEquivalentTo(message);
    }
}
