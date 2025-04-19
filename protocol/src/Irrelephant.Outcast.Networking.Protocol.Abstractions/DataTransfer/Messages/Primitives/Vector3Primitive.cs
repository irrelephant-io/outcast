using System.Diagnostics;

namespace Irrelephant.Outcast.Networking.Protocol.Abstractions.DataTransfer.Messages.Primitives;

[DebuggerDisplay("Vector3Primitive({X}, {Y}, {Z})")]
public record struct Vector3Primitive(float X, float Y, float Z);
