using System.Runtime.InteropServices;

namespace Irrelephant.Outcast.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = sizeof(int))]
public readonly record struct TlvHeader(
    int MessageType,
    int MessageLength
)
{
    public static int Size { get; } = Marshal.SizeOf<TlvHeader>();

    public static TlvHeader Unpack(Memory<byte> buffer)
    {
        return new(
            MessageType: MemoryMarshal.Read<int>(buffer.Span),
            MessageLength: MemoryMarshal.Read<int>(buffer.Span[sizeof(int)..])
        );
    }

    public void PackInto(Memory<byte> buffer)
    {
        BitConverter.TryWriteBytes(buffer.Span, MessageType);
        BitConverter.TryWriteBytes(buffer.Span[sizeof(int)..], MessageLength);
    }
}
