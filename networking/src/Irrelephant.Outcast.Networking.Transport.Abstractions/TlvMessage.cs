namespace Irrelephant.Outcast.Networking.Transport.Abstractions;

public readonly record struct TlvMessage(
    TlvHeader Header,
    Memory<byte> MessageValue
)
{
    public int Size => TlvHeader.Size + Header.MessageLength;

    public Memory<byte> PackInto(Memory<byte> buffer)
    {
        Header.PackInto(buffer);
        MessageValue.Span.CopyTo(buffer.Span[TlvHeader.Size..]);

        return buffer[..(TlvHeader.Size + Header.MessageLength)];
    }
}
