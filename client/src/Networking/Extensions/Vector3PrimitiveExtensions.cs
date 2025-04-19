using Godot;
using Irrelephant.Outcast.Protocol.Abstractions.DataTransfer.Messages.Primitives;

namespace Irrelephant.Outcast.Client.Networking.Extensions;

public static class Vector3PrimitiveExtensions
{
    public static Vector3 ToVector3(this Vector3Primitive vector) =>
        new(vector.X, vector.Y, vector.Z);
}
