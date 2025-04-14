namespace Irrelephant.Outcast.Protocol.DataTransfer;

public readonly record struct TlvMessage(
    TlvHeader Header,
    Memory<byte> MessageValue
)
{
    public Memory<byte> PackInto(Memory<byte> buffer)
    {
        Header.PackInto(buffer);
        MessageValue.Span.CopyTo(buffer.Span[TlvHeader.Size..]);

        return buffer[..(TlvHeader.Size + Header.MessageLength)];
    }
}
