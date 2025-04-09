namespace Irrelephant.Outcast.Protocol;

public readonly record struct TlvMessage(
    TlvHeader Header,
    Memory<byte> MessageValue
)
{
    public void PackInto(Memory<byte> buffer)
    {
        Header.PackInto(buffer);
        MessageValue.Span.CopyTo(buffer.Span[TlvHeader.Size..]);
    }
};
