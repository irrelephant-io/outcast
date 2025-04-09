using System.Runtime.InteropServices;

namespace Irrelephant.Outcast.Protocol;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct TlvHeader(
    int MessageType,
    int MessageLength
)
{
    public static int Size { get; } = Marshal.SizeOf<TlvHeader>();

    public static TlvHeader Unpack(byte[] buffer) =>
        new(
            MessageType: BitConverter.ToInt32(buffer, 0),
            MessageLength: BitConverter.ToInt32(buffer, sizeof(int))
        );

    public void PackInto(Memory<byte> buffer)
    {
        BitConverter.TryWriteBytes(buffer.Span, MessageType);
        BitConverter.TryWriteBytes(buffer.Span[sizeof(int)..], MessageLength);
    }

}
