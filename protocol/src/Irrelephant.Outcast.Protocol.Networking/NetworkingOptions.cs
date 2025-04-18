using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Encoding;

namespace Irrelephant.Outcast.Protocol.Networking;

public class NetworkingOptions
{
    public int BufferSize { get; set; } = 256;
    public int MaxResizableBufferSize { get; set; } = 1024;
    public required IMessageCodec MessageCodec { get; set; }
}
